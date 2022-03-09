using MapThis.Services.ExistingMethodsControl.Dto;
using MapThis.Services.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;

namespace MapThis.Services.ExistingMethodsControl.Factories.Interfaces
{
    public interface IExistingMethodControlFactory
    {
        IExistingMethodsControlService Create(IList<ExistingMethodDto> existingMethodList);
    }
}
