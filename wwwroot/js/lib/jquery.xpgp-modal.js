window.xpgpExampleModal = {
    title: "Confirm your password",
    mandatory: true, // Does the modal have an X button in the corner?
    inputs: {
        // Creates input element. The objects key (password)
        // sets the name attribute, and the keys/values
        // within the object specify the other attributes
        // This would create the following element:
        // <input name="password" type="password" placeholder="Password">
        password: {
            type: "password",
            placeholder: "Password"
        }
    },
    buttons: {
        // Creates button element. The object's key (Confirm)
        // sets the inner html. The function's parameter (inputs)
        // is an objects where the keys are the keys from the inputs object
        // defined above, and the values are the input elements' values.
        // If you type "password12345" into the password input,
        // the inputs object would look like this:
        // { password: "password12345" }
        // The generated button element would look like this:
        // <button>Confirm</button>
        Confirm: (inputs) => {
            inputs.password = inputs.password.trim();

            if (inputs.password.length >= 8) { // Valid password length
                // Do something with password

                return true; // Close the modal
            }

            return false; // Don't close the modal
        }
    }
};

const xpgpModalDefaultClasses = {
    curtain: "xpgp-modal-curtain", // Darkens the screen to increase contrast
    modalContainer: "xpgp-modal-container",
    modal: "xpgp-modal",
    title: "xpgp-modal-title",
    closeButton: "xpgp-modal-closebutton",
    inputContainer: "xpgp-modal-input-container",
    input: "content-input",
    buttonContainer: "xpgp-modal-button-container",
    button: "content-panel-button content-panel-button-highlighted",
};

$(() => {
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

        html += `</div>`;

        return html;
    }

    function xpgpModalCreateButtons(buttons, modalClasses) {
        let html = "";

        for (let buttonKey in buttons) {
            if (!buttons.hasOwnProperty(buttonKey)) continue;

            let button = buttons[buttonKey];

            html += `<button class="${modalClasses.button}">${buttonKey}</button>`;
        }

        return html;
    }

    $.xpgpModal = function (modalTemplate, modalOverrideClasses) {
        let modalClasses = xpgpModalDefaultClasses;

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

        let modal = `
            <div class="${modalClasses.curtain}">&nbsp</div>

            <div class="${modalClasses.modalContainer}">
                ${
                    modalTemplate.mandatory ?
                    `<div class="${modalClasses.closeButton}">&times;</div>` : ""
                }

                <div class="${modalClasses.modal}">            
                    <h1 class="${modalClasses.title}"></h1>

                    <div class="${modalClasses.inputContainer}">
                        ${xpgpModalCreateInputs(modalTemplate.inputs, modalClasses)}
                    </div>

                    <div class="${modalClasses.buttonContainer}">
                        ${xpgpModalCreateButtons(modalTemplate.buttons, modalClasses)}                
                    </div>
                </div>
            </div>
        `;

        console.log(modal);
    }
});