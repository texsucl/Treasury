//console polyfill
(function (con) {
    'use strict';
    var prop, method;
    var empty = {};
    var dummy = function () { };
    var properties = 'memory'.split(',');
    var methods = ('assert,clear,count,debug,dir,dirxml,error,exception,group,' +
       'groupCollapsed,groupEnd,info,log,markTimeline,profile,profiles,profileEnd,' +
       'show,table,time,timeEnd,timeline,timelineEnd,timeStamp,trace,warn').split(',');
    while (prop = properties.pop()) con[prop] = con[prop] || empty;
    while (method = methods.pop()) con[method] = con[method] || dummy;
})(this.console = this.console || {}); // Using `this` for web workers.

/*
* base
* 2016.06.22 ben tsao new
* fb
*
*/
(function () {
    'use strict';

    var root = this,
        fb = root.fb = { cultures: {} },
        FUNCTION = "function",
        STRING = "string";

    //references to array methods.
    fb.slice = [].slice;
    fb.log = function (msg) {
        if (console && console.log)
            console.log(msg);
    };

    // Events
    // ---------------
    var preventDefault = function () {
        this._defaultPrevented = true;
    };

    var isDefaultPrevented = function () {
        return this._defaultPrevented === true;
    };

    fb.Events = {
        on: function (eventName, callbacks, one) {
            var that = this,
                idx,
                eventNames = typeof eventName === STRING ? [eventName] : eventName,
                length,
                original,
                callback,
                callbacksIsFunction = typeof callbacks === FUNCTION,
                events;

            that._events = that._events || {};

            if (callbacks === undefined) {
                for (idx in eventName) {
                    that.on(idx, eventName[idx]);
                }
                return that;
            }

            for (idx = 0, length = eventNames.length; idx < length; idx++) {
                eventName = eventNames[idx];
                callback = callbacksIsFunction ? callbacks : callbacks[eventName];
                if (callback) {
                    if (one) {
                        original = callback;
                        callback = function () {
                            that.off(eventName, callback);
                            original.apply(that, arguments);
                        };
                        callback.original = original;
                    }
                    events = that._events[eventName] = that._events[eventName] || [];
                    events.push(callback);
                }
            }

            return that;
        },

        off: function (eventName, callback) {
            var that = this,
                events = that._events[eventName],
                idx;

            if (eventName === undefined) {
                that._events = {};
            } else if (events) {
                if (callback) {
                    for (idx = events.length - 1; idx >= 0; idx--) {
                        if (events[idx] === callback || events[idx].original === callback) {
                            events.splice(idx, 1);
                        }
                    }
                } else {
                    that._events[eventName] = [];
                }
            }

            return that;
        },

        trigger: function (eventName, options) {
            var that = this,
                events = that._events ? that._events[eventName] : undefined,
                idx,
                length,
                e = { sender: that };

            if (events) {
                e._defaultPrevented = false;
                e.preventDefault = preventDefault;
                e.isDefaultPrevented = isDefaultPrevented;
                events = events.slice();
                for (idx = 0, length = events.length; idx < length; idx++) {
                    if (events[idx].call(that, e, options) === false) {
                        e.preventDefault();
                    }
                    if (e._defaultPrevented === true) return false;
                }

                //maybe trigger 'all' event

                return e._defaultPrevented === false;
            }

            if (fb && fb.isDebug) {
                fb.log('debug - trigger : ' + eventName);
                fb.log(e);
                fb.log(options);
            }

            return false;
        }
    };

    $.extend(fb, fb.Events);

    fb.mask = function (label, delay) {
        $(document.body).each(function () {
            if (delay !== undefined && delay > 0) {
                var element = $(this);
                element.data("_mask_timeout", setTimeout(function () { fb.maskElement(element, label) }, delay));
            } else {
                fb.maskElement($(this), label);
            }
        });
    };

    fb.unmask = function () {
        $(document.body).each(function () {
            fb.unmaskElement($(this));
        });
    };

    fb.isMasked = function (element) {
        return element.hasClass("masked");
    };

    fb.maskElement = function (element, label) {
        label = label === undefined ? locale.waiting : label;

        //if this element has delayed mask scheduled then remove it and display the new one
        if (element.data("_mask_timeout") !== undefined) {
            clearTimeout(element.data("_mask_timeout"));
            element.removeData("_mask_timeout");
        }

        if (fb.isMasked(element)) {
            fb.unmaskElement(element);
        }

        if (element.css("position") == "static") {
            element.addClass("masked-relative");
        }

        element.addClass("masked");

        var maskDiv = $('<div class="loadmask"></div>');

        //auto height fix for IE
        if (navigator.userAgent.toLowerCase().indexOf("msie") > -1) {
            maskDiv.height(element.height() + parseInt(element.css("padding-top")) + parseInt(element.css("padding-bottom")));
            maskDiv.width(element.width() + parseInt(element.css("padding-left")) + parseInt(element.css("padding-right")));
        }

        //fix for z-index bug with selects in IE6
        if (navigator.userAgent.toLowerCase().indexOf("msie 6") > -1) {
            element.find("select").addClass("masked-hidden");
        }

        element.append(maskDiv);

        if (label !== undefined) {
            var maskMsgDiv = $('<div class="loadmask-msg" style="display:none;"></div>');
            maskMsgDiv.append('<div><img src="' + fb.url("Images/loading.gif") + '" alt="" height="20" width="20" />' + label + '</div>');
            element.append(maskMsgDiv);

            //calculate center position
            maskMsgDiv.css("top", Math.round(element.height() / 2 - (maskMsgDiv.height() - parseInt(maskMsgDiv.css("padding-top")) - parseInt(maskMsgDiv.css("padding-bottom"))) / 2) + "px");
            maskMsgDiv.css("left", Math.round(element.width() / 2 - (maskMsgDiv.width() - parseInt(maskMsgDiv.css("padding-left")) - parseInt(maskMsgDiv.css("padding-right"))) / 2) + "px");

            maskMsgDiv.show();
        }

    };

    fb.unmaskElement = function (element) {
        //if this element has delayed mask scheduled then remove it
        if (element.data("_mask_timeout") !== undefined) {
            clearTimeout(element.data("_mask_timeout"));
            element.removeData("_mask_timeout");
        }

        element.find(".loadmask-msg,.loadmask").remove();
        element.removeClass("masked");
        element.removeClass("masked-relative");
        element.find("select").removeClass("masked-hidden");
    };

    fb.msg = function (msg, callback, options) {
        var baseOption = {
                title: locale.alertTitle,
                message: msg,
                type: BootstrapDialog.TYPE_INFO,
                closable: true,
                draggable: true,
                buttonLabel: locale.close,
                buttonClass: 'btn-info rounded',
                callback: callback
            },
            mOptions = $.extend(true, {}, baseOption, options);
        BootstrapDialog.alert(mOptions);
    };

    fb.msgError = function (msg, callback, options) {
        var baseOption = {
                title: locale.errorTitle,
                message: msg,
                type: BootstrapDialog.TYPE_DANGER,
                closable: true,
                draggable: true,
                buttonLabel: locale.close,
                buttonClass: 'btn-danger rounded',
                callback: callback
            },
            mOptions = $.extend(true, {}, baseOption, options);
        BootstrapDialog.alert(mOptions);
    };

    fb.msgConfirm = function (msg, callback, options) {
        var baseOption = {
                title: locale.confirmTitle,
                message: msg,
                type: BootstrapDialog.TYPE_PRIMARY,
                closable: true,
                draggable: true,
                btnCancelLabel: locale.cancel,
				btnCancelClass: "btn-danger rounded",
                btnOKLabel: locale.confirmOK,
                btnOKClass: 'btn-success rounded',
                callback: callback
            },
            mOptions = $.extend(true, {}, baseOption, options);
        BootstrapDialog.confirm(mOptions);
    };

    fb.url = function (url) {
        return baseUrl + url;
    };

    fb.noCopyPaste = function () {
        $('.nopaste').bind("cut copy paste", function (e) {
            e.preventDefault();
        });
    };

    fb.convertNETDateTime = function(sNetDate) {
        if (sNetDate == null) return null;
        if (sNetDate instanceof Date) return sNetDate;
        var r = /\/Date\(([0-9]+)\)\//i
        var matches = sNetDate.match(r);
        if (matches.length == 2) {
            return new Date(parseInt(matches[1]));
        } else {
            return sNetDate;
        }
    };
}).call(this);


