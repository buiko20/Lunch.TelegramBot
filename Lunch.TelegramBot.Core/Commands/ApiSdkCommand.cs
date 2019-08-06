using System.Threading.Tasks;
using ApiAiSDK;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public sealed class ApiSdkCommand : Command
    {
        private readonly ApiAi _apiAi;

        public ApiSdkCommand(CommandSettings settings) : base(settings)
        {
            var aiConfiguration = new AIConfiguration(settings.Token, SupportedLanguage.Russian);
            _apiAi = new ApiAi(aiConfiguration);
        }

        public override string Help => @"/бот — обращение к боту";

        protected override async Task<bool> ExecuteInternalAsync(TelegramBotClient bot, Message m)
        {
            if (IsMessageForCommand(m))
            {
                var response = _apiAi.TextRequest(m.Text);
                string answer = response.Result.Fulfillment.Speech;
                if (string.IsNullOrWhiteSpace(answer))
                {
                    answer = "Я не знаю...";
                }

                await bot.SendTextMessageAsync(m.Chat.Id, answer).ConfigureAwait(false);
                return false;
            }

            return true;
        }
    }
}
