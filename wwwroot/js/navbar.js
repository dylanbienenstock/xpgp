var accountMenuOpen = false;

$(() => {
    $("html, body").resize(() => {
        if (accountMenuOpen) {
            positionAccountMenu();
        }
    });

    $("#navbar-userinfo").click((e) => {
        if (!accountMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            $("#navbar-account-menu").slideDown(150);
            positionAccountMenu();
            accountMenuOpen = true;
        }
    });

    $("html, body").click(function(e) {
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
        $accountMenu.width($userInfo.outerWidth()).offset({
            left: $userInfo.offset().left,
            top: $userInfo.height() - 20
        });
    } else { // Mobile
        $accountMenu.css({ width: "auto" });

        $accountMenu.offset({
            left: $userInfo.offset().left - $accountMenu.width() + $userInfo.outerWidth(),
            top: $userInfo.height() - 20
        });
    }
}