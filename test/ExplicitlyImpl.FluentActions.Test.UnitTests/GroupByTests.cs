﻿using ExplicitlyImpl.AspNetCore.Mvc.FluentActions;
using System.Linq;
using Xunit;

namespace ExplicitlyImpl.FluentActions.Test.UnitTests
{
    public class GroupByTests
    {
        [Fact(DisplayName = "GroupBy inside single action")]
        public void FluentControllerBuilder_FluentActionWithGroupBy()
        {
            var action = new FluentAction("/route/url", HttpMethod.Get)
                .GroupBy("CustomGroupName")
                .To(() => "Hello");

            Assert.Equal("CustomGroupName", action.Definition.GroupName);
        }

        [Fact(DisplayName = "GroupBy inside action collection")]
        public void FluentControllerBuilder_FluentActionCollectionWithGroupBy()
        {
            var actionCollection = FluentActionCollection.DefineActions(actions => 
            {
                actions.GroupBy("CustomGroupName");

                actions
                    .RouteGet("/users", "ListUsers")
                    .UsingService<IUserService>()
                    .To(userService => userService.ListUsers());

                actions
                    .RoutePost("/users", "AddUser")
                    .UsingService<IUserService>()
                    .UsingBody<UserItem>()
                    .To((userService, user) => userService.AddUser(user));

                actions
                    .RouteGet("/users/{userId}", "GetUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .To((userService, userId) => userService.GetUserById(userId));

                actions
                    .RoutePut("/users/{userId}", "UpdateUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .UsingBody<UserItem>()
                    .To((userService, userId, user) => userService.UpdateUser(userId, user));

                actions
                    .RouteDelete("/users/{userId}", "RemoveUser")
                    .UsingService<IUserService>()
                    .UsingRouteParameter<int>("userId")
                    .To((userService, userId) => userService.RemoveUser(userId));
            });

            Assert.Equal("CustomGroupName", actionCollection.GroupName);

            foreach (var action in actionCollection)
            {
                Assert.Equal("CustomGroupName", action.Definition.GroupName);
            }
        }
    }
}
