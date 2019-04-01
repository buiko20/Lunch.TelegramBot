using System;
using System.Runtime.Serialization;

namespace Lunch.TelegramBot.Common.Exceptions
{
    public class ConfigFileException : Exception
    {
        public ConfigFileException()
        {
        }

        public ConfigFileException(string message)
            : base(message)
        {
        }

        public ConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ConfigFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
