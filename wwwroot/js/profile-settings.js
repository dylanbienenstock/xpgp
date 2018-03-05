$(() => {
    $("#profile-pic-hover").click((e) => {
        e.preventDefault();
        $("#profile-pic-input").trigger("click");
    });

    $("#profile-pic-input").change(function() {
        if (this.files && this.files[0]) {
            var reader = new FileReader();

            reader.onload = function(e) {
                $("#profile-pic")
                    .attr('src', e.target.result)
                    .width(128)
                    .height(128);
            };

            reader.readAsDataURL(this.files[0]);
        }
    });
});