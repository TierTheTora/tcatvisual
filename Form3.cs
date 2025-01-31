using System;
using System.Windows.Forms;
using System.Media;

namespace TCAT_Visual
{
    public partial class Form3 : Form
    {
        public static Form1 form;
        public Form3(Form1 form1)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            form = form1;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            form.resetText();
            Form1.del = true;
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            SystemSounds.Hand.Play();
            if (!Convert.ToBoolean(Properties.Settings.Default["dark"]))
            {
                this.BackColor = System.Drawing.Color.White;
                this.ForeColor = System.Drawing.Color.Black;
                this.button1.BackColor = System.Drawing.Color.White;
                this.button1.ForeColor = System.Drawing.Color.Black;
                this.label1.ForeColor = System.Drawing.Color.Black;
                this.button2.BackColor = System.Drawing.Color.White;
                this.button2.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                this.BackColor = System.Drawing.Color.Black;
                this.ForeColor = System.Drawing.Color.White;
                this.button1.BackColor = System.Drawing.Color.Black;
                this.button1.ForeColor = System.Drawing.Color.White;
                this.label1.ForeColor = System.Drawing.Color.White;
                this.button2.BackColor = System.Drawing.Color.Black;
                this.button2.ForeColor = System.Drawing.Color.White;
            }
        }
    }
}
