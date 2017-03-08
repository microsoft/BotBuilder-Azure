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
            OverflowHanding = LargeMessageMode.Discard;
            MessageTrimRate = 0.10f;
        }
        /// <summary>
        /// Informs logger to compress messages, default is false
        /// </summary>
        public bool CompressMessage { get; set; }
        /// <summary>
        /// Informs logger what to do with messages that exceed length limit, default is Discard
        /// </summary>
        public LargeMessageMode OverflowHanding { get; set; }

        //if Overflow is set to trim, MessageTrimRate controls how much string to trim at one time, default is 0.10
        public float MessageTrimRate;
    }
}