// 调用基类方法处理操作事务
base.EndOperationTransaction(e);
this.OperationResult.IsShowMessage = true;

// 1. 判断当前表单操作是否为“审核(Audit)”或“二次更新(SecondUpdate)”
bool isAuditOrUpdate = this.FormOperation.Operation.Equals("Audit") 
    || string.Equals(this.FormOperation.Operation, "SecondUpdate", StringComparison.OrdinalIgnoreCase);

if (isAuditOrUpdate)
{
    Common common = new Common();
    List rowIdSqlList = new List(); // 存储更新行ID的 SQL 语句列表
    List moBomList = new List(); // 存储提取的生产单与物料上下文对象
    List billNoList = new List();   // 单据编号列表
    List seqList = new List();      // 行序号列表

    // 2. 遍历表单传进来的单据数据实体 (生产订单数据)
    DynamicObject[] dataEntitys = e.DataEntitys;
    for (int i = 0; i < dataEntitys.Length; i++)
    {
        DynamicObject dynamicObject = dataEntitys[i];
        
        // 读取单据编号、ID、单据类型、生产组织
        string billNo = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject["BillNo"]) ? string.Empty : Convert.ToString(dynamicObject["BillNo"]);
        billNoList.Add(billNo);
        string id = Convert.ToString(dynamicObject["Id"]);
        string billTypeId = Convert.ToString(dynamicObject["BillType_Id"]);
        DynamicObject billType = common.GetBase(this.Context, "BOS_BillType", "FBILLTYPEID", billTypeId);
        
        string prdOrgId = Convert.ToString(dynamicObject["PrdOrgId_Id"]);
        DynamicObject prdOrg = common.GetBase(this.Context, "ORG_Organizations", "FOrgID", prdOrgId);

        // 读取自定义的基准用量分子/分母
        decimal f_JYBZYLFZ = Convert.ToDecimal(dynamicObject["F_JYBZYLFZ"]);
        decimal f_JYBZYLFM = Convert.ToDecimal(dynamicObject["F_JYBZYLFM"]);

        // 获取补充用料对应表单集合与单据明细单头 (TreeEntity)
        DynamicObjectCollection bcyyldybs = dynamicObject["F_JY_BCYYLDYB"] as DynamicObjectCollection;
        DynamicObjectCollection treeEntityCollection = dynamicObject["TreeEntity"] as DynamicObjectCollection;

        // 遍历单据明细行
        foreach (DynamicObject currentEntry in treeEntityCollection)
        {
            string seq = ObjectUtils.IsNullOrEmptyOrWhiteSpace(currentEntry["Seq"]) ? string.Empty : Convert.ToString(currentEntry["Seq"]);
            // 获取成型线路大类 ID
            string zxcxdl = ObjectUtils.IsNullOrEmptyOrWhiteSpace(currentEntry["F_SHDL_zxcxdl"]) 
                ? string.Empty 
                : Convert.ToString((currentEntry["F_SHDL_zxcxdl"] as DynamicObject)["Id"]);
            seqList.Add(seq);

            // 获取物料扩展属性列表
            List> materialDictList = new List>();
            foreach (string fieldName0 in this.MaterialFieldName0)
            {
                materialDictList.Add(this.GetMaterial(common, fieldName0, bcyyldybs, currentEntry, 0, zxcxdl, prdOrgId));
            }
            foreach (string fieldName1 in this.MaterialFieldName1)
            {
                materialDictList.Add(this.GetMaterial(common, fieldName1, bcyyldybs, currentEntry, 1, zxcxdl, prdOrgId));
            }

            // 封装为 PPBOM 自定义中间实体
            PRD_MO_Operation.PPBOM item = new PRD_MO_Operation.PPBOM
            {
                BillNo = billNo,
                Id = id,
                BillType_Id = billTypeId,
                BillType = billType,
                PrdOrgId_Id = prdOrgId,
                PrdOrgId = prdOrg,
                F_JYBZYLFZ = f_JYBZYLFZ,
                F_JYBZYLFM = f_JYBZYLFM,
                Seq = seq,
                EntityId = Convert.ToString(currentEntry["Id"]),
                Qty = Convert.ToDecimal(currentEntry["Qty"]),
                PlanFinishDate = Convert.ToDateTime(currentEntry["PlanFinishDate"]),
                DictionaryList = materialDictList,
                zxcxdl = zxcxdl
            };
            moBomList.Add(item);
        }
    }

    // 3. 读取生产用料清单 (PRD_PPBOM) 元数据并构建 OQL 过滤条件
    BusinessInfo businessInfo = MetaDataServiceHelper.GetFormMetaData(this.Context, "PRD_PPBOM").BusinessInfo;
    OQLFilter oQLFilter = OQLFilter.CreateHeadEntityFilter(
        string.Format(" FMOBillNO in ('{0}') and FMOEntrySeq in ('{1}') ", string.Join("','", billNoList), string.Join("','", seqList))
    );

    // 获取所有字段Key并加载对应的生产用料清单数据
    List fieldList = businessInfo.GetFieldList();
    List fieldKeys = new List();
    foreach (Field field in fieldList)
    {
        fieldKeys.Add(field.get_Key());
    }
    DynamicObject[] ppBomDataArray = BusinessDataServiceHelper.Load(this.Context, businessInfo, SelectorItemInfo.CreateItems(fieldKeys.ToArray()), oQLFilter);

    // 获取库存状态预设基础资料 (KCZT01_SYS)
    DynamicObject defaultStockStatus = common.GetBase(this.Context, "BD_StockStatus", "FNumber", "KCZT01_SYS");

    // 4. 匹配用料清单与订单数据，更新及追加物料明细
    foreach (DynamicObject ppBomData in ppBomDataArray)
    {
        foreach (PRD_MO_Operation.PPBOM moBom in moBomList)
        {
            // 判断订单号和行号是否匹配
            if (moBom.BillNo.Equals(Convert.ToString(ppBomData["MOBillNO"])) && moBom.Seq.Equals(Convert.ToString(ppBomData["MOEntrySeq"])))
            {
                DynamicObjectCollection ppBomEntries = ppBomData["PPBomEntry"] as DynamicObjectCollection;
                List existingMaterialIds = new List();

                // 4.1 遍历现有的用料清单明细，依据路线大类与分类查询 SQL，补充工序(OperID)和发料仓库(StockID)
                foreach (DynamicObject entry in ppBomEntries)
                {
                    DynamicObject materialObj = entry["MaterialID"] as DynamicObject;
                    DynamicObjectCollection matBaseCollection = materialObj["MaterialBase"] as DynamicObjectCollection;
                    
                    string categoryId = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matBaseCollection[0]["CategoryID"]) ? "" : Convert.ToString((matBaseCollection[0]["CategoryID"] as DynamicObject)["Id"]);
                    string mujuType = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialObj["F_JY_MUJULX"]) ? "" : Convert.ToString(materialObj["F_JY_MUJULX"]);
                    
                    string operId = string.Empty;
                    string flckId = string.Empty;

                    // 从成型工序对应表(T_JY_CXGXDYB)查找对应的工序和发料仓库
                    string sql = string.Format(
                        " select T2.FOPERID,T2.F_JY_FLCK from T_JY_CXGXDYB T1,T_JY_CXGXDYBEntry T2 " +
                        " where T1.FDOCUMENTSTATUS = 'C' and T1.FID = T2.FID and T2.F_SHDL_ZXCXDL = '{0}' " +
                        " and T2.FCATEGORYID = '{1}' and T2.F_JY_MUJULX = '{2}' and T1.FUSEORGID = '{3}' ",
                        moBom.zxcxdl, categoryId, mujuType, moBom.PrdOrgId_Id
                    );

                    using (IDataReader reader = DBServiceHelper.ExecuteReader(this.Context, "/*dialect*/" + sql))
                    {
                        while (reader.Read())
                        {
                            if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(reader["FOPERID"]))
                            {
                                operId = Convert.ToString(reader["FOPERID"]);
                                flckId = Convert.ToString(reader["F_JY_FLCK"]);
                            }
                        }
                    }

                    // 赋值工序与发料仓库
                    entry["OperID"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(operId) ? materialObj["FOperID"] : operId;
                    entry["OptQueue"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialObj["FOptQueue"]) ? "0" : materialObj["FOptQueue"];

                    if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(flckId))
                    {
                        OQLFilter stockFilter = OQLFilter.CreateHeadEntityFilter(string.Format(" FStockId = '{0}' ", flckId));
                        DynamicObject stockObj = BusinessDataServiceHelper.LoadFromCache(this.Context, "BD_STOCK", null, stockFilter).FirstOrDefault();
                        entry["StockID"] = stockObj;
                        entry["StockID_Id"] = flckId;
                    }

                    // 记录已存在的物料 ID
                    if (!existingMaterialIds.Contains(Convert.ToString(entry["MaterialID_Id"])))
                    {
                        existingMaterialIds.Add(Convert.ToString(entry["MaterialID_Id"]));
                    }
                }

                int currentSeq = ppBomEntries.Count;

                // 4.2 判断需要追加的物料明细，若不在已有的清单中，则动态创建并插入新行
                foreach (Dictionary materialDict in moBom.DictionaryList)
                {
                    if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialDict))
                    {
                        DynamicObject targetMaterial = materialDict["material"] as DynamicObject;
                        bool isNewMaterial = true;

                        foreach (string matId in existingMaterialIds)
                        {
                            if (matId.Equals(Convert.ToString(targetMaterial["Id"])))
                            {
                                isNewMaterial = false; // 已存在则不再重复添加
                            }
                        }

                        if (isNewMaterial)
                        {
                            // 创建用料清单明细行实体
                            DynamicObject newEntry = new DynamicObject(businessInfo.GetEntity("FEntity").DynamicObjectType);
                            DynamicObjectCollection matBase = targetMaterial["MaterialBase"] as DynamicObjectCollection;
                            DynamicObjectCollection matPlan = targetMaterial["MaterialPlan"] as DynamicObjectCollection;
                            DynamicObjectCollection matProduce = targetMaterial["MaterialProduce"] as DynamicObjectCollection;

                            newEntry["Seq"] = currentSeq + 1;
                            newEntry["ReplaceGroup"] = newEntry["Seq"];
                            newEntry["MOType_Id"] = moBom.BillType_Id;
                            newEntry["MOType"] = moBom.BillType;
                            newEntry["MoId"] = moBom.Id;
                            newEntry["MoBillNo"] = moBom.BillNo;
                            newEntry["MoEntryId"] = moBom.EntityId;
                            newEntry["MoEntrySeq"] = moBom.Seq;
                            newEntry["MaterialID_Id"] = targetMaterial["Id"];
                            newEntry["MaterialID"] = targetMaterial;
                            newEntry["BOMID_Id"] = (materialDict["BOM"] as DynamicObject)["Id"];
                            newEntry["BOMID"] = materialDict["BOM"] as DynamicObject;
                            newEntry["SupplyType"] = "C"; // 倒冲领料等供应类型
                            newEntry["MaterialType"] = "1";

                            // 计算分子分母与标准用量(MustQty)
                            decimal num = Convert.ToDecimal(materialDict["numerator"]);
                            decimal den = Convert.ToDecimal(materialDict["denominator"]);
                            newEntry["MustQty"] = (!num.Equals(decimal.Zero) && !den.Equals(decimal.Zero)) ? (num / den * moBom.Qty) : moBom.Qty;
                            newEntry["Numerator"] = (!num.Equals(decimal.Zero)) ? num : decimal.One;
                            newEntry["Denominator"] = (!den.Equals(decimal.Zero)) ? den : decimal.One;
                            newEntry["BaseNumerator"] = newEntry["Numerator"];
                            newEntry["BaseDenominator"] = newEntry["Denominator"];
                            newEntry["BOMNumerator"] = newEntry["Numerator"];
                            newEntry["BOMDenominator"] = newEntry["Denominator"];

                            // 根据特殊标识ator重新纠正分子分母及用量
                            if (Convert.ToInt32(materialDict["ator"]) == 1)
                            {
                                newEntry["Numerator"] = (!num.Equals(decimal.Zero)) ? num : moBom.F_JYBZYLFZ;
                                newEntry["Denominator"] = (!den.Equals(decimal.Zero)) ? den : moBom.F_JYBZYLFM;
                                newEntry["BaseNumerator"] = newEntry["Numerator"];
                                newEntry["BaseDenominator"] = newEntry["Denominator"];
                                newEntry["BOMNumerator"] = newEntry["Numerator"];
                                newEntry["BOMDenominator"] = newEntry["Denominator"];
                                newEntry["MustQty"] = (!num.Equals(decimal.Zero) && !den.Equals(decimal.Zero)) 
                                    ? (num / den * moBom.Qty) 
                                    : (moBom.F_JYBZYLFZ / moBom.F_JYBZYLFM * moBom.Qty);
                            }

                            // 基础控制属性赋值
                            newEntry["SupplyOrg_Id"] = moBom.PrdOrgId_Id;
                            newEntry["SupplyOrg"] = moBom.PrdOrgId;
                            newEntry["IssueType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["IssueType"]) ? "2" : matProduce[0]["IssueType"];
                            newEntry["BackFlushType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["BKFLTime"]) ? "2" : matProduce[0]["BKFLTime"];
                            newEntry["OverControlMode"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["OverControlMode"]) ? "2" : matProduce[0]["OverControlMode"];
                            newEntry["ReserveType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matPlan[0]["ReserveType"]) ? "1" : matPlan[0]["ReserveType"];
                            newEntry["UseRate"] = Convert.ToDouble("100");
                            newEntry["UnitID"] = matProduce[0]["ProduceUnitId"] as DynamicObject;
                            newEntry["UnitID_Id"] = (matProduce[0]["ProduceUnitId"] as DynamicObject)["Id"];
                            newEntry["BaseUnitID"] = matBase[0]["BaseUnitId"] as DynamicObject;
                            newEntry["BaseUnitID_Id"] = (matBase[0]["BaseUnitId"] as DynamicObject)["Id"];

                            // 获取发料仓库
                            OQLFilter stockFilter2 = OQLFilter.CreateHeadEntityFilter(string.Format(" FStockId = '{0}' ", materialDict["flck"]));
                            DynamicObject stockObj2 = BusinessDataServiceHelper.LoadFromCache(this.Context, "BD_STOCK", null, stockFilter2).FirstOrDefault();
                            newEntry["StockID"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialDict["flck"]) ? (matProduce[0]["PickStockId"] as DynamicObject) : stockObj2;
                            newEntry["StockID_Id"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialDict["flck"]) ? (matProduce[0]["PickStockId"] as DynamicObject)["Id"] : materialDict["flck"];
                            
                            newEntry["FixScrapQty"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["FIXLOSS"]) ? "0" : matProduce[0]["FIXLOSS"];
                            newEntry["OperID"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialDict["operid"]) ? targetMaterial["FOperID"] : materialDict["operid"];
                            newEntry["OptQueue"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(targetMaterial["FOptQueue"]) ? "0" : targetMaterial["FOptQueue"];
                            
                            // 数量冗余字段计算
                            newEntry["NoPickedQty"] = newEntry["MustQty"];
                            newEntry["StdQty"] = newEntry["MustQty"];
                            newEntry["NeedQty"] = newEntry["MustQty"];
                            newEntry["IsMrpRun"] = true;
                            newEntry["OwnerTypeId"] = "BD_OwnerOrg";
                            newEntry["OwnerID_Id"] = moBom.PrdOrgId_Id;
                            newEntry["OwnerID"] = moBom.PrdOrgId;
                            newEntry["StockStatusId"] = defaultStockStatus;
                            newEntry["StockStatusId_Id"] = defaultStockStatus["Id"];
                            newEntry["NeedDate"] = moBom.PlanFinishDate;

                            // 4.3 查询计量单位转换率并计算基本单位数量
                            decimal conversionRate = decimal.One;
                            string unitSql = string.Format(
                                "--目标单位=当前单位*换算分母/换算分子\r\n" +
                                "select convert(float,FCONVERTDENOMINATOR/FCONVERTNUMERATOR) ConversionRate " +
                                "from (" +
                                "    SELECT FDESTUNITID,FCURRENTUNITID,FCONVERTDENOMINATOR,FCONVERTNUMERATOR FROM T_BD_UNITCONVERTRATE " +
                                "    UNION " +
                                "    SELECT FCURRENTUNITID FDESTUNITID,FDESTUNITID FCURRENTUNITID,FCONVERTNUMERATOR FCONVERTDENOMINATOR,FCONVERTDENOMINATOR FCONVERTNUMERATOR FROM T_BD_UNITCONVERTRATE " +
                                ") T where FCURRENTUNITID = '{0}' and FDESTUNITID = '{1}'",
                                Convert.ToString(newEntry["UnitID_Id"]), Convert.ToString(newEntry["BaseUnitID_Id"])
                            );

                            using (IDataReader unitReader = DBServiceHelper.ExecuteReader(this.Context, "/*dialect*/" + unitSql))
                            {
                                while (unitReader.Read())
                                {
                                    if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(unitReader["ConversionRate"]))
                                    {
                                        conversionRate = Convert.ToDecimal(unitReader["ConversionRate"]);
                                    }
                                }
                            }

                            // 计算基本单位下的对应数量
                            newEntry["BaseNoPickedQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
                            newEntry["BaseStdQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
                            newEntry["BaseNeedQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
                            newEntry["BaseMustQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
                            newEntry["GroupByOwnerId_Id"] = moBom.PrdOrgId_Id;
                            newEntry["GroupByOwnerId"] = moBom.PrdOrgId;
                            newEntry["PPBomEntryType"] = "0";
                            newEntry["F_JY_CheckBox"] = true;

                            currentSeq++;
                            ppBomEntries.Add(newEntry); // 将新创建的明细追加到用料清单
                            existingMaterialIds.Add(Convert.ToString(targetMaterial["Id"]));
                            rowIdSqlList.Add(common.UpDateRowId(Convert.ToString(ppBomData["Id"]), Convert.ToString(targetMaterial["Id"])));
                        }
                    }
                }
                
                // 自定义更新用料清单
                common.UpDatePPBOM(this.Context, moBom.BillNo, moBom.Seq, existingMaterialIds);
            }
        }
    }

    // 5. 加载引用对象并批量保存用料清单数据
    AppServiceContext.get_DBService().LoadReferenceObject(this.Context, ppBomDataArray, businessInfo.GetDynamicObjectType(), true);
    try
    {
        // 尝试一次性整批保存
        IOperationResult operationResult = BusinessDataServiceHelper.Save(this.Context, businessInfo, ppBomDataArray, null, "");
        if (operationResult.IsSuccess)
        {
            foreach (OperateResult item in operationResult.OperateResult)
            {
                this.OperationResult.OperateResult.Add(item);
            }
        }
        else
        {
            foreach (ValidationErrorInfo error in operationResult.ValidationErrors)
            {
                this.OperationResult.ValidationErrors.Add(error);
            }
        }
    }
    catch (Exception ex)
    {
        // 若整批保存抛异常，则退化为单条逐个保存，确保部分保存成功
        List values = new List { ex.Message, ex.StackTrace };
        OperateResult errResult = new OperateResult();
        errResult.set_SuccessStatus(false);
        errResult.set_Name("PRD_PPBOM");
        errResult.set_Message(string.Join("\r\n", values));
        errResult.set_MessageType(1);
        errResult.set_PKValue(Guid.NewGuid().ToString("N"));
        this.OperationResult.OperateResult.Add(errResult);

        // 逐条 Save
        foreach (DynamicObject singlePpBom in ppBomDataArray)
        {
            IOperationResult operationResult2 = BusinessDataServiceHelper.Save(this.Context, businessInfo, singlePpBom, null, "");
            if (operationResult2.IsSuccess)
            {
                foreach (OperateResult item in operationResult2.OperateResult)
                {
                    this.OperationResult.OperateResult.Add(item);
                }
            }
            else
            {
                foreach (ValidationErrorInfo error in operationResult2.ValidationErrors)
                {
                    this.OperationResult.ValidationErrors.Add(error);
                }
            }
        }
    }

    // 6. 批量执行 SQL 更新（例如同步更新 RowID 等数据库逻辑）
    DBServiceHelper.ExecuteBatch(this.Context, rowIdSqlList);



}