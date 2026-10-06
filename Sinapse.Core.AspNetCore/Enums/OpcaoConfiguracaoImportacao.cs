namespace Sinapse.Core.AspNetCore
{
    public enum OpcaoConfiguracaoImportacao
    {
        OSExisteIgnorar = 1,
        AtualizarTodosOsCampos = 2,
        AtualizarApenasCamposNaoPreenchidos = 3,
        AtualizarOsSeguintesCampos = 4,
        ObjAssociadoCadastraAutomatico = 5,
        ObjAssociadoIgnorarCampo = 6,
        ObjAssociadoIgnorarOS = 7,
    }
}
