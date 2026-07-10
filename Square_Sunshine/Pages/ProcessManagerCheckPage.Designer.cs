using Sunny.UI;
using LiveCharts;
using LiveCharts.Defaults;
using System.Windows.Forms;
using SquareSiliconStickCheck.Tools;
using SquareSiliconStickCheck.Parameters;
using System.Drawing;
using System.Threading;
using System;
using System.Collections;
using SiliconRoundBarCheck.Tools;


namespace SquareSiliconStickCheck.Pages
{
    partial class ProcessManagerCheckPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        /// <summary
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
            this.panelRadiusView_Full = new Sunny.UI.UITitlePanel();
            this.panelRadiusViewFull_Full = new Sunny.UI.UITitlePanel();
            this.uiButton2 = new Sunny.UI.UIButton();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.uiButton1 = new Sunny.UI.UIButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            this.uiScrollingText1 = new Sunny.UI.UIScrollingText();
            this.labelRightAngle = new System.Windows.Forms.Label();
            this.labelLeftAngle = new System.Windows.Forms.Label();
            this.labelDownAngle = new System.Windows.Forms.Label();
            this.labelTopAngle = new System.Windows.Forms.Label();
            this.labelDownLeftDiag = new System.Windows.Forms.Label();
            this.labelDownRightDiag = new System.Windows.Forms.Label();
            this.labelRightLeftDiag = new System.Windows.Forms.Label();
            this.labelRightRightDiag = new System.Windows.Forms.Label();
            this.labelLeftLeftDiag = new System.Windows.Forms.Label();
            this.labelLeftRightDiag = new System.Windows.Forms.Label();
            this.labelTopLeftDiag = new System.Windows.Forms.Label();
            this.labelTopRightDiag = new System.Windows.Forms.Label();
            this.labelLength = new System.Windows.Forms.Label();
            this.labelDownDiag = new System.Windows.Forms.Label();
            this.labelRightDiag = new System.Windows.Forms.Label();
            this.labelLeftDiag = new System.Windows.Forms.Label();
            this.labelTopDiag = new System.Windows.Forms.Label();
            this.labelLRLength = new System.Windows.Forms.Label();
            this.labelTDLength = new System.Windows.Forms.Label();
            this.labelRDLength = new System.Windows.Forms.Label();
            this.labelLDLength = new System.Windows.Forms.Label();
            this.labelRTLength = new System.Windows.Forms.Label();
            this.labelLTLength = new System.Windows.Forms.Label();
            this.textBoxJB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new Sunny.UI.UIButton();
            this.buttonStart = new Sunny.UI.UIButton();
            this.buttonStop = new Sunny.UI.UIButton();
            this._rightBtn = new Sunny.UI.UIButton();
            this._tabMainLineControl = new Sunny.UI.UITabControl();
            this._tabPageLegnth = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonStartScan = new Sunny.UI.UIButton();
            this.panelRadiusViewFull_Full.SuspendLayout();
            this.uiTitlePanel1.SuspendLayout();
            this._tabMainLineControl.SuspendLayout();
            this._tabPageLegnth.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelRadiusView_Full
            // 
            this.panelRadiusView_Full.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.panelRadiusView_Full.Location = new System.Drawing.Point(15, 83);
            this.panelRadiusView_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusView_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusView_Full.Name = "panelRadiusView_Full";
            this.panelRadiusView_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusView_Full.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusView_Full.ShowCollapse = true;
            this.panelRadiusView_Full.ShowText = false;
            this.panelRadiusView_Full.Size = new System.Drawing.Size(1760, 1050);
            this.panelRadiusView_Full.Style = Sunny.UI.UIStyle.Custom;
            this.panelRadiusView_Full.TabIndex = 9;
            this.panelRadiusView_Full.Text = "数据";
            this.panelRadiusView_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusView_Full.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            // 
            // panelRadiusViewFull_Full
            // 
            this.panelRadiusViewFull_Full.Controls.Add(this.uiButton2);
            this.panelRadiusViewFull_Full.Controls.Add(this.textBox2);
            this.panelRadiusViewFull_Full.Controls.Add(this.label5);
            this.panelRadiusViewFull_Full.Controls.Add(this.textBox1);
            this.panelRadiusViewFull_Full.Controls.Add(this.label4);
            this.panelRadiusViewFull_Full.Controls.Add(this.uiButton1);
            this.panelRadiusViewFull_Full.Controls.Add(this.label3);
            this.panelRadiusViewFull_Full.Controls.Add(this.label2);
            this.panelRadiusViewFull_Full.Controls.Add(this.uiTitlePanel1);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopAngle);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelDownDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRightDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLeftDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTopDiag);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLRLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelTDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLDLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelRTLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.labelLTLength);
            this.panelRadiusViewFull_Full.Controls.Add(this.textBoxJB);
            this.panelRadiusViewFull_Full.Controls.Add(this.label1);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonSave);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonStart);
            this.panelRadiusViewFull_Full.Controls.Add(this.buttonStop);
            this.panelRadiusViewFull_Full.Controls.Add(this._rightBtn);
            this.panelRadiusViewFull_Full.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panelRadiusViewFull_Full.Location = new System.Drawing.Point(0, 0);
            this.panelRadiusViewFull_Full.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelRadiusViewFull_Full.MinimumSize = new System.Drawing.Size(1, 1);
            this.panelRadiusViewFull_Full.Name = "panelRadiusViewFull_Full";
            this.panelRadiusViewFull_Full.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.panelRadiusViewFull_Full.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.panelRadiusViewFull_Full.ShowText = false;
            this.panelRadiusViewFull_Full.Size = new System.Drawing.Size(1780, 1050);
            this.panelRadiusViewFull_Full.Style = Sunny.UI.UIStyle.Custom;
            this.panelRadiusViewFull_Full.TabIndex = 0;
            this.panelRadiusViewFull_Full.Text = null;
            this.panelRadiusViewFull_Full.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.panelRadiusViewFull_Full.TitleColor = System.Drawing.Color.Gray;
            // 
            // uiButton2
            // 
            this.uiButton2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.uiButton2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton2.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.uiButton2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton2.Location = new System.Drawing.Point(1082, 53);
            this.uiButton2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton2.Name = "uiButton2";
            this.uiButton2.Size = new System.Drawing.Size(90, 30);
            this.uiButton2.Style = Sunny.UI.UIStyle.Custom;
            this.uiButton2.StyleCustomMode = true;
            this.uiButton2.TabIndex = 44;
            this.uiButton2.Text = "Test";
            this.uiButton2.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton2.Click += new System.EventHandler(this.uiButton2_Click_1);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(746, 53);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(72, 29);
            this.textBox2.TabIndex = 43;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label5.Location = new System.Drawing.Point(599, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(157, 21);
            this.label5.TabIndex = 42;
            this.label5.Text = "Machine number：";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(499, 51);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(71, 29);
            this.textBox1.TabIndex = 41;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label4.Location = new System.Drawing.Point(437, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 21);
            this.label4.TabIndex = 40;
            this.label4.Text = "Model：";
            // 
            // uiButton1
            // 
            this.uiButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.uiButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uiButton1.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.uiButton1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Location = new System.Drawing.Point(1466, 50);
            this.uiButton1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiButton1.Name = "uiButton1";
            this.uiButton1.Size = new System.Drawing.Size(90, 30);
            this.uiButton1.Style = Sunny.UI.UIStyle.Custom;
            this.uiButton1.StyleCustomMode = true;
            this.uiButton1.TabIndex = 39;
            this.uiButton1.Text = "Test";
            this.uiButton1.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiButton1.Click += new System.EventHandler(this.uiButton1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label3.Location = new System.Drawing.Point(1175, 230);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 21);
            this.label3.TabIndex = 38;
            this.label3.Text = "Face verticality_T:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label2.Location = new System.Drawing.Point(829, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(157, 21);
            this.label2.TabIndex = 37;
            this.label2.Text = "Face verticality_H：";
            // 
            // uiTitlePanel1
            // 
            this.uiTitlePanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uiTitlePanel1.Controls.Add(this.uiScrollingText1);
            this.uiTitlePanel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTitlePanel1.Location = new System.Drawing.Point(1179, 384);
            this.uiTitlePanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTitlePanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiTitlePanel1.Name = "uiTitlePanel1";
            this.uiTitlePanel1.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.uiTitlePanel1.ShowText = false;
            this.uiTitlePanel1.Size = new System.Drawing.Size(380, 472);
            this.uiTitlePanel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiTitlePanel1.StyleCustomMode = true;
            this.uiTitlePanel1.TabIndex = 36;
            this.uiTitlePanel1.Text = "Status bar";
            this.uiTitlePanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiTitlePanel1.TitleColor = System.Drawing.Color.Gray;
            // 
            // uiScrollingText1
            // 
            this.uiScrollingText1.BackColor = System.Drawing.Color.DimGray;
            this.uiScrollingText1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiScrollingText1.Location = new System.Drawing.Point(3, 38);
            this.uiScrollingText1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiScrollingText1.Name = "uiScrollingText1";
            this.uiScrollingText1.ScrollingType = Sunny.UI.UIScrollingText.UIScrollingType.LeftToRight;
            this.uiScrollingText1.Size = new System.Drawing.Size(374, 429);
            this.uiScrollingText1.Style = Sunny.UI.UIStyle.Custom;
            this.uiScrollingText1.TabIndex = 1;
            // 
            // labelRightAngle
            // 
            this.labelRightAngle.AutoSize = true;
            this.labelRightAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightAngle.Location = new System.Drawing.Point(1175, 338);
            this.labelRightAngle.Name = "labelRightAngle";
            this.labelRightAngle.Size = new System.Drawing.Size(139, 21);
            this.labelRightAngle.TabIndex = 34;
            this.labelRightAngle.Text = "Side verticality_4:";
            // 
            // labelLeftAngle
            // 
            this.labelLeftAngle.AutoSize = true;
            this.labelLeftAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftAngle.Location = new System.Drawing.Point(829, 338);
            this.labelLeftAngle.Name = "labelLeftAngle";
            this.labelLeftAngle.Size = new System.Drawing.Size(139, 21);
            this.labelLeftAngle.TabIndex = 33;
            this.labelLeftAngle.Text = "Side verticality_3:";
            // 
            // labelDownAngle
            // 
            this.labelDownAngle.AutoSize = true;
            this.labelDownAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownAngle.Location = new System.Drawing.Point(437, 338);
            this.labelDownAngle.Name = "labelDownAngle";
            this.labelDownAngle.Size = new System.Drawing.Size(139, 21);
            this.labelDownAngle.TabIndex = 32;
            this.labelDownAngle.Text = "Side verticality_2:";
            // 
            // labelTopAngle
            // 
            this.labelTopAngle.AutoSize = true;
            this.labelTopAngle.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopAngle.Location = new System.Drawing.Point(80, 335);
            this.labelTopAngle.Name = "labelTopAngle";
            this.labelTopAngle.Size = new System.Drawing.Size(139, 21);
            this.labelTopAngle.TabIndex = 31;
            this.labelTopAngle.Text = "Side verticality_1:";
            // 
            // labelDownLeftDiag
            // 
            this.labelDownLeftDiag.AutoSize = true;
            this.labelDownLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownLeftDiag.Location = new System.Drawing.Point(437, 787);
            this.labelDownLeftDiag.Name = "labelDownLeftDiag";
            this.labelDownLeftDiag.Size = new System.Drawing.Size(118, 21);
            this.labelDownLeftDiag.TabIndex = 30;
            this.labelDownLeftDiag.Text = "4_Projectio1：";
            // 
            // labelDownRightDiag
            // 
            this.labelDownRightDiag.AutoSize = true;
            this.labelDownRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownRightDiag.Location = new System.Drawing.Point(839, 787);
            this.labelDownRightDiag.Name = "labelDownRightDiag";
            this.labelDownRightDiag.Size = new System.Drawing.Size(118, 21);
            this.labelDownRightDiag.TabIndex = 29;
            this.labelDownRightDiag.Text = "4_Projectio2：";
            // 
            // labelRightLeftDiag
            // 
            this.labelRightLeftDiag.AutoSize = true;
            this.labelRightLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightLeftDiag.Location = new System.Drawing.Point(437, 663);
            this.labelRightLeftDiag.Name = "labelRightLeftDiag";
            this.labelRightLeftDiag.Size = new System.Drawing.Size(118, 21);
            this.labelRightLeftDiag.TabIndex = 28;
            this.labelRightLeftDiag.Text = "3_Projectio1：";
            // 
            // labelRightRightDiag
            // 
            this.labelRightRightDiag.AutoSize = true;
            this.labelRightRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightRightDiag.Location = new System.Drawing.Point(839, 663);
            this.labelRightRightDiag.Name = "labelRightRightDiag";
            this.labelRightRightDiag.Size = new System.Drawing.Size(118, 21);
            this.labelRightRightDiag.TabIndex = 27;
            this.labelRightRightDiag.Text = "3_Projectio2：";
            // 
            // labelLeftLeftDiag
            // 
            this.labelLeftLeftDiag.AutoSize = true;
            this.labelLeftLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftLeftDiag.Location = new System.Drawing.Point(437, 556);
            this.labelLeftLeftDiag.Name = "labelLeftLeftDiag";
            this.labelLeftLeftDiag.Size = new System.Drawing.Size(118, 21);
            this.labelLeftLeftDiag.TabIndex = 26;
            this.labelLeftLeftDiag.Text = "2_Projectio1：";
            // 
            // labelLeftRightDiag
            // 
            this.labelLeftRightDiag.AutoSize = true;
            this.labelLeftRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftRightDiag.Location = new System.Drawing.Point(839, 556);
            this.labelLeftRightDiag.Name = "labelLeftRightDiag";
            this.labelLeftRightDiag.Size = new System.Drawing.Size(118, 21);
            this.labelLeftRightDiag.TabIndex = 25;
            this.labelLeftRightDiag.Text = "2_Projectio2：";
            // 
            // labelTopLeftDiag
            // 
            this.labelTopLeftDiag.AutoSize = true;
            this.labelTopLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopLeftDiag.Location = new System.Drawing.Point(437, 450);
            this.labelTopLeftDiag.Name = "labelTopLeftDiag";
            this.labelTopLeftDiag.Size = new System.Drawing.Size(118, 21);
            this.labelTopLeftDiag.TabIndex = 24;
            this.labelTopLeftDiag.Text = "1_Projectio1：";
            // 
            // labelTopRightDiag
            // 
            this.labelTopRightDiag.AutoSize = true;
            this.labelTopRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopRightDiag.Location = new System.Drawing.Point(829, 450);
            this.labelTopRightDiag.Name = "labelTopRightDiag";
            this.labelTopRightDiag.Size = new System.Drawing.Size(118, 21);
            this.labelTopRightDiag.TabIndex = 23;
            this.labelTopRightDiag.Text = "1_Projectio2：";
            // 
            // labelLength
            // 
            this.labelLength.AutoSize = true;
            this.labelLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLength.Location = new System.Drawing.Point(839, 59);
            this.labelLength.Name = "labelLength";
            this.labelLength.Size = new System.Drawing.Size(79, 21);
            this.labelLength.TabIndex = 22;
            this.labelLength.Text = "Length：";
            this.labelLength.Click += new System.EventHandler(this.labelLength_Click);
            // 
            // labelDownDiag
            // 
            this.labelDownDiag.AutoSize = true;
            this.labelDownDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelDownDiag.Location = new System.Drawing.Point(80, 787);
            this.labelDownDiag.Name = "labelDownDiag";
            this.labelDownDiag.Size = new System.Drawing.Size(121, 21);
            this.labelDownDiag.TabIndex = 21;
            this.labelDownDiag.Text = "Arc length_4：";
            // 
            // labelRightDiag
            // 
            this.labelRightDiag.AutoSize = true;
            this.labelRightDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRightDiag.Location = new System.Drawing.Point(80, 663);
            this.labelRightDiag.Name = "labelRightDiag";
            this.labelRightDiag.Size = new System.Drawing.Size(121, 21);
            this.labelRightDiag.TabIndex = 20;
            this.labelRightDiag.Text = "Arc length_3：";
            // 
            // labelLeftDiag
            // 
            this.labelLeftDiag.AutoSize = true;
            this.labelLeftDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLeftDiag.Location = new System.Drawing.Point(80, 556);
            this.labelLeftDiag.Name = "labelLeftDiag";
            this.labelLeftDiag.Size = new System.Drawing.Size(121, 21);
            this.labelLeftDiag.TabIndex = 19;
            this.labelLeftDiag.Text = "Arc length_2：";
            // 
            // labelTopDiag
            // 
            this.labelTopDiag.AutoSize = true;
            this.labelTopDiag.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTopDiag.Location = new System.Drawing.Point(80, 447);
            this.labelTopDiag.Name = "labelTopDiag";
            this.labelTopDiag.Size = new System.Drawing.Size(121, 21);
            this.labelTopDiag.TabIndex = 18;
            this.labelTopDiag.Text = "Arc length_1：";
            // 
            // labelLRLength
            // 
            this.labelLRLength.AutoSize = true;
            this.labelLRLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLRLength.Location = new System.Drawing.Point(437, 230);
            this.labelLRLength.Name = "labelLRLength";
            this.labelLRLength.Size = new System.Drawing.Size(150, 21);
            this.labelLRLength.TabIndex = 17;
            this.labelLRLength.Text = "Diagonal length 2:";
            // 
            // labelTDLength
            // 
            this.labelTDLength.AutoSize = true;
            this.labelTDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelTDLength.Location = new System.Drawing.Point(80, 227);
            this.labelTDLength.Name = "labelTDLength";
            this.labelTDLength.Size = new System.Drawing.Size(150, 21);
            this.labelTDLength.TabIndex = 16;
            this.labelTDLength.Text = "Diagonal length 1:";
            // 
            // labelRDLength
            // 
            this.labelRDLength.AutoSize = true;
            this.labelRDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRDLength.Location = new System.Drawing.Point(1175, 134);
            this.labelRDLength.Name = "labelRDLength";
            this.labelRDLength.Size = new System.Drawing.Size(83, 21);
            this.labelRDLength.TabIndex = 15;
            this.labelRDLength.Text = "Edge_D：";
            // 
            // labelLDLength
            // 
            this.labelLDLength.AutoSize = true;
            this.labelLDLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLDLength.Location = new System.Drawing.Point(829, 134);
            this.labelLDLength.Name = "labelLDLength";
            this.labelLDLength.Size = new System.Drawing.Size(82, 21);
            this.labelLDLength.TabIndex = 14;
            this.labelLDLength.Text = "Edge_C：";
            // 
            // labelRTLength
            // 
            this.labelRTLength.AutoSize = true;
            this.labelRTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelRTLength.Location = new System.Drawing.Point(437, 134);
            this.labelRTLength.Name = "labelRTLength";
            this.labelRTLength.Size = new System.Drawing.Size(81, 21);
            this.labelRTLength.TabIndex = 13;
            this.labelRTLength.Text = "Edge_B：";
            // 
            // labelLTLength
            // 
            this.labelLTLength.AutoSize = true;
            this.labelLTLength.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.labelLTLength.Location = new System.Drawing.Point(80, 131);
            this.labelLTLength.Name = "labelLTLength";
            this.labelLTLength.Size = new System.Drawing.Size(82, 21);
            this.labelLTLength.TabIndex = 12;
            this.labelLTLength.Text = "Edge_A：";
            // 
            // textBoxJB
            // 
            this.textBoxJB.Location = new System.Drawing.Point(221, 51);
            this.textBoxJB.Name = "textBoxJB";
            this.textBoxJB.Size = new System.Drawing.Size(188, 29);
            this.textBoxJB.TabIndex = 11;
            this.textBoxJB.TextChanged += new System.EventHandler(this.textBoxJB_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.label1.Location = new System.Drawing.Point(90, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 21);
            this.label1.TabIndex = 10;
            this.label1.Text = "Crystal knitting：";
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSave.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonSave.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSave.Location = new System.Drawing.Point(1370, 50);
            this.buttonSave.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(90, 30);
            this.buttonSave.Style = Sunny.UI.UIStyle.Custom;
            this.buttonSave.StyleCustomMode = true;
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "Save";
            this.buttonSave.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click_1);
            // 
            // buttonStart
            // 
            this.buttonStart.BackColor = System.Drawing.Color.Yellow;
            this.buttonStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStart.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonStart.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStart.Location = new System.Drawing.Point(1178, 50);
            this.buttonStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(90, 30);
            this.buttonStart.Style = Sunny.UI.UIStyle.Custom;
            this.buttonStart.StyleCustomMode = true;
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.buttonStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStop.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStop.Location = new System.Drawing.Point(1274, 50);
            this.buttonStop.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(90, 30);
            this.buttonStop.Style = Sunny.UI.UIStyle.Custom;
            this.buttonStop.StyleCustomMode = true;
            this.buttonStop.TabIndex = 0;
            this.buttonStop.Text = "Stop";
            this.buttonStop.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click_1);
            // 
            // _rightBtn
            // 
            this._rightBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(0, 362);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.Style = Sunny.UI.UIStyle.Custom;
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            // 
            // _tabMainLineControl
            // 
            this._tabMainLineControl.Controls.Add(this._tabPageLegnth);
            this._tabMainLineControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this._tabMainLineControl.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._tabMainLineControl.ItemSize = new System.Drawing.Size(150, 40);
            this._tabMainLineControl.Location = new System.Drawing.Point(25, 85);
            this._tabMainLineControl.MainPage = "主页";
            this._tabMainLineControl.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this._tabMainLineControl.Name = "_tabMainLineControl";
            this._tabMainLineControl.SelectedIndex = 0;
            this._tabMainLineControl.Size = new System.Drawing.Size(1780, 1050);
            this._tabMainLineControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this._tabMainLineControl.Style = Sunny.UI.UIStyle.Custom;
            this._tabMainLineControl.TabIndex = 22;
            this._tabMainLineControl.TabSelectedForeColor = System.Drawing.Color.White;
            this._tabMainLineControl.TabSelectedHighColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._tabMainLineControl.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            // 
            // _tabPageLegnth
            // 
            this._tabPageLegnth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this._tabPageLegnth.Controls.Add(this.panelRadiusViewFull_Full);
            this._tabPageLegnth.Location = new System.Drawing.Point(0, 40);
            this._tabPageLegnth.Name = "_tabPageLegnth";
            this._tabPageLegnth.Size = new System.Drawing.Size(1780, 1010);
            this._tabPageLegnth.TabIndex = 4;
            this._tabPageLegnth.Text = "Process";
            this._tabPageLegnth.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // buttonStartScan
            // 
            this.buttonStartScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStartScan.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStartScan.Location = new System.Drawing.Point(1425, 687);
            this.buttonStartScan.MinimumSize = new System.Drawing.Size(1, 1);
            this.buttonStartScan.Name = "buttonStartScan";
            this.buttonStartScan.Size = new System.Drawing.Size(90, 30);
            this.buttonStartScan.TabIndex = 0;
            this.buttonStartScan.Text = "开始扫描";
            this.buttonStartScan.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.buttonStartScan.Visible = false;
            this.buttonStartScan.Click += new System.EventHandler(this.ButtonStartScan_Click);
            // 
            // ProcessManagerCheckPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(1940, 1100);
            this.Controls.Add(this._tabMainLineControl);
            this.Name = "ProcessManagerCheckPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1002;
            this.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ShowTitle = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "Measure process";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessManagerPage_FormClosing);
            this.Load += new System.EventHandler(this.ProcessManagerCheckPage_Load);
            this.panelRadiusViewFull_Full.ResumeLayout(false);
            this.panelRadiusViewFull_Full.PerformLayout();
            this.uiTitlePanel1.ResumeLayout(false);
            this._tabMainLineControl.ResumeLayout(false);
            this._tabPageLegnth.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void ProcessManagerPage_Shown(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                if (true == panelRadiusView_Full.Visible)
                {
                    panelRadiusView_Full.Refresh();
                }
            }
            
        }

        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                Stop();
            }
        }

        private void ProcessManagerPage_FormClosing1(object sender, FormClosingEventArgs e)
        {
            if (SettingParameter.Instance().NDaemon == 1 || SettingParameter.Instance().NDaemon == 2)
            {
                Stop();
            }
        }

        private void ProcessManagerPage_SizeChanged(object sender, EventArgs e)
        {
            Refresh();
            Invalidate();
        }

        private void _rightBtn_Click(object sender, EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();
            this._rightBtn.Visible = false;

            SettingParamPage.instance.ShowRightBtnFunc(false);
            //MainResultListPage.instance.ShowRightBtnFunc(false);
            //MainStatisticPage.instance.ShowRightBtnFunc(false);

        }

     

        private void ButtonTotal_Click(object sender, EventArgs e)
        {
            
        }



        public enum emResultType
        {
            EM_RESULTRADIUS = 0,
            EM_RESULTYINGLI = 1,
            EM_RESULTYINLIE = 2,
            EM_RESULTLJ = 3,
            EM_RESULTTOTAL = 4,
        };

        private void drawLine(PictureBox ob, PaintEventArgs e, int nIndex, int nType)
        {
            if (ob.Image == null)
            {
                return;
            }
            Graphics g = e.Graphics;
            Pen myPen = new Pen(Color.DeepPink, 5);
            int nHeight = ob.Image.Height;

            ArrayList infoArray = null;
            switch (nType)
            {
                case (int)emResultType.EM_RESULTYINGLI:
                    {
                        infoArray = _subYingLiPictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTYINLIE:
                    {
                        infoArray = _subYinLiePictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTLJ:
                    {
                        infoArray = _subLJPictureIndexInfo[nIndex];
                        break;
                    }
                case (int)emResultType.EM_RESULTTOTAL:
                    {
                        infoArray = _subTotalPictureIndexInfo;
                        break;
                    }
                default:
                    {
                        infoArray = _subYingLiPictureIndexInfo[nIndex];
                        break;
                    }

            }
           


            for (int i = 0; i < infoArray.Count; i++)
            {
                int[] nArray = (int[])infoArray[i];

                g.DrawLine(myPen, new Point(nArray[0], 0), new Point(nArray[0], nHeight));
                g.DrawLine(myPen, new Point(nArray[1], 0), new Point(nArray[1], nHeight));
                g.FillRectangle(new SolidBrush(Color.FromArgb(125, Color.Cyan)), new Rectangle(nArray[0], 0, (nArray[1] - nArray[0]), nHeight));

            }


        }




      

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            
        }

        private void ButtonStartScanStick_Click(object sender, System.EventArgs e)
        {
            
            ProcessManager.Instance().ScanSquareSiliconStickRound(_strSiliconSerialNum);


        }
        private void ReleaseCameras()
        {
             LogHelper.Info("Silicon","ReleaseCameras begin");
            try
            {
               

              


            }
            catch (Exception ex)
            {
                 LogHelper.Info("Silicon","ReleaseCameras Exception " + ex.Message);
            }

             LogHelper.Info("Silicon","ReleaseCameras End");
        }

       

       


        public enum emCameraType
        {
            EM_HIKVIDEO = 0,
            EM_HIKLJ = 1,
            EM_BVYINLIE = 2,
            EM_BVYINGLI = 3
        };

        private void InitDevFunction(int nType)
        {
            switch(nType)
            {
               
                case (int)emCameraType.EM_BVYINLIE:
                    {
                       
                        break;
                    }
                case (int)emCameraType.EM_BVYINGLI:
                    {
                        
                        break;
                    }
            }
        }


        private void StopCameras()
        {
            try
            {
                
               
            }
            catch(Exception ex)
            {
                 LogHelper.Info("Silicon","StopCameras exception " + ex.Message);
            }
           
        }

        private bool InitCameras()
        {
             LogHelper.Info("Silicon","InitCameras Begin");


            

            LogHelper.Info("Silicon","InitCameras End");
            return true;
        }

        
        private void ButtonStartScan_Click(object sender, System.EventArgs e)
        {

            ProcessManager.Instance().InitMatResource();
            m_threadRefreshVideoView = new Thread(UpdateSquareInfoChartView);
            m_threadRefreshVideoView.Start();

         
        }

        #endregion

     
       
        private System.Windows.Forms.Timer Timer { get; set; }
       
        private UIButton buttonStart;
        private UIButton buttonStop;
        private UIButton buttonStartScan;
     
        private UIButton _rightBtn;
        private GroupBox groupBox1;
        private GroupBox groupBox2;


        private UITabControl _tabMainLineControl;
        private TabPage _tabPageLegnth;   //直径
        private System.Windows.Forms.TabPage _tabPageAngle;  //应力

        private UITitlePanel panelRadiusView_Full;
        private UITitlePanel panelRadiusViewFull_Full;


        private UIButton buttonSave;
        private UIButton buttonTotal;

       
        private int _nNoCollapsedRadiusViewHeight;
        private int _nCollapsedRadiusViewHeight;
        private int _nBeginRaidusViewY;
        private int _nRadiusViewWidth;
        private int _nSplitViewHeight;
        private int _nRadiusViewPictureboxWidth;
        private int _nRadiusViewPictureboxHeight;
        private int _nRadiusViewResultPictureboxWidth;
        private int _nRadiusViewResultPictureboxHeight;
        private int _nMainPanelViewWidth;
        private int _nMainPanelViewHeight;





        private bool _bDragging = false;
        private int _nDragLineIndex = 0;
        private Label label1;
        private TextBox textBoxJB;
        private Label labelTopLeftDiag;
        private Label labelTopRightDiag;
        private Label labelLength;
        private Label labelDownDiag;
        private Label labelRightDiag;
        private Label labelLeftDiag;
        private Label labelTopDiag;
        private Label labelLRLength;
        private Label labelTDLength;
        private Label labelRDLength;
        private Label labelLDLength;
        private Label labelRTLength;
        private Label labelLTLength;
        private Label labelDownLeftDiag;
        private Label labelDownRightDiag;
        private Label labelRightLeftDiag;
        private Label labelRightRightDiag;
        private Label labelLeftLeftDiag;
        private Label labelLeftRightDiag;
        private Label labelRightAngle;
        private Label labelLeftAngle;
        private Label labelDownAngle;
        private Label labelTopAngle;
        private UITitlePanel uiTitlePanel1;
        private UIScrollingText uiScrollingText1;
        private Label label3;
        private Label label2;
        private UIButton uiButton1;
        private TextBox textBox1;
        private Label label4;
        private TextBox textBox2;
        private Label label5;
        private UIButton uiButton2;
    }
}