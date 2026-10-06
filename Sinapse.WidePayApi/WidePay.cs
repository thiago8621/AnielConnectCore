using RestSharp;
using Sinapse.Util;
using Sinapse.WidePayApi.Entity;
using Sinapse.WidePayApi.Server;
using Sinapse.WidePayApi.Server.Enums;
using Sinapse.WidePayApi.Server.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sinapse.WidePayApi
{
    public class WidePay
    {
        //private string versao = "v1";

        /// <summary>
        /// Registra uma cobrança no WidePay
        /// </summary>
        /// <param name="cobranca">Cobrança que será enviada para o WidePay</param>
        /// <returns></returns>

        public RetornoCobranca gerarCobranca(Cobranca cobranca, String id, String token)
        {
            WidePayBase wpb = new WidePayBase();

            List<Parameter> parametros = new List<Parameter>();

            //CAMPOS OBRIGATÓRIOS PARA A GERAÇÃO DO BOLETO
            parametros.Add(new Parameter("forma", getCobrancaValue(cobranca, "forma"), ParameterType.GetOrPost));
            parametros.Add(new Parameter("cliente", getCobrancaValue(cobranca, "cliente"), ParameterType.GetOrPost));
            parametros.Add(new Parameter("pessoa", getCobrancaValue(cobranca, "pessoa"), ParameterType.GetOrPost));
            parametros.Add(new Parameter(getCobrancaValue(cobranca, "pessoa").ToString().Equals("JURÍDICA") ? "cnpj" : "cpf",
              getCobrancaValue(cobranca, "cpf_cnpj"), ParameterType.GetOrPost));

            string itens = JsonService.SerializeJasonMapper((List<Item>)getCobrancaProperty(cobranca, "itens"));
            parametros.Add(new Parameter("itens", itens, ParameterType.GetOrPost));

            if ((getCobrancaValue(cobranca, "forma").ToString().Equals("Boleto,Pix")) && (getCobrancaValue(cobranca, "vencimento") != null))
            {
                DateTime data = Convert.ToDateTime(getCobrancaValue(cobranca, "vencimento"));
                parametros.Add(new Parameter("vencimento", String.Format("{0}-{1}-{2}", data.Year, data.Month.ToString().PadLeft(2, '0'), data.Day.ToString().PadLeft(2, '0')),
                    ParameterType.GetOrPost));
            }
            else if (getCobrancaValue(cobranca, "forma").ToString().Equals("Boleto,Pix"))
            {
                throw new NullReferenceException("Data de Vencimento não informada");
            }
            //FIM - CAMPOS OBRIGATÓRIOS PARA A GERAÇÃO DO BOLETO

            //CAMPOS OPCIONAIS
            var enviarEmail = getCobrancaValue(cobranca, "enviar");
            if ((enviarEmail != null) && (enviarEmail.ToString().Equals("E-MAIL")))
            {
                parametros.Add(new Parameter("enviar", enviarEmail, ParameterType.GetOrPost));
                parametros.Add(new Parameter("email", getCobrancaValue(cobranca, "email"), ParameterType.GetOrPost));
                parametros.Add(new Parameter("mensagem", getCobrancaValue(cobranca, "mensagem"), ParameterType.GetOrPost));
            }

            var telefone = getCobrancaProperty(cobranca, "telefone");
            if ((telefone != null) && (telefone.ToString().Length > 0))
                parametros.Add(new Parameter("telefone", telefone, ParameterType.GetOrPost));

            var end = getCobrancaProperty(cobranca, "endereco");
            if (end != null)
                parametros.Add(new Parameter("endereco", JsonService.SerializeJasonMapper((Endereco)end), ParameterType.GetOrPost));

            var referencia = getCobrancaValue(cobranca, "referencia");
            if ((referencia != null) && (referencia.ToString().Length > 0))
                parametros.Add(new Parameter("referencia", referencia, ParameterType.GetOrPost));

            var urlNotificacao = getCobrancaProperty(cobranca, "notificacao");
            if ((urlNotificacao != null) && (urlNotificacao.ToString().Length > 0))
                parametros.Add(new Parameter("notificacao", urlNotificacao, ParameterType.GetOrPost));

            if (!getCobrancaValue(cobranca, "forma").ToString().Equals("Boleto,Pix"))
            {
                var urlRedirecionamento = getCobrancaProperty(cobranca, "redirecionamento");
                if ((urlRedirecionamento != null) && (urlRedirecionamento.ToString().Length > 0))
                    parametros.Add(new Parameter("redirecionamento", urlRedirecionamento, ParameterType.GetOrPost));
                {
                    var boleto = getCobrancaValue(cobranca, "Boleto,Pix");
                    if (boleto != null)
                        parametros.Add(new Parameter("Boleto,Pix", JsonService.SerializeJasonMapper((Boleto)boleto), ParameterType.GetOrPost));
                }
                //FIM - CAMPOS OPCIONAIS
            }
            RetornoCobranca rc = wpb.PostHTTP<RetornoCobranca>(Chamada.Adicionar, id, token, parametros);
            return rc;
        }

        /// <summary>
        /// Carrega o boleto definido. Permite atualizar a data de vencimento do mesmo
        /// </summary>
        /// <param name="idBoleto">Identificador do Boleto gerado</param>
        /// <param name="vencimento">Nova Data de Vencimento para o boleto</param>
        /// <returns></returns>
        public RetornoCobrancaBoleto obterBoleto(String id, String token, String idBoleto, DateTime? vencimento = null)
        {
            List<Parameter> parametros = new List<Parameter>();
            parametros.Add(new Parameter("id", idBoleto, ParameterType.GetOrPost));

            if (vencimento != null)
            {

                parametros.Add(new Parameter("vencimento", String.Format("{0}-{1}-{2}", vencimento.Value.Year, vencimento.Value.Month.ToString().PadLeft(2, '0'),
                    vencimento.Value.Day.ToString().PadLeft(2, '0')),
                    ParameterType.GetOrPost));
            }

            WidePayBase wpb = new WidePayBase();

            RetornoCobrancaBoleto rbc = wpb.PostHTTP<RetornoCobrancaBoleto>(Chamada.Boleto, id, token, parametros);

            return rbc;
        }

        /// <summary>
        /// Retorna as últimas cobranças
        /// </summary>
        /// <param name="idCobrancas">Array com as IDs das Cobranças geradas, pode ser nulo caso deseja retornar as últimas</param>
        /// <returns></returns>
        public List<Cobranca> getCobrancas(String id, String token, String[] idCobrancas)
        {
            List<Parameter> parametros = new List<Parameter>();

            if (idCobrancas != null)
            {
                String ids = "[";
                foreach (String idCob in idCobrancas.ToList())
                {
                    if (ids.Count() > 1)

                        ids += String.Format(",\"{0}\"", idCob);
                    else
                        ids += String.Format("\"{0}\"", idCob);
                }
                ids += "]";

                parametros.Add(new Parameter("id", ids, ParameterType.GetOrPost));
            }

            WidePayBase wpb = new WidePayBase();

            RetornoCobrancas rcs = wpb.PostHTTP<RetornoCobrancas>(Chamada.Consultar, id, token, parametros);

            return rcs.cobrancas;
        }

        /// <summary>
        /// Cancelamento de uma cobrança
        /// </summary>
        /// <param name="idCobrancas">Array com as IDs das Cobranças geradas</param>
        /// <returns>Total de itens afetados</returns>
        public int cancelarCobranca(String id, String token, String[] idCobrancas)
        {
            List<Parameter> parametros = new List<Parameter>();

            if (idCobrancas != null)
            {
                String ids = "[";
                foreach (String idCob in idCobrancas.ToList())
                {
                    if (ids.Count() > 1)
                        ids += String.Format(",\"{0}\"", idCob);
                    else
                        ids += String.Format("\"{0}\"", idCob);
                }
                ids += "]";

                parametros.Add(new Parameter("id", ids, ParameterType.GetOrPost));
            }

            WidePayBase wpb = new WidePayBase();

            RetornoCobrancaCancelada rcc = wpb.PostHTTP<RetornoCobrancaCancelada>(Chamada.Cancelar, id, token, parametros);

            return rcc.total;
        }

        private object getCobrancaValue(Object obj, String name)
        {
            var field = typeof(Cobranca).GetMember(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if ((field == null) || (field.Count() == 0))
                return null;
            try
            {
                return ((FieldInfo)field[0]).GetValue(obj);

            }
            catch
            {
                return ((MemberInfo)(typeof(Cobranca).GetMember(name)[0]));
            }
        }


        private object getCobrancaProperty(Object obj, String name)
        {
            return obj.GetType().GetProperty(name).GetValue(obj);
        }
    }

}


