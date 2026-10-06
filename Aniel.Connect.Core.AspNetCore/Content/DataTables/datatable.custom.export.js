/**
 *  Modelo dos Dados do Empregado
 *  na tela da Folha de Ponto
 */
const dadosEmpregadoModelo = {
    table: {
        widths: ['*', '*', '*'],
        body: [
            [
                { text: 'Matricula: (MATRICULA)', alignment: 'left', style: 'tableCell' },
                { text: 'Colaborador(a): (COLABORADOR)', colSpan: 2, alignment: 'left', style: 'tableCell' },
                {}
            ],
            [
                { text: 'Matricula Auxiliar: (MATRICULA_AUXILIAR)', alignment: 'left', style: 'tableCell' },
                { text: 'CPF: (CPF)', alignment: 'left', style: 'tableCell' },
                { text: 'Tipo de Jornada: (TIPO_JORNADA)', alignment: 'left', style: 'tableCell' },
            ],
            [
                { text: 'PIS: (PIS)', alignment: 'left', style: 'tableCell' },
                { text: 'CTPS/Série: (CTPS_SERIE)', alignment: 'left', style: 'tableCell' },
                { text: 'Data de Admissão: (DATA_ADMISSAO)', alignment: 'left', style: 'tableCell' },
            ],
            [
                { text: 'Função: (FUNCAO)', alignment: 'left', style: 'tableCell' },
                { text: 'Departamento: (DEPARTAMENTO)', alignment: 'left', style: 'tableCell' },
                { text: 'Centro de Custo: (CENTRO_CUSTO)', alignment: 'left', style: 'tableCell' },
            ]
        ],
    },
    layout: 'noBorders',
    border: [false, false, true, false],
    margin: [0, 10]
}

/**
 *  Modelo dos Dados da Empresa
 *  na tela da Folha de Ponto
 */
const dadosEmpregadorModelo = {
    table: {
        //widths: ['*', '*', '*'],
        body: [
            [
                { text: 'Empregador: (EMPREGADOR)', alignment: 'left', style: 'tableCell' }
            ],
            [
                { text: 'CNPJ: (CNPJ)', alignment: 'left', style: 'tableCell' }
            ],
            [
                { text: 'Endereço: (ENDERECO)', alignment: 'left', style: 'tableCell' }
            ]
        ],
    },
    layout: 'noBorders',
    border: [true, false, false, false],
    margin: [0, 10]
}

/**
 * Modelo dos Totalizadores na
 * tela de Folha de Ponto
 */
const dadosTotalizadosModelo = {
    table: {
        widths: ['*', '*'],
        body: [
            [
                { text: 'Total de Horas Trabalhadas: (HORAS_TRABALHADAS)', style: 'tableCellLeft' },
                { text: 'Total de Horas em Débito: (HORAS_DEBITO)', style: 'tableCellLeft' },
            ],
            [
                { text: 'Total de Horas Extras: ', style: 'tableCellLeft' },
                { text: 'Total de Horas em Débito após Compensação: (HORAS_DEBITO_COMPENSADO)', style: 'tableCellLeft' },
            ],
            [
                {
                    table: {
                        widths: ['35%', '10%'],
                        body: "(HORAS_EXTRAS)"
                    },
                    layout: 'noBorders'
                },
                { text: 'Total Adicional Noturno: (ADIC_NOTURNO)', style: 'tableCellLeft' },
            ],
            [
                {
                    "text": "Total de Horas Extras Compensadas: ",
                    "style": "tableCellLeft"
                },
                { text: 'Total de Horas em Sobreaviso: (SOBREAVISO)', style: 'tableCellLeft' }
            ],
            [
                {
                    table: {
                        widths: ['35%', '10%'],
                        body: "(HORAS_EXTRAS_COMPENSADAS)"
                    },
                    layout: 'noBorders'
                },
                {}
            ]
        ]
    },
    layout: 'noBorders'
}

/**
 * Modelo dos Totalizadores do
 * Banco de Horas na tela de Folha de Ponto
 */
const dadosBancoHorasModelo = {
    table: {
        widths: ['*'],
        body: [
            [{ text: "Banco de Horas", style: "tableHeader" }],
            [{ text: "Saldo Atual do Banco de Horas: (SALDO_BANCO)", style: 'tableCellLeft' }],
            [{ text: "Quantidade de Horas Extras do Banco de Horas que Foram Pagas: (HORAS_PAGAS)", style: 'tableCellLeft' }],
            [{ text: "Quantidade de Horas em Débito no Banco de Horas que Foram Descontadas: (HORAS_DEBITO)", style: 'tableCellLeft' }],
        ]
    },
    layout: 'noBorders',
    margin: [0, 0, 0, 0]
}

/**
 * Modelo do titulo com a logo
 * da empresa ao lado
 */
const modeloTituloLogoEmpresa = {
    columns: [{
        image: "(IMAGEM_BASE64)",
        width: 150,
        alignment: "center"
    },
    {
        text: "(TITULO_RELATORIO)",
        fontSize: 20,
        margin: [-150, 45, 0, 0]
    }]
}

const modeloTituloLogoEmpresaPeriodo = {
    columns: [{
        image: "(IMAGEM_BASE64)",
        width: 190
    },
    {
        table: {
            body: [
                [{
                    text: "(TITULO_RELATORIO)",
                    fontSize: 20,
                    margin: [150, 24, 0, 0]
                }],
                [{
                    text: "Período: (DATA_INICIO) a (DATA_FIM)",
                    fontSize: 10,
                    margin: [120, 0, 0, 0]
                }]
            ]
        },
        layout: 'noBorders'
    }]
}

const larguraColunasFolhaPonto = ['8.5%', '5.25%', '15%', '22.5%', '6.25%', '7.5%', '6.25%', '7.5%', '6.25%', '7.5%', '7.5%']