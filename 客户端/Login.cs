using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 客户端
{
    public partial class Login : Form
    {
        
        public Login()
        {
            InitializeComponent();
            skinEngine1.SkinFile = Application.StartupPath + @"\皮肤\Wave\WaveColor1.ssk";
        }


        private void button1_CLick(object sender, EventArgs e)
        {
            while (true)
            {
                if (userTxt.Text == null && userTxt.Text == "")
                {
                    return;
                }
                else
                {
                    Client c = new Client(userTxt.Text, pwdTxt.Text);
                    c.Show();
                    this.Hide();
                    return;
                }
            }
        }
    }
}
