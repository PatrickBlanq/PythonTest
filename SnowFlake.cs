// ============================================================
// 一、获取当前单据的数据对象
// ============================================================

// this.Model.DataObject
// 表示：当前界面正在操作的“整张单据”的实际数据。
//  billObj  单据的一整行数据
// 它不是“单据结构定义”，而是“真正的数据”。
// 类型是 DynamicObject，里面保存了单据头、单据体等数据。
DynamicObject billObj = this.Model.DataObject;


// ============================================================
// 二、获取单据字段的“元数据”
// ============================================================
//
// 注意：下面这些 fldXXX 不是具体的数据！
//
// 它们是“字段定义 / 元数据”。
// 可以理解成：
//     fldBillNo  = 告诉程序“FBillNo这个字段是什么”
//     fldDate    = 告诉程序“F_JD_Date这个字段是什么”
//     fldSupplier= 告诉程序“F_JD_Supplier这个字段是什么”
//
// 真正的数据还在 billObj 里面。
//     Field / BaseDataField / Entity
//             ↓
//          字段定义
//     DynamicObject / DynamicObjectCollection
//             ↓
//          实际数据
// ------------------------------------------------------------
// 1、获取单据编号字段的元数据
// ------------------------------------------------------------
Field fldBillNo = this.View.BillBusinessInfo.GetField("FBillNo");

// fldBillNo：
// 表示“FBillNo这个字段的定义”
// 注意：它不是 "PO000001" 这样的具体值。
// 如果想取真正的单据编号，要通过：
//     fldBillNo.DynamicProperty.GetValue(billObj)
// ------------------------------------------------------------
// 2、获取日期字段的元数据
// ------------------------------------------------------------
Field fldDate =
    this.View.BillBusinessInfo.GetField("F_JD_Date");

// fldDate 表示 F_JD_Date 这个字段的“定义”。


// ------------------------------------------------------------
// 3、获取供应商字段的元数据
// ------------------------------------------------------------
//
// F_JD_Supplier 是基础资料字段。
// 因此它的类型不是普通 Field，而是 BaseDataField。
//
// BaseDataField 本身也是一种 Field，
// 只不过它额外知道：
//     这个字段关联的是哪个基础资料。
// 例如：供应商、客户、物料、部门、员工等。
//
BaseDataField fldSupplier =
    this.View.BillBusinessInfo.GetField("F_JD_Supplier")
    as BaseDataField;


// ------------------------------------------------------------
// 4、获取物料字段的元数据
// ------------------------------------------------------------
//
// F_JD_FMaterialId 同样是基础资料字段。
// 所以使用 BaseDataField。
//
BaseDataField fldMaterial =
    this.View.BillBusinessInfo.GetField("F_JD_FMaterialId")
    as BaseDataField;


// ------------------------------------------------------------
// 5、获取数量字段的元数据
// ------------------------------------------------------------
Field fldQty =
    this.View.BillBusinessInfo.GetField("F_JD_Qty");


// ------------------------------------------------------------
// 6、获取单据体 FEntity 的元数据
// ------------------------------------------------------------
//
// Entity 和 Field 不一样。
//
// Field：描述“一个字段”
// Entity：描述“一个单据体”
//
// 比如：
//
//     FEntity
//       ├── F_JD_FMaterialId
//       ├── F_JD_Qty
//       └── 其他字段
//
// entity 就是“FEntity这个单据体的定义”。
//
Entity entity =
    this.ListView.BillBusinessInfo.GetEntity("FEntity");


// ============================================================
// 三、读取单据头中的数据
// ============================================================

// ------------------------------------------------------------
// 1、读取单据内码
// ------------------------------------------------------------
//
// billObj 是当前整张单据的数据对象。
// DynamicObject 可以通过 [0]、[字段属性] 等方式获取数据。
//
// 这里假设 billObj[0] 对应单据主键 FID。
// 实际项目里更推荐根据字段元数据读取，这样可读性更好。
//
long billId =
    Convert.ToInt64(billObj[0]);


