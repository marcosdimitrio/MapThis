using MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Dto;
using MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Factories.Interfaces
{
    public interface IExistingMethodControlFactory
    {
        IExistingMethodsControlService Create(IList<ExistingMethodDto> existingMethodList);
    }
}
