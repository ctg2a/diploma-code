using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using MathNet.Numerics.Distributions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics;
using System.IO;
using System.IdentityModel.Claims;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1.Pkcs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace диплом_форма__framework_
{
    public partial class Form1 : Form
    {
        //инициализация параметров метода муравьиных колоний
        public List<List<double>> PF = new List<List<double>>();
        public List<List<double>> population = new List<List<double>>();
        public List<List<double>> I = new List<List<double>>();
        public List<double> omega = new List<double>();
        public List<double> p = new List<double>();
        public int M;
        public int R;
        public int K;
        public double ksi;
        public double q;
        public int xx;
        public string selectedFilePath = "0";
        static double eps = 1;

        public Form1()
        {
            //настройка размеров окна программы
            InitializeComponent();
            this.Width = 1472;
            this.Height = 1015;
            textBox1.Text = ("200");
            textBox2.Text = ("400");
            textBox3.Text = ("200");
            textBox4.Text = ("0,85");
            textBox5.Text = ("0,1");
            textBox6.Text = ("10");
            textBox8.Text = ("200");
            textBox9.Text = ("400");
            textBox10.Text = ("200");
            textBox11.Text = ("0,85");
            textBox12.Text = ("0,1");
            textBox13.Text = ("2");
            textBox20.Text = ("600");
            textBox19.Text = ("800");
            textBox18.Text = ("1500");
            textBox17.Text = ("0,85");
            textBox16.Text = ("0,1");
            textBox15.Text = ("5");
            this.chart3.ChartAreas[0].AxisX.LabelStyle.Format = "0.0000";

        }
        static Random random = new Random();
        //объявление целевых функций задач ZDT1, ZDT2, ZDT3

        static double g(List<double> I)
        {
            int xx = I.Count;
            double sum = 0;

            for (int i = 1; i < xx; i++)
            {
                sum += I[i] / (xx - 1);
            }

            return 1 + 9 * sum;
        }

        static double f1(List<double> I, int i)
        {
            double f = 0;
            if (i <= 3)
            {
                f = I[0];

            }
            if (i == 5)
            {
                f = I[0];
            }
            if (i == 4)
            {
                f = 4 * I[0] * I[0] + 4 * I[1] * I[1];
            }
            if (i == 6)

            {
                string filePath = "C:\\Users\\User\\Downloads\\res3.txt";
                List<double> avgReturns = new List<double>();
                bool isAvgReturns = true;
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.Contains("средняя доходность"))
                    {
                        isAvgReturns = true;
                        continue; 
                    }
                    else if (line.Contains("ковариационная матрица"))
                    {
                        isAvgReturns = false;
                        break; 
                    }
                    string[] values = line.Split(' ');
                    if (isAvgReturns)
                    {
                        avgReturns.AddRange(values.Select(str => double.Parse(str.Replace('.', ','))));
                    }

                    for (int j = 0; j < I.Count - 1; j++)
                    {
                        f -= avgReturns[j] * I[j];
                    }
                }
            }
            return f;
        }

        static double f2(List<double> I, int i)
        {
            double f = 0;
            if (i == 1)
            {
                f = g(I) * (1 - Math.Sqrt(I[0] / g(I)));
            }
            if (i == 2)
            {
                f = g(I) * (1 - Math.Pow(I[0] / g(I), 2));
            }
            if (i == 3)
            {
                f = g(I) * (1 - Math.Sqrt(I[0] / g(I)) - (I[0] / g(I)) * Math.Sin(10 * Math.PI * I[0]));

            }
            if (i == 4)
            {
                f = Math.Pow(I[0] - 5, 2) + Math.Pow(I[1] - 5, 2);
            }
            if (i == 5)
            {
                f = I[1];
            }
            if (i == 6)
            {
                string filePath = "C:\\Users\\User\\Downloads\\res3.txt";
                List<List<double>> covariance = new List<List<double>>();
                bool isAvgReturns = true;
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.Contains("средняя доходность"))
                    {
                        isAvgReturns = false;
                        continue; 
                    }
                    else if (line.Contains("ковариационная матрица"))
                    {
                        isAvgReturns = true;
                        continue; 
                    }
                    string[] values = line.Split(' ');
                    if (isAvgReturns)
                    {
                        covariance.Add(values.Select(double.Parse).ToList());
                    }
                }
                for (int k = 0; k < I.Count - 1; k++)
                {
                    for (int j = 0; j < I.Count - 1; j++)
                    {
                        f += I[k] * covariance[k][j] * I[k];
                    }
                }
                f = Math.Sqrt(f);
            }

            return f;
        }
        //функция, считающая степень ошибки каждого решения
        static void gg(ref List<List<double>> I, int j, int xx)
        {
            foreach (var row in I)
            {
                if (row.Count > xx) 
                {
                    row.RemoveAt(row.Count - 1); 
                }
            }

            if (j == 4)
            {
                List<double> Ig1 = new List<double>();
                List<double> Ig2 = new List<double>();
                for (int i = 0; i < I.Count(); i++)
                {
                    Ig1.Add(Math.Max(0, (Math.Pow(I[i][0] - 5, 2) + I[i][1] * I[i][1] - 25)));
                    Ig2.Add(Math.Max(0, (7.7 - Math.Pow(I[i][0] - 8, 2) - Math.Pow(I[i][1] + 3, 2))));
                }
                double maxIg1 = Ig1.Max();
                double maxIg2 = Ig2.Max();

                for (int i = 0; i < I.Count(); i++)
                {
                    if (maxIg1 == 0 & maxIg2 == 0)
                    {
                        I[i].Add(0);
                    }
                    else if (maxIg1 == 0 & maxIg2 != 0)
                    {
                        I[i].Add(Ig2[i] / maxIg2);
                    }
                    else if (maxIg1 != 0 & maxIg2 == 0)
                    {
                        I[i].Add(Ig1[i] / maxIg1);
                    }
                    else
                    {
                        I[i].Add(Ig1[i] / maxIg1 + Ig2[i] / maxIg2);
                    }

                }
            }
            if (j == 5)
            {
                List<double> Ig1 = new List<double>();
                List<double> Ig2 = new List<double>();
                for (int i = 0; i < I.Count(); i++)
                {
                    Ig1.Add(Math.Max(0, -I[i][0] * I[i][0] - I[i][1] * I[i][1] + 1 + 0.1 * Math.Cos(16 * Math.Atan(I[i][0] / I[i][1]))));
                    Ig2.Add(Math.Max(0, (I[i][0] - 0.5) * (I[i][0] - 0.5) + (I[i][1] - 0.5) * (I[i][1] - 0.5) - 0.5));
                }
                double maxIg1 = Ig1.Max();
                double maxIg2 = Ig2.Max();

                for (int i = 0; i < I.Count(); i++)
                {
                    if (maxIg1 == 0 & maxIg2 == 0)
                    {
                        I[i].Add(0);
                    }
                    else if (maxIg1 == 0 & maxIg2 != 0)
                    {
                        I[i].Add(Ig2[i] / maxIg2);
                    }
                    else if (maxIg1 != 0 & maxIg2 == 0)
                    {
                        I[i].Add(Ig1[i] / maxIg1);
                    }
                    else
                    {
                        I[i].Add(Ig1[i] / maxIg1 + Ig2[i] / maxIg2);
                    }
                }
            }
            if (j == 6)
            {
                double val = -1;
                List<double> Ig1 = new List<double>();
                for (int i = 0; i < I.Count(); i++)
                {
                    for (int z = 0; z < xx; z++)
                    {

                        val += I[i][z];
                    }
                    Ig1.Add(val);
                    val = -1;
                }
                double maxIg1 = Ig1.Max();
                for (int i = 0; i < I.Count(); i++)
                {
                    I[i].Add(Math.Abs(Ig1[i]));
                }
            }
        }
        //вспомогательная функция, разделяющая решения, у которых степень ошибки = 0 и у которых !=0
        static void dis(ref List<List<double>> population, ref List<List<double>> I, int flag, int xx)
        {
            List<List<double>> In = new List<List<double>>();
            List<List<double>> populationn = new List<List<double>>();
            List<List<double>> Iz = new List<List<double>>();
            List<List<double>> populationz = new List<List<double>>();

            gg(ref I, flag, xx);

            for (int i = 0; i < I.Count(); i++)
            {
                if (I[i][xx] > eps)
                {
                    In.Add(new List<double>(I[i]));
                    populationn.Add(new List<double>(population[i]));
                }
                else
                {
                    Iz.Add(new List<double>(I[i]));
                    populationz.Add(new List<double>(population[i]));
                }
            }
            if (populationn.Count() == 0)
            {
                eps = eps / 2;
                Console.WriteLine("eps=" + eps);
            }

            I.Clear();
            I.AddRange(Iz);
            I.AddRange(In);
            population.Clear();
            population.AddRange(populationz);
            population.AddRange(populationn);
        }
        //функция, реализующая метод муравьиных колоний для многокритериальной оптимизации
        static void ants(int M, int R, int xx, double ksi, List<double> a, List<double> b, int flag, List<double> p, ref List<List<double>> I, ref List<List<double>> population)
        {
            if (flag > 3)
            {
                foreach (var row in I)
                {
                    if (row.Count > xx) 
                    {
                        row.RemoveAt(row.Count - 1); 
                    }
                }
            }

            List<double> sum_sig = new List<double>();
            List<double> mo = new List<double>();
            List<double> sig = new List<double>();


            for (int m = 0; m < M; m++)
            {
                int jj = roulette_wheel(p);
                for (int i = 0; i < xx; i++)
                {
                    mo.Add(I[jj][i]);
                    sum_sig.Add(0);
                }
                for (int j = 0; j < xx; j++)
                {
                    for (int i = 0; i < R; i++)
                    {
                        sum_sig[j] = sum_sig[j] + Math.Abs(I[i][j] - I[jj][j]);
                    }
                }
                for (int i = 0; i < xx; i++)
                {
                    sig.Add(ksi * sum_sig[i] / (R - 1));
                }

                I.Add(new List<double>());
                for (int i = 0; i < xx; i++)
                {
                    I[R + m].Add(NextGaussian(mo[i], sig[i], a[i], b[i]));
                }
                population.Add(new List<double> { f1(I[R + m], flag), f2(I[R + m], flag) });

            }

            if (flag <= 3)
            {
                non_dom_sort(ref population, ref I, false);
            }
            if (flag >= 4)
            {
                dis(ref population, ref I, flag, xx);
            }

            int k = 0;

            I.RemoveRange(R, M);
            population.RemoveRange(R, M);
            if (flag == 6)
            {
                if (eps > 0.000152587890625) {
                    for (int i = 0; i < I.Count(); i++)
                    {

                        if (I[i][xx] > eps)
                        {
                            k = 1;
                        }
                    }
                    if (k == 0)
                    {
                        eps = eps / 2;
                        Console.WriteLine("eps=" + eps);
                    }
                }
            }

        }
        //вспомогательная функция для недоминируемой сортировки 
        static bool Dominates(List<double> individual_1, List<double> individual_2)
        {
            for (int i = 0; i < individual_1.Count; i++)
            {
                if (individual_1[i] > individual_2[i])
                {
                    return false;
                }
            }
            return individual_1.Zip(individual_2, (val_1, val_2) => val_1 < val_2).Any(result => result);
        }

        static double NextGaussian(double mean, double stdDev, double a, double b)
        {
            double res = b + 1;
            while (res > b || res < a)
            {
                res = Normal.Sample(random, mean, stdDev);
            }
            return res;
        }
        //функция, реализующая недоминируемую сортировку
        static void non_dom_sort(ref List<List<double>> population, ref List<List<double>> I, bool flag)
        {
            List<List<double>> population1 = new List<List<double>>();
            List<List<double>> I1 = new List<List<double>>();

            int c = 0;
            population1 = new List<List<double>>();
            I1 = new List<List<double>>();

            while (population.Count > 0)
            {
                for (int i = 0; i < population.Count; i++)
                {
                    for (int j = 0; j < population.Count; j++)
                    {
                        if (i != j)
                        {
                            if (Dominates(population[j], population[i]))
                            {
                                c = c + 1;
                            }

                        }
                    }

                    if (c == 0)
                    {
                        population1.Add(new List<double>(population[i]));
                        I1.Add(new List<double>(I[i]));
                    }
                    else
                    {
                        c = 0;
                    }
                }

                foreach (var item in population1)
                {
                    population.RemoveAll(x => Enumerable.SequenceEqual(x, item));
                }

                foreach (var item in I1)
                {
                    I.RemoveAll(x => Enumerable.SequenceEqual(x, item));
                }
                if (flag)
                {
                    population.Clear();
                    I.Clear();
                    break;
                }

            }
            population.AddRange(population1);
            I.AddRange(I1);
        }
        static int roulette_wheel(List<double> p)
        {
            int sol = 0;
            int k = p.Count();
            List<double> q = new List<double>(k);
            q.Add(p[0]);
            double r = random.NextDouble(); 

            for (int i = 1; i < k; i++)
            {
                if (r > q[i - 1])
                {
                    sol = i;
                }
                q.Add(q[i - 1] + p[i]);

            }

            return sol;

        }

        //обработчик события нажатия на первую кнопку в первом окне (Примеры задач с ограничениями-интервалами) для решения задачи
        private void button1_Click(object sender, EventArgs e)
        {
            population.Clear();
            I.Clear();
            M = Convert.ToInt32(textBox1.Text);
            R = Convert.ToInt32(textBox2.Text);
            K = Convert.ToInt32(textBox3.Text);
            ksi = double.Parse(textBox4.Text);
            q = double.Parse(textBox5.Text);
            xx = Convert.ToInt32(textBox6.Text);
            int jj;
            List<double> a = new List<double>();
            List<double> b = new List<double>();
            for (int i = 0; i < xx; i++)
            {
                a.Add(0);
                b.Add(1);
            }
            int flag = 0;
            if (comboBox1.SelectedItem.ToString() == "ZDT1")
            {
                flag = 1;
            }
            if (comboBox1.SelectedItem.ToString() == "ZDT2")
            {
                flag = 2;
            }
            if (comboBox1.SelectedItem.ToString() == "ZDT3")
            {
                flag = 3;
            }

            population.Clear();
            I.Clear();
            omega.Clear();
            p.Clear();


            for (int i = 0; i < R; i++)
            {
                omega.Add(1 / (q * R * Math.Sqrt(2 * Math.PI)) * Math.Exp(-i * i / (2 * q * q * R * R)));
            }
            double s = omega.Sum();
            for (int i = 0; i < R; i++)
            {
                p.Add(omega[i] / s);
            }

            for (int i = 0; i < R; i++)
            {
                List<double> I1 = new List<double>();
                for (int j = 0; j < xx; j++)
                {
                    I1.Add(random.NextDouble());
                }
                I.Add(I1);
            }



            for (int i = 0; i < R; i++)
            {

                population.Add(new List<double> { f1(I[i], flag ), f2(I[i],flag) });
            }


            non_dom_sort(ref population, ref I, false);
            

            for (int k = 0; k < K; k++)
            {
                Console.WriteLine(k);
                ants(M, R, xx, ksi, a, b, flag, p, ref I, ref population);

            }
            non_dom_sort(ref population, ref I, true);

           
            for (int i = 0; i < population.Count; i++)
            {
                this.chart1.Series[0].Points.AddXY(population[i][0], population[i][1]);
            }

            foreach (var innerList in population)
            {
                foreach (var item in innerList)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
        }
        //обработчик события нажатия на вторую кнопку в первом окне (Примеры задач с ограничениями-интервалами) для удаления графика
        private void button2_Click(object sender, EventArgs e)
        {
            this.chart1.Series[0].Points.Clear();
        }
        //обработчик события нажатия на третью кнопку в первом окне (Примеры задач с ограничениями-интервалами) для закрытия окна
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();

        }
        //обработчик события по выбору нужной задачи из списка в первом окне (Примеры задач с ограничениями-интервалами)
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PF.Clear();
            this.chart1.Series[1].Points.Clear();
            double x = 0;
            double y;
            if (comboBox1.SelectedItem.ToString() == "ZDT1")
            {
                pictureBox1.Image = System.Drawing.Image.FromFile("C:\\Users\\User\\source\\repos\\диплом форма (framework)\\диплом форма (framework)\\sn3.png");
                while (x <= 1)
                {
                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }
            }
            if (comboBox1.SelectedItem.ToString() == "ZDT2")
            {
                pictureBox1.Image = System.Drawing.Image.FromFile("C:\\Users\\User\\source\\repos\\диплом форма (framework)\\диплом форма (framework)\\sn2.png");
                while (x <= 1)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - x * x;
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);

                }
            }
            if (comboBox1.SelectedItem.ToString() == "ZDT3")
            {
                
                pictureBox1.Image = System.Drawing.Image.FromFile("C:\\Users\\User\\source\\repos\\диплом форма (framework)\\диплом форма (framework)\\sn1.png");
                while (x <= 0.08300153492746176)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x) - x * Math.Sin(10 * Math.PI * x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }

                this.chart1.Series[1].Points.AddXY(double.NaN, double.NaN);
                x = 0.18222872803016799;
                while (x <= 0.2577623633886105)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x) - x * Math.Sin(10 * Math.PI * x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }
                this.chart1.Series[1].Points.AddXY(double.NaN, double.NaN);
                x = 0.4093136748080724;
                while (x <= 0.45388210409000745)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x) - x * Math.Sin(10 * Math.PI * x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }
                this.chart1.Series[1].Points.AddXY(double.NaN, double.NaN);
                x = 0.6183967944394682;
                while (x <= 0.6525117038036113)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x) - x * Math.Sin(10 * Math.PI * x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }
                this.chart1.Series[1].Points.AddXY(double.NaN, double.NaN);
                x = 0.8233317983263985;
                while (x <= 0.851832865436154)
                {

                    List<double> PFl = new List<double>();
                    PFl.Add(x);
                    y = 1 - Math.Sqrt(x) - x * Math.Sin(10 * Math.PI * x);
                    PFl.Add(y);
                    this.chart1.Series[1].Points.AddXY(x, y);
                    x += 0.002;
                    PF.Add(PFl);
                }
            }
        }
        //обработчик события по выбору нужной задачи из списка во втором окне (Примеры задач с ограничениями типа неравенств)
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            PF.Clear();
            this.chart2.Series[1].Points.Clear();
            this.chart2.ChartAreas[0].AxisX.LabelStyle.Format = "0";
            double y1;
            double y2;

            if (comboBox2.SelectedItem.ToString() == "BNH")
            {
                double x1 = 0;
                double x2 = 3;
                while (x1 < 3)
                {
                    List<double> PFl = new List<double>();
                    y1 = 4 * x1 * x1 + 4 * x1 * x1;
                    y2 = 2 * Math.Pow(x1 - 5, 2);
                    PFl.Add(y1);
                    PFl.Add(y2);
                    this.chart2.Series[1].Points.AddXY(y1, y2);
                    x1 += 0.05;
                    PF.Add(PFl);
                }
                while (x1 < 5)
                {
                    List<double> PFl = new List<double>();
                    y1 = 4 * x1 * x1 + 4 * x2 * x2;
                    y2 = Math.Pow(x1 - 5, 2) + Math.Pow(x2 - 5, 2);
                    PFl.Add(y1);
                    PFl.Add(y2);
                    this.chart2.Series[1].Points.AddXY(y1, y2);
                    x1 += 0.05;
                    PF.Add(PFl);
                }
                
                foreach (var innerList in PF)
                {
                    foreach (var item in innerList)
                    {
                        Console.Write(item + " ");
                    }
                    Console.WriteLine();
                }
            }
            if (comboBox2.SelectedItem.ToString() == "TNK")
            {
                string[] lines = File.ReadAllLines("C:\\Users\\User\\Downloads\\TNK.txt");

                foreach (string line in lines)
                {
                    string[] parts = line.Split(' '); 
                    double x = double.Parse(parts[0]);
                    double y = double.Parse(parts[1]);
                    this.chart2.Series[1].Points.AddXY(x, y);
                    PF.Add(new List<double>{ x,y});
                }
            }
        }
        //обработчик события нажатия на первую кнопку во втором окне (Примеры задач с ограничениями типа неравенств) для решения задачи
        private void button4_Click(object sender, EventArgs e)
        {
            M = Convert.ToInt32(textBox8.Text);
            R = Convert.ToInt32(textBox9.Text);
            K = Convert.ToInt32(textBox10.Text);
            ksi = double.Parse(textBox11.Text);
            q = double.Parse(textBox12.Text);
            xx = 2;
            int jj;
            List<double> a = new List<double>();
            List<double> b = new List<double>();
            int flag = 0;
            if (comboBox2.SelectedItem.ToString() == "BNH")
            {
             
                flag = 4;
                a.Add(0);
                a.Add(0);
                b.Add(3);
                b.Add(5);
            }
            if (comboBox2.SelectedItem.ToString() == "TNK")
            {
                flag = 5;
                a.Add(0);
                a.Add(0);
                b.Add(Math.PI);
                b.Add(Math.PI);
            }

            population.Clear();
            I.Clear();
            omega.Clear();
            p.Clear();

            for (int i = 0; i < R; i++)
            {
                omega.Add(1 / (q * R * Math.Sqrt(2 * Math.PI)) * Math.Exp(-i * i / (2 * q * q * R * R)));
            }
            double s = omega.Sum();
            for (int i = 0; i < R; i++)
            {
                p.Add(omega[i] / s);
            }

            for (int i = 0; i < R; i++)
            {
                I.Add(new List<double> { random.NextDouble() * Math.PI, random.NextDouble() * Math.PI });

            }
            List<List<double>> In = new List<List<double>>();
            List<List<double>> populationn = new List<List<double>>();
            List<List<double>> Iz = new List<List<double>>();
            List<List<double>> populationz = new List<List<double>>();

            for (int i = 0; i < I.Count(); i++)
            {
                population.Add(new List<double> { f1(I[i], flag), f2(I[i], flag) });
            }
            for (int k = 0; k < K; k++)
            {
                Console.WriteLine(k);
                ants(M, R, xx, ksi, a, b, flag, p, ref I, ref population);

            }

            gg(ref I, flag, xx);

            for (int i = 0; i < I.Count(); i++)
            { 

                if (I[i][xx] == 0)
                {
                    Iz.Add(new List<double>(I[i]));
                    populationz.Add(new List<double>(population[i]));
                }

            }

            non_dom_sort(ref populationz, ref Iz, true);

            I.Clear();
            I.AddRange(Iz);
            population.Clear();
            population.AddRange(populationz);


            for (int i = 0; i < population.Count; i++)
            {
                this.chart2.Series[0].Points.AddXY(population[i][0], population[i][1]);
            }
            foreach (var innerList in population)
            {
                foreach (var item in innerList)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
            this.chart2.SaveImage(@"C:\Users\User\Downloads\\test.jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        //обработчик события нажатия на вторую кнопку во втором окне (Примеры задач с ограничениями типа неравенств) для удаления графика
        private void button5_Click(object sender, EventArgs e)
        {
            this.chart2.Series[0].Points.Clear();
        }
        //обработчик события нажатия на третью кнопку во втором окне (Примеры задач с ограничениями типа неравенств) для закрытия окна
        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //обработчик события нажатия на первую кнопку в третьем окне (Задача оптимизации инвестиционного портфеля) для решения задачи
        private void button9_Click(object sender, EventArgs e)
        {
            eps = 5;
            M = Convert.ToInt32(textBox20.Text);
            R = Convert.ToInt32(textBox19.Text);
            K = Convert.ToInt32(textBox18.Text);
            ksi = double.Parse(textBox17.Text);
            q = double.Parse(textBox16.Text);
            xx = 10;
            int jj;
            List<double> a = new List<double> ();
            List<double> b = new List<double> ();
            for (int i = 0; i < xx; i++)
            {
                a.Add(0);
                b.Add(1);
            }
            int flag = 6;

            population.Clear();
            I.Clear();
            omega.Clear();
            p.Clear();


            for (int i = 0; i < R; i++)
            {
                omega.Add(1 / (q * R * Math.Sqrt(2 * Math.PI)) * Math.Exp(-i * i / (2 * q * q * R * R)));
            }
            double s = omega.Sum();
            for (int i = 0; i < R; i++)
            {
                p.Add(omega[i] / s);
            }

            for (int i = 0; i < R; i++)
            {
                List<double> row = new List<double>();
                for (int j = 0; j < xx; j++)
                {
                    row.Add(random.NextDouble()); 
                }
                I.Add(row);
            }
            List<List<double>> In = new List<List<double>>();
            List<List<double>> populationn = new List<List<double>>();
            List<List<double>> Iz = new List<List<double>>();
            List<List<double>> populationz = new List<List<double>>();

            for (int i = 0; i < I.Count(); i++)
            {
                population.Add(new List<double> { f1(I[i], flag), f2(I[i], flag) });
            }
            for (int k = 0; k < K; k++)
            {
                Console.WriteLine(k);
                ants(M, R, xx, ksi, a, b, flag, p, ref I, ref population);

            }

            foreach (var row in I)
            {
                if (row.Count > xx) 
                {
                    row.RemoveAt(row.Count - 1); 
                }
            }

            double val = -1;
            for (int i = 0; i < I.Count(); i++)
            {
                for (int z = 0; z < xx; z++)
                {

                    val += I[i][z];
                }
                I[i].Add(Math.Abs(val));
                Console.WriteLine(Math.Abs(val));
                val = -1;
            }

            Console.WriteLine("eps =" + eps);

            for (int i = 0; i < I.Count(); i++)
            {

                if (I[i][xx] < eps)
                {
                    Iz.Add(new List<double>(I[i]));
                    populationz.Add(new List<double>(population[i]));
                }

            }

            non_dom_sort(ref populationz, ref Iz, true);

            I.Clear();
            I.AddRange(Iz);
            population.Clear();
            population.AddRange(populationz);


            foreach (var row in I)
            {
                if (row.Count > xx) 
                {
                    row.RemoveAt(row.Count - 1); 
                }
            }
            Console.WriteLine("Я готов");
            foreach (var innerList in I)
            {
                foreach (var item in innerList)
                {
                    Console.Write(item + " ");
                }
                Console.WriteLine();
            }
            for (int i = 0; i< population.Count(); i++)
            {
               if(population[i][0] < 0)
                {
                    population[i][0] = population[i][0] * (-1);
                }
            }


            for (int i = 0; i < population.Count; i++)
            {
                this.chart3.Series[0].Points.AddXY(population[i][0], population[i][1]);
            }


            for (int i = 0; i < I.Count; i++)
            {
                for (int j = 0; j < I[i].Count; j++)
                {
                    I[i][j] = Math.Round(I[i][j], 2);
                }
            }
            for (int i = 0; i < population.Count; i++)
            {
                for (int j = 0; j < population[i].Count; j++)
                {
                    population[i][j] = Math.Round(population[i][j], 5);
                }
            }

        }
        //обработчик события нажатия на вторую кнопку в третьем окне (Задача оптимизации инвестиционного портфеля) для удаления графика

        private void button8_Click(object sender, EventArgs e)
        {
            this.chart3.Series[0].Points.Clear();
        }
        //обработчик события нажатия на третью кнопку в третьем окне (Задача оптимизации инвестиционного портфеля) для закрытия окна

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //обработчик события нажатия на четвертую кнопку в третьем окне (Задача оптимизации инвестиционного портфеля) для создания отчета с решением задачи

        private void button10_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            saveFileDialog.Title = "Сохранить отчет как...";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                Document doc = new Document();

                try
                {
                    PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                    doc.Open();

                    Paragraph title = new Paragraph("Parameters of the Modified Ant Colony Optimization Method");
                    title.Alignment = Element.ALIGN_CENTER; 
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table1 = new PdfPTable(6);
                    table1.WidthPercentage = 100;
                    string[] columnNames = { "M", "R", "K", "ksi", "q", "e" };

                    foreach (string columnName in columnNames)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(columnName));
                        table1.AddCell(cell);
                    }
                    string[] columnNames1 = { M.ToString(), R.ToString(), K.ToString(), ksi.ToString(), q.ToString(), eps.ToString() };

                    foreach (string columnName in columnNames1)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(columnName));
                        table1.AddCell(cell);
                    }
                    doc.Add(table1);
                    doc.Add(new Paragraph("\n"));

                    string filePat = "C:\\Users\\User\\Downloads\\res3.txt";

                    List<double> averageReturns = new List<double>();
                    List<List<double>> risk = new List<List<double>>();
                    using (StreamReader reader = new StreamReader(filePat))
                    {
                        string line;
                        bool readingRisk = false;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line == "средняя доходность")
                            {
                                readingRisk = false;
                                continue;
                            }
                            else if (line == "ковариационная матрица")
                            {
                                readingRisk = true;
                                continue;
                            }

                            List<double> values = new List<double>(Array.ConvertAll(line.Split(), double.Parse));

                            if (!readingRisk)
                            {
                                averageReturns.AddRange(values);
                            }
                            else
                            {
                                risk.Add(values);
                            }
                        }

                        title = new Paragraph("Average Returns");
                        title.Alignment = Element.ALIGN_CENTER; 
                        doc.Add(title);
                        doc.Add(new Paragraph("\n"));

                        PdfPTable table2 = new PdfPTable(averageReturns.Count);
                        table2.WidthPercentage = 100;
                        for (int j = 0; j < averageReturns.Count; j++)
                        {
                            table2.AddCell(averageReturns[j].ToString());
                        }
                        doc.Add(table2);
                        doc.Add(new Paragraph("\n"));
                        title = new Paragraph("Covariance Matrix");
                        title.Alignment = Element.ALIGN_CENTER; 
                        doc.Add(title);
                        doc.Add(new Paragraph("\n"));
                        PdfPTable table3 = new PdfPTable(risk.Count);
                        table2.WidthPercentage = 100;
                        for (int j = 0; j < risk.Count; j++)
                        {
                            foreach (var value in risk[j])
                            {
                                table3.AddCell(value.ToString());
                            }
                        }
                        doc.Add(table3);
                        doc.Add(new Paragraph("\n"));
                        title = new Paragraph("Values ​​of securities portfolio weights");
                        title.Alignment = Element.ALIGN_CENTER; 
                        doc.Add(title);
                        doc.Add(new Paragraph("\n"));

                        PdfPTable table = new PdfPTable(I[0].Count + 2);
                        table.WidthPercentage = 100;

                        for (int i = 0; i < I[0].Count; i++)
                        {
                            table.AddCell($"x{i + 1}");
                        }

                        table.AddCell("E");
                        table.AddCell("V");

                        for (int j = 0; j < I.Count; j++)
                        {
                            foreach (var value in I[j])
                            {
                                table.AddCell(value.ToString());
                            }
                            table.AddCell(population[j][0].ToString());
                            table.AddCell(population[j][1].ToString());
                        }

                        doc.Add(table);
                        doc.Add(new Paragraph("\n"));
                        title = new Paragraph("Risk vs. Profit Dependency Chart");
                        title.Alignment = Element.ALIGN_CENTER; 
                        doc.Add(title);
                        doc.Add(new Paragraph("\n"));

                        using (MemoryStream ms = new MemoryStream())
                        {
                            chart3.SaveImage(ms, ChartImageFormat.Png);
                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(ms.GetBuffer());
                            img.ScaleToFit(520, 390);
                            doc.Add(img);
                        }

                        Console.WriteLine($"Отчет сохранен в файле: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании отчета: {ex.Message}");
                }
                finally
                {
                    doc.Close();
                }
            }
        }
        //обработчик события нажатия на пятую кнопку в третьем окне (Задача оптимизации инвестиционного портфеля) для скрытия точного решения

        private void button11_Click(object sender, EventArgs e)
        {
            this.chart2.Series[1].Enabled = !this.chart2.Series[1].Enabled;
        }
        //обработчик события нажатия на четвертую кнопку во втором окне (Примеры задач с ограничениями типа неравенств) для создания отчета

        private void button12_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < I.Count; i++)
            {
                for (int j = 0; j < I[i].Count; j++)
                {
                    I[i][j] = Math.Round(I[i][j], 4);
                }
            }
            for (int i = 0; i < population.Count; i++)
            {
                for (int j = 0; j < population[i].Count; j++)
                {
                    population[i][j] = Math.Round(population[i][j], 4);
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            saveFileDialog.Title = "Сохранить отчет как...";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                Document doc = new Document();

                try
                {
                    PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                    doc.Open();

                    Paragraph title = new Paragraph("Parameters of the Modified Ant Colony Optimization Method");
                    title.Alignment = Element.ALIGN_CENTER; 
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table1 = new PdfPTable(6);
                    table1.WidthPercentage = 100;
                    string[] columnNames = { "M", "R", "K", "ksi", "q", "n" };

                    foreach (string columnName in columnNames)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(columnName));
                        table1.AddCell(cell);
                    }
                    string[] columnNames1 = { M.ToString(), R.ToString(), K.ToString(), ksi.ToString(), q.ToString(), xx.ToString() };

                    foreach (string columnName in columnNames1)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(columnName));
                        table1.AddCell(cell);
                    }
                    doc.Add(table1);
                    doc.Add(new Paragraph("\n"));


                    title = new Paragraph("Values ​​of the solution vector and vector of objective functions");
                    title.Alignment = Element.ALIGN_CENTER; 
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(I[0].Count + 2);
                    table.WidthPercentage = 100;

                    for (int i = 0; i < I[0].Count; i++)
                    {
                        table.AddCell($"x{i + 1}");
                    }

                    table.AddCell("f1");
                    table.AddCell("f2");
                    Console.WriteLine("I " + I.Count);
                    Console.WriteLine("pop " + population.Count);

                    for (int j = 0; j < I.Count; j++)
                    {
                        foreach (var value in I[j])
                        {
                            table.AddCell(value.ToString());
                        }
                        table.AddCell(population[j][0].ToString());
                        table.AddCell(population[j][1].ToString());
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph("\n"));
                    title = new Paragraph("Graph of the dependency between two criteria");
                    title.Alignment = Element.ALIGN_CENTER; 
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    using (MemoryStream ms = new MemoryStream())
                    {
                        chart1.SaveImage(ms, ChartImageFormat.Png);
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(ms.GetBuffer());

                        img.ScaleToFit(520, 390); 

                        iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph();
                        paragraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER; 
                        paragraph.Add(img);

                        doc.Add(paragraph);
                    }

                    Console.WriteLine($"Отчет сохранен в файле: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании отчета: {ex.Message}");
                }
                finally
                {
                    doc.Close();
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            this.chart1.Series[1].Enabled = !this.chart1.Series[1].Enabled;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\"; 
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"; 
            openFileDialog.FilterIndex = 1; 

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
            }
        }
    }
}
