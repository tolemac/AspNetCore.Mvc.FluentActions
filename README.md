# [Fluent Actions for ASP.NET Core MVC](https://www.nuget.org/)
Fluent Actions is a tool to define your web logic in an expressive and typed manner without manually creating any controllers or action methods.  

Usage example:

```
app.UseMvcWithFluentActions(actions =>
{
    actions.Map("/").To(() => "Hello World!"));
}
```

Another example:

```
app.UseMvcWithFluentActions(actions =>
{
    actions
        .Map("/api/users/{userId}")
        .UsingService<IUserService>()
        .UsingRouteParameter<int>("userId")
        .To((userService, userId) => userService.GetUserWithId(userId));
}
```

See [CHANGELOG.md](CHANGELOG.md) for a list of recent updates.

## Purpose

This tool was created to solve a couple of issues me and my team faced at a couple of projects. Relevant issues of our use case(s) were:

1. We must use .NET MVC
1. Our "web logic" is separated into its own layer (database, business logic, etc outside)
1. We wanted a more _explicit_ way of defining our web layer

If you take a look at the routing definition of the Visual Studio template used when creating a .NET MVC app:

```
app.UseMvc(routes =>
{
    routes.MapRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
});
```

This kind of routing is fairly implicit and it is pretty hard to know what this web app does without looking in other (implicitly defined) places. We also felt that this is true for the relation between controllers and views:

```
public ActionResult Index() 
{
  return View();
}
```

You can be more explicit than this using vanilla MVC and this tool is just that, an extension/repackaging of a couple of MVC web app implementations that veered towards the more explicit and direct way. Hopefully its usage will also result in a more clear and expressive solution as well.

## Installation

Add this library to your project using NuGet:

```
Install-Package ExplicitlyImpl.AspNetCore.Mvc.FluentActions
```

You also need to replace these two lines in your `Startup.cs` file:

```
// services.AddMvc();
services.AddMvcWithFluentActions();

// app.UseMvc(routes => ...);
app.UseMvcWithFluentActions(actions => ...); // see usage chapter below 
```

## How to use

Fluent actions are added inside the `Startup.cs` file but the fluent action definitions can be placed in one or many other files. 

```
app.UseMvcWithFluentActions(actions => 
{
  actions.Map("/helloWorld").To(() => "Hello World!"));
  actions.Add(MyFluentActions.SingleReference);
  actions.Add(MyFluentActions.MultipleReferences);
}
```

If we examine the first defined action in the code above:

```
actions
  .Map("/helloWorld")
  .To(() => "Hello World!"));
```

- The first statement `Map` defines the routing, any **GET** requests to **/helloWorld** will be handled by this action.
- The second statement `To` defines what will happen when someone makes a **GET** request to this url. In this case, a plain text "Hello World!" will be returned by the web app. 

How do we know how the web app writes our output to the HTTP response? The code above is equivalent to an action method in a controller looking like this:

```
[HttpGet]
[Route("/helloWorld")]
public string HelloWorldAction()
{
    return "Hello World!";
}
```

Fluent actions are only wrapping the tools that makes up the framework .NET MVC. We can still use MVC tools and concepts to implement our web app. In fact, `UseMvcWithFluentActions` can also define routes as you normally do in MVC (using `IAction<Route>`) and attribute routing also works together with fluent actions. 

### Using-definitions

If you need to define any kind of input for your action, use a using-definition. Each `UsingX` statement will become a parameter to your delegate in the `To` statement (in the same order as you call them). 

```
actions
  .Map("/hello")
  .UsingQueryStringParameter<string>("name")
  .To(name => $"Hello {name}!"));
```

The `name` parameter in `To` is of type `string`, inferred from the generic type of `UsingQueryStringParameter`.

Above fluent action is equivalent to the following action method:

```
[HttpGet]
[Route("/hello")]
public string HelloAction([FromQuery]string name)
{
    return $"Hello {name}!";
}
```

Lets look at a previous example:

```
actions
    .Map("/api/users/{userId}")
    .UsingService<IUserService>()
    .UsingRouteParameter<int>("userId")
    .To((userService, userId) => userService.GetUserWithId(userId));
```

This is equivalent to the following action method:

```
[HttpGet]
[Route("/api/users/{userId}")]
public string GetUserAction([FromServices]IUserService userService, [FromRoute]int userId)
{
    return userService.GetUserWithId(userId);
}
```

