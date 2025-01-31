using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Resources;

namespace TCAT_Visual
{

    public partial class Form1 : Form
    {
        private static bool autorem = false;
        public static bool del = false;
        private static Form2 emsel;
        private static System.Globalization.CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            InitializeComponent();
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {

            const int WM_SIZE = 0x0005;
            const int SIZE_MINIMIZED = 1;

            if (m.Msg == WM_SIZE)
            {
                int windowState = m.WParam.ToInt32();

                if (windowState == SIZE_MINIMIZED)
                    if (!(emsel is null))
                        emsel.Visible = false;
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            bool isDarkMode = Convert.ToBoolean(Properties.Settings.Default["dark"] ?? false);
            this.checkBox2.Checked = isDarkMode;
            ApplyDarkMode(isDarkMode);
            ToggleRemove(Convert.ToBoolean(Properties.Settings.Default["remt"] ?? "False"));
            this.openFileDialog1.FileName = "";
            GC.Collect();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://tier.game-cave.net/tcat/manual/#visual",
                UseShellExecute = true
            });
            GC.Collect();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            this.label1.Text = Convert.ToString(this.richTextBox1.Text.Length);
            if ((this.richTextBox1.Text.Length >= 2001 && !this.checkBox3.Checked) || (this.richTextBox1.Text.Length >= 4097 && this.checkBox3.Checked))
                this.label1.ForeColor = System.Drawing.Color.Red;
            else
            {
                if (Convert.ToBoolean(Properties.Settings.Default["dark"]))
                    this.label1.ForeColor = System.Drawing.Color.White;
                else
                    this.label1.ForeColor = System.Drawing.Color.Black;
            }
            //string markdownText = this.richTextBox1.Text;
            //string html = Markdown.ToHtml(markdownText);
            DiscordMarkdownParser parser = new DiscordMarkdownParser();
            string html = parser.Parse(this.richTextBox1.Text);

            this.webBrowser1.DocumentText = html;


