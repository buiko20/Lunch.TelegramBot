using System;
using System.Windows.Forms;
using log4net;
using Lunch.TelegramBot.Dependency;

namespace Lunch.TelegramBot.Forms
{
    public partial class FormMain : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FormMain));

        public FormMain()
        {
            InitializeComponent();
        }

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

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            SetControlsEnable(false);
            var bot = CompositionRoot.Resolve<Core.Bot.TelegramBot>();
            await bot.InitializeAsync();

            var botInfo = await bot.GetBotInfoAsync();
            lblBotInfo.Text =
$@"First Name: {botInfo.FirstName}
Last name: {botInfo.LastName}
Username: {botInfo.Username}
LanguageCode: {botInfo.LanguageCode}
Id: {botInfo.Id}";

            SetControlsEnable(true);
            btnStart.Enabled = false;
        }

        private void SetControlsEnable(bool value)
        {
            btnStart.Enabled = value;
            rtbLog.Enabled = value;
        }
    }
}
