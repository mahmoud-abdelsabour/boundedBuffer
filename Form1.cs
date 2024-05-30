using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace boundedBuffer
{
    public partial class Form1 : Form
    {
        class BoundedBuffer
        {
            public int?[] buffer;
            public int size;
            public int inIndex = 0;
            private int outIndex = 0;
            private SemaphoreSlim mutex;
            private SemaphoreSlim empty;
            private SemaphoreSlim full;

            public BoundedBuffer(int size)
            {
                this.size = size;
                buffer = new int?[size];
                mutex = new SemaphoreSlim(1);
                empty = new SemaphoreSlim(size);
                full = new SemaphoreSlim(0);
            }

            public void Produce(int item)
            {
                empty.Wait();
                mutex.Wait();

                buffer[inIndex] = item;
                inIndex = (inIndex + 1) % size;

                mutex.Release();
                full.Release();
            }

            public int? Consume()
            {
                full.Wait();
                mutex.Wait();

                int? item = buffer[outIndex];
                buffer[outIndex] = null;
                outIndex = (outIndex + 1) % size;

                mutex.Release();
                empty.Release();

                return item;
            }
        }
        public class LineActor
        {
            public Point Start, End;
            public Pen Pen;
        }
        Bitmap off;
        List<List<LineActor>> Buffer = new List<List<LineActor>>();
        List<string> BufferContent = new List<string>();
        public int yTop;
        public int yBottom;
        public List<int> X = new List<int>();
        int f = 0;
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            Paint += Form1_Paint;
            BackColor = Color.White;
            Text = "Bounded Buffer";
        }
        // Main
        BoundedBuffer buffer;
        public void main(int size)
        {
            DrawBuffer(size);
            buffer = new BoundedBuffer(size);
        }

        // Drawing
        public void DrawBuffer(int size)
        {
            List<LineActor> lines = new List<LineActor>();

            int xStart = 20;
            int xEnd = 20;
            int yStart = ClientSize.Height / 4;
            int yEnd = yStart + 50;

            for (int i = 0; i < size + 1; i++)
            {
                X.Add(xStart + 5);
                LineActor VL = new LineActor();
                VL.Pen = new Pen(Color.Blue, 3);
                VL.Start = new Point(xStart, yStart);
                VL.End = new Point(xEnd, yEnd);
                lines.Add(VL);

                xStart = xEnd += 50;
            }
            xEnd -= 50;
            xStart = 20;
            yStart = yEnd = ClientSize.Height / 4;
            yTop = yStart;

            for (int i = 0; i < 2; i++)
            {
                LineActor HL = new LineActor();
                HL.Pen = new Pen(Color.Blue, 3);
                HL.Start = new Point(xStart, yStart);
                HL.End = new Point(xEnd, yEnd);
                lines.Add(HL);

                yStart = yEnd += 50;
                yBottom = yStart;
            }

            Buffer.Add(lines);
            DrawDubb(this.CreateGraphics());
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawDubb(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            off = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        public void DrawScene(Graphics g)
        {
            g.Clear(BackColor);

            for (int j = 0; j < Buffer.Count; j++)
            {
                List<LineActor> ptrav = Buffer[j];
                for (int i = 0; i < ptrav.Count; i++)
                {
                    LineActor pt = ptrav[i];
                    g.DrawLine(pt.Pen, pt.Start, pt.End);
                }
            }

            //for (int i = 0; i < BufferContent.Count; i++)
            //{
            //    string s = BufferContent[i];
            //    Point p = new Point(X[i], yTop + 5);
            //    g.DrawString(s, new Font("Arial", 20, FontStyle.Regular), Brushes.Black, p);
            //}

            if (buffer != null)
            {
                for (int i = 0; i < buffer.size; i++)
                {
                    if (buffer.buffer[i] != null)
                    {
                        string s = buffer.buffer[i].ToString();
                        Point p = new Point(X[i], yTop + 5);
                        g.DrawString(s, new Font("Arial", 20, FontStyle.Regular), Brushes.Black, p);
                    }
                }
            }
        }
        public void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(off);
            DrawScene(g2);
            g.DrawImage(off, 0, 0);

        }

        private void textBox1_TextChanged(object sender, EventArgs e) // buffer size
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e) // producer value
        {

        }

        private void button1_Click(object sender, EventArgs e) // create
        {
            int content = int.Parse(textBox1.Text);
            main(content);
            textBox1.Text = "";
        }

        private void button2_Click(object sender, EventArgs e) // produce
        {
            Thread producer = new Thread(() => {

                buffer.Produce(int.Parse(textBox2.Text));

                BufferContent.Add(textBox2.Text);

                //textBox2.Text = "";

                Thread.Sleep(100);


                this.Invoke((MethodInvoker)delegate
                {
                    DrawDubb(this.CreateGraphics());
                });

                //this.Invoke((MethodInvoker)delegate
                //{
                //    MessageBox.Show("Done !  Next item index ->" + buffer.inIndex);
                //});

            });

            producer.Start();
        }

        private void button3_Click(object sender, EventArgs e) // Remove
        {
            Thread consumer = new Thread(() => {
                
                
                int? item = buffer.Consume();
                MessageBox.Show($"Consumed: {item}");
                Thread.Sleep(200);
                DrawDubb(this.CreateGraphics());

            });

            consumer.Start();
        }
    }
}
