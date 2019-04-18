using System;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Main.Dependency;
using Ninject.Parameters;

namespace Lunch.TelegramBot.Main.Forms
{
    public partial class FormMain : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FormMain));

        public FormMain()
        {
            InitializeComponent();
        }

        #region Events Handlers

        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
            else
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            notifyIcon.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) =>
            notifyIcon_MouseDoubleClick(null, null);

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) =>
            Close();

        private void btnTryLunchCommand_Click(object sender, EventArgs e)
        {
            foreach (var command in CompositionRoot.Commands)
            {
                command.IsExecutableNow(isLoggable: true);
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            SetControlsEnable(false);
            var bot = CompositionRoot.Resolve<Core.Api.TelegramBot>();
            await bot.InitializeAsync();

            var botInfo = await bot.GetBotInfoAsync();
            lblInfo.Text =
$@"First Name: {botInfo.FirstName}
Last name: {botInfo.LastName}
Username: {botInfo.Username}
LanguageCode: {botInfo.LanguageCode}
Id: {botInfo.Id}";

            SetControlsEnable(true);
            btnStart.Enabled = false;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
        }

        #endregion Events Handlers

        private void SetControlsEnable(bool value)
        {
            btnClearLog.Enabled = value;
            btnStart.Enabled = value;
            btnTryIsCommandsExecutable.Enabled = value;
            rtbLog.Enabled = value;
        }
    }
}
