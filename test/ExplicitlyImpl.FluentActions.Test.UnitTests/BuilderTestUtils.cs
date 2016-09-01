﻿using ExplicitlyImpl.AspNetCore.Mvc.FluentActions;
using ExplicitlyImpl.FluentActions.Test.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace ExplicitlyImpl.FluentActions.Test.UnitTests
{
    public static class BuilderTestUtils
    {
        public static void AssertConstantValuesOfBuiltController(FluentActionControllerDefinition builtController, FluentActionBase fluentAction)
        {
            var builtControllerTypeInfo = builtController.TypeInfo;

            Assert.Equal(fluentAction.Url, builtController.Url);
            Assert.Equal("HandlerAction", builtController.ActionName);
            Assert.Equal(builtControllerTypeInfo.Name, builtController.Id);
            Assert.Equal(builtControllerTypeInfo.Name, builtController.Name);
            Assert.Equal(fluentAction, builtController.FluentAction);

            Assert.True(builtControllerTypeInfo.Name.StartsWith("FluentAction"));
            Assert.True(builtControllerTypeInfo.Name.EndsWith("Controller"));
        }

        private static void BuildActionAndCompareToStaticAction(FluentActionBase fluentAction, Type staticControllerType, object[] actionMethodArguments = null)
        {
            var builtController = BuildController(fluentAction);

            AssertConstantValuesOfBuiltController(builtController, fluentAction);

            CompareBuiltControllerToStaticController(builtController.TypeInfo.UnderlyingSystemType, staticControllerType);

            CompareActionMethodResults(fluentAction, builtController, staticControllerType, actionMethodArguments);
        }

        public static void CompareActionMethodResults(FluentActionBase fluentAction, FluentActionControllerDefinition builtController, Type staticControllerType, object[] actionMethodArguments = null)
        {
            var resultsFromBuiltController = InvokeActionMethod(builtController.TypeInfo, actionMethodArguments);
            var resultsFromStaticController = InvokeActionMethod(staticControllerType.GetTypeInfo(), actionMethodArguments);
            
            if (!fluentAction.Definition.ReturnType.IsAssignableFrom(resultsFromBuiltController.GetType()))
            {
                throw new Exception($"Incorrect return type from invoked action method of built controller {builtController.Name} ({resultsFromBuiltController.GetType().Name} should be {fluentAction.Definition.ReturnType}).");
            }

            if (!fluentAction.Definition.ReturnType.IsAssignableFrom(resultsFromStaticController.GetType()))
            {
                throw new Exception($"Incorrect return type from invoked action method of statically defined controller {staticControllerType.Name} ({resultsFromStaticController.GetType().Name} should be {fluentAction.Definition.ReturnType}).");
            }

            if (
                fluentAction.Definition.ReturnType.GetTypeInfo().IsGenericType &&
                typeof(Task).IsAssignableFrom(fluentAction.Definition.ReturnType))
            {
                resultsFromBuiltController = GetTaskResult(resultsFromBuiltController);
                resultsFromStaticController = GetTaskResult(resultsFromStaticController);
            }

            if (!IsEqual(resultsFromBuiltController, resultsFromStaticController))
            {
                throw new Exception($"Results from invoked action methods does not match between built controller {builtController.Name} and statically defined controller {staticControllerType.Name} ({resultsFromBuiltController} vs {resultsFromStaticController}).");
            }
        }

        private static object GetTaskResult(object taskWithResult)
        {
            try
            {
                var genericArgumentType = taskWithResult.GetType().GetGenericArguments()[0];
                var resultProperty = typeof(Task<>).MakeGenericType(genericArgumentType).GetProperty("Result");
                return resultProperty.GetValue(taskWithResult);
            } 
            catch (Exception exception)
            {
                throw new Exception("Could not get results of generic task.", exception);
            }
        }

        private static bool IsEqual(object value1, object value2)
        {
            var typeInfo = value1.GetType().GetTypeInfo();

            if (typeInfo.IsPrimitive)
            {
                return value1.Equals(value2);
            } 
            else if (value1 is string)
            {
                return value1 != null && value1.Equals(value2);
            }
            else if (value1 is IEnumerable)
            {
                return Enumerable.SequenceEqual((IEnumerable<object>)value1, (IEnumerable<object>)value2);
            } 
            else
            {
                return value1 != null && value1.Equals(value2);
            }
        }

        private static object InvokeActionMethod(TypeInfo controllerTypeInfo, object[] arguments)
        {
            try
            {
                var instance = Activator.CreateInstance(controllerTypeInfo.UnderlyingSystemType);
                var method = controllerTypeInfo.GetMethod("HandlerAction");
                return method.Invoke(instance, arguments);
            } 
            catch (Exception exception)
            {
                throw new Exception($"Could not invoke action method for controller type {controllerTypeInfo.Name}.", exception);
            }
        }

        public static void CompareBuiltControllerToStaticController(Type builtControllerType, Type staticControllerType)
        {
            var comparer = new TypeComparer(new TypeComparisonFeature[]
            {
                TypeComparisonFeature.HandlerActionMethod
            }, new TypeComparerOptions());

            var comparisonResult = comparer.Compare(builtControllerType, staticControllerType);

            if (!comparisonResult.CompleteMatch)
            {
                throw new Exception(string.Format(
                    "Dynamically created controller {0} does not match statically defined controller {1}: {2}",
                    staticControllerType.Name,
                    builtControllerType.Name,
                    string.Join(" ", comparisonResult.MismatchingFeaturesResults
                        .Select(comparedFeaturesResult => comparedFeaturesResult.Message))));
            }
        }

        public static FluentActionControllerDefinition BuildController(FluentActionBase fluentAction)
        {
            var controllerBuilder = new FluentActionControllerDefinitionBuilder();
            return controllerBuilder.Build(fluentAction);
        }
    }
}