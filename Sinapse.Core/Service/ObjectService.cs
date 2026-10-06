using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Sinapse.Core
{
    public static class ObjectService
    {

        public static Boolean AreEquals(Object objetoEsperado, Object objetoAtual)
        {
            foreach (PropertyInfo property in objetoEsperado.GetType().GetProperties())
            {
                object valueObjEsperado = property.GetValue(objetoEsperado, null);
                object valueObjAtual = property.GetValue(objetoAtual, null);
                bool objetosDiferentes = false;
                String msgLista = String.Empty;

                if ((property.PropertyType.IsClass) && (property.PropertyType.IsSubclassOf(typeof(IModel))))
                {
                    if ((valueObjEsperado == null) && (valueObjAtual!=null))
                        objetosDiferentes = true;
                    else
                        if ((valueObjEsperado != null) && (valueObjAtual == null))
                            objetosDiferentes = true;
                        else
                            if ((valueObjEsperado != null) && (valueObjAtual != null))
                            {
                                if (((IModel)valueObjEsperado).Id != ((IModel)valueObjAtual).Id)
                                    objetosDiferentes = true;
                            }
                }
                else
                    if ((property.PropertyType.IsGenericType) &&                        
                        (property.PropertyType.GetGenericTypeDefinition() == Type.GetType("System.Collections.Generic.IList`1")))
                    {
                        IList listaEsperado = (IList)valueObjEsperado;
                        IList listaAtual = (IList)valueObjAtual;

                        foreach (object objEsperado in listaEsperado)
                        {
                            if ((listaAtual == null) || (listaAtual.Count == 0))
                            {
                                msgLista = "Não foram encontrados objetos na lista recuperada.";
                                objetosDiferentes = true;
                            }
                            else
                            {
                                bool encontrou = false;

                                foreach (object objAtual in listaAtual)
                                {
                                    if (((IModel)objAtual).Id == ((IModel)objEsperado).Id)
                                    {
                                        encontrou = true;
                                        break;
                                    }
                                }

                                if (!encontrou)
                                {
                                    objetosDiferentes = true;

                                    msgLista = String.Format("O ID {0} não foi encontrado na lista do objeto atual.", ((IModel)objEsperado).Id);
                                }
                            }

                            if (objetosDiferentes)
                                break;
                        }
                    }
                    else
                        if (valueObjEsperado != null)
                        {
                            if (!(valueObjEsperado.Equals(valueObjAtual)))
                            {
                                objetosDiferentes = true;
                            }
                        }else
                            if ((valueObjEsperado == null) && (valueObjAtual != null))
                            {
                                objetosDiferentes = true;
                            }


                if(objetosDiferentes)
                {
                    String mensagem = String.Format("{0}.{1} estão divergentes. \n", objetoEsperado.GetType().Name, property.Name);

                    if (msgLista == String.Empty)
                    {

                        if (valueObjEsperado != null)
                            mensagem += String.Format("Valor Esperado:{0}\n", valueObjEsperado);
                        else
                            mensagem += "Valor Esperado: null \n";

                        if (valueObjAtual != null)
                            mensagem += String.Format("Valor Atual:{0}\n", valueObjAtual);
                        else
                            mensagem += "Valor Atual: null \n";
                    }
                    else
                        mensagem += msgLista;

                    throw new DiferentObjectsException(mensagem);
                }
            }
            return true;
        }

        public static void CopyTo(List<Object> listaDe, List<Object> listaPara)
        {
            foreach (Object objDe in listaDe)
            {
                Object ObjNovo = listaPara.GetType().GetGenericTypeDefinition().GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (PropertyInfo prop in objDe.GetType().GetProperties())
                {
                    try
                    {
                        ObjNovo.GetType().GetProperty(prop.Name).SetValue(ObjNovo, prop.GetValue(objDe, Type.EmptyTypes), Type.EmptyTypes);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        public static void CopyTo(Object de, Object para)
        {
            if (de == null)
            {
                throw new Exception("Objeto 'de' não informado - copyto");
            }
            if (para == null)
            {
                throw new Exception("Objeto 'para' não informado - copyto");
            }


            foreach (PropertyInfo prop in de.GetType().GetProperties())
            {
                try
                {
                    para.GetType().GetProperty(prop.Name).SetValue(para, prop.GetValue(de, Type.EmptyTypes), Type.EmptyTypes);
                }
                catch
                {
                    continue;
                }
            }
            
        }

        public static Boolean IsInterfaceOf(Type pClassType, Type pInterfaceType)
        {
            return pClassType.GetInterface(pInterfaceType.ToString()) != null;
        }

    }
}
