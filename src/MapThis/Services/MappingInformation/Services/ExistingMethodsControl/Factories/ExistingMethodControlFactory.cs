using MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Dto;
using MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Factories
{
    [Export(typeof(IExistingMethodControlFactory))]
    public class ExistingMethodControlFactory : IExistingMethodControlFactory
    {
        public IExistingMethodsControlService Create(IList<ExistingMethodDto> existingMethodList)
        {
            return new ExistingMethodsControlService(existingMethodList);
        }
    }
}
