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
    /// 
    /// </summary>
    public class AzureStorageQueueActivityLogger : IActivityLogger
    {

        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private CloudQueueClient _cloudQueueClient;
        private CloudQueue _queueClient;
        private QueueLoggerSettings _queueLoggerSettings;

        private int _textMaxLength = 1024 * 20; //60k
        private int _preCompressedMaxTextLength = 1024 * 50; //180k - assuming 3x compression

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings"></param>
        public AzureStorageQueueActivityLogger(CloudQueue queueClient, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {
            _queueLoggerSettings = loggerSettings;
            //set the defaults
            if (_queueLoggerSettings == null)
                _queueLoggerSettings = new QueueLoggerSettings();
            _queueClient = queueClient;
            _jsonSerializerSettings = settings;
        }

        public async Task LogAsync(IActivity activity)
        {

            var message = activity.AsMessageActivity();

            int maxMessagelength = _queueLoggerSettings.CompressMessage ? _preCompressedMaxTextLength : _textMaxLength;

            //handle discard case
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Discard &&
                message.Text.Length > maxMessagelength)
                return;

            //Exception is requested
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Error)
                throw new ArgumentException($"Message length of {message.Text.Length} is larger than {maxMessagelength} allowed.");

            //if trim, trim the message
            if (_queueLoggerSettings.LargeMessageHandlingPattern == LargeMessageMode.Trim && message.Text.Length > maxMessagelength)
            {
                message.Text = message.Text.Substring(0, maxMessagelength);
            }

            string jsonMsg = JsonConvert.SerializeObject(activity, _jsonSerializerSettings);

            //send compressed or plain message
            if (_queueLoggerSettings.CompressMessage)
                await _queueClient.AddMessageAsync(new CloudQueueMessage(jsonMsg.Compress()));
            else
            {
                await _queueClient.AddMessageAsync(new CloudQueueMessage(jsonMsg));
            }
        }

        byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}