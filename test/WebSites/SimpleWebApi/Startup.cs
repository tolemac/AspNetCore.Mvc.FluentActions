﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ExplicitlyImpl.AspNetCore.Mvc.FluentEndpoints;

namespace SimpleWebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvcWithFluentEndpoints();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<INoteService, NoteService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //app.UseMvcWithFluentEndpoints(Endpoints.AllExternal);

            app.UseMvcWithFluentEndpoints(endpoints =>
            {
                endpoints
                    .Add("/api/users", HttpMethod.Get, "List users.")
                    .UsingService<IUserService>()
                    .HandledBy(userService => userService.List())
                    .RenderedBy("users/list.cshtml");

                endpoints
                    .Add("/api/users", HttpMethod.Post)
                    .UsingService<IUserService>()
                    .UsingBody<UserItem>()
                    .HandledBy((userService, user) => userService.Add(user));

                endpoints
                    .Add("/api/users/{userId}", HttpMethod.Get)
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .HandledBy((userService, userId) => userService.Get(userId));

                endpoints
                    .Add("/api/users/{userId}", HttpMethod.Put)
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .UsingBody<UserItem>()
                    .HandledBy((userService, userId, user) => userService.Update(userId, user));

                endpoints
                    .Add("/api/users/{userId}", HttpMethod.Delete)
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .HandledBy((userService, userId) => userService.Remove(userId));

                //endpoints
                //    .Add("/api/users/{userId}", HttpMethod.Put)
                //    .UsingService<IUserService>()
                //    .UsingDataModel<UserItem>(model =>  
                //        model.InitiallyBoundFromBody();  
                //        model.BindUrlParameter<int>("userId", user => user.Id); 
                //    )  
                //    .HandledBy((userService, user) => userService.Update(user));

                //endpoints
                //    .Add("/api/users/{userId}", HttpMethod.Get)
                //    .UsingController<UserController>()
                //    .UsingParameter<int>("userId")
                //    .HandledBy((userController, userId) => userController.Edit(userId));

                //endpoints
                //    .Add("/api/users/{userId}", HttpMethod.Get)
                //    .HandledByController<UserController>("Get");

                //endpoints
                //    .Add("/api/users/{userId}", HttpMethod.Get)
                //    .UsingService<IUserService>()
                //    .UsingParameter<int>("userId")
                //    .HandledBy((userService, userId) => userService.Get(userId))
                //    .UsingService<IJsonUtilsService>()
                //    .HandledBy((result, jsonUtilsService) => jsonUtilsService.Encode(result));

                //endpoints
                //    .Add("/api/users/{userId}", HttpMethod.Get)
                //    .UsingService<IUserService>()
                //    .UsingParameter<int>("userId")
                //    .HandledBy((userService, userId) => userService.Get(userId))
                //    .ToJson()
            });
        }
    }
}
