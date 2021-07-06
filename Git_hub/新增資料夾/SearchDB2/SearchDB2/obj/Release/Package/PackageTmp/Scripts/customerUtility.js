(function (window, undefind) {
    var customerUtility = {};

    window.customerUtility = customerUtility;

    customerUtility.addoption = function (selectId, obj) {
        $.each(obj, function (key, data) {
            let value = data.value || '';
            let text = data.text || '';
            let Value = data.Value || '';
            let Text = data.Text || '';
            if (value != '' && text != '')
                $("#" + selectId).append($("<option></option>").attr("value", data.value).text(data.text));
            if(Value != '' && Text != '')
                $("#" + selectId).append($("<option></option>").attr("value", data.Value).text(data.Text));
        })
    }

    customerUtility.getVersion = function (selectId, datepickerId, tableName, url) {
        //#region 選擇reportDate 後要觸發的動作
        tableName = tableName || '';
        var versionFun = {};
        versionFun.tableName = tableName;
        versionFun.fail = function () {
            $("#" + selectId + " option").remove();
            var optionObj = [];
            optionObj.push({ value: "", text: "" });
            customerUtility.addoption(selectId, optionObj);
        }
        versionFun.success = function checkReportDate() {
            $.ajax({
                type: "POST",
                data: JSON.stringify({
                    date: $('#' + datepickerId).val(),
                    tableName: versionFun.tableName
                }),
                dataType: "json",
                url: url,
                contentType: 'application/json',
            })
            .done(function (result) {
                $("#" + selectId + " option").remove();
                if (result.RETURN_FLAG) {
                    var optionObj = [];
                    optionObj.push({ value: "", text: "" })
                    $.each(result.Datas.Data, function (key, value) {
                        optionObj.push({ value: value, text: value })
                    })
                    customerUtility.addoption(selectId, optionObj);
                }
                else {
                    versionFun.fail();
                }
            });
        }

        return versionFun;

        //#endregion 選擇reportDate 後要觸發的動作
    }

    customerUtility.getD54 = function (datepickerId, tableName, url)
    {
        var Fun = {};
        Fun.fail = function () {
        }
        Fun.success = function ()
        {
            $.ajax({
                type: "POST",
                data: JSON.stringify({
                    date: $('#' + datepickerId).val()
                }),
                dataType: "json",
                url: url,
                contentType: 'application/json',
            })
            .done(function (result) {
                if (result.RETURN_FLAG)
                {
                    customerUtility.alert(result.DESCRIPTION, 'w');
                }
            });
        }
        return Fun;
    }

    customerUtility.readCookie = function readCookie(name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    }

    customerUtility.onbeforeunloadFlag = true;
    customerUtility.unloadUrl = '';
    customerUtility.onbeforeunloadfun = function () {
        if (customerUtility.onbeforeunloadFlag) {
            //document.cookie = '.ASPXAUTH=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
            //$.ajax({
            //    type: "GET",
            //    url: customerUtility.unloadUrl,
            //    async: false
            //})
            //.done(function () {

            //});
        }
        else {
            customerUtility.onbeforeunloadFlag = true;
        }
    };

    //var delete_cookie = function (name) {
    //    document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    //};
    
    customerUtility.reportUrl = '';
    customerUtility.reportCommonUrl = '';
    //data => title,className
    //parms => 要傳入sql的參數
    //extensionParms => 報表其他而外的參數
    customerUtility.report = function (data, parms, extensionParms) {
        $.ajax({
            type: "POST",
            url: customerUtility.reportCommonUrl,
            contentType: 'application/json',
            data: JSON.stringify({
                data: data,
                parms: parms,
                extensionParms: extensionParms
            }),
        })
        .done(function (result) {
            if (result.RETURN_FLAG) {
                window.open(customerUtility.reportUrl);
            }
            else
                customerUtility.alert(result.DESCRIPTION,'e');
        })
    };

    customerUtility.reportModel = function (
        title,
        className
        ) {
        var obj = {};
        obj['title'] = title;
        obj['className'] = className;
        return obj;
    };

    customerUtility.reportParm = function (
        key,
        value
        ) {
        var obj = {};
        obj['key'] = key;
        obj['value'] = value;
        return obj;
    };


    $('.select-editable input').on('input',
        function () {
            $(this).prev().val('');
        });
    $('.select-editable select').on('change',
        function () {
        $(this).next().trigger('focusout');
    });
       
    customerUtility.remove = function (obj, el) {
        // if the collections is an array
        if (obj instanceof Array) {
            // use jquery's `inArray` method because ie8 
            // doesn't support the `indexOf` method
            if (typeof el == "number") {
                obj.splice(el, 1);
            }
            else {
                if ($.inArray(el, obj) != -1) {
                    obj.splice($.inArray(el, obj), 1);
                }
            }

        }
            // it's an object
        else if (obj.hasOwnProperty(el)) {
            delete obj[el];
        }
        return obj;
    }

    customerUtility.fixCheckbox = function ()
    {
        $('.checkbox').find('input[type=checkbox]').next('[type=hidden]').remove();
    }

    customerUtility.alert = function (message, type)
    {
        let flag = '';
        flag = 'toastr';
        //flag = 'alert';
        if (flag == 'toastr')
        {
            type = type || '';
            if (type == 's') //
                toastr.success(message);
            else if (type == 'w') //warning
                toastr.warning(message);
            else if (type == 'i') //info
                toastr.info(message);
            else if (type == 'e') //error
                toastr.error(message);
            else
                alert(message);
        }
        else
            alert(message);
    }

    customerUtility.checkCacheCommonUrl = '';
    customerUtility.checkDialog = function (id, key, message)
    {
        id = id || '';
        if (id != '')
        {
            let divId = 'IFRS9CheckDialog';
            let dialogId = divId + id;        
            let _message = message || '';
            if (_message == '') {
                $.ajax({
                    type: "POST",
                    url: customerUtility.checkCacheCommonUrl,
                    contentType: 'application/json',
                    data: JSON.stringify({
                        Key: key
                    }),
                })
                .done(function (result) {
                    _message = result.message;
                    let _title = result.title;
                    checkDataDialog(divId, dialogId, id, _message, _title, key);
                })
            }
            else {
                checkDataDialog(divId, dialogId, id, _message);
            }
        }
    }

    function checkDataDialog(divId,dialogId, id, message,title,key)
    {
        title = title || '';
        message = message || '';
        key = key || '';
        if (message != '')
        {
            let str = '';
            str += '<div id ="' + dialogId + '" style="display:none">'
            str += '<table style="width:100%">';
            str += '<tr>';
            str += '<td colspan="2">';
            str += '<textarea id="' + id + '" readonly maxlength="255" style="overflow-y: scroll; white-space:pre-wrap;max-width:100%;width:100%;height:500px;" ></textarea>';
            str += '</td>';
            str += '</tr>';
            if (key != '')
            {
                str += '<tr>';
                str += '<td style="width:100px">';
                str += '<label>下載訊息檔 : </label>';
                str += '</td>';
                str += '<td>';
                str += "<a href='#' class='openDialog dlfile' id='" + id + "DLFile'  return:false; name='" + id + "' title='" + title + "'>" + title + "</a>";
                str += '</td>';
                str += '</tr>';
            }         
            str += '</table>';
            str += '</div>';
            $('#' + divId).append(str);
            $('#' + id).val(message);
            $("#" + dialogId).dialog({
                title: "IFRS9檢核訊息 " + title,
                position: { my: "top+5%", at: "center top", of: window },
                width: '60%',
                height: 'auto',
                autoOpen: false,
                resizable: true,
                closeText: "取消",
                modal: true,
                close: function (event, ui) {
                    this.remove();
                }
            });
            $("#" + dialogId).dialog("open");
            $('#' + id).scrollTop(0);
            if (key != '')
            {
                $('#' + id + 'DLFile').off('click');
                $('#' + id + 'DLFile').on('click', function () {
                    window.location.href = customerUtility.checkMessageDL + key;
                });
            }
        }
    }

})(window);