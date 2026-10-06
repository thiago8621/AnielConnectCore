var imgDataTable = "<img src='" + baseUrl + "/Images/loading/loading-spinner-grey.gif'> Processando</img>";

var traducaoDataTable = {
    "sEmptyTable": "Nenhum registro encontrado",
    "sInfo": "Mostrando de _START_ até _END_ de _TOTAL_ registros",
    "sInfoEmpty": "Mostrando 0 até 0 de 0 registros",
    "sInfoFiltered": "(Filtrados de _MAX_ registros)",
    "sInfoPostFix": "",
    "sInfoThousands": ".",
    "sLengthMenu": "Exibir _MENU_ resultados por página",
    "sLoadingRecords": "Carregando...",
    "sProcessing": imgDataTable,
    "sZeroRecords": "Nenhum registro encontrado",
    "sSearch": "Pesquisar",
    "oPaginate": {
        "sNext": "Próximo",
        "sPrevious": "Anterior",
        "sFirst": "Primeiro",
        "sLast": "Último"
    },
    "oAria": {
        "sSortAscending": ": Ordenar colunas de forma ascendente",
        "sSortDescending": ": Ordenar colunas de forma descendente"
    }
};

var traducaoDataTableSimples = {
    "sEmptyTable": "Nenhum registro encontrado",
    "sInfo": "Mostrando _TOTAL_ registros",
    "sInfoEmpty": "Mostrando 0 registros",
    "sInfoFiltered": "(Filtrados de _MAX_ registros)",
    "sInfoPostFix": "",
    "sInfoThousands": ".",
    "sLengthMenu": "Exibir _MENU_ resultados por página",
    "sLoadingRecords": "Carregando...",
    "sProcessing": imgDataTable,
    "sZeroRecords": "Nenhum registro encontrado",
    "sSearch": "Pesquisar",
    "oPaginate": {
        "sNext": "Próximo",
        "sPrevious": "Anterior",
        "sFirst": "Primeiro",
        "sLast": "Último"
    },
    "oAria": {
        "sSortAscending": ": Ordenar colunas de forma ascendente",
        "sSortDescending": ": Ordenar colunas de forma descendente"
    }
};


var oldExportAction = function (self, e, dt, button, config) {
    if (button[0].className.indexOf('buttons-excel') >= 0) {
        if ($.fn.dataTable.ext.buttons.excelHtml5.available(dt, config)) {
            $.fn.dataTable.ext.buttons.excelHtml5.action.call(self, e, dt, button, config);
        }
        else {
            $.fn.dataTable.ext.buttons.excelFlash.action.call(self, e, dt, button, config);
        }
    } else if (button[0].className.indexOf('buttons-copy') >= 0) {
        $.fn.dataTable.ext.buttons.copyHtml5.action(self, e, dt, button, config);
    } else if (button[0].className.indexOf('buttons-csv') >= 0) {
        if ($.fn.dataTable.ext.buttons.csvHtml5.available(dt, config)) {
            $.fn.dataTable.ext.buttons.csvHtml5.action(self, e, dt, button, config);
        }
        else {
            $.fn.dataTable.ext.buttons.csvFlash.action(self, e, dt, button, config);
        }
    } else if (button[0].className.indexOf('buttons-print') >= 0) {
        $.fn.dataTable.ext.buttons.print.action(self, e, dt, button, config);
    }
};

var newExportAction = function (e, dt, button, config) {
    var self = this;
    var oldStart = dt.settings()[0]._iDisplayStart;

    dt.one('preXhr', function (e, s, data) {
        // Just this once, load all data from the server...
        data.start = 0;
        data.length = 2147483647;

        dt.one('preDraw', function (e, settings) {
            // Call the original action function 
            oldExportAction(self, e, dt, button, config);

            dt.one('preXhr', function (e, s, data) {
                // DataTables thinks the first item displayed is index 0, but we're not drawing that.
                // Set the property to what it was before exporting.
                settings._iDisplayStart = oldStart;
                data.start = oldStart;
            });

            // Reload the grid with the original page. Otherwise, API functions like table.cell(this) don't work properly.
            setTimeout(dt.ajax.reload, 0);

            // Prevent rendering of the full data to the DOM
            return false;
        });
    });

    // Requery the server with the new one-time export settings
    dt.ajax.reload();
};


var buttonsDataTable = [{
    text: "Copiar Selecionados",
    extend: "copyHtml5",
    className: "btn-sm",
    exportOptions: {
        modifier: {
            selected: null
        }
    },
    bShowAll: true
}, {
    text: "CSV",
    extend: "csvHtml5",
    className: "btn-sm",
    oSelectorOpts: {
        page: 'all'
    },
    bShowAll: true
}, {
    text: "Excel",
    extend: "excelHtml5",
    className: "btn-sm",
    oSelectorOpts: {
        page: 'all'
    },
    action: newExportAction,
    bShowAll: true
}, {
    text: "PDF",
    extend: "pdfHtml5",
    className: "btn-sm",
    oSelectorOpts: {
        page: 'all'
    },
    bShowAll: true
}, {
    text: "Imprimir",
    extend: "print",
    className: "btn-sm",
    oSelectorOpts: {
        page: 'all'
    },
    bShowAll: true
    }];

//
// Updates "Select all" control in a data table
//
function updateDataTableSelectAllCtrl(table) {
    var $table = table.table().node();
    var $chkbox_all = $('tbody input[type="checkbox"]', $table);
    var $chkbox_checked = $('tbody input[type="checkbox"]:checked', $table);
    var chkbox_select_all = $('thead input[name="select_all"]', $table).get(0);

    // If none of the checkboxes are checked
    if ($chkbox_checked.length === 0) {
        chkbox_select_all.checked = false;
        if ('indeterminate' in chkbox_select_all) {
            chkbox_select_all.indeterminate = false;
        }

        // If all of the checkboxes are checked
    } else if ($chkbox_checked.length === $chkbox_all.length) {
        chkbox_select_all.checked = true;
        if ('indeterminate' in chkbox_select_all) {
            chkbox_select_all.indeterminate = false;
        }

        // If some of the checkboxes are checked
    } else {
        chkbox_select_all.checked = true;
        if ('indeterminate' in chkbox_select_all) {
            chkbox_select_all.indeterminate = true;
        }
    }
}