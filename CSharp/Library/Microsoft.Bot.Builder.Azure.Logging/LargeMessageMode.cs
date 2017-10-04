namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Enumeration for handling of large messages
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
}