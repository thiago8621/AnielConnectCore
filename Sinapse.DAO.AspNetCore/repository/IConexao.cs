using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sinapse.DAO.AspNetCore.repository
{
    public interface IConexao<T> where T : class
    {
        Task<long> Max(Expression<Func<T, object>> expression, Expression<Func<T, bool>> where = null);
        Task<List<T>> query(string tableName, string[] campos, string where);
        Task Remove(T obj);
        Task Remove(IEnumerable<T> obj);
        Task RemoveAll();
        Task Save(T obj);
        Task Save(List<T> obj);
        Task SaveOrUpdate(T obj);
        Task SaveOrUpdate(IEnumerable<T> listGravacao);

        /// <summary>
        ///     Cumpre a mesma função do método <see cref="SaveOrUpdate(T)" /> para novos objetos criados a partir de outros objetos do NHibernate. <br /> <br />
        ///     
        ///     O <see cref="Merge(T)"/> procura na session um objeto com o mesmo identificador do objeto passado. <br /> <br />
        /// 
        ///     Caso o objeto esteja presente na sessão, o hibernate passa as alterações do objeto vindo pelo parâmetro
        ///     para o objeto persistente da session. <br /> <br /> 
        ///     
        ///     Caso não haja um objeto persistente carregado na session, o NHibernate procura se existe algum objeto no banco com o mesmo 
        ///     identificador. Se houver algum registro em banco o NHibernate o carrega na session e o atualiza, se não houver o NHibernate
        ///     cria um novo registro com as informações do objeto informado e o insere no banco.
        /// </summary>
        /// <param name="obj">O objeto a ser atualizado ou gravado</param>
        Task Merge(T obj);

        /// <summary>
        ///     Cumpre a mesma função do método <see cref="SaveOrUpdate(IEnumerable{T})" /> para novos objetos criados a partir de outros objetos do NHibernate. <br /> <br />
        ///     
        ///     O <see cref="Merge(IEnumerable{T})"/> procura na session um objeto com o mesmo identificador do objeto passado. <br /> <br />
        /// 
        ///     Caso o objeto esteja presente na sessão, o hibernate passa as alterações do objeto vindo pelo parâmetro
        ///     para o objeto persistente da session. <br /> <br /> 
        ///     
        ///     Caso não haja um objeto persistente carregado na session, o NHibernate procura se existe algum objeto no banco com o mesmo 
        ///     identificador. Se houver algum registro em banco o NHibernate o carrega na session e o atualiza, se não houver o NHibernate
        ///     cria um novo registro com as informações do objeto informado e o insere no banco.
        /// </summary>
        /// <param name="listGravacao">Lista de objeto a serem atualizados ou gravados</param>
        Task Merge(IEnumerable<T> listGravacao);
        Task<IList<T>> select();
        Task<IList<T>> select(Expression<Func<T, bool>> expression);
        Task<IList<T>> selectSql(string consulta);

        IQueryable<T> QueryableSelect();

        Task Update(T obj);
    }
}