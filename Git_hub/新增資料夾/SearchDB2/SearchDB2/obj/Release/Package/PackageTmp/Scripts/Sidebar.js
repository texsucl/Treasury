///* off-canvas sidebar toggle */
//$('[data-toggle=offcanvas]').click(function () {
//    $('.row-offcanvas').toggleClass('active');
//    $('.collapse').toggleClass('in').toggleClass('hidden-xs').toggleClass('visible-xs');
//});
(function (window, undefind) {
    if ($('.menu-list') != null) {
        $('.menu-list ul.sub-menu li,.HomeMenu').click(function () {
            $(this).children('a:first')[0].click();
        })
        var menu = $('#vbMenu').val();
        var subMenu = $('#vbSubmenu').val();
        if (menu !== undefined && menu.length > 0) {
            $('#menu-content li').removeClass('active');
            $('#' + menu).addClass('active');
            if (menu !== 'HomeMain')
                $('#' + menu).click();
            if (subMenu !== undefined && subMenu.length > 0) {
                $('#' + subMenu).addClass('active');
                $('.nav-side-menu').animate({ scrollTop: $('#' + menu).offset().top }, 800);
            }
        }
    }
})(window);