(function (window, undefind) {
    var message = {};

    window.message = message;
    message.number = '請輸入數字';
    message.price = '僅可輸入金額';
    message.positiveInt = '僅可輸入正整數';
    message.english = '僅可輸入英文字母';
    message.englishUpper = '僅可輸入英文大寫字母';
    message.rate = '不符合利率格式(99.9999)';
    message.englishNumber = '僅可輸入英文字母和數字';
    message.date = "不符合日期格式 (yyyy/mm/dd)";
    message.time = "不符合時間格式 (HH:MM)";
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