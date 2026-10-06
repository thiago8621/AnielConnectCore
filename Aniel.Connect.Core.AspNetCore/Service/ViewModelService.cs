using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Aniel.Connect.Core.AspNetCore.Service
{
    public static class ViewModelService
    {
        public static String GetDescricaoNaoRequeridos(Type TipoViewModel, String CampoModel)
        {
            PropertyInfo propViewModel = TipoViewModel.GetProperty(CampoModel);

            try
            {
                DisplayAttribute displayAtt = (DisplayAttribute)propViewModel.GetCustomAttribute(typeof(DisplayAttribute), false);

                if (propViewModel.GetCustomAttribute(typeof(RequiredAttribute), false) != null)
                    return "";
                else
                    return ResourcesCore.AspNetCore.ResourceService.GetResource(displayAtt.ResourceType, displayAtt.Name);
            }
            catch
            {
                return "";
            }
        }

        public static String GetDescricao(Type TipoViewModel, String CampoModel)
        {
            PropertyInfo propViewModel = TipoViewModel.GetProperty(CampoModel);

            try
            {
                DisplayAttribute displayAtt = (DisplayAttribute)propViewModel.GetCustomAttribute(typeof(DisplayAttribute), false);

                return ResourcesCore.AspNetCore.ResourceService.GetResource(displayAtt.ResourceType, displayAtt.Name);
            }
            catch
            {
                return "";
            }
        }
    }
}
