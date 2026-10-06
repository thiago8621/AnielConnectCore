/*
 verifica_cpf_cnpj

 Verifica se é CPF ou CNPJ.
 CNPJ: 14 caracteres, últimos 2 obrigatoriamente numéricos (novo padrão alfanumérico).
 CPF : 11 caracteres, somente numéricos.
*/
function verifica_cpf_cnpj(valor) {
    valor = valor.toString().toUpperCase().replace(/[^A-Z0-9]/g, '');

    if (valor.length === 11 && /^[0-9]+$/.test(valor)) {
        return 'CPF';
    }

    if (valor.length === 14 && /^[0-9]+$/.test(valor.slice(12))) {
        return 'CNPJ';
    }

    return false;
}

/*
 calc_digitos_posicoes (CPF)
 Mantido exatamente igual ao original.
*/
function calc_digitos_posicoes(digitos, posicoes = 10, soma_digitos = 0) {
    digitos = digitos.toString();

    for (var i = 0; i < digitos.length; i++) {
        soma_digitos = soma_digitos + (digitos[i] * posicoes);
        posicoes--;
        if (posicoes < 2) {
            posicoes = 9;
        }
    }

    soma_digitos = soma_digitos % 11;
    soma_digitos = soma_digitos < 2 ? 0 : 11 - soma_digitos;

    return digitos + soma_digitos;
}

/*
 valida_cpf
 Mantido exatamente igual ao original.
*/
function valida_cpf(valor) {
    valor = valor.toString().replace(/[^0-9]/g, '');

    var digitos = valor.substr(0, 9);
    var novo_cpf = calc_digitos_posicoes(digitos);
    novo_cpf = calc_digitos_posicoes(novo_cpf, 11);

    return novo_cpf === valor;
}

/*
 valida_cnpj

 Atualizado para o novo padrão alfanumérico (Nota Técnica 2025.001 - Receita Federal).
 Posições  1-12: alfanuméricas (A-Z e 0-9)
 Posições 13-14: exclusivamente numéricas (dígitos verificadores)
 Regra do dígito: ASCII do caractere - 48 (funciona para letras e números)
*/
function valida_cnpj(valor) {
    valor = valor.toString().toUpperCase().replace(/[^A-Z0-9]/g, '');

    if (valor.length !== 14) return false;
    if (/^(.)\1+$/.test(valor)) return false;

    function calcDV(digitos, pesoInicial) {
        var peso = pesoInicial, soma = 0;
        for (var i = 0; i < digitos.length; i++) {
            soma += (digitos.charCodeAt(i) - 48) * peso--;
            if (peso < 2) peso = 9;
        }
        var resto = soma % 11;
        return (resto < 2 ? 0 : 11 - resto).toString();
    }

    var base12 = valor.substring(0, 12);
    var dv1 = calcDV(base12, 5);
    var dv2 = calcDV(base12 + dv1, 6);

    return valor.substring(12) === (dv1 + dv2);
}

/*
 valida_cpf_cnpj
 Mantido exatamente igual ao original — só chama as funções atualizadas.
*/
function valida_cpf_cnpj(valor) {
    var valida = verifica_cpf_cnpj(valor);

    valor = valor.toString().toUpperCase().replace(/[^A-Z0-9]/g, '');

    if (valida === 'CPF') return valida_cpf(valor);
    if (valida === 'CNPJ') return valida_cnpj(valor);

    return false;
}

/*
 formata_cpf_cnpj
 Atualizado para aceitar letras na máscara do CNPJ: AA.AAA.AAA/AAAA-00
*/
function formata_cpf_cnpj(valor) {
    var formatado = false;
    var valida = verifica_cpf_cnpj(valor);

    valor = valor.toString().toUpperCase().replace(/[^A-Z0-9]/g, '');

    if (valida === 'CPF' && valida_cpf(valor)) {
        formatado = valor.substr(0, 3) + '.';
        formatado += valor.substr(3, 3) + '.';
        formatado += valor.substr(6, 3) + '-';
        formatado += valor.substr(9, 2);
    }

    if (valida === 'CNPJ' && valida_cnpj(valor)) {
        formatado = valor.substr(0, 2) + '.';
        formatado += valor.substr(2, 3) + '.';
        formatado += valor.substr(5, 3) + '/';
        formatado += valor.substr(8, 4) + '-';
        formatado += valor.substr(12, 2);
    }

    return formatado;
}