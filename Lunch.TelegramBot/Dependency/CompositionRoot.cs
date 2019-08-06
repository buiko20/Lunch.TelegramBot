using System;
using System.Linq;
using log4net;
using Lunch.TelegramBot.Core.Bot;
using Lunch.TelegramBot.Core.Commands;
using Lunch.TelegramBot.Forms;
using Ninject;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Lunch.TelegramBot.Dependency
{
    internal static class CompositionRoot
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CompositionRoot));
        private static readonly IKernel Kernel;

        static CompositionRoot()
        {
            Kernel = new StandardKernel();
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

        public static void RegisterDependencies()
        {
            var settings = Resolve<BotSettings>();
            var commands = Kernel.GetAll<Command>().ToList();
            Register<Core.Bot.TelegramBot>(new LunchBot(settings, commands));

            RegisterForms(Kernel);
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
            Kernel.GetBindings(obj.GetType())
                .ToList()
                .ForEach(binding => Kernel.RemoveBinding(binding));

            Kernel.Bind<T>().ToConstant(obj);
        }

        private static void RegisterForms(IBindingRoot kernel)
        {
            kernel.Bind<FormMain>().To<FormMain>();
        }
    }
}
