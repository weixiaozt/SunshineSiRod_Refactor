using SquareSiliconStickCheck.Data;
using SquareSiliconStickCheck.Parameters;
using Sunny.UI;
using System;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SquareSiliconStickCheck.Pages
{
    partial class SettingParamPage
    {
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
            this._rightBtn = new Sunny.UI.UIButton();
            this.button2 = new Sunny.UI.UIButton();
            this.button1 = new Sunny.UI.UIButton();
            this.uiLabel29 = new Sunny.UI.UILabel();
            this.label4 = new Sunny.UI.UILabel();
            this.tb3DLaserLeftIP = new Sunny.UI.UIIPTextBox();
            this.tb3DLaserDownIP = new Sunny.UI.UIIPTextBox();
            this.tb3DLaserTopIP = new Sunny.UI.UIIPTextBox();
            this.tb3DLaserRightIP = new Sunny.UI.UIIPTextBox();
            this.tb3DLaserLeftPort = new System.Windows.Forms.TextBox();
            this.uiPLCMoveParameters = new Sunny.UI.UIGroupBox();
            this.groupBoxDown = new Sunny.UI.UIGroupBox();
            this.uiLabel6 = new Sunny.UI.UILabel();
            this.uiLabel7 = new Sunny.UI.UILabel();
            this.tb3DLocalDownIP = new Sunny.UI.UIIPTextBox();
            this.groupBoxTop = new Sunny.UI.UIGroupBox();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.uiLabel5 = new Sunny.UI.UILabel();
            this.tb3DLocalTopIP = new Sunny.UI.UIIPTextBox();
            this.groupBoxRight = new Sunny.UI.UIGroupBox();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.tb3DLocalRightIP = new Sunny.UI.UIIPTextBox();
            this.groupboxLeft = new Sunny.UI.UIGroupBox();
            this.tb3DLocalLeftIP = new Sunny.UI.UIIPTextBox();
            this.tb3DLaserDownPort = new System.Windows.Forms.TextBox();
            this.uiLabel14 = new Sunny.UI.UILabel();
            this.uiLabel15 = new Sunny.UI.UILabel();
            this.textBoxCheckMinVerticalLength = new System.Windows.Forms.TextBox();
            this.textBoxCheckMinLength = new System.Windows.Forms.TextBox();
            this.uiLabelLocalLeftIP = new Sunny.UI.UILabel();
            this.uiLabelLocalRightIP = new Sunny.UI.UILabel();
            this.uiLabelLocalDownIP = new Sunny.UI.UILabel();
            this.uiLabelLocalTopIP = new Sunny.UI.UILabel();
            this.tb3DLaserTopPort = new System.Windows.Forms.TextBox();
            this.tb3DLaserRightPort = new System.Windows.Forms.TextBox();
            this.tbMoveControlPort = new System.Windows.Forms.TextBox();
            this.tbMoveControlIP = new Sunny.UI.UIIPTextBox();
            this.uiLabel9 = new System.Windows.Forms.Label();
            this.uiLabel8 = new System.Windows.Forms.Label();
            this.uiGroupBox1 = new Sunny.UI.UIGroupBox();
            this.uiGroupBox2 = new Sunny.UI.UIGroupBox();
            this.numericUpDownTDLength = new System.Windows.Forms.NumericUpDown();
            this.uiLabel13 = new Sunny.UI.UILabel();
            this.numericUpDownLRLength = new System.Windows.Forms.NumericUpDown();
            this.uiLabel12 = new Sunny.UI.UILabel();
            this.numericUpDownRTLength = new System.Windows.Forms.NumericUpDown();
            this.uiLabel11 = new Sunny.UI.UILabel();
            this.numericUpDownLTLength = new System.Windows.Forms.NumericUpDown();
            this.uiLabel10 = new Sunny.UI.UILabel();
            this.uiPLCMoveParameters.SuspendLayout();
            this.groupBoxDown.SuspendLayout();
            this.groupBoxTop.SuspendLayout();
            this.groupBoxRight.SuspendLayout();
            this.groupboxLeft.SuspendLayout();
            this.uiGroupBox1.SuspendLayout();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTDLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLRLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRTLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLTLength)).BeginInit();
            this.SuspendLayout();
            // 
            // _rightBtn
            // 
            this._rightBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this._rightBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Location = new System.Drawing.Point(3, 343);
            this._rightBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this._rightBtn.Name = "_rightBtn";
            this._rightBtn.Size = new System.Drawing.Size(20, 196);
            this._rightBtn.Style = Sunny.UI.UIStyle.Custom;
            this._rightBtn.TabIndex = 3;
            this._rightBtn.Text = ">";
            this._rightBtn.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this._rightBtn.Visible = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(114)))), ((int)(((byte)(0)))));
            this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button2.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.button2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Location = new System.Drawing.Point(483, 844);
            this.button2.MinimumSize = new System.Drawing.Size(1, 1);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(174, 44);
            this.button2.Style = Sunny.UI.UIStyle.Custom;
            this.button2.TabIndex = 14;
            this.button2.Text = "取消";
            this.button2.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FillColor = System.Drawing.SystemColors.ActiveCaption;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(46, 844);
            this.button1.MinimumSize = new System.Drawing.Size(1, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(174, 44);
            this.button1.Style = Sunny.UI.UIStyle.Custom;
            this.button1.TabIndex = 13;
            this.button1.Text = "确定";
            this.button1.TipsFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // uiLabel29
            // 
            this.uiLabel29.AutoSize = true;
            this.uiLabel29.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel29.Location = new System.Drawing.Point(368, 39);
            this.uiLabel29.Name = "uiLabel29";
            this.uiLabel29.Size = new System.Drawing.Size(62, 21);
            this.uiLabel29.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel29.TabIndex = 2;
            this.uiLabel29.Text = "LocalIP:";
            this.uiLabel29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(23, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 21);
            this.label4.Style = Sunny.UI.UIStyle.Custom;
            this.label4.TabIndex = 0;
            this.label4.Text = "IP:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserLeftIP
            // 
            this.tb3DLaserLeftIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb3DLaserLeftIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLaserLeftIP.Location = new System.Drawing.Point(118, 39);
            this.tb3DLaserLeftIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLaserLeftIP.MinimumSize = new System.Drawing.Size(1, 16);
            this.tb3DLaserLeftIP.Name = "tb3DLaserLeftIP";
            this.tb3DLaserLeftIP.Padding = new System.Windows.Forms.Padding(5);
            this.tb3DLaserLeftIP.ShowText = false;
            this.tb3DLaserLeftIP.Size = new System.Drawing.Size(177, 29);
            this.tb3DLaserLeftIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLaserLeftIP.TabIndex = 1;
            this.tb3DLaserLeftIP.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserDownIP
            // 
            this.tb3DLaserDownIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb3DLaserDownIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLaserDownIP.Location = new System.Drawing.Point(118, 39);
            this.tb3DLaserDownIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLaserDownIP.MinimumSize = new System.Drawing.Size(1, 16);
            this.tb3DLaserDownIP.Name = "tb3DLaserDownIP";
            this.tb3DLaserDownIP.Padding = new System.Windows.Forms.Padding(5);
            this.tb3DLaserDownIP.ShowText = false;
            this.tb3DLaserDownIP.Size = new System.Drawing.Size(177, 29);
            this.tb3DLaserDownIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLaserDownIP.TabIndex = 1;
            this.tb3DLaserDownIP.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserTopIP
            // 
            this.tb3DLaserTopIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb3DLaserTopIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLaserTopIP.Location = new System.Drawing.Point(118, 39);
            this.tb3DLaserTopIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLaserTopIP.MinimumSize = new System.Drawing.Size(1, 16);
            this.tb3DLaserTopIP.Name = "tb3DLaserTopIP";
            this.tb3DLaserTopIP.Padding = new System.Windows.Forms.Padding(5);
            this.tb3DLaserTopIP.ShowText = false;
            this.tb3DLaserTopIP.Size = new System.Drawing.Size(177, 29);
            this.tb3DLaserTopIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLaserTopIP.TabIndex = 1;
            this.tb3DLaserTopIP.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserRightIP
            // 
            this.tb3DLaserRightIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tb3DLaserRightIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLaserRightIP.Location = new System.Drawing.Point(118, 39);
            this.tb3DLaserRightIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLaserRightIP.MinimumSize = new System.Drawing.Size(1, 16);
            this.tb3DLaserRightIP.Name = "tb3DLaserRightIP";
            this.tb3DLaserRightIP.Padding = new System.Windows.Forms.Padding(5);
            this.tb3DLaserRightIP.ShowText = false;
            this.tb3DLaserRightIP.Size = new System.Drawing.Size(177, 29);
            this.tb3DLaserRightIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLaserRightIP.TabIndex = 1;
            this.tb3DLaserRightIP.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserLeftPort
            // 
            this.tb3DLaserLeftPort.Location = new System.Drawing.Point(0, 0);
            this.tb3DLaserLeftPort.Name = "tb3DLaserLeftPort";
            this.tb3DLaserLeftPort.Size = new System.Drawing.Size(100, 21);
            this.tb3DLaserLeftPort.TabIndex = 0;
            // 
            // uiPLCMoveParameters
            // 
            this.uiPLCMoveParameters.Controls.Add(this.groupBoxDown);
            this.uiPLCMoveParameters.Controls.Add(this.groupBoxTop);
            this.uiPLCMoveParameters.Controls.Add(this.groupBoxRight);
            this.uiPLCMoveParameters.Controls.Add(this.groupboxLeft);
            this.uiPLCMoveParameters.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiPLCMoveParameters.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiPLCMoveParameters.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.uiPLCMoveParameters.Location = new System.Drawing.Point(30, 57);
            this.uiPLCMoveParameters.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiPLCMoveParameters.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiPLCMoveParameters.Name = "uiPLCMoveParameters";
            this.uiPLCMoveParameters.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiPLCMoveParameters.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.uiPLCMoveParameters.Size = new System.Drawing.Size(971, 551);
            this.uiPLCMoveParameters.Style = Sunny.UI.UIStyle.Custom;
            this.uiPLCMoveParameters.TabIndex = 28;
            this.uiPLCMoveParameters.TabStop = false;
            this.uiPLCMoveParameters.Text = "3D相机";
            this.uiPLCMoveParameters.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxDown
            // 
            this.groupBoxDown.Controls.Add(this.uiLabel6);
            this.groupBoxDown.Controls.Add(this.uiLabel7);
            this.groupBoxDown.Controls.Add(this.tb3DLaserDownIP);
            this.groupBoxDown.Controls.Add(this.tb3DLocalDownIP);
            this.groupBoxDown.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxDown.Location = new System.Drawing.Point(16, 425);
            this.groupBoxDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxDown.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupBoxDown.Name = "groupBoxDown";
            this.groupBoxDown.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupBoxDown.Size = new System.Drawing.Size(816, 91);
            this.groupBoxDown.Style = Sunny.UI.UIStyle.Custom;
            this.groupBoxDown.TabIndex = 52;
            this.groupBoxDown.TabStop = false;
            this.groupBoxDown.Text = "下";
            this.groupBoxDown.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel6
            // 
            this.uiLabel6.AutoSize = true;
            this.uiLabel6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel6.Location = new System.Drawing.Point(23, 39);
            this.uiLabel6.Name = "uiLabel6";
            this.uiLabel6.Size = new System.Drawing.Size(26, 21);
            this.uiLabel6.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel6.TabIndex = 0;
            this.uiLabel6.Text = "IP:";
            this.uiLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel7
            // 
            this.uiLabel7.AutoSize = true;
            this.uiLabel7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel7.Location = new System.Drawing.Point(368, 39);
            this.uiLabel7.Name = "uiLabel7";
            this.uiLabel7.Size = new System.Drawing.Size(62, 21);
            this.uiLabel7.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel7.TabIndex = 2;
            this.uiLabel7.Text = "LocalIP:";
            this.uiLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLocalDownIP
            // 
            this.tb3DLocalDownIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLocalDownIP.Location = new System.Drawing.Point(469, 39);
            this.tb3DLocalDownIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLocalDownIP.MinimumSize = new System.Drawing.Size(1, 1);
            this.tb3DLocalDownIP.Name = "tb3DLocalDownIP";
            this.tb3DLocalDownIP.Padding = new System.Windows.Forms.Padding(1);
            this.tb3DLocalDownIP.ShowText = false;
            this.tb3DLocalDownIP.Size = new System.Drawing.Size(177, 39);
            this.tb3DLocalDownIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLocalDownIP.TabIndex = 3;
            this.tb3DLocalDownIP.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxTop
            // 
            this.groupBoxTop.Controls.Add(this.uiLabel4);
            this.groupBoxTop.Controls.Add(this.uiLabel5);
            this.groupBoxTop.Controls.Add(this.tb3DLaserTopIP);
            this.groupBoxTop.Controls.Add(this.tb3DLocalTopIP);
            this.groupBoxTop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxTop.Location = new System.Drawing.Point(16, 286);
            this.groupBoxTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxTop.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupBoxTop.Name = "groupBoxTop";
            this.groupBoxTop.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupBoxTop.Size = new System.Drawing.Size(816, 91);
            this.groupBoxTop.Style = Sunny.UI.UIStyle.Custom;
            this.groupBoxTop.TabIndex = 51;
            this.groupBoxTop.TabStop = false;
            this.groupBoxTop.Text = "上";
            this.groupBoxTop.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel4
            // 
            this.uiLabel4.AutoSize = true;
            this.uiLabel4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.Location = new System.Drawing.Point(23, 39);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(26, 21);
            this.uiLabel4.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel4.TabIndex = 0;
            this.uiLabel4.Text = "IP:";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel5
            // 
            this.uiLabel5.AutoSize = true;
            this.uiLabel5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel5.Location = new System.Drawing.Point(368, 39);
            this.uiLabel5.Name = "uiLabel5";
            this.uiLabel5.Size = new System.Drawing.Size(62, 21);
            this.uiLabel5.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel5.TabIndex = 2;
            this.uiLabel5.Text = "LocalIP:";
            this.uiLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLocalTopIP
            // 
            this.tb3DLocalTopIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLocalTopIP.Location = new System.Drawing.Point(469, 39);
            this.tb3DLocalTopIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLocalTopIP.MinimumSize = new System.Drawing.Size(1, 1);
            this.tb3DLocalTopIP.Name = "tb3DLocalTopIP";
            this.tb3DLocalTopIP.Padding = new System.Windows.Forms.Padding(1);
            this.tb3DLocalTopIP.ShowText = false;
            this.tb3DLocalTopIP.Size = new System.Drawing.Size(177, 39);
            this.tb3DLocalTopIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLocalTopIP.TabIndex = 3;
            this.tb3DLocalTopIP.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxRight
            // 
            this.groupBoxRight.Controls.Add(this.uiLabel2);
            this.groupBoxRight.Controls.Add(this.uiLabel3);
            this.groupBoxRight.Controls.Add(this.tb3DLaserRightIP);
            this.groupBoxRight.Controls.Add(this.tb3DLocalRightIP);
            this.groupBoxRight.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBoxRight.Location = new System.Drawing.Point(16, 160);
            this.groupBoxRight.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxRight.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupBoxRight.Name = "groupBoxRight";
            this.groupBoxRight.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupBoxRight.Size = new System.Drawing.Size(816, 91);
            this.groupBoxRight.Style = Sunny.UI.UIStyle.Custom;
            this.groupBoxRight.TabIndex = 50;
            this.groupBoxRight.TabStop = false;
            this.groupBoxRight.Text = "右";
            this.groupBoxRight.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.AutoSize = true;
            this.uiLabel2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel2.Location = new System.Drawing.Point(23, 39);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(26, 21);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 0;
            this.uiLabel2.Text = "IP:";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel3
            // 
            this.uiLabel3.AutoSize = true;
            this.uiLabel3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel3.Location = new System.Drawing.Point(368, 39);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(62, 21);
            this.uiLabel3.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel3.TabIndex = 2;
            this.uiLabel3.Text = "LocalIP:";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLocalRightIP
            // 
            this.tb3DLocalRightIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLocalRightIP.Location = new System.Drawing.Point(469, 39);
            this.tb3DLocalRightIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLocalRightIP.MinimumSize = new System.Drawing.Size(1, 1);
            this.tb3DLocalRightIP.Name = "tb3DLocalRightIP";
            this.tb3DLocalRightIP.Padding = new System.Windows.Forms.Padding(1);
            this.tb3DLocalRightIP.ShowText = false;
            this.tb3DLocalRightIP.Size = new System.Drawing.Size(177, 39);
            this.tb3DLocalRightIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLocalRightIP.TabIndex = 3;
            this.tb3DLocalRightIP.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupboxLeft
            // 
            this.groupboxLeft.Controls.Add(this.label4);
            this.groupboxLeft.Controls.Add(this.uiLabel29);
            this.groupboxLeft.Controls.Add(this.tb3DLaserLeftIP);
            this.groupboxLeft.Controls.Add(this.tb3DLocalLeftIP);
            this.groupboxLeft.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupboxLeft.Location = new System.Drawing.Point(16, 35);
            this.groupboxLeft.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupboxLeft.MinimumSize = new System.Drawing.Size(1, 1);
            this.groupboxLeft.Name = "groupboxLeft";
            this.groupboxLeft.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.groupboxLeft.Size = new System.Drawing.Size(816, 91);
            this.groupboxLeft.Style = Sunny.UI.UIStyle.Custom;
            this.groupboxLeft.TabIndex = 49;
            this.groupboxLeft.TabStop = false;
            this.groupboxLeft.Text = "左";
            this.groupboxLeft.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb3DLocalLeftIP
            // 
            this.tb3DLocalLeftIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb3DLocalLeftIP.Location = new System.Drawing.Point(469, 39);
            this.tb3DLocalLeftIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tb3DLocalLeftIP.MinimumSize = new System.Drawing.Size(1, 1);
            this.tb3DLocalLeftIP.Name = "tb3DLocalLeftIP";
            this.tb3DLocalLeftIP.Padding = new System.Windows.Forms.Padding(1);
            this.tb3DLocalLeftIP.ShowText = false;
            this.tb3DLocalLeftIP.Size = new System.Drawing.Size(177, 39);
            this.tb3DLocalLeftIP.Style = Sunny.UI.UIStyle.Custom;
            this.tb3DLocalLeftIP.TabIndex = 3;
            this.tb3DLocalLeftIP.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb3DLaserDownPort
            // 
            this.tb3DLaserDownPort.Location = new System.Drawing.Point(0, 0);
            this.tb3DLaserDownPort.Name = "tb3DLaserDownPort";
            this.tb3DLaserDownPort.Size = new System.Drawing.Size(100, 21);
            this.tb3DLaserDownPort.TabIndex = 0;
            // 
            // uiLabel14
            // 
            this.uiLabel14.AutoSize = true;
            this.uiLabel14.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel14.Location = new System.Drawing.Point(27, 350);
            this.uiLabel14.Name = "uiLabel14";
            this.uiLabel14.Size = new System.Drawing.Size(130, 21);
            this.uiLabel14.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel14.TabIndex = 75;
            this.uiLabel14.Text = "CheckMinLength:";
            this.uiLabel14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel15
            // 
            this.uiLabel15.AutoSize = true;
            this.uiLabel15.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel15.Location = new System.Drawing.Point(27, 430);
            this.uiLabel15.Name = "uiLabel15";
            this.uiLabel15.Size = new System.Drawing.Size(181, 21);
            this.uiLabel15.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel15.TabIndex = 75;
            this.uiLabel15.Text = "CheckMinVerticalLength:";
            this.uiLabel15.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxCheckMinVerticalLength
            // 
            this.textBoxCheckMinVerticalLength.Location = new System.Drawing.Point(230, 430);
            this.textBoxCheckMinVerticalLength.Name = "textBoxCheckMinVerticalLength";
            this.textBoxCheckMinVerticalLength.Size = new System.Drawing.Size(204, 29);
            this.textBoxCheckMinVerticalLength.TabIndex = 73;
            // 
            // textBoxCheckMinLength
            // 
            this.textBoxCheckMinLength.Location = new System.Drawing.Point(230, 350);
            this.textBoxCheckMinLength.Name = "textBoxCheckMinLength";
            this.textBoxCheckMinLength.Size = new System.Drawing.Size(204, 29);
            this.textBoxCheckMinLength.TabIndex = 73;
            // 
            // uiLabelLocalLeftIP
            // 
            this.uiLabelLocalLeftIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabelLocalLeftIP.Location = new System.Drawing.Point(0, 0);
            this.uiLabelLocalLeftIP.Name = "uiLabelLocalLeftIP";
            this.uiLabelLocalLeftIP.Size = new System.Drawing.Size(100, 23);
            this.uiLabelLocalLeftIP.TabIndex = 0;
            this.uiLabelLocalLeftIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabelLocalRightIP
            // 
            this.uiLabelLocalRightIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabelLocalRightIP.Location = new System.Drawing.Point(0, 0);
            this.uiLabelLocalRightIP.Name = "uiLabelLocalRightIP";
            this.uiLabelLocalRightIP.Size = new System.Drawing.Size(100, 23);
            this.uiLabelLocalRightIP.TabIndex = 0;
            this.uiLabelLocalRightIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabelLocalDownIP
            // 
            this.uiLabelLocalDownIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabelLocalDownIP.Location = new System.Drawing.Point(0, 0);
            this.uiLabelLocalDownIP.Name = "uiLabelLocalDownIP";
            this.uiLabelLocalDownIP.Size = new System.Drawing.Size(100, 23);
            this.uiLabelLocalDownIP.TabIndex = 0;
            this.uiLabelLocalDownIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabelLocalTopIP
            // 
            this.uiLabelLocalTopIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabelLocalTopIP.Location = new System.Drawing.Point(0, 0);
            this.uiLabelLocalTopIP.Name = "uiLabelLocalTopIP";
            this.uiLabelLocalTopIP.Size = new System.Drawing.Size(100, 23);
            this.uiLabelLocalTopIP.TabIndex = 0;
            this.uiLabelLocalTopIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb3DLaserTopPort
            // 
            this.tb3DLaserTopPort.Location = new System.Drawing.Point(0, 0);
            this.tb3DLaserTopPort.Name = "tb3DLaserTopPort";
            this.tb3DLaserTopPort.Size = new System.Drawing.Size(100, 21);
            this.tb3DLaserTopPort.TabIndex = 0;
            // 
            // tb3DLaserRightPort
            // 
            this.tb3DLaserRightPort.Location = new System.Drawing.Point(0, 0);
            this.tb3DLaserRightPort.Name = "tb3DLaserRightPort";
            this.tb3DLaserRightPort.Size = new System.Drawing.Size(100, 21);
            this.tb3DLaserRightPort.TabIndex = 0;
            // 
            // tbMoveControlPort
            // 
            this.tbMoveControlPort.Location = new System.Drawing.Point(453, 39);
            this.tbMoveControlPort.Name = "tbMoveControlPort";
            this.tbMoveControlPort.Size = new System.Drawing.Size(177, 29);
            this.tbMoveControlPort.TabIndex = 3;
            // 
            // tbMoveControlIP
            // 
            this.tbMoveControlIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbMoveControlIP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbMoveControlIP.Location = new System.Drawing.Point(118, 39);
            this.tbMoveControlIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbMoveControlIP.MinimumSize = new System.Drawing.Size(1, 16);
            this.tbMoveControlIP.Name = "tbMoveControlIP";
            this.tbMoveControlIP.Padding = new System.Windows.Forms.Padding(5);
            this.tbMoveControlIP.ShowText = false;
            this.tbMoveControlIP.Size = new System.Drawing.Size(177, 29);
            this.tbMoveControlIP.Style = Sunny.UI.UIStyle.Custom;
            this.tbMoveControlIP.TabIndex = 1;
            this.tbMoveControlIP.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel9
            // 
            this.uiLabel9.AutoSize = true;
            this.uiLabel9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiLabel9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel9.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.uiLabel9.Location = new System.Drawing.Point(368, 39);
            this.uiLabel9.Name = "uiLabel9";
            this.uiLabel9.Size = new System.Drawing.Size(41, 21);
            this.uiLabel9.TabIndex = 64;
            this.uiLabel9.Text = "Port:";
            this.uiLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel8
            // 
            this.uiLabel8.AutoSize = true;
            this.uiLabel8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiLabel8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel8.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.uiLabel8.Location = new System.Drawing.Point(23, 39);
            this.uiLabel8.Name = "uiLabel8";
            this.uiLabel8.Size = new System.Drawing.Size(26, 21);
            this.uiLabel8.TabIndex = 65;
            this.uiLabel8.Text = "IP:";
            this.uiLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.uiLabel8);
            this.uiGroupBox1.Controls.Add(this.uiLabel9);
            this.uiGroupBox1.Controls.Add(this.tbMoveControlIP);
            this.uiGroupBox1.Controls.Add(this.tbMoveControlPort);
            this.uiGroupBox1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiGroupBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.uiGroupBox1.Location = new System.Drawing.Point(30, 643);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox1.Size = new System.Drawing.Size(816, 91);
            this.uiGroupBox1.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox1.TabIndex = 53;
            this.uiGroupBox1.TabStop = false;
            this.uiGroupBox1.Text = "PLC";
            this.uiGroupBox1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.numericUpDownTDLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel13);
            this.uiGroupBox2.Controls.Add(this.numericUpDownLRLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel12);
            this.uiGroupBox2.Controls.Add(this.numericUpDownRTLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel11);
            this.uiGroupBox2.Controls.Add(this.numericUpDownLTLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel10);
            this.uiGroupBox2.Controls.Add(this.textBoxCheckMinLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel14);
            this.uiGroupBox2.Controls.Add(this.textBoxCheckMinVerticalLength);
            this.uiGroupBox2.Controls.Add(this.uiLabel15);
            this.uiGroupBox2.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiGroupBox2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiGroupBox2.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.uiGroupBox2.Location = new System.Drawing.Point(1037, 69);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiGroupBox2.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Padding = new System.Windows.Forms.Padding(0, 32, 0, 0);
            this.uiGroupBox2.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.uiGroupBox2.Size = new System.Drawing.Size(971, 551);
            this.uiGroupBox2.Style = Sunny.UI.UIStyle.Custom;
            this.uiGroupBox2.TabIndex = 54;
            this.uiGroupBox2.TabStop = false;
            this.uiGroupBox2.Text = "修正";
            this.uiGroupBox2.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // numericUpDownTDLength
            // 
            this.numericUpDownTDLength.DecimalPlaces = 2;
            this.numericUpDownTDLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDownTDLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownTDLength.Location = new System.Drawing.Point(167, 274);
            this.numericUpDownTDLength.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownTDLength.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownTDLength.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.numericUpDownTDLength.MinimumSize = new System.Drawing.Size(100, 0);
            this.numericUpDownTDLength.Name = "numericUpDownTDLength";
            this.numericUpDownTDLength.Size = new System.Drawing.Size(204, 29);
            this.numericUpDownTDLength.TabIndex = 56;
            this.numericUpDownTDLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // uiLabel13
            // 
            this.uiLabel13.AutoSize = true;
            this.uiLabel13.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel13.Location = new System.Drawing.Point(27, 274);
            this.uiLabel13.Name = "uiLabel13";
            this.uiLabel13.Size = new System.Drawing.Size(80, 21);
            this.uiLabel13.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel13.TabIndex = 55;
            this.uiLabel13.Text = "TDLength:";
            this.uiLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownLRLength
            // 
            this.numericUpDownLRLength.DecimalPlaces = 2;
            this.numericUpDownLRLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDownLRLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownLRLength.Location = new System.Drawing.Point(167, 200);
            this.numericUpDownLRLength.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownLRLength.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownLRLength.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.numericUpDownLRLength.MinimumSize = new System.Drawing.Size(100, 0);
            this.numericUpDownLRLength.Name = "numericUpDownLRLength";
            this.numericUpDownLRLength.Size = new System.Drawing.Size(204, 29);
            this.numericUpDownLRLength.TabIndex = 57;
            this.numericUpDownLRLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // uiLabel12
            // 
            this.uiLabel12.AutoSize = true;
            this.uiLabel12.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel12.Location = new System.Drawing.Point(27, 200);
            this.uiLabel12.Name = "uiLabel12";
            this.uiLabel12.Size = new System.Drawing.Size(79, 21);
            this.uiLabel12.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel12.TabIndex = 53;
            this.uiLabel12.Text = "LRLength:";
            this.uiLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownRTLength
            // 
            this.numericUpDownRTLength.DecimalPlaces = 2;
            this.numericUpDownRTLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDownRTLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownRTLength.Location = new System.Drawing.Point(167, 127);
            this.numericUpDownRTLength.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownRTLength.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownRTLength.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.numericUpDownRTLength.MinimumSize = new System.Drawing.Size(100, 0);
            this.numericUpDownRTLength.Name = "numericUpDownRTLength";
            this.numericUpDownRTLength.Size = new System.Drawing.Size(204, 29);
            this.numericUpDownRTLength.TabIndex = 52;
            this.numericUpDownRTLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // uiLabel11
            // 
            this.uiLabel11.AutoSize = true;
            this.uiLabel11.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel11.Location = new System.Drawing.Point(27, 127);
            this.uiLabel11.Name = "uiLabel11";
            this.uiLabel11.Size = new System.Drawing.Size(78, 21);
            this.uiLabel11.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel11.TabIndex = 51;
            this.uiLabel11.Text = "RTLength:";
            this.uiLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownLTLength
            // 
            this.numericUpDownLTLength.DecimalPlaces = 2;
            this.numericUpDownLTLength.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.numericUpDownLTLength.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDownLTLength.Location = new System.Drawing.Point(167, 49);
            this.numericUpDownLTLength.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.numericUpDownLTLength.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDownLTLength.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            -2147418112});
            this.numericUpDownLTLength.MinimumSize = new System.Drawing.Size(100, 0);
            this.numericUpDownLTLength.Name = "numericUpDownLTLength";
            this.numericUpDownLTLength.Size = new System.Drawing.Size(204, 29);
            this.numericUpDownLTLength.TabIndex = 50;
            this.numericUpDownLTLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // uiLabel10
            // 
            this.uiLabel10.AutoSize = true;
            this.uiLabel10.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel10.Location = new System.Drawing.Point(27, 49);
            this.uiLabel10.Name = "uiLabel10";
            this.uiLabel10.Size = new System.Drawing.Size(76, 21);
            this.uiLabel10.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel10.TabIndex = 48;
            this.uiLabel10.Text = "LTLength:";
            this.uiLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SettingParamPage
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.ClientSize = new System.Drawing.Size(1940, 1087);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.uiPLCMoveParameters);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this._rightBtn);
            this.Name = "SettingParamPage";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 1004;
            this.ShowTitle = true;
            this.Style = Sunny.UI.UIStyle.Custom;
            this.Text = "配置";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.uiPLCMoveParameters.ResumeLayout(false);
            this.groupBoxDown.ResumeLayout(false);
            this.groupBoxDown.PerformLayout();
            this.groupBoxTop.ResumeLayout(false);
            this.groupBoxTop.PerformLayout();
            this.groupBoxRight.ResumeLayout(false);
            this.groupBoxRight.PerformLayout();
            this.groupboxLeft.ResumeLayout(false);
            this.groupboxLeft.PerformLayout();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTDLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLRLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRTLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownLTLength)).EndInit();
            this.ResumeLayout(false);

        }

        private void numericUpDownTDLength_ValueChanged(object sender, double value)
        {
            float fValue = numericUpDownTDLength.Text.ToFloat();

            fValue -= 0.01f;
            numericUpDownTDLength.Text = fValue.ToString("0.01");
        }





        private void numericUpDownTDLength_KeyDown(object sender, KeyEventArgs e)
        {
            float fValue = numericUpDownTDLength.Text.ToFloat();

            fValue -= 0.01f;
            numericUpDownTDLength.Text = fValue.ToString("0.01");
        }

        private void numericUpDownTDLength_KeyUp(object sender, KeyEventArgs e)
        {
            float fValue = numericUpDownTDLength.Text.ToFloat();

            fValue += 0.01f;
            numericUpDownTDLength.Text = fValue.ToString("0.01");
        }

        private void _rightBtn_Click(object sender, System.EventArgs e)
        {
            FormMain.formMainF.showAsideFunc();


            SettingParamPage.instance.ShowRightBtnFunc(false);
            //MainStatisticPage.instance.ShowRightBtnFunc(false);


        }

        #endregion
        private UIButton button2;
        private UIButton button1;
        private UIButton _rightBtn;
        private UILabel uiLabel29;

        private UILabel label4;
        private System.Windows.Forms.TextBox tb3DLaserLeftPort;
        private UIGroupBox uiPLCMoveParameters;
        private UILabel uiLabel6;
        private UILabel uiLabel7;
        private System.Windows.Forms.TextBox tb3DLaserDownPort;
        private UILabel uiLabel4;
        private UILabel uiLabel5;
        private System.Windows.Forms.TextBox tb3DLaserTopPort;
        private UILabel uiLabel2;
        private UILabel uiLabel3;
        private UIIPTextBox tb3DLaserLeftIP;
        private UIIPTextBox tb3DLaserDownIP;
        private UIIPTextBox tb3DLaserTopIP;
        private UIIPTextBox tb3DLaserRightIP;
        private UIIPTextBox tb3DLocalLeftIP;
        private UIIPTextBox tb3DLocalRightIP;
        private UIIPTextBox tb3DLocalTopIP;
        private UIIPTextBox tb3DLocalDownIP;



        private System.Windows.Forms.TextBox tb3DLaserRightPort;
        private UIGroupBox groupboxLeft;
        private UIGroupBox groupBoxDown;
        private UIGroupBox groupBoxTop;
        private UIGroupBox groupBoxRight;
        private System.Windows.Forms.TextBox tbMoveControlPort;
        private UIIPTextBox tbMoveControlIP;
        private Label uiLabel9;
        private Label uiLabel8;
        private UIGroupBox uiGroupBox1;
        private UIGroupBox uiGroupBox2;
        private UILabel uiLabel10;
        private NumericUpDown numericUpDownTDLength;
        private NumericUpDown numericUpDownLRLength;
        private NumericUpDown numericUpDownRTLength;
        private NumericUpDown numericUpDownLTLength;
        private System.Windows.Forms.TextBox textBoxCheckMinLength;
        private System.Windows.Forms.TextBox textBoxCheckMinVerticalLength;

        private UILabel uiLabel13;
        private UILabel uiLabel12;
        private UILabel uiLabel11;
        private UILabel uiLabel14;
        private UILabel uiLabel15;
        private UILabel uiLabelLocalLeftIP;
        private UILabel uiLabelLocalRightIP;
        private UILabel uiLabelLocalTopIP;
        private UILabel uiLabelLocalDownIP;



    }
}