using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Core;
using Lunch.TelegramBot.Core.Api;
using Lunch.TelegramBot.Core.Commands;
using Lunch.TelegramBot.Main.Forms;
using Ninject;
using Ninject.Parameters;

namespace Lunch.TelegramBot.Main.Dependency
{
    internal static class CompositionRoot
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CompositionRoot));
        private static readonly IKernel Kernel;

        static CompositionRoot()
        {
            Kernel = new StandardKernel();
            RegisterDependency(Kernel);
            RegisterForms(Kernel);
        }

        public static void Dispose()
        {
            try
            {
                Kernel.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error("Kernel dispose error", e);
            }
        }

        public static T Resolve<T>()
        {
            return Kernel.Get<T>();
        }

        public static T Resolve<T>(params IParameter[] parameters)
        {
            return Kernel.Get<T>(parameters);
        }

        public static void Register<T>(T obj)
        {
            Kernel.GetBindings(typeof(T))
                .ToList()
                .ForEach(binding => Kernel.RemoveBinding(binding));

            Kernel.Bind<T>().ToConstant(obj);
        }

        private static void RegisterDependency(IKernel kernel)
        {
            var commands = GetCommands();
            var settings = ConfigUtils.ReadBotSettings();
            Register<Core.Api.TelegramBot>(new LunchBot(settings, commands));
        }

        private static void RegisterForms(IKernel kernel)
        {
            kernel.Bind<FormMain>().To<FormMain>();
        }

        private static List<Command> GetCommands()
        {
            var settings = ConfigUtils.ReadBotSettings();
            var commands = typeof(Command).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)))
                .Select(t =>
                {
                    var getName = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), null, t.GetMethod("GetName"));
                    var setting = settings.CommandSettings.SingleOrDefault(
                        s => s.Name.Equals(getName(), StringComparison.OrdinalIgnoreCase));
                    return (Command) Activator.CreateInstance(t, setting);
                })
                .ToArray();

            var help = (HelpCommand)commands.Single(c => c.GetName().Equals("Help", StringComparison.OrdinalIgnoreCase));
            var result = commands.OrderBy(_ => _?.Settings?.Order).ToList();
            help.Commands = result;
            return result;
        }
    }
}
