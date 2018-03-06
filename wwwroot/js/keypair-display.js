var selectedKeyPair;

$(() => {
    // Show keypair owner on hover over bubble
    $(".keypair-view-button-icon-container").hover(function() {
        $(this).parent()
            .find(".keypair-view-button-title")
            .text($(this).attr("data-ownertext").trim())
            .css({ fontStyle: "italic" });
    }, function() {
        $(this).parent()
            .find(".keypair-view-button-title")
            .text($(this).parent().attr("data-name"))
            .css({ fontStyle: "unset" });            
    });

    $(".keypair-view-button").click(function(e) {
        e.preventDefault();

        if (e.target.className.startsWith("keypair-view-button-icon")) {
            clickOwnerIcon(e);

            return;
        }

        selectedKeyPair = this;

        if (e.target.className.startsWith("pinned-save-button")) {
            toggleKeyPairSaved(selectedKeyPair);

            return;
        }
        
        if (window.hideKeyPairDisplay) $("#keypair-display-inner").hide();
        
        $("#keypair-display-loading").css({ display: "flex" });
        $(".keypair-view-button").not(selectedKeyPair).removeClass("keypair-view-button-selected");
        $(selectedKeyPair).addClass("keypair-view-button-selected");

        if ($(this).attr("data-ismine") == "True") {
            $("#keypair-display-button-delete").show();
            $("#keypair-display-button-save").hide();

            if ($(selectedKeyPair).attr("data-ispinned") == "False") {
                $("#keypair-display-button-pin").show();
                $("#keypair-display-button-unpin").hide();
            } else {
                $("#keypair-display-button-pin").hide();
                $("#keypair-display-button-unpin").show();
            }
        } else {
            $("#keypair-display-button-delete").hide();
            $("#keypair-display-button-pin").hide();
            $("#keypair-display-button-unpin").hide();
            $("#keypair-display-button-save").show();
        }

        correctSaveButton(selectedKeyPair);

        if (!firstClick && $(document.body).width() <= 768) {
            if (typeof(keyPairViewScrollRight) == Function) {
                keyPairViewScrollRight();
            }

            $("#content-panel-viewkeypairs").velocity({
                transform: "translateX(-100vw)"
            }, {
                duration: 400,
                complete: () => {
                    $("#content-panel-header-back").fadeIn();
                }
            });

            $(selectedKeyPair).blur();
        }

        firstClick = false;

        if (keyPairClicked) {
            keyPairClicked(selectedKeyPair);
        }
    });

    $(".keypair-view-button").first().click();

    $("#keypair-display-button-delete").click((e) => {
        var response = prompt("Are you sure? Please type the name of the key:")

        if (response != null && 
            response.trim().toLowerCase() == 
            $(selectedKeyPair).attr("data-name").trim().toLowerCase()) {

            e.preventDefault();
            window.location.href = $(selectedKeyPair).attr("data-deleteurl");
            
            alert("Keypair deleted. This action cannot be undone.");
        } else {
            alert("Keypair not deleted.");
        }
    });

    $("#keypair-display-button-save").click((e) => {
        toggleKeyPairSaved(selectedKeyPair);
    });

    $("#keypair-display-button-pin").click((e) => {
        e.preventDefault();

        $("#keypair-display-button-pin-form")
            .children()
            .val($(selectedKeyPair).attr("data-keypairid"));
        
        $("#keypair-display-button-pin-form").submit();
    });

    $("#keypair-display-button-unpin").click((e) => {
        e.preventDefault();

        $("#keypair-display-button-unpin-form")
            .children()
            .val($(selectedKeyPair).attr("data-keypairid"));

        $("#keypair-display-button-unpin-form").submit();
    });

    $("#keypair-display-button-download").click((e) => {
        e.preventDefault();
        window.location.href = $(selectedKeyPair).attr("data-downloadurl");
    });

    $("#keypair-display-button-share").click(() => {
        copyToClipboard(document.location.origin + $(selectedKeyPair).attr("data-viewurl"));
        alert("Link copied to clipboard.")
    });

    $("#content-panel-header-back").click(function () {
        $(this).fadeOut(() => {
            if (typeof(keyPairViewScrollLeft) == Function) {
                keyPairViewScrollLeft();
            }

            $("#content-panel-viewkeypairs").velocity({
                transform: "translateX(0)"
            }, {
                duration: 400
            });
        });
    });
});

function clickOwnerIcon(e) {
    e.target.displayingOwner = !e.target.displayingOwner;

    e.stopPropagation();

    if (!e.target.displayingOwner) {
        $(this)
            .find(".keypair-view-button-title")
            .text($(e.target).parent().parent().attr("data-ownertext"))
            .css({ fontStyle: "italic" });
    } else {
        $(this)
            .find(".keypair-view-button-title")
            .text($(this).attr("data-name"))
            .css({ fontStyle: "unset" })
    }
}