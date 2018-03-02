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
            left: $userInfo.offset().left - $accountMenu.width() + $userInfo.outerWidth() + 16,
            top: $userInfo.height() - 20
        });
    }
}