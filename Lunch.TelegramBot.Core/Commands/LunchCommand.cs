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
            "После хорошего обеда можно простить кого угодно. Даже тех, кто сделал плохой обед. Так что идём обедать!",
            "До обеда я ничего не соображаю… Это не мое астральное время! Сейчас моя карма на подзарядке, мозги в починке, а я сам в отключке... Время включаться!",
            "Работа - это такое место, где до обеда хочется есть, после обеда - спать, и всё время преследует чувство, что тебе мало платят. Забудем про это чувство на время обеда.",
            "Демократия — это два волка и один ягненок, ставящие на голосование меню обеда или что там у нас сегодня?",
            "Убийство — всегда промах. Никогда не следует делать того, о чём нельзя поболтать с людьми после обеда. Готовьте истории - время обедать!",
            "Чай и бутерброд с колбасой всегда делают жизнь чуть более лёгкой, а неразрешимые вопросы чуть более простыми. Сейчас время лёгкой жизни и простых вопросов!",
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
