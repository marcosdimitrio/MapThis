﻿using Microsoft.CodeAnalysis;

namespace MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Dto
{
    public class ExistingMethodDto
    {
        public INamedTypeSymbol SourceType { get; set; }
        public INamedTypeSymbol TargetType { get; set; }
    }
}
