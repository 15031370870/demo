using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 服务端
{
    public partial class Server : Form
    {
        private delegate void kongjian(string str, int a);
        private ServerEvent serverevent;


        public Server()
        {
            InitializeComponent();
            //CheckForIllegalCrossThreadCalls = false;
        }

        private void updatelistbox(string str, int a)
        {
            if (a == 0)
            {
                receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                receiveTxt.AppendText(str+"\r\n");
            }
            else if (a == 1 && !str.Substring(0, 1).Equals("$"))
            {
                receiveTxt.Text.Remove(0,str.Length);
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

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int port = int.Parse(portTxt.Text);
                serverevent.setIPport(ipTxt.Text,port);
            }
            catch
            {
                MessageBox.Show("输入正确的格式");
                return;
            }
            if (serverevent.getflag() == false)
            {
                serverevent.Open();
                if (serverevent.getflag())
                {
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                    receiveTxt.AppendText("服务器创建成功"+"\r\n");
                }
                else
                {
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                    receiveTxt.AppendText("服务器创建失败"+"\r\n");
                }
            }
            else
            {
                MessageBox.Show("已创建");
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {
            if (serverevent.getflag())
            {
                serverevent.close();
                receiveTxt.SelectionAlignment = HorizontalAlignment.Left;
                receiveTxt.AppendText("服务端关闭"+"\r\n");
            }
            else
            {
                MessageBox.Show("已关闭");
            }
        }



        private void button1_Click(object sender, EventArgs e)
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
                    MessageBox.Show("请选择客户端");
                    return;
                }
                if (serverevent.getflag())
                {
                    receiveTxt.SelectionAlignment = HorizontalAlignment.Right;
                    receiveTxt.AppendText("服务端" + serverevent.GetCurrentTime()+"\r\n"+ sendTxt.Text+"\r\n");
                    serverevent.send(sendTxt.Text, a, listBox2.Text);
                    sendTxt.Text = "";
                }
                else
                {
                    MessageBox.Show("未打开服务端");
                }
            }
            else
            {
                MessageBox.Show("输入发送的内容");
            }
        }



        private void Server_Load(object sender, EventArgs e)
        {
            serverevent = new ServerEvent(ipTxt.Text, 12000);
            serverevent.listboxaction += new ServerEvent.weituo(wei);
            serverevent.comboboxaction += new ServerEvent.weituo(wei);
            skinEngine1.SkinFile = Application.StartupPath + @"\皮肤\WinXP\XPBlue.ssk";

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
                        MessageBox.Show("请选择客户端");
                        return;
                    }
                    if (serverevent.getflag())
                    {
                        receiveTxt.SelectionAlignment = HorizontalAlignment.Right;
                        receiveTxt.AppendText("服务端" + serverevent.GetCurrentTime()+"\r\n"+ sendTxt.Text + "\r\n");
                        serverevent.send(sendTxt.Text, a, listBox2.Text);
                        sendTxt.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("未打开服务端");
                    }
                }
                else
                {
                    MessageBox.Show("输入发送的内容");
                }
            }
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serverevent.getflag())
            {
                serverevent.close();
            }
        }


        private void receiveChanged(object sender, EventArgs e)
        {
            //文本框选中的起始点在最后
            receiveTxt.SelectionStart = receiveTxt.TextLength;
            //将控件内容滚动到当前插入符号位置
            receiveTxt.ScrollToCaret();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        //private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Brush myBrush = Brushes.Black; //初始化字体颜色=黑色  
        //                                   //此处需要根据当前item中的文字算出item的高，比如算出后是90，则
        //    this.receiveTxt.ItemHeight = 90;

        //    e.Graphics.DrawString(receiveTxt.Items[e.Index].ToString(), e.Font, myBrush, e.Bounds, null);

        //    e.DrawFocusRectangle();

        //}

    }
}
