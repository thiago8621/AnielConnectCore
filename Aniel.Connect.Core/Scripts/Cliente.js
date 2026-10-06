// Datepicker
$('.date').datepicker({
    format: 'dd/mm/yyyy',
    language: 'pt-BR',
    clearDate: true
});

//Novo Endereço
$("#iNovoEndereco").click(function () {
    $("#iDivNewAdress").removeClass("d-none");
});

$("#iMesmoEndereco").click(function () {
    $("#iDivNewAdress").addClass("d-none");
});

