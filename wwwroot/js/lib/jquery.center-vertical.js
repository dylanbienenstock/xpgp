$.fn.extend({
    centerVertical: function () {
        $(this).offset({
            top: $(this).parent().offset().top + $(this).parent().innerHeight() / 2 - $(this).outerHeight() / 2,
            left: $(this).offset().left
        });
    }
});