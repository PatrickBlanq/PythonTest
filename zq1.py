from System import DateTime

def DataChanged(e):
    key = e.Field.Key.ToUpper()

    if key == "F_JY_YINSEDATE":  # 如果是印色日期      
        dt = this.Model.GetValue('F_JY_YINSEDate') 
        if dt:
            result = get_target_month(dt)
            this.Model.SetValue("F_SHDL_zxyxqzy", result)   # 回写测试


def get_target_month(dt):
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
