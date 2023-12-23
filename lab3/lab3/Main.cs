using System.IO.Compression;
using System.Runtime.InteropServices.Marshalling;
using System.Xml.Linq;
using System.Runtime.InteropServices;

class Program
{
    public static DataItem Func1_FDI(double x)
    {
        return new DataItem(x, x - 2, x + 2);
    }

    public static void Func1_FV(double x, ref double y1, ref double y2)
    {
        y1 = x * x * x;
        y2 = x * x * x;
    }

    public static double f1(double x) => x * x + 3 * x + 1; //Функция для начальной аппроксимации
    public static void test1()
    {
        FValues F = Func1_FV;
        double[] x = { 0, 0.1, 0.14, 0.2234, 0.311, 0.4999, 0.55, 0.6, 0.8, 1}; //Неравномерная сетка
        
        V2DataArray V2A = new V2DataArray("Quant", new DateTime(2023, 1, 1), x, F);
 
        SplineData SP = new SplineData(V2A, 10, 1000);
        SP.SplineMklCall(f1);
        Console.WriteLine(SP.ToLongString("f7"));
        SP.Save("Result_test1.txt", "f7");
    }

    public static void Func2_FV(double x, ref double y1, ref double y2)
    {
        y1 = x * x;
        y2 = x * x * x;
    }

    public static double f2(double x) => x + 1; //Начальное приближение
    public static void test2()
    {
        FValues F = Func2_FV;
        double[] x = { 0, 0.0123, 0.144, 0.2234, 0.576, 0.689, 0.7, 0.8, 1}; //Неравномерная сетка

        V2DataArray V2A = new V2DataArray("Quant", new DateTime(2023, 1, 1), x, F);

        SplineData SP = new SplineData(V2A, 9, 1000);
        SP.SplineMklCall(f2);
        Console.WriteLine(SP.ToLongString("f7"));
        SP.Save("Result_test2.txt", "f7");
    }

    static void Main()
    {
        FValues F1 = Func1_FV;
        FDI F2 = Func1_FDI;
        test1(); // Построение сплайна для функции x^3
        test2(); // Построение сплайна для функции x^2
    }
}


