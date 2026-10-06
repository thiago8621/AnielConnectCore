using System.ComponentModel.DataAnnotations;

namespace Sinapse.ModelLog.AspNetCore
{
    public enum TipoOperLog
    {
        Insercao = 1,
        Alteracao = 2,
        Exclusao = 3,
        Baixa = 4,
        Importacao = 5
    }
}
