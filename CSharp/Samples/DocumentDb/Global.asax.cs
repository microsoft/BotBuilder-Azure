using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.DocumentDb.Controllers;
using Microsoft.Bot.Sample.DocumentDb.Dialogs;
using Microsoft.WindowsAzure.Storage;


namespace Microsoft.Bot.Sample.DocumentDb
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            {
                //Using the local DocDbEmulator, which should be installed and started. Otherwise, edit the docDbEmulatorUri and docDbEmulatorKey variables to your DocDb database
                //Reference: https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator

                //Fixed docDb emulator local Uri.
                Uri docDbEmulatorUri = new Uri("https://localhost:8081");

                //Fixed docDb emulator key
                const string docDbEmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

                // http://docs.autofac.org/en/latest/integration/webapi.html#quick-start
                var builder = new ContainerBuilder();

                // register the Bot Builder module
                builder.RegisterModule(new DialogModule());
                // register the alarm dependencies
                builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                builder
                    .RegisterInstance(new EchoDialog())
                    .As<IDialog<object>>();

                var store = new DocumentDbBotDataStore(docDbEmulatorUri, docDbEmulatorKey);
                builder.Register(c => store)
                    .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                    .AsSelf()
                    .SingleInstance();


                builder.Register(c => new CachingBotDataStore(store,
                                                              CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();

                // Get your HttpConfiguration.
                var config = GlobalConfiguration.Configuration;

                // Register your Web API controllers.
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

                // OPTIONAL: Register the Autofac filter provider.
                builder.RegisterWebApiFilterProvider(config);

                // Set the dependency resolver to be Autofac.
                //var container = builder.Build();
                builder.Update(Conversation.Container);
                config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);
                
            }

            // WebApiConfig stuff
            GlobalConfiguration.Configure(config =>
            {
                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            });
        }

        public static ILifetimeScope FindContainer()
        {
            var config = GlobalConfiguration.Configuration;
            var resolver = (AutofacWebApiDependencyResolver)config.DependencyResolver;
            return resolver.Container;
        }
    }
}