using System.Threading.Tasks;
using log4net;
using Lunch.TelegramBot.Common.Extensions;
using Lunch.TelegramBot.Common.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public class LunchCommand : Command
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LunchCommand));
        private readonly string[] _lunchPhrases;

        public LunchCommand(CommandSettings settings) : base(settings)
        {
            _lunchPhrases = ConfigUtils.DeserializeObject<string[]>(settings.Data.ToString());
        }

        public override string Help => $"В рабочие дни напоминает про обед в {Settings.Time.Value.To24Time()}";

        protected override async Task<bool> ExecuteInternalAsync(TelegramBotClient bot, Message m)
        {
            Logger.Info($"{nameof(LunchCommand)} execute");
            await bot.SendTextMessageAsync(m.Chat.Id, _lunchPhrases.GetRandom()).ConfigureAwait(false);
            return true;
        }
    }
}
