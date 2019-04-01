using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Lunch.TelegramBot.Common.Configuration
{
    public static class ConfigUtils
    {
        private static readonly string SettingsFilePath;

        static ConfigUtils()
        {
            SettingsFilePath = Path.GetFullPath("BotSettings.config");
        }

        public static BotSettings ReadBotSettings()
        {
            var result = new BotSettings();
            var document = CreateXmlDocument(SettingsFilePath);

            result.InitializationDelay = int.Parse(GetAttribute(document, nodeName: "InitializationDelay").Value);
            result.Key = GetAttribute(document, nodeName: "Key").Value;
            result.ClientAccessToken = GetAttribute(document, nodeName: "ClientAccessToken").Value;
            result.CommandSettings.AddRange(ReadCommandSettings(document).OrderBy(_ => _.Order));

            return result;
        }

        private static XmlDocument CreateXmlDocument(string path)
        {
            var document = new XmlDocument();
            document.Load(path);
            return document;
        }

        private static IEnumerable<CommandSettings> ReadCommandSettings(XmlDocument document)
        {
            var result = new List<CommandSettings>();
            foreach (var commandsNode in GetCommandsNodes(document))
            {
                string name = commandsNode.Attributes.GetNamedItem("Name")?.Value;
                bool.TryParse(commandsNode.Attributes.GetNamedItem("Enabled")?.Value, out bool enabled);
                DateTime.TryParse(commandsNode.Attributes.GetNamedItem("Time")?.Value, out DateTime time);
                int.TryParse(commandsNode.Attributes.GetNamedItem("Order")?.Value, out int order);
                string daysToExcludeValue = commandsNode.Attributes.GetNamedItem("DaysToExclude")?.Value;
                var daysToExclude = new List<DayOfWeek>();
                if (daysToExcludeValue != null)
                {
                    string[] temp = daysToExcludeValue.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    daysToExclude.AddRange(temp.Select(day => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day, ignoreCase: true)));
                }

                result.Add(new CommandSettings(name, enabled, time, order, daysToExclude));
            }

            return result;
        }

        private static IEnumerable<XmlNode> GetCommandsNodes(XmlDocument document)
        {
            var commandsNode = document.SelectSingleNode("BotSettings/Commands");
            var nodes = commandsNode.SelectNodes("Command");
            return nodes.Cast<XmlNode>().ToArray();
        }

        private static XmlNode GetAttribute(XmlDocument document, string nodeName)
        {
            var node = document.SelectSingleNode($"BotSettings/{nodeName}");
            return node.Attributes.GetNamedItem("Value");
        }
    }
}
