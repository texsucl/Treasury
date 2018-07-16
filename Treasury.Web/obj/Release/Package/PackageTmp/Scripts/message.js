﻿(function (window, undefind) {
    var message = {};

    window.message = message;
    message.number = '請輸入數字';
    message.positiveInt = '僅可輸入正整數';
    message.english = '僅可輸入英文字母';
    message.date = "不符合日期格式 (yyyy/mm/dd)";
    message.maxlength = function (value) {
        return "不能超過" + value + "字元!";
    }
    message.minlength = function (value) {
        return "不能少於" + value + "字元!";
    }
    message.required = function (value) {
        return value + "為必填!";
    }
})(window);