/*
* lib
* 2016.07.12 ben tsao new
* common check
*
*/
(function () {
	'use strict';

	fb.checkID = function (idStr) {
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

	fb.checkIDOut = function (idStr) {
		// 依照字母的編號排列，存入陣列備用。
		var letters = new Array('A', 'B', 'C', 'D',
			'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M',
			'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
			'X', 'Y', 'W', 'Z', 'I', 'O');
		// 儲存各個乘數
		var multiply = new Array(1, 9, 8, 7, 6, 5,
								 4, 3, 2, 1);
		var nums = new Array(3);
		var firstChar;
		var secondChar;
		var firstNum;
		var lastNum;
		var total = 0;
		// 撰寫「正規表達式」。第一、二個字為英文字母，
		var regExpID = /^[a-z][a-z]\d{8}$/i;

		//使用「正規表達式」檢驗格式
		if (idStr.search(regExpID) == -1) {
			return false;
		} else {
			// 取出第一個字元和最後一個數字。
			firstChar = idStr.charAt(0).toUpperCase();
			secondChar = idStr.charAt(1).toUpperCase();
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
		// 找出第二個字母對應的數字，並取出個位數字。
		for (var i = 0; i < 26; i++) {
			if (secondChar == letters[i]) {
				firstNum = i + 10;
				nums[2] = firstNum % 10;
				break;
			}
		}
		// 執行加總計算
		for (var i = 0; i < multiply.length; i++) {
			if (i < 3) {
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

	fb.checkIDIsOut = function (idStr) {
		var regExpID = /^[a-z][a-z]\d{8}$/i;
		if (idStr.search(regExpID) == -1) {
			return false;
		} else {
			return true;
		}
	};

	fb.checkEmailIsFubon = function (email) {
		var sEmail = email.toLowerCase();
		if (sEmail.indexOf("@fubon.com") >= 0 || sEmail.indexOf("@life.fubon.com") >= 0) {
			return true;
		}
		return false;
	};

	fb.checkEmail = function (email) {
		if (fb.isNullOrUndefined(email)) return false;
		if (email.length < 1 || email.length > 100) return false;

		var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/igm;
		return re.test(email);
	};
	
	fb.checkTelM = function (tel) {
		if (fb.isNullOrUndefined(tel)) return false;
		if (tel.length != 10) return false;

		var re = /^\d{10}$/igm;
		return re.test(tel);
	};
	
	fb.checkSex = function (idStr, gender) {
		if(fb.checkID(idStr) || fb.checkIDOut(idStr)){
			var secondChar = idStr.charAt(1).toUpperCase();
			if ((secondChar === "1" || secondChar === "A" || secondChar === "C") && gender === "2") {
				return true;
			} else if ((secondChar === "2" || secondChar === "B" || secondChar === "D") && gender === "1") {
				return true;
			}
			return false;
		} else {
			return false;
		}
	};
	fb.checkIdentity = function (idStr, identity) {
		if(fb.checkID(idStr) || fb.checkIDOut(idStr)){
			var secondChar = idStr.charAt(1).toUpperCase();
			if ((secondChar === "1" || secondChar === "2") && identity === "2") {
				return true;
			} else if ((secondChar === "A" || secondChar === "B" || secondChar === "C" || secondChar === "D") && identity === "1") {
				return true;
			}
			return false;
		} else {
			return false;
		}
	};
}).call(this);
/*
 * combobox
 * 2017.06.21 ben tsao new
 * common combobox
 *
 */
(function() {
    'use strict';
    
	$.fn.combobox = function (callback) {
		var combolist = [];
		var combolist2 = [];

		$(this).focus(function () {
			$(this).val('');
			$(this).parent().find("ul").slideDown();
		});
		//shows dropdown when element focuses

		$(this).focusout(function () {
			$(this).parent().find("ul").slideUp();
		});
		//hides element when out of focus

		$("ul li", $(this).parent()).each(function () {
			$(this).click(function () {
				$(this).parent()
					.parent()
					.find("input")
				.val($(this).text())
				.attr("inval", $(this).attr('value'))
				.attr("idval", $(this).attr('id'))
				.prop("isinput", false)
				.css("background","#fff");
				callback();
			}); //value of li is set as the text value
			combolist.push($(this).text());
			//pushes values of li to an array  
			combolist2.push($(this).text().toLowerCase());
			/*pushes values of li to an array in lowercase -
		  added just in case the user does not want the values to be case sensetive  */
		}); //each li

		$("input").keyup(function () {
			var val = $(this).val().trim();
			if ($.inArray(val, combolist) > -1 || $.inArray(val, combolist2) > -1) {
			} else {
				if(val !== "請選擇" && val !== "") {
					$(this).attr("inval", val);
					$(this).prop("isinput", true);
				}
			}
		});
	};
}).call(this);

/*
* core
* 2016.07.18 Ben new
* Class
*
*/
(function () {
    'use strict';

    var STRING = "string",
        FUNCTION = "function";

    //modal
    var zindex = {
        _base: 10000,

        get: function () {
            return ++this._base;
        }
    }
    fb.zindex = zindex;

    // Class
    // ---------------
    function Class() { };

    Class.extend = function (proto) {
        var base = function () { },
            member,
            that = this,
            subclass = proto && proto.init ? proto.init : function () {
                that.apply(this, arguments);
            },
            fn;

        base.prototype = that.prototype;
        fn = subclass.fn = subclass.prototype = new base();

        for (member in proto) {
            if (proto[member] != null && proto[member].constructor === Object) {
                // Merge object members
                fn[member] = $.extend(true, {}, base.prototype[member], proto[member]);
            } else {
                fn[member] = proto[member];
            }
        }

        fn.constructor = subclass;
        subclass.extend = that.extend;

        return subclass;
    };

    fb.Class = Class;
    // Add Events To Class
    // on, off, first, trigger
    $.extend(fb.Class.prototype, fb.Events);

    // Add ww core function and object
    // ---------------
    fb.stringify = $.proxy(JSON.stringify, JSON);
}).call(this);

/*
* http
* 2016.06.30 Ben new 
* 
* require : 
*
*/
(function () {
	'use strict';

	var log = fb.log;

	var getError = function (data) {
		var errors = [];
		if (data && data.Errors){
			errors = data.Errors;
		} else if ($.isArray(data)) {
			errors = data;
		}

		return errors.join('\r\n');
	};

	var baseOption = {
		type: "POST",
		dataType: "json",
		contentType: "application/json; charset=utf-8",
		cache: false,
		doMask: false,
		headers: { "cache-control": "no-cache" }
	};

	var ajax = function (options) {

		options = $.extend(true, {}, baseOption, options);

		var whenDone = options.whenDone,
			whenFail = options.whenFail;


		if (options.type === 'GET') {
			options.data = $.param(options.data, true);
		}

		if (options.doMask) fb.mask();
		return $.ajax(options).done(function (data, textStatus, jqXHR) {
			var errors = getError(data);

			if (errors.length > 0) {
				fb.msgError(errors);
				fb.unmask();
				if (whenFail) whenFail(errors, data);
			} else {
				if (whenDone) whenDone(data, textStatus, jqXHR);
			}
		}).fail(function (jqXHR, textStatus, errorThrown) {
		    var errors = ["系統發生錯誤"];
			if (jqXHR && jqXHR.responseText && jqXHR.responseText.length) {
				try {
					var json = JSON.parse(jqXHR.responseText);
						errors = getError(json);
				}
				catch (exception) {
					//errors = [textStatus + errorThrown];
					window.location.reload();
				}
			}
			else if (textStatus && errorThrown)
				errors = [textStatus + errorThrown];
			else if (textStatus)
				errors = [textStatus];
			else if (errorThrown)
				errors = [errorThrown];
			
			if (jqXHR && jqXHR.status === 0) {
			    return; //Skip this error
			} else if(jqXHR && jqXHR.status === 440){
				window.location.href = window.location.href;
			} else {
				fb.msgError(errors);
				fb.unmask();
				if (whenFail) whenFail(errors);
			}
		}).always(function () {
			if (options.doMask) fb.unmask();
		});
	};

	//options :
	//  whenDone: function ()
	//  whenFail: function ()
	//  doMask
	fb.http = {
		get: function (options) {
			var getOption = {
				type: "GET"
			};
			options = $.extend(true, {}, getOption, options);
			if (!_.isString(options.data))
				options.data = JSON.stringify(options.data);

			options.data = JSON.parse(options.data);

			return ajax(options);
		},

		post: function (options) {
			if(_.isString(options.data)){
				options.data = JSON.parse(options.data);
			}

			var $token = $("body input[name=__RequestVerificationToken]:first-child");
			if ($token.length == 0) {
				$token = $("input[name=__RequestVerificationToken]:first");
			}
			var token = $token.length > 0 ? $token.val() : "";
			options.headers = {};
			options.headers.__RequestVerificationToken = token;
			options.data = JSON.stringify(options.data);
			return ajax(options);
		}
	};

}).call(this);
/*
 * lib
 * 2016.06.22 ben tsao new
 * common lib
 *
 */
(function() {
    'use strict';

    fb.varConst = {};
    fb.ns = 'fb';
    fb.attr = function(value) {
        return "data-" + fb.ns + "-" + value;
    };

    fb.isNullOrUndefined = function(text) {
        if (text === null || text === undefined || text === "") return true;
        return false;
    };

    fb.padLeft = function(str, len, itemChar) {
        itemChar = itemChar || "0";
        return (str + '').length >= len ? str : (new Array(len - String(str).length + 1)).join(String(itemChar)) + str;
    };

    fb.padRight = function(str, len, itemChar) {
        itemChar = itemChar || "0";
        return (str + '').length >= len ? str : str + (new Array(len - String(str).length + 1)).join(String(itemChar));
    };

    var idCounter = 0;
    fb.uniqueId = function(prefix, len) {
        var id = ++idCounter;
        prefix = String(prefix == null ? '' : prefix);
        return len ? prefix + fb.padLeft(id, len) : prefix + id;
    }

    var ampRegExp = /&/g,
        ltRegExp = /</g,
        quoteRegExp = /"/g,
        aposRegExp = /'/g,
        gtRegExp = />/g;
    fb.htmlEncode = function(value) {
        return ("" + value).replace(ampRegExp, "&amp;")
            .replace(ltRegExp, "&lt;")
            .replace(gtRegExp, "&gt;")
            .replace(quoteRegExp, "&quot;")
            .replace(aposRegExp, "&#39;");
    };

    fb.round = function(value, precision) {
        precision = precision || 0;

        value = value.toString().split('e');
        value = Math.round(+(value[0] + 'e' + (value[1] ? (+value[1] + precision) : precision)));

        value = value.toString().split('e');
        value = +(value[0] + 'e' + (value[1] ? (+value[1] - precision) : -precision));

        return value.toFixed(precision);
    };

    fb.math = {
        _lengthOfDecimal: function(num) {
            var len;
            try {
                len = num.toString().split('.')[1].length;
            } catch (e) {
                len = 0;
            }
            return len;
        },

        add: function() {
            var args = Array.prototype.slice.call(arguments) || [],
                result = 0;

            if (args.length === 0) {
                return 0;
            } else if (args.length === 1) {
                return args[0];
            }

            for (var i = 0; i < args.length; i++) {
                var decLen1 = this._lengthOfDecimal(result),
                    decLen2 = this._lengthOfDecimal(args[i]),
                    base = Math.pow(10, Math.max(decLen1, decLen2)),
                    fixedNum = decLen1 > decLen2 ? decLen1 : decLen2;
                result = ((result * base + args[i] * base) / base).toFixed(fixedNum);
            }

            return +result;
        },

        subtract: function(arg1, arg2) {
            return this.add(arg1, -arg2);
        },

        multiply: function() {
            var args = Array.prototype.slice.call(arguments) || [],
                result = 1;

            if (args.length === 0) {
                return 0;
            } else if (args.length === 1) {
                return args[0];
            }

            for (var i = 0; i < args.length; i++) {
                var num = args[i],
                    base = this._lengthOfDecimal(result) + this._lengthOfDecimal(num);

                result = +(result.toString().replace('.', '')),
                    num = +(num.toString().replace('.', ''));
                base = Math.pow(10, base);
                result = result * num / base;
            }

            return result;
        },

        divide: function(arg1, arg2) {
            if (!arg1 || !arg2) {
                return 0;
            }
            var decLen1 = this._lengthOfDecimal(arg1),
                decLen2 = this._lengthOfDecimal(arg2),
                num1 = arg1.toString().replace('.', ''),
                num2 = arg2.toString().replace('.', '');
            return (num1 / num2) * Math.pow(10, decLen2 - decLen1);
        },

        round: fb.round
    };

    fb.promise = function(func) {
        var deferred = function() {
            return $.Deferred(func).promise();
        };
        return $.when(deferred());
    };

    fb.wait = function(check, timeout, checkInterval) {
        var dfd = jQuery.Deferred();

        var checkHandle = setInterval(function() {
            if (!check || check()) {
                clearInterval(checkHandle);
                dfd.resolve();
            }
        }, checkInterval || 50);

        var timeoutHandle = setTimeout(function() {
            if (dfd.state() == "pending") {
                clearInterval(checkHandle);
                clearTimeout(timeoutHandle);
                dfd.reject();
            }
        }, timeout || 5000);

        return dfd.promise();
    };
    var notInAjax = function() {
        return !$.active;
    };
    fb.waitAjax = function(callback, timeout, checkInterval) {
        timeout = timeout || 10000;
        //must always wait >=50
        return callback ? fb.wait(notInAjax, timeout, checkInterval).done(callback) : fb.wait(notInAjax, timeout, checkInterval);
    };

    fb.isEndsWith = function(str, suffix) {
        return str.indexOf(suffix, str.length - suffix.length) !== -1;
    };

    fb.resetForm = function() {
        $('input, textarea').not(':input[type=button], :input[type=submit], :input[type=reset], :input:disabled, input[readonly]').val('');
        $(".field-validation-error").empty();
    };

    fb.getAge = function(date) {
        if (!fb.isDate(date)) return 0;
        if (date.length == 9 || date.length == 8 || date.length == 7) date = fb.DateFromTwToEn(date);
        var birthday = new Date(date);
        var today = new Date();
        var years = today.getFullYear() - birthday.getFullYear();

        // Reset birthday to the current year.
        birthday.setFullYear(today.getFullYear());

        // If the user's birthday has not occurred yet this year, subtract 1.
        if (today < birthday) {
            years--;
        }
        return years;
    };

    fb.isDate = function(text) {
        if (fb.isNullOrUndefined(text)) return false;
        if (text.length == 9 || text.length == 8 || text.length == 7) text = fb.DateFromTwToEn(text);
        if (text.length != 10) return false;

        var comp = text.split('/'),
            y = parseInt(comp[0], 10),
            m = parseInt(comp[1], 10),
            d = parseInt(comp[2], 10),
            date = new Date(y, m - 1, d);

        if (date.getFullYear() == y && date.getMonth() + 1 == m && date.getDate() == d) {
            return true;
        } else {
            return false;
        }
    };

    fb.DateFromTwToEn = function(text) {
        if (fb.isNullOrUndefined(text) || (text.length != 9 && text.length != 8 && text.length != 7)) return text;
        var dateArr = text.split("/"),
            year = parseInt(dateArr[0]) + 1911;
        return year + "/" + dateArr[1] + "/" + dateArr[2];
    };

    fb.DateFromEnToTw = function(text) {
        if (fb.isNullOrUndefined(text) || text.length != 10) return text;
        var dateArr = text.split("/"),
            year = parseInt(dateArr[0]) - 1911;
        return year + "/" + dateArr[1] + "/" + dateArr[2];
    };

    fb.ADtoChina = function(yyyy, mm, dd) {
        yyyy = yyyy - 1911;
        return yyyy + "年" + mm + "月" + dd + "日";
    };

    fb.SetSelectDefaultOption = function(selector) {
        $('#' + selector + ' option').filter(function() {
            return $(this).text() == locale.PleaseSelect;
        }).attr('selected', true);
    };

    fb.GetMaskName = function(text) {
        var result = text;
        if (fb.isNullOrUndefined(text)) return result;
        var tLen = text.length;
        if (tLen === 1) {
            result = "O";
        } else if (tLen === 2) {
            result = text.substr(0, 1) + "O";
        } else if (tLen > 2) {
            result = text.substr(0, 1);
            for (var i = 1; i<=(tLen-2);++i){
                result += "O";
            }
            result += text.substr(tLen - 1);
        }
        return result;
    };

    fb.GetMaskUserID = function(text) {
        if (fb.isNullOrUndefined(text) || text.trim().length !== 10) return text;
        return fb.ReplaceBetween(text, 2, 7, 'OOOOO');
    };

    fb.ReplaceBetween = function(text, start, end, what) {
        return text.substring(0, start) + what + text.substring(end);
    }

    fb.DeviceType = function() {
        if(fb.isNullOrUndefined(navigator)){
            return "web";
        } else {
            if(fb.isNullOrUndefined(navigator.userAgent)) {
                return "web";
            } else if (/Android/i.test(navigator.userAgent)) {
                return "android";
            }
            else if (/iPhone|iPad|iPod/i.test(navigator.userAgent)) {
                return "ios";
            }
            else
                return "web";
        }
    }

    fb.RedirectUrl = function(url) {
        var deviceType = fb.DeviceType();
        if(deviceType === "android") {
            document.location = url;
        } else {
            window.location = url;
        }
    }
}).call(this);


(function (window, undefind) {
    if ($('.menu-list') != null) {

        $('ul.main-menu li a').each(function () {
            if ($($(this))[0].href == String(window.location))
                $(this).parent().addClass('active');
        });


        //$('.menu-list ul.sub-menu li,.HomeMenu').click(function () {
        //    $(this).children('a:first')[0].click();
        //})
        //var menu = $('#vbMenu').val();
        //var subMenu = $('#vbSubmenu').val();
        //if (menu !== undefined && menu.length > 0) {
        //    $('#menu-content li').removeClass('active');
        //    $('#' + menu).addClass('active');
        //    if (menu !== 'HomeMain')
        //        $('#' + menu).click();
        //    if (subMenu !== undefined && subMenu.length > 0) {
        //        $('#' + subMenu).addClass('active');
        //        $('.nav-side-menu').animate({ scrollTop: $('#' + menu).offset().top }, 800);
        //    }
        //}
    }
})(window);
