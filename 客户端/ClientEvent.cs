using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 客户端
{
    class ClientEvent
    {
        #region 委托
        public delegate void weituo(string str, int a, int b);
        public event weituo uplistbox;
        public event weituo upcombobox;
        #endregion
        string username;//用户名
        string password;//密码
        //private string localip;
        //private int localport;
        private Thread Thclient;
        private Socket client;
        private bool flag = false;
        private string ipadress;
        private int port;


        public ClientEvent(string ipadress, int port)
        {
            this.ipadress = ipadress;
            this.port = port;
        }


        ~ClientEvent()
        {
            close();
        }



        public void Open()
        {
            if (flag == false)
            {
                try
                {
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Connect(ipadress, port);
                }
                catch
                {
                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }
                    catch { client.Close(); }
                    finally { flag = false; }
                    return;
                }
                //发送用户名到服务器
                byte[] by = Encoding.UTF8.GetBytes(username);
                client.Send(by, by.Length, SocketFlags.None);
                flag = true;

                Thclient = new Thread(listen);
                Thclient.IsBackground = true;
                Thclient.Start();
            }
        }


        private void listen()
        {
            while (true)
            {
                try
                {
                    byte[] redate = new byte[1024];
                    int length = client.Receive(redate, SocketFlags.None);
                    if (length == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(redate, 0, length);
                    if (str != "" && str != string.Empty)
                    {
                        Thread Thdeal = new Thread(new ThreadStart(delegate ()
                        {
                            deal(str);
                        }));
                        Thdeal.IsBackground = true;
                        Thdeal.Start();
                    }

                }
                catch
                {
                    break;
                }
            }
            close();
        }


        private void deal(string str)
        {
            if (str.Substring(0, 1).Equals("@"))
            {
                username = str;
                upcombobox(username, 0, 0);
            }
            //else if (str.Substring(0, 1).Equals("%"))
            //{
            //    char[] a = new char[] { ':' };
            //    string[] a1 = str.Substring(1).Split(a);
            //    localip = a1[0];
            //    localport = int.Parse(a1[1]);
            //}
            else if (str.Substring(0, 1).Equals("$"))
                upcombobox("@" + str.Substring(1), 1, 0);
            else if (str.Substring(0, 1).Equals("&"))
            {
                uplistbox("服务端".Substring(0)+GetCurrentTime(), 0, 1);
                uplistbox(str.Substring(1), 0, 1);
            }
            else if (str.Substring(0, 1).Equals("#"))
            {
                int a = str.Substring(1).IndexOf("#");
                string ip = str.Substring(1).Remove(a);
                string message = str.Substring(a + 2);
                //try
                //{
                //    byte[] b = Encoding.UTF8.GetBytes("%" + ip + "%发送成功");
                //    client.Send(b, b.Length, SocketFlags.None);
                //}
                //catch
                //{
                //    close();
                //    return;
                //}
                uplistbox(ip, 0, 1);
                uplistbox(message, 0, 1);               
            }            
        }






        public void send(string str, int a, string st, string st1)
        {
            if (flag)
            {
                try
                {
                    if (a == 1)
                    {
                        str = "$" + str;//与服务端通讯
                    }
                    if (a == 0 && st == "TCP")
                    {
                        str = "#" + st1.Substring(1) + "#" + str; //与客户端通讯
                    }
                    byte[] messge = Encoding.UTF8.GetBytes(str);
                    client.Send(messge, messge.Length, SocketFlags.None);
                }
                catch
                {
                    close();
                }
            }
        }






        public void close()
        {
            if (flag)
            {
                if (client != null)
                {
                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        client.Close();
                    }

                }
                try
                {
                    upcombobox("", 2, 0);
                    uplistbox("与服务器断开，请重新连接", 0, 1);
                    flag = false;
                    Thclient.Abort();
                    Thclient = null;
                }
                catch { }

            }
        }



        public void setipport(string ipadress, int port, string username, string password)
        {
            this.ipadress = ipadress;
            this.port = port;
            this.username = username;
            this.password = password;
        }



        /// <summary>
        /// 获取当前系统时间的方法
        /// </summary>
        /// <returns>当前时间</returns>
        public DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        public bool getflag()
        {
            return flag;
        }
    }
}
