using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace implementacion_funcionVA
{
    public partial class Form1 : Form
    {

        int n = 3; //rows
        int m = 100; //columns
        int floor_code = 9;
        int pipe_code = 4;
        int blank_code = 0;
        int jump_space = 6;
        bool validate = false;
        double[,] temp1;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int jugable = 0;
            int no_jugable = 0;
            for (int k = 0; k < 1; k++)
            {

                //1 = goomba 0.01
                //2 = soild 0,1
                //3 = ladrillo 0,136
                //4 = coin 0,012
                //5 = pipe 0,009
                //6 = koopa 0,008
                List<double> elements = new List<double>() { 1, 2, 3, 4, 5 ,6};
                List<double> pdf = new List<double>() { 0.01f, 0.1f, 0.136f, 0.012f, 0.009f, 0.008f };
                double p = new System.Random((int)DateTime.Now.Ticks).NextDouble();
                temp1 = Superva(elements, pdf, p);

                richTextBox1.Text = string.Empty;
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j < m; j++)
                        richTextBox1.Text += temp1[i, j].ToString() + "   ";
                    richTextBox1.Text += System.Environment.NewLine;
                }

                coord start = new coord(n - 1, 0);
                coord end = new coord(n - 1, m - 1);

                if (bread_first_traversal(start, end, temp1))
                {
                    label1.Text = "Jugable";
                    jugable++;
                }
                else
                {
                    label1.Text = "No Jugable";
                    no_jugable++;
                }
            }
            label1.Text = jugable.ToString() + "::" + no_jugable.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        bool bread_first_traversal(coord start, coord end, double[,] world)
        {
            List<coord> queue = new List<coord>();
            queue.Add(start);
            world[start.X, start.Y] = 8;
            while (queue.Count > 0)
            {
                coord current = queue[queue.Count - 1];
                queue.Remove(current);
                if (current.X >= 0 && current.Y == m-1)
                {
                    richTextBox2.Text = string.Empty;
                    for (int i = 0; i <= n; i++)
                    {
                        for (int j = 0; j < m; j++)
                            richTextBox2.Text += world[i, j].ToString() + "   ";
                        richTextBox2.Text += System.Environment.NewLine;
                    }
                    return true;
                }
                foreach (coord c in next_Positions(current.X, current.Y))
                {
                    if (isValid(c.X, c.Y, n, m) && (world[c.X, c.Y] == 0 || world[c.X, c.Y] == 1 || world[c.X, c.Y] == 6))
                    {
                        world[c.X, c.Y] = 8;
                        queue.Add(new coord(c.X, c.Y));
                    }
                }
            }
            richTextBox2.Text = string.Empty;
            for (int i = 0; i <= n; i++)
            {
                for (int j = 0; j < m; j++)
                    richTextBox2.Text += world[i, j].ToString() + "   ";
                richTextBox2.Text += System.Environment.NewLine;
            }
            return false;
        }

        bool isValid(int row, int col, int max_row, int max_col)
        {
            return row >= 0 && col >= 0 && row < max_row && col < max_col;
        }

        List<coord> next_Positions(int row, int col)
        {
            List<coord> res = new List<coord>() { new coord(row - 1, col + 0), new coord(row + 0, col - 1), new coord(row + 0, col + 1), new coord(row + 1, col + 0) ,
            new coord(row - 1, col - 1), new coord(row + 1, col - 1), new coord(row - 1, col + 1), new coord(row + 1, col + 1) };
            return res;
        }

        List<double> Cumsum(List<double> pdf)
        {
            List<double> res = new List<double>() { pdf[0] };
            for (int i = 0; i < pdf.Count - 1; i++)
                res.Add(res[i] + pdf[i + 1]);
            return res;
        }

        double Va(List<double> x, List<double> pdf, double p)
        {
            List<double> cdf = Cumsum(pdf);
            for (int i = 0; i < cdf.Count; i++)
                if (p < cdf[i])
                    return x[i];
            return blank_code; //blank space
        }

        //validate left on elementA
        double[,] ConditionalValidation(double[,] world, double elementA, double elementB, double probA, double probB, double p)
        {
            double t = p;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    if (world[i, j] == elementA)
                    {
                        if (j - 1 >= 0) //validate matrix border
                        {
                            while (t == p)
                                p = new System.Random((int)DateTime.Now.Ticks).NextDouble();
                            t = p;
                            double conditional = probA / probB;
                            if (p <= conditional)
                                world[i, j - 1] = elementB;
                        }
                    }
                }
            return world;
        }

        double[,] Validate_pipes(double[,] temp)
        {
            for (int i = n - 1; i >= 0; i--)
                for (int j = 0; j < m; j++)
                    if (temp[i, j] == pipe_code)//validate pipe
                    {
                        if (temp[n, j] == floor_code && i == n - 1)//if floor exists and the pipe is over then
                        {
                            int t = i - 1;
                            int k = j;
                            while (t >= 0)//clean the space over the pipe 
                            {
                                temp[t, k] = blank_code;
                                if (k < m - 1)
                                {
                                    k++;
                                    temp[t, k] = blank_code;
                                }
                                t--;
                            }
                        }
                        else
                            temp[i, j] = blank_code;
                    }
            return temp;
        }

        double[,] Validate_floor(double[,] res)
        {
            int cont = 0;
            for (int i = 0; i < m; i++)
            {
                if (res[n, i] == blank_code)
                    cont++;
                else
                {
                    if (cont > jump_space)
                    {
                        while (cont > jump_space)
                        {
                            cont -= jump_space;
                            res[n, i - cont] = floor_code;
                        }
                        cont = 0;
                    }
                }
            }
            return res;
        }

        double[,] Add_floor(double[,] temp) // 9 is floor
        {
            double[,] res = new double[n + 1, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                    res[i, j] = temp[i, j];
            double p = new System.Random().NextDouble();
            double t = p;
            for (int i = 0; i < m; i++)
            {
                while (t == p)
                    p = new System.Random((int)DateTime.Now.Ticks).NextDouble();
                t = p;
                res[n, i] = Va(new List<double>() { floor_code }, new List<double>() { 0.75f }, p);
            }
            res[n, m - 1] = floor_code;
            if (validate)
                return Validate_pipes(Validate_floor(res));
            return res;
        }

        double[,] Superva(List<double> x, List<double> pdf, double p)
        {
            double t = p;
            double[,] res = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    while (t == p)
                        p = new System.Random((int)DateTime.Now.Ticks).NextDouble();
                    t = p;
                    double temp = Va(x, pdf, p);
                    res[i, j] = temp;
                    if (temp == pipe_code && j + 1 < m)//clean the space in front of the pipe
                    {
                        j++;
                        res[i, j] = blank_code;
                    }
                }
            return Add_floor(res);
        }


    }

    public class coord
    {
        int x;
        int y;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public coord(int ejeX, int ejeY)
        {
            x = ejeX;
            y = ejeY;
        }
    }

}
