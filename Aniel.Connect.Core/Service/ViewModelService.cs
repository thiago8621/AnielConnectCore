using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aniel.Connect.Core.Service
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
                    return Resources.ResourceService.GetResource(displayAtt.ResourceType, displayAtt.Name);
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

                return Resources.ResourceService.GetResource(displayAtt.ResourceType, displayAtt.Name);
            }
            catch
            {
                return "";
            }
        }
    }
}
