using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Sinapse.DAO.AspNetCore;
using Sinapse.Util.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aniel.Connect.Core.AspNetCore
{

    public class BaseApiController : Controller
    {
        // O IdentityUserRepository depende da estrutura de identity do ASP.NET antigo e não foi migrado para ASP.NET Core

        //public async static Task<String> GetTokenByLogin(String pLogin)
        //{
        //    using (ISession session = HibernateUtil.GetSession())
        //    {
        //        using (IdentityUserRepository service = new IdentityUserRepository(session, null))
        //        {
        //            try
        //            {
        //                return (await service.FindByWhere(p => p.UserName == pLogin)).Token;
        //            }
        //            catch
        //            {
        //                throw new Exception(String.Format("Não foi possível pegar token do login {0}", pLogin));
        //            }
        //        }
        //    }
        //}

        #region ModelError

        protected void ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var mensagens = from o in ModelState.SelectMany(x => x.Value.Errors)
                                select o.ErrorMessage;

                throw new Exception(String.Join("", mensagens));
            }
        }

        protected void AddModelError(String MessageErro, String MessageInnerErro = "", String MessageInnerErro2 = "")
        {
            CONSTANTES_CORE.log4Net.Error($"{MessageErro} - {MessageInnerErro2}", new Exception(MessageInnerErro));

            ModelState.Clear();

            if (String.IsNullOrEmpty(MessageInnerErro))
            {
                TrataMensagemErro(MessageErro, false);
            }
            else if ((MessageInnerErro.StartsWith("[") && MessageInnerErro.EndsWith("]")))
            {
                try
                {
                    foreach (String erro in JsonService.DeserializeJasonMapper<List<String>>(MessageInnerErro))
                    {
                        TrataMensagemErro(erro, true);
                    }
                }
                catch
                {
                    TrataMensagemErro(MessageErro, true);
                }
            }
            else
            {
                TrataMensagemErro(MessageErro, false);
            }
        }

        private void TrataMensagemErro(String Erro, Boolean ErroServidor)
        {
            if (CONSTANTES_CORE.TrataErro)
            {
                if (ErroServidor)
                {
                    ModelState.AddModelError("", ResourcesCore.AspNetCore.RsBase.ERROAORETORNARDADOSDOSERVIDOR);
                }
                else
                {
                    String[] ignorados = new String[] { "FROM", "NHIBERNATE", "DATETIME", "INT32", "DOUBLE", "INT64", "STRING", "INT16", "FLOAT", "PARSE", "BOOLEAN" };

                    foreach (var ignorado in ignorados)
                    {
                        if (Erro.ToUpperInvariant().Contains(ignorado))
                        {
                            Erro = ResourcesCore.AspNetCore.RsBase.ERROAORETORNARDADOSDOSERVIDOR;
                            break;
                        }
                    }

                    ModelState.AddModelError("", Erro);
                }
            }
            else
            {
                ModelState.AddModelError("", Erro);
            }
        }

        protected void AddModelError(IEnumerable<String> erros)
        {
            ModelState.Clear();

            foreach (var error in erros)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected void AddModelError(Dictionary<String, String> erros)
        {
            ModelState.Clear();

            foreach (var key in erros.Keys)
            {
                ModelState.AddModelError(key, erros[key]);
            }
        }

        #endregion

        #region HttpRequest
        [HttpPost]
        public PostApiRetorno Post<T>(String pUrlPortal, String pUrlWebApi, T obj) where T : new()
        {
            var client = new RestClient($"{pUrlPortal}/{pUrlWebApi}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(obj);

            IRestResponse<PostApiRetorno> response = client.Execute<PostApiRetorno>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Post Status: {response.StatusCode} Url: {pUrlWebApi} Obj: {JsonService.SerializeObject(obj)}");
            }
        }
        /// <summary>
        /// Execute WebApi no padrão Sinapse para retornar valores via get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pUrlPortal">CONSTANTES_CORE.URLPortal</param>
        /// <param name="pUrlWebApi"> "api/empresaapi/?t={0}&p={1}" ou "api/empresaapi?id={0}" </param>
        /// <returns></returns>
        protected GetApiRetorno<T> GetByUrl<T>(String pUrlPortal, String pUrlWebApi)
        {
            var client = new RestClient($"{pUrlPortal}/{pUrlWebApi}");

            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "");
            request.AddHeader("Accept", "application/json");

            var response = client.Execute<GetApiRetorno<T>>(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Erro PostByUrl Status: {response.StatusCode} Url: {pUrlWebApi} Erro: {response.ErrorMessage} {response.ErrorException}");
            }

            return JsonService.DeserializeObject<GetApiRetorno<T>>(response.Content);
        }

        #endregion
    }
}
