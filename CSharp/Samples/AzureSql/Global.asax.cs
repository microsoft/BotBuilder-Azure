using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.AzureSql.Dialogs;
using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;

namespace Microsoft.Bot.Sample.AzureSql
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            {
                var config = GlobalConfiguration.Configuration;

                Conversation.UpdateContainer(
                    builder =>
                        {
                            builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                            var store = new SqlBotDataStore(ConfigurationManager.ConnectionStrings["BotDataContextConnectionString"].ConnectionString);

                            builder.Register(c => store)
                                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                .AsSelf()
                                .SingleInstance();


                            // Register your Web API controllers.
                            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                            builder.RegisterWebApiFilterProvider(config);

                        });

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
