$(() => {
    $("#keypair-display-button-download").click((e) => {
        e.preventDefault();
        document.location.href = window.downloadUrl;
    });
});