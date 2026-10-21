

        internal Dictionary<string, object> GetMaterial(Common common,
            string materialField,
            DynamicObjectCollection bcyyldybs,
            DynamicObject treeEntity, int ator, string zxcxdl,
            string prdOrgId_Id)
        {
            // 判断字段是否为空
            if (ObjectUtils.IsNullOrEmptyOrWhiteSpace(treeEntity[materialField]))
                return null;

            decimal numerator = 0m;
            decimal denominator = 0m;

            // 遍历补充用料定义表
            foreach (DynamicObject row in bcyyldybs)
            {
                if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(row["F_JY_BaoCai"]))
                {
                    if (materialField.Equals(Convert.ToString(row["F_JY_BaoCai"])))
                    {
                        numerator = ObjectUtils.IsNullOrEmptyOrWhiteSpace(row["F_JY_YLFZ"])
                            ? decimal.Zero
                            : Convert.ToDecimal(row["F_JY_YLFZ"]);

                        denominator = ObjectUtils.IsNullOrEmptyOrWhiteSpace(row["F_JY_YLFM"])
                            ? decimal.Zero
                            : Convert.ToDecimal(row["F_JY_YLFM"]);
                    }
                }
            }

            // 读取物料元数据字段
            var materialMeta = MetaDataServiceHelper.GetFormMetaData(this.Context, "BD_MATERIAL");
            var fieldList = materialMeta.BusinessInfo.GetFieldList();
            var selectorKeys = fieldList.Select(f => f.Key).ToArray();

            // 加载物料基础资料
            var materialObj = treeEntity[materialField] as DynamicObject;
            string materialId = Convert.ToString(materialObj["Id"]);

            DynamicObject materialBase = common.GetBase(
                this.Context,
                "BD_MATERIAL",
                "FMATERIALID",
                materialId,
                SelectorItemInfo.CreateItems(selectorKeys));

            var materialBaseRows = materialBase["MaterialBase"] as DynamicObjectCollection;

            string categoryId = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialBaseRows[0]["CategoryID"])
                ? ""
                : Convert.ToString(((DynamicObject)materialBaseRows[0]["CategoryID"])["Id"]);

            string mujuType = ObjectUtils.IsNullOrEmptyOrWhiteSpace(materialBase["F_JY_MUJULX"])
                ? ""
                : Convert.ToString(materialBase["F_JY_MUJULX"]);

            string operId = string.Empty;
            string flck = string.Empty;

            // 查询工序与发料仓
            string sql = string.Format(@"
        select T2.FOPERID, T2.F_JY_FLCK
        from T_JY_CXGXDYB T1
        join T_JY_CXGXDYBEntry T2 on T1.FID = T2.FID
        where T1.FDOCUMENTSTATUS = 'C'
          and T2.F_SHDL_ZXCXDL = '{0}'
          and T2.FCATEGORYID = '{1}'
          and T2.F_JY_MUJULX = '{2}'
          and T1.FUSEORGID = '{3}'",
                zxcxdl, categoryId, mujuType, prdOrgId_Id);

            using (IDataReader reader = DBServiceHelper.ExecuteReader(this.Context, "/*dialect*/" + sql))
            {
                while (reader.Read())
                {
                    if (!ObjectUtils.IsNullOrEmptyOrWhiteSpace(reader["FOPERID"]))
                    {
                        operId = Convert.ToString(reader["FOPERID"]);
                        flck = Convert.ToString(reader["F_JY_FLCK"]);
                    }
                }
            }

            // 读取 BOM
            DynamicObject[] bomList = common.GetBases(
                this.Context,
                "ENG_BOM",
                "FMATERIALID",
                Convert.ToString(materialBase["Id"]));

            DynamicObject newestBom = new DynamicObject(MetaDataServiceHelper.GetFormMetaData(this.Context, "ENG_BOM").BusinessInfo.GetDynamicObjectType());

            decimal maxVersion = 0m;

            foreach (DynamicObject bom in bomList)
            {
                string number = Convert.ToString(bom["Number"]);
                string versionStr = number.Split('_')[1].Substring(1);
                decimal version = Convert.ToDecimal(versionStr);

                if (version > maxVersion)
                {
                    maxVersion = version;
                    newestBom = bom;
                }
            }

            // 返回结果字典
            return new Dictionary<string, object>
    {
        { "BOM", newestBom },
        { "material", materialBase },
        { "ator", ator },
        { "numerator", numerator },
        { "denominator", denominator },
        { "operid", operId },
        { "flck", flck }
    };
        }
