namespace Sinapse.Model.enumerados
{
    public enum Status_Elemento
    {
        //[Display(Name = "ATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Ativo = 1,

        //[Display(Name = "PREVISTO", ResourceType = typeof(Resources.RsCadastros))]
        Previsto = 2,

        //[Display(Name = "CONSTRUIDO", ResourceType = typeof(Resources.RsCadastros))]
        Construido = 3,

        //[Display(Name = "AGUARDANDO_CERTIFICACAO", ResourceType = typeof(Resources.RsCadastros))]
        AguardandoCertificacao = 4,

        //[Display(Name = "MANUTENCAO", ResourceType = typeof(Resources.RsCadastros))]
        Manutencao = 5,

        //[Display(Name = "INATIVO", ResourceType = typeof(Resources.RsCadastros))]
        Inativo = 0,
    }
}