// ------------------------------------------------------------
// 2、读取单据编号
// ------------------------------------------------------------
//
// fldBillNo 是字段“定义”。
// billObj 是真正的数据。
//
// fldBillNo.DynamicProperty
//      ↓
// 表示 FBillNo 这个字段对应的 DynamicProperty
//
// GetValue(billObj)
//      ↓
// 从 billObj 中读取 FBillNo 的真实值
//
// 例如：
//     FBillNo = "CGDD0001"
//
// 注意：这里应该转换成 string，
// 原代码写成 Convert.ToDateTime 是不正确的。
//
string billNo =
    Convert.ToString(
        fldBillNo.DynamicProperty.GetValue(billObj)
    );


// ------------------------------------------------------------
// 3、给单据编号重新赋值
// ------------------------------------------------------------
//
// SetValue(对象, 值)
//
// 意思就是：
//     把 billNo 写回 billObj 的 FBillNo 字段。
//
// 可以理解为：
//
//     fldBillNo → 告诉程序我要修改哪个字段
//     billObj   → 告诉程序我要修改哪张单据
//     billNo    → 修改成什么值
//
fldBillNo.DynamicProperty.SetValue(
    billObj,
    billNo
);

DateTime fldDateValue =
    Convert.ToDateTime(
        fldDate.DynamicProperty.GetValue(billObj)
    );


fldDate.DynamicProperty.SetValue(
    billObj,
    fldDateValue
);


// ============================================================
// 四、读取“供应商”基础资料
// ============================================================

// ------------------------------------------------------------
// 1、获取当前单据里的供应商对象
// ------------------------------------------------------------
//
// 因为 F_JD_Supplier 是 BaseDataField，
// 所以它对应的值通常不是简单的 long 或 string，
// 而是一个 DynamicObject。
//
// 例如：
//
//     billObj
//       │
//       └── F_JD_Supplier
//              │
//              ▼
//       DynamicObject
//          [0] = 100001
//          FNumber = "GYS001"
//          FName   = "北京供应商"
//
// DynamicProperty.GetValue(billObj)
//     ↓
// 获取 billObj 中 F_JD_Supplier 的实际值。
//
DynamicObject fldSupplierValue =
    fldSupplier.DynamicProperty.GetValue(billObj)
    as DynamicObject;


// ============================================================
// 五、根据供应商内码，加载供应商业务对象
// ============================================================
//
// 这一句是整个代码中最容易理解错的地方。
//
// fldSupplierValue：
//     当前单据中已经存在的“供应商对象”
//
// fldSupplierValue[0]：
//     通常是这个供应商的内码
//
// 假设：
//     fldSupplierValue[0] = 100001
//
// 那么：
//
//     new object[] { fldSupplierValue[0] }
//
// 实际就是：
//
//     new object[] { 100001 }
//
// 表示：
//     我要加载内码为 100001 的基础资料。
//
// ------------------------------------------------------------
//
// fldSupplier.RefFormDynamicObjectType
//
// 表示：
//     F_JD_Supplier 这个基础资料字段，
//     所引用的“基础资料业务对象类型”。
//
// 也就是说告诉金蝶：
//
//     100001 是什么对象？
//     是供应商！
//
// ------------------------------------------------------------
//
// LoadFromCache
//
// 表示通过 BusinessDataServiceHelper 从缓存中加载业务对象。
// 返回的是 DynamicObject[]，因为这个方法支持一次加载多个对象。
//
// 例如：
//     new object[] { 100001, 100002, 100003 }
//
// 就可能返回：
//
//     supplierObjs[0] → 供应商100001
//     supplierObjs[1] → 供应商100002
//     supplierObjs[2] → 供应商100003
//
// 你现在只传一个ID，所以通常只有一个对象。
//
DynamicObject[] supplierObjs =
    BusinessDataServiceHelper.LoadFromCache(
        this.Context,

        // 要加载的基础资料内码集合
        new object[]
        {
            fldSupplierValue[0]
        },

        // 供应商基础资料对应的 DynamicObject 类型
        fldSupplier.RefFormDynamicObjectType
    );


