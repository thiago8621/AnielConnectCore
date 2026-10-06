using NHibernate;
using NHibernate.Classic;
using Sinapse.Model.AspNetCore.enumerados;
using System;


namespace Sinapse.Model.AspNetCore.nhibernate
{
    public class BaseLog : IModelLog, ILifecycle
    {
        public virtual Int64 Id { get; set; }

        public virtual String UsuarioCriacao { get; set; }

        public virtual DateTime? DataHoraCriacao { get; set; }
        public virtual TipoOperacao TipoOperacao { get; set; }

        public virtual String Token { get; set; }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public virtual LifecycleVeto OnDelete(NHibernate.ISession s)
        {
            return LifecycleVeto.NoVeto;
        }

        public virtual void OnLoad(NHibernate.ISession s, object id)
        {

        }

        public virtual LifecycleVeto OnSave(NHibernate.ISession s)
        {
            DataHoraCriacao = DateTime.Now;

            return LifecycleVeto.NoVeto;
        }

        public virtual LifecycleVeto OnUpdate(ISession s)
        {
            return LifecycleVeto.NoVeto;
        }
    }
}
