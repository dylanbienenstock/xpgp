$(function () {
    $("html, body").resize(function () {
        centerNavbarElements();
    });

    centerNavbarElements();
    setTimeout(centerNavbarElements, 125);
});

function centerNavbarElements() {
    // $("#navbar-searchbar-container").centerVertical();
    // $("#navbar-searchbar-button-icon").centerVertical();
    // $("#navbar-menu-icon").centerVertical();
    // $("#navbar-userinfo-icon").centerVertical();
}