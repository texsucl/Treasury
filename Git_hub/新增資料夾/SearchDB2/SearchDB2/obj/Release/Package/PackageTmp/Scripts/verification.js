(function (window, undefind) {
    var verified = {};
    var created = {};
    var dateFormat;
    dateFormat = /^((?!0000)[0-9]{4}[/]((0[1-9]|1[0-2])[/](0[1-9]|1[0-9]|2[0-8])|(0[13-9]|1[0-2])[/](29|30)|(0[13578]|1[02])[/]31)|([0-9]{2}(0[48]|[2468][048]|[13579][26])|(0[48]|[2468][048]|[13579][26])00)[/]02[/]29)$/;

    window.verified = verified;
    window.created = created;

    String.prototype.Blength = function () {
        var arr = this.match(/[^\x00-\xff]/ig);
        return arr == null ? this.length : this.length + arr.length;
    }

    var errorPlacementfun = function (error, element)
    {
        if (element.is('input:text') &&
            element.prev() != null &&
            element.prev().is('select')) {
            error.appendTo(element.parent().parent().next());
        }
        else {
            error.appendTo(element.parent());
        }
    }

    verified.number = function (formid, elementid) {
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        });
        $('#' + elementid).rules('add', {
            number: true,
            messages: {
                number: '請輸入數字',
            }
        })
    }

    verified.minlength = function (formid, elementid, value, msg) {
        value = value || 10;
        msg = msg || message.minlength(value);
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        });
        $('#' + elementid).rules('add', {
            minlength: value,
            messages: {
                minlength: msg,
            }
        })
    }
    verified.maxlength = function (formid, elementid, value, msg) {
        value = value || 10;
        msg = msg || message.maxlength(value);
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        });
        $('#' + elementid).rules('add', {
            maxlength: value,
            messages: {
                maxlength: msg,
            }
        })
    }

    verified.required = function (formid, elementid, message) {
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        });
        $('#' + elementid).rules('add', {
            required: true,
            messages: {
                required: message,
            }
        })
    }

    verified.datepicker = function (formid, datepickerid, reportDateFlag) {
        reportDateFlag = reportDateFlag || false;

        $("#" + formid).validate({
            //rules: {
            //    datepicker: { dateFormate: date }
            //},
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證
        $.validator.addMethod("reportDateFormate",
        function (value, element, arg) {
            return verified.isDate(value, true);
        }, message.reportDate);

        $.validator.addMethod("dateFormate",
        function (value, element, arg) {
            return verified.isDate(value, false);
        }, message.date);
        //#endregion
        if (reportDateFlag)
        $('#' + datepickerid).rules('add', {
            reportDateFormate: true,
        })
        else
        $('#' + datepickerid).rules('add', {
            dateFormate: true,
        })
    }

    created.createDatepicker = function (datepickerid, reportDateFlag, date, completeEvent) {
        reportDateFlag = reportDateFlag || false;
        var d = null;
        if (!(date === d)) {
            if (reportDateFlag) {
                d = verified.reportDate();
            }
            else {
                if (verified.isDate(date, false)) {
                    d = verified.datepickerStrToDate(date);
                }
                else {
                    d = getOnlyDate();
                }
            }
        }

        $("#" + datepickerid).datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: 'yy/mm/dd',
            showOn: "both",
            buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
            onSelect: function (value) {
                if (verified.isDate(value, reportDateFlag))
                {
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
                }
            },
            onClose: function (value) {
                if (verified.isDate(value, reportDateFlag))
                {
                    if(typeof completeEvent == 'function')
                        completeEvent();
                }
                if (typeof completeEvent != "undefined" &&
                    typeof completeEvent.success == 'function' &&
                    typeof completeEvent.fail == 'function')
                {
                    if (verified.isDate(value, reportDateFlag))
                    {
                        completeEvent.success();
                    }
                    else
                    {
                        completeEvent.fail();
                    }
                }
            }
        }).datepicker('setDate', d);

        if (typeof completeEvent != "undefined" &&
            typeof completeEvent.success == 'function')
        {
            completeEvent.success();
        }
    }

    created.clearDatepickerRangeValue = function (
        datepickerStartid, datepickerEndid) {
        $("#" + datepickerStartid).val('');
        $("#" + datepickerStartid).datepicker("option", "maxDate", null);
        $("#" + datepickerEndid).val('');
        $('#' + datepickerEndid).datepicker("option", "minDate", null);
    }

    created.createDatepickerRange = function (datepickerStartid,
        datepickerEndid, reportDateFlag) {
        var format = 'yy/mm/dd';
        reportDateFlag = reportDateFlag || false;

        var from = $("#" + datepickerStartid)
                    .datepicker({
                        changeMonth: true,
                        changeYear: true,
                        dateFormat: format,
                        showOn: "both",
                        buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
                        onSelect: function (value) {
                            to.datepicker("option", "minDate", getDate(this));
                            if (verified.isDate(value, reportDateFlag)) {
                                $(this).parent().children().each(function () {
                                    if ($(this).is('label') && $(this).hasClass('error'))
                                        $(this).remove();
                                    if ($(this).is('input') && $(this).hasClass('error'))
                                        $(this).removeClass('error');
                                })
                            }                          
                        }
                    });

        from.off('change');
        from.on('change', function () {
            if($(this).val() == '')
                to.datepicker("option", "minDate", null);
        });

        var to = $("#" + datepickerEndid).datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: format,
            showOn: "both",
            buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
            onSelect: function (value) {
                from.datepicker("option", "maxDate", getDate(this));
                if (verified.isDate(value, reportDateFlag)) {
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
                }
            }
        });

        to.off('change');
        to.on('change', function () {
            if ($(this).val() == '')
                from.datepicker("option", "maxDate", null);
        });

        function getDate(element) {
            var date;
            try {
                date = $.datepicker.parseDate(format, element.value);
            } catch (error) {
                date = null;
            }
            return date;
        }
    }

    verified.rdlcDate = function (value) {
        value = value || '';
        if (verified.isDate(value))
            return value.replace(/\//g, '-');
        else
            return value;
    }

    verified.isDate = function (value, reportDate) {
        reportDate = reportDate || false;
        value = value || '';
        if ((typeof reportDate === 'boolean') && reportDate) {
            return dateFormat.test(value) ? verifiedReportDate(value) : false;
        }
        else
            return dateFormat.test(value);
    }

    verified.reportDate = function () {
        var d = getOnlyDate();
        var day = d.getDate();
        if (day <= 5) {
            d.setDate(1); //設定為當月份的第一天
            d.setDate(d.getDate() - 1); //將日期-1為上月的最後一天
            return d;
        }
        else {
            //d.setDate(25);
            d.setDate(1); //第一天
            d.setMonth((d.getMonth() + 1)); //下一個月
            d.setDate(d.getDate() - 1); //這個月最後一天
            return d;
        }
    }

    //formate string(yyyy/MM/dd) to date 失敗回傳 false
    verified.datepickerStrToDate = function (value) {
        if (dateFormat.test(value)) {
            var d = value.split('/');
            return new Date(d[0] + '-' + d[1] + '-' + d[2]);
        }
        return false;
    }

    verified.dateToStr = function (value)
    {
        return value.getFullYear() + '/' + padLeft((value.getMonth() + 1), 2) + '/' + (value.getDate())
    }

    verified.textAreaLength = function (evt) {
        var arrayExceptions = [8, 16, 17, 18, 20, 27, 35, 36, 37,
         38, 39, 40, 45, 46, 144];
        var maxValue = 255;
        var obj = $(this);
        if (typeof (obj.attr('maxlength')) != 'undefined') {
            maxValue = parseInt(obj.attr('maxlength'));
        }
        if ((obj.val().Blength() + 1 > maxValue) && $.inArray(evt.keyCode, arrayExceptions) === -1) {
            return false;
        }
    }

    function verifiedReportDate(value) {
        if (dateFormat.test(value)) {           
            let datepicker = verified.datepickerStrToDate(value);
            if (!datepicker) {
                return false;
            }
            if (datepicker.getDate() === 25)
                return true;
            let d = getOnlyDate();
            d.setFullYear(datepicker.getFullYear());
            d.setDate(1);
            d.setMonth(datepicker.getMonth());
            //d.setDate(1); //第一天
            d.setMonth((d.getMonth() + 1)); //下一個月
            d.setDate(d.getDate() - 1); //這個月最後一天
            if (datepicker.getDate() === d.getDate())
                return true;
            return false;
        }
        return false;
    }

    function getOnlyDate() {
        var d = new Date();
        d = new Date(d.getFullYear() + '-' + padLeft((d.getMonth() + 1), 2) + '-' + padLeft((d.getDate()),2));
        return d;
    }

    function padLeft(str, lenght, padStr) {
        str = (str || '') + '';
        if (typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if (str.length >= lenght)
            return str;
        else
            return padLeft(padStr + str, lenght , padStr);
    }
    function padRight(str, lenght, padStr) {
        str = (str || '') + '';
        if (typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if (str.length >= lenght)
            return str;
        else
            return padRight(str + padStr, lenght, padStr);
    }
})(window);