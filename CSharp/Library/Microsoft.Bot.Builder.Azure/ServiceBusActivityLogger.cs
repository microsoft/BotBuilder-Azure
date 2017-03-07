using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Logger setting to control compression and how to handle large messages
    /// </summary>
    public class QueueLoggerSettings
    {
        /// <summary>
        /// Creates an instance of QueueLoggerSettings with default settings
        /// </summary>
        public QueueLoggerSettings()
        {
            CompressMessage = false;
            LargeMessageHandlingPattern = LargeMessageMode.Discard;
        }
        /// <summary>
        /// Informs logger to compress messages, default is false
        /// </summary>
        public bool CompressMessage { get; set; }
        /// <summary>
        /// Informs logger what to do with messages that exceed length limit, default is Discard
        /// </summary>
        public LargeMessageMode LargeMessageHandlingPattern { get; set; }
    }

    /// <summary>
    /// Enumeration for hadling of large messages
    /// </summary>
    public enum LargeMessageMode
    {
        /// <summary>
        /// Drop the message if exceeds the length limit
        /// </summary>
        Discard,
        /// <summary>
        /// Limit the text to be within the allowed limits
        /// </summary>
        Trim,
        /// <summary>
        /// Throw an exception if message exceeds allowed limit
        /// </summary>
        Error
    }


    /// <summary>
    /// Log activities to Service Bus Queue. 
    /// </summary>
    /// <remarks>
    /// Activities are limited to xx when converted to JSON, xx if compressed.  Disposition of larger messages is controlled by QueueLoggerSettings.
    /// </remarks>
    public class ServiceBusActivityLogger : IActivityLogger
    {
        private readonly QueueClient _client;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        private readonly int _textMaxLength = 1024*60; //60k
        private readonly int _preCompressedMaxTextLength = 1024*180; //180k - assuming 3x compression

        /// <summary>
        /// Constructs an instance of ServiceBusActivityLogger
        /// </summary>
        /// <param name="client">Reference to a QueueClient instance</param>
        /// <param name="loggerSettings">Settings informing the logger how to handle large messages and whether compression is required</param>
        /// <param name="settings">JSON serialziation settings used to write the formatted JSON message before adding to the queue</param>
        public ServiceBusActivityLogger(QueueClient client, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            //set default settings
            if (loggerSettings == null)
                _queueLoggerSettings = new QueueLoggerSettings();
            else
                _queueLoggerSettings = loggerSettings;

            _jsonSerializerSettings = settings;
            
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

            int maxMessagelength = _queueLoggerSettings.CompressMessage ? _preCompressedMaxTextLength : _textMaxLength;

            //handle discard case
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Discard &&
                message.Text.Length > maxMessagelength)
                return;

            //if error is requested
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Error)
                throw new ArgumentException($"Message length of {message.Text.Length} is larger than {maxMessagelength} allowed.");

            //if trim, trim the message
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Trim &&
                message.Text.Length > maxMessagelength)
            {
                
                message.Text = message.Text.Substring(0, maxMessagelength);
            }

            string jsonMsg = JsonConvert.SerializeObject(activity, _jsonSerializerSettings);

            //send compressed or plain message
            if (_queueLoggerSettings.CompressMessage)
                await _client.SendAsync(new BrokeredMessage(jsonMsg.Compress()));
            else
            {
                await _client.SendAsync(new BrokeredMessage(jsonMsg));
            }            
        }
    }
}