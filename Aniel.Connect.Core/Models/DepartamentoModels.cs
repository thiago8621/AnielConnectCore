using Aniel.PontoEletronico.Core;
using Aniel.PontoEletronico.Core.Service;
//using Aniel.PontoEletronico.Model.enumerado;
//using Aniel.PontoEletronico.Model.nhibernate;
using Sinapse.Model.nhibernate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Aniel.PontoEletronico.Models
{
    public class DepartamentoListaModel
    {
        public virtual Int64 Id { get; set; }

        public virtual String Codigo { get; set; }

        public virtual String Descricao { get; set; }

        public virtual String Status { get; set; }

    }


    public class DepartamentoCadastroModel : IViewModel<Int64>
    {
        public virtual Int64 Id { get; set; }

        public virtual String Codigo { get; set; }

        public virtual String Descricao { get; set; }

        [Display(Name = "STATUS", ResourceType = typeof(Resources.RsPontoEletronico))]
        public virtual Boolean Status { get; set; }

        public class Departamento_DataTableRequest : DataTableRequest
        {

        }

    }
}