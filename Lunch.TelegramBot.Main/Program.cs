using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Main.Dependency;
using Lunch.TelegramBot.Main.Forms;

namespace Lunch.TelegramBot.Main
{
    internal static class Program
    {
        private static ILog _logger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                ConfigureLog4Net();
                _logger = LogManager.GetLogger(typeof(FormMain));

                VerifyConfigFile();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(CompositionRoot.Resolve<FormMain>());
            }
            catch (Exception e)
            {
                _logger.Error("Error in Application", e);
                MessageBox.Show(e.Message, @"Error in Application", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                CompositionRoot.Dispose();
            }
        }

        private static void ConfigureLog4Net()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var logConfigPath = new System.IO.FileInfo(@"log4net.config");
            XmlConfigurator.Configure(logRepository, logConfigPath);
        }

        private static void VerifyConfigFile()
        {
            var settings = ConfigUtils.ReadBotSettings();
        }
    }
}
