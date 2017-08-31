﻿// Licensed under the MIT License. See LICENSE file in the root of the solution for license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ExplicitlyImpl.AspNetCore.Mvc.FluentActions
{
    public class FluentActionCollection : IEnumerable<FluentActionBase>
    {
        public List<FluentActionBase> FluentActions { get; internal set; }

        public FluentActionCollectionConfig Config { get; internal set; }

        internal FluentActionCollection(FluentActionCollectionConfig config)
        {
            FluentActions = new List<FluentActionBase>();
            Config = config ?? new FluentActionCollectionConfig();
        }

        public FluentAction Route(string routeTemplate, HttpMethod httpMethod, string id = null)
        {
            var fluentAction = new FluentAction(routeTemplate, httpMethod, id);
            ConfigureAction(fluentAction);
            FluentActions.Add(fluentAction);
            return fluentAction;
        }

        public FluentAction RouteDelete(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Delete, id);
        }

        public FluentAction RouteGet(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Get, id);
        }

        public FluentAction RouteHead(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Head, id);
        }

        public FluentAction RouteOptions(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Options, id);
        }

        public FluentAction RoutePatch(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Patch, id);
        }

        public FluentAction RoutePost(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Post, id);
        }

        public FluentAction RoutePut(string routeTemplate, string id = null)
        {
            return Route(routeTemplate, HttpMethod.Put, id);
        }

        public void Add(FluentActionBase fluentAction)
        {
            ConfigureAction(fluentAction);
            FluentActions.Add(fluentAction);
        }

        public void Add(FluentActionCollection fluentActions)
        {
            foreach (var fluentAction in fluentActions)
            {
                ConfigureAction(fluentAction);
                FluentActions.Add(fluentAction);
            }
        }

        public IEnumerator<FluentActionBase> GetEnumerator()
        {
            return FluentActions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ConfigureAction(FluentActionBase fluentAction)
        {
            if (Config.GroupName != null && fluentAction.Definition.GroupName == null)
            {
                fluentAction.Definition.GroupName = Config.GroupName;
            }

            if (Config.ParentType != null && fluentAction.Definition.ParentType == null)
            {
                fluentAction.Definition.ParentType = Config.ParentType;
            }

            if (Config.GetTitleFunc != null && fluentAction.Definition.Title == null)
            {
                fluentAction.Definition.Title = Config.GetTitleFunc(fluentAction.Definition);
            }

            if (Config.GetDescriptionFunc != null && fluentAction.Definition.Description == null)
            {
                fluentAction.Definition.Description = Config.GetDescriptionFunc(fluentAction.Definition);
            }

            foreach (var customAttribute in Config.CustomAttributes)
            {
                fluentAction.Definition.CustomAttributes.Add(customAttribute);
            }

            foreach (var customAttributeOnClass in Config.CustomAttributesOnClass)
            {
                fluentAction.Definition.CustomAttributesOnClass.Add(customAttributeOnClass);
            }
        }

        public static FluentActionCollection DefineActions(
            Action<FluentActionCollectionConfigurator> configureFluentActions, 
            Action<FluentActionCollection> addFluentActions)
        {
            var configurator = new FluentActionCollectionConfigurator(new FluentActionCollectionConfig());
            configureFluentActions(configurator);

            var actionCollection = new FluentActionCollection(configurator.Config);

            addFluentActions(actionCollection);

            return actionCollection;
        }

        public static FluentActionCollection DefineActions(Action<FluentActionCollection> addFluentActions)
        {
            return DefineActions(config => { }, addFluentActions);
        }
    }

    public class FluentActionCollectionConfigurator
    {
        internal FluentActionCollectionConfig Config { get; set; }

        public FluentActionCollectionConfigurator(FluentActionCollectionConfig config)
        {
            Config = config;
        }

        public void GroupBy(string groupName)
        {
            Config.GroupName = groupName;
        }

        public void InheritingFrom(Type parentType)
        {
            if (parentType != typeof(Controller) && !parentType.GetTypeInfo().IsSubclassOf(typeof(Controller)))
            {
                throw new Exception($"Cannot make fluent action controller inherit from a class that is not derived from the Controller class (${parentType.FullName}).");
            }

            Config.ParentType = parentType;
        }

        public void InheritingFrom<T>() where T : Controller
        {
            Config.ParentType = typeof(T);
        }

        public void SetTitle(Func<FluentActionDefinition, string> getTitleFunc)
        {
            Config.GetTitleFunc = getTitleFunc;
        }

        public void SetTitleFromResource(
            Type resourceType,
            Func<FluentActionDefinition, string> getResourceNameFunc,
            bool ignoreMissingValues = false)
        {
            SetTitleFromResource(resourceType, getResourceNameFunc, CultureInfo.CurrentUICulture, ignoreMissingValues);
        }

        public void SetTitleFromResource(
            Type resourceType,
            Func<FluentActionDefinition, string> getResourceNameFunc,
            CultureInfo culture,
            bool ignoreMissingValues = false)
        {
            SetTitle(action =>
            {
                try
                {
                    return GetResourceValue(resourceType, getResourceNameFunc(action), culture, ignoreMissingValues);
                } catch (Exception exception)
                {
                    throw new Exception($"Could not get title from resource {resourceType} for action {action}: {exception.Message ?? ""}", exception);
                }
            });
        }

        public void SetDescription(Func<FluentActionDefinition, string> getDescriptionFunc)
        {
            Config.GetDescriptionFunc = getDescriptionFunc;
        }

        public void SetDescriptionFromResource(
            Type resourceType, 
            Func<FluentActionDefinition, string> getResourceNameFunc, 
            bool ignoreMissingValues = false)
        {
            SetDescriptionFromResource(resourceType, getResourceNameFunc, CultureInfo.CurrentUICulture, ignoreMissingValues);
        }

        public void SetDescriptionFromResource(
            Type resourceType, 
            Func<FluentActionDefinition, string> getResourceNameFunc, 
            CultureInfo culture, 
            bool ignoreMissingValues = false)
        {
            SetDescription(action =>
            {
                try
                {
                    return GetResourceValue(resourceType, getResourceNameFunc(action), culture, ignoreMissingValues);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Could not get description from resource {resourceType} for action {action}: {exception.Message ?? ""}", exception);
                }
            });
        }

        public void WithCustomAttribute<T>()
        {
            WithCustomAttribute<T>(new Type[0], new object[0]);
        }

        public void WithCustomAttribute<T>(Type[] constructorArgTypes, object[] constructorArgs)
        {
            var attributeConstructorInfo = typeof(T).GetConstructor(constructorArgTypes);
            WithCustomAttribute<T>(attributeConstructorInfo, constructorArgs);
        }

        public void WithCustomAttribute<T>(Type[] constructorArgTypes, object[] constructorArgs, string[] namedProperties, object[] propertyValues)
        {
            var attributeConstructorInfo = typeof(T).GetConstructor(constructorArgTypes);

            WithCustomAttribute<T>(
                attributeConstructorInfo,
                constructorArgs,
                namedProperties.Select(propertyName => typeof(T).GetProperty(propertyName)).ToArray(),
                propertyValues);
        }

        public void WithCustomAttribute<T>(ConstructorInfo constructor, object[] constructorArgs)
        {
            WithCustomAttribute<T>(
                constructor,
                constructorArgs,
                new PropertyInfo[0],
                new object[0],
                new FieldInfo[0],
                new object[0]);
        }

        public void WithCustomAttribute<T>(ConstructorInfo constructor, object[] constructorArgs, FieldInfo[] namedFields, object[] fieldValues)
        {
            WithCustomAttribute<T>(
                constructor,
                constructorArgs,
                new PropertyInfo[0],
                new object[0],
                namedFields,
                fieldValues);
        }

        public void WithCustomAttribute<T>(ConstructorInfo constructor, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
        {
            WithCustomAttribute<T>(
                constructor,
                constructorArgs,
                namedProperties,
                propertyValues,
                new FieldInfo[0],
                new object[0]);
        }

        public void WithCustomAttribute<T>(ConstructorInfo constructor, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
        {
            Config.CustomAttributes.Add(new FluentActionCustomAttribute()
            {
                Type = typeof(T),
                Constructor = constructor,
                ConstructorArgs = constructorArgs,
                NamedProperties = namedProperties,
                PropertyValues = propertyValues,
                NamedFields = namedFields,
                FieldValues = fieldValues,
            });
        }

        public void WithCustomAttributeOnClass<T>()
        {
            WithCustomAttributeOnClass<T>(new Type[0], new object[0]);
        }

        public void WithCustomAttributeOnClass<T>(Type[] constructorArgTypes, object[] constructorArgs)
        {
            var attributeConstructorInfo = typeof(T).GetConstructor(constructorArgTypes);
            WithCustomAttributeOnClass<T>(attributeConstructorInfo, constructorArgs);
        }

        public void WithCustomAttributeOnClass<T>(
            Type[] constructorArgTypes, 
            object[] constructorArgs, 
            string[] namedProperties, 
            object[] propertyValues)
        {
            var attributeConstructorInfo = typeof(T).GetConstructor(constructorArgTypes);

            WithCustomAttributeOnClass<T>(
                attributeConstructorInfo,
                constructorArgs,
                namedProperties.Select(propertyName => typeof(T).GetProperty(propertyName)).ToArray(),
                propertyValues);
        }

        public void WithCustomAttributeOnClass<T>(ConstructorInfo constructor, object[] constructorArgs)
        {
            WithCustomAttributeOnClass<T>(
                constructor,
                constructorArgs,
                new PropertyInfo[0],
                new object[0],
                new FieldInfo[0],
                new object[0]);
        }

        public void WithCustomAttributeOnClass<T>(
            ConstructorInfo constructor, 
            object[] constructorArgs, 
            FieldInfo[] namedFields, 
            object[] fieldValues)
        {
            WithCustomAttributeOnClass<T>(
                constructor,
                constructorArgs,
                new PropertyInfo[0],
                new object[0],
                namedFields,
                fieldValues);
        }

        public void WithCustomAttributeOnClass<T>(ConstructorInfo constructor, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues)
        {
            WithCustomAttributeOnClass<T>(
                constructor,
                constructorArgs,
                namedProperties,
                propertyValues,
                new FieldInfo[0],
                new object[0]);
        }

        public void WithCustomAttributeOnClass<T>(ConstructorInfo constructor, object[] constructorArgs, PropertyInfo[] namedProperties, object[] propertyValues, FieldInfo[] namedFields, object[] fieldValues)
        {
            Config.CustomAttributesOnClass.Add(new FluentActionCustomAttribute()
            {
                Type = typeof(T),
                Constructor = constructor,
                ConstructorArgs = constructorArgs,
                NamedProperties = namedProperties,
                PropertyValues = propertyValues,
                NamedFields = namedFields,
                FieldValues = fieldValues,
            });
        }

        public void Authorize(string policy = null, string roles = null, string activeAuthenticationSchemes = null)
        {
            WithCustomAttribute<AuthorizeAttribute>(
                new Type[] { typeof(string) },
                new object[] { policy },
                new string[] { "Roles", "ActiveAuthenticationSchemes" },
                new object[] { roles, activeAuthenticationSchemes });
        }

        public void AuthorizeClass(string policy = null, string roles = null, string activeAuthenticationSchemes = null)
        {
            WithCustomAttributeOnClass<AuthorizeAttribute>(
                new Type[] { typeof(string) },
                new object[] { policy },
                new string[] { "Roles", "ActiveAuthenticationSchemes" },
                new object[] { roles, activeAuthenticationSchemes });
        }

        private static string GetResourceValue(Type resourceSourceType, string resourceName, CultureInfo culture, bool ignoreMissingValues = false)
        {

            var resourceValue = new ResourceManager(resourceSourceType).GetString(resourceName, culture);

            if (resourceValue == null && !ignoreMissingValues)
            {
                throw new Exception($"Resource is missing value for name {resourceName}.");
            }

            return resourceValue;
        }
    }

    public class FluentActionCollectionConfig
    {
        public string GroupName { get; internal set; }

        public Type ParentType { get; internal set; }

        public Func<FluentActionDefinition, string> GetTitleFunc { get; internal set; }

        public Func<FluentActionDefinition, string> GetDescriptionFunc { get; internal set; }

        public IList<FluentActionCustomAttribute> CustomAttributes { get; internal set; }

        public IList<FluentActionCustomAttribute> CustomAttributesOnClass { get; internal set; }

        public FluentActionCollectionConfig()
        {
            CustomAttributes = new List<FluentActionCustomAttribute>();
            CustomAttributesOnClass = new List<FluentActionCustomAttribute>();
        }

        internal FluentActionCollectionConfig Clone()
        {
            return new FluentActionCollectionConfig
            {
                GroupName = GroupName,
                ParentType = ParentType,
                GetTitleFunc = GetTitleFunc,
                GetDescriptionFunc = GetDescriptionFunc,
                CustomAttributes = new List<FluentActionCustomAttribute>(CustomAttributes),
                CustomAttributesOnClass = new List<FluentActionCustomAttribute>(CustomAttributesOnClass)
            };
        }
    }
}
