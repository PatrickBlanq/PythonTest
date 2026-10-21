def DataChanged(e):
    key = e.Field.Key.ToUpper()

    if key == "F_JY_YINSEDATE":
      


        produceDate = e.NewValue
        if produceDate is None:
            return

        # 假设保质期天数从单据头某字段取，或写死
        # 例：从单据头 FShelfLife (整数) 取
        shelfLife = model.GetValue("FShelfLife")
        if shelfLife is None or shelfLife == 0:
            shelfLife = 730   # 默认两年

        # 计算有效期
        expDate = DateTime.Parse(str(produceDate)).AddDays(int(shelfLife))

        # 遍历明细单据体 (假设明细 Key = FEntity, 有效期字段 Key = FExpiryDate)
        entryKey = "FEntity"
        rowCount = model.GetEntryRowCount(entryKey)

        for i in range(rowCount):
            model.SetValue("FExpiryDate", expDate, i)

        # 刷新界面
        view.UpdateView(entryKey)