//import { Constants } from './Constants';

class Constants {

    static ConnectedLogo = "/utilities/connected.svg";
    static DisConnectedLogo = "/utilities/disconnected.svg";
}

async function connectDirectoryToWeb(url, folder, id) {

    document.getElementById(`${id}_logo`).classList.add("hidden");
    document.getElementById(`${id}_spinner`).classList.remove("hidden");

    let result = await AjaxCall(url, folder);

    result = await result.json();

    console.log(result);

    document.getElementById(`${id}_spinner`).classList.add("hidden");
    document.getElementById(`${id}_logo`).classList.remove("hidden");

    if (result.success) {

        document.getElementById(`${id}_btnConnect`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.ConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-danger");
        document.getElementById(`${id}_web`).classList.add("bg-primary");
        document.getElementById(`${id}_tr1`).classList.remove("hidden");
        document.getElementById(`${id}_tr2`).classList.remove("hidden");
        document.getElementById(`${id}_btnDisConnect`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}
function connectDirectoryToApp(folder, id) {

}

function connectFileToWeb(file, id) {

}

function connectFileToApp(file, id) {

}

async function disConnectDirectoryFromWeb(url, folder, id) {

    document.getElementById(`${id}_logo`)?.classList.add("hidden");
    document.getElementById(`${id}_spinner`)?.classList.remove("hidden");

    let result = await AjaxCall(url, folder);

    result = await result.json();

    console.log(result);

    document.getElementById(`${id}_spinner`)?.classList.add("hidden");
    document.getElementById(`${id}_logo`)?.classList.remove("hidden");

    if (result.success) {

        document.getElementById(`${id}_btnDisConnect`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.DisConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-primary");
        document.getElementById(`${id}_web`).classList.add("bg-danger");
        document.getElementById(`${id}_tr1`).classList.add("hidden");
        document.getElementById(`${id}_tr2`).classList.add("hidden");
        document.getElementById(`${id}_btnConnect`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}
function disConnectDirectoryFromApp(folder, id) {

}

function disConnectFileFromWeb(file, id) {

}

function disConnectFileFromApp(file, id) {

}

async function AjaxCall(address, itemName) {
    const forgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    var response = await fetch(address, {
        method: "POST",
        headers: {
            "Content-Type": 'application/json',
            "X-CSRF-TOKEN": forgeryToken
        },
        body: JSON.stringify(itemName)
    });
    return response;
}

function ShowSuccessToast(title, message) {

    const toastHTML = document.getElementById("toast_success");

    if (toastHTML) {

        document.getElementById("success_title").innerHTML = title;
        document.getElementById("success_body").innerHTML = message;

        const toast = new bootstrap.Toast(toastHTML);

        toast.show();
    }
}

function ShowErrorToast(title, message) {

    const toastHTML = document.getElementById("toast_error");

    if (toastHTML) {

        document.getElementById("error_title").innerHTML = title;
        document.getElementById("error_body").innerHTML = message;

        const toast = new bootstrap.Toast(toastHTML);

        toast.show();
    }
}