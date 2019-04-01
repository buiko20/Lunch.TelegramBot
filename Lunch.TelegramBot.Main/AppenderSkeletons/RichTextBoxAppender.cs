using System;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Core;

namespace Lunch.TelegramBot.Main.AppenderSkeletons
{
    public class RichTextBoxAppender : AppenderSkeleton
    {
        private RichTextBox _rtb;
        private Form _form;

        public string FormName { get; set; }
        public string RtbName { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (_rtb == null)
            {
                if (string.IsNullOrWhiteSpace(FormName) ||
                    string.IsNullOrWhiteSpace(RtbName))
                    return;

                var _form = Application.OpenForms[FormName];
                if (_form == null) return;

                _rtb = _form.Controls[RtbName] as RichTextBox;
                if (_rtb == null) return;

                _form.Closing += (sender, args) => _rtb = null;
            }

            void Write() => _rtb.AppendText($"{loggingEvent.TimeStamp}: {loggingEvent.RenderedMessage}{Environment.NewLine}");
            _form = Application.OpenForms[FormName];
            _form?.Invoke((Action) Write);
        }
    }
}
