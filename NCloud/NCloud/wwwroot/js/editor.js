const TIMEOUTMS = 30000;
async function PeriodicSave(file, content, address) {

    const spinner = document.getElementById("autosavespinner");
    const text = document.getElementById("statustext");

    spinner?.classList.remove("hidden");
    text.innerHTML = "Save in progress...";

    let response = await SaveData(file, content, address);

    response = await response.json();

    spinner?.classList.add("hidden");

    if (response.redirection !== "") {
        window.location.href = response.redirection;
    }

    if (response.success) {
        text.innerHTML = `Last saved: ${new Date().toLocaleString()}`;
    }
    else {
        text.innerHTML = "Error while saving document...";
    }

    setTimeout(async () => await PeriodicSave(file, content, address), TIMEOUTMS);
}

async function UserSave(file, content, address) {

    const spinner = document.getElementById("usersavespinner");
    const btnSave = document.getElementById("usersavebtn");

    spinner?.classList.remove("hidden");
    btnSave.disabled = true;

    let response = await SaveData(file, content, address);

    response = await response.json();

    spinner?.classList.add("hidden");
    btnSave.disabled = false;

    if (response.redirection !== "") {
        window.location.href = response.redirection;
    }

    if (response.success) {
        document.getElementById("statustext").innerHTML = `Last saved: ${new Date().toLocaleString()}`;

        ShowSuccessToast("Success", response.message);
    }
    else {
        ShowErrorToast("Error", response.message);
    }
}

async function SaveData(file, content, address) {

    const forgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    var response = await fetch(address, {
        method: "POST",
        headers: {
            "Content-Type": 'application/json',
            "X-CSRF-TOKEN": forgeryToken
        },
        body: JSON.stringify({
            file: file,
            content: content
        })
    });

    return response;
}