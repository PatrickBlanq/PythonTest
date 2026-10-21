IPrinter printer =
    new LogoDecorator(
        new QRDecorator(
            new NormalPrinter()));

printer.Print();

var p1 = new NormalPrinter();

var p2 = new QRDecorator(p1);

var p3 = new LogoDecorator(p2);

IPrinter printer = p3;

public interface IPrinter
{
    void Print();
}

public class NormalPrinter : IPrinter
{
    public void Print()
    {
        Console.WriteLine("打印箱码");
    }
}

public abstract class PrinterDecorator : IPrinter
{
    protected IPrinter _printer;

    public PrinterDecorator(IPrinter printer)
    {
        _printer = printer;
    }

    public virtual void Print()
    {
        _printer.Print();
    }
}

public class QRDecorator : PrinterDecorator
{
    public QRDecorator(IPrinter printer) : base(printer)
    {
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine("打印二维码");
    }
}

public class LogoDecorator : PrinterDecorator
{
    public LogoDecorator(IPrinter printer) : base(printer)
    {
    }

    public override void Print()
    {
        base.Print();
        Console.WriteLine("打印Logo");
    }
}



