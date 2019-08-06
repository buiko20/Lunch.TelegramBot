using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using Lunch.TelegramBot.Common.Utils;
using Lunch.TelegramBot.Core.Bot;
using Lunch.TelegramBot.Core.Commands;
using Lunch.TelegramBot.Dependency;
using Lunch.TelegramBot.Forms;

namespace Lunch.TelegramBot
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
                RegisterBotSettings();
                RegisterCommands();
                CompositionRoot.RegisterDependencies();

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
            var logConfigPath = new FileInfo(@"log4net.config");
            XmlConfigurator.Configure(logRepository, logConfigPath);
        }

        private static void RegisterBotSettings()
        {
            string configData = File.ReadAllText(@"BotSettings.json");
            var settings = ConfigUtils.DeserializeObject<BotSettings>(configData);
            CompositionRoot.Register(settings);
        }

        private static void RegisterCommands()
        {
            var settings = CompositionRoot.Resolve<BotSettings>();
            var commands = new List<Command>(settings.CommandsSettings.Count);
            foreach (CommandSettings setting in settings.CommandsSettings)
            {
                var commandType = Type.GetType(setting.Name, throwOnError: true, ignoreCase: false);
                Command command;
                if (commandType == typeof(BotHelpCommand))
                    command = (Command)Activator.CreateInstance(commandType, setting, commands);
                else
                    command = (Command)Activator.CreateInstance(commandType, setting);
                commands.Add(command);
                CompositionRoot.Register(command);
            }
        }
    }
}
