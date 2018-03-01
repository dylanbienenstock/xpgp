$(function () {
    $("#content-panel-newkeypair-button-generate").click(() => {
        generateKeyPair();
    });

    $(".keypair-view-button").click(function() {
        $(".keypair-view-button").not(this).removeClass("keypair-view-button-selected");
        $(this).addClass("keypair-view-button-selected");

        $("#content-panel-qrcode").empty().qrcode({
            width: 180,
            height: 180,
            text: "http://localhost:5050/" + 
                  $(this).attr("data-userid") + "/" +
                  $(this).attr("data-keypairid") + "/"
        });

        $("#keypair-display-name").text($(this).attr("data-name"));
        $("#keypair-display-owner").text("Owned by " + $(this).attr("data-owner"));
    });

    $(".keypair-view-button").first().click();
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
        $("#content-panel-qrcode-loading").hide();     

        if (response.success) {
            $("#content-panel-qrcode").qrcode({
                width: 180,
                height: 180,
                text: response.url
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