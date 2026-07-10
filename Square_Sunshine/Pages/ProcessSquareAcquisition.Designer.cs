using Sunny.UI;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    partial class ProcessSquareAcquisition
    {
        [DllImport("user32", CharSet = CharSet.Unicode)] 
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo); 
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;      
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;

        private delegate void UpdateViewDele(int nType);


        private int _nSplitViewWidth = 10;
        private int _nRaidusViewHeight = 900;
        private int _nRadiusViewWidth = 600;
        private int _nViewBeginY = 50;
        private int _nRadiusViewPictureboxWidth = 150;
        private int _nRadiusViewPictureboxHeight = 900;
        private int _nNoCollapsedRadiusViewHeight = 900;
        private int _nNoCollapsedRadiusViewWidth = 700;
        private int _nCollapsedRadiusViewWidth = 50;
        private UpdateViewDele refreshViewFunc = null;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        
        private void InitializeComponent()
        {
            this.labelLTLength = new System.Windows.Forms.Label();
            this.labelRTLength = new System.Windows.Forms.Label();
            this.labelTDLength = new System.Windows.Forms.Label();
            this.textBoxLTLength = new System.Windows.Forms.TextBox();
            this.textBoxTDLength = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.uiButton2 = new Sunny.UI.UIButton();
            this.uiTextBox8 = new Sunny.UI.UITextBox();
            this.test = new Sunny.UI.UIButton();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.uiIntegerUpDown1 = new Sunny.UI.UIIntegerUpDown();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.uiButton1 = new Sunny.UI.UIButton();
            this.uiTextBox33 = new Sunny.UI.UITextBox();
            this.uiComboBox1 = new Sunny.UI.UIComboBox();
            this.uiLabel36 = new Sunny.UI.UILabel();
            this.uiLabel35 = new Sunny.UI.UILabel();
            this.uiAvatar1 = new Sunny.UI.UIAvatar();
            this.SuspendLayout();
            // 
            // labelLTLength
            // 
            this.labelLTLength.AutoSize = true;
            this.labelLTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLTLength.Location = new System.Drawing.Point(12, 214);
            this.labelLTLength.Name = "labelLTLength";
            this.labelLTLength.Size = new System.Drawing.Size(162, 21);
            this.labelLTLength.TabIndex = 31;
            this.labelLTLength.Text = "Edge_A（Actual）：";
            // 
            // labelRTLength
            // 
            this.labelRTLength.AutoSize = true;
            this.labelRTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRTLength.Location = new System.Drawing.Point(12, 281);
            this.labelRTLength.Name = "labelRTLength";
            this.labelRTLength.Size = new System.Drawing.Size(161, 21);
            this.labelRTLength.TabIndex = 32;
            this.labelRTLength.Text = "Edge_B（Actual）：";
            // 
            // labelTDLength
            // 
            this.labelTDLength.AutoSize = true;
            this.labelTDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTDLength.Location = new System.Drawing.Point(15, 364);
            this.labelTDLength.Name = "labelTDLength";
            this.labelTDLength.Size = new System.Drawing.Size(230, 21);
            this.labelTDLength.TabIndex = 35;
            this.labelTDLength.Text = "Diagonal length 1（Actual）:";
            // 
            // textBoxLTLength
            // 
            this.textBoxLTLength.Location = new System.Drawing.Point(274, 211);
            this.textBoxLTLength.Name = "textBoxLTLength";
            this.textBoxLTLength.Size = new System.Drawing.Size(116, 29);
            this.textBoxLTLength.TabIndex = 55;
            // 
            // textBoxTDLength
            // 
            this.textBoxTDLength.Location = new System.Drawing.Point(274, 356);
            this.textBoxTDLength.Name = "textBoxTDLength";
            this.textBoxTDLength.Size = new System.Drawing.Size(116, 29);
            this.textBoxTDLength.TabIndex = 59;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(12, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 21);
            this.label1.TabIndex = 61;
            this.label1.Text = "Mode code：";
            // 
            // uiLabel6
            // 
            this.uiLabel6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel6.Location = new System.Drawing.Point(10, 9);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(111, 23);
            this.uiLabel6.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel6.TabIndex = 66;
            this.uiLabel6.Text = "Contents：";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiButton2
            // 
            this.uiButton2.BackColor = System.Drawing.Color.White;
            this.uiButton2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton2.Enabled = false;
            this.uiButton2.FillColor = System.Drawing.Color.LightSteelBlue;
            this.uiButton2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton2.ForeColor = System.Drawing.Color.Black;
            this.uiButton2.Location = new System.Drawing.Point(739, 7);
            this.uiButton2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton2.Name = "uiButton2";
            this.uiButton2.Size = new System.Drawing.Size(168, 54);
            this.uiButton2.Style = Sunny.UI.UIStyle.Custom;
            this.uiButton2.StyleCustomMode = true;
            this.uiButton2.TabIndex = 65;
            this.uiButton2.Text = "Start calibration";
            this.uiButton2.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton2.Click += new System.EventHandler(this.uiButton2_Click);
            // 
            // uiTextBox8
            // 
            this.uiTextBox8.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.uiTextBox8.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox8.Location = new System.Drawing.Point(142, 9);
            this.uiTextBox8.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox8.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox8.Name = "uiTextBox8";
            this.uiTextBox8.Padding = new System.Windows.Forms.Padding(5);
            this.uiTextBox8.ShowText = false;
            this.uiTextBox8.Size = new System.Drawing.Size(487, 36);
            this.uiTextBox8.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox8.TabIndex = 64;
            this.uiTextBox8.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiTextBox8.Watermark = "";
            // 
            // test
            // 
            this.test.BackColor = System.Drawing.Color.Transparent;
            this.test.Cursor = System.Windows.Forms.Cursors.Hand;
            this.test.FillColor = System.Drawing.Color.DimGray;
            this.test.FillColor2 = System.Drawing.Color.Silver;
            this.test.FillDisableColor = System.Drawing.Color.Silver;
            this.test.FillHoverColor = System.Drawing.Color.Silver;
            this.test.FillPressColor = System.Drawing.Color.Transparent;
            this.test.FillSelectedColor = System.Drawing.Color.Transparent;
            this.test.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.test.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.test.ForeHoverColor = System.Drawing.Color.Transparent;
            this.test.ForePressColor = System.Drawing.Color.Transparent;
            this.test.ForeSelectedColor = System.Drawing.Color.Transparent;
            this.test.Location = new System.Drawing.Point(636, 13);
            this.test.MinimumSize = new System.Drawing.Size(1, 1);
            this.test.Name = "test";
            this.test.RectColor = System.Drawing.Color.Transparent;
            this.test.RectDisableColor = System.Drawing.Color.Transparent;
            this.test.RectHoverColor = System.Drawing.Color.Transparent;
            this.test.RectPressColor = System.Drawing.Color.Transparent;
            this.test.RectSelectedColor = System.Drawing.Color.Transparent;
            this.test.Size = new System.Drawing.Size(45, 32);
            this.test.Style = Sunny.UI.UIStyle.Custom;
            this.test.TabIndex = 67;
            this.test.Text = "...";
            this.test.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.test.Click += new System.EventHandler(this.test_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(274, 281);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(116, 29);
            this.textBox1.TabIndex = 68;
            // 
            // uiIntegerUpDown1
            // 
            this.uiIntegerUpDown1.Enabled = false;
            this.uiIntegerUpDown1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiIntegerUpDown1.Location = new System.Drawing.Point(188, 85);
            this.uiIntegerUpDown1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiIntegerUpDown1.MinimumSize = new System.Drawing.Size(100, 0);
            this.uiIntegerUpDown1.Name = "uiIntegerUpDown1";
            this.uiIntegerUpDown1.RectColor = System.Drawing.Color.LightSteelBlue;
            this.uiIntegerUpDown1.ShowText = false;
            this.uiIntegerUpDown1.Size = new System.Drawing.Size(116, 50);
            this.uiIntegerUpDown1.Style = Sunny.UI.UIStyle.Custom;
            this.uiIntegerUpDown1.StyleCustomMode = true;
            this.uiIntegerUpDown1.TabIndex = 69;
            this.uiIntegerUpDown1.Text = "uiIntegerUpDown1";
            this.uiIntegerUpDown1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiIntegerUpDown1.ValueChanged += new Sunny.UI.UIIntegerUpDown.OnValueChanged(this.uiIntegerUpDown1_ValueChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(774, 281);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(116, 29);
            this.textBox2.TabIndex = 76;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(774, 356);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(116, 29);
            this.textBox3.TabIndex = 75;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(774, 211);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(116, 29);
            this.textBox4.TabIndex = 74;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label2.Location = new System.Drawing.Point(522, 364);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(230, 21);
            this.label2.TabIndex = 73;
            this.label2.Text = "Diagonal length 2（Actual）:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label3.Location = new System.Drawing.Point(519, 281);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 21);
            this.label3.TabIndex = 72;
            this.label3.Text = "Edge_D（Actual）：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label4.Location = new System.Drawing.Point(519, 214);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 21);
            this.label4.TabIndex = 71;
            this.label4.Text = "Edge_C（Actual）：";
            // 
            // uiButton1
            // 
            this.uiButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton1.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.uiButton1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Location = new System.Drawing.Point(1034, 80);
            this.uiButton1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton1.Name = "uiButton1";
            this.uiButton1.Size = new System.Drawing.Size(59, 28);
            this.uiButton1.Style = Sunny.UI.UIStyle.Custom;
            this.uiButton1.StyleCustomMode = true;
            this.uiButton1.TabIndex = 89;
            this.uiButton1.Text = "Login";
            this.uiButton1.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Click += new System.EventHandler(this.uiButton1_Click);
            // 
            // uiTextBox33
            // 
            this.uiTextBox33.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.uiTextBox33.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.uiTextBox33.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTextBox33.Location = new System.Drawing.Point(1238, 77);
            this.uiTextBox33.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTextBox33.MinimumSize = new System.Drawing.Size(1, 16);
            this.uiTextBox33.Name = "uiTextBox33";
            this.uiTextBox33.Padding = new System.Windows.Forms.Padding(5);
            this.uiTextBox33.ShowText = false;
            this.uiTextBox33.Size = new System.Drawing.Size(154, 23);
            this.uiTextBox33.Style = Sunny.UI.UIStyle.Custom;
            this.uiTextBox33.TabIndex = 88;
            this.uiTextBox33.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiTextBox33.Watermark = "";
            // 
            // uiComboBox1
            // 
            this.uiComboBox1.BackColor = System.Drawing.Color.Gray;
            this.uiComboBox1.DataSource = null;
            this.uiComboBox1.FillColor = System.Drawing.Color.White;
            this.uiComboBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiComboBox1.Items.AddRange(new object[] {
            "admin",
            "Operator"});
            this.uiComboBox1.Location = new System.Drawing.Point(1238, 15);
            this.uiComboBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiComboBox1.MinimumSize = new System.Drawing.Size(63, 0);
            this.uiComboBox1.Name = "uiComboBox1";
            this.uiComboBox1.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.uiComboBox1.Size = new System.Drawing.Size(154, 22);
            this.uiComboBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiComboBox1.TabIndex = 87;
            this.uiComboBox1.Text = "admin";
            this.uiComboBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiComboBox1.Watermark = "";
            // 
            // uiLabel36
            // 
            this.uiLabel36.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel36.Location = new System.Drawing.Point(1099, 84);
            this.uiLabel36.Name = "uiLabel36";
            this.uiLabel36.Size = new System.Drawing.Size(121, 19);
            this.uiLabel36.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel36.TabIndex = 86;
            this.uiLabel36.Text = "Password：";
            this.uiLabel36.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel35
            // 
            this.uiLabel35.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel35.Location = new System.Drawing.Point(1099, 18);
            this.uiLabel35.Name = "uiLabel35";
            this.uiLabel35.Size = new System.Drawing.Size(113, 19);
            this.uiLabel35.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel35.TabIndex = 85;
            this.uiLabel35.Text = "Account：";
            this.uiLabel35.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiAvatar1
            // 
            this.uiAvatar1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiAvatar1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.uiAvatar1.Location = new System.Drawing.Point(1026, -3);
            this.uiAvatar1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiAvatar1.Name = "uiAvatar1";
            this.uiAvatar1.Size = new System.Drawing.Size(81, 64);
            this.uiAvatar1.Style = Sunny.UI.UIStyle.Custom;
            this.uiAvatar1.StyleCustomMode = true;
            this.uiAvatar1.TabIndex = 84;
            this.uiAvatar1.Text = "uiAvatar1";
            // 
            // ProcessSquareAcquisition
            // 
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(1940, 1100);
            this.ControlBoxFillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.Controls.Add(this.uiButton1);
            this.Controls.Add(this.uiTextBox33);
            this.Controls.Add(this.uiComboBox1);
            this.Controls.Add(this.uiLabel36);
            this.Controls.Add(this.uiLabel35);
            this.Controls.Add(this.uiAvatar1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.uiIntegerUpDown1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.test);
            this.Controls.Add(this.uiLabel6);
            this.Controls.Add(this.uiButton2);
            this.Controls.Add(this.uiTextBox8);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxTDLength);
            this.Controls.Add(this.textBoxLTLength);
            this.Controls.Add(this.labelTDLength);
            this.Controls.Add(this.labelRTLength);
            this.Controls.Add(this.labelLTLength);
            this.MaximumSize = new System.Drawing.Size(2560, 1440);
            this.Name = "ProcessSquareAcquisition";
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "检测结果";
            this.ZoomScaleRect = new System.Drawing.Rectangle(19, 19, 800, 480);
            this.Initialize += new System.EventHandler(this.ProcessSquareAcquisition_Initialize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }







        #endregion

        private Label labelLTLength;
        private Label labelRTLength;
        private Label labelTDLength;
        private TextBox textBoxLTLength;
        private TextBox textBoxTDLength;
        private Label label1;
        private UILabel uiLabel6;
        private UIButton uiButton2;
        private UITextBox uiTextBox8;
        private UIButton test;
        private TextBox textBox1;
        private UIIntegerUpDown uiIntegerUpDown1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private Label label2;
        private Label label3;
        private Label label4;
        private UIButton uiButton1;
        private UITextBox uiTextBox33;
        private UIComboBox uiComboBox1;
        private UILabel uiLabel36;
        private UILabel uiLabel35;
        private UIAvatar uiAvatar1;
    }
}