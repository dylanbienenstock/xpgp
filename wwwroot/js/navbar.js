// TODO: Make this file less redundant

var accountMenuOpen = false;
var mobileMenuOpen = false;
var notificationMenuOpen = false;
var notificationMenuEverOpened = false;

$(() => {
    $(".notification-content-datetime").timeago();

    $("html, body").resize(() => {
        if (accountMenuOpen) {
            positionAccountMenu();
        }

        if (notificationMenuOpen) {
            positionNotificationMenu();
        }
    });

    $("#navbar-menu-icon").click((e) => {
        if (!mobileMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            $("#navbar-mobile-menu").slideDown(150);
            $("#navbar-account-menu").hide();
            $("#navbar-notification-menu").hide();            
            mobileMenuOpen = true;
            accountMenuOpen = false;
            notificationMenuOpen = false;
        }
    });

    $("#navbar-userinfo").click((e) => {
        if (!accountMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            $("#navbar-account-menu").slideDown(150);
            $("#navbar-mobile-menu").hide();
            $("#navbar-notification-menu").hide();
            positionAccountMenu();
            accountMenuOpen = true;
            mobileMenuOpen = false;
            notificationMenuOpen = false;
        }
    });

    $("#navbar-bell").click((e) => {
        if (!notificationMenuOpen) {
            e.stopPropagation();
            e.preventDefault();

            if (!notificationMenuEverOpened) {
                let notificationIds = [];
                let notifications = $(`.notification[data-seen="False"]`);

                if (notifications.length) {
                    notifications.each(function() {
                        notificationIds.push($(this).attr("data-id"));
                    })

                    $.post(
                        document.location.origin + "/NotificationsSeen",
                        { NotificationIdsRaw: notificationIds.join() },
                        (res) => {
                            console.error(res);
                        }
                    );
                }
            }

            notificationMenuEverOpened = true;

            $("#navbar-notification-menu").slideDown(150);
            $("#navbar-mobile-menu").hide();
            $("#navbar-account-menu").hide();
            $("#navbar-bell-inner").attr("data-blip", "False");
            positionNotificationMenu();
            notificationMenuOpen = true;            
            mobileMenuOpen = false;
            accountMenuOpen = false;     
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

        if (!e.target.id.startsWith("navbar-notification")) {
            if (notificationMenuOpen) {
                $("#navbar-notification-menu").slideUp(150, () => {
                    // "Merge" notification sections
                    $("#navbar-notification-menu-notifications-new-title").remove();
                    $("#navbar-notification-menu-notifications-new").css({ 
                        border: "none",
                        padding: "8px 8px 0px 8px"
                    });

                    $("#navbar-notification-menu-notifications-old").css({
                        padding: "0px 8px 8px 8px"
                    });
                });

                notificationMenuOpen = false;
            }
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

function positionNotificationMenu() {
    var $accountMenu = $("#navbar-notification-menu");
    var $userInfo = $("#navbar-bell-inner");

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