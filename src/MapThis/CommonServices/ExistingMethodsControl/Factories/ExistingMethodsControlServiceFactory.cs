﻿using MapThis.CommonServices.ExistingMethodsControl.Dto;
using MapThis.CommonServices.ExistingMethodsControl.Factories.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using System.Collections.Generic;
using System.Composition;

namespace MapThis.CommonServices.ExistingMethodsControl.Factories
{
    [Export(typeof(IExistingMethodsControlServiceFactory))]
    public class ExistingMethodsControlServiceFactory : IExistingMethodsControlServiceFactory
    {
        public IExistingMethodsControlService Create(IList<ExistingMethodDto> existingMethodList)
        {
            return new ExistingMethodsControlService(existingMethodList);
        }
    }
}
