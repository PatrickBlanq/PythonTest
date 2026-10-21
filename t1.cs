using Kingdee.BOS.Core.DynamicForm.PlugIn;

using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

using Kingdee.BOS.Util;

using System.ComponentModel;



namespace Jac.XkDemo.BOS.Business.PlugIn

{

    /// <summary>

    /// 【表单插件】锁定解锁单元格

    /// </summary>

    [Description("【表单插件】锁定解锁单元格"), HotUpdate]

    public class LockGridCellFormPlugIn : AbstractDynamicFormPlugIn

    {

        public override void BarItemClick(BarItemClickEventArgs e)

        {

            base.BarItemClick(e);

            var entityKey = "FPOOrderEntry";

            var entity = this.View.BusinessInfo.GetEntity(entityKey);

            var fieldKey = this.Model.GetEntryCurrentFieldKey(entityKey);

            var rowIndex = this.Model.GetEntryCurrentRowIndex(entityKey);

            var rowData = this.Model.GetEntityDataObject(entity, rowIndex);

            if (e.BarItemKey.EqualsIgnoreCase("LockCell"))

            {

                this.View.StyleManager.SetEnabled(fieldKey, rowData, null, false);

                this.View.ShowMessage("单元格已锁定！");

            }

            else if (e.BarItemKey.EqualsIgnoreCase("UnLockCell"))

            {

                this.View.StyleManager.SetEnabled(fieldKey, rowData, null, true);

                this.View.ShowMessage("单元格已解锁！");

            }

        }

    }


                if (this.FormOperation.OperationId == 8)

            {

                foreach (var dataEntity in e.DataEntitys)

                {

                    // 直接SQL修改备注字段的数据

                    var sql = string.Format("UPDATE {0} SET F_Jac_Remarks=N'保存时间：{1}',FChangeReason=N'保存时间：{1}' WHERE FID={2}", 
                    this.BusinessInfo.GetEntity(0).TableName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), dataEntity[0]);

                    DBUtils.Execute(Context, sql);

                }

            }
 


}