﻿// Licensed under the MIT License. See LICENSE file in the root of the solution for license information.

namespace ExplicitlyImpl.AspNetCore.Mvc.FluentActions
{
    public class FluentActionUsingRequestDefinition : FluentActionUsingDefinition
    {
        public override bool IsControllerProperty => true;

        public override string ControllerPropertyName => "Request";
    }
}
