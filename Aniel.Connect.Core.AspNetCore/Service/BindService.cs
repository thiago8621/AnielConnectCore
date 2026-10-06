using Sinapse.Model.AspNetCore.nhibernate;
using System;
using System.Linq;
using System.Reflection;

namespace Aniel.Connect.Core.AspNetCore.Service
{
    public static class BindService
    {
        public static O BindViewModelToObj<K, O>(IViewModel<K> pView, O pObj)
        {
            O Model = pObj;

            if (pObj == null)
                Model = (O)typeof(O).GetType().GetConstructor(Type.EmptyTypes).Invoke(null);

            PropertyInfo[] propsView = pView.GetType().GetProperties()
                                                      .Where(x => Attribute.IsDefined(x, typeof(BindModelViewAttribute), false))
                                                      .ToArray();
            foreach (var prop in propsView)
            {
                BindModelViewAttribute attProp = (BindModelViewAttribute)prop.GetCustomAttribute(typeof(BindModelViewAttribute), false);
                String namePropModel = attProp.Property ?? prop.Name;

                try
                {
                    Model.GetType().GetProperty(namePropModel).SetValue(Model, prop.GetValue(pView));
                }
                catch
                {
                    throw new Exception(String.Concat("Problemas ao fazer bind entre objetos: ", pObj.GetType().Name, "-", namePropModel, "(Obj), ", pView.GetType().Name, "-", prop.Name, "(Model View)"));
                }
            }

            return Model;
        }

        public static void BindObjToViewModel<O, K>(O pObj, IViewModel<K> pView) 
        {
            PropertyInfo[] propsView = pView.GetType().GetProperties()
                                                     .Where(x => Attribute.IsDefined(x, typeof(BindModelViewAttribute), false))
                                                     .ToArray();
            foreach (var prop in propsView)
            {
                BindModelViewAttribute attProp = (BindModelViewAttribute)prop.GetCustomAttribute(typeof(BindModelViewAttribute), false);
                String namePropModel = attProp.Property ?? prop.Name;

                try
                {
                    Object valueObject = pObj.GetType().GetProperty(namePropModel).GetValue(pObj);
                    pView.GetType().GetProperty(prop.Name).SetValue(pView, valueObject);
                }
                catch
                {
                    throw new Exception(String.Concat("Problemas ao fazer bind entre objetos: ", pObj.GetType().Name, "-", namePropModel, "(Obj), ", pView.GetType().Name, "-", prop.Name, "(Model View)"));
                }
            }
        }
    }
}
