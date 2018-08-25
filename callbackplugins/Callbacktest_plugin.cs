// Copyright (c) 2014-2018, Els_kom org.
// https://github.com/Elskom/
// All rights reserved.
// license: MIT, see LICENSE for more details.

namespace callbacktest_plugin
{
    using System.Windows.Forms;
    using Els_kom_Core.Classes;
    using Els_kom_Core.Interfaces;

    public class Callbacktest_plugin : ICallbackPlugin
    {
        public string PluginName => "Callback Test Plugin";
        public bool SupportsSettings => true;
        public bool ShowModal => true;
        public Form SettingsWindow => new CallbacktestForm();

        public void TestModsCallback()
        {
            SettingsFile.Settingsxml.ReopenFile();
            int.TryParse(SettingsFile.Settingsxml.Read("ShowTestMessages"), out var callbacksetting1);
            if (callbacksetting1 > 0)
            {
                MessageBox.Show(
                    "Testing this callback interface.",
                    "Info!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
