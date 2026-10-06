using Aniel.Connect.ControllersWebApi;
using Aniel.Connect.Models.T5;
using Microsoft.AspNet.Identity;
using NHibernate;
using RestSharp;
using Sinapse.DAO;
using Sinapse.DAO.repository;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;


namespace Aniel.Connect.Core
{

    public class BaseApiController : ApiController
    {
        private List<AreaModuloModel> _areas = null;

        public List<AreaModuloModel> Areas
        {
            get
            {
                if (this._areas == null)
                {
                    this._areas = GetAreas(User.Identity.GetUserId());
                }
                return (List<AreaModuloModel>)this._areas;
            }
        }
        private List<AreaModuloModel> GetAreas(String Id_Usuario)
        {
            using (var api = new AcessoApiController())
            {
                GetApiRetorno<List<AreaModuloModel>> retorno = api.GetArea_ModuloUsuario(Id_Usuario).Result;

                return retorno.Objeto;
            }
        }

        #region AutoComplete
        public async static Task<String> GetTokenByLogin(String pLogin)
        {
            using (ISession session = HibernateUtil.GetSession())
            {
                using (IdentityUserRepository service = new IdentityUserRepository(session, null))
                {
                    try
                    {
                        return (await service.FindByWhere(p => p.UserName == pLogin)).Token;
                    }
                    catch
                    {
                        throw new Exception(String.Format("Não foi possível pegar token do login {0}", pLogin));
                    }
                }
            }
        }
        #endregion

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
            CONSTANTES.log4Net.Error($"{MessageErro} - {MessageInnerErro2}", new Exception(MessageInnerErro));

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
            if (CONSTANTES.TrataErro)
            {
                if (ErroServidor)
                {
                    ModelState.AddModelError("", Resources.RsBase.ERROAORETORNARDADOSDOSERVIDOR);
                }
                else
                {
                    String[] ignorados = new String[] { "FROM", "NHIBERNATE", "DATETIME", "INT32", "DOUBLE", "INT64", "STRING", "INT16", "FLOAT", "PARSE", "BOOLEAN" };

                    foreach (var ignorado in ignorados)
                    {
                        if (Erro.ToUpperInvariant().Contains(ignorado))
                        {
                            Erro = Resources.RsBase.ERROAORETORNARDADOSDOSERVIDOR;
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


        [HttpPost]
        public async Task<PostApiRetorno<Int64>> PostAsync<T>(String pUrlPortal, String pUrlWebApi, T obj) where T : new()
        {
            var client = new RestClient($"{pUrlPortal}/{pUrlWebApi}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(obj);

            IRestResponse<PostApiRetorno<Int64>> response = await client.ExecuteAsync<PostApiRetorno<Int64>>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Post Status: {response.Data.Ok} Url: {pUrlWebApi} Obj: {JsonService.SerializeObject(obj)}");
            }
        }


        [HttpPost]
        public async Task<PostApiRetorno<String>> PostAsyncString<T>(String pUrlPortal, String pUrlWebApi, T obj) where T : new()
        {
            var client = new RestClient($"{pUrlPortal}/{pUrlWebApi}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(obj);

            IRestResponse<PostApiRetorno<String>> response = await client.ExecuteAsync<PostApiRetorno<String>>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Post Status: {response.Data.Ok} Url: {pUrlWebApi} Obj: {JsonService.SerializeObject(obj)}");
            }
        }

        [HttpPost]
        public GetApiRetorno<T> Post<T>(String pUrlPortal, String pUrlWebApi, object obj) where T : new()
        {
            var client = new RestClient($"{pUrlPortal}/{pUrlWebApi}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");
            request.AddJsonBody(obj);

            IRestResponse<GetApiRetorno<T>> response = client.Execute<GetApiRetorno<T>>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Post Status: {response.StatusCode} Url: {pUrlWebApi} Obj: {JsonService.SerializeObject(obj)}");
            }
        }

        [HttpPost]
        public T PostIntegracao<T>(String pUrlIntegracao, object obj, Dictionary<string, string> headers) where T : new()
        {
            var client = new RestClient($"{pUrlIntegracao}");
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            if (headers.Count() > 0)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            request.AddJsonBody(obj);

            IRestResponse<T> response = client.Execute<T>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Post Status: {response.StatusCode} Url: {pUrlIntegracao} Obj: {JsonService.SerializeObject(obj)}");
            }
        }

        [HttpPost]
        public T GetIntegracao<T>(String pUrlIntegracao, Dictionary<string, string> headers) where T : new()
        {
            var client = new RestClient($"{pUrlIntegracao}");
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "d6a5b694-ec96-c2cd-ff0a-5e9efe83f981");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            if (headers.Count() > 0)
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);

            IRestResponse<T> response = client.Execute<T>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                throw new Exception($"Erro Get Status: {response.StatusCode} Url: {pUrlIntegracao}");
            }
        }

        /// <summary>
        /// Execute WebApi no padrão Sinapse para retornar valores via get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pUrlPortal">CONSTANTES.URLPortal</param>
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
