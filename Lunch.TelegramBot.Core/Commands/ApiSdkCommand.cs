using System;
using System.Threading.Tasks;
using ApiAiSDK;
using Lunch.TelegramBot.Common.Configuration;
using Lunch.TelegramBot.Core.Api;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Lunch.TelegramBot.Core.Commands
{
    public sealed class ApiSdkCommand : Command
    {
        private readonly ApiAi _apiAi;

        public ApiSdkCommand(CommandSettings settings) : base(settings)
        {
            var botSettings = ConfigUtils.ReadBotSettings();
            if (settings.Enabled)
            {
                if (string.IsNullOrWhiteSpace(botSettings.ClientAccessToken))
                    throw new ArgumentException("ApiSDK key is empty.", nameof(botSettings.ClientAccessToken));
                var aiConfiguration = new AIConfiguration(botSettings.ClientAccessToken, SupportedLanguage.Russian);
                _apiAi = new ApiAi(aiConfiguration);
            }
        }

        public override string Help => @"/бот — обращение к боту";

        public override string GetName() => "ApiSdk";

        public override async Task<bool> ExecuteAsync(TelegramBotClient bot, Message m)
        {
            if (m.Text.IndexOf("/бот", StringComparison.OrdinalIgnoreCase) != -1)
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
