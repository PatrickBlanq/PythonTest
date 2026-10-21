
	Common common = new Common();
	List<string> list = new List<string>();
	List<PRD_MO_Operation.PPBOM> list2 = new List<PRD_MO_Operation.PPBOM>();
	List<string> list3 = new List<string>();
	List<string> list4 = new List<string>();
	DynamicObject[] dataEntitys = e.get_DataEntitys();
	for (int i = 0; i < dataEntitys.Length; i++)
	{
		DynamicObject dynamicObject = dataEntitys[i];
		string text = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject.get_Item("BillNo")) ? string.Empty : Convert.ToString(dynamicObject.get_Item("BillNo"));
		list3.Add(text);
		string id = Convert.ToString(dynamicObject.get_Item("Id"));
		string text2 = Convert.ToString(dynamicObject.get_Item("BillType_Id"));
		DynamicObject @base = common.GetBase(base.get_Context(), "BOS_BillType", "FBILLTYPEID", text2);
		string text3 = Convert.ToString(dynamicObject.get_Item("PrdOrgId_Id"));
		DynamicObject base2 = common.GetBase(base.get_Context(), "ORG_Organizations", "FOrgID", text3);
		decimal f_JYBZYLFZ = Convert.ToDecimal(dynamicObject.get_Item("F_JYBZYLFZ"));
		decimal f_JYBZYLFM = Convert.ToDecimal(dynamicObject.get_Item("F_JYBZYLFM"));
		DynamicObjectCollection bcyyldybs = dynamicObject.get_Item("F_JY_BCYYLDYB") as DynamicObjectCollection;
		DynamicObjectCollection dynamicObjectCollection = dynamicObject.get_Item("TreeEntity") as DynamicObjectCollection;
		foreach (DynamicObject current in dynamicObjectCollection)
		{
			string text4 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(current.get_Item("Seq")) ? string.Empty : Convert.ToString(current.get_Item("Seq"));
			string zxcxdl = ObjectUtils.IsNullOrEmptyOrWhiteSpace(current.get_Item("F_SHDL_zxcxdl")) ? string.Empty : Convert.ToString((current.get_Item("F_SHDL_zxcxdl") as DynamicObject).get_Item("Id"));
			list4.Add(text4);
			List<Dictionary<string, object>> list5 = new List<Dictionary<string, object>>();
			foreach (string current2 in this.MaterialFieldName0)
			{
				list5.Add(this.GetMaterial(common, current2, bcyyldybs, current, 0, zxcxdl, text3));
			}
			foreach (string current3 in this.MaterialFieldName1)
			{
				list5.Add(this.GetMaterial(common, current3, bcyyldybs, current, 1, zxcxdl, text3));
			}
			PRD_MO_Operation.PPBOM item = new PRD_MO_Operation.PPBOM
			{
				BillNo = text,
				Id = id,
				BillType_Id = text2,
				BillType = @base,
				PrdOrgId_Id = text3,
				PrdOrgId = base2,
				F_JYBZYLFZ = f_JYBZYLFZ,
				F_JYBZYLFM = f_JYBZYLFM,
				Seq = text4,
				EntityId = Convert.ToString(current.get_Item("Id")),
				Qty = Convert.ToDecimal(current.get_Item("Qty")),
				PlanFinishDate = Convert.ToDateTime(current.get_Item("PlanFinishDate")),
				DictionaryList = list5,
				zxcxdl = zxcxdl
			};
			list2.Add(item);
		}
	}
	BusinessInfo businessInfo = MetaDataServiceHelper.GetFormMetaData(base.get_Context(), "PRD_PPBOM").get_BusinessInfo();
	OQLFilter oQLFilter = OQLFilter.CreateHeadEntityFilter(string.Format(" FMOBillNO in ('{0}') and FMOEntrySeq in ('{1}') ", string.Join("','", list3), string.Join("','", list4)));
	List<Field> fieldList = businessInfo.GetFieldList();
	List<string> list6 = new List<string>();
	foreach (Field current4 in fieldList)
	{
		list6.Add(current4.get_Key());
	}
	DynamicObject[] array = BusinessDataServiceHelper.Load(base.get_Context(), businessInfo, SelectorItemInfo.CreateItems(list6.ToArray()), oQLFilter);
	DynamicObject base3 = common.GetBase(base.get_Context(), "BD_StockStatus", "FNumber", "KCZT01_SYS");
	DynamicObject[] array2 = array;
	for (int j = 0; j < array2.Length; j++)
	{
		DynamicObject dynamicObject2 = array2[j];
		foreach (PRD_MO_Operation.PPBOM current5 in list2)
		{
			bool flag3 = current5.BillNo.Equals(Convert.ToString(dynamicObject2.get_Item("MOBillNO"))) && current5.Seq.Equals(Convert.ToString(dynamicObject2.get_Item("MOEntrySeq")));
			if (flag3)
			{
				DynamicObjectCollection dynamicObjectCollection2 = dynamicObject2.get_Item("PPBomEntry") as DynamicObjectCollection;
				List<string> list7 = new List<string>();
				foreach (DynamicObject current6 in dynamicObjectCollection2)
				{
					DynamicObject dynamicObject3 = current6.get_Item("MaterialID") as DynamicObject;
					DynamicObjectCollection dynamicObjectCollection3 = dynamicObject3.get_Item("MaterialBase") as DynamicObjectCollection;
					string text5 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection3[0].get_Item("CategoryID")) ? "" : Convert.ToString((dynamicObjectCollection3[0].get_Item("CategoryID") as DynamicObject).get_Item("Id"));
					string text6 = ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject3.get_Item("F_JY_MUJULX")) ? "" : Convert.ToString(dynamicObject3.get_Item("F_JY_MUJULX"));
					string text7 = string.Empty;
					string text8 = string.Empty;
					using (IDataReader dataReader = DBServiceHelper.ExecuteReader(base.get_Context(), "/*dialect*/" + string.Format(" select T2.FOPERID,T2.F_JY_FLCK from T_JY_CXGXDYB T1,T_JY_CXGXDYBEntry T2  where T1.FDOCUMENTSTATUS = 'C' and T1.FID = T2.FID and T2.F_SHDL_ZXCXDL = '{0}' and T2.FCATEGORYID = '{1}' and T2.F_JY_MUJULX = '{2}' and T1.FUSEORGID = '{3}' ", new object[]
					{
						current5.zxcxdl,
						text5,
						text6,
						current5.PrdOrgId_Id
					})))
					{
						while (dataReader.Read())
						{
							bool flag4 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dataReader["FOPERID"]);
							if (flag4)
							{
								text7 = Convert.ToString(dataReader["FOPERID"]);
								text8 = Convert.ToString(dataReader["F_JY_FLCK"]);
							}
						}
					}
					current6.set_Item("OperID", ObjectUtils.IsNullOrEmptyOrWhiteSpace(text7) ? dynamicObject3.get_Item("FOperID") : text7);
					current6.set_Item("OptQueue", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject3.get_Item("FOptQueue")) ? "0" : dynamicObject3.get_Item("FOptQueue"));
					bool flag5 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(text8);
					if (flag5)
					{
						OQLFilter oQLFilter2 = OQLFilter.CreateHeadEntityFilter(string.Format(" FStockId = '{0}' ", text8));
						DynamicObject dynamicObject4 = BusinessDataServiceHelper.LoadFromCache(base.get_Context(), "BD_STOCK", null, oQLFilter2).FirstOrDefault<DynamicObject>();
						current6.set_Item("StockID", dynamicObject4);
						current6.set_Item("StockID_Id", text8);
					}
					bool flag6 = !list7.Contains(Convert.ToString(current6.get_Item("MaterialID_Id")));
					if (flag6)
					{
						list7.Add(Convert.ToString(current6.get_Item("MaterialID_Id")));
					}
				}
				int num = dynamicObjectCollection2.Count;
				foreach (Dictionary<string, object> current7 in current5.DictionaryList)
				{
					bool flag7 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(current7);
					if (flag7)
					{
						DynamicObject dynamicObject5 = current7["material"] as DynamicObject;
						bool flag8 = true;
						foreach (string current8 in list7)
						{
							bool flag9 = current8.Equals(Convert.ToString(dynamicObject5.get_Item("Id")));
							if (flag9)
							{
								flag8 = false;
							}
						}
						bool flag10 = flag8;
						if (flag10)
						{
							DynamicObject dynamicObject6 = new DynamicObject(businessInfo.GetEntity("FEntity").get_DynamicObjectType());
							DynamicObjectCollection dynamicObjectCollection4 = dynamicObject5.get_Item("MaterialBase") as DynamicObjectCollection;
							DynamicObjectCollection dynamicObjectCollection5 = dynamicObject5.get_Item("MaterialPlan") as DynamicObjectCollection;
							DynamicObjectCollection dynamicObjectCollection6 = dynamicObject5.get_Item("MaterialProduce") as DynamicObjectCollection;
							DynamicObjectCollection dynamicObjectCollection7 = dynamicObject5.get_Item("MaterialInvPty") as DynamicObjectCollection;
							dynamicObject6.set_Item("Seq", num + 1);
							dynamicObject6.set_Item("ReplaceGroup", dynamicObject6.get_Item("Seq"));
							dynamicObject6.set_Item("MOType_Id", current5.BillType_Id);
							dynamicObject6.set_Item("MOType", current5.BillType);
							dynamicObject6.set_Item("MoId", current5.Id);
							dynamicObject6.set_Item("MoBillNo", current5.BillNo);
							dynamicObject6.set_Item("MoEntryId", current5.EntityId);
							dynamicObject6.set_Item("MoEntrySeq", current5.Seq);
							dynamicObject6.set_Item("MaterialID_Id", dynamicObject5.get_Item("Id"));
							dynamicObject6.set_Item("MaterialID", dynamicObject5);
							dynamicObject6.set_Item("BOMID_Id", (current7["BOM"] as DynamicObject).get_Item("Id"));
							dynamicObject6.set_Item("BOMID", current7["BOM"] as DynamicObject);
							dynamicObject6.set_Item("SupplyType", "C");
							dynamicObject6.set_Item("MaterialType", "1");
							decimal num2 = Convert.ToDecimal(current7["numerator"]);
							decimal num3 = Convert.ToDecimal(current7["denominator"]);
							dynamicObject6.set_Item("MustQty", (!num2.Equals(decimal.Zero) && !num3.Equals(decimal.Zero)) ? (num2 / num3 * current5.Qty) : current5.Qty);
							dynamicObject6.set_Item("Numerator", (!num2.Equals(decimal.Zero)) ? num2 : decimal.One);
							dynamicObject6.set_Item("Denominator", (!num3.Equals(decimal.Zero)) ? num3 : decimal.One);
							dynamicObject6.set_Item("BaseNumerator", (!num2.Equals(decimal.Zero)) ? num2 : decimal.One);
							dynamicObject6.set_Item("BaseDenominator", (!num3.Equals(decimal.Zero)) ? num3 : decimal.One);
							dynamicObject6.set_Item("BOMNumerator", (!num2.Equals(decimal.Zero)) ? num2 : decimal.One);
							dynamicObject6.set_Item("BOMDenominator", (!num3.Equals(decimal.Zero)) ? num3 : decimal.One);
							bool flag11 = current7["ator"].Equals(1);
							if (flag11)
							{
								dynamicObject6.set_Item("Numerator", (!num2.Equals(decimal.Zero)) ? num2 : current5.F_JYBZYLFZ);
								dynamicObject6.set_Item("Denominator", (!num3.Equals(decimal.Zero)) ? num3 : current5.F_JYBZYLFM);
								dynamicObject6.set_Item("BaseNumerator", (!num2.Equals(decimal.Zero)) ? num2 : current5.F_JYBZYLFZ);
								dynamicObject6.set_Item("BaseDenominator", (!num3.Equals(decimal.Zero)) ? num3 : current5.F_JYBZYLFM);
								dynamicObject6.set_Item("BOMNumerator", (!num2.Equals(decimal.Zero)) ? num2 : current5.F_JYBZYLFZ);
								dynamicObject6.set_Item("BOMDenominator", (!num3.Equals(decimal.Zero)) ? num3 : current5.F_JYBZYLFM);
								dynamicObject6.set_Item("MustQty", (!num2.Equals(decimal.Zero) && !num3.Equals(decimal.Zero)) ? (num2 / num3 * current5.Qty) : (current5.F_JYBZYLFZ / current5.F_JYBZYLFM * current5.Qty));
							}
							dynamicObject6.set_Item("SupplyOrg_Id", current5.PrdOrgId_Id);
							dynamicObject6.set_Item("SupplyOrg", current5.PrdOrgId);
							dynamicObject6.set_Item("IssueType", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection6[0].get_Item("IssueType")) ? "2" : dynamicObjectCollection6[0].get_Item("IssueType"));
							dynamicObject6.set_Item("BackFlushType", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection6[0].get_Item("BKFLTime")) ? "2" : dynamicObjectCollection6[0].get_Item("BKFLTime"));
							dynamicObject6.set_Item("OverControlMode", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection6[0].get_Item("OverControlMode")) ? "2" : dynamicObjectCollection6[0].get_Item("OverControlMode"));
							dynamicObject6.set_Item("ReserveType", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection5[0].get_Item("ReserveType")) ? "1" : dynamicObjectCollection5[0].get_Item("ReserveType"));
							dynamicObject6.set_Item("UseRate", Convert.ToDouble("100"));
							dynamicObject6.set_Item("UnitID", dynamicObjectCollection6[0].get_Item("ProduceUnitId") as DynamicObject);
							dynamicObject6.set_Item("UnitID_Id", (dynamicObjectCollection6[0].get_Item("ProduceUnitId") as DynamicObject).get_Item("Id"));
							dynamicObject6.set_Item("BaseUnitID", dynamicObjectCollection4[0].get_Item("BaseUnitId") as DynamicObject);
							dynamicObject6.set_Item("BaseUnitID_Id", (dynamicObjectCollection4[0].get_Item("BaseUnitId") as DynamicObject).get_Item("Id"));
							OQLFilter oQLFilter3 = OQLFilter.CreateHeadEntityFilter(string.Format(" FStockId = '{0}' ", current7["flck"]));
							DynamicObject dynamicObject7 = BusinessDataServiceHelper.LoadFromCache(base.get_Context(), "BD_STOCK", null, oQLFilter3).FirstOrDefault<DynamicObject>();
							dynamicObject6.set_Item("StockID", ObjectUtils.IsNullOrEmptyOrWhiteSpace(current7["flck"]) ? (dynamicObjectCollection6[0].get_Item("PickStockId") as DynamicObject) : dynamicObject7);
							dynamicObject6.set_Item("StockID_Id", ObjectUtils.IsNullOrEmptyOrWhiteSpace(current7["flck"]) ? (dynamicObjectCollection6[0].get_Item("PickStockId") as DynamicObject).get_Item("Id") : current7["flck"]);
							dynamicObject6.set_Item("FixScrapQty", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObjectCollection6[0].get_Item("FIXLOSS")) ? "0" : dynamicObjectCollection6[0].get_Item("FIXLOSS"));
							dynamicObject6.set_Item("OperID", ObjectUtils.IsNullOrEmptyOrWhiteSpace(current7["operid"]) ? dynamicObject5.get_Item("FOperID") : current7["operid"]);
							dynamicObject6.set_Item("OptQueue", ObjectUtils.IsNullOrEmptyOrWhiteSpace(dynamicObject5.get_Item("FOptQueue")) ? "0" : dynamicObject5.get_Item("FOptQueue"));
							dynamicObject6.set_Item("NoPickedQty", dynamicObject6.get_Item("MustQty"));
							dynamicObject6.set_Item("StdQty", dynamicObject6.get_Item("MustQty"));
							dynamicObject6.set_Item("NeedQty", dynamicObject6.get_Item("MustQty"));
							dynamicObject6.set_Item("IsMrpRun", true);
							dynamicObject6.set_Item("OwnerTypeId", "BD_OwnerOrg");
							dynamicObject6.set_Item("OwnerID_Id", current5.PrdOrgId_Id);
							dynamicObject6.set_Item("OwnerID", current5.PrdOrgId);
							dynamicObject6.set_Item("StockStatusId", base3);
							dynamicObject6.set_Item("StockStatusId_Id", base3.get_Item("Id"));
							dynamicObject6.set_Item("NeedDate", current5.PlanFinishDate);
							decimal d = decimal.One;
							using (IDataReader dataReader2 = DBServiceHelper.ExecuteReader(base.get_Context(), "/*dialect*/" + string.Format("--目标单位=当前单位*换算分母/换算分子\r\n            select convert(float,FCONVERTDENOMINATOR/FCONVERTNUMERATOR) ConversionRate --换算分母/换算分子\r\n            from (\r\n\t            SELECT FDESTUNITID,FCURRENTUNITID,FCONVERTDENOMINATOR,FCONVERTNUMERATOR FROM T_BD_UNITCONVERTRATE\r\n\t            UNION --计量单位_转换率\r\n\t            SELECT FCURRENTUNITID FDESTUNITID,FDESTUNITID FCURRENTUNITID,FCONVERTNUMERATOR FCONVERTDENOMINATOR,FCONVERTDENOMINATOR FCONVERTNUMERATOR FROM T_BD_UNITCONVERTRATE\r\n            ) T\r\n            where FCURRENTUNITID = '{0}' --当前单位\r\n\t            and FDESTUNITID = '{1}' --目标单位", Convert.ToString(dynamicObject6.get_Item("UnitID_Id")), Convert.ToString(dynamicObject6.get_Item("BaseUnitID_Id")))))
							{
								while (dataReader2.Read())
								{
									bool flag12 = !ObjectUtils.IsNullOrEmptyOrWhiteSpace(dataReader2["ConversionRate"]);
									if (flag12)
									{
										d = Convert.ToDecimal(dataReader2["ConversionRate"]);
									}
								}
							}
							dynamicObject6.set_Item("BaseNoPickedQty", Convert.ToDecimal(dynamicObject6.get_Item("MustQty")) * d);
							dynamicObject6.set_Item("BaseStdQty", Convert.ToDecimal(dynamicObject6.get_Item("MustQty")) * d);
							dynamicObject6.set_Item("BaseNeedQty", Convert.ToDecimal(dynamicObject6.get_Item("MustQty")) * d);
							dynamicObject6.set_Item("BaseMustQty", Convert.ToDecimal(dynamicObject6.get_Item("MustQty")) * d);
							dynamicObject6.set_Item("GroupByOwnerId_Id", current5.PrdOrgId_Id);
							dynamicObject6.set_Item("GroupByOwnerId", current5.PrdOrgId);
							dynamicObject6.set_Item("PPBomEntryType", "0");
							dynamicObject6.set_Item("F_JY_CheckBox", true);
							num++;
							dynamicObjectCollection2.Add(dynamicObject6);
							list7.Add(Convert.ToString(dynamicObject5.get_Item("Id")));
							list.Add(common.UpDateRowId(Convert.ToString(dynamicObject2.get_Item("Id")), Convert.ToString(dynamicObject5.get_Item("Id"))));
						}
					}
				}
				common.UpDatePPBOM(base.get_Context(), current5.BillNo, current5.Seq, list7);
			}
		}
	}
	AppServiceContext.get_DBService().LoadReferenceObject(base.get_Context(), array, businessInfo.GetDynamicObjectType(), true);
	try
	{
		IOperationResult operationResult = BusinessDataServiceHelper.Save(base.get_Context(), businessInfo, array, null, "");
		bool isSuccess = operationResult.get_IsSuccess();
		if (isSuccess)
		{
			foreach (OperateResult current9 in operationResult.get_OperateResult())
			{
				base.get_OperationResult().get_OperateResult().Add(current9);
			}
		}
		else
		{
			foreach (ValidationErrorInfo current10 in operationResult.get_ValidationErrors())
			{
				base.get_OperationResult().get_ValidationErrors().Add(current10);
			}
		}
	}
	catch (Exception ex)
	{
		List<string> values = new List<string>
		{
			ex.Message,
			ex.StackTrace
		};
		Collection<OperateResult> arg_1487_0 = base.get_OperationResult().get_OperateResult();
		OperateResult expr_143E = new OperateResult();
		expr_143E.set_SuccessStatus(false);
		expr_143E.set_Name("PRD_PPBOM");
		expr_143E.set_Message(string.Join("\r\n", values));
		expr_143E.set_MessageType(1);
		expr_143E.set_PKValue(Guid.NewGuid().ToString("N"));
		arg_1487_0.Add(expr_143E);
		DynamicObject[] array3 = array;
		for (int k = 0; k < array3.Length; k++)
		{
			DynamicObject dynamicObject8 = array3[k];
			IOperationResult operationResult2 = BusinessDataServiceHelper.Save(base.get_Context(), businessInfo, dynamicObject8, null, "");
			bool isSuccess2 = operationResult2.get_IsSuccess();
			if (isSuccess2)
			{
				foreach (OperateResult current11 in operationResult2.get_OperateResult())
				{
					base.get_OperationResult().get_OperateResult().Add(current11);
				}
			}
			else
			{
				foreach (ValidationErrorInfo current12 in operationResult2.get_ValidationErrors())
				{
					base.get_OperationResult().get_ValidationErrors().Add(current12);
				}
			}
		}
	}
	DBServiceHelper.ExecuteBatch(base.get_Context(), list);
