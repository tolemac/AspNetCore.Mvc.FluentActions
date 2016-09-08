﻿// Licensed under the MIT License. See LICENSE file in the root of the solution for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExplicitlyImpl.AspNetCore.Mvc.FluentActions
{
    public enum FluentActionHandlerType
    {
        Unknown,
        Func,
        View,
        PartialView,
        ViewComponent,
        Controller
    }

    public class FluentActionHandlerDefinition
    {
        public FluentActionHandlerType Type { get; set; }

        public IList<FluentActionUsingDefinition> Usings { get; set; }

        public Type ReturnType { get; set; }

        public Delegate Delegate { get; set; }

        public string PathToView { get; set; }

        public LambdaExpression Expression { get; set; }

        public FluentActionHandlerDefinition()
        {
            Type = FluentActionHandlerType.Unknown;
            Usings = new List<FluentActionUsingDefinition>();
        }
    }

    public class FluentActionDefinition
    {
        public readonly string RouteTemplate;

        public readonly HttpMethod HttpMethod;

        public readonly string Title;

        public IList<FluentActionHandlerDefinition> Handlers { get; set; }

        internal FluentActionHandlerDefinition CurrentHandler
        {
            get
            {
                if (!Handlers.Any())
                {
                    Handlers.Add(new FluentActionHandlerDefinition());
                }

                return Handlers.Last();
            }
        }

        public Type ReturnType => Handlers?.LastOrDefault()?.ReturnType;

        public bool IsMapRoute => Handlers.Count == 1 && Handlers.First().Type == FluentActionHandlerType.Controller;

        public FluentActionDefinition(string routeTemplate, HttpMethod httpMethod, string title = null)
        {
            RouteTemplate = routeTemplate;
            HttpMethod = httpMethod;
            Title = title;

            Handlers = new List<FluentActionHandlerDefinition>();
        }

        public override string ToString()
        {
            return $"[{HttpMethod}]/{RouteTemplate ?? "?"}";
        }
    }

    public class FluentActionBase
    {
        public readonly FluentActionDefinition Definition;

        public string RouteTemplate => Definition.RouteTemplate;

        public HttpMethod HttpMethod => Definition.HttpMethod;

        public string Title => Definition.Title;

        public FluentActionBase(HttpMethod httpMethod, string routeTemplate, string title = null)
        {
            Definition = new FluentActionDefinition(routeTemplate, httpMethod, title);
        }

        public FluentActionBase(string routeTemplate, HttpMethod httpMethod, string title = null)
            : this(httpMethod, routeTemplate, title) { }

        public FluentActionBase(FluentActionDefinition actionDefinition)
        {
            Definition = actionDefinition;
        }

        public override string ToString()
        {
            return Definition.ToString();
        }
    }

    public class FluentAction : FluentActionBase
    {
        public FluentAction(HttpMethod httpMethod, string routeTemplate, string title = null)
            : base(httpMethod, routeTemplate, title) { }

        public FluentAction(string routeTemplate, HttpMethod httpMethod, string title = null)
            : base(httpMethod, routeTemplate, title) { }

        public FluentAction(FluentActionDefinition fluentActionDefinition)
            : base(fluentActionDefinition) { }

        public virtual FluentActionWithUsing<TU1> Using<TU1>(FluentActionUsingDefinition usingDefinition)
        {
            return new FluentActionWithUsing<TU1>(Definition, usingDefinition);
        }

        public virtual FluentActionWithUsing<TU1> UsingService<TU1>()
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingServiceDefinition
            {
                Type = typeof(TU1)
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingService<TU1>(TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingServiceDefinition
            {
                Type = typeof(TU1),
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingRouteParameter<TU1>(string name)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingRouteParameterDefinition
            {
                Type = typeof(TU1),
                Name = name
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingRouteParameter<TU1>(string name, TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingRouteParameterDefinition
            {
                Type = typeof(TU1),
                Name = name,
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingQueryStringParameter<TU1>(string name)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingQueryStringParameterDefinition
            {
                Type = typeof(TU1),
                Name = name
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingQueryStringParameter<TU1>(string name, TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingQueryStringParameterDefinition
            {
                Type = typeof(TU1),
                Name = name,
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingHeader<TU1>(string name)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingHeaderParameterDefinition
            {
                Type = typeof(TU1),
                Name = name
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingHeader<TU1>(string name, TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingHeaderParameterDefinition
            {
                Type = typeof(TU1),
                Name = name,
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingBody<TU1>()
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingBodyDefinition
            {
                Type = typeof(TU1)
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingBody<TU1>(TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingBodyDefinition
            {
                Type = typeof(TU1),
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingForm<TU1>()
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingFormDefinition
            {
                Type = typeof(TU1)
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingForm<TU1>(TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingFormDefinition
            {
                Type = typeof(TU1),
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingFormValue<TU1>(string key)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingFormValueDefinition
            {
                Type = typeof(TU1),
                Key = key
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingFormValue<TU1>(string key, TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingFormValueDefinition
            {
                Type = typeof(TU1),
                Key = key,
                HasDefaultValue = true,
                DefaultValue = defaultValue
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingModelBinder<TU1>(Type modelBinderType, string parameterName = null)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingModelBinderDefinition
            {
                Type = typeof(TU1),
                ModelBinderType = modelBinderType,
                ParameterName = parameterName
            });
        }

        public virtual FluentActionWithUsing<TU1> UsingModelBinder<TU1>(Type modelBinderType, string parameterName, TU1 defaultValue)
        {
            return new FluentActionWithUsing<TU1>(Definition, new FluentActionUsingModelBinderDefinition
            {
                Type = typeof(TU1),
                ModelBinderType = modelBinderType,
                HasDefaultValue = true,
                DefaultValue = defaultValue,
                ParameterName = parameterName
            });
        }

        public virtual FluentActionWithUsing<HttpContext> UsingHttpContext()
        {
            return new FluentActionWithUsing<HttpContext>(Definition, new FluentActionUsingHttpContextDefinition
            {
                Type = typeof(HttpContext)
            });
        }

        public virtual FluentActionWithUsing<ViewDataDictionary> UsingViewData()
        {
            return new FluentActionWithUsing<ViewDataDictionary>(Definition, new FluentActionUsingViewDataDefinition
            {
                Type = typeof(ViewDataDictionary)
            });
        }

        public virtual FluentActionWithController<TC> ToController<TC>() where TC : Controller
        {
            return new FluentActionWithController<TC>(Definition, new FluentActionUsingControllerDefinition
            {
                Type = typeof(TC)
            });
        }

        public FluentActionWithResult<TR> To<TR>(Func<TR> handlerFunc)
        {
            return new FluentActionWithResult<TR>(Definition, handlerFunc);
        }

        public static FluentAction Route(string routeTemplate, HttpMethod httpMethod, string title = null)
        {
            return new FluentAction(httpMethod, routeTemplate, title);
        }

        public static FluentAction RouteDelete(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Delete, title);
        }

        public static FluentAction RouteGet(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Get, title);
        }

        public static FluentAction RouteHead(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Head, title);
        }

        public static FluentAction RouteOptions(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Options, title);
        }

        public static FluentAction RoutePatch(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Patch, title);
        }

        public static FluentAction RoutePost(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Post, title);
        }

        public static FluentAction RoutePut(string routeTemplate, string title = null)
        {
            return Route(routeTemplate, HttpMethod.Put, title);
        }
    }
}
