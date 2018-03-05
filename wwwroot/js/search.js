$(() => {
    $(".profile-pic-container, .profile-info").click(function() {
        document.location.href = $(this).parent().attr("data-profileurl");
    });
});