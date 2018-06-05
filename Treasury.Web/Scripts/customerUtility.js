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
        //flag = 'toastr';
        flag = 'alert';
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

})(window);