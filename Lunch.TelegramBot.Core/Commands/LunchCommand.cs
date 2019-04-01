using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Common.Extensions;
using Lunch.TelegramBot.Core.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public class LunchCommand : Command
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LunchCommand));
        private static readonly string[] LunchPhrases =
        {
            "Пора на обед!",
            "Работа работой, а обед по расписанию!",
            "Откладываем баги и идём обедать :)",
            "Обед! Обед! Обед!",
            "После хорошего обеда можно простить кого угодно. Даже тех, кто сделал плохой обед.",
            "До обеда я ничего не соображаю… Это не мое астральное время! Сейчас моя карма на подзарядке, мозги в починке, а я сам в отключке...",
            "Работа - это такое место, где до обеда хочется есть, после обеда - спать, и всё время преследует чувство, что тебе мало платят.",
            "Демократия — это два волка и один ягненок, ставящие на голосование меню обеда.",
            "Убийство — всегда промах. Никогда не следует делать того, о чём нельзя поболтать с людьми после обеда.",
            "Любой может испортить обед. Но повар делает это профессионально.",
            "Чай и бутерброд с колбасой всегда делают жизнь чуть более лёгкой, а неразрешимые вопросы чуть более простыми.",
            "Доброе утро начнётся с обеда.",
            "*(Завтрак++)! *(Завтрак++)! *(Завтрак++)!",
            "Диета завтра, обед - сегодня. Читай каждый день.",
        };

        public LunchCommand(CommandSettings settings) : base(settings)
        {
        }

        public override string Help => $"В рабочие дни напоминает про обед в {Settings.Time:HH:mm:ss}";

        public override string GetName() => "Lunch";

        public override async Task<bool> ExecuteAsync(TelegramBotClient bot, Message m)
        {
            if (!IsExecutableNow()) return true;
            Logger.Info($"{nameof(LunchCommand)} execute");
            await bot.SendTextMessageAsync(m.Chat.Id, LunchPhrases.GetRandom()).ConfigureAwait(false);
            return true;
        }
    }
}
