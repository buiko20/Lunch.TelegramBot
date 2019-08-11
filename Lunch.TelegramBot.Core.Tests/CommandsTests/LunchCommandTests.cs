using System;
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
    internal class LunchCommandTests
    {
        private readonly Command _command;
        private readonly Mock<ITelegramBotClient> _botClientMock;

        public LunchCommandTests()
        {
            var settings = new CommandSettings
            {
              //  Name = "Lunch.TelegramBot.Core.Commands.LunchCommand, Lunch.TelegramBot.Core",
              //  Order = 2,
              //  Time = TimeSpan.Parse("11:55:00"),
              //  DaysToExclude = new[] { DayOfWeek.Sunday, DayOfWeek.Saturday },
                Data = "[ \"Пора на обед!\" ]"
            };

            _command = new LunchCommand(settings);
            _botClientMock = new Mock<ITelegramBotClient>(MockBehavior.Default) { Name = "TelegramBotClientMock" };
        }

        [Test]
        public void HelpShouldNotBeEmpty()
        {
            _command.Settings.DaysToExclude = new[] { DateTime.Now.DayOfWeek };
            Assert.False(string.IsNullOrWhiteSpace(_command.Help));
        }

        [Test]
        public void CommandExecutableTests()
        {
            for (int i = 0; i < 3; i++)
            {
                _command.Settings.Time = DateTime.Now.TimeOfDay;
                _command.Settings.DaysToExclude = new DayOfWeek[0];
                Assert.True(_command.IsExecutableNow());

                _command.Settings.Time = DateTime.Now.TimeOfDay;
                _command.Settings.DaysToExclude = new[] { DateTime.Now.DayOfWeek };
                Assert.False(_command.IsExecutableNow());

                Thread.Sleep(4000);
            }
        }

        [Test]
        public void CommandResultNotEmpty()
        {
            _botClientMock.Reset();

            var message = new Message
            {
                Text = "123",
                Chat = new Chat { Id = 123 }
            };
            _botClientMock.Setup(t => t.SendTextMessageAsync(message.Chat.Id, It.IsAny<string>(), It.IsAny<ParseMode>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<IReplyMarkup>(), It.IsAny<CancellationToken>()));
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
