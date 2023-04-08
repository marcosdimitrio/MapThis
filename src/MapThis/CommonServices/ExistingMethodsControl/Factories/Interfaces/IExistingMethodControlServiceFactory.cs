using MapThis.CommonServices.ExistingMethodsControl.Dto;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;

namespace MapThis.CommonServices.ExistingMethodsControl.Factories.Interfaces
{
    public interface IExistingMethodControlServiceFactory
    {
        IExistingMethodsControlService Create(IList<ExistingMethodDto> existingMethodList);
    }
}
