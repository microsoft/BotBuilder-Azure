namespace Microsoft.Bot.Builder.Azure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Microsoft.Bot.Builder.Azure.SqlBotDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Microsoft.Bot.Builder.Azure.SqlBotDataContext context)
        {
        }
    }
}