#### List of using-definitions
Take a look at [Model Binding in ASP.NET Core MVC](https://docs.asp.net/en/latest/mvc/models/model-binding.html#customize-model-binding-behavior-with-attributes) for a better understanding of how the equivalent code of most of these using-definitions work.

- UsingBody
- UsingForm
- UsingFormValue
- UsingHeader
- UsingHttpContext
- UsingModelBinder
- UsingResultFromHandler (only used when piping multiple handlers)
- UsingRouteParameter
- UsingService
- UsingQueryStringParameter

#### UsingBody

This fluent action:

```
actions
  .Map("/hello")
  .UsingBody<UserItem>()
  .To(user => $"Hello {user.Name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action([FromBody]UserItem user)
{
    return $"Hello {user.Name}!";
}
```


#### UsingForm

This fluent action:

```
actions
  .Map("/hello")
  .UsingForm<UserItem>()
  .To(user => $"Hello {user.Name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action([FromForm]UserItem user)
{
    return $"Hello {user.Name}!";
}
```

#### UsingFormValue

This fluent action:

```
actions
  .Map("/hello")
  .UsingFormValue<string>("name")
  .To(name => $"Hello {name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action([FromForm("name")string name)
{
    return $"Hello {name}!";
}
```

#### UsingHeader

This fluent action:

```
actions
  .Map("/hello")
  .UsingHeader<string>("Content-Type")
  .To(contentType => $"Hello, your Content-Type is: {contentType}"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action([FromHeader("ContentType")string contentType)
{
    return $"Hello, your Content-Type is: {contentType}";
}
```

#### UsingHttpContext

This fluent action:

```
actions
  .Map("/hello")
  .UsingHttpContext()
  .To(httpContext => $"Hello, your request path is: {httpContext.Request.Path}"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action()
{
    return $"Hello, your request path is: {HttpContext.Request.Path}";
}
```

#### UsingModelBinder

This fluent action:

```
actions
  .Map("/hello")
  .UsingModelBinder<UserItem>(typeof(MyModelBinder))
  .To(user => $"Hello {user.Name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action([FromModelBinder(typeof(MyModelBinder))UserItem user])
{
    return $"Hello {user.Name}!";
}
```

#### UsingResultFromHandler

This fluent action:

```
actions
  .Map("/hello")
  .To(() => "Hello"))
  .UsingResultFromHandler()
  .To(hello => hello + " World!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello")]
public string Action()
{
    return $"Hello World!";
}
```

#### UsingRouteParameter

This fluent action:

```
actions
  .Map("/hello/{name}")
  .UsingRouteParameter<string>("name")
  .To(name => $"Hello {name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello/{name}")]
public string Action([FromRoute]string name)
{
    return $"Hello {name}!";
}
```

#### UsingService

This fluent action:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .To(userService => userService.List()));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/users")]
public IList<UserItem> Action([FromServices]IUserService userService)
{
    return userService.List();
}
```

This assumes you have defined a dependency injection mapping for a `IUserService` in `StartUp.cs`.

#### UsingQueryStringParameter

This fluent action:

```
actions
  .Map("/hello/{name}")
  .UsingQueryStringParameter<string>("name")
  .To(name => $"Hello {name}!"));
```

Is equivalent to the following action method in a controller:

```
[HttpGet]
[Route("/hello/{name}")]
public string Action([FromQuery]string name)
{
    return $"Hello {name}!";
}
```

### Specifying HTTP method

You can specify which HTTP method to use for an action in the `Map` statement:

```
actions
  .Map("/users", HttpMethod.Post)
  .UsingService<IUserService>()
  .UsingBody<UserItem>()
  .To((userService, user) => userService.Add(user));
```

### `ToView`

To pipe your output from a `To` statement to an MVC view, you can use `ToView`:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .To(userService => userService.List()))
  .ToView("~/views/users/list.cshtml");
```

This is equivalent to:

```
[HttpGet]
[Route("/users")]
public ActionResult Action([FromServices]IUserService userService)
{
    var users = userService.List();
    return View("~/views/users/list.cshtml", users);
}
```

### `ToPartialView`

To pipe your output from a `To` statement to an MVC partial view, you can use `ToPartialView`:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .To(userService => userService.List()))
  .ToPartialView("~/views/users/list.cshtml");
```

This is equivalent to:

```
[HttpGet]
[Route("/users")]
public ActionResult Action([FromServices]IUserService userService)
{
    var users = userService.List();
    return PartialView("~/views/users/list.cshtml", users);
}
```

### `ToViewComponent`

To pipe your output from a `To` statement to an MVC view component, you can use `ToViewComponent`:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .To(userService => userService.List()))
  .ToViewComponent("~/views/users/list.cshtml");
```

This is equivalent to:

```
[HttpGet]
[Route("/users")]
public ActionResult Action([FromServices]IUserService userService)
{
    var users = userService.List();
    return ViewComponent("~/views/users/list.cshtml", users);
}
```

### Code block in `To`

If you want to you can also write code like this:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .UsingQueryStringParameter<int>("userId")
  .To((userService, userId) => 
  {
    var user = userService.GetById(userId);
    return $"Hello {user.Name}!";
  });
```

### Asynchronous handlers (currently not supporting piping handlers)

You can use async/await delegates:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .ToAsync(async userService => await userService.ListAsync());
```

### Piping handlers (currently not supporting asynchronous handlers)

You can use multiple `To` statements for an action:

```
actions
  .Map("/users")
  .UsingService<IUserService>()
  .To(userService => userService.List())
  .UsingQueryStringParameter<string>("name")
  .UsingResultFromHandler()
  .To((name, users) => $"Hello {name}! We got {users.Count} users!");
```

Why would you pipe handlers? Well, we are currently using some extension methods on top of our explicitly defined actions that are specific to our project business logic. Those extensions can be implemented using this concept.
