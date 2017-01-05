using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The azure bot utilities and helpers.
    /// </summary>
    /// <remarks>
    /// This class is obsolete now. Use <see cref="BotService"/> instead.
    /// </remarks>
    [Obsolete("This class is obsolete. Use BotService instead.", false)]
    public static class AzureBot
    {
        /// <summary>
        /// The bot authenticator.
        /// </summary>
        public static BotAuthenticator Authenticator => BotService.Authenticator;

        /// <summary>
        /// Update the <see cref="Conversation.Container"/> for azure bots.
        /// </summary>
        public static void Initialize()
        {
            BotService.Initialize(Assembly.GetCallingAssembly());
        }
    }
}
