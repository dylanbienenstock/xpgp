window.hideKeyPairDisplay = true;

function keyPairClicked(selectedKeyPair) {
    var publicKeyUrl = document.location.origin + $(selectedKeyPair).attr("data-viewurl");

    console.log(publicKeyUrl);

    $.ajax({
        type: "GET", url: publicKeyUrl, success: function(response) {
            $("#content-panel-qrcode").empty().qrcode({
                width: 180,
                height: 180,
                text: document.location.origin + $(selectedKeyPair).attr("data-viewurl")
            });

            $("#keypair-display-name").text($(selectedKeyPair).attr("data-name"));
            $("#keypair-display-owner").text("Owned by " + $(selectedKeyPair).attr("data-owner"));
            $("#keypair-display-publickey").text(response);
            $("#keypair-display-inner").css({ display: "flex" });
        }
    });
}