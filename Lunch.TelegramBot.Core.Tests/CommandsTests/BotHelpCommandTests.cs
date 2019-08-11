using System.Collections.Generic;
using System.Threading;
using Lunch.TelegramBot.Core.Commands;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Lunch.TelegramBot.Core.Tests.CommandsTests
{
    [TestFixture]
    internal class BotHelpCommandTests
    {
        private readonly Command _command;
        private readonly Mock<ITelegramBotClient> _botClientMock;

        public BotHelpCommandTests()
        {
            var settings = new CommandSettings
            {
             //   Name = "Lunch.TelegramBot.Core.Commands.BotHelpCommand, Lunch.TelegramBot.Core",
             //   Order = 1,
                TriggerWords = new[] { "/help" }
            };

            _command = new BotHelpCommand(settings, new List<Command>());
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
                Text = "/help",
                Chat = new Chat { Id = 123 }
            };
            _botClientMock.Setup(t => t.SendTextMessageAsync(message.Chat.Id, _command.Help, It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()));
            bool result = _command.ExecuteAsync(_botClientMock.Object, message).Result;

            Assert.False(result);
            _botClientMock.Verify();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _command.Dispose();
        }
    }
}
