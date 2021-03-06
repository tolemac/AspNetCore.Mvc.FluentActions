﻿// Licensed under the MIT License. See LICENSE file in the root of the solution for license information.

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;

namespace ExplicitlyImpl.AspNetCore.Mvc.FluentActions
{
    public class FluentActionControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public FluentActionControllerFeatureProviderContext Context { get; set; }

        public FluentActionControllerFeatureProvider(FluentActionControllerFeatureProviderContext context)
        {
            Context = context;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controllerDefinition in Context.ControllerDefinitions)
            {
                if (!feature.Controllers.Contains(controllerDefinition.TypeInfo))
                {
                    feature.Controllers.Add(controllerDefinition.TypeInfo);
                }
            }
        }
    }

    public class FluentActionControllerFeatureProviderContext
    {
        public IEnumerable<FluentActionControllerDefinition> ControllerDefinitions { get; set; }
    }
}
