using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Log activities to Service Bus Queue. 
    /// </summary>
    /// <remarks>
    /// Activities are limited to size of Service Bus queue payload.  Disposition of larger messages is controlled by QueueLoggerSettings.
    /// for more information on limits see https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted
    /// </remarks>
    public class ServiceBusActivityLogger : IActivityLogger
    {
        private readonly QueueClient _client;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        //controls how much message we will keep after the cut
        private readonly float _cutCoefficient;

        /// <summary>
        /// Constructs an instance of ServiceBusActivityLogger
        /// </summary>
        /// <param name="client">Reference to a QueueClient instance</param>
        /// <param name="queueSettings">Settings informing the logger how to handle large messages and whether compression is required</param>
        /// <param name="settings">JSON serialziation settings used to write the formatted JSON message before adding to the queue</param>
        public ServiceBusActivityLogger(QueueClient client, QueueLoggerSettings queueSettings = null,
            JsonSerializerSettings settings = null)
        {

            SetField.NotNull(out _client, nameof(client), client);

            //set the defaults
            _queueLoggerSettings = queueSettings ?? new QueueLoggerSettings();

            _jsonSerializerSettings = settings;
            _cutCoefficient = 1 - _queueLoggerSettings.MessageTrimRate;
        }

        byte[] GetBytes(string message)
        {
            return _queueLoggerSettings.CompressMessage ? message.Compress() : Encoding.UTF8.GetBytes(message);
        }

        /// <summary>
        /// Logs a single Activity message
        /// </summary>
        /// <param name="activity">An activity to be logged</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">thrown when message exceeds length limit</exception>
        public async Task LogAsync(IActivity activity)
        {
            var message = activity.AsMessageActivity();

            string jsonMsg = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            var bytes = GetBytes(jsonMsg);

            if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Discard)
            {
                //if fails do not do anything....
                try
                {
                    await _client.SendAsync(new BrokeredMessage(bytes));
                }
                catch
                {
                    // ignored
                }
            }
            else if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Error)
            {
                //let it fail                    
                await _client.SendAsync(new BrokeredMessage(bytes));
            }
            else if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Trim)
            {
                do
                {
                    try
                    {
                        await _client.SendAsync(new BrokeredMessage(bytes));
                        return;
                    }
                    catch
                    {
                        //cut off some of the text to fit
                        message.Text = message.Text.Substring(0, (int)(message.Text.Length * _cutCoefficient));
                        jsonMsg = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
                        bytes = GetBytes(jsonMsg);
                    }

                } while (true);
            }
        }
    }
}