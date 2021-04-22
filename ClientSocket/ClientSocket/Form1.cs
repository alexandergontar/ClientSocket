using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace ClientSocket
{
    public partial class Form1 : Form
    {
        private TcpClient clientSocket;
        private NetworkStream serverStream;
        private byte[] outStream;
        private string str = "LED=DISC";
        private string IPadd;
        private int tcp_port;
        private int t;
        private int h;
        private bool on_off=false, mon=false;
        private Bitmap bitmap;
        private delegate void requestUnit(object sender, EventArgs e);
        private event requestUnit req;

        public Serial s_conn;


        public Form1()
        {
            InitializeComponent();
            this.FormClosing += ProgramClose;
            req += new requestUnit(button1_Click);
            //gr = tempBar.CreateGraphics();
            // bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //req(null, null);
        }

       

        

        public void msg(string mesg)
        {
           // textBox1.Text = Environment.NewLine + " >> " + mesg;
            textBox1.Invoke(new Action(()=> { textBox1.Text = Environment.NewLine + " >> " + mesg; }));
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            
                clientSocket = new System.Net.Sockets.TcpClient();
                msg("Connecting...");
                toolStripStatusLabel1.Text = "Connecting...";



                try
                {
                    tcp_port = (int)portNum.Value;
                    IPadd = textHostName.Text;

                  
                   //clientSocket.Connect(IPadd, tcp_port);
                var result = clientSocket.BeginConnect(IPadd, tcp_port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
                if (!success)
                {
                    throw new Exception("Failed to connect.");
                }

                  serverStream = clientSocket.GetStream();
                  toolStripStatusLabel1.Text = "Client Socket Program - Server Connected ...";
                

            }
                catch (Exception ex) { toolStripStatusLabel1.Text = ex.Message; return; }

                try
                {
                   // serverStream = clientSocket.GetStream();
                    if (radioBtnOn.Checked) str = "LED=ON";
                    if (radioBtnOff.Checked) str = "LED=OFF";
                    if (radioBtnAuto.Checked) str = "LED=Auto "+setTemp.Value.ToString();
                //outStream = Encoding.UTF8.GetBytes("GET /HTTP/ 1.1 /" + str + "\r\n");
                outStream = Encoding.UTF8.GetBytes(" /"+str + "\r\n");
                   serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();
                    byte[] inStream = new byte[100025];
                    Console.WriteLine("-----------"+ (int)clientSocket.ReceiveBufferSize);
                    serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                    string returndata = Encoding.UTF8.GetString(inStream);
                    if (returndata.IndexOf("ON") != -1) on_off = true;
                    else on_off = false;
                    Regex reg = new Regex(@"\d+");
                    MatchCollection matches = reg.Matches(returndata);
                if (matches.Count == 2)
                {
                    t = int.Parse(matches[0].Value);
                    h = int.Parse(matches[1].Value);
                }
                if (matches.Count == 3)
                {
                    t = int.Parse(matches[1].Value);
                    h = int.Parse(matches[2].Value);
                }
                msg(returndata);
                clientSocket.Close();
                //Picture box

                bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.Image = null;
                Graphics gr = Graphics.FromImage(bitmap);
                // gr.FillRectangle(Brushes.Red, 10, 180-2*t, 5, 2*t);
                // gr.FillRectangle(Brushes.Blue, 50, 180 - 2*h, 5, 2*h);
                    gr.DrawEllipse(Pens.Black, 73, 23, 39, 39);
                    gr.DrawEllipse(Pens.Black, 73, 113, 39, 39);
                if (on_off)
                {
                    gr.FillEllipse(Brushes.Green, 75, 25, 35, 35);                    
                }
                else
                {
                    gr.FillEllipse(Brushes.Black, 75, 115, 35, 35);                    
                }
                pictureBox1.Image = bitmap;
                //Chart area
                chart1.Series["Temperature"].Points.Clear();
                chart1.Series["Humidity"].Points.Clear();
                chart1.Series["Temperature"].Points.AddXY("T1",t);
               // chart1.Series["Temperature"].Points.AddXY("T2", h);
               // chart1.Series["Humidity"].Points.AddXY("H1", t);
                chart1.Series["Humidity"].Points.AddXY("H2", h);

            }

                catch (Exception ex) {  toolStripStatusLabel1.Text= ex.Message; }
            
        }

              
        private void ProgramClose(Object sender, FormClosingEventArgs e)
        {
          try {
                NetworkStream serverStream = clientSocket.GetStream();
                outStream = Encoding.UTF8.GetBytes("GET /HTTP/ 1.1 /LED=DISC\r\n");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
              }
               catch { }
        }

        private void portNum_ValueChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Port Changed", "Warning");
        }

        private void btnSerial_Click(object sender, EventArgs e)
        {
            s_conn = new Serial();
            s_conn.ShowDialog();
        }

        private void btnMonitor_Click(object sender, EventArgs e)
        {

           if (!mon)
            {
                
                btnMonitor.Text = "Stop";
                mon = true;
            }
            else
            {
                
                btnMonitor.Text = "Mon";
                mon = false;
            }
             new Task(()=> {
               while (mon)
               {
                Console.Beep();
                req(null, null);
                System.Threading.Thread.Sleep(10000);
               }

            }).Start();

 

        }
    }
}
