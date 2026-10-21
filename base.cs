                        //newEntry["BOMID_Id"] = (materialDict["BOM"] as DynamicObject)["Id"];
                        //newEntry["BOMID"] = materialDict["BOM"] as DynamicObject;
                        //newEntry["MOType_Id"] = moBom.BillType_Id;
                        //newEntry["MOType"] = moBom.BillType;
                        //newEntry["MoId"] = moBom.Id;
                        //newEntry["MoBillNo"] = moBom.BillNo;
                        //newEntry["MoEntryId"] = moBom.EntityId;
                        //newEntry["MoEntrySeq"] = moBom.Seq;
                        
 // 计算分子分母与标准用量(MustQty)
 //decimal num = Convert.ToDecimal(materialDict["numerator"]);
 //decimal den = Convert.ToDecimal(materialDict["denominator"]);
 newEntry["MustQty"] = 10;
 newEntry["Numerator"] = decimal.One;
 newEntry["Denominator"] = decimal.One;
 newEntry["BaseNumerator"] = newEntry["Numerator"];
 newEntry["BaseDenominator"] = newEntry["Denominator"];
 newEntry["BOMNumerator"] = newEntry["Numerator"];
 newEntry["BOMDenominator"] = newEntry["Denominator"];

 // 基础控制属性赋值
 //newEntry["SupplyOrg_Id"] = moBom.PrdOrgId_Id;
 //newEntry["SupplyOrg"] = moBom.PrdOrgId;
 newEntry["IssueType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["IssueType"]) ? "2" : matProduce[0]["IssueType"];
 newEntry["BackFlushType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["BKFLTime"]) ? "2" : matProduce[0]["BKFLTime"];
 newEntry["OverControlMode"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["OverControlMode"]) ? "2" : matProduce[0]["OverControlMode"];
 newEntry["ReserveType"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matPlan[0]["ReserveType"]) ? "1" : matPlan[0]["ReserveType"];
 newEntry["UseRate"] = Convert.ToDouble("100");
 newEntry["UnitID"] = matProduce[0]["ProduceUnitId"] as DynamicObject;
 newEntry["UnitID_Id"] = (matProduce[0]["ProduceUnitId"] as DynamicObject)["Id"];
 newEntry["BaseUnitID"] = matBase[0]["BaseUnitId"] as DynamicObject;
 newEntry["BaseUnitID_Id"] = (matBase[0]["BaseUnitId"] as DynamicObject)["Id"];


 // 发料仓库
 //OQLFilter stockFilter2 = OQLFilter.CreateHeadEntityFilter(string.Format(" FStockId = '{0}' ", materialDict["flck"]));
 //DynamicObject stockObj2 = BusinessDataServiceHelper.LoadFromCache(this.Context, "BD_STOCK", null, stockFilter2).FirstOrDefault();
 newEntry["StockID"] = produceInfo["PICKSTOCKID"];
 newEntry["StockID_Id"] = produceInfo["PICKSTOCKID_id"];

 newEntry["FixScrapQty"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(matProduce[0]["FIXLOSS"]) ? "0" : matProduce[0]["FIXLOSS"];
 newEntry["OperID"] = 99;
 newEntry["OptQueue"] = ObjectUtils.IsNullOrEmptyOrWhiteSpace(targetMaterial["FOptQueue"]) ? "0" : targetMaterial["FOptQueue"];

 // 数量冗余字段计算
 newEntry["NoPickedQty"] = newEntry["MustQty"];
 newEntry["StdQty"] = newEntry["MustQty"];
 newEntry["NeedQty"] = newEntry["MustQty"];
 newEntry["IsMrpRun"] = true;
 newEntry["OwnerTypeId"] = "BD_OwnerOrg";
 //newEntry["OwnerID_Id"] = moBom.PrdOrgId_Id;
 //newEntry["OwnerID"] = moBom.PrdOrgId;
 //newEntry["StockStatusId"] = defaultStockStatus;
 //newEntry["StockStatusId_Id"] = defaultStockStatus["Id"];
 //newEntry["NeedDate"] = moBom.PlanFinishDate;


 // 计算基本单位下的对应数量
 decimal conversionRate = decimal.One;
 newEntry["BaseNoPickedQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
 newEntry["BaseStdQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
 newEntry["BaseNeedQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
 newEntry["BaseMustQty"] = Convert.ToDecimal(newEntry["MustQty"]) * conversionRate;
 //newEntry["GroupByOwnerId_Id"] = moBom.PrdOrgId_Id;
 //newEntry["GroupByOwnerId"] = moBom.PrdOrgId;
 newEntry["PPBomEntryType"] = "0";
 newEntry["F_JY_CheckBox"] = true;
