﻿using System.Net;
using Funq;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Formats;
using ServiceStack.OrmLite;
using ServiceStack.Razor;

//The entire C# code for the stand-alone RazorRockstars demo.
namespace RazorRockstars.CompiledViews.SelfHost
{
    public class AppHost : AppSelfHostBase
    {
        public AppHost() : base("Razor Rockstars", typeof(RockstarsService).Assembly) { }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                DebugMode = true,
                EmbeddedResourceBaseTypes = { GetType(), typeof(BaseTypeMarker) },
            });

            Plugins.Add(new RazorFormat {
                LoadFromAssemblies = { typeof(RockstarsService).Assembly }
            });
            Plugins.Add(new MarkdownFormat());

            Plugins.Add(new PostmanFeature());
            Plugins.Add(new CorsFeature());

            container.Register<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));

            using (var db = container.Resolve<IDbConnectionFactory>().OpenDbConnection())
            {
                db.CreateTableIfNotExists<Rockstar>();
                db.InsertAll(RockstarsService.SeedData);
            }

            this.CustomErrorHttpHandlers[HttpStatusCode.NotFound] = new RazorHandler("/notfound");
            this.CustomErrorHttpHandlers[HttpStatusCode.Unauthorized] = new RazorHandler("/login");
        }
    }
}