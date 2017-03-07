using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    public class QueueLoggerSettings
    {
        public QueueLoggerSettings()
        {
            CompressMessage = false;
            LargeMessageHandlingPattern = LargeMessageMode.Discard;
        }
        public bool CompressMessage { get; set; }
        public LargeMessageMode LargeMessageHandlingPattern { get; set; }
    }

    public enum LargeMessageMode
    {
        Discard,
        Trim,
        Error
    }
    /// <summary>
    /// 
    /// </summary>
    public class ServiceBusActivityLogger : IActivityLogger
    {
        private readonly QueueClient _client;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private CloudQueueClient _cloudQueueClient;
        private QueueLoggerSettings _queueLoggerSettings;

        private int _textMaxLength = 1024*60; //60k
        private int _preCompressedMaxTextLength = 1024*180; //180k - assuming 3x compression

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings"></param>
        public ServiceBusActivityLogger(QueueClient client, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {
            //set default settings
            if (loggerSettings == null)
                _queueLoggerSettings = new QueueLoggerSettings();
            else
                _queueLoggerSettings = loggerSettings;

            _jsonSerializerSettings = settings;
            _client = client;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
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