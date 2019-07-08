using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 服务端
{
    class ServerEvent
    {

        #region 事件委托
        public delegate void weituo(string str, int a, int b);
        public event weituo listboxaction;
        public event weituo comboboxaction;
        #endregion


        private IPEndPoint ipendpoint;
        private Socket server;
        private Thread Thlisten;
        private bool flag = false;
        private string ipaddress;
        private int port;
        public string username;//用户名
        private Dictionary<string, Socket> clientlist = new Dictionary<string, Socket>();

        public ServerEvent(string ipaddress, int port)
        {
            this.ipaddress = ipaddress;
            this.port = port;
        }

        ~ServerEvent()
        {
            foreach (var item in clientlist)
            {
                try
                {
                    item.Value.Shutdown(SocketShutdown.Both);
                }
                catch { }
            }
            try
            {
                if (server != null)
                {
                    try
                    {
                        server.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                        server.Close();
                    }
                }
                flag = false;
                Thlisten.Abort();
                Thlisten = null;
            }
            catch { }
        }

        public void Open()
        {
            if (flag == false)
            {
                try
                {
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ipendpoint = new IPEndPoint(IPAddress.Parse(ipaddress), port);
                    server.Bind(ipendpoint);
                    flag = true;
                }
                catch
                {
                    server.Close();
                    flag = false;
                    return;
                }
                server.Listen(100);
                Thlisten = new Thread(listen);
                Thlisten.IsBackground = true;
                Thlisten.Start();
            }
        }

        private void listen()
        {
            while (true)
            {
                try
                {
                    Socket client = server.Accept();
                    if (client != null)
                    {
                        Thread Thclient = new Thread(receive);
                        Thclient.IsBackground = true;
                        Thclient.Start(client);
                    }
                }
                catch
                {
                    break;
                }
            }
            MessageBox.Show("服务端异常断开");
            close();
        }


        private void receive(object o)
        {

            #region  客户端连接显示
            Socket client1 = o as Socket;
            //获取客户端连接地址
            string remoip = client1.RemoteEndPoint.ToString();

            byte[] user = new byte[1024];
            int userSum = client1.Receive(user, SocketFlags.None);
            username = Encoding.UTF8.GetString(user, 0, userSum);
            remoip = username;
            clientlist.Add(remoip, client1);
            comboboxaction(username, 0, 0);
            listboxaction(username + "在" + GetCurrentTime() + "连接上服务器", 0, 1);
            #endregion

            //byte[] by = Encoding.UTF8.GetBytes("%" + remoip);
            //client1.Send(by, by.Length, SocketFlags.None);
            Thread.Sleep(500);
            tongzhi(username, client1);


            #region 服务器接收客户端传来的数据
            while (true)
            {
                try
                {
                    byte[] redate = new byte[1024];
                    int l = client1.Receive(redate, SocketFlags.None);
                    if (l == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(redate, 0, l);
                    if (str != "" && str != string.Empty)
                    {
                        Thread Thread1 = new Thread(new ThreadStart(delegate ()
                        {
                            deal(str, remoip);
                        }));
                        Thread1.IsBackground = true;
                        Thread1.Start();
                    }
                }
                catch
                {
                    break;
                }
            }
            #endregion

            

            clientlist.Remove(remoip);
            try
            {
                client1.Close();
            }
            catch { }
            comboboxaction(remoip, 1, 0);
            listboxaction(remoip + "在"+GetCurrentTime()+"与服务器断开", 0, 1);


            #region  通知其他客户端我已下线
            foreach (var item in clientlist)
            {
                if (!item.Key.Equals(remoip))
                {
                    byte[] b = Encoding.UTF8.GetBytes("$" + remoip);
                    try
                    {
                        item.Value.Send(b, b.Length, SocketFlags.None);
                        Thread.Sleep(500);
                    }
                    catch
                    {
                        shifang(item.Key);
                    }
                }
            }
            #endregion
        }



        #region 客户端向服务端请求处理
        public void deal(string str, string ipnow)
        {
            //客户端与服务端通信
            if (str.Substring(0, 1).Equals("$"))
            {
                listboxaction(ipnow.Substring(0)+GetCurrentTime(), 0, 1);
                listboxaction(str.Substring(1), 0, 1);
                //if (clientlist.ContainsKey(ipnow))
                //{
                //    try
                //    {
                //        byte[] b = Encoding.UTF8.GetBytes("&发送成功");
                //        clientlist[ipnow].Send(b, b.Length, SocketFlags.None);
                //    }
                //    catch
                //    {
                //        shifang(ipnow);
                //    }
                //}
            }
            //客户端与客户端通信
            else if (str.Substring(0, 1).Equals("#"))
            {
                int a = str.Substring(1).IndexOf("#");
                string st = str.Substring(1).Remove(a);
                string message = str.Substring(a + 2);
                if (clientlist.ContainsKey(st))
                {
                    try
                    {
                        byte[] b = Encoding.UTF8.GetBytes("#" + ipnow + GetCurrentTime() + "#" + message);
                        clientlist[st].Send(b, b.Length, SocketFlags.None);
                    }
                    catch
                    {
                        shifang(st);
                        if (clientlist.ContainsKey(ipnow))
                        {
                            try
                            {
                                byte[] b1 = Encoding.UTF8.GetBytes("&对方已与服务器断开连接");
                                clientlist[ipnow].Send(b1, b1.Length, SocketFlags.None);
                            }
                            catch
                            {
                                shifang(ipnow);
                            }
                        }
                    }
                }
            }
            else if (str.Substring(0, 1).Equals("%"))
            {
                int a = str.Substring(1).IndexOf("%");
                string st = str.Substring(1).Remove(a);
                string message = str.Substring(a + 2);
                if (clientlist.ContainsKey(st))
                {
                    try
                    {
                        byte[] b = Encoding.UTF8.GetBytes("&" + message);
                        clientlist[st].Send(b, b.Length, SocketFlags.None);
                    }
                    catch
                    {
                        shifang(st);
                    }
                }
            }
        }
        #endregion


        public void close()
        {
            if (flag)
            {
                foreach (var item in clientlist)
                {
                    try
                    {
                        item.Value.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                }
                try
                {
                    if (server != null)
                    {
                        try
                        {
                            server.Shutdown(SocketShutdown.Both);
                        }
                        catch
                        {
                            server.Close();
                        }
                    }
                    flag = false;
                    Thlisten.Abort();
                    Thlisten = null;
                }
                catch
                {

                }
            }
        }

        private void shifang(string st)
        {
            if (clientlist.ContainsKey(st))
            {
                try
                {
                    clientlist[st].Close();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { clientlist.Remove(st); }
            }
        }

        private void tongzhi(string str, Socket clientNow)
        {
            byte[] st = Encoding.UTF8.GetBytes("@" + str);
            foreach (var item in clientlist)
            {
                if (!item.Value.Equals(clientNow))
                {
                    try
                    {
                        item.Value.Send(st, st.Length, SocketFlags.None);
                        Thread.Sleep(500);
                    }
                    catch { shifang(item.Key); }
                    byte[] st1 = Encoding.UTF8.GetBytes("@" + item.Key);
                    try
                    {
                        clientNow.Send(st1, st1.Length, SocketFlags.None);
                        Thread.Sleep(500);
                    }
                    catch { shifang(str); }
                }

            }
        }
        public void send(string str, int a, string ip)
        {
            if (flag)
            {

                #region 对单个客服发送
                if (a == 0)
                {
                    if (clientlist.ContainsKey(ip))
                    {
                        try
                        {
                            byte[] by = Encoding.UTF8.GetBytes("&" + str);
                            clientlist[ip].Send(by, by.Length, SocketFlags.None);
                        }
                        catch
                        {
                            try
                            {
                                clientlist[ip].Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally { clientlist.Remove(ip); }
                        }
                    }
                }
                #endregion


                #region 对所有客户发送
                else if (a == 1)
                {
                    string[] str1 = new string[clientlist.Count];
                    int i = 0;
                    foreach (var item in clientlist)
                    {
                        try
                        {
                            byte[] by = Encoding.UTF8.GetBytes("&" + str);
                            item.Value.Send(by, by.Length, SocketFlags.None);
                        }
                        catch
                        {
                            str1[i] = item.Key;
                            i++;
                        }
                    }
                    for (int j = 0; j < i; j++)
                    {

                        if (clientlist.ContainsKey(str1[j]))
                        {
                            try
                            {
                                clientlist[str1[j]].Close();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally { clientlist.Remove(str1[j]); }
                        }
                    }
                }
                #endregion
            }

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

        public void setIPport(string ipaddress, int port)
        {
            this.ipaddress = ipaddress;
            this.port = port;

        }
    }
}
