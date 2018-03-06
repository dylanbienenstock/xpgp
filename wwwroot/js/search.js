$(() => {
    $(".profile-pic-container, .profile-info").click(function() {
        document.location.href = $(this).parent().attr("data-profileurl");
    });

    $(".keypair-view-button").click(function(e) {
        if (e.target.className.startsWith("pinned-save-button")) {
            toggleKeyPairSaved(this);

            return;
        }

        document.location.href = $(this).parent().parent().attr("data-profileurl");        
    });
});