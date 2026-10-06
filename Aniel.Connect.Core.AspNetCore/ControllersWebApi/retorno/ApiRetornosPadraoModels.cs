using System;
using System.Collections.Generic;

namespace Aniel.Connect.Core.AspNetCore
{

    public class GetApiRetorno
    {
        public Boolean Ok { get; set; }

        public String Mensagem { get; set; }

        public String InnerException { get; set; }

    }

    public class GetApiRetorno<T> : GetApiRetorno
    {
        public T Objeto { get; set; }

    }

    public class GetApiRetornoAjuste<T> : GetApiRetorno<T>
    {
        public Boolean PeriodoFechado { get; set; }

    }

    public class GetApiRetornoEscalaFeriado<T> : GetApiRetorno<T>
    {
        public T ListaFuncionariosNaoEscalado { get; set; }
    }

    public class GetApiRetornoCount<T>
    {
        public Boolean Ok { get; set; }
        public int RecordsTotal { get; set; }

        public String Mensagem { get; set; }

        public String InnerException { get; set; }

        public T Objeto { get; set; }

    }

    public class DataTableResponse<T>
    {
        public int draw { get; set; }
        //public long? recordsTotal { get; set; }
        public int? recordsFiltered { get; set; }
        public IEnumerable<T> data { get; set; }
        public string error { get; set; }
    }

    public class DataTableResponseMensagens<T>
    {
        public int draw { get; set; }
        //public long? recordsTotal { get; set; }
        public int? recordsFiltered { get; set; }
        public IEnumerable<T> data { get; set; }
        public string error { get; set; }

        private List<PostApiMensagem> _mensagens;
        public List<PostApiMensagem> mensagens
        {
            get
            {
                if (_mensagens == null)
                    _mensagens = new List<PostApiMensagem>();
                return _mensagens;
            }
            set
            {
                _mensagens = value;
            }
        }
    }


    public class DataTableResponseDynamicColumns<T>
    {
        public int draw { get; set; }
        public String columns { get; set; }

        public int? recordsFiltered { get; set; }
        public List<T> data { get; set; }
        public string error { get; set; }
    }

    public class DataTableResponseMultipleDynamicColumns<TData, TKey> : DataTableResponse<TData>
    {
        public List<Column> dynamicColumns { get; set; }
        public List<DataTableDynamicDataByKey<TKey>> listDynamicData { get; set; }
    }

    public class DataTableDynamicData
    {
        public string column { get; set; }
        public object value { get; set; }
    }

    public class DataTableDynamicDataByKey<T>
    {
        public T key { get; set; }
        public List<DataTableDynamicData> dynamicData { get; set; }
    }

    public class Column
    {
        public Column()
        {
            visible = true;
            colspan_complex = 2;
        }
        public string data { get; set; }
        public string title { get; set; }
        public Boolean visible { get; set; }
        public Boolean orderable { get; set; }
        public string className { get; set; }
        public string data_value { get; set; }
        public string complex_header { get; set; }
        public Int32 colspan_complex { get; set; }
    }




    public class PostApiRetorno
    {
        private List<PostApiMensagem> _mensagens;

        public Boolean Ok { get; set; }

        public String Mensagem { get; set; }

        public List<PostApiMensagem> Mensagens
        {
            get
            {
                if (_mensagens == null)
                    _mensagens = new List<PostApiMensagem>();
                return _mensagens;
            }
            set
            {
                _mensagens = value;
            }
        }

        public String InnerException { get; set; }
    }



    public class PostApiRetorno<K> : PostApiRetorno
    {
        public K IdObjeto { get; set; }
    }

    public enum PostApiMensagemType
    {
        Erro = 0,
        Info = 1
    }

    public class PostApiMensagem
    {
        public String Mensagem { get; set; }
        public PostApiMensagemType Tipo { get; set; }
    }



    public class DeleteApiRetorno<K>
    {
        public Boolean Ok { get; set; }

        public String Mensagem { get; set; }

        public String InnerException { get; set; }

        public K IdObjeto { get; set; }

    }

    public class Response
    {
        public bool result { get; set; }

        public string message { get; set; }

        public string exception { get; set; }

        public string stacktrace { get; set; }
    }

    public class Response<T> : Response
    {
        public T valor { get; set; }
    }

    public class MidiaMessenger
    {
        public string attachment_id { get; set; }
    }

    public class PostApiTwilio
    {
        public string sid { get; set; }
        public string service_sid { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_upload_updated { get; set; }
        public DateTime date_updated { get; set; }
        public int size { get; set; }
        public string content_type { get; set; }
        public object filename { get; set; }
        public string author { get; set; }
        public string category { get; set; }
        public object message_sid { get; set; }
        public object channel_sid { get; set; }
        public string url { get; set; }
        public bool is_multipart_upstream { get; set; }

    }

    public class PostGetFaturasAbertas
    {
        public string id { get; set; }
        public string id_Conta_Receber { get; set; }
        public string valor_Previsto { get; set; }
        public string status { get; set; }
        public string data_Vencimento { get; set; }
        public string protocolo { get; set; }

    }
}