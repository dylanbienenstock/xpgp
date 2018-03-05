var selectedKeyPair;

$(() => {
    $(".keypair-view-button-icon-container").hover(function() {
        $(this).parent()
            .find(".keypair-view-button-title")
            .text("Owner: " + $(this).attr("data-ownertext").trim());
    }, function() {
        $(this).parent()
            .find(".keypair-view-button-title")
            .text($(this).parent().attr("data-name"));
    });

    $(".keypair-view-button").click(function(e) {
        e.preventDefault();

        if (e.target.className.startsWith("keypair-view-button-icon")) {
            e.target.displayingOwner = !e.target.displayingOwner;

            e.stopPropagation();

            console.log(e.target);

            if (!e.target.displayingOwner) {
                $(this)
                    .find(".keypair-view-button-title")
                    .text("Owner: " + $(e.target).parent().parent().attr("data-ownertext"));
            } else {
                $(this)
                    .find(".keypair-view-button-title")
                    .text($(this).attr("data-name"));
            }

            return;
        }

        selectedKeyPair = this;
        
        if (window.hideKeyPairDisplay) $("#keypair-display-inner").hide();
        
        $("#keypair-display-loading").css({ display: "flex" });
        $(".keypair-view-button").not(selectedKeyPair).removeClass("keypair-view-button-selected");
        $(selectedKeyPair).addClass("keypair-view-button-selected");

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