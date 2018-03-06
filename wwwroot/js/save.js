function correctSaveButton(keyPair) {
    var saved = $(keyPair).attr("data-issaved") == "True";
    var $saveButton = $("#keypair-display-button-save");

    var $pinned = $(".keypair-view-button[data-ispinned=True]");
    var pinnedSaved = $pinned.attr("data-issaved") == "True";
    var $pinnedSaveButton = $pinned.find(".pinned-save-button");

    if (saved) {
        $saveButton
            .removeClass("content-panel-button-highlighted")
            .addClass("content-panel-button-danger")
            .text("Unsave");
    } else {
        $saveButton
            .removeClass("content-panel-button-danger")
            .addClass("content-panel-button-highlighted")
            .text("Save");
    }

    if (pinnedSaved) {
        $pinnedSaveButton
            .addClass("pinned-save-button-remove");
    } else {
        $pinnedSaveButton
            .removeClass("pinned-save-button-remove");
    }
}

function toggleKeyPairSaved(keyPair) {
    var saved = $(keyPair).attr("data-issaved") == "True";
    var keyPairId = $(keyPair).attr("data-keypairid");
    var $pinnedSaveButton = $(".pinned-save-button");
    var $keyPairViewButton = $(`.keypair-view-button[data-keypairid=${keyPairId}]`);

    if (!saved) {
        $keyPairViewButton.attr("data-issaved", "True");

        $.ajax(document.location.origin + "/SaveKeyPair", {
            method: "POST",
            data: {
                "KeyPairId": $(keyPair).attr("data-keypairid"),
                "UserId": $(keyPair).attr("data-userid")
            }
        });
    } else {
        $keyPairViewButton.attr("data-issaved", "False");

        $.ajax(document.location.origin + "/UnSaveKeyPair", {
            method: "POST",
            data: {
                "KeyPairId": $(keyPair).attr("data-keypairid"),
                "UserId": $(keyPair).attr("data-userid")
            }
        });
    }

    correctSaveButton(keyPair);
}