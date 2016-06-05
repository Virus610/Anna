using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IRCBot610
{
    public partial class PasswordForm : Form
    {
        IRCBotForm form;

        public PasswordForm(IRCBotForm form)
        {
            InitializeComponent();

            this.form = form;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            form.password = passwordBox.Text;
            form.nick = usernameBox.Text.ToLower();
            form.connect();
            Hide();
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OKButton.PerformClick();
            }
        }

        private void PasswordForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void PasswordForm_Load(object sender, EventArgs e)
        {
            try
            {
                usernameBox.Text = form.nick;
            }
            catch (Exception ex) { }
        }
    }
}