            GC.Collect();
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.richTextBox1.Text) && (this.radioButton1.Checked || this.radioButton2.Checked || this.radioButton3.Checked || this.radioButton4.Checked))
            {
                string message = this.richTextBox1.Text.Replace("\n", "\\n").Replace("\"", "\\\"");
                string webhook = "";

                if ((this.radioButton1.Checked || this.radioButton2.Checked) && !string.IsNullOrEmpty(this.richTextBox1.Text))
                {
                    webhook = Decode(S()[0]);
                    if (this.radioButton1.Checked)
                        message = "<@&1238536214022328330> " + message;
                    else if (this.radioButton2.Checked)
                        message = "<@&1115342914000781382> " + message;
                }

                else if (this.radioButton3.Checked && !string.IsNullOrEmpty(this.richTextBox1.Text))
                    webhook = Decode(S()[1]);
                else if (this.radioButton4.Checked && !string.IsNullOrEmpty(this.richTextBox1.Text))
                    webhook = Decode(S()[2]);
                string json = $"{{\"content\": \"{message}\"}}";

                string colour;

                if (!(this.colorDialog1.Color == Color.Empty)) {
                    int r = this.colorDialog1.Color.R;
                    int g = this.colorDialog1.Color.G;
                    int b = this.colorDialog1.Color.B;

                    int dC = (r * 65536) + (g * 256) + b;
                    colour = dC.ToString();
                }
                else
                    colour = "0";

                string embed = $@"
                {{
                    ""embeds"": [
                        {{
                            ""description"": ""{message}"",
                            ""color"": {colour}
                        }}
                    ]
                }}";

                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpContent content;
                        if (this.checkBox3.Checked)
                            content = new StringContent(embed, Encoding.UTF8, "application/json");
                        else
                            content = new StringContent(json, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.PostAsync(webhook, content);

                        if (!response.IsSuccessStatusCode)
                            MessageBox.Show($"Failed to send message. Status code: {response.StatusCode}");
                        else
                            if (autorem)
                            this.richTextBox1.Text = "";


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error sending message: {ex.Message}");
                    }
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool r = this.checkBox1.Checked;
            ToggleRemove(r);
            Properties.Settings.Default["remt"] = r;
            Properties.Settings.Default.Save();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            bool isDarkMode = this.checkBox2.Checked;
            ApplyDarkMode(isDarkMode);
            Properties.Settings.Default["dark"] = isDarkMode;
            Properties.Settings.Default.Save();
            this.label1.Text = Convert.ToString(this.richTextBox1.Text.Length);
            if ((this.richTextBox1.Text.Length >= 2001 && !this.checkBox3.Checked) || (this.richTextBox1.Text.Length >= 4097 && this.checkBox3.Checked))
                this.label1.ForeColor = System.Drawing.Color.Red;
            else
            {
                if (Convert.ToBoolean(Properties.Settings.Default["dark"]))
                    this.label1.ForeColor = System.Drawing.Color.White;
                else
                    this.label1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void ApplyDarkMode(bool enable)
        {
            if (enable)
            {
                this.BackColor = System.Drawing.Color.Black;
                this.ForeColor = System.Drawing.Color.White;
                this.richTextBox1.BackColor = System.Drawing.Color.Black;
                this.richTextBox1.ForeColor = System.Drawing.Color.White;
                this.button1.BackColor = System.Drawing.Color.Black;
                this.button1.ForeColor = System.Drawing.Color.White;
                this.button2.BackColor = System.Drawing.Color.Black;
                this.button2.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                this.BackColor = System.Drawing.Color.White;
                this.ForeColor = System.Drawing.Color.Black;
                this.richTextBox1.BackColor = System.Drawing.Color.White;
                this.richTextBox1.ForeColor = System.Drawing.Color.Black;
                this.button1.BackColor = System.Drawing.Color.White;
                this.button1.ForeColor = System.Drawing.Color.Black;
                this.button2.BackColor = System.Drawing.Color.White;
                this.button2.ForeColor = System.Drawing.Color.Black;
            }
            if (!(emsel is null))
                emsel.ChangeColour();
        }
        private void ToggleRemove(bool rem)
        {
            this.checkBox1.Checked = rem;
            autorem = rem;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E)
            {
                e.SuppressKeyPress = true;
                if (emsel is null) {
                    Form2 form2 = new Form2(this);
                    emsel = form2;
                    form2.Show();
                    emsel.Disposed += (s, q) => 
                    { 
                        emsel = null;
                        GC.Collect();
                    };
                    emsel.VisibleChanged += (s, q) => 
                    {
                        if (!emsel.Visible)
                        {
                            emsel.Dispose();
                            emsel = null;
                        }
                    };
                }
            }
            else if (e.Control && e.Shift && e.KeyCode == Keys.D)
            {
                new Form3(this).ShowDialog();
            }
            else if (e.Control && e.KeyCode == Keys.O)
                this.openFileDialog1.ShowDialog();
        }

        public void AddEmoji(string emoji)
        {
            this.richTextBox1.Text += emoji;
        }

        private string[] S()
        {
            string resourceName = "TCAT_Visual.webh.txt";

            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string fileContent = reader.ReadToEnd();

                        string[] lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                        return lines;
                    }
                }
                else
                    return new string[0];
            }
        }

        private string Decode(string en)
        {
            en = en.Substring(2);
            string r = new string(en.Reverse().ToArray());
            byte[] bytes = Convert.FromBase64String(r);
            GC.Collect();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.colorDialog1.ShowDialog();
            if (!(this.colorDialog1.Color == Color.Empty))
            {
                this.panel1.BackColor = this.colorDialog1.Color;
                this.checkBox3.Checked = true;
            }
            else
                this.checkBox3.Checked = false;
        }

        public void resetText()
        {
            this.richTextBox1.Text = "";
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            this.label1.Text = Convert.ToString(this.richTextBox1.Text.Length);
            if ((this.richTextBox1.Text.Length >= 2001 && !this.checkBox3.Checked) || (this.richTextBox1.Text.Length >= 4097 && this.checkBox3.Checked))
                this.label1.ForeColor = System.Drawing.Color.Red;
            else
            {
                if (Convert.ToBoolean(Properties.Settings.Default["dark"]))
                    this.label1.ForeColor = System.Drawing.Color.White;
                else
                    this.label1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.openFileDialog1.FileName))
            {
                using (StreamReader reader = new StreamReader(this.openFileDialog1.FileName))
                {
                    string content = reader.ReadToEnd();
                    Form3 form3 = new Form3(this);
                    form3.ShowDialog();
                    if (del)
                        this.richTextBox1.Text = content;
                    del = false;
                }
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme.Contains(":"))
                e.Cancel = true;
        }

        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TCAT_Visual.Form2", typeof(Form2).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        public class DiscordMarkdownParser
        {
            public static string ConvertImageToBase64(Bitmap image)
            {
                if (image != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
                else
                    return "";
            }
            public string Parse(string markdown)
            {
                markdown = Regex.Replace(markdown, @"^> \s*([-\*])\s+(.*)$", "<blockquote><li>$2</li></blockquote>", RegexOptions.Multiline);

                string[] objs = {
                    "1115343714001375252", // 19
                    "1115343780850171934", 
                    "1115343896982065254", 
                    "1115343852425973810", 
                    "1115875918829846610", // 5 
                    "1115352448748490752", 
                    "1115345615963033721", 
                    "1115344023851368610", // 8
                    "1115876408871370843", 
                    "1137648137335476254", // 10 
                    "1137647936914862150", // 11 C
                    "1128378792440758292", //
                    "1125069735294402661", 
                    "1118590842010075206", // 4
                    "1117180762551242865", 
                    "1117180689058635836", //  
                    "1117180601066332310", // d
                    "1117180522590908426", // 8
                    "1115343599077425273", // 27Y
                    "1115343659404115978", // 28W
                    "1151030241888845865", // 29MB
                    "1147594291045212210", 
                    "1147541146197835797", // CT
                    "1144305769622274130", 
                    "1144302997891985548", // d
                    "1143440742312841347", // d
                    "1140218766349828197", // d
                    "1115343538541051955", // F
                    "1320434924825608253", // F
                };
                /*
                 matching index of objs will be the n value of obj
                 */

                string pattern = @"<:(.*?):\d+\s*(.*)>";
                var matches = Regex.Matches(markdown, pattern);
                int index = 1;

                foreach (Match match in matches)
                {
                    string id = match.Groups[0].Value;
                    id = Regex.Replace(id, @"\D", "");
                    id = id.Length > 19 ? id.Substring(id.Length - 19) : id;
                    index += Array.FindIndex(objs, item => item.StartsWith(id));

                    string n = index.ToString();
                    object obj = ResourceManager.GetObject($"pictureBox{n}.Image", resourceCulture);
                    Bitmap i = (Bitmap)obj;
                    string img = ConvertImageToBase64(i);
                    markdown = Regex.Replace(markdown, @"<:(.*?):\d+\s*(.*)>", $"<img src=\"data:image/png;base64,{img}\" style=\"width: 25px;\">", RegexOptions.Multiline);
                }

                markdown = Regex.Replace(markdown, @"^\s*([-\*])\s+(.*)$", "<li>$2</li>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"(<li>.*?</li>)", "<ul>$0</ul>", RegexOptions.Singleline);
                markdown = Regex.Replace(markdown, @"^-# (.*?)$", "<h6>$1</h6>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"\|\|(.*?)\|\|", "<span style=\"background-color: #444444; color: #eeeeee;\">$1</span>");
                markdown = Regex.Replace(markdown, @"~~(.*?)~~", "<strike>$1</strike>");
                markdown = Regex.Replace(markdown, @"\*\*\*(.*?)\*\*\*", "<strong><i>$1</i></strong>");
                markdown = Regex.Replace(markdown, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
                markdown = Regex.Replace(markdown, @"\*(.*?)\*", "<i>$1</i>");
                markdown = Regex.Replace(markdown, @"__(.*?)__", "<u>$1</u>");
                markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)", "<a href='$2'>$1</a>");
                markdown = Regex.Replace(markdown, @"```([\s\S]*?)```", "<div style=\"background-color: #444444; color: #eeeeee;\"><code>$1</code></div>", RegexOptions.Singleline);
                markdown = Regex.Replace(markdown, @"`(.*?)`", "<code style=\"background-color: #444444; color: #eeeeee;\">$1</code>", RegexOptions.Singleline);
                markdown = Regex.Replace(markdown, @"_(.*?)_", "<i>$1</i>");
                markdown = Regex.Replace(markdown, @"^# (.*?)$", "<h1>$1</h1>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"^## (.*?)$", "<h2>$1</h2>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"^### (.*?)$", "<h3>$1</h3>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"^> \s*(.+?)(?=\n|\z)", "<blockquote>$1</blockquote>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"\n\s{2,}[-\*]\s*(.*?)(?=\n|$)", "<ul><li>$1</li></ul>", RegexOptions.Multiline);
                markdown = Regex.Replace(markdown, @"^\d+\. \s*(.*)$", "<ol start=\"$0\"><li>$1</li></ol>", RegexOptions.Multiline);

                markdown = Regex.Replace(markdown, @"([^\n]+)(?=\n?(?![ \t]*[-\*]))", "<p>$1</p>", RegexOptions.Multiline);

                return markdown;
            }
        }
    }
}

/*
        ⣿⣿⣿⣿⣿⣿⣯⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣝⡻⣿⣿⠏⢢⣿⢿⠋⢁⣤⣱⣾⡾⡟⣏⣶⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
        ⣿⣿⣿⣿⣯⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿⣿⣿⣿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣜⢻⣎⠉⣁⣢⣼⠿⢛⣫⣽⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣟⣿⣿
        ⣿⣿⣿⣟⣿⣿⡿⣷⣿⣿⣾⣿⣽⣾⣿⣿⡿⣯⣿⣾⣿⣿⡿⣷⣿⣯⣿⣾⣿⢿⣷⣿⡿⣿⣿⣾⡐⢨⣏⢱⣴⣾⣿⣿⣿⣿⣿⣿⣿⢿⣟⣿⣿⣽⣿⢿⣾⣿⣷⣿⣿⣿⣻⣿⣿
        ⣿⣿⣷⣿⡿⣷⣿⡿⣿⣾⣿⣽⡿⣯⣿⣾⣿⢿⣿⣽⣿⢾⣿⢿⣯⣿⣿⣽⡿⢛⣩⡴⢷⠒⣌⠐⠃⠑⠂⠻⣿⣿⣿⣽⣿⣾⣟⣷⣿⡿⣿⣻⣽⣿⣾⣿⢿⣳⣿⣿⣾⣟⣿⣷⣿
        ⣿⣿⣼⡿⣿⣿⣻⣿⡿⣧⣿⣟⣿⣿⣧⣿⣼⣿⣤⣧⣼⣧⣼⣧⣼⠿⠟⡛⡘⠛⠤⡘⠄⠃⡀⡘⠣⢤⠤⣀⠀⠘⠛⠿⣧⣿⣼⣻⣟⣿⡿⣿⢿⣼⣧⣿⣿⡿⣿⣼⣟⣿⣟⣿⣼
        ⣿⣿⣽⡿⣿⣽⡿⣽⡿⣟⣷⣿⣻⣾⢯⣷⣿⣳⣿⣯⠷⠟⣋⠵⣠⢋⡜⣁⠎⣍⠲⣈⣡⢏⡤⢳⠴⡈⠆⡤⢒⡰⡈⠴⢠⠤⣉⠙⠛⠷⣿⣿⣻⣯⢿⡾⣷⢿⡿⣽⣾⢿⣽⣯⣿
        ⣿⣟⣾⡿⣿⣳⣿⣟⣿⢿⣻⣾⢯⣟⣿⡷⠿⠛⠋⠁⠈⡐⠀⢊⠩⢩⠒⡡⠞⡬⠓⠌⡁⠋⠀⠁⠀⠀⠀⠀⠀⡀⠈⠈⡁⠐⠀⠉⠒⠠⠄⠛⠿⣿⣟⣿⡽⣟⣿⢯⣟⣯⣿⢾⣽
        ⣿⢯⣷⢿⣻⣽⣾⢯⣿⣻⢯⣿⡿⣛⠡⡔⣤⢒⡌⢦⣡⡔⡡⢆⡑⠢⠘⠴⡩⢄⢇⡲⠱⠊⠜⠃⠋⠈⠑⠈⠁⠉⢈⠁⠈⠑⠀⠄⠄⠀⠀⡀⠀⠉⢿⣿⡽⣿⣽⣻⢯⣿⢾⡿⣽
        ⣿⣻⣽⢿⣯⣟⣾⢿⣷⠟⠛⠁⠀⠀⠃⠌⡀⠃⢜⢣⠳⣞⡱⢊⡐⢡⢫⠜⡡⠋⠀⢐⣀⣤⣤⣶⣶⣿⣾⣿⣿⣿⣿⣾⣿⡋⠝⠘⠌⠋⠈⠤⠀⢄⠀⠹⣟⡷⣯⣟⡿⣽⣯⢿⣻
        ⣿⣯⣟⣯⣿⡹⣭⣳⣮⣾⣿⣯⣿⣷⣦⣤⣀⠂⡈⠤⠙⣤⠣⢇⠲⢃⠊⠀⣁⣀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⢿⣿⣿⣷⡹⢆⢣⣀⠡⣈⢀⠡⢀⠒⡀⠈⢥⣛⡾⣽⣻⢾⣟⣿
        ⣿⣳⢯⣿⣾⣷⣿⣾⣽⡿⢿⣿⣿⣿⠟⠿⠿⣿⣷⣾⣷⣾⣽⣮⣷⣤⣴⣿⣿⣿⣿⣿⠿⠛⠋⠉⠉⢩⣿⣿⣿⣿⣶⣯⣷⣽⡮⠖⠈⠃⠑⠈⠒⠃⠂⠀⠢⠄⠉⠙⠧⣿⣻⠾⣽
        ⠯⠟⠉⠉⢁⣀⣤⣀⠤⣐⡤⣀⠤⢭⠿⠶⠶⠿⠿⠿⢛⠛⠛⠛⠙⠛⠛⠛⠻⠿⠿⠿⠶⠶⠶⠶⠶⠾⠿⠟⠛⠉⠀⠀⠀⢀⢀⡀⡘⠂⣁⠑⢂⠰⢄⠢⡄⣄⢢⢈⢀⢀⠀⠉⠉
        ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⣶⣃⣌⣤⣭⣾⣿⣷⠆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⢀⠒⡐⢂⡡⢄⡴⣐⡌⡙⢮⣙⢎⡷⣜⢮⡹⢏⡯⣝⡚⢦⢡
        ⠀⠀⢀⡞⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠉⠉⠉⠉⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⠈⠀⠒⡈⠄⢂⡑⢚⣿⡷⡈⠦⡙⢎⠲⣍⠶⣙⢮⡵⢎⡵⣊⠦
        ⡀⠔⠀⠣⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⠠⠄⢂⠁⠂⡔⢈⠂⡔⡬⠛⠔⡡⢎⠑⣎⠱⣌⠫⣝⢮⡵⢫⡖⢭⢒
        ⡅⠠⠈⠀⠙⠠⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠠⠀⠄⠠⢀⠂⠄⠡⠐⠠⢈⢡⣰⡴⠞⡩⢄⠡⠌⡰⢡⠊⡔⢫⢄⡛⣬⢳⣝⡳⣜⠣⢎
        ⣷⣤⠁⠀⠀⠀⠀⠐⠠⢀⠀⡀⠀⢀⠀⢀⠀⡀⡀⠰⣀⠐⡀⠀⠀⢀⠠⠀⠐⢀⠂⡁⠂⠌⡐⣁⣄⣢⣬⣴⠷⣛⢏⢫⠱⣈⠓⡴⢈⡔⢢⠑⣣⢚⡜⣧⢎⡽⣎⢳⠎⡵⣈⠳⣈
        ⣿⣿⣷⣮⣀⡀⠈⠀⠀⠀⠐⢁⠚⠤⣌⢂⡒⣐⢩⠓⣬⠲⣥⣦⣥⣦⣶⣬⣶⣾⣶⢿⡿⣟⢻⠛⣭⢋⠵⣈⠞⡰⢊⠥⢣⡑⢎⡰⢣⡘⢤⢫⡴⢫⡞⣱⢫⠞⣡⢋⠘⣔⣢⢧⣽
        ⣿⣾⣻⢿⣿⣿⣷⣤⣄⠀⠐⠠⢠⠂⢄⠣⡙⡌⢏⠽⣩⢟⡳⢭⢏⠷⣙⠮⡱⢎⠦⡓⠼⣈⠧⡙⡴⣉⠶⣡⠚⡥⢋⡼⢡⠞⣌⡓⠧⡜⣭⡚⣭⢣⠓⡐⢃⡬⣄⡨⣟⣾⣽⣻⢿
        ⣿⣳⡿⣟⣷⡿⣽⡿⣿⣷⣦⡄⣀⡈⠀⠃⡑⠎⣍⠲⣅⠮⣑⢫⡜⡜⣡⢞⡱⢊⡵⣉⢎⡱⣊⢕⡲⢡⠞⡤⢛⠴⣉⠶⣭⡚⣥⢋⠷⡉⠦⢑⡠⢡⣴⣰⢮⡿⣽⣟⣿⣽⡾⣟⣿
        ⣿⣿⣽⣿⣯⣿⣿⣽⣿⣽⣿⣿⣿⣿⣿⣦⣤⠀⠈⠑⠘⢲⢩⡖⣼⢱⢣⡞⢸⢱⢢⡍⣮⢱⡎⠚⣴⢫⡜⣼⢩⡎⣽⢳⣧⣿⠐⠋⢲⢱⣶⣯⡟⣿⣶⣿⡟⣿⣽⣾⡟⣾⡟⣿⣽
        ⣿⣽⡿⣽⣾⣟⣷⣿⢯⣷⣿⣞⣯⣿⣻⣿⣿⣿⣦⣵⣂⠡⢣⡜⣥⢳⡞⣼⣣⣏⣮⢗⣮⢳⣎⢷⡱⢎⡷⣣⡟⣼⣣⡿⡜⢢⣙⣾⣿⣿⣻⣾⣟⣯⣿⣞⣿⣽⣟⣷⢿⡿⣽⣟⣿
        ⣿⣯⣿⣟⣿⣾⢿⣽⡿⣟⣾⡿⣽⣷⣿⣳⣿⢯⣿⡿⣿⣿⣦⣌⡐⢣⢛⢳⡛⢾⣽⡾⣭⠷⣾⣭⢷⡻⣜⣷⢻⣳⠹⣰⣽⣿⣿⡿⣟⣾⣟⣷⡿⣯⣷⣿⣻⣾⣟⣯⣿⢿⣟⣯⣿
        ⣿⣿⢾⣟⣿⣾⣿⣻⣿⢿⣻⣽⡿⣷⣟⣯⣿⣿⣯⣿⣟⣿⡿⣿⣿⣶⡈⢭⡘⢇⣮⠱⢩⠛⠴⡏⣛⠷⢎⡖⢫⢴⣿⣿⣿⢿⣽⣿⡿⣯⣿⣯⣿⣿⣽⣾⣿⣳⣿⣟⣯⣿⣿⣻⣽
        ⣿⣿⡿⣿⣿⣾⣿⣽⡿⣿⣟⣿⣿⣟⣿⣻⣽⣾⡿⣽⣿⣯⣿⣟⣿⣿⣿⠠⡉⢆⠰⠉⡊⡙⠠⠓⡌⡡⠛⣌⠢⣿⣿⣿⢾⡿⣿⣾⢿⡿⣷⡿⣷⡿⣯⣿⣾⣿⣽⣾⡿⣟⣾⡿⣿
*/