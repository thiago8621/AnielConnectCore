/**
 * Tipo de dado para ordenação de timespan pelo Datatable
 * 
 * Uso: 
 *     columns: [{ orderDataType: "timespan" }]
 */
$.fn.dataTable.ext.order['timespan'] = function (settings, col) {

    return this.api().column(col, { order: 'index' })
                     .nodes()
                     .map(val => {
                         let time = $(val).text().replace(/:/g, "");
                         return Number(time);
                     }).toArray()
}