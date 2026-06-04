using System;
using System.Windows;

namespace BlankApp2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
         var s=   CalculateMatrixDimensions(3, 3, 10);
        }
        public static (int Rows, int Columns) CalculateMatrixDimensions(int MaxRow, int MaxColumn, int Num)
        {
            // 1. 优先查找完美因子解 (R*C = Num)
            for (int i = 1; i * i <= Num; i++)
            {
                if (Num % i == 0)
                {
                    // 检查两种因子组合
                    int R1 = i, C1 = Num / i;
                    int R2 = Num / i, C2 = i;

                    if (R1 <= MaxRow && C1 <= MaxColumn)
                        return (R1, C1);  // 返回首组有效解[1,5](@ref)

                    if (R2 <= MaxRow && C2 <= MaxColumn)
                        return (R2, C2);
                }
            }

            // 2. 列优先策略：固定最大列数
            int C = Math.Min(MaxColumn, Num);
            int R_min = (Num + C - 1) / C;  // 向上取整计算行数
            if (R_min <= MaxRow)
                return (R_min, C);  // 返回列优先解[2,7](@ref)

            // 3. 行优先策略：固定最大行数
            int R = Math.Min(MaxRow, Num);
            int C_min = (Num + R - 1) / R;  // 向上取整计算列数
            if (C_min <= MaxColumn)
                return (R, C_min);  // 返回行优先解[5,8](@ref)

            // 4. 保底解（理论必存在）
            return (MaxRow, MaxColumn);  // 返回最大可能矩阵[6,9](@ref)
        }
        public void Dispose()
        {
            
        }
    }
}
