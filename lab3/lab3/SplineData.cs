using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


class SplineData
{
    public V2DataArray? V2DataLink = null;
    public int m;                  // Число узлов равномерной сетки == m (для построения)
    public double[]? SplineValues = null; //Значения на сетке (для вычисления)
    public int MaxItersNum;
    public int StopReason;
    public double ResidualMin;
    public List<SplineDataItem>? ApproximationRes { get; set; }
    public int ItersNum;
    public SplineData(V2DataArray V2A, int NodesNum, int MaxItersNum)
    {
        this.V2DataLink = V2A;
        this.m = NodesNum;
        this.MaxItersNum = MaxItersNum;
        this.SplineValues = new double[V2A.Net.Length];
        ApproximationRes = new List<SplineDataItem>();
    }

    public void SplineMklCall(Func<double, double> F_Init) 
    {
        
        // F_Init - функция, чтобы получить начальное приближение на равномерной сетке
        
        int nS = V2DataLink.Net.Length;                             //Число неравномерной узлов сетки
        double[] grid = this.V2DataLink.Net;                        //Неравномерная сетка

        double xL = this.V2DataLink.Net[0];
        double xR = this.V2DataLink.Net[V2DataLink.Net.Length - 1];
        double[] X = { xL, xR };                                    //Концы отрезка для равномерной сетки
        double[] Uniform_grid = new double[m];                      //Равномерная сетка
        double hS = (xR - xL) / (m - 1);                            // шаг сетки
        Uniform_grid[0] = xL;
        for (int j = 0; j < m; ++j)
        {
            Uniform_grid[j] = Uniform_grid[0] + hS * j;
        }
        double[] ApprStartVals = new double[m];
        for (int i = 0; i < m; ++i)
        {
            ApprStartVals[i] = F_Init(Uniform_grid[i]); //Начальное приближение
        }

        int nY = 1;                                     //Размерность векторной функции
        double[] y_true = new double[nS];
        for (int i = 0; i < nS; ++i)
        {
            y_true[i] = this.V2DataLink.Field_values[0, i]; //Истинные значения на неравномерной сетке
        }
       
        SplineValues = new double[nS];
        try
        {
            OptimSplineInterpolation(
                m,
                X,
                nY,
                nS,
                grid,
                y_true,
                ApprStartVals,
                SplineValues,
                ref StopReason,
                ref ResidualMin,
                MaxItersNum,
                ref ItersNum);
            ResidualMin = 0;
            for(int i = 0; i < nS; ++i)
            {
                ResidualMin += (y_true[i] - SplineValues[i]) * (y_true[i] - SplineValues[i]);
                ApproximationRes.Add(new SplineDataItem(grid[i], y_true[i], SplineValues[i]));
            }
            ResidualMin = Math.Sqrt(ResidualMin);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сплайн-интерполяции\n{ex}");
        }
    }

    [DllImport("Dll_for_lab3.dll",
    CallingConvention = CallingConvention.Cdecl)]
    public static extern
    void OptimSplineInterpolation(
    int m,                   // число узлов сплайна на равномерной сетке == m (для построения)
    double[] X,              // массив узлов сплайна на равномерной сетке (для построения)
    int nY,                  // размерность векторной функции (для построения)
    int nS,                  // число узлов сетки, на которой вычисляются значения сплайна (для расчета)
    double[] grid,           // Сетка, на которой происходит вычисление значений сплайна (для расчета)
    double[] true_y,         // Истинные значения функции на сетке grid 
    double[] ApprStartVals,  // Начальное приближение
    double[] SplineValues,   // Значения сплайна на сетке grid (искомое)
    ref int StpReason,       // Причина остановки
    ref double MinResVal,    // Минимальное значение невязки
    int MaxIters,            // Максимальное число итераций
    ref int Iters);          // Сделаное число итераций


    public string ToLongString(string format)
    {
        string ans = "";
        ans += V2DataLink.ToLongString(format);
        for(int i = 0; i < ApproximationRes.Count(); ++i)
        {
            ans += ApproximationRes[i].ToString(format);
        }
        ans += $"ResidualMin = {ResidualMin.ToString()}\n";
        ans += $"StopReason = {StopReason.ToString()}\n";
        ans += $"ItersNum = {ItersNum}\n";

        return ans;
    }

    public void Save(string filename, string format)
    {
        try
        {
            StreamWriter writer = new StreamWriter(filename, false);
            writer.Write(ToLongString(format));
            writer.Close(); 
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception!");
            Console.WriteLine(ex.Message);
        }
    }


}

