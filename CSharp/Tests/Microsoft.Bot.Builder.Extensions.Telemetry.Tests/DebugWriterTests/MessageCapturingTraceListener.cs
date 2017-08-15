using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests.DebugWriterTests
{
    public class MessageCapturingTraceListener : TraceListener
    {
        private readonly IList<string> _capturedMessages;

        public IEnumerable<string> CapturedMessages => new ReadOnlyCollection<string>(_capturedMessages);

        public MessageCapturingTraceListener()
        {
            _capturedMessages = new List<string>();
        }

        public override void Write(string message)
        {
            var lines = message.Split("\n".ToCharArray());

            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    _capturedMessages.Add(line);
                }
            }
        }

        public override void WriteLine(string message)
        {
            Write($"{message}\n");
        }
    }
}