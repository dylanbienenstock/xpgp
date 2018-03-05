var accountMenuOpen = false;
var mobileMenuOpen = false;

$(() => {
    $("html, body").resize(() => {
        if (accountMenuOpen) {
            positionAccountMenu();
        }
    });

    $("#navbar-menu-icon").click((e) => {
        if (!mobileMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            $("#navbar-mobile-menu").slideDown(150);
            $("#navbar-account-menu").hide();
            mobileMenuOpen = true;
            accountMenuOpen = false;
        }
    });

    $("#navbar-userinfo").click((e) => {
        if (!accountMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            $("#navbar-account-menu").slideDown(150);
            $("#navbar-mobile-menu").hide();            
            positionAccountMenu();
            accountMenuOpen = true;
            mobileMenuOpen = false;
        }
    });

    $("html, body").click(function(e) {
        if (!e.target.id.startsWith("navbar-mobile")) {
            $("#navbar-mobile-menu").slideUp(150);
            mobileMenuOpen = false;
        }

        if (!e.target.id.startsWith("navbar-account")) {
            $("#navbar-account-menu").slideUp(150);
            accountMenuOpen = false;
        }
    });
});

function positionAccountMenu() {
    var $accountMenu = $("#navbar-account-menu");
    var $userInfo = $("#navbar-userinfo"); 

    if ($(document.body).width() > 768) { // Desktop
        if ($userInfo.outerWidth() < 175) {
            $accountMenu.css({ width: "auto" }).offset({
                left: $userInfo.offset().left + $userInfo.outerWidth() / 2 - $accountMenu.outerWidth() / 2,
                top: $userInfo.height() - 20
            });
        } else {
            $accountMenu.width($userInfo.outerWidth()).offset({
                left: $userInfo.offset().left,
                top: $userInfo.height() - 20
            });
        }
    } else { // Mobile
        $accountMenu.css({ width: "auto" });

        $accountMenu.offset({
            left: $("body").innerWidth() - $accountMenu.outerWidth(),
            top: $userInfo.height() - 20
        });
    }
}