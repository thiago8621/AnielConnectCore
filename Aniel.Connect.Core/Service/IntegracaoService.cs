using Aniel.MGTEC.Core.Models;
using Sinapse.Core.Attributes;
using Sinapse.Core.Entity;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Aniel.MGTEC.Core.Service
{
    public static class IntegracaoService
    {
        public static List<Type> GetTiposPermitidos()
        {
            List<Type> tipos = new List<Type>();
            tipos.Add(typeof(String));
            tipos.Add(typeof(Int16));
            tipos.Add(typeof(Int32));
            tipos.Add(typeof(Int64));
            tipos.Add(typeof(Double));
            tipos.Add(typeof(DateTime));
            tipos.Add(typeof(Boolean));

            return tipos;
        }

        public static List<IntegracaoTipoObjeto> GetTiposObjetos(Assembly assembly)
        {
            List<IntegracaoTipoObjeto> retorno = new List<IntegracaoTipoObjeto>();

            foreach (var tipo in assembly.GetTypes()
                .Where(x => Attribute.IsDefined(x, typeof(IntegracaoAttribute), false))
                .ToArray())
            {
                foreach (var metodo in tipo.GetMethods()
                    .Where(x => Attribute.IsDefined(x, typeof(IntegracaoMetodoAttribute), false))
                    .ToArray())
                {
                    IntegracaoMetodoAttribute att = (IntegracaoMetodoAttribute)metodo.GetCustomAttribute(typeof(IntegracaoMetodoAttribute), false);
                    retorno.Add(new IntegracaoTipoObjeto() { TipoControle = tipo, MetodoControle = metodo, NomeObjeto = att.NomeObjeto, TipoViewModel = att.TipoParametro.FullName });
                }
            }

            return retorno;
        }

        public static String GetDescritipoCampos(Type pTipo)
        {
            StringBuilder retorno = new StringBuilder();

            List<Type> tiposPermitidos = GetTiposPermitidos();

            foreach (PropertyInfo prop in pTipo.GetProperties())
            {
                if (tiposPermitidos.Contains(prop.PropertyType))
                {
                    String Descricao = ViewModelService.GetDescricao(pTipo, prop.Name);
                    String Tipo = Resources.ResourceService.GetResource(typeof(Resources.RsIntegracao), prop.PropertyType.Name.ToUpper());
                    Boolean ReadOnly = prop.GetCustomAttribute(typeof(IntegracaoReadOnlyAttribute), false) != null;
                    

                    if (prop.PropertyType == typeof(String))
                    {
                        MaxLengthAttribute attMax = (MaxLengthAttribute)prop.GetCustomAttribute(typeof(MaxLengthAttribute), false);
                        if (attMax != null)
                            Tipo += String.Format(" ({0})", attMax.Length);
                    }

                    if (!String.IsNullOrEmpty(Descricao) && !ReadOnly)
                        retorno.AppendLine(String.Format("<b>{0}</b> - {1} <br/>", Descricao, Tipo));
                }
            }

            return retorno.ToString();
        }

        public static List<IntegracaoCampo> GetCampos(Type pTipo)
        {
            List<IntegracaoCampo> retorno = new List<IntegracaoCampo>();

            List<Type> tiposPermitidos = GetTiposPermitidos();

            foreach (PropertyInfo prop in pTipo.GetProperties())
            {
                if (tiposPermitidos.Contains(prop.PropertyType))
                {
                    String Descricao = ViewModelService.GetDescricao(pTipo, prop.Name);

                    Boolean Requirido = prop.GetCustomAttribute(typeof(RequiredAttribute), false) != null;

                    Boolean ReadOnly = prop.GetCustomAttribute(typeof(IntegracaoReadOnlyAttribute), false) != null;

                    if (!String.IsNullOrEmpty(Descricao) && !ReadOnly)
                        retorno.Add(new IntegracaoCampo() { PropName = prop.Name, Descricao = Descricao, Requirido = Requirido });
                }
            }

            return retorno;
        }


        /// <summary>
        /// Faz a importação do objeto chamando 
        /// </summary>
        /// <param name="tipoViewModel"></param>
        /// <param name="camposParaDe"></param>
        /// <param name="Valores"></param>
        /// <param name="controle"></param>
        /// <param name="metodo"></param>
        /// <returns>Retorna Mensagens da importação</returns>
        public static List<IntegracaoStatusImportacao> ImportarObjetos(Type tipoViewModel, 
            Dictionary<String, ExcelTitulo> camposParaDe, 
            List<ExcelValor> Valores, 
            BaseController controle, 
            MethodInfo metodo,
            Boolean Importar)
        {
            List<IntegracaoStatusImportacao> retorno = new List<IntegracaoStatusImportacao>();
            Int32 linha = 2;
            foreach (ExcelValor valor in Valores)
            {
                Object objeto = tipoViewModel.GetConstructor(Type.EmptyTypes).Invoke(null);

                try
                {
                    //Preenche as propriedades do ModelView
                    foreach (String key in camposParaDe.Keys)
                    {
                        Object propValue = valor.Valor[camposParaDe[key].Coluna];

                        try
                        {
                            tipoViewModel.GetProperty(key).SetValue(objeto, Convert.ChangeType(propValue, tipoViewModel.GetProperty(key).PropertyType));
                        }
                        catch
                        {
                            throw new Exception(String.Format(Resources.Resources.INTEGRACAO_TIPOINVALIDO, key, camposParaDe[key].Descricao, tipoViewModel.GetProperty(key).PropertyType.Name));
                        }
                    }


                    var results = new List<ValidationResult>();
                    var validationContext = new ValidationContext(objeto, null, null);
                    Validator.TryValidateObject(objeto, validationContext, results, true);

                    if (objeto is IValidatableObject)
                        results.AddRange((objeto as IValidatableObject).Validate(validationContext));

                    foreach (var erro in results)
                        retorno.Add(new IntegracaoStatusImportacao() { Linha = linha, Mensagem = erro.ErrorMessage, Status = IntegracaoTipoStatus.ErroNaImportacao });

                    if (results.Count == 0) // Caso não tenha erro chama o metodo de persistir
                    {
                        if (Importar)
                            metodo.Invoke(controle, new object[] { objeto });

                        if (!Importar)
                            retorno.Add(new IntegracaoStatusImportacao() { Status = IntegracaoTipoStatus.ProntoParaImportar, Linha = linha, Mensagem = Resources.RsIntegracao.PRONTOPARAIMPORTAR });
                        else
                            retorno.Add(new IntegracaoStatusImportacao() { Status = IntegracaoTipoStatus.Importado, Linha = linha, Mensagem = Resources.RsIntegracao.IMPORTADO });
                    }

                }
                catch (Exception ex)
                {
                    retorno.Add(new IntegracaoStatusImportacao()
                    {
                        Linha = linha,
                        Mensagem = String.Format("{0} - {2}", Resources.Resources.INTEGRACAO_ERROOBJETO, ex.Message),
                        Status = IntegracaoTipoStatus.ErroNaImportacao
                    });
                }

                linha++;
            }

            return retorno;

        }

    }
}
