using Lunch.TelegramBot.Core.Commands;
using Moq;
using NUnit.Framework;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Lunch.TelegramBot.Core.Tests.CommandsTests
{
    [TestFixture]
    internal class ApiSdkCommandTests
    {
        private readonly Command _command;
        private readonly Mock<ITelegramBotClient> _botClientMock;

        public ApiSdkCommandTests()
        {
            var settings = new CommandSettings
            {
              //  Name = "Lunch.TelegramBot.Core.Commands.ApiSdkCommand, Lunch.TelegramBot.Core",
              //  Order = 4,
                Data = "440c227eeaa5460e8fdde00fe5bbf31e",
                TriggerWords = new[] { "/бот", "/bot" }
            };

            _command = new ApiSdkCommand(settings);
            _botClientMock = new Mock<ITelegramBotClient>(MockBehavior.Default) { Name = "TelegramBotClientMock" };
        }

        [Test]
        public void HelpShouldNotBeEmpty()
        {
            Assert.False(string.IsNullOrWhiteSpace(_command.Help));
        }

        [Test]
        public void CommandShouldBeAlwaysExecutable()
        {
            Assert.True(_command.IsExecutableNow());
        }

        [Test]
        public void CommandResultNotEmpty()
        {
            _botClientMock.Reset();

            var message = new Message
            {
                Text = "/bot, привет!",
                Chat = new Chat { Id = 123 }
            };
            _botClientMock.Setup(t => t.SendTextMessageAsync(message.Chat.Id, _command.Help, It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()));
            bool result = _command.ExecuteAsync(_botClientMock.Object, message).Result;

            Assert.True(result);
            _botClientMock.Verify();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _command.Dispose();
        }
    }
}
