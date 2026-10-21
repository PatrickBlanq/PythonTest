import clr
clr.AddReference("Kingdee.BOS")
clr.AddReference("Kingdee.BOS.Core")
clr.AddReference("Kingdee.BOS.App")
clr.AddReference("System")
clr.AddReference("System.Core")
from Kingdee.BOS import *
from Kingdee.BOS.Core import *
from Kingdee.BOS.App import *
from Kingdee.BOS.App.Data import *
from System.Collections.Generic import *
from System import *
def AfterBarItemClick(e):
    if(e.BarItemKey=="BGP_tbButton_1"):
        userName = this.Context.UserName
        if (userName<>"张强"): 
            this.View.ShowErrMessage("没有权限")
            return
        entryfid = []
        n=0
        rows = this.ListView.SelectedRowsInfo
        this.View.ShowMessage("选中了{0}行，更新成功{1}行".format(rows.Count,len(rows)))
        return
        if(len(rows)==0):
            this.View.ShowMessage("请先选择行!")
            return
        for i in range(len(rows)):
            e=str(rows[i].PrimaryKeyValue)
            if e not in entryfid:
                entryfid.append(e)
        fids = list(set(entryfid))
        entryfid = []
        for i in range(len(fids)):
            entryfid.append(fids[i])
            if(i>=100 or i == len(fids)-1):
                n=n+up(entryfid)
                entryfid = []
        this.ListView.Refresh()
        this.View.ShowMessage("选中了{0}行，更新成功{1}行".format(rows.Count,n))
        	
def up(fid):
    ids = list(set(fid))
    s="','"
    filterstr = s.join(ids)
    sql = "/*dialect*/ UPDATE T_SAL_ORDERENTRY SET F_BGP_zxkhjmb =isnull(( SELECT DISTINCT c.Fid from T_SAL_ORDER a INNER JOIN T_SAL_ORDERENTRY b ON b.FID = a.FID INNER JOIN T_SAL_ORDERENTRY_F d ON d.FID = b.FID INNER JOIN SHDL_t_Cust100053 C ON C.F_BGP_HS001 = B.F_BGP_HS001 AND C.F_BGP_BZXS001 = B.F_BGP_BZXS001 AND C.FCUSTID = A.FCUSTID AND C.F_BGP_ZXJMLX = b.F_BGP_ZXJMLX AND C.F_BGP_zxdjsfzk = b.F_BGP_zxdjsfzk AND C.FTaxPrice = d.FTAXPRICE WHERE b.Fid in ('" + filterstr + "')),0) WHERE FID in ('" + filterstr + "')";
    #this.View.ShowMessage(sql)
    #c = DBUtils.Execute(this.Context, sql)
    return 1 #c