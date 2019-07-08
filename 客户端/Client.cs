using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 客户端
{
    public struct FLASHWINFO
    {
        public UInt32 cbSize;
        public IntPtr hwnd;
        public UInt32 dwFlags;
        public UInt32 uCount;
        public UInt32 dwTimeout;
    }
    public partial class Client : Form
    {
        //消息提醒窗体闪动
        public const UInt32 FLASHW_STOP = 0;
        public const UInt32 FLASHW_CAPTION = 1;
        public const UInt32 FLASHW_TRAY = 2;
        public const UInt32 FLASHW_ALL = 3;
        public const UInt32 FLASHW_TIMER = 4;
        public const UInt32 FLASHW_TIMERNOFG = 12;
        [DllImport("user32.dll")]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr handle, bool invert);

        //消息提醒图标闪烁
        private Icon blank = new Icon("icon/d.ico");
        private Icon normal = new Icon("icon/h.ico");
        private bool _status = true;


        private string username;//用户名
        private string password;//密码
        private delegate void kongjian(string str, int a);
        private ClientEvent clientevent;
        

        public Client(string username, string password)
        {
            InitializeComponent();
            this.username = username;
            this.password = password;
        }

        private void updatelistbox(string str, int a)
        {
            if (a == 0)
            {
                receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                receiveTxt.AppendText(str + "\r\n");

                if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    timer1.Enabled = true;
                    timer1.Start();

                    //窗体闪动
                    FLASHWINFO fInfo = new FLASHWINFO();
                    fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
                    fInfo.hwnd = this.Handle;
                    fInfo.dwFlags = FLASHW_TRAY | FLASHW_TIMERNOFG;
                    fInfo.uCount = UInt32.MaxValue;
                    fInfo.dwTimeout = 0;
                    FlashWindowEx(ref fInfo);
                }
                ////通过文件播放
                SoundPlayer vSoundPlayer = new SoundPlayer();
                vSoundPlayer.Stream = new FileStream(@"\VSWork\聊天软件升级版3.0\客户端\客户端\bin\Debug\sound\8407.wav",
                    FileMode.Open, FileAccess.Read);
                vSoundPlayer.Play();
            }
        }

        private void updatecombobox(string str, int a)
        {
            if (a == 0)
            {
                listBox2.Items.Add(str);
            }
            else if (a == 1)
            {
                listBox2.Items.Remove(str);
            }
            else if (a == 2)
            {
                listBox2.DataSource = null;
                listBox2.Items.Clear();
            }
        }

        private void wei(string str, int a, int b)
        {
            if (b == 0)
            {
                BeginInvoke(new kongjian(updatecombobox), new object[] { str, a });
            }
            else if (b == 1)
            {
                BeginInvoke(new kongjian(updatelistbox), new object[] { str, a });
            }
        }





        //连接服务器
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                clientevent.setipport(ipTxt.Text, int.Parse(portTxt.Text), username, password);
            }
            catch
            {
                MessageBox.Show("输入正确格式");
                return;
            }
            if (clientevent.getflag() == false)
            {
                clientevent.Open();
                if (clientevent.getflag())
                {
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                    receiveTxt.AppendText("恭喜" + username + "连接成功" + "\r\n");
                }
                else
                {
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                    receiveTxt.AppendText("不好意思" + username + "，你连接失败" + "\r\n");
                }
            }
            else
            {
                MessageBox.Show("你已连接，无须再连");
            }
        }



        //与服务器断开
        private void button2_Click(object sender, EventArgs e)
        {
            if (clientevent.getflag())
            {
                clientevent.close();
            }
            else
            {
                MessageBox.Show(username + "与服务器断开");
            }
        }



        //发送内容
        private void button3_Click(object sender, EventArgs e)
        {
            if (sendTxt.Text != "")
            {
                int a = 0;
                if (checkBox1.Checked)
                {
                    a = 1;
                }
                if (a == 0 && listBox2.Text == "")
                {
                    MessageBox.Show("请选择聊天对象");
                    return;
                }
                if (comboBox2.Text == "UDP")
                {
                    MessageBox.Show("未实现udp通讯，请选择tcp");
                    return;
                }
                receiveTxt.SelectionAlignment = HorizontalAlignment.Right;
                receiveTxt.AppendText(username + clientevent.GetCurrentTime() + "\r\n" + sendTxt.Text);
                receiveTxt.AppendText("\r\n"+"\r\n");
                clientevent.send(sendTxt.Text+"\r\n", a, comboBox2.Text, listBox2.Text);
                sendTxt.Text = "";
            }
        }



        private void receiveChanged(object sender, EventArgs e)
        {
            //文本框选中的起始点在最后
            receiveTxt.SelectionStart = receiveTxt.TextLength;
            //将控件内容滚动到当前插入符号位置
            receiveTxt.ScrollToCaret();
        }




        /// <summary>
        /// 快捷键 Enter 发送信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            //如果用户按下了Enter键
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (sendTxt.Text.Trim() != "")
                {
                    int a = 0;
                    if (checkBox1.Checked)
                    {
                        a = 1;
                    }
                    if (a == 0 && listBox2.Text == "")
                    {
                        MessageBox.Show("请选择聊天对象");
                        return;
                    }
                    if (comboBox2.Text == "UDP")
                    {
                        MessageBox.Show("未实现udp通讯，请选择tcp");
                        return;
                    }
                    //则调用 服务器向客户端发送信息的方法
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Right;
                    receiveTxt.AppendText(username + clientevent.GetCurrentTime() + "\r\n" + sendTxt.Text);
                    receiveTxt.AppendText("\r\n");
                    clientevent.send(sendTxt.Text, a, comboBox2.Text, listBox2.Text);
                    sendTxt.Text = "";
                }
            }
        }

        //图标闪烁
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_status)
                notifyIcon1.Icon = normal;
            else
                notifyIcon1.Icon = blank;

            _status = !_status;
            Thread.Sleep(500);
        }
        //单击图标弹出消息框
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            timer1.Stop();
            notifyIcon1.Icon = blank;
        }


        private void Client_Load(object sender, EventArgs e)
        {
            clientevent = new ClientEvent(ipTxt.Text, 12000);
            clientevent.upcombobox += new ClientEvent.weituo(wei);
            clientevent.uplistbox += new ClientEvent.weituo(wei);
            comboBox2.Text = comboBox2.Items[0].ToString();
            skinEngine1.SkinFile = Application.StartupPath + @"\皮肤\Wave\WaveColor1.ssk";
        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (clientevent.getflag())
            {
                clientevent.close();
            }
        }
    }
}
