using System;
using System.Windows.Forms;

namespace TCAT_Visual
{
    public partial class Form2 : Form
    {
        public Form1 form;
        public Form2(Form1 form1)
        {
            form = form1;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(Properties.Settings.Default["dark"]))
            {
                this.BackColor = System.Drawing.Color.Black;
                this.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                this.BackColor = System.Drawing.Color.White;
                this.ForeColor = System.Drawing.Color.Black;
            }
        }

        public void ChangeColour()
        {
            if (!Convert.ToBoolean(Properties.Settings.Default["dark"]))
            {
                this.BackColor = System.Drawing.Color.Black;
                this.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                this.BackColor = System.Drawing.Color.White;
                this.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void PicBoxClick(object sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is string emoji)
                form.AddEmoji(emoji);
        }
    }
}
