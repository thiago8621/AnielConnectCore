using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinapse.Core
{
    public enum TipoEmail
    {
        NInformado = 0,
        RenovarSenha = 1,
        AtivarConta = 2,
        ConfirmacaoPagamento = 3,
        PedidoSucesso = 4,
        Atendimento = 5,
        SenhaTemporaria = 6,
        AtivarContaRepresentante = 7,
        NaoSouCadastrado = 8,
        InnovationExecucaoAutoOk= 9,
        InnovationExecucaoAutoErro = 10,
        ManagerExecucaoAutoOk = 11,
        MagagerExecucaoAutoErro = 12
    }
}
