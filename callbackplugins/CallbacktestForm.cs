// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace callbacktest_plugin
{
    using System;
    using System.Windows.Forms;
    using Els_kom_Core.Classes;

    public partial class CallbacktestForm : Form
    {
        public CallbacktestForm() => InitializeComponent();
        internal int callbacksetting1;
        internal int callbacksetting1_temp;

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBox1.Checked == true)
            {
                callbacksetting1_temp = 1;
            }
            else if (callbacksetting1_temp > 0)
            {
                callbacksetting1_temp = 0;
            }
        }

        private void CallbacktestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsFile.Settingsxml.ReopenFile();
            if (callbacksetting1 != callbacksetting1_temp)
            {
                callbacksetting1 = callbacksetting1_temp;
                SettingsFile.Settingsxml.Write("ShowTestMessages", callbacksetting1.ToString());
            }
            SettingsFile.Settingsxml.Save();
        }

        private void CallbacktestForm_Load(object sender, EventArgs e)
        {
            callbacksetting1 = 0;
            callbacksetting1_temp = 0;
            SettingsFile.Settingsxml.ReopenFile();
            int.TryParse(SettingsFile.Settingsxml.Read("ShowTestMessages"), out callbacksetting1);
            if (callbacksetting1 > 0)
            {
                CheckBox1.Checked = true;
            }
        }

        private void Label1_Click(object sender, EventArgs e) => CheckBox1.Checked = CheckBox1.Checked ? false : true;
    }
}
