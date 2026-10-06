using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sinapse.DAO;
using Sinapse.Model.nhibernate;
using Sinapse.Util;

namespace Aniel.Connect.Core
{
    public class TesteBase<TCadastroModel> : IDisposable
    {

        protected Api CriaApi<Api>() where Api : BaseApiController, new()
        {
            return new Api();
        }

        protected async Task<DataTableResponse<TypeResponseGetAll>> ExecGetAll<Api, RequestGetAll, TypeResponseGetAll>(Api api)
            where Api : BaseApiController, new()
            where RequestGetAll : DataTableRequest, new()
        {
            var mInfo = api.GetType().GetMethod("GetAll");
            
            var request = new RequestGetAll
            {
                draw = 10,
                order = new DataTableOrder [] { new DataTableOrder { column = 1, dir = "asc" } }
            };

            var task = (Task<DataTableResponse<TypeResponseGetAll>>)mInfo.Invoke(api, new[] { request });

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return (DataTableResponse<TypeResponseGetAll>)resultProperty.GetValue(task);
        }

        protected async Task<PostApiRetorno<TypeResponseGravar>> ExecGravar<Api, TypeRequestGravar, TypeResponseGravar>(Api api, ParamWebApi<TypeRequestGravar> request)
            where Api : BaseApiController, new()
            where TypeRequestGravar : IViewModel<TypeResponseGravar>, new()
        {
            var mInfo = api.GetType().GetMethod("Gravar");

            var task = (Task<PostApiRetorno<TypeResponseGravar>>)mInfo.Invoke(api, new[] { request });

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return (PostApiRetorno<TypeResponseGravar>)resultProperty.GetValue(task);
        }

        protected async Task<GetApiRetorno<TypeResponseFindById>> ExecFindById<Api, TypeRequestFindById, TypeResponseFindById>(Api api, TypeRequestFindById request)
            where Api : BaseApiController, new()
            where TypeResponseFindById : IViewModel<Int64>, new()
        {
            var mInfo = api.GetType().GetMethod("FindById");

            var task = (Task<GetApiRetorno<TypeResponseFindById>>)mInfo.Invoke(api, new Object[] { request });

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return (GetApiRetorno<TypeResponseFindById>)resultProperty.GetValue(task);
        }

        protected async Task<DeleteApiRetorno<TypeResponseExcluir>> ExecExcluir<Api, TypeRequestExcluir, TypeResponseExcluir>(Api api, ParamWebApi<TypeRequestExcluir> request)
            where Api : BaseApiController, new()
        {
            var mInfo = api.GetType().GetMethod("Excluir");

            var task = (Task<DeleteApiRetorno<TypeResponseExcluir>>)mInfo.Invoke(api, new Object[] { request });

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return (DeleteApiRetorno<TypeResponseExcluir>)resultProperty.GetValue(task);
        }

        protected async Task GetAll<Api, RequestGetAll, TypeResponseGetAll>()
            where Api : BaseApiController, new()
            where RequestGetAll : DataTableRequest, new()
        {
            using (var api = CriaApi<Api>())
            {
                try
                {
                    var retornoGetAll = await ExecGetAll<Api, RequestGetAll, TypeResponseGetAll>(api);
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, $"GetAll com erro {ex.Message} {ex.InnerException?.Message}");
                }
            }
        }

