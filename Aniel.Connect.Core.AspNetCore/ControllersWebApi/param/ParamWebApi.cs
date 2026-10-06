using Sinapse.Model.AspNetCore.core;
using System;
using System.Collections.Generic;

namespace Aniel.Connect.Core.AspNetCore
{
    public class ParamWebApi<T> : IParamWebApi
    {
        public virtual String Usuario { get; set; }
        public virtual String NomeUsuario { get; set; }

        public virtual String Token { get; set; }

        public virtual Boolean Info { get; set; }

        public virtual T Objeto { get; set; }
        
    
    }

    //[ModelBinder(typeof(DataTableModelBinder))]
    public class DataTableRequest
    {
        public DataTableRequest()
        {
            order = new DataTableOrder[] { };
            columns = new DataTableColumn[] { };
            search = new DataTableSearch();
        }

        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public int recordsTotal { get; set; }

        public DataTableOrder[] order { get; set; }
        public DataTableColumn[] columns { get; set; }
        public DataTableSearch search { get; set; }
    }


    public class AlterarStatusViewModel
    {
        public List<Int64> ids { get; set; }
        public Int16 status { get; set; }
        public String usuario { get; set; }
        public Int64? id_motivo_cancelamento { get; set; }
    }

    
    public class AlterarStatusAtendimentoViewModel
    {
        public List<Int64> ids { get; set; }
        public String usuario { get; set; }
        public String NomeUsuario { get; set; }
        public Int16 status { get; set; }
        public Int16 motivo_status { get; set; }
    }

    public class ExcluirListaViewModel
    {
        public List<Int64> ids { get; set; }
    }

    public class ObjBusca<T>
    {
        public virtual T Value { get; set; }
        public virtual String Label { get; set; }
    }

    public class ObjCombo<T>
    {
        public virtual List<IObjComboResult<T>> results { get; set; }

        public virtual Int32 total_count { get; set; }

        public static implicit operator ObjCombo<T>(ObjCombo<long?> v)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjComboOptGroup<T, Y>
    {
        public virtual List<IObjComboOptGroupResult<T, Y>> results { get; set; }

        public virtual Int32 total_count { get; set; }
    }

    public interface IObjComboResult<T>
    {
        T Id { get; set; }
        String Text { get; set; }
    }

    public interface IObjComboOptGroupResult<T, Y>
    {
        Y Id_Grupo { get; set; }
        String Text { get; set; }
        List<IObjComboResult<T>> Children { get; set; }
    }

    public class ObjComboResult<T> : IObjComboResult<T>
    {
        public virtual T Id { get; set; }
        public virtual String Text { get; set; }
    }

    /// <summary>
    /// Item de retorno para o select2 com agrupamento
    /// </summary>
    /// <typeparam name="T">Tipo do Id dos items</typeparam>
    /// <typeparam name="Y">Tipo do Id dos grupos</typeparam>

    public class ObjComboOptGroupResult<T, Y> : IObjComboOptGroupResult<T, Y>
    {
        public virtual Y Id_Grupo { get; set; }
        public virtual String Text { get; set; }
        public virtual List<IObjComboResult<T>> Children { get; set; }
    }


    public class RequestObjComboAvoConfig<T, Y, Z, C> : RequestObjComboAvo<T, Y, Z>
    {
        public virtual C config { get; set; }
    }

    public class RequestObjComboPaiConfig<T, Y, C> : RequestObjComboPai<T, Y>
    {
        public virtual C config { get; set; }
    }

    public class RequestObjComboConfig<T, C> : RequestObjCombo<T>
    {
        public virtual C config { get; set; }
    }


    /// <summary>
    /// Request para Select2
    /// </summary>
    /// <typeparam name="T">Tipo do Id</typeparam>
    /// <typeparam name="Y">Tipo do IdPai</typeparam>
    /// <typeparam name="Z">Tipo do IdAvo</typeparam>
    public class RequestObjComboAvo<T, Y, Z> : RequestObjCombo
    {
        public virtual Z idavo { get; set; }
        public virtual Y idpai { get; set; }
        public virtual T id { get; set; }        
    }

    /// <summary>
    /// Request para Select2
    /// </summary>
    /// <typeparam name="T">Tipo do Id</typeparam>
    /// <typeparam name="Y">Tipo do IdPai</typeparam>
    public class RequestObjComboPai<T, Y> : RequestObjCombo
    {
        public virtual Y idpai { get; set; }
        public virtual T id { get; set; }        
    }

    /// <summary>
    /// Request para Select2
    /// </summary>
    /// <typeparam name="T">Tipo do Id</typeparam>
    public class RequestObjCombo<T> : RequestObjCombo
    {
        public virtual T id { get; set; }

    }

    public class RequestObjCombo
    {        
        public virtual String q { get; set; }
        public virtual Int32 page { get; set; }
        public virtual Boolean soativos { get; set; }
         
        public virtual String id_usuario { get; set; }
        public virtual String cod_area { get; set; }
    }

    public class DataTableOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class DataTableSearch
    {
        public string value { get; set; }
        public string option { get; set; }
        public bool regex { get; set; }
    }

    public class DataTableColumn
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }

        public DataTableSearch search { get; set; }
    }
}