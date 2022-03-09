using MapThis.Services.ExistingMethodsControl.Dto;
using MapThis.Services.ExistingMethodsControl.Factories.Interfaces;
using MapThis.Services.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.Services.ExistingMethodsControl.Factories
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