        protected async Task<PostApiRetorno<TypeResponseGravar>> Gravar<Api, TypeRequestGravar, TypeResponseGravar>(ParamWebApi<TypeRequestGravar> request)
            where Api : BaseApiController, new()
            where TypeRequestGravar : IViewModel<TypeResponseGravar>, new()
        {
            using (var api = CriaApi<Api>())
            {
                try
                {
                    var retornoGravar = await ExecGravar<Api, TypeRequestGravar, TypeResponseGravar>(api, request);

                    Assert.IsTrue(retornoGravar.Ok, $"Gravar com Problemas {retornoGravar.Mensagem}");

                    return retornoGravar;
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, $"Gravar com erro {ex.Message} {ex.InnerException?.Message}");
                    return default;
                }
            }
        }
        //GetApiRetorno<BancoCadastroModel>

        protected async Task<TypeResponseFindById> FindById<Api, TypeRequestFindById, TypeResponseFindById>(TypeRequestFindById request)
        where Api : BaseApiController, new()
        where TypeResponseFindById : IViewModel<Int64>, new()
        {
            using (var api = CriaApi<Api>())
            {
                try
                {
                    var retornoFindById = await ExecFindById<Api, TypeRequestFindById, TypeResponseFindById>(api, request);

                    Assert.IsTrue(retornoFindById.Ok, $"FindById com Problemas Id {request} - {retornoFindById.Mensagem}");

                    return retornoFindById.Objeto;
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, $"FindById com erro {ex.Message} {ex.InnerException?.Message}");
                    return default;
                }
            }
        }

        public async Task<TypeResponseFindById> Excluir<Api, TypeRequestFindById, TypeResponseFindById>(ParamWebApi<TypeRequestFindById> request)
            where Api : BaseApiController, new()
        {
            using (var api = CriaApi<Api>())
            {
                try
                {
                    var retornoFindById = await ExecExcluir<Api, TypeRequestFindById, TypeResponseFindById>(api, request);

                    Assert.IsTrue(retornoFindById.Ok, $"Excluir com Problemas Id {request} - {retornoFindById.Mensagem}");

                    return retornoFindById.IdObjeto;
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, $"Excluir com erro {ex.Message} {ex.InnerException?.Message}");
                    return default;
                }
            }
        }

        /// <summary>
        /// Testa o crud caso a api eteja seguindo o padrão (FindById, )
        /// </summary>
        /// <typeparam name="Api"></typeparam>
        /// <typeparam name="TypeRequestGravar"></typeparam>
        /// <param name="ObjInsert"></param>
        /// <param name="CompareAB"></param>
        /// <param name="AlterValues"></param>
        /// <returns></returns>
        public async Task Crud_Basico<Api, TypeRequestGravar>(TypeRequestGravar ObjInsert, Expression<Func<TypeRequestGravar, TypeRequestGravar, bool>> CompareAB,
            Expression<Func<TypeRequestGravar, TypeRequestGravar>> AlterValues)
            where Api : BaseApiController, new()
            where TypeRequestGravar : IViewModel<Int64>, new()
        {

            Int64 Id_ObjTeste = (await Gravar<Api, TypeRequestGravar, long>(new ParamWebApi<TypeRequestGravar>
            {
                Objeto = ObjInsert,
                Usuario = CONSTANTES.USUARIO_PADRAO
            })).IdObjeto;

            try
            {
                TypeRequestGravar ObjFindById = await FindById<Api, Int64, TypeRequestGravar>(Id_ObjTeste);
                

                if (CompareAB != null)
                if (CompareAB.Compile()(ObjInsert, ObjFindById))
                {
                    Assert.IsTrue(false, $"Valores gravados com problema. Json FindById: {JsonService.SerializeObject(ObjFindById)}");
                }

                //Alteração
                if (AlterValues != null)
                    ObjFindById = AlterValues.Compile()(ObjFindById);

                await Gravar<Api, TypeRequestGravar, Int64>(new ParamWebApi<TypeRequestGravar>
                {
                    Objeto = ObjFindById,
                    Usuario = CONSTANTES.USUARIO_PADRAO
                });

            }
            finally
            {
                await Excluir<Api, Int64, Int64>(new ParamWebApi<Int64>
                {
                    Objeto = Id_ObjTeste,
                    Usuario = CONSTANTES.USUARIO_PADRAO
                });
            }
        }

        #region IDisposable Support
        private Boolean disposedValue;// To detect redundant calls

        // IDisposable
        protected virtual void Dispose(Boolean disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }
            this.disposedValue = true;
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
