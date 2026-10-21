public interface IBoxPrintService
{
    void Print(BoxData data);
}

public class CustomerAPrintStrategy : IBoxPrintStrategy
{
    public void Print(BoxData data)
    {
         
    }
}

public class PrintStrategyFactory
{
    public static IBoxPrintStrategy Create(string printType)
    {
        return printType switch
        {
            "NORMAL" => new NormalPrintStrategy(),
            "EXPORT" => new ExportPrintStrategy(),
            "GS1" => new GS1PrintStrategy(),
            _ => throw new Exception("未知打印方案")
        };
    }
}

var strategy = PrintStrategyFactory.Create(printType);
strategy.Print(data);