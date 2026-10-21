
public enum TimeBlockType
{
    StandardWork, // 计划工作时间
    CheckIn,      // 实际打卡覆盖时间
    Leave,        // 请假单
    OvertimeApp,  // 加班申请单
    BusinessTrip  // 出差单
}

public class TimeBlock
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeBlockType Type { get; set; }
    public string SubType { get; set; } // 如：事假、病假、平日加班
}

public interface IAttendancePipeline
{
    void Process(AttendanceContext context);
}

public class AttendanceContext
{
    public string MemberId { get; set; }
    public DateTime Date { get; set; }
    public TimeBlock StandardShift { get; set; }     // 标准班次
    public TimeBlock ActualPunch { get; set; }        // 实际打卡
    public List<TimeBlock> Applications { get; set; } // 各种单据列表
    
    // 最终计算结果
    public decimal FinalWorkHours { get; set; }
    public decimal LateMinutes { get; set; }
    public decimal LeaveHours { get; set; }
    public decimal OvertimeHours { get; set; }
}
public class LeaveProcessor : IAttendancePipeline
{
    public void Process(AttendanceContext context)
    {
        var leaveBlocks = context.Applications
            .Where(a => a.Type == TimeBlockType.Leave)
            .ToList();

        if (!leaveBlocks.Any()) return;

        foreach (var leave in leaveBlocks)
        {
            // 计算请假与标准上班时间的重叠部分（扣减应出勤或抵扣迟到）
            var overlap = GetOverlap(context.StandardShift, leave);
            context.LeaveHours += (decimal)overlap.TotalHours;
        }
    }

    private TimeSpan GetOverlap(TimeBlock b1, TimeBlock b2)
    {
        var start = b1.StartTime > b2.StartTime ? b1.StartTime : b2.StartTime;
        var end = b1.EndTime < b2.EndTime ? b1.EndTime : b2.EndTime;
        return end > start ? end - start : TimeSpan.Zero;
    }
}
 public class OvertimeProcessor : IAttendancePipeline
{
    public void Process(AttendanceContext context)
    {
        var otApp = context.Applications.FirstOrDefault(a => a.Type == TimeBlockType.OvertimeApp);
        if (otApp == null) return; // 无申请单不计加班

        // 取“打卡时间”与“加班申请时间”的交集（按两者最小值计算）
        var validStart = context.ActualPunch.StartTime > otApp.StartTime ? context.ActualPunch.StartTime : otApp.StartTime;
        var validEnd = context.ActualPunch.EndTime < otApp.EndTime ? context.ActualPunch.EndTime : otApp.EndTime;

        if (validEnd > validStart)
        {
            var validOtMinutes = (validEnd - validStart).TotalMinutes;
            // 按半小时向下取整规则
            context.OvertimeHours = (decimal)Math.Floor(validOtMinutes / 30.0) * 0.5m;
        }
    }
}
public class AttendanceEngine
{
    private readonly List<IAttendancePipeline> _pipelines;

    public AttendanceEngine()
    {
        // 挂载单据处理流水线，顺序执行
        _pipelines = new List<IAttendancePipeline>
        {
            new LeaveProcessor(),       // 1. 处理请假抵扣
            new OvertimeProcessor(),    // 2. 处理加班单核销
            new PunchCorrectionProcessor() // 3. 处理补卡单
        };
    }

    public AttendanceContext Calculate(AttendanceContext context)
    {
        foreach (var pipeline in _pipelines)
        {
            pipeline.Process(context);
        }
        return context;
    }
}