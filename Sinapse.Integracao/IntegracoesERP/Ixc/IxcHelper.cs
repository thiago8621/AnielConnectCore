using Aniel.Connect.Core.Helpers;
using Newtonsoft.Json.Linq;
using RestSharp;
using Sinapse.Integracao.IntegracoesERP.Ixc.Models;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;

namespace Sinapse.Integracao.IntegracoesERP.Ixc
{
    public class IxcHelper : HttpHelper, IDisposable
    {

        private readonly string _url;
        private readonly string _token;

        public IxcHelper(string url, string token)
        {
            this._url = url;
            this._token = token;

        }

        public OrdemDeServicoResponse GetOs(string numOs)
        {
            var url = _url + "su_oss_chamado";
            var grid_param = new List<OrdemDeServicoParamRequest>();
            var request = new RequestIXC()
            {
                qtype = "su_oss_chamado.protocolo",
                query = numOs,
                oper = "=",
                page = "1",
                rp = "2000",
                sortname = "su_oss_chamado.id",
                sortorder = "desc",
                grid_param = JsonService.SerializeObject(grid_param)
            };

            var requestStr = JsonService.SerializeObject(request);
            var responseStr = Get(url, _token, requestStr, "listar");
            var retorno = JsonService.DeserializeObject<ResponseIXC<OrdemDeServicoResponse>>(responseStr);
            return retorno.registros != null ? retorno.registros.FirstOrDefault() : null;
        }

        public ResponseIXC<AssuntoResponse> GetAssunto(string id)
        {
            try
            {
                var url = _url + "su_oss_assunto";

                var request = new RequestIXC()
                {
                    qtype = "su_oss_assunto.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "su_oss_assunto.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<AssuntoResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            };
        }
        public ResponseIXC<ClienteResponse> GetCliente(string id)
        {
            try
            {
                var url = _url + "cliente";

                var request = new RequestIXC()
                {
                    qtype = "cliente.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "cliente.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<ClienteResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<CidadeResponse> GetCidade(string id)
        {
            try
            {
                var url = _url + "cidade";

                var request = new RequestIXC()
                {
                    qtype = "cidade.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "cidade.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<CidadeResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<UFResponse> GetUF(string id)
        {
            try
            {
                var url = _url + "uf";

                var request = new RequestIXC()
                {
                    qtype = "uf.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "uf.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<UFResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<LoginResponse> GetLogin(string id)
        {
            try
            {
                var url = _url + "radusuarios";

                var request = new RequestIXC()
                {
                    qtype = "radusuarios.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "20",
                    sortname = "radusuarios.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<LoginResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<CaixaFtthResponse> GetCaixaFtth(string id)
        {
            try
            {
                var url = _url + "rad_caixa_ftth";
                var request = new RequestIXC()
                {
                    qtype = "rad_caixa_ftth.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "20",
                    sortname = "rad_caixa_ftth.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<CaixaFtthResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<Cliente_ContratoResponse> GetClienteContrato(string id)
        {
            try
            {
                var url = _url + "cliente_contrato";

                var request = new RequestIXC()
                {
                    qtype = "cliente_contrato.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "cliente_contrato.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<Cliente_ContratoResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        public ResponseIXC<VdContratoResponse> GetVdContrato(string id)
        {
            try
            {
                var url = _url + "vd_contratos";

                var request = new RequestIXC()
                {
                    qtype = "vd_contratos.id",
                    query = $"{id}",
                    oper = "=",
                    page = "1",
                    rp = "1",
                    sortname = "vd_contratos.id",
                    sortorder = "desc"
                };

                var requestStr = JsonService.SerializeObject(request);
                var responseStr = Get(url, _token, requestStr, "listar");

                return JsonService.DeserializeObject<ResponseIXC<VdContratoResponse>>(responseStr);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na comunicação com o IXC: {ex.Message}");
            }
        }
        

        
    }
}