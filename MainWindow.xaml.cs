using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using MathNet.Numerics;

namespace ChM3
{
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Func();

        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }


        class MyTable
        {
            public MyTable(double x, double y1, double y2)
            {
                this.X = x.ToString("F3");
                this.Ay = y1.ToString("F10");
                this.Ym= y2.ToString("F10");
            }
            public string X { get; set; }
            public string Ay { get; set; }
            public string Ym { get; set; }
        }

        private void Func()
        {
            DataContext = null;
            int N = Convert.ToInt32(Ntext.Text);
            double ax = -1, bx = 1;
            double h = (bx - ax) / (N);
            double[] x = new double[N + 1];
            List<double> y = new List<double>();
            double[,] ACB = new double[N, N];
            double[] F = new double[N];
            double a = Convert.ToDouble(Atext.Text);
            double YAnalytic(double x) => (Trig.Cos(Math.PI*x) + x + ((x * SpecialFunctions.Erf(x / Math.Sqrt(2 * a)) + Math.Sqrt((2 * a) / Math.PI) * Math.Exp(-(x * x) / (2 * a))) / (SpecialFunctions.Erf(1 / Math.Sqrt(2 * a)) + Math.Sqrt(2 * a / Math.PI) * Math.Exp(-1 / (2 * a)))));


            for (int i = 0; i <= N; i++)
            {
                x[i] = ax + i * h;
            }

            for (int i = 0; i <= N; i++)
            {
                y.Add(YAnalytic(x[i]));
            }

            ACB[0, 0] = -a + ((x[0] * h) / 2);
            ACB[0, 1] = 2 * a - ((-1) * h * h);
            F[0] = -(1 + a * Math.Pow(Math.PI, 2)) * Math.Cos(Math.PI*x[0]) - Math.PI * x[0] * Math.Sin(Math.PI * x[0]) + (((-a + ((x[0] * h) / 2)) * (-1)) / (h * h));

            for (int i = 1; i < N - 1; i++)
            {
                ACB[i, i - 1] = -a + ((x[i] * h) / 2);
                ACB[i, i] = 2 * a - ((-1) * h * h);
                ACB[i, i + 1] = -a - ((x[i] * h) / 2);
                F[i] = -(1 + a * Math.Pow(Math.PI, 2)) * Math.Cos(Math.PI*x[i]) - Math.PI * x[i] * Math.Sin(Math.PI * x[i]);
            }

            ACB[N - 1, N - 2] = 2 * a - ((-1) * h * h);
            ACB[N - 1, N - 1] = -a - ((x[N-1] * h) / 2);
            F[N - 1] = -(1 + a * Math.Pow(Math.PI, 2)) * Math.Cos(Math.PI * x[N-1]) - Math.PI * x[N-1] * Math.Sin(Math.PI * x[N-1]) + (((-a - ((x[N-1] * h) / 2)) * (1)) / (h * h));


            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Console.Write(ACB[i, j].ToString("f3") + "  ");
                }
                Console.WriteLine(" = " + F[i]);
            }

            var ACBmatrix = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseOfArray(ACB);
            var Fvector = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(F);
            Fvector = Fvector * -Math.Pow(h, 2);
            var Yvector = ACBmatrix.Solve(Fvector);


            List<double> yres = new List<double>();

            for (int i = 0; i < N; i++)
            {
                yres.Add(Yvector[i]);
            }
            yres.Insert(0, -1);
            yres[N] = 1;

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Analytic Y",
                    Values = new ChartValues<double>(y.ToList<double>())
                },
                new LineSeries
                {
                    Title = "Y method",
                    Values = new ChartValues<double>(yres)
                }
            };
            Labels = x.Select(x => x.ToString("F2")).ToArray();
            YFormatter = value => value.ToString("F2");
            DataContext = this;
            List<MyTable> result = new List<MyTable>();
            result.Add(new MyTable(x[0], y[0], -1));
            for (int i = 1; i < N; i++)
            {
                result.Add(new MyTable(x[i], y[i], yres[i]));
            }
            result.Add(new MyTable(x[N], y[N], 1));

            grid.ItemsSource = result;
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            Func();
        }

    }
}