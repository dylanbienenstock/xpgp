function copyToClipboard(text) {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(text).select();
    document.execCommand("copy");
    $temp.remove();
}

var firstClick = true; // First key view button click is simulated;

$(function () {
    $("html, body").scrollLeft(0);

    $("#content-panel-newkeypair-button-generate").click(() => {
        generateKeyPair();
    });

    $(".keypair-view-button").click(function() {
        $(".keypair-view-button").not(this).removeClass("keypair-view-button-selected");
        $(this).addClass("keypair-view-button-selected");

        $("#content-panel-qrcode").empty().qrcode({
            width: 180,
            height: 180,
            text: document.location.origin + 
                  $(this).attr("data-userid") + "/" +
                  $(this).attr("data-keypairid") + "/"
        });

        $("#keypair-display-name").text($(this).attr("data-name"));
        $("#keypair-display-owner").text("Owned by " + $(this).attr("data-owner"));

        if (!firstClick && $(document.body).width() <= 768) {
            $('html, body').animate({
                scrollLeft: $(document.body).width()
            }, () => {
                $("#content-panel-header-back").fadeIn();
            });

            $(this).blur();
        }

        firstClick = false;
    });

    $(".keypair-view-button").first().click();

    $("#content-panel-header-back").click(function() {
        $(this).fadeOut(() => {
            $('html, body').animate({
                scrollLeft: 0
            });
        });
    });
});

function generateKeyPair() {
    $("#content-panel-qrcode-loading").animate({ opacity: 1 });

    $("#content-panel-newkeypair-button-generate")
        .addClass("content-panel-button-disabled")
        .removeClass("content-panel-button-highlighted")        
        .addClass("content-panel-button-unhighlighted")        
        .prop("disabled", false);

    var $form = $("#content-panel-newkeypair-form");

    $form.ajaxSubmit(function(response) {
        console.log(response);
        
        $("#content-panel-qrcode-loading").hide();     

        if (response.success) {
            $("#content-panel-qrcode").qrcode({
                width: 180,
                height: 180,
                text: response.viewUrl
            });

            $("#content-panel-newkeypair-button-download").click(function (e) {
                e.preventDefault();
                window.location.href = response.downloadUrl;
            });

            $("#content-panel-newkeypair-button-share").click(function (e) {
                copyToClipboard(document.location.origin + response.viewUrl);
            });

            $form.find("input").prop("disabled", true);
            $form.find("input, select").css({ borderColor: "#232323" });        
            
            $("#content-panel-newkeypair-button-download, #content-panel-newkeypair-button-share")
                .removeClass("content-panel-button-disabled")
                .addClass("content-panel-button-highlighted")
                .prop("disabled", false);
        }
    });
}