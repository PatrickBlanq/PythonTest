namespace Jac.XkDemo.BOS.App.PlugIn

{

    /// <summary>

    /// 【服务插件】列表批量操作时取消部分单据的操作

    /// </summary>

    [Description("【服务插件】列表批量操作时取消部分单据的操作"), HotUpdate]

    public class CancelSomeBillOpOperationServicePlugIn : AbstractOperationServicePlugIn

    {

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)

        {

            base.BeginOperationTransaction(e);

            var newObjs = new List<DynamicObject>();

            for (var i = 0; i < e.DataEntitys.Length; ++i)

            {

                var dynamicObject = e.DataEntitys[i];

                var billNo = dynamicObject["BillNo"] as string;

                if (billNo != null && (billNo.Contains("3") || billNo.Contains("4")))

                {

                    var error = new ValidationErrorInfo(

                        "", // 出错的字段Key，可以空

                        Convert.ToString(dynamicObject[0]), // 数据包内码，必填，后续操作会据此内码避开此数据包

                        i, // 出错的数据包在全部数据包中的顺序

                        i, // 出错的数据行在全部数据行中的顺序，如果校验基于单据头，此为0

                        "E1", // 错误编码，可以任意设定一个字符，主要用于追查错误来源

                        string.Format("单据编号【{0}】有非法字符，当前单据的操作取消！", billNo), // 错误的详细提示信息

                        "单据合法性检查", // 错误的简明提示信息

                        ErrorLevel.FatalError // 错误级别：警告、错误

                    );

                    this.OperationResult.ValidationErrors.Add(error);

                    continue;

                }

                newObjs.Add(dynamicObject);

            }

            e.DataEntitys = newObjs.ToArray();

        }

    }

}


  public class RollbackDataOperationServicePlugIn : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_Jac_Text");
        }
        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            // 调整当前操作的数据包
            var billNoField = this.BusinessInfo.GetBillNoField();
            foreach (var dataEntity in e.DataEntitys)
            {
                dataEntity["F_Jac_Text"] = dataEntity[billNoField.PropertyName];
            }
            // 模拟操作异常
            throw new Exception("操作事务内发生了异常，操作已终止！");
        }
        public override void RollbackData(OperationRollbackDataArgs e)
        {
            base.RollbackData(e);
            // 回滚当前操作的数据包
            foreach (var dataEntity in e.DataEntitys)
            {
                dataEntity["F_Jac_Text"] = "";
            }
        }
    }
    