(function (window, undefind) {
    var jqgridCustom = {};

    var defaultrowNum = 10;
    var defaultrowList = [10, 20, 30];
    var defaultheight = 'auto';
    var defaultwidth = 'auto';

    window.jqgridCustom = jqgridCustom;


    //#region jqgridCustom.createDialog 範例
    //var obj = [
    //   { 'name': 'testtxt', 'type': 'string', 'title': '測試', 'max': '3', 'req': 'true' },
    //   { 'name': 'testdate', 'type': 'date', 'title': '測試日期', 'req': 'true' }
    //]
    //jqgridCustom.createDialog(dialogid, obj);
    //#endregion

    jqgridCustom.createDialog =
    function (dialogid, data) {
        var str = '';
        str += '<input type="hidden" id="actionType" value="" />';
        str += '<form id="' + dialogid + 'Form">';
        str += '<table style="width:100%">';
        var reqobjs = [];
        var datepickers = [];
        var reqobj = function (type, name, msg) {
            this.type = type;
            this.name = name;
            this.msg = msg;
        };

        $.each(data, function (dkey, dvalue) {
            var tr = '<tr id="' + dialogid + dvalue.name + 'tr'+ '" ';
            var tdtitle = '<td style="white-space:nowrap; text-align:right">';
            var tdinput = '<td style="white-space:nowrap;padding-bottom: 5px;padding-top: 5px;">';
            var tdinput2 = '';
            var name = dialogid + dvalue.name;
            var type = '';
            var text = '';
            var beforeStr = '';
            var afterStr = '';
            $.each(dvalue, function (key, value) {
                if (key === 'name')
                    name = (dialogid + value);
                if (key === 'title')
                {
                    text = value;
                    tdtitle += (value + ' :&ensp;');
                }
                if (key === 'type') {
                    switch (value) {
                        case 'selectOption':
                            tdinput2 += '<select class="form-control" id="' + name + '" name="' + name + '" style="width:215px;display:inline-block"></select'
                            break;
                        case 'date':
                            type = 'date';
                            tdinput2 += '<input type="text" style="width:180px" ';
                            tdinput2 += ('id="' + name + '" name="' + name + '" ');
                            datepickers.push(name);
                            break;
                        case 'string':
                        default:
                            type = 'string';
                            tdinput2 += '<input type="text" style="width:215px" ';
                            tdinput2 += ('id="' + name + '" name="' + name + '"');
                            break;
                    }
                }
                if (key === 'max' && type !== 'date')
                    tdinput2 += (' maxlength = ' + value + ' ');
                if (key === 'req' && value === 'true')
                    reqobjs.push(new reqobj(key, name, text));
                if(key === 'hide' && value === 'true')
                    tr += (' hidden ');
                if (key === 'dis' && value === 'true')
                    tdinput2 += (' disabled ');
                if(key === 'num' && value === 'true')
                    reqobjs.push(new reqobj(key, name, text));
                if (key === 'cls')
                {
                    tr += ' class="';
                    $.each(value.split(','), function (i, v) {
                        tr += (v + ' ');
                    });
                    tr += '" ';
                }
                if (key === 'beforeStr')
                {
                    beforeStr = value;
                }
                if (key === 'afterStr')
                {
                    afterStr = value;
                }
            })
            tdtitle += '</td>';
            tdinput2 += ('>' + afterStr + '</td>');
            tr += '>';
            tr += tdtitle;
            tr += (tdinput + beforeStr + tdinput2);
            tr += '</tr>';
            str += tr;
        })
        str += ('<tr id="customer' + dialogid + 'tr">');
        str += '<tr class="actbtn">';
        str += '<td colspan="2" style="white-space:nowrap; text-align:center">'
        str += '<input type="button" class=" btn btn-primary" style="margin-right:30px;margin-top:10px;float:left;" id="' + dialogid + 'btnSave" value="儲存" />';
        str += '<input type="button" class=" btn btn-primary" style="margin-right:30px;margin-top:10px;float:left;" id="' + dialogid + 'btnDelete" value="刪除" />';
        str += '<input type="button" class=" btn btn-primary" style="margin-top:10px;float:right;" id="' + dialogid + 'btnCancel" value="取消" /></td>';
        str += '</tr>';
        str += '</table>';
        str += '</form>';
        $('#' + dialogid).append(str);

        $.each(datepickers, function (key, value) {
            $("#" + value).datepicker({
                changeMonth: true,
                changeYear: true,
                dateFormat: 'yyyy-mm-dd',
                showOn: "both",
                buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
                onSelect: function (value) {
                    if (verified.isDate(value))
                        $(this).parent().children().each(function () {
                            if ($(this).is('label') && $(this).hasClass('error'))
                                $(this).remove();
                            if ($(this).is('input') && $(this).hasClass('error'))
                                $(this).removeClass('error');
                        })
                }
            });
        })

        $.each(reqobjs, function (key, value) {
            if (value.type === 'req')
                verified.required(dialogid + 'Form', value.name, message.required(value.msg));
            if (value.type === 'date')
                verified.datepicker(dialogid + 'Form', value.name, false, $('#' + value.name).val());
            if (value.type === 'num')
                verified.number(dialogid + 'Form', value.name);
        })

        $("#" + dialogid).dialog({
            autoOpen: false,
            resizable: true,
            height: 'auto',
            width: 'auto',
            position: { my: "center", at: "center", of: window },
            closeText: "取消",
            modal: true
        });

        $('#' + dialogid + 'btnCancel').on('click', function () {
            $('#' + dialogid).dialog('close');
        });
    }

    //#region jqgridCustom.randerAction 範例
    //colName unshift Actions
    //colModel unshift { name: "act", index: "act", width: 100, sortable: false }
    //jqgridCustom.createDialog(dialogid, obj);
    //jqgridCustom.randerAction(jqGridId, 'A41',fun());
    //#endregion

    jqgridCustom.randerAction =
    function (jqGridId, viewId, fun) {
        var ids = $("#" + jqGridId).jqGrid('getDataIDs');
        if (typeof fun != "undefined" && typeof fun.Edit == "function" && typeof fun.Dele == "function" && typeof fun.View == "function")
        {
            $.each(ids, function (key, i)
            {
                var divStart = '<div class="btn-group">';
                var edit = "<a title='修改' class='btn actionEditIcon' style='padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;'" +
                           " href='#' id='" + viewId + jqGridId + "Edit" + i + "' return:false;><i class='fa fa-pencil-square-o fa-lg'></i></a>";
                var view = "<a title='檢視' class='btn actionViewIcon' style='padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;'" +
                           " href='#' id='" + viewId + jqGridId + "View" + i + "' return:false;><i class='fa fa-search fa-lg'></i></a>";
                var dele = "<a title='刪除' class='btn actionDeleIcon' style='padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;'" +
                           " href='#' id='" + viewId + jqGridId + "Dele" + i + "' return:false;><i class='fa fa-trash fa-lg'></i></a>";
                var divEnd = '</div>';
                $("#" + jqGridId).jqGrid('setRowData', i , { act: divStart + edit + view + dele + divEnd });
                $('#' + viewId + jqGridId + "Edit" + i).on('click', function () { fun.Edit(i) });
                $('#' + viewId + jqGridId + "View" + i).on('click', function () { fun.View(i)  });
                $('#' + viewId + jqGridId + "Dele" + i).on('click', function () { fun.Dele(i) });
            })
        }
        else
        {
            for (var i = 0; i < ids.length; i++) {
                var divStart = '<div class="btn-group">';
                var edit = '<a title="修改" class="btn actionEditIcon" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                           ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'Edit(' + (i + 1) + ');\" return:false;><i class="fa fa-pencil-square-o fa-lg"></i></a>';
                var view = '<a title="檢視" class="btn actionViewIcon" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                           ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'View(' + (i + 1) + ')\" return:false;><i class="fa fa-search fa-lg"></i></a>';
                var dele = '<a title="刪除" class="btn actionDeleIcon" style="padding-right:4px;padding-left:4px;padding-bottom:0px;padding-top:0px;"' +
                           ' href="#" onclick=\"javascript:' + viewId + jqGridId + 'Dele(' + (i + 1) + ')\" return:false;><i class="fa fa-trash fa-lg"></i></a>';
                var divEnd = '</div>';
                $("#" + jqGridId).jqGrid('setRowData', i + 1, { act: divStart + edit + view + dele + divEnd });
            }
        }
    }


    jqgridCustom.createJqgridByCache =
        function (
            jqgridDivId,
            listId,
            pagerId,
            url,
            postData,
            colNameArray,
            colModelArray,
            caption,
            page,
            completeFun,
            rownumberFlag,
            width,
            height,
            rowNum,
            rowList,
            overflowFlag
            )
        {
            rowNum = rowNum || defaultrowNum;
            rowList = rowList || defaultrowList;
            height = height || defaultheight;
            width = width || defaultwidth;
            rownumberFlag = rownumberFlag || false;
            overflowFlag = overflowFlag || true;
            page = page || '1';
            clearJqgrid(jqgridDivId);
            $('#' + jqgridDivId).append('<table id="' + listId + '"></table>');
            $('#' + jqgridDivId).append('<div id="' + pagerId + '"></div>');
            $('#' + listId).jqGrid({
                url: url,
                datatype: 'json',
                mtype: 'POST',
                postData: postData,
                jsonReader:
                {
                    repeatitems: false,
                },
                colNames: colNameArray,
                colModel: colModelArray,
                rowNum: rowNum,
                rowList: rowList,
                rownumbers: rownumberFlag,
                rownumWidth:40,
                pager: '#' + pagerId,
                page: page,
                height: height,
                width: width,
                viewrecords: true,
                caption: caption,
                resizable: false,
                //autoencode: true,
                viewsortcols: [true, 'vertival', true],
                ajaxRowOptions: { contentType: "application/json" },
                serializeRowData: function (data) {
                    return JSON.stringify(data);
                },
                loadComplete: function () {
                    jqgridCustom.updatePagerIcons($(this));
                    if (typeof completeFun != "undefined" && typeof completeFun == "function")
                        completeFun(listId);
                    if (overflowFlag)
                        $('#' + jqgridDivId).find('.ui-jqgrid-bdiv').css('overflow', 'unset');
                    if (page != '1' && $("#" + listId).getRowData().length == 0)
                    {
                        jqgridCustom.createJqgridByCache(
                        jqgridDivId,
                        listId,
                        pagerId,
                        url,
                        postData,
                        colNameArray,
                        colModelArray,
                        caption,
                        "1",
                        completeFun,
                        rownumberFlag,
                        width,
                        height,
                        rowNum,
                        rowList,
                        overflowFlag
                        )
                    }
                }
            });
            $('#' + listId).jqGrid('navGrid', '#' + pagerId, { edit: false, add: false, del: false, search: false, refresh: false });
        }

    jqgridCustom.createJqgridByLocal =
        function (
            jqgridDivId,
            listId,
            pagerId,
            localdata,
            colNameArray,
            colModelArray,
            caption,
            page,
            completeFun,
            rownumberFlag,
            width,
            height,
            rowNum,
            rowList,
            overflowFlag
            )
        {
            rowNum = rowNum || defaultrowNum;
            rowList = rowList || defaultrowList;
            height = height || defaultheight;
            width = width || defaultwidth;
            page = page || '1';
            rownumberFlag = rownumberFlag || false;
            overflowFlag = overflowFlag || true;
            clearJqgrid(jqgridDivId);
            $('#' + jqgridDivId).append('<table id="' + listId + '"></table>');
            $('#' + jqgridDivId).append('<div id="' + pagerId + '"></div>');
            $('#' + listId).jqGrid({
                data: localdata,
                datatype: 'local',
                colNames: colNameArray,
                colModel: colModelArray,
                rowNum: rowNum,
                rowList: rowList,
                rownumbers: rownumberFlag,
                rownumWidth: 40,
                pager: '#' + pagerId,
                page : page,
                height: height,
                width: width,
                viewrecords: true,
                caption: caption,
                resizable: false,
                shrinkToFit: false,
                //autoencode: true,
                viewsortcols: [true, 'vertival', true],
                ajaxRowOptions: { contentType: "application/json" },
                serializeRowData: function (data) {
                    return JSON.stringify(data);
                },
                loadComplete: function () {
                    jqgridCustom.updatePagerIcons($(this));
                    if (page != '1' && $("#" + listId).getRowData().length == 0) {
                        jqgridCustom.createJqgridByLocal(
                        jqgridDivId,
                        listId,
                        pagerId,
                        localdata,
                        colNameArray,
                        colModelArray,
                        caption,
                        "1",
                        completeFun,
                        rownumberFlag,
                        rowNum,
                        rowList,
                        height,
                        width)
                    }
                    if (typeof completeFun != "undefined" && typeof completeFun == "function")
                        completeFun(listId);
                    if (overflowFlag)
                        $('#' + jqgridDivId).find('.ui-jqgrid-bdiv').css('overflow', 'unset');
                }
            });
            $('#' + listId).jqGrid('navGrid', '#' + pagerId, { edit: false, add: false, del: false ,search: false,refresh : false});
        }

    //#region jqgridCustom.updatePagerIcons 範例
    //loadComplete
    //var table = $(this);
    //jqgridCustom.updatePagerIcons(table);
    //#endregion
    jqgridCustom.updatePagerIcons =
    function updatePagerIcons(table,div)  //table => loadComplete this.table
    {
        div = div || '';
        //var replacement =
        //{
        //    'ui-icon-seek-first': 'ace-icon fa fa-angle-double-left bigger-140',
        //    'ui-icon-seek-prev': 'ace-icon fa fa-angle-left bigger-140',
        //    'ui-icon-seek-next': 'ace-icon fa fa-angle-right bigger-140',
        //    'ui-icon-seek-end': 'ace-icon fa fa-angle-double-right bigger-140'
        //};
        //if (div != '') {
        //    $('#' + div + ' .ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
        //        var icon = $(this);
        //        var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

        //        if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
        //    })
        //}
        //else {
        //    $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
        //        var icon = $(this);
        //        var $class = $.trim(icon.attr('class').replace('ui-icon', ''));

        //        if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
        //    })
        //}
        $(table).parents('.jqd:first').find('.openDialog').each(function () {
            var title = $(this).attr('title');
            $(this).parent('td:first').attr('title', title);
        });
        $(table).parents('.jqd:first').find('.cbox').each(function () {
            var id = $(this).attr('id');
            if (id.indexOf("cb") > -1) //全選按鈕
            {               
                var checkbox = $(this);
                //var id = $(this).attr('id');
                var parent = checkbox.parent();
                if (!parent.hasClass("checkbox-info"))
                {
                    parent.addClass("checkbox");
                    parent.addClass("checkbox-info");
                    parent.css("margin-left", "1px");
                    $(checkbox).after('<label>&nbsp;</label>');
                }
            }
            else //其餘
            {
                var checkbox = $(this);
                var parent = checkbox.parent();
                var divId = checkbox.attr('id') + "div";
                var div = '<div id="' + divId + '" class="checkbox checkbox-info" style="text-align:left;padding-top:0px;margin-top:0px;margin-bottom:0px;"></div>';
                checkbox.before(div);               
                $('#'+divId).append(checkbox);
                $(checkbox).after('<label>&nbsp;</label>');
            }
        });
    }

    jqgridCustom.checkboxSet =
    function (tableId, btnId)
    {
        var checkbox = $('#' + tableId).parents('.jqd:first').find('.cbox[Id^="cb"]');
        if (typeof checkbox != 'undefind') {
            checkbox.on('change', function () {
                setTimeout(
                 function () {
                     $('#' + btnId).prop('disabled', set());
                 }
                 , 1);                              
            })
        }
        $('#' + btnId).prop('disabled', set());
        $('#' + tableId).find('.cbox').each(function () {
            $(this).off('change');
            $(this).on('change', function () {               
                $('#' + btnId).prop('disabled', set());
                if (typeof checkbox != 'undefind')
                {
                    setTimeout(
                        function(){
                            checkbox.prop('checked', all())
                        }
                        , 1);
                }
            })
        })
        function all()
        {
            return $('#' + tableId).find('.cbox').length == $('#' + tableId).find('.cbox:checked').length;
        }
        function set()
        {
            return $('#' + tableId).find('.cbox:checked').length == 0;
        }
    }

    jqgridCustom.getPage = function (id) {
        return $('#' + id).find('.ui-pg-input').val() || '1' ;
    }

    jqgridCustom.getRowNum = function (id) {
        return $('#' + id).find('.ui-pg-selbox').val() || defaultrowNum;
    }

    jqgridCustom.hideFrozenTitle =
    function () {
        $('.ui-jqgrid-view > .frozen-div').find('.ui-jqgrid-resize').hide()
    }

    function clearJqgrid(gridDivId) {
        $('#' + gridDivId).children().remove();
    }

})(window);