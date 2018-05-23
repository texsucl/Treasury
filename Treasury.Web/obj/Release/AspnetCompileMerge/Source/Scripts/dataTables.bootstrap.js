/*! DataTables Bootstrap 3 integration
 * ©2011-2015 SpryMedia Ltd - datatables.net/license
 */

/**
 * DataTables integration for Bootstrap 3. This requires Bootstrap 3 and
 * DataTables 1.10 or newer.
 *
 * This file sets the defaults and adds options to DataTables to style its
 * controls using Bootstrap. See http://datatables.net/manual/styling/bootstrap
 * for further information.
 */
(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD
        define(['jquery', 'datatables.net'], function ($) {
            return factory($, window, document);
        });
    }
    else if (typeof exports === 'object') {
        // CommonJS
        module.exports = function (root, $) {
            if (!root) {
                root = window;
            }

            if (!$ || !$.fn.dataTable) {
                // Require DataTables, which attaches to jQuery, including
                // jQuery if needed and have a $ property so we can access the
                // jQuery object that is used
                $ = require('datatables.net')(root, $).$;
            }

            return factory($, root, root.document);
        };
    }
    else {
        // Browser
        factory(jQuery, window, document);
    }
}(function ($, window, document, undefined) {
    'use strict';
    var DataTable = $.fn.dataTable;


    /* Set the defaults for DataTables initialisation */
    $.extend(true, DataTable.defaults, {
        processing: true,
        serverSide: true,
        filter: false,
        orderMulti: false,
        dom:
            "<'row'<'col-sm-4' l><'col-sm-20'f>>" +
            "<'row'<'toolbar col-sm-24'>>" +
            "<'row'<'col-sm-24'tr>>" +
            "<'row'<'col-sm-10'i><'col-sm-14'p>>",
        renderer: 'bootstrap',
        bJQueryUI: true,
        language: {
            "paginate": {
                "first": locale.pageFirst,
                "last": locale.pageLast,
                "next": ">>",
                "previous": "<<"
            },
            "sEmptyTable": locale.searchNoData,
            "sInfo": locale.tableDisplayPaginate,
            "sInfoEmpty": locale.searchNoData,
            "sLoadingRecords": locale.waiting,
            "sProcessing": locale.waiting,
            "sSearch": locale.search,
            "sLengthMenu": locale.showPageRecords,
            "sZeroRecords": locale.searchNoData
        }
    });


    /* Default class modification */
    $.extend(DataTable.ext.classes, {
        sWrapper: "dataTables_wrapper form-inline dt-bootstrap",
        sFilterInput: "form-control input-sm",
        sLengthSelect: "form-control input-sm",
        sProcessing: "dataTables_processing panel panel-default"
    });


    /* Bootstrap paging button renderer */
    DataTable.ext.renderer.pageButton.bootstrap = function (settings, host, idx, buttons, page, pages) {
        var api = new DataTable.Api(settings);
        var classes = settings.oClasses;
        var lang = settings.oLanguage.oPaginate;
        var aria = settings.oLanguage.oAria.paginate || {};
        var btnDisplay, btnClass, counter = 0;

        var attach = function (container, buttons) {
            var i, ien, node, button;
            var clickHandler = function (e) {
                e.preventDefault();
                if (!$(e.currentTarget).hasClass('disabled') && api.page() != e.data.action) {
                    api.page(e.data.action).draw('page');
                }
            };

            for (i = 0, ien = buttons.length ; i < ien ; i++) {
                button = buttons[i];

                if ($.isArray(button)) {
                    attach(container, button);
                }
                else {
                    btnDisplay = '';
                    btnClass = '';

                    switch (button) {
                        case 'ellipsis':
                            btnDisplay = '&#x2026;';
                            btnClass = 'disabled';
                            break;

                        case 'first':
                            btnDisplay = lang.sFirst;
                            btnClass = button + (page > 0 ?
                                '' : ' disabled');
                            break;

                        case 'previous':
                            btnDisplay = lang.sPrevious;
                            btnClass = button + (page > 0 ?
                                '' : ' disabled');
                            break;

                        case 'next':
                            btnDisplay = lang.sNext;
                            btnClass = button + (page < pages - 1 ?
                                '' : ' disabled');
                            break;

                        case 'last':
                            btnDisplay = lang.sLast;
                            btnClass = button + (page < pages - 1 ?
                                '' : ' disabled');
                            break;

                        default:
                            btnDisplay = button + 1;
                            btnClass = page === button ?
                                'active' : '';
                            break;
                    }

                    if (btnDisplay) {
                        node = $('<li>', {
                            'class': classes.sPageButton + ' ' + btnClass,
                            'id': idx === 0 && typeof button === 'string' ?
								settings.sTableId + '_' + button :
								null
                        })
                            .append($('<a>', {
                                'href': '#',
                                'aria-controls': settings.sTableId,
                                'aria-label': aria[button],
                                'data-dt-idx': counter,
                                'tabindex': settings.iTabIndex
                            })
                                .html(btnDisplay)
                            )
                            .appendTo(container);

                        settings.oApi._fnBindAction(
                            node, { action: button }, clickHandler
                        );

                        counter++;
                    }
                }
            }
        };

        // IE9 throws an 'unknown error' if document.activeElement is used
        // inside an iframe or frame. 
        var activeEl;

        try {
            // Because this approach is destroying and recreating the paging
            // elements, focus is lost on the select button which is bad for
            // accessibility. So we want to restore focus once the draw has
            // completed
            activeEl = $(host).find(document.activeElement).data('dt-idx');
        }
        catch (e) { }

        attach(
            $(host).empty().html('<ul class="pagination"/>').children('ul'),
            buttons
        );

        if (activeEl) {
            $(host).find('[data-dt-idx=' + activeEl + ']').focus();
        }
    };


    return DataTable;
}));

(function (factory) {
    if (typeof define === "function" && define.amd) {
        define(["jquery", "moment", "datatables"], factory);
    } else {
        factory(jQuery, moment);
    }
}(function ($, moment) {
    $.fn.dataTable.moment = function (format, locale) {
        var types = $.fn.dataTable.ext.type;

        // Add type detection
        types.detect.unshift(function (d) {
            d = fb.convertNETDateTime(d);
            // Strip HTML tags if possible
            if (d && d.replace) {
                d = d.replace(/<.*?>/g, '');
            }

            // Null and empty values are acceptable
            if (d === '' || d === null) {
                return 'moment-' + format;
            }

            return moment(d, format, locale, true).isValid() ?
                'moment-' + format :
                null;
        });

        // Add sorting method - use an integer for the sorting
        types.order['moment-' + format + '-pre'] = function (d) {
            if (d && d.replace) {
                d = d.replace(/<.*?>/g, '');
            }
            return d === '' || d === null ?
                -Infinity :
                parseInt(moment(d, format, locale, true).format('x'), 10);
        };
    };

    $.fn.dataTable.render.moment = function (from, to, locale) {
        // Argument shifting
        if (arguments.length === 1) {
            locale = 'zh_tw';
            to = from;
            from = 'YYYY/MM/DD HH:mm:ss';
        }
        else if (arguments.length === 2) {
            locale = 'zh_tw';
        }

        return function (d, type, row) {
            d = fb.convertNETDateTime(d);
            var m = window.moment(d, from, locale, true);
            // Order and type get a number value from Moment, everything else
            // sees the rendered value
            return m.format(type === 'sort' || type === 'type' ? 'x' : to);
        };
    };
}));