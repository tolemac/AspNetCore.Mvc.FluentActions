﻿using ExplicitlyImpl.AspNetCore.Mvc.FluentActions;

namespace SimpleApi
{
    public static class UserActions
    {
        public static FluentActionCollection All => FluentActionCollection.DefineActions(
            config =>
            {
                config.GroupBy("UserActions");
                config.SetTitleFromResource(typeof(Localization.Actions), action => $"{action.Id}_Title");
                config.SetDescriptionFromResource(typeof(Localization.Actions), action => $"{action.Id}_Description");
            },
            actions =>
            {
                actions
                    .RouteGet("/users", "ListUsers")
                    .UsingService<IUserService>()
                    .To(userService => userService.List());

                actions
                    .RoutePost("/users", "AddUser")
                    .UsingService<IUserService>()
                    .UsingBody<UserItem>()
                    .To((userService, user) => userService.Add(user));

                actions
                    .RouteGet("/users/{userId}", "GetUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .To((userService, userId) => userService.Get(userId));

                actions
                    .RoutePut("/users/{userId}", "UpdateUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .UsingBody<UserItem>()
                    .To((userService, userId, user) => userService.Update(userId, user));

                actions
                    .RouteDelete("/users/{userId}", "RemoveUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .To((userService, userId) => userService.Remove(userId));
            }
        );
    }
}
