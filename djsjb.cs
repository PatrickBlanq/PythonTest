
DynamicObject billObj = this.Model.DataObject;


// 首先获取各种元素的元数据
Field fldBillNo = this.View.BillBusinessInfo.GetField("FBillNo");
Field fldDate = this.View.BillBusinessInfo.GetField("F_JD_Date");
BaseDataField fldSupplier = this.View.BillBusinessInfo.GetField("F_JD_Supplier") as BaseDataField;
BaseDataField fldMaterial = this.View.BillBusinessInfo.GetField("F_JD_FMaterialId") as BaseDataField;
Field fldQty = this.View.BillBusinessInfo.GetField("F_JD_Qty");
Entity entity = this.ListView.BillBusinessInfo.GetEntity("FEntity");
///这个是无敌的。
IViewService viewService = ServiceHelper.GetService();
DynamicObject[] stockObjs = viewService.LoadFromCache(this.Context,
new object[] { stockId },
// 读取单据内码
long billId = Convert.ToInt64(billObj[0]);
//单据编号
string billNo = Convert.ToDateTime(fldBillNo.DynamicProperty.GetValue(billObj));
fldBillNo.DynamicProperty.SetValue(billObj, billNo));
// 日期
DateTime fldDateValue = Convert.ToDateTime(fldDate.DynamicProperty.GetValue(billObj));
fldDate.DynamicProperty.SetValue(billObj, fldDateValue)
// 供应商：基础资料字段
DynamicObject fldSupplierValue = fldSupplier.DynamicProperty.GetValue(billObj) as DynamicObject;

// 设置供应商基础字段值
DynamicObject[] supplierObjs = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadFromCache(
this.Context,
new object[] { fldSupplierValue[0] },
fldSupplier.RefFormDynamicObjectType);

fldSupplier.RefIDDynamicProperty.SetValue(billObj, supplierObjs[0][0]);
fldSupplier.DynamicProperty.SetValue(billObj, supplierObjs[0]);

// 基础资料属性值
if (fldSupplierValue != null)
{
long supplierId = Convert.ToInt64(fldSupplierValue[0]);
string supplierNumber = fldSupplier.GetRefPropertyValue(fldSupplierValue, "FNumber").ToString();
string supplierName = fldSupplier.GetRefPropertyValue(fldSupplierValue, "FName").ToString();
}

// 单据体的字段
DynamicObjectCollection entityRows = entity.DynamicProperty.GetValue(billObj) as DynamicObjectCollection;
foreach (var entityRow in entityRows)
{
// 内码
long entityId = Convert.ToInt64(entity1Row[0]);
// 物料：基础资料字段
DynamicObject fldMaterialValue = fldMaterial.DynamicProperty.GetValue(entityRow) as DynamicObject;
if (fldMaterialValue != null)
{
long materialId = Convert.ToInt64(fldMaterialValue[0]);
string materialNumber = fldMaterial.GetRefPropertyValue(fldMaterialValue, "FNumber").ToString();
string materialName = fldMaterial.GetRefPropertyValue(fldMaterialValue, "FName").ToString();
}
// 数量
decimal fldQtyValue = Convert.ToDecimal(fldQty.DynamicProperty.GetValue(entityRow));
fldQty.DynamicProperty.SetValue(entityRow, fldQtyValue);
}

// 给单据体添加新行
DynamicObject newRow = new DynamicObject(entity.DynamicObjectType);
entityRows.Add(newRow);

 
// 物料字段（基础资料）
DynamicObject[] materialObjs = Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.LoadFromCache(
    this.Context,
    new object[] { 100001 }, // 假设物料内码
    fldMaterial.RefFormDynamicObjectType);

fldMaterial.RefIDDynamicProperty.SetValue(newRow, materialObjs[0][0]);
fldMaterial.DynamicProperty.SetValue(newRow, materialObjs[0]);

// 数量字段
fldQty.DynamicProperty.SetValue(newRow, 10m);

// 其他普通字段
fldDate.DynamicProperty.SetValue(billObj, DateTime.Now);
fldBillNo.DynamicProperty.SetValue(billObj, "JD20260921001");

// 保存单据
Kingdee.BOS.ServiceHelper.BusinessDataServiceHelper.Save(
    this.Context,
    this.View.BillBusinessInfo,
    billObj);



IOperationResult result =
    BusinessDataServiceHelper.Save(
        this.Context,
        new DynamicObject[]
        {
            billObj
        }
    );

