$(() => {
    $("html, body").resize(() => {
        $("#register-errors").css({
            width: $("#logreg-form").width() + 16,
            height: $("#logreg-form").height() + 16,
        });
    });

    $("#register-errors-close").click(() => {
        $("#register-errors").fadeOut();
    });
});