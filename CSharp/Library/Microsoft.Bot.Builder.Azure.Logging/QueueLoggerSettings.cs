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
}