﻿using ExplicitlyImpl.AspNetCore.Mvc.FluentActions;
using ExplicitlyImpl.FluentActions.Test.UnitTests.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ExplicitlyImpl.FluentActions.Test.UnitTests
{
    public class BuilderTestsWithModelBinders
    {
        [Fact(DisplayName = "1 model binder (string), returns string")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderReturnsString()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .To(name => $"Hello {name}!"),
                typeof(ControllerWithModelBinderReturnsString),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "1 model binder (string), returns string async")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderReturnsStringAsync()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .To(async name => { await Task.Delay(1); return $"Hello {name}!"; }),
                typeof(ControllerWithModelBinderReturnsStringAsync),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "1 model binder (string) with name property, returns string")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithNamePropertyReturnsString()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), "NoOpName2")
                    .To(name => $"Hello {name}!"),
                typeof(ControllerWithModelBinderAndNamePropertyReturnsString),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "1 model binder (string) with name property, returns string async")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithNamePropertyReturnsStringAsync()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), "NoOpName2")
                    .To(async name => { await Task.Delay(1); return $"Hello {name}!"; }),
                typeof(ControllerWithModelBinderAndNamePropertyReturnsStringAsync),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "1 model binder (string) with used default value, returns string")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithUsedDefaultValueReturnsString()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), null, "Hanzel")
                    .To(name => $"Hello {name}!"),
                typeof(ControllerWithModelBinderAndDefaultValueReturnsString),
                new object[] { Type.Missing });
        }

        [Fact(DisplayName = "1 model binder (string) with used default value, returns string async")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithUsedDefaultValueReturnsStringAsync()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), null, "Hanzel")
                    .To(async name => { await Task.Delay(1); return $"Hello {name}!"; }),
                typeof(ControllerWithModelBinderAndDefaultValueReturnsStringAsync),
                new object[] { Type.Missing });
        }

        [Fact(DisplayName = "1 model binder (string) with unused default value, returns string")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithUnusedDefaultValueReturnsString()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), null, "Hanzel")
                    .To(name => $"Hello {name}!"),
                typeof(ControllerWithModelBinderAndDefaultValueReturnsString),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "1 model binder (string) with unused default value, returns string async")]
        public void FluentControllerBuilder_FluentActionUsingModelBinderWithUnusedDefaultValueReturnsStringAsync()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder), null, "Hanzel")
                    .To(async name => { await Task.Delay(1); return $"Hello {name}!"; }),
                typeof(ControllerWithModelBinderAndDefaultValueReturnsStringAsync),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "2 model binders (string, identical), returns string")]
        public void FluentControllerBuilder_FluentActionUsingTwoIdenticalModelBindersReturnsString()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .To((name1, name2) => $"Hello {name1}! I said hello {name2}!"),
                typeof(ControllerWithTwoIdenticalModelBindersReturnsString),
                new object[] { "Charlie" });
        }

        [Fact(DisplayName = "2 model binders (string, identical), returns string async")]
        public void FluentControllerBuilder_FluentActionUsingTwoIdenticalModelBindersReturnsStringAsync()
        {
            BuilderTestUtils.BuildActionAndCompareToStaticActionWithResult(
                new FluentAction("/route/url", HttpMethod.Get)
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .UsingModelBinder<string>(typeof(NoOpBinder))
                    .To(async (name1, name2) => { await Task.Delay(1); return $"Hello {name1}! I said hello {name2}!"; }),
                typeof(ControllerWithTwoIdenticalModelBindersReturnsStringAsync),
                new object[] { "Charlie" });
        }
    }
}
