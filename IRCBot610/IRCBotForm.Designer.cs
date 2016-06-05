namespace IRCBot610
{
    partial class IRCBotForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IRCBotForm));
            this.InputBox = new System.Windows.Forms.TextBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.userBox = new System.Windows.Forms.ListBox();
            this.voiceOnBox = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.debugBox = new System.Windows.Forms.CheckBox();
            this.whitelistBox = new System.Windows.Forms.CheckBox();
            this.sayNamesBox = new System.Windows.Forms.CheckBox();
            this.blindBox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.waitBox = new System.Windows.Forms.TextBox();
            this.waitSlider = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.volumeBox = new System.Windows.Forms.TextBox();
            this.volumeSlider = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.rateBox = new System.Windows.Forms.TextBox();
            this.rateSlider = new System.Windows.Forms.TrackBar();
            this.ConsoleBox = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waitSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rateSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // InputBox
            // 
            this.InputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InputBox.Location = new System.Drawing.Point(12, 391);
            this.InputBox.Name = "InputBox";
            this.InputBox.Size = new System.Drawing.Size(417, 20);
            this.InputBox.TabIndex = 0;
            this.InputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputBox_KeyDown);
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.userBox);
            this.panel1.Location = new System.Drawing.Point(435, 111);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(114, 310);
            this.panel1.TabIndex = 5;
            // 
            // userBox
            // 
            this.userBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.userBox.FormattingEnabled = true;
            this.userBox.Location = new System.Drawing.Point(0, 0);
            this.userBox.Name = "userBox";
            this.userBox.Size = new System.Drawing.Size(114, 303);
            this.userBox.TabIndex = 6;
            // 
            // voiceOnBox
            // 
            this.voiceOnBox.AutoSize = true;
            this.voiceOnBox.Checked = true;
            this.voiceOnBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.voiceOnBox.Location = new System.Drawing.Point(226, 51);
            this.voiceOnBox.Name = "voiceOnBox";
            this.voiceOnBox.Size = new System.Drawing.Size(68, 17);
            this.voiceOnBox.TabIndex = 8;
            this.voiceOnBox.Text = "Voice on";
            this.voiceOnBox.UseVisualStyleBackColor = true;
            this.voiceOnBox.CheckedChanged += new System.EventHandler(this.voiceOnBox_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.debugBox);
            this.panel2.Controls.Add(this.whitelistBox);
            this.panel2.Controls.Add(this.sayNamesBox);
            this.panel2.Controls.Add(this.blindBox);
            this.panel2.Controls.Add(this.voiceOnBox);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.waitBox);
            this.panel2.Controls.Add(this.waitSlider);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.volumeBox);
            this.panel2.Controls.Add(this.volumeSlider);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.rateBox);
            this.panel2.Controls.Add(this.rateSlider);
            this.panel2.Location = new System.Drawing.Point(12, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(537, 93);
            this.panel2.TabIndex = 6;
            // 
            // debugBox
            // 
            this.debugBox.AutoSize = true;
            this.debugBox.Location = new System.Drawing.Point(6, 72);
            this.debugBox.Name = "debugBox";
            this.debugBox.Size = new System.Drawing.Size(87, 17);
            this.debugBox.TabIndex = 18;
            this.debugBox.Text = "Debug mode";
            this.debugBox.UseVisualStyleBackColor = true;
            // 
            // whitelistBox
            // 
            this.whitelistBox.AutoSize = true;
            this.whitelistBox.Location = new System.Drawing.Point(6, 51);
            this.whitelistBox.Name = "whitelistBox";
            this.whitelistBox.Size = new System.Drawing.Size(95, 17);
            this.whitelistBox.TabIndex = 17;
            this.whitelistBox.Text = "Whitelist mode";
            this.whitelistBox.UseVisualStyleBackColor = true;
            this.whitelistBox.CheckedChanged += new System.EventHandler(this.whitelistBox_CheckedChanged);
            // 
            // sayNamesBox
            // 
            this.sayNamesBox.AutoSize = true;
            this.sayNamesBox.Location = new System.Drawing.Point(226, 72);
            this.sayNamesBox.Name = "sayNamesBox";
            this.sayNamesBox.Size = new System.Drawing.Size(78, 17);
            this.sayNamesBox.TabIndex = 16;
            this.sayNamesBox.Text = "Say names";
            this.sayNamesBox.UseVisualStyleBackColor = true;
            this.sayNamesBox.CheckedChanged += new System.EventHandler(this.sayNamesBox_CheckedChanged);
            // 
            // blindBox
            // 
            this.blindBox.AutoSize = true;
            this.blindBox.Location = new System.Drawing.Point(116, 51);
            this.blindBox.Name = "blindBox";
            this.blindBox.Size = new System.Drawing.Size(78, 17);
            this.blindBox.TabIndex = 15;
            this.blindBox.Text = "Blind mode";
            this.blindBox.UseVisualStyleBackColor = true;
            this.blindBox.CheckedChanged += new System.EventHandler(this.blindBox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(223, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Speech delay";
            // 
            // waitBox
            // 
            this.waitBox.Location = new System.Drawing.Point(295, 2);
            this.waitBox.Name = "waitBox";
            this.waitBox.Size = new System.Drawing.Size(32, 20);
            this.waitBox.TabIndex = 14;
            this.waitBox.TextChanged += new System.EventHandler(this.waitBox_TextChanged);
            // 
            // waitSlider
            // 
            this.waitSlider.LargeChange = 15;
            this.waitSlider.Location = new System.Drawing.Point(223, 21);
            this.waitSlider.Maximum = 60;
            this.waitSlider.Name = "waitSlider";
            this.waitSlider.Size = new System.Drawing.Size(104, 45);
            this.waitSlider.TabIndex = 12;
            this.waitSlider.TickFrequency = 10;
            this.waitSlider.Scroll += new System.EventHandler(this.waitSlider_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(113, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Voice volume";
            // 
            // volumeBox
            // 
            this.volumeBox.Location = new System.Drawing.Point(185, 2);
            this.volumeBox.Name = "volumeBox";
            this.volumeBox.Size = new System.Drawing.Size(32, 20);
            this.volumeBox.TabIndex = 10;
            // 
            // volumeSlider
            // 
            this.volumeSlider.LargeChange = 4;
            this.volumeSlider.Location = new System.Drawing.Point(113, 21);
            this.volumeSlider.Maximum = 100;
            this.volumeSlider.Name = "volumeSlider";
            this.volumeSlider.Size = new System.Drawing.Size(104, 45);
            this.volumeSlider.TabIndex = 8;
            this.volumeSlider.TickFrequency = 10;
            this.volumeSlider.Scroll += new System.EventHandler(this.volumeSlider_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Voice speed";
            // 
            // rateBox
            // 
            this.rateBox.Location = new System.Drawing.Point(75, 2);
            this.rateBox.Name = "rateBox";
            this.rateBox.Size = new System.Drawing.Size(32, 20);
            this.rateBox.TabIndex = 7;
            this.rateBox.TextChanged += new System.EventHandler(this.rateBox_TextChanged);
            // 
            // rateSlider
            // 
            this.rateSlider.LargeChange = 4;
            this.rateSlider.Location = new System.Drawing.Point(3, 21);
            this.rateSlider.Minimum = -10;
            this.rateSlider.Name = "rateSlider";
            this.rateSlider.Size = new System.Drawing.Size(104, 45);
            this.rateSlider.TabIndex = 5;
            this.rateSlider.Scroll += new System.EventHandler(this.rateSlider_Scroll);
            // 
            // ConsoleBox
            // 
            this.ConsoleBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConsoleBox.Location = new System.Drawing.Point(12, 111);
            this.ConsoleBox.Name = "ConsoleBox";
            this.ConsoleBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.ConsoleBox.Size = new System.Drawing.Size(417, 274);
            this.ConsoleBox.TabIndex = 7;
            this.ConsoleBox.Text = "";
            this.ConsoleBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.ConsoleBox_LinkClicked);
            this.ConsoleBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ConsoleBox_MouseUp);
            // 
            // IRCBotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 423);
            this.Controls.Add(this.ConsoleBox);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.InputBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(370, 461);
            this.Name = "IRCBotForm";
            this.Text = "IRCBot610";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IRCBotForm_FormClosing);
            this.Load += new System.EventHandler(this.IRCBotForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waitSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.volumeSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rateSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox InputBox;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox userBox;
        private System.Windows.Forms.CheckBox voiceOnBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox waitBox;
        private System.Windows.Forms.TrackBar waitSlider;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox volumeBox;
        private System.Windows.Forms.TrackBar volumeSlider;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rateBox;
        private System.Windows.Forms.TrackBar rateSlider;
        private System.Windows.Forms.CheckBox blindBox;
        private System.Windows.Forms.RichTextBox ConsoleBox;
        private System.Windows.Forms.CheckBox sayNamesBox;
        private System.Windows.Forms.CheckBox whitelistBox;
        private System.Windows.Forms.CheckBox debugBox;
    }
}

