class Constants {

    static ConnectedLogo = "/utilities/connected.svg";
    static DisConnectedLogo = "/utilities/disconnected.svg";
}

$(function () {
    $('[data-toggle="tooltip"]').tooltip();
});

async function copyItemToCloudClipBoard(url, itemName) {

    console.log(itemName);

    let response = await AjaxCall(url, itemName);

    response = await response.json()

    if (response.success) {
        ShowSuccessToast("Success", response.message);
    } else {
        ShowErrorToast("Error", response.message);
    }
}


function copyToClipBoard(url) {

    try {
        navigator.clipboard.writeText(url);

        ShowSuccessToast("Success", "Url copied to clipboard");
    }
    catch {
        ShowErrorToast("Error", "Error while copying url to clipboard!");
    }
}

async function connectDirectoryToWeb(url, folder, id) {

    document.getElementById(`${id}_logo_2`).classList.add("hidden");
    document.getElementById(`${id}_spinner_2`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectDirWeb`).disabled = true;

    let result = await AjaxCall(url, folder);

    result = await result.json();

    document.getElementById(`${id}_spinner_2`).classList.add("hidden");
    document.getElementById(`${id}_logo_2`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectDirWeb`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_btnConnectDirWeb`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.ConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-danger");
        document.getElementById(`${id}_web`).classList.add("bg-primary");
        document.getElementById(`${id}_tr1`).classList.remove("hidden");
        document.getElementById(`${id}_tr2`).classList.remove("hidden");
        document.getElementById(`${id}_btnDisConnectDirWeb`).classList.remove("hidden");
        document.getElementById(`${id}_link`).href = result.result;
        document.getElementById(`${id}_link`).innerHTML = result.result;

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}
async function connectDirectoryToApp(url, folder, id) {

    document.getElementById(`${id}_logo_4`).classList.add("hidden");
    document.getElementById(`${id}_spinner_4`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectDirApp`).disabled = true;


    let result = await AjaxCall(url, folder);

    result = await result.json();

    document.getElementById(`${id}_spinner_4`).classList.add("hidden");
    document.getElementById(`${id}_logo_4`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectDirApp`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_btnConnectDirApp`).classList.add("hidden");
        document.getElementById(`${id}_app`).src = Constants.ConnectedLogo;
        document.getElementById(`${id}_app`).classList.remove("bg-danger");
        document.getElementById(`${id}_app`).classList.add("bg-primary");
        document.getElementById(`${id}_btnDisConnectDirApp`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function connectFileToWeb(url, file, id) {

    document.getElementById(`${id}_logo_2`).classList.add("hidden");
    document.getElementById(`${id}_spinner_2`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectFileWeb`).disabled = true;


    let result = await AjaxCall(url, file);

    result = await result.json();

    document.getElementById(`${id}_spinner_2`).classList.add("hidden");
    document.getElementById(`${id}_logo_2`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectFileWeb`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_btnConnectFileWeb`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.ConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-danger");
        document.getElementById(`${id}_web`).classList.add("bg-primary");
        document.getElementById(`${id}_tr1`).classList.remove("hidden");
        document.getElementById(`${id}_tr2`).classList.remove("hidden");
        document.getElementById(`${id}_btnDisConnectFileWeb`).classList.remove("hidden");
        document.getElementById(`${id}_link`).href = result.result;
        document.getElementById(`${id}_link`).innerHTML = result.result;

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function connectFileToApp(url, file, id) {

    document.getElementById(`${id}_logo_4`).classList.add("hidden");
    document.getElementById(`${id}_spinner_4`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectFileApp`).disabled = true;


    let result = await AjaxCall(url, file);

    result = await result.json();

    document.getElementById(`${id}_spinner_4`).classList.add("hidden");
    document.getElementById(`${id}_logo_4`).classList.remove("hidden");
    document.getElementById(`${id}_btnConnectFileApp`).disabled = false;


    if (result.success) {

        document.getElementById(`${id}_btnConnectFileApp`).classList.add("hidden");
        document.getElementById(`${id}_app`).src = Constants.ConnectedLogo;
        document.getElementById(`${id}_app`).classList.remove("bg-danger");
        document.getElementById(`${id}_app`).classList.add("bg-primary");
        document.getElementById(`${id}_btnDisConnectFileApp`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function disConnectDirectoryFromWeb(url, folder, id) {

    document.getElementById(`${id}_logo_1`)?.classList.add("hidden");
    document.getElementById(`${id}_spinner_1`)?.classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectDirWeb`).disabled = true;


    let result = await AjaxCall(url, folder);

    result = await result.json();

    document.getElementById(`${id}_spinner_1`)?.classList.add("hidden");
    document.getElementById(`${id}_logo_1`)?.classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectDirWeb`).disabled = false;


    if (result.success) {

        document.getElementById(`${id}_btnDisConnectDirWeb`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.DisConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-primary");
        document.getElementById(`${id}_web`).classList.add("bg-danger");
        document.getElementById(`${id}_tr1`).classList.add("hidden");
        document.getElementById(`${id}_tr2`).classList.add("hidden");
        document.getElementById(`${id}_btnConnectDirWeb`).classList.remove("hidden");
        removeQRCodeFromItem(id);

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}
async function disConnectDirectoryFromApp(url, folder, id) {

    document.getElementById(`${id}_logo_3`).classList.add("hidden");
    document.getElementById(`${id}_spinner_3`).classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectDirApp`).disabled = true;

    let result = await AjaxCall(url, folder);

    result = await result.json();

    document.getElementById(`${id}_spinner_3`).classList.add("hidden");
    document.getElementById(`${id}_logo_3`).classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectDirApp`).disabled = false;


    if (result.success) {

        document.getElementById(`${id}_btnDisConnectDirApp`).classList.add("hidden");
        document.getElementById(`${id}_app`).src = Constants.DisConnectedLogo;
        document.getElementById(`${id}_app`).classList.remove("bg-primary");
        document.getElementById(`${id}_app`).classList.add("bg-danger");
        document.getElementById(`${id}_btnConnectDirApp`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function disConnectFileFromWeb(url, file, id) {

    document.getElementById(`${id}_logo_1`)?.classList.add("hidden");
    document.getElementById(`${id}_spinner_1`)?.classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectFileWeb`).disabled = true;

    let result = await AjaxCall(url, file);

    result = await result.json();

    document.getElementById(`${id}_spinner_1`)?.classList.add("hidden");
    document.getElementById(`${id}_logo_1`)?.classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectFileWeb`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_btnDisConnectFileWeb`).classList.add("hidden");
        document.getElementById(`${id}_web`).src = Constants.DisConnectedLogo;
        document.getElementById(`${id}_web`).classList.remove("bg-primary");
        document.getElementById(`${id}_web`).classList.add("bg-danger");
        document.getElementById(`${id}_tr1`).classList.add("hidden");
        document.getElementById(`${id}_tr2`).classList.add("hidden");
        document.getElementById(`${id}_btnConnectFileWeb`).classList.remove("hidden");
        removeQRCodeFromItem(id);

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function disConnectFileFromApp(url, file, id) {

    document.getElementById(`${id}_logo_3`).classList.add("hidden");
    document.getElementById(`${id}_spinner_3`).classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectFileApp`).disabled = true;

    let result = await AjaxCall(url, file);

    result = await result.json();

    document.getElementById(`${id}_spinner_3`).classList.add("hidden");
    document.getElementById(`${id}_logo_3`).classList.remove("hidden");
    document.getElementById(`${id}_btnDisConnectFileApp`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_btnDisConnectFileApp`).classList.add("hidden");
        document.getElementById(`${id}_app`).src = Constants.DisConnectedLogo;
        document.getElementById(`${id}_app`).classList.remove("bg-primary");
        document.getElementById(`${id}_app`).classList.add("bg-danger");
        document.getElementById(`${id}_btnConnectFileApp`).classList.remove("hidden");

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function disConnectItemFromAppSharing(url, itemName, id, containerName, refreshUrl) {

    document.getElementById(`${id}_btnImage`).classList.add("hidden");
    document.getElementById(`${id}_btnSpinner`).classList.remove("hidden");

    let result = await AjaxCall(url, itemName);

    result = await result.json();

    document.getElementById(`${id}_btnSpinner`).classList.add("hidden");
    document.getElementById(`${id}_btnImage`).classList.remove("hidden");

    if (result.success) {

        const itemsContainer = document.getElementById(containerName);

        itemsContainer.removeChild(document.getElementById(id));

        if (itemsContainer.childElementCount == 0) {
            window.location.href = refreshUrl;

            return;
        }

        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

async function getQRCodeForItem(address, id) {

    document.getElementById(`${id}_qrCreateText`).classList.add("hidden");
    document.getElementById(`${id}_qrSpinner`).classList.remove("hidden");
    document.getElementById(`${id}_qrBtn`).disabled = true;

    let result = await AjaxCall(address, document.getElementById(`${id}_link`).href);

    result = await result.json();

    document.getElementById(`${id}_qrSpinner`).classList.add("hidden");
    document.getElementById(`${id}_qrCreateText`).classList.remove("hidden");
    document.getElementById(`${id}_qrBtn`).disabled = false;

    if (result.success) {

        document.getElementById(`${id}_img`).src = result.result
        document.getElementById(`${id}_qrBtn`).classList.add("hidden");
        document.getElementById(`${id}_qrDiv`).classList.remove("hidden");


        ShowSuccessToast("Success", result.message);
    }
    else {
        ShowErrorToast("Error", result.message);
    }
}

function removeQRCodeFromItem(id) {

    document.getElementById(`${id}_qrDiv`).classList.add("hidden");
    document.getElementById(`${id}_qrBtn`).classList.remove("hidden");
}

async function AjaxCall(address, itemName) {
    const forgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {

        var response = await fetch(address, {
            method: "POST",
            headers: {
                "Content-Type": 'application/json',
                "X-CSRF-TOKEN": forgeryToken
            },
            body: JSON.stringify(itemName),
            signal: AbortSignal.timeout(10000)
        });

    } catch (error) {

        if (error.name === 'AbortError') {
            return new Response(JSON.stringify({
                success: false,
                message: "Error - server not reachable"
            }));
        }

        return new Response(JSON.stringify({
            success: false,
            message: "Error while executing action"
        }));
    }

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