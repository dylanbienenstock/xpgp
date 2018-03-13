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
                text: document.location.origin + response.viewUrl
            });

            $("#content-panel-newkeypair-button-download").click(function (e) {
                e.preventDefault();
                window.location.href = response.downloadUrl;
            });

            $("#content-panel-newkeypair-button-share").click(function (e) {
                copyToClipboard(document.location.origin + response.viewUrl);
                $.xpgpModal({
                    title: "Notice",
                    text: "Link has been copied to clipboard."
                });
            });

            $form.find("input").prop("disabled", true);
            $form.find("input, select").css({ borderColor: "#232323" });        
            
            $("#content-panel-newkeypair-button-download, #content-panel-newkeypair-button-share")
                .removeClass("content-panel-button-disabled")
                .addClass("content-panel-button-highlighted")
                .prop("disabled", false);
        }
        else
        {
            alert(response.errors.join("\n"));
            location.reload();
        }
    });
}