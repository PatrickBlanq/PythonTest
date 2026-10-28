#!/bin/bash
# 自动关机脚本 - 当本月出站流量 >= LIMIT_GB 时触发
# 依赖：vnstat
# 注意：shutdown是关机标志
# OFF_GB是偏移

WEBHOOK_URL="https://discord.com/api/webhooks/1504765562537115668/wxD2Wk-9g4tmNwfaWUa8RK2J0cuauFkNoJ6Ezo8YOjOWJMsW4KFyH1yd9wA5Vd-gIjld"

LIMIT_GB=199.8
OFF_GB=5
MONTH=$(date +%Y-%m)
FLAG="/root/shutdown.${MONTH}.flag"
WARN_FLAG="/root/traffic.${MONTH}.flag"

VNSTAT_BIN=$(which vnstat)

# 如果标志文件存在，则退出
if [ -f "$FLAG" ]; then
    echo "已触发过关机，本月内不再执行"
    exit 0
fi

# 获取当月出站流量
OUT_RAW=$($VNSTAT_BIN -m | awk -v ym="$(date +%Y-%m)" '$1==ym {print $5" "$6}')

# 判断并换算
if [[ $OUT_RAW == *GiB* ]]; then
    OUT_GB=$(echo "$OUT_RAW" | sed 's/GiB//' | awk '{print $1}')
elif [[ $OUT_RAW == *MiB* ]]; then
    OUT_GB=$(echo "$OUT_RAW" | sed 's/MiB//' | awk '{printf "%.2f",$1/1024}')
elif [[ $OUT_RAW =~ ^[0-9.]+$ ]]; then
    OUT_GB=$OUT_RAW
else
    echo "无法识别 vnstat 单位: $OUT_RAW"
    exit 1
fi

echo "当前出站流量: ${OUT_GB} GB (限制: ${LIMIT_GB} GB) 提示流量 $((LIMIT_GB - OFF_GB)) GB"

# 发送 Discord 消息函数
send_discord() {
    local MSG="$1"
    curl -s -X POST "$WEBHOOK_URL" \
        -H "Content-Type: application/json" \
        -d "{\"content\": \"$MSG\"}"
}

# 提前提醒：超过 LIMIT_GB - OFF_GB
if awk "BEGIN {exit !($OUT_GB >= ($LIMIT_GB - $OFF_GB))}"; then

    # 未提醒过才发送
    if [ ! -f "$WARN_FLAG" ]; then
        date > "$WARN_FLAG"
        WARN_MSG="⚠ GCP当前流量 ${OUT_GB} GB，接近上限 ${LIMIT_GB} GB！"
        echo "$WARN_MSG"
        send_discord "$WARN_MSG"
    fi
fi


# 超过上限：关机
if awk "BEGIN {exit !($OUT_GB >= $LIMIT_GB)}"; then
    date >> $FLAG
    SHUT_MSG="⛔ 流量已超过 ${LIMIT_GB} GB（当前 ${OUT_GB} GB），系统将关机！"
    echo "$SHUT_MSG"

    send_discord "$SHUT_MSG"

    
    /sbin/shutdown -h now
fi