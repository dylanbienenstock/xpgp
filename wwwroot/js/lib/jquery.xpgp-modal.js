window.xpgpExampleModal = {
    title: "Confirm credentials",
    text: "Enter your email address and password.",
    inputs: {
        // (OPTIONAL)
        // Creates input element. The objects key (password)
        // sets the name attribute, and the keys/values
        // within the object specify the other attributes
        // This would create the following elements:
        // <input name="email" type="text" placeholder="Email address">
        // <input name="password" type="password" placeholder="Password">
        email: {
            type: "text",
            placeholder: "Email address",
            autocomplete: "off"
        },
        password: {
            type: "password",
            placeholder: "Password"
        }
    },
    buttons: {
        // (OPTIONAL)
        // Creates button element. The object's key (Confirm)
        // sets the inner html. The function's parameter (inputs)
        // is an objects where the keys are the keys from the inputs object
        // defined above, and the values are the input elements' values.
        // If you type "password12345" into the password input,
        // the inputs object would look like this:
        // { email: "...", password: "password12345" }
        // The generated button elements would look like this:
        // <button data-name="Cancel">Cancel</button>
        // <button data-name="Confirm">Confirm</button>
        Cancel: () => { return true; },
        Confirm: (inputs) => {
            xpgpModalLoading(true);

            setTimeout(() => { // Pretend to wait a bit
                xpgpModalClose();
            }, 2000);

            return false; // Don't close the modal
        }
    }
};

const xpgpModalDefaultClasses = {
    // Darkens the screen to increase contrast
    curtain: "xpgp-modal-curtain",
    // Applies after creation, used for transitions
    curtainVisible: "xpgp-modal-curtain-visible",
    modalContainer: "xpgp-modal-container",
    modalContainerVisible: "xpgp-modal-container-visible",
    modal: "xpgp-modal",
    modalContent: "xpgp-modal-content",
    title: "xpgp-modal-title",
    textContainer: "xpgp-modal-text-container",
    text: "xpgp-modal-text",
    inputContainer: "xpgp-modal-input-container",
    input: "xpgp-modal-input",
    buttonContainer: "xpgp-modal-button-container",
    button: "content-panel-button content-panel-button-highlighted",
    containerHidden: "xpgp-modal-container-hidden",
    loadingSpinner: "xpgp-modal-loading",
    loadingSpinnerVisible: "xpgp-modal-loading-visible"
};

$(() => { 
    var xpgpModalOpen = false;
    
    function xpgpModalCreateInputs(inputs, modalClasses) {
        let html = "";

        for (let inputKey in inputs) {
            if (!inputs.hasOwnProperty(inputKey)) continue;

            let input = inputs[inputKey];

            html += `<input name="${inputKey}" class="${modalClasses.input}"`;

            for (let inputAttrKey in input) {
                if (!input.hasOwnProperty(inputAttrKey)) continue;

                html += ` ${inputAttrKey}="${input[inputAttrKey]}"`;
            }

            html += `>`;
        }

        return html;
    }

    function xpgpModalCreateButtons(buttons, modalClasses) {
        let html = "";

        for (let buttonKey in buttons) {
            if (!buttons.hasOwnProperty(buttonKey)) continue;

            let button = buttons[buttonKey];

            html += `<button class="${modalClasses.button}" data-name="${buttonKey}">${buttonKey}</button>`;
        }

        return html;
    }

    $("html, body").keydown((e) => {
        if (e.key == "Escape") {
            e.preventDefault();
            e.stopPropagation();
            $.xpgpModal(window.xpgpExampleModal);
        }
    });  

    $.xpgpModal = function(modalTemplate, modalOverrideClasses) {
        console.log(xpgpModalOpen);

        if (xpgpModalOpen) {
            console.error("Only one modal can be open at a time.");

            return;
        }

        // If there are no buttons specified, make one
        modalTemplate.buttons = modalTemplate.buttons || {
            Okay: () => { return true; }
        }
        
        xpgpModalOpen = true;
        var modalClasses = xpgpModalDefaultClasses;

        if (modalOverrideClasses) {
            modalClasses = {};

            for (let modalClassKey in xpgpModalDefaultClasses) {
                if (!xpgpModalDefaultClasses.hasOwnProperty(modalClassKey)) continue;

                if (modalOverrideClasses.hasOwnProperty(modalClassKey)) {
                    modalClasses[modalClassKey] = modalOverrideClasses[modalClassKey];

                    continue;
                }

                modalClasses[modalClassKey] = xpgpModalDefaultClasses[modalClassKey];
            }
        }

        let html = `
            <div class="${modalClasses.curtain}">&nbsp</div>

            <div class="${modalClasses.modalContainer}">
                <div class="${modalClasses.modal}">      
                    <div class="${modalClasses.title}">${modalTemplate.title}</div>

                    <div class="${modalClasses.modalContent}">
                        <img class="${modalClasses.loadingSpinner}" src="/img/loading.svg">

                        ${
                            modalTemplate.text ?
                            `
                                <div class="${modalClasses.textContainer}">
                                    <p class="${modalClasses.text}">${modalTemplate.text}</p>
                                </div>
                            ` : ""
                        }

                        ${
                            modalTemplate.inputs ? 
                            `
                                <div class="${modalClasses.inputContainer}">
                                    ${xpgpModalCreateInputs(modalTemplate.inputs, modalClasses)}
                                </div>
                            `: ""
                        }

                        <div class="${modalClasses.buttonContainer}">
                            ${xpgpModalCreateButtons(modalTemplate.buttons, modalClasses)}                
                        </div>
                    </div>
                </div>
            </div>
        `;

        let modal = $(html).prependTo($("body"));  

        setTimeout(() => { // Do this after the DOM operation is finished
            let curtain = $(`[class="${modalClasses.curtain}"]`);
            let modalContainer = $(`[class="${modalClasses.modalContainer}"]`);

            curtain.addClass(modalClasses.curtainVisible);
            modalContainer.addClass(modalClasses.modalContainerVisible);

            window.xpgpModalClose = function() {
                curtain.removeClass(modalClasses.curtainVisible);
                modalContainer.removeClass(modalClasses.modalContainerVisible);

                let transitionend = `
                    transitionend
                    webkitTransitionEnd
                    oTransitionEnd
                    otransitionend
                    MSTransitionEnd
                `;
                
                modalContainer.on(transitionend, function(e) {
                    if (e.target.className != modalContainer.attr("class")) return;
                    
                    curtain.remove();
                    modalContainer.unbind(transitionend);
                    modalContainer.remove();
                    xpgpModalOpen = false;
                });
            }

            window.xpgpModalLoading = function(loading) {
                if (loading) {
                    $(`[class="${modalClasses.textContainer}"], [class="${modalClasses.inputContainer}"], [class="${modalClasses.buttonContainer}"]`)
                        .addClass(modalClasses.containerHidden);

                    $(`[class="${modalClasses.loadingSpinner}"]`).addClass(modalClasses.loadingSpinnerVisible);
                } else {
                    $(`[class*="${modalClasses.containerHidden}"]`)
                        .removeClass(modalClasses.containerHidden);

                    $(`[class*="${modalClasses.loadingSpinnerVisible}"]`)
                        .removeClass(modalClasses.loadingSpinnerVisible);
                }
            }

            $(`[class="${modalClasses.button}"]`).click(function() {
                let buttonKey = $(this).attr("data-name");

                if (typeof modalTemplate.buttons[buttonKey] == "function") {
                    let inputs = {};

                    $(`[class="${modalClasses.inputContainer}"]`).children()
                    .each(function() {
                        inputs[$(this).attr("name")] = $(this).val();
                    });

                    let shouldClose = modalTemplate.buttons[buttonKey](inputs);

                    if (shouldClose) {
                        window.xpgpModalClose();
                    }
                }
            });
        }, 100);
    }
});