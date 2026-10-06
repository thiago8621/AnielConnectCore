using Aniel.Connect.Integracoes.SerasaExperian.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Aniel.Connect.Integracoes.SerasaExperian.Interfaces
{
    public interface IAuthService
    {
        Task<AuthToken> ObterTokenAuth();
    }
}