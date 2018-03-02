window.hideKeyPairDisplay = false;
var selectedKeyPairId;
var selectedUserId;

function keyPairClicked(selectedKeyPair) {
    $("#content-header-text").text("Encrypt using " + $(selectedKeyPair).attr("data-name"));
    selectedKeyPairId = $(selectedKeyPair).attr("data-keypairid");
    selectedUserId = $(selectedKeyPair).attr("data-userid");
}

$(() => {
    $("#keypair-display-button-encrypt").click(function() {
        $.ajax({ 
            url: window.encryptUrl,
            type: "POST",
            data: { 
                keyPairId: selectedKeyPairId,
                userId: selectedUserId,
                text: $("#encrypt-input").val()
            }, 
            success: function(response) {
                $("#encrypt-input").val(response);
            }
        });
    });

    $("#keypair-display-button-copy").click(function () {
        copyToClipboard($("#encrypt-input").val());
        alert("Copied.");
    });
});