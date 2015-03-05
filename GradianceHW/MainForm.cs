/*
 * This application does homework from http://newgradiance.com/ for you. 
 * I didn't want to do my homework, so I made something to do it for me. 
 * By the transitive property, doesn't that still mean I did my homework?
 * -Chris Caruso
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradianceHW
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Controls.Add(this.webBrowser);
        }

        private async void btn_Login_Click(object sender, EventArgs e)
        {
            using (var gradianceContext = new GradianceContext(this.webBrowser))
            {
                var classes = await gradianceContext.Login(tb_Username.Text, tb_Password.Text);

                if (classes != null)
                {
                    this.btn_Login.Enabled = false;

                    foreach (var c in classes)
                    {
                        this.lb_Classes.Items.Add(c);
                    }

                    this.groupBox2.Visible = true;
                }
                else
                {
                    MessageBox.Show("Login failed. Please try again.");
                }
            }


        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (lb_Classes.SelectedItem != null)
            {
                var clas = lb_Classes.SelectedItem as GradianceClass;

                var hws = await clas.GetHomeworks();

                foreach (var hw in hws)
                {
                    this.lb_Homeworks.Items.Add(hw);
                }

                this.groupBox3.Visible = true;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (lb_Homeworks.SelectedItem != null)
            {
                var hw = lb_Homeworks.SelectedItem as GradianceHomework;

                await hw.LoadPastSubmissions();

                await hw.DoHomework();
            }
        }
    }
}
