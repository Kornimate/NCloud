async function connectDirectoryToWeb(url, folder, id) {

    document.getElementById(`${id}_logo`).classList.add("hidden");
    document.getElementById(`${id}_spinner`).classList.remove("hidden");

    let result = await AjaxCall(url, folder);

    result = await result.json();

    console.log(result);

    document.getElementById(`${id}_spinner`).classList.add("hidden");
    document.getElementById(`${id}_logo`).classList.remove("hidden");

    console.log(result);
}
function connectDirectoryToApp(folder, id) {

}

function connectFileToWeb(file, id) {

}

function connectFileToApp(file, id) {

}

async function disConnectDirectoryFromWeb(folder, id) {

    document.getElementById(`${id}_logo`).classList.add("hidden");
    document.getElementById(`${id}_spinner`).classList.remove("hidden");

    let result = await AjaxCall(url, folder);

    result = await result.json();

    console.log(result);

    document.getElementById(`${id}_spinner`).classList.add("hidden");
    document.getElementById(`${id}_logo`).classList.remove("hidden");
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