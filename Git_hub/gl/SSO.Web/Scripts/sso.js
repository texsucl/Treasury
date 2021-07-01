
//轉大寫
function InputToUpper(obj) {
    if (obj.value != "") {
        obj.value = obj.value.toUpperCase();
    }
}


//檢核字串是否為大寫英文    add by daiyu 20180130
function checkBEng(str) {
    var regExp = /^[A-Z]+$/;

    if (regExp.test(str))
        return true;
    else
        return false;
};


//年月預設值(民國年)
function getChtYYYYMM() {
    var date = new Date();
    var currentYear = date.getFullYear() - 1911;
    var currentMonth = date.getMonth() + 1;

    return leftPad(String(currentYear), 4, '0') +
        leftPad(String(currentMonth), 2, '0')
}

//年月日預設值(西元年)
function getCurDate() {
    var date = new Date();
    var currentYear = date.getFullYear();
    var currentMonth = date.getMonth() + 1;
    var currentDay = date.getDate();

    return leftPad(String(currentYear), 4, '0') +
        leftPad(String(currentMonth), 2, '0') +
        leftPad(String(currentDay), 2, '0')
}


//年度預設值
function getChtYYYY() {
    var date = new Date();
    var currentYear = date.getFullYear() - 1911;

    return leftPad(String(currentYear), 4, '0') 
}

//月份預設值
function getMM() {
    var date = new Date();
    var currentMonth = date.getMonth() + 1;

    return leftPad(String(currentMonth), 2, '0')
}


//左補字串
function leftPad(val, length, letter) {
    var str = '' + val;
    while (str.length < length) {
        str = letter + str;
    }
    return str;
}

//西元日期檢核
function checkDate(str, require) {
    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (!/^(\d){8}$/.test(str)) return false;
    var y = str.substr(0, 4),
        m = parseInt(str.substr(4, 2)),
        d = parseInt(str.substr(6, 2));

    //var currentYear = date.getFullYear();
    //var currentMonth = date.getMonth() + 1;


    var testDate = new Date(y, m - 1, d);

    if ((testDate.getMonth() + 1) == m && testDate.getDate() == d) {
        return true;
    } else {
        return false;
    }

}


////西元年月檢核
function checkYearMonth(str, require) {

    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 6)
        return false;

    if (!checkYear(str.substr(0, 4), require))
        return false;


    if (!checkMonth(str.substr(4, 2), require))
        return false;


    return true;

}



////民國年月檢核
function checkTwYearMonth(str, require) {

    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 6)
        return false;

    if (!checkTwYear(str.substr(0,4), require))
        return false;


    if (!checkMonth(str.substr(4, 2), require)) 
        return false;

        
    return true;

}

//西元年4碼檢核
//str:要檢查的字串
//require:是否為必輸
function checkYear(str, require) {

    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 4)
        return false;

    return checkNum(str);

}


//民國年4碼檢核
//str:要檢查的字串
//require:是否為必輸
function checkTwYear(str, require) {

    if (!str) {
        if (require)
            return false;
        else
            return true;
    }
    
    if (str.length != 4)
        return false;

    return checkNum(str);

}

//月份檢核
function checkMonth(str, require) {

    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 2)
        return false;

    var iYear = parseFloat(str)

    if (checkNum(str))
        if (iYear <= 0 || iYear > 12)
            return false;
        else
            return true;
    else
        return false;
}



//半型轉全型
function halfToFull(str) {
    var temp = "";
    for (var i = 0; i < str.length; i++) {
        var charCode = str.charCodeAt(i);
        if (charCode <= 126 && charCode >= 33) {
            charCode += 65248;
        } else if (charCode == 32) { // 半形空白轉全形
            charCode = 12288;
        }
        temp = temp + String.fromCharCode(charCode);
    }
    return temp;

};


//批號檢核
function checkBatchNo(str, require) {
    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 14)
        return false;

    return checkLetter(str);
}


//傳票號碼檢核
function checkTicketNo(str, require) {
    if (!str) {
        if (require)
            return false;
        else
            return true;
    }

    if (str.length != 10)
        return false;

    return checkLetter(str);
}


//身份證字號檢核
function checkID(idStr, require) {

    if (!idStr) {
        if (require)
            return false;
        else
            return true;
    }


    // 依照字母的編號排列，存入陣列備用。
    var letters = new Array('A', 'B', 'C', 'D',
        'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M',
        'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
        'X', 'Y', 'W', 'Z', 'I', 'O');
    // 儲存各個乘數
    var multiply = new Array(1, 9, 8, 7, 6, 5,
                             4, 3, 2, 1);
    var nums = new Array(2);
    var firstChar;
    var firstNum;
    var lastNum;
    var total = 0;
    // 撰寫「正規表達式」。第一個字為英文字母，
    // 第二個字為1或2，後面跟著8個數字，不分大小寫。
    var regExpID = /^[a-z](1|2)\d{8}$/i;
    // 使用「正規表達式」檢驗格式
    if (idStr.search(regExpID) == -1) {
        return false;
    } else {
        // 取出第一個字元和最後一個數字。
        firstChar = idStr.charAt(0).toUpperCase();
        lastNum = idStr.charAt(9);
    }
    // 找出第一個字母對應的數字，並轉換成兩位數數字。
    for (var i = 0; i < 26; i++) {
        if (firstChar == letters[i]) {
            firstNum = i + 10;
            nums[0] = Math.floor(firstNum / 10);
            nums[1] = firstNum - (nums[0] * 10);
            break;
        }
    }
    // 執行加總計算
    for (var i = 0; i < multiply.length; i++) {
        if (i < 2) {
            total += nums[i] * multiply[i];
        } else {
            total += parseInt(idStr.charAt(i - 1)) *
                     multiply[i];
        }
    }
    // 和最後一個數字比對
    if (((10 - (total % 10)) % 10) != lastNum) {
        return false;
    }
    return true;
};



//檢核字串是否為英數字
function checkLetter(str) {
    var regExp = /^[\d|a-zA-Z]+$/;

    if (regExp.test(str))
        return true;
    else
        return false;
};


//檢核字串是否為數字
function checkNum(str) {
    var regExp = /^[-?0-9]*$/;

    if (regExp.test(str))
        return true;
    else
        return false;
};


//檢核字串是否為數字(正整數)
function checkNumP(str) {
    var regExp = /^[0-9]*$/;

    if (regExp.test(str))
        return true;
    else
        return false;
};




//處理凍結grid時...凍結行的高度與原grid行不一致問題
resizeColumnHeader = function () {
    var rowHight, resizeSpanHeight,
        // get the header row which contains
        headerRow = $(this).closest("div.ui-jqgrid-view")
            .find("table.ui-jqgrid-htable>thead>tr.ui-jqgrid-labels");

    // reset column height
    headerRow.find("span.ui-jqgrid-resize").each(function () {
        this.style.height = '';
    });

    // increase the height of the resizing span
    resizeSpanHeight = 'height: ' + headerRow.height() + 'px !important; cursor: col-resize;';
    headerRow.find("span.ui-jqgrid-resize").each(function () {
        this.style.cssText = resizeSpanHeight;
    });

    // set position of the dive with the column header text to the middle
    rowHight = headerRow.height();
    headerRow.find("div.ui-jqgrid-sortable").each(function () {
        var ts = $(this);
        ts.css('top', (rowHight - ts.outerHeight()) / 2 + 'px');
    });
},
       fixPositionsOfFrozenDivs = function () {
           var $rows;
           if (typeof this.grid.fbDiv !== "undefined") {
               $rows = $('>div>table.ui-jqgrid-btable>tbody>tr', this.grid.bDiv);
               $('>table.ui-jqgrid-btable>tbody>tr', this.grid.fbDiv).each(function (i) {
                   var rowHight = $($rows[i]).height(), rowHightFrozen = $(this).height();
                   if ($(this).hasClass("jqgrow")) {
                       $(this).height(rowHight);
                       rowHightFrozen = $(this).height();
                       if (rowHight !== rowHightFrozen) {
                           $(this).height(rowHight + (rowHight - rowHightFrozen));
                       }
                   }
               });
               $(this.grid.fbDiv).height(this.grid.bDiv.clientHeight);
               $(this.grid.fbDiv).css($(this.grid.bDiv).position());
           }
           if (typeof this.grid.fhDiv !== "undefined") {
               $rows = $('>div>table.ui-jqgrid-htable>thead>tr', this.grid.hDiv);
               $('>table.ui-jqgrid-htable>thead>tr', this.grid.fhDiv).each(function (i) {
                   var rowHight = $($rows[i]).height(), rowHightFrozen = $(this).height();
                   $(this).height(rowHight);
                   rowHightFrozen = $(this).height();
                   if (rowHight !== rowHightFrozen) {
                       $(this).height(rowHight + (rowHight - rowHightFrozen));
                   }
               });
               $(this.grid.fhDiv).height(this.grid.hDiv.clientHeight);
               $(this.grid.fhDiv).css($(this.grid.hDiv).position());
           }
       },
       fixGboxHeight = function () {
           var gviewHeight = $("#gview_" + $.jgrid.jqID(this.id)).outerHeight(),
               pagerHeight = $(this.p.pager).outerHeight();

           $("#gbox_" + $.jgrid.jqID(this.id)).height(gviewHeight + pagerHeight);
           gviewHeight = $("#gview_" + $.jgrid.jqID(this.id)).outerHeight();
           pagerHeight = $(this.p.pager).outerHeight();
           $("#gbox_" + $.jgrid.jqID(this.id)).height(gviewHeight + pagerHeight);
       };








