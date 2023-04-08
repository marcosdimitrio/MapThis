﻿using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;

namespace MapThis.Dto
{
    public class MapCollectionInformationDto
    {
        public MethodInformationDto MethodInformation { get; }
        public IMethodGenerator ChildMethodGenerator { get; }
        public OptionsDto Options { get; }

        public MapCollectionInformationDto(MethodInformationDto methodInformation, IMethodGenerator childMethodGenerator, OptionsDto options)
        {
            MethodInformation = methodInformation;
            ChildMethodGenerator = childMethodGenerator;
            Options = options;
        }

    }
}
