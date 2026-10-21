namespace ConsoleApp1;


internal class Program
{
    static void Main(string[] args)
    {
        var material = new MaterialHandler();
        var lot = new LotHandler();
        var qty = new QtyHandler();

        material.SetNext(lot).SetNext(qty);
        material.Handle(
            new PrintContext
            {
                MaterialNo = "A001",
                LotNo = "LOT001",
                Qty = 100
            });
    }
}
public class PrintContext
{
    public string MaterialNo { get; set; }

    public string LotNo { get; set; }

    public int Qty { get; set; }
}

public abstract class Handler
{
    protected Handler Next;

    public Handler SetNext(Handler next)
    {
        Next = next;

        return next;
    }

    public virtual void Handle(PrintContext context)
    {
        if (Next != null)
        {
            Next.Handle(context);
        }
    }
}

public class MaterialHandler : Handler
{
    public override void Handle(PrintContext context)
    {
        if (string.IsNullOrWhiteSpace(context.MaterialNo))
        {
            throw new Exception("物料不能为空");
        }

        Console.WriteLine("物料检查通过");

        base.Handle(context);
    }
}

public class LotHandler : Handler
{
    public override void Handle(PrintContext context)
    {
        if (string.IsNullOrWhiteSpace(context.LotNo))
        {
            throw new Exception("批号不能为空");
        }

        Console.WriteLine("批号检查通过");

        base.Handle(context);
    }
}

public class QtyHandler : Handler
{
    public override void Handle(PrintContext context)
    {
        if (context.Qty <= 0)
        {
            throw new Exception("数量错误");
        }

        Console.WriteLine("数量检查通过");

        base.Handle(context);
    }
}

