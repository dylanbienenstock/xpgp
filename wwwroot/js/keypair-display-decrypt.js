window.hideKeyPairDisplay = false;
var selectedKeyPairId;
var selectedUserId;

function keyPairClicked(selectedKeyPair) {
    $("#content-header-text").text("Decrypt using " + $(selectedKeyPair).attr("data-name"));
    selectedKeyPairId = $(selectedKeyPair).attr("data-keypairid");
    selectedUserId = $(selectedKeyPair).attr("data-userid");
}

$(() => {
    $("#keypair-display-button-decrypt").click(function() {
        $.ajax({ 
            url: window.decryptUrl,
            type: "POST",
            data: { 
                keyPairId: selectedKeyPairId,
                userId: selectedUserId,
                text: $("#decrypt-input").val()
            }, 
            success: function(response) {
                $("#decrypt-input").val(response);
            }
        });
    });
});