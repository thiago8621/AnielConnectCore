using Aniel.PontoEletronico.Core;
using Aniel.PontoEletronico.Core.Helpers;
//using Aniel.PontoEletronico.Model.enumerado;
//using Aniel.PontoEletronico.Model.nhibernate;
using Aniel.PontoEletronico.Models;
using NHibernate;
using NHibernate.Criterion;
using Sinapse.DAO.repository;
using Sinapse.Model.enumerados;
using Sinapse.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using static Aniel.PontoEletronico.Models.DepartamentoCadastroModel;

namespace Aniel.PontoEletronico.ControllersWebApi
{
    public class DepartamentoApiController : BaseApiController
    {

        [HttpPost()]
        [Route("api/departamentoapi/GetAll")]
        public async Task<DataTableResponse<DepartamentoListaModel>> GetAll(Departamento_DataTableRequest request)
        {
            List<DepartamentoListaModel> retorno = new List<DepartamentoListaModel>();
            IOrderedEnumerable<DepartamentoListaModel> listaOrdenada = null;
            Int32 recordsTotal = 0;

            using (ISession session = HibernateUtil.GetSession())
            {

                Dictionary<String, Object> parametros = new Dictionary<string, object>();

                //Count
                using (var com = session.Connection.CreateCommand())
                {
                    StringBuilder sql = new StringBuilder("SELECT COUNT(A.ID) CO FROM TB_TIPO_ATENDIMENTO A ")
                        .AppendLine("WHERE STATUS = 1");

                    using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                    {
                        if (await reader.ReadAsync())
                        {
                            recordsTotal = Convert.ToInt32(reader["CO"]);
                        }
                    }
                }

                //Select
                using (var com = session.Connection.CreateCommand())
                {

                    StringBuilder sql = new StringBuilder();

                    sql.AppendLine("SELECT A.ID, A.NOME, B.NOME AS NOME_SCRIPT, A.PRIORIDADE_PADRAO, A.STATUS ");
                    sql.AppendLine("FROM TB_TIPO_ATENDIMENTO A");
                    sql.AppendLine("LEFT JOIN TB_SCRIPT_ATENDIMENTO B");
                    sql.AppendLine("ON (A.ID_SCRIPT = B.ID)");
                    //  sql.AppendLine("WHERE A.STATUS = 1 ");

                    //Filtrando
                    if (!String.IsNullOrEmpty(request.search.value))
                    {
                        sql.AppendLine("WHERE (UPPER(A.NOME) LIKE UPPER(@SEARCH)) ");
                        parametros.Add("@SEARCH", String.Format("%{0}%", request.search.value.ToUpperInvariant()));
                    }


                    //Busca com paginacao
                    using (var reader = await CommandHelper.ExecuteReaderFirst(com, sql, parametros, session.Connection, null, request.length, request.start))
                    {
                        while (await reader.ReadAsync())
                        {
                            retorno.Add(new DepartamentoListaModel
                            {
                                Id = Convert.ToInt64(reader["ID"]),
                                Codigo = Convert.ToString(reader["CODIGO"]),
                                Descricao = Convert.ToString(reader["DESCRICAO"]),
                                Status = HtmlEnumExtensions.GetEnumDescription<Status_Default>(Convert.ToString(reader["STATUS"]))
                            });
                        }
                    }

                }
            }

            //Ordenacao
            if (request.order != null && request.order.Any())
            {
                int sortColumnIndex = request.order[0].column;
                string sortDirection = request.order[0].dir;
                switch (sortColumnIndex)
                {
                    case 1:     // Nome
                        {
                            if (sortDirection == "asc")
                                listaOrdenada = retorno.OrderBy(p => p.Descricao);
                            else
                                listaOrdenada = retorno.OrderByDescending(p => p.Codigo);
                            break;
                        }
                }
            }

            //Page e retornando DataTable
            return new DataTableResponse<DepartamentoListaModel>
            {
                draw = request.draw,
                //recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = listaOrdenada,
                error = ""
            };

        }


        [HttpPost()]
        public async Task<GetApiRetorno<IEnumerable<DepartamentoListaModel>>> GetList()
        {
            GetApiRetorno<IEnumerable<DepartamentoListaModel>> retorno = new GetApiRetorno<IEnumerable<DepartamentoListaModel>>();

            using (ISession session = HibernateUtil.GetSession())
            {
                try
                {
                    var lista = await new Conexao<Tipo_Atendimento>(session).select(p => p.Status == Status_Default.Ativo);

                    var listaRetorno = new List<Tipo_AtendimentoListaModel>();

                    foreach (var obj in lista)
                    {
                        Tipo_AtendimentoListaModel tmp = new Tipo_AtendimentoListaModel
                        {
                            Id = obj.Id,
                            Nome = obj.Nome,
                            Categoria_Atendimento = Convert.ToString(obj.Categoria_Atendimento?.Id),
                            Prioridade_Padrao = Convert.ToString(obj.Prioridade_Padrao.Value),
                            Criar_Os_Encerrar = Convert.ToString(obj.Criar_Os_Encerrar),
                            Exibir_Ola = Convert.ToString(obj.Exibir_Ola)
                        };

                        tmp.Script = obj.Script?.Nome;

                        listaRetorno.Add(tmp);
                    }

                    retorno.Objeto = listaRetorno;

                    retorno.Ok = true;
                }
                catch (Exception ex)
                {
                    retorno.Mensagem = ex.Message;
                    if (ex.InnerException != null) retorno.InnerException += ex.InnerException.Message;
                }
            }
            return retorno;

        }


        [HttpPost()]
        [Route("api/DepartamentoApi/GetCombo")]
        public async Task<ObjCombo<Int64>> GetCombo(RequestObjCombo<Int64?> request)
        {
            ObjCombo<Int64> retorno = new ObjCombo<Int64>
            {
                results = new List<IObjComboResult<Int64>>()
            };

            using (ISession session = HibernateUtil.GetSession())
            {
                try
                {

                    //Filtros 
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();

                    StringBuilder sqlFrom = new StringBuilder("FROM TB_DEPARTAMENTO A ");

                    List<String> condicoes = new List<String>();

                    if (request.soativos)
                    {
                        condicoes.Add($"A.STATUS = 1");
                    }


                    if (request.id.HasValue)
                    {
                        condicoes.Add($"A.ID = {request.id}");
                    }

                    if (request != null && !String.IsNullOrEmpty(request.q))
                    {
                        condicoes.Add($"( UPPER(A.DESCRICAO) LIKE '%{StringService.Truncate(request.q.ToUpperInvariant(), 60)}%')");
                    }

                    if (condicoes.Count() > 0)
                    {
                        sqlFrom.AppendLine($"WHERE ( {String.Join(" AND ", condicoes) } ) ");
                    }

                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder("SELECT COUNT(A.ID) AS CO ");
                        sql.AppendLine(sqlFrom.ToString());

                        using (var reader = await CommandHelper.ExecuteReader(com, sql, parametros, session.Connection, null))
                        {
                            if (await reader.ReadAsync())
                                retorno.total_count = Convert.ToInt32(reader["CO"]);
                        }
                    }

                    //Select
                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder("SELECT A.ID, A.NOME, A.ID_CATEGORIA_ATENDIMENTO,A.PRIORIDADE_PADRAO,A.ID_SCRIPT,A.CRIAR_OS_ENCERRAR ");
                        sql.AppendLine(sqlFrom.ToString());
                        sql.AppendLine("ORDER BY A.NOME");

                        using (var reader = await CommandHelper.ExecuteReaderPage(com, sql, parametros, session.Connection, null, request.page))
                        {
                            while (await reader.ReadAsync())
                            {
                                var obj = new DepartamentoComboModel
                                {
                                    Id = Convert.ToInt32(reader["ID"]),
                                    Text = Convert.ToString(reader["NOME"]),
                                    Categoria_Atendimento = Convert.ToString(reader["ID_CATEGORIA_ATENDIMENTO"]),
                                    Prioridade_Padrao = Convert.ToString(reader["PRIORIDADE_PADRAO"]),
                                    Script = Convert.ToString(reader["ID_SCRIPT"]),
                                    Criar_Os_Encerrar = Convert.ToString(reader["CRIAR_OS_ENCERRAR"])
                                };
                                retorno.results.Add(obj);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return retorno;

        }


        private Tipo_AtendimentoCadastroModel GetViewObj(Tipo_Atendimento obj)
        {
            return new Tipo_AtendimentoCadastroModel
            {
                Id = obj.Id,
                Id_Script = obj.Script != null ? obj?.Script?.Id : (Int64?)null,
                Nome = obj.Nome,
                Prioridade_Padrao = obj?.Prioridade_Padrao,
                Status = obj.Status == Status_Default.Ativo,
                Id_Categoria_Atendimento = obj.Categoria_Atendimento != null ? obj?.Categoria_Atendimento?.Id : (Int64?)null,
                Criar_Os_Encerrar = obj.Criar_Os_Encerrar,
                Exibir_Ola = obj.Exibir_Ola,
                Desc_Tipo_Servico = obj?.Desc_Tipo_Servico,
                CodCt = obj.CodCt,
                Id_Script_SuporteTecnico = obj?.Script_SuporteTecnico?.Id,
                Motivos = (obj.Motivos.Count() == 0) ? new List<Tipo_AtendimentoMotivoCadastroModel>() :
                                                   obj.Motivos.Select(p => new Tipo_AtendimentoMotivoCadastroModel
                                                   {
                                                       Id = p.Id,
                                                       Id_Motivo = p.Motivo == null ? 0 : p.Motivo.Id,
                                                       Desc_Motivo = p.Motivo == null ? String.Empty : p.Motivo.Descricao
                                                   }).ToList()
            };
        }

        private void BindViewToObj(ISession session, ref Tipo_Atendimento objBanco, Tipo_AtendimentoCadastroModel view)
        {
            objBanco.Id = view.Id;
            objBanco.Nome = view.Nome;
            objBanco.Prioridade_Padrao = view.Prioridade_Padrao;
            objBanco.Script = view.Id_Script.HasValue ? new BuscaGenericaRepository<Script_Atendimento>(session, null).FindById(view.Id_Script.Value).Result : null;
            objBanco.Criar_Os_Encerrar = view.Criar_Os_Encerrar;
            objBanco.Exibir_Ola = view.Exibir_Ola;
            objBanco.Categoria_Atendimento = view.Id_Categoria_Atendimento.HasValue ? new BuscaGenericaRepository<Categoria_Atendimento>(session, null).FindById(view.Id_Categoria_Atendimento.Value).Result : null;
            objBanco.Desc_Tipo_Servico = view.Desc_Tipo_Servico;
            objBanco.Script_SuporteTecnico = view.Id_Script_SuporteTecnico.HasValue ? new BuscaGenericaRepository<Script_SuporteTecnico>(session, null).FindById(view.Id_Script_SuporteTecnico.Value).Result : null;

            List<Tipo_Atendimento_Motivo_Status> meiosTmp = objBanco.Motivos.ToList();
            foreach (var motivo in view.Motivos.FindAll(p => p.Id == 0))
            {
                meiosTmp.Add(new Tipo_Atendimento_Motivo_Status
                {
                    Motivo = new BuscaGenericaRepository<Motivo_Status_Atendimento>(session, null).FindById(motivo.Id_Motivo.Value).Result,
                    Tipo_Atendimento = objBanco
                });
            }
            objBanco.Motivos = meiosTmp;
        }

        [HttpPost()]
        [Route("api/tipo_atendimentoapi/AlterarStatus")]
        public async Task<PostApiRetorno<Int64>> AlterarStatus(AlterarStatusViewModel value)
        {
            PostApiRetorno<Int64> retorno = new PostApiRetorno<Int64>();
            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();
                    using (var com = session.Connection.CreateCommand())
                    {
                        String _ids = $"({String.Join(",", value.ids)})";

                        StringBuilder sql = new StringBuilder($"UPDATE TB_TIPO_ATENDIMENTO SET STATUS = {value.status} WHERE ID IN {_ids}");

                        await CommandHelper.ExecuteNonQuery(com, sql, new Dictionary<String, Object>(), session.Connection, null);

                        retorno.Ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Mensagem = ex.Message;
                retorno.InnerException = ex.InnerException != null ? ex.InnerException.Message : String.Empty;
            }

            return retorno;
        }

        [HttpGet()]
        [Route("api/tipo_atendimentoapi/busca")]
        public async Task<List<ObjBusca<Int64>>> busca(String term)
        {
            List<ObjBusca<Int64>> retorno = new List<ObjBusca<Int64>>();

            using (ISession session = HibernateUtil.GetSession())
            {
                try
                {
                    //Filtros 
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();

                    //Select
                    using (var com = session.Connection.CreateCommand())
                    {
                        StringBuilder sql = new StringBuilder();

                        sql.AppendLine("SELECT A.ID, A.NOME");
                        sql.AppendLine("FROM TB_TIPO_ATENDIMENTO A");
                        sql.AppendLine("WHERE (UPPER(A.NOME) LIKE @VALOR) ");
                        parametros.Add("@VALOR", $"%{term.ToUpperInvariant()}%");

                        using (var reader = await CommandHelper.ExecuteReaderFirst(com, sql, parametros, session.Connection, null, 10, null))
                        {
                            while (await reader.ReadAsync())
                            {
                                retorno.Add(new ObjBusca<Int64>
                                {
                                    Value = Convert.ToInt64(reader["ID"]),
                                    Label = Convert.ToString(reader["NOME"])
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return retorno;
        }

        [HttpPost()]
        [Route("api/tipo_atendimentoapi/excluirlista")]
        public async Task<PostApiRetorno<Int64>> ExcluirLista(ExcluirListaViewModel value)
        {
            PostApiRetorno<Int64> retorno = new PostApiRetorno<Int64>();
            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    Dictionary<String, Object> parametros = new Dictionary<string, object>();
                    using (var com = session.Connection.CreateCommand())
                    {
                        String _ids = $"({String.Join(",", value.ids)})";

                        StringBuilder sql = new StringBuilder($"DELETE FROM TB_TIPO_ATENDIMENTO WHERE ID IN {_ids}");

                        await CommandHelper.ExecuteNonQuery(com, sql, new Dictionary<String, Object>(), session.Connection, null);

                        retorno.Ok = true;
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.Mensagem = ex.Message;
                retorno.InnerException = ex.InnerException != null ? ex.InnerException.Message : String.Empty;
            }

            return retorno;
        }


        [HttpGet()]
        [Route("api/Tipo_AtendimentoApi/FindById")]
        public async Task<GetApiRetorno<Tipo_AtendimentoCadastroModel>> FindById(Int64 Id)
        {
            GetApiRetorno<Tipo_AtendimentoCadastroModel> retorno = new GetApiRetorno<Tipo_AtendimentoCadastroModel>();

            using (ISession session = HibernateUtil.GetSession())
            {
                try
                {
                    retorno.Objeto = GetViewObj(await new BuscaGenericaRepository<Tipo_Atendimento>(session, null).FindById(Id));
                    retorno.Ok = true;
                }
                catch (Exception ex)
                {
                    retorno.Mensagem = ex.Message;
                    if (ex.InnerException != null) retorno.InnerException += ex.InnerException.Message;
                }
            }
            return retorno;
        }


        [HttpPost()]
        [Route("api/Tipo_AtendimentoApi/Gravar")]
        public async Task<PostApiRetorno<Int64>> Gravar([FromBody]ParamWebApi<Tipo_AtendimentoCadastroModel> value)
        {
            PostApiRetorno<Int64> retorno = new PostApiRetorno<Int64>();

            try
            {
                Tipo_Atendimento modelo = null;

                using (ISession session = HibernateUtil.GetSession())
                {
                    using (var repository = new Conexao<Tipo_Atendimento>(session))
                    {
                        if (value.Objeto.Id != 0)
                            modelo = (await repository.select(p => p.Id == value.Objeto.Id)).FirstOrDefault();

                        if (modelo == null)
                        {
                            modelo = new Tipo_Atendimento
                            {
                                Status = Status_Default.Ativo,
                                UsuarioCriacao = value.Usuario
                            };
                        }
                        else
                        {
                            modelo.Status = value.Objeto.Status ? Status_Default.Ativo : Status_Default.Inativo;
                            modelo.UsuarioAlteracao = value.Usuario;
                        }

                        BindViewToObj(session, ref modelo, value.Objeto);

                        await repository.SaveOrUpdate(modelo);
                    }
                    retorno.IdObjeto = modelo.Id;
                    retorno.Ok = true;
                }
            }
            catch (Exception ex)
            {
                retorno.IdObjeto = 0;
                retorno.Mensagem = ex.Message;
                if (ex.InnerException != null) retorno.InnerException = ex.InnerException.Message;
            }

            return retorno;
        }

        [Route("api/Tipo_AtendimentoApi/Excluir")]
        [HttpPost()]
        public async Task<DeleteApiRetorno<Int64>> Excluir([FromBody]ParamWebApi<Int64> value)
        {
            DeleteApiRetorno<Int64> retorno = new DeleteApiRetorno<Int64>();

            try
            {
                using (ISession session = HibernateUtil.GetSession())
                {
                    using (var repository = new Conexao<Tipo_Atendimento>(session))
                    {

                        Tipo_Atendimento modelo = null;
                        if (value.Objeto != 0)
                        {
                            modelo = (await repository.select(p => p.Id == value.Objeto)).FirstOrDefault();

                            await repository.Remove(modelo);

                            retorno.IdObjeto = modelo.Id;
                            retorno.Ok = true;
                        }
                        else
                        {
                            throw new Exception(Resources.Resources.NAOENCONTRADO);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retorno.IdObjeto = 0;
                retorno.Mensagem = ex.Message;
                if (ex.InnerException != null) retorno.InnerException = ex.InnerException.Message;
            }

            return retorno;
        }
    }
}
