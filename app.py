from datetime import datetime, date, timedelta

def get_target_month():
    #today = datetime.strptime("2026/07/31", "%Y/%m/%d").date()
    today = date.today()

    final_day_month = (today.replace(day=1) + timedelta(days=32)).replace(day=1) - timedelta(days=1)

    if today == final_day_month:
        return today.strftime("%Y-%m")
    else:
        first_day_this_month = today.replace(day=1)
        last_month_last_day = first_day_this_month - timedelta(days=1)
        return last_month_last_day.strftime("%Y-%m")

print(get_target_month())
