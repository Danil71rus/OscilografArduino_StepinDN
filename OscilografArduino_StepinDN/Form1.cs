using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace OscilografArduino_StepinDN
{
    public partial class Form1 : Form
    {
        SerialPort myPort;
        double u = 0;
        private int width = 765, wigth_sum = 100, height = 465, viewX = 0, viewY = 0, scale = 40;
        private double graphBegin = 0, graphEnd = 10, graphStep = 0.01;
        private List<PointF> listPoint = new List<PointF>() { };
        private Point clipPosition;
        private bool clip, start = false;
        private double time = 0;
        //-------задающие воздействие--------------
        private double f(double x)
        {
            if (checkBox1.Checked == true)
            { return -Math.Cos(x); }
            return u;
        }
        //-------------------------------------------
        double perevod(string f)
        {
            double e;
            string[] b = f.Split(new Char[] { '.' });
            string t;
            try
            {
                t = b[0] + "," + b[1];
                e = Convert.ToDouble(t);
                return e;
            }
            catch (Exception rt)
            {
                return 0.0;
            }
        }
       
        public Form1()
        {
            InitializeComponent();
        }     

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            int speed = Convert.ToInt32(comboBox2.Text.ToString());
            string name = comboBox1.Text.ToString();

            try
            {
                timer1.Interval = Convert.ToInt32(textBox1.Text.ToString());

                myPort = new SerialPort(name, speed, Parity.None, 8, StopBits.One);
                myPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                myPort.Open();
                //label5.Text = myPort.ReadLine().ToString();
                buttonConnect.Enabled = false;
                buttonRun.Enabled = true;
                buttonDisconnect.Enabled = true;
                button1.Enabled = true;
                MessageBox.Show("Успешное соединением с " + name);
                label7.Text = "Connect";
                label7.ForeColor = Color.Blue;
            }
            catch (Exception o)
            {
                MessageBox.Show("Проблема с соединением " + name);
                MessageBox.Show(o.Message);
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                buttonConnect.Enabled = true;
                buttonDisconnect.Enabled = false;
                buttonRun.Enabled = false;
                button1.Enabled = false;

                myPort.Close();
                myPort.Dispose();
                // PCI.PCIDeviceClose();
                label5.Text = "";
                label7.Text = "Disconnect";
                label7.ForeColor = Color.Red;
            }
            catch (Exception w)
            {
                MessageBox.Show(w.Message);
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == true) { timer1.Enabled = false; start = true; }
            else if (timer1.Enabled == false) { timer1.Enabled = true; start = true; }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //---------------------------------------------------------- 
            label5.Text = (-u).ToString(); //myPort.ReadLine().ToString();
            //------------------------------------------------------------

            Invalidate(true);
            time += (double)timer1.Interval / 1000;                   // интервал времени ----------------при изменеии парметров таймера учитывапть эту функцию


            double my = f(time);          //применение функции с входным воздействием 
            viewX = -(int)(time * scale) + 50; //следование за построеной функцией по оси Х
            // viewY = -(int)(my) * scale;   //следование за построеной функцией по оси Y

            listPoint.Add(new PointF((float)(time * scale), (float)(my * scale)));
            label4.Text = "Interval: " + timer1.Interval + "ms;   " + Math.Round(time, 3).ToString() + " c";
            if (listPoint.Count == 3000)  //обновление каждые 300с (5 мин) //  5мин*60с*10=3000
            {
                listPoint.Clear();
                wigth_sum = 100;
                time = 0;
            }

            if (width + wigth_sum <= time * 10 * 4) //--расширение построения сетки 
            {
                wigth_sum += 700;
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(width / 2 + viewX, height / 2 + viewY);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            for (int i = (int)scale; i < width * 2 + wigth_sum; i += (int)scale)
            {
                e.Graphics.DrawLine(Pens.LightGray, i, -height * 2, i, height * 2);
                e.Graphics.DrawLine(Pens.LightGray, -i, -height * 2, -i, height * 2);

                e.Graphics.DrawLine(Pens.LightGray, -width * 2, i, width * 2 + wigth_sum, i);
                e.Graphics.DrawLine(Pens.LightGray, -width * 2, -i, width * 2 + wigth_sum, -i);

                e.Graphics.DrawString((i / scale).ToString(), DefaultFont, Brushes.Black, i, 0);
                e.Graphics.DrawString((-i / scale).ToString(), DefaultFont, Brushes.Black, -i, 0);
            }
            /*for (int i = (int)scale; i < height * 2; i += (int)scale)
            {
                e.Graphics.DrawString((i / scale).ToString(), DefaultFont, Brushes.Black, 0, -i);
                e.Graphics.DrawString((-i / scale).ToString(), DefaultFont, Brushes.Black, 0, i);
            }*/
            //скользащая нумерация
            if (start)
            {
                for (int i = (int)scale; i < height * 2; i += (int)scale)
                {
                    e.Graphics.DrawString((i / scale).ToString(), DefaultFont, Brushes.Black, -viewX, -i);
                    e.Graphics.DrawString((-i / scale).ToString(), DefaultFont, Brushes.Black, -viewX, i);
                }
                //скользящая ось абцис
                e.Graphics.DrawLine(Pens.Gray, -viewX, -height * 2, -viewX, height * 2);
                /*  var temp = listPoint.Where(p => p.X == (double)(-viewX / scale) - 0.3 || p.X <= (double)(-viewX / scale) + 0.3);
                  foreach (var i in temp)
                  {
                      label5.Text = (-i.Y/scale).ToString();
                  }*/
                // label5.Text = ((double)-viewX / scale).ToString();
            }
            e.Graphics.DrawLine(Pens.Gray, -width * 2, 0, width * 2 + wigth_sum, 0);
            e.Graphics.DrawLine(Pens.LightGray, 0, -height * 2, 0, height * 2);



            //--------------func*-----------------

            for (int i = 1; i < listPoint.Count; i++)
            {
                e.Graphics.DrawLine(Pens.Red, listPoint[i - 1], listPoint[i]);
            }

            //------------------------------------   
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            clip = true;
            clipPosition = new Point(e.X - viewX, e.Y - viewY);
            Invalidate(true); 
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (clip)
            {
                viewX = e.X - clipPosition.X;
                viewY = e.Y - clipPosition.Y;
                Invalidate(true);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            clip = false; 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listPoint.Clear();
            wigth_sum = 100;
            time = 0;
            label4.Text = "";
            viewX = -(int)(time * scale); //следование за построеной функцией по оси Х
            // viewY = -(int) scale;   //следование за построеной функцией по оси Y
            label5.Text = "";
            Invalidate(true);
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            width = pictureBox1.Width;
            height = pictureBox1.Height;
            Invalidate(true);
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;

            //string msg = sp.ReadExisting();
            try
            {
                string w = sp.ReadLine();
                if (w != String.Empty)
                {
                    BeginInvoke(new Action(() => richTextBox1.AppendText(w)));

                    u = -perevod(w);
                    //  u = -Convert.ToDouble(w.Replace('.', ','));
                }
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message);
            }


        }

        private void Oscilograf(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comboBox1.Items.Add(port);
            }
        }
    }
}
