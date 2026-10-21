
#引入clr运行库
import clr
#添加对cloud插件开发的常用组件的引用
clr.AddReference("System")
clr.AddReference("System.Web.Extensions")
clr.AddReference("Kingdee.BOS")
clr.AddReference("Kingdee.BOS.Core")
clr.AddReference("Newtonsoft.Json")
#导入cloud基础库中的常用实体对象（分命名空间导入，不会递归导入）
import sys
from System import *
from System.Collections.Generic import *
from System.Threading import *
from System.IO import *
from System.Net import *
from System.Text import *
from System.Security.Cryptography import *
from System.Web.Script.Serialization import *
from System.Collections.Generic import Dictionary
from Newtonsoft.Json.Linq import *


def DataChanged(e):
    key = e.Field.Key.ToUpper()

    if (key == "F_JY_YINSEDATE"):  # 如果是印色日期      
        F_JY_YINSEDate = this.Model.GetValue('F_JY_YINSEDate')#取印色日期         
        F_JY_Date = calculate_F_JY_Date(F_JY_YINSEDate) #用印色日期计算有效期
        F_SHDL_zxyxqzy = get_target_month(F_JY_Date)#用 有效期 计算 有限期至月
        this.Model.SetValue("F_SHDL_zxyxqzy", F_SHDL_zxyxqzy)  #写到有限期至月


def get_target_month(dt):
    #当月最后一天返回本月,否则返回上个月
    today = dt  

    first_day = DateTime(today.Year, today.Month, 1)

    next_month = first_day.AddDays(32)
    next_month_first = DateTime(next_month.Year, next_month.Month, 1)

    final_day_month = next_month_first.AddDays(-1)

    if today.Date == final_day_month.Date:
        return today.ToString("yyyy-MM")
    else:
        last_month_last_day = first_day.AddDays(-1)
        return last_month_last_day.ToString("yyyy-MM")

def calculate_F_JY_Date(dt):
    # 加 5 年，再减 1 天
    return dt.AddYears(5).AddDays(-1)