// ============================================================
// 六、把加载出来的供应商对象写回单据
// ============================================================

// ------------------------------------------------------------
// 1、设置供应商ID
// ------------------------------------------------------------
//
// supplierObjs[0]
//     ↓
// 加载出来的第一个供应商 DynamicObject
//
// supplierObjs[0][0]
//     ↓
// 这个供应商 DynamicObject 的内码
//
// 所以：
//     supplierObjs[0][0]
// 通常就是：
//     100001
//
// RefIDDynamicProperty
//     ↓
// BaseDataField 对应的“内码属性”
//
// SetValue(billObj, supplierObjs[0][0])
//     ↓
// 把供应商内码写进当前单据。
//
fldSupplier.RefIDDynamicProperty.SetValue(
    billObj,
    supplierObjs[0][0]
);


// ------------------------------------------------------------
// 2、设置供应商完整对象
// ------------------------------------------------------------
//
// DynamicProperty 对应的是供应商这个字段本身。
//
// 这里把：
//
//     supplierObjs[0]
//
// 整个供应商 DynamicObject 放进去。
//
// 所以：
//
//     RefIDDynamicProperty
//         ↓
//       供应商ID
//
//     DynamicProperty
//         ↓
//       供应商完整对象
//
//
fldSupplier.DynamicProperty.SetValue(
    billObj,
    supplierObjs[0]
);


// ============================================================
// 七、读取供应商的具体属性
// ============================================================
//
// 这一段建议放在 fldSupplierValue != null 之后，
// 防止空对象导致异常。
//

if (fldSupplierValue != null)
{
    // --------------------------------------------------------
    // 1、读取供应商内码
    // --------------------------------------------------------
    //
    // fldSupplierValue[0]
    //     ↓
    // 供应商内码
    //
    long supplierId =
        Convert.ToInt64(fldSupplierValue[0]);


    // --------------------------------------------------------
    // 2、读取供应商编码
    // --------------------------------------------------------
    //
    // fldSupplierValue 本身是 DynamicObject。
    //
    // 但是基础资料字段有专门的方法：
    //
    //     GetRefPropertyValue(...)
    //
    // 用来获取基础资料引用对象的属性。
    //
    // "FNumber"
    //     ↓
    // 供应商编码
    //
    string supplierNumber =
        fldSupplier.GetRefPropertyValue(
            fldSupplierValue,
            "FNumber"
        ).ToString();


    // --------------------------------------------------------
    // 3、读取供应商名称
    // --------------------------------------------------------
    //
    // "FName"
    //     ↓
    // 供应商名称
    //
    string supplierName =
        fldSupplier.GetRefPropertyValue(
            fldSupplierValue,
            "FName"
        ).ToString();
}


// ============================================================
// 八、获取单据体的实际数据
// ============================================================
//
// 前面的 entity 是：
//     FEntity 单据体“定义”
//
// 这里的 entityRows 才是：
//     FEntity 当前实际有多少行数据。
//
// DynamicObjectCollection 可以理解成：
//
//     一个 DynamicObject 集合
//
// 比如当前单据有3行：
//
//     entityRows
//       │
//       ├── entityRow  ← 第1行
//       ├── entityRow  ← 第2行
//       └── entityRow  ← 第3行
//
//
//
// entity.DynamicProperty.GetValue(billObj)
//
// 表示：
//     从当前单据 billObj 中，
//     把 FEntity 这个单据体的数据取出来。
//
DynamicObjectCollection entityRows =
    entity.DynamicProperty.GetValue(billObj)
    as DynamicObjectCollection;


// ============================================================
// 九、逐行处理单据体
// ============================================================

// entityRow 就是单据体中的“一行”。
// 每一行本身也是一个 DynamicObject。
//
// 例如：
//
//     第1行
//        F_JD_FMaterialId = 物料A
//        F_JD_Qty         = 10
//
//     第2行
//        F_JD_FMaterialId = 物料B
//        F_JD_Qty         = 20
//
// 那么 foreach 就是一行一行处理。
//
foreach (DynamicObject entityRow in entityRows)
{
    // --------------------------------------------------------
    // 1、读取单据体行内码
    // --------------------------------------------------------
    //
    // 原代码这里写的是 entity1Row[0]，应该是 entityRow。
    //
    // entityRow[0]
    //     ↓
    // 当前单据体行的内码
    //
    long entityId =
        Convert.ToInt64(entityRow[0]);


    // --------------------------------------------------------
    // 2、获取当前行的“物料”对象
    // --------------------------------------------------------
    //
    // fldMaterial：
    //     F_JD_FMaterialId 字段的元数据
    //
    // entityRow：
    //     当前单据体这一行的数据
    //
    // GetValue(entityRow)
    //     ↓
    // 获取当前行的物料值。
    //
    // 因为物料是基础资料，
    // 所以得到的通常也是 DynamicObject。
    //
    DynamicObject fldMaterialValue =
        fldMaterial.DynamicProperty.GetValue(entityRow)
        as DynamicObject;


    // --------------------------------------------------------
    // 3、判断物料是否为空
    // --------------------------------------------------------
    //
    if (fldMaterialValue != null)
    {
        // ----------------------------------------------------
        // 3.1、物料内码
        // ----------------------------------------------------
        //
        // fldMaterialValue[0]
        //     ↓
        // 物料内码
        //
        long materialId =
            Convert.ToInt64(fldMaterialValue[0]);


        // ----------------------------------------------------
        // 3.2、物料编码
        // ----------------------------------------------------
        //
        // "FNumber"
        //     ↓
        // 物料编码
        //
        string materialNumber =
            fldMaterial.GetRefPropertyValue(
                fldMaterialValue,
                "FNumber"
            ).ToString();


        // ----------------------------------------------------
        // 3.3、物料名称
        // ----------------------------------------------------
        //
        // "FName"
        //     ↓
        // 物料名称
        //
        string materialName =
            fldMaterial.GetRefPropertyValue(
                fldMaterialValue,
                "FName"
            ).ToString();
    }


    // --------------------------------------------------------
    // 4、读取当前行的数量
    // --------------------------------------------------------
    //
    // fldQty：
    //     F_JD_Qty 这个字段的定义
    //
    // entityRow：
    //     当前单据体行
    //
    // GetValue(entityRow)
    //     ↓
    // 从当前行读取数量
    //
    decimal fldQtyValue =
        Convert.ToDecimal(
            fldQty.DynamicProperty.GetValue(entityRow)
        );


    // --------------------------------------------------------
    // 5、把数量重新设置回去
    // --------------------------------------------------------
    //
    // 这里只是演示 SetValue 的用法。
    //
    // 实际上：
    //
    //     读取数量
    //     ↓
    //     没有修改
    //     ↓
    //     再写回去
    //
    // 业务上没有实际变化。
    //
    // 如果你的目的是修改数量，
    // 可以改成：
    //
    //     fldQty.DynamicProperty.SetValue(
    //         entityRow,
    //         100
    //     );
    //
    fldQty.DynamicProperty.SetValue(
        entityRow,
        fldQtyValue
    );
}


// ============================================================
// 十、给单据体增加一行
// ============================================================
//
// 到这里：
//
//     entity
//         ↓
//     FEntity 单据体的“定义”
//
//     entityRows
//         ↓
//     FEntity 当前实际数据
//
// 如果要新增一行，必须先创建一个符合 FEntity
// 数据结构的 DynamicObject。
//
// entity.DynamicObjectType
//     ↓
//     FEntity 每一行应该使用什么 DynamicObject 类型
//
// 所以：
//
DynamicObject newRow =
    new DynamicObject(
        entity.DynamicObjectType
    );


// ------------------------------------------------------------
// 把新行加入单据体
// ------------------------------------------------------------
//
// Add(newRow)
//     ↓
// 把新创建的这一行加入 FEntity 数据集合。
//
// 加完以后：
//
//     entityRows
//       ├── 原来的第1行
//       ├── 原来的第2行
//       ├── 原来的第3行
//       └── newRow       ← 新增
//
entityRows.Add(newRow);
```
