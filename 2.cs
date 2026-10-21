// 1. 获取用料清单（PRD_PPBOM）的元数据
FormMetadata metaPPBOM = MetaDataServiceHelper.GetFormMetaData(this.Context, "PRD_PPBOM") as FormMetadata;

// 2. 加载你需要追加明细的用料清单主对象 (ppbomFid 为主表 FID)
DynamicObject ppbomObj = BusinessDataServiceHelper.LoadSingle(
    this.Context,
    ppbomFid,
    metaPPBOM.BusinessInfo.GetDynamicObjectType()
);

if (ppbomObj != null)
{
    // 3. 获取明细分录集合（FEntity）
    DynamicObjectCollection entryRows = ppbomObj["PPBOMEntry"] as DynamicObjectCollection; 
    // 注意：如果属性标识符不是 "PPBOMEntry"，可以通过 metaPPBOM.BusinessInfo.GetEntity("FEntity").DynamicProperty.Name 获取

    // 4. 创建一行全新的分录
    DynamicObject newRow = new DynamicObject(metaPPBOM.BusinessInfo.GetEntity("FEntity").DynamicObjectType);

    // 5. 设置分录的核心属性（直接设置在 newRow 上即可，系统会自动同步到 _C 和 _Q 表）
    newRow["MaterialId"] = objMaterial;           // 用料物料（DynamicObject 或传递 ID）
    newRow["MustQty"] = 10;                        // 标准应发数量
    newRow["MustQtyL"] = 10;                       // 辅助单位应发数量（如有）
    newRow["MoEntryId"] = fMOENTRYID;              // 关联生产订单行 ID
    
    // 如果要设置仓库、仓位、领料方式（底层存储在 _C / _Q 表的字段直接在这里赋 key 即可）：
    newRow["StockId"] = stockObj;                  // 发料仓库
    newRow["StockBinId"] = binObj;                 // 发料仓位
    newRow["IssueType"] = "1";                     // 领料方式：1-直接领料

    // 6. 将新分录追加到单据体集合中
    entryRows.Add(newRow);

    // 7. 保存整个用料清单对象
    IOperationResult saveResult = BusinessDataServiceHelper.Save(
        this.Context,
        metaPPBOM.BusinessInfo,
        new DynamicObject[] { ppbomObj }
    );

    // 8. 结果校验
    if (!saveResult.IsSuccess)
    {
        string errorMsg = string.Join("\r\n", saveResult.ValidationErrors.Select(e => e.Message));
        throw new Exception("新增用料分录失败：\r\n" + errorMsg);
    }
}