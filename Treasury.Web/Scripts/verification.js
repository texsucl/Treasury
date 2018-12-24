(function (window, undefind) {
    var verified = {};
    var created = {};
    var dateFormat;
    var positiveInt;
    var englishFormat;
    var englishUpperFormat;
    var englishNumberFormat;
    var rateFormat;
    var timeFormat;

    dateFormat = /^((?!0000)[0-9]{4}[/|-]((0[1-9]|1[0-2])[/|-](0[1-9]|1[0-9]|2[0-8])|(0[13-9]|1[0-2])[/|-](29|30)|(0[13578]|1[02])[/|-]31)|([0-9]{2}(0[48]|[2468][048]|[13579][26])|(0[48]|[2468][048]|[13579][26])00)[/|-]02[/|-]29)$/;
    englishFormat = /^[a-zA-Z]+$/;
    englishUpperFormat = /^[A-Z]+$/;
    positiveInt = /^[0-9]+$/;
    englishNumberFormat = /^[a-zA-Z0-9]+$/;
    priceFormate = /^([0-9]{1,})+(.[0-9]{1,})?$/;
    //rateFormat = /^([0-9]{1,2})?(\.[0-9]{1,4})?$/;
    rateFormat = /^(?![0]{1})([0-9]{1,2})?(\.[0-9]{1,4})?$|(([0]{1})+(\.[0-9]{1,4}))+$/;
    timeFormat = /^([01][0-9]|2[0-3]):[0-5][0-9]$/;

    window.verified = verified;
    window.created = created;

    String.prototype.Blength = function () {
        var arr = this.match(/[^\x00-\xff]/ig);
        return arr == null ? this.length : this.length + arr.length;
    }

    var errorPlacementfun = function (error, element) {
        if (element.is('input:text') &&
            element.prev() != null &&
            element.prev().is('select')) {
            error.appendTo(element.parent().parent().next());
        }
        else {
            error.appendTo(element.parent());
        }
    }

    verified.positiveInt = function (formid, elementid, msg) {
        msg = msg || message.english;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("positiveInt",
        function (value, element, arg) {
            return verified.isPositiveInt(value);
        }, message.positiveInt);
        //#endregion
        $('#' + elementid).rules('add', {
            positiveInt: true,
        })
    }

    verified.price = function (formid, elementid, msg) {
        msg = msg || message.price;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("priceFormate",
        function (value, element, arg) {
            return verified.isPrice(value);
        }, message.price);
        //#endregion
        $('#' + elementid).rules('add', {
            priceFormate: true,
        })
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
                number: message.number,
            }
        })
    }

    verified.english = function (formid, elementid, msg) {
        msg = msg || message.english;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("englishFormate",
        function (value, element, arg) {
            return verified.isEnglish(value);
        }, message.english);
        //#endregion
        $('#' + elementid).rules('add', {
            englishFormate: true,
        })
    }

    verified.englishUpper = function (formid, elementid, msg) {
        msg = msg || message.english;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("englishUpperFormate",
        function (value, element, arg) {
            return verified.isEnglishUpper(value);
        }, message.englishUpper);
        //#endregion
        $('#' + elementid).rules('add', {
            englishUpperFormate: true,
        })
    }

    verified.rate = function (formid, elementid, msg) {
        msg = msg || message.rate;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("rateFormat",
        function (value, element, arg) {
            return verified.isRate(value);
        }, message.rate);
        //#endregion
        $('#' + elementid).rules('add', {
            rateFormat: true,
        })
    }

    verified.time = function (formid, elementid, msg) {
        msg = msg || message.time;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })
        //#region 客製化驗證
        $.validator.addMethod("timeFormat",
        function (value, element, arg) {
            return verified.isTime(value);
        }, message.time);
        //#endregion
        $('#' + elementid).rules('add', {
            timeFormat: true,
        })
    }

    verified.englishNumber = function (formid, elementid, msg) {
        msg = msg || message.english;
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("englishNumberFormate",
        function (value, element, arg) {
            return verified.isEnglishNumber(value);
        }, message.englishNumber);
        //#endregion
        $('#' + elementid).rules('add', {
            englishNumberFormate: true,
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
        message = message || message.required('該欄位');
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

    verified.TWDate = function (formid, elementid, message) {
        message = message || '不符合民國日期格式(yyymmdd or yymmdd)';
        $("#" + formid).validate({
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("TWdateFormate",
        function (value, element, arg) {
            return verified.isTWDate(value);
        }, message);
        //#endregion
        $('#' + elementid).rules('add', {
            TWdateFormate: true,
        })
    }

    verified.datepicker = function (formid, datepickerid) {

        $("#" + formid).validate({
            //rules: {
            //    datepicker: { dateFormate: date }
            //},
            errorPlacement: function (error, element) {
                errorPlacementfun(error, element);
            }
        })

        //#region 客製化驗證

        $.validator.addMethod("dateFormate",
        function (value, element, arg) {
            return verified.isDate(value);
        }, message.date);
        //#endregion
        $('#' + datepickerid).rules('add', {
            dateFormate: true,
        })
    }

    created.createDatepicker = function (datepickerid, date, completeEvent) {
        var d = null;
        if (!(date === d)) {
            if (verified.isDate(date)) {
                d = verified.datepickerStrToDate(date);
            }
            else {
                d = created.getOnlyDate();
            }
        }

        $("#" + datepickerid).datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: 'yy/mm/dd',
            showOn: "both",
            buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
            onSelect: function (value) {
                if (verified.isDate(value)) {
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
                }
            },
            onClose: function (value) {
                if (verified.isDate(value)) {
                    if (typeof completeEvent == 'function')
                        completeEvent();
                }
                if (typeof completeEvent != "undefined" &&
                    typeof completeEvent.success == 'function' &&
                    typeof completeEvent.fail == 'function') {
                    if (verified.isDate(value)) {
                        completeEvent.success();
                    }
                    else {
                        completeEvent.fail();
                    }
                }
            }
        }).datepicker('setDate', d);

        if (typeof completeEvent != "undefined" &&
            typeof completeEvent.success == 'function') {
            completeEvent.success();
        }
    }

    //datepicker清除範圍限定
    created.clearDatepickerRangeValue = function (
        datepickerStartid, datepickerEndid) {
        $("#" + datepickerStartid).val('');
        $("#" + datepickerStartid).datepicker("option", "maxDate", null);
        $("#" + datepickerEndid).val('');
        $('#' + datepickerEndid).datepicker("option", "minDate", null);
    }

    //範圍datepicker
    created.createDatepickerRange = function (datepickerStartid,
        datepickerEndid, format, completerStartEvent, completerEndEvent) {
        format = format || 'yy/mm/dd';

        var from = $("#" + datepickerStartid)
                    .datepicker({
                        changeMonth: true,
                        changeYear: true,
                        dateFormat: format,
                        showOn: "both",
                        buttonText: '<i class="fa fa-calendar fa-2x toggle-btn"></i>',
                        onSelect: function (value) {
                            to.datepicker("option", "minDate", getDate(this));
                            if (verified.isDate(value)) {
                                $(this).parent().children().each(function () {
                                    if ($(this).is('label') && $(this).hasClass('error'))
                                        $(this).remove();
                                    if ($(this).is('input') && $(this).hasClass('error'))
                                        $(this).removeClass('error');
                                })
                            }
                        },
                        onClose: function (value) {
                            if (verified.isDate(value)) {
                                if (typeof completerStartEvent == 'function')
                                    completerStartEvent();
                            }
                            if (typeof completerStartEvent != "undefined" &&
                                typeof completerStartEvent.success == 'function' &&
                                typeof completerStartEvent.fail == 'function') {
                                if (verified.isDate(value)) {
                                    completerStartEvent.success();
                                }
                                else {
                                    completerStartEvent.fail();
                                }
                            }
                        }
                    });

        from.off('change');
        from.on('change', function () {
            if ($(this).val() == '')
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
                if (verified.isDate(value)) {
                    $(this).parent().children().each(function () {
                        if ($(this).is('label') && $(this).hasClass('error'))
                            $(this).remove();
                        if ($(this).is('input') && $(this).hasClass('error'))
                            $(this).removeClass('error');
                    })
                }
            },
            onClose: function (value) {
                if (verified.isDate(value)) {
                    if (typeof completerEndEvent == 'function')
                        completerEndEvent();
                }
                if (typeof completerEndEvent != "undefined" &&
                    typeof completerEndEvent.success == 'function' &&
                    typeof completerEndEvent.fail == 'function') {
                    if (verified.isDate(value)) {
                        completerEndEvent.success();
                    }
                    else {
                        completerEndEvent.fail();
                    }
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

    verified.isDate = function (value) {
        value = value || '';
        return dateFormat.test(value);
    }

    verified.isTWDate = function (value) {
        value = value || '';
        if (value == '')
            return true;
        if (verified.checkSpace(value))
            return false;
        var len = value.length;
        if (len == 7) {
            return dateFormat.test((Number(value.substr(0, 3)) + 1911) + '/' + value.substr(3, 2) + '/' + value.substr(5, 2));
        }
        else if (len == 6) {
            return dateFormat.test((Number(value.substr(0, 2)) + 1911) + '/' + value.substr(2, 2) + '/' + value.substr(4, 2));
        }
        return false;
    }

    created.dateToTWDate = function (value) {
        value = value || '';
        if (value == '')
            return '';
        if (verified.checkSpace(value))
            return '';
        var arr = value.split('/');
        if (arr.length == 3) {
            return (Number(arr[0]) + 1911) + '/' + created.padLeft(arr[1], 2, '0') + '/' + created.padLeft(arr[2], 2, '0');
        }
        return '';
    }

    verified.isPrice = function (value) {
        value = value || '';
        if (value == '')
            return true;
        return priceFormate.test(value);
    }

    verified.isEnglish = function (value) {
        value = value || '';
        return englishFormat.test(value);
    }

    verified.isEnglishUpper = function (value) {
        value = value || '';
        return englishUpperFormat.test(value);
    }

    verified.isRate = function (value) {
        value = value || '';
        return rateFormat.test(value);
    }

    verified.isTime = function (value) {
        value = value || '';
        if (value == '')
            return true;
        return timeFormat.test(value);
    }

    verified.isPositiveInt = function (value) {
        value = value || '';
        if (value == '')
            return true;
        return positiveInt.test(value);
    }

    verified.isEnglishNumber = function (value) {
        value = value || '';
        return englishNumberFormat.test(value);
    }

    verified.reportDate = function () {
        var d = created.getOnlyDate();
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

    //formate string(yyyy/MM/dd or yyyy-MM-dd) to date 失敗回傳 false
    verified.datepickerStrToDate = function (value) {
        if (dateFormat.test(value)) {
            if (value.split('/').length > 2) {
                var d = value.split('/');
                return new Date(d[0] + '-' + d[1] + '-' + d[2]);
            }
            if (value.split('-').length > 2) {
                var d = value.split('-');
                return new Date(d[0] + '-' + d[1] + '-' + d[2]);
            }
        }
        return false;
    }

    verified.dateToStr = function (value) {
        return value.getFullYear() + '/' + created.padLeft((value.getMonth() + 1), 2) + '/' + (value.getDate())
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

    //檢查有無空白
    verified.checkSpace = function checkSpace(s) {
        if (s.match(/\x20/i)) {
            return true;
        } else {
            return false;
        }
    }

    verified.changData = function (value) {
        value = value || '';
        if (value.trim() == '')
            return '(改為空值)';
        return value;
    }

    function verifiedReportDate(value) {
        if (dateFormat.test(value)) {
            var datepicker = verified.datepickerStrToDate(value);
            if (!datepicker) {
                return false;
            }
            if (datepicker.getDate() === 25)
                return true;
            var d = created.getOnlyDate();
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

    created.getOnlyDate = function getOnlyDate() {
        var d = new Date();
        d = new Date(d.getFullYear() + '-' + created.padLeft((d.getMonth() + 1), 2) + '-' + created.padLeft((d.getDate()), 2));
        return d;
    }

    created.getOnlyDateStr = function getOnlyDateStr(backSlashFlag, tawFlag, notLinkFlag) {
        backSlashFlag = backSlashFlag || false;
        tawFlag = tawFlag || false;
        notLinkFlag = notLinkFlag || false;
        var d = new Date();
        var r = '';
        r = tawFlag ? d.getFullYear() - 1911 : d.getFullYear();
        r += ((notLinkFlag ? '' : (backSlashFlag ? '/' : '-')) + created.padLeft((d.getMonth() + 1), 2) + (notLinkFlag ? '' : (backSlashFlag ? '/' : '-')) + created.padLeft((d.getDate()), 2));
        return r;
    }

    created.uuid = function _uuid() {
        var d = Date.now();
        if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
            d += performance.now(); //use high-precision timer if available
        }
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    };

    created.padLeft = function padLeft(str, lenght, padStr) {
        str = (str || '') + '';
        if (typeof lenght != 'number')
            return str;
        padStr = padStr || '0';
        if (str.length >= lenght)
            return str;
        else
            return padLeft(padStr + str, lenght, padStr);
    }
    created.padRight = function padRight(str, lenght, padStr) {
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