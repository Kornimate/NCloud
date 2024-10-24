﻿const TIMEOUTMS = 30000;
async function PeriodicSave(file, content, encoding, address) {

    const spinner = document.getElementById("autosavespinner");
    const text = document.getElementById("statustext");

    spinner?.classList.remove("hidden");
    text.innerHTML = "Save in progress...";

    try {
        let response = await SaveData(file, content, encoding, address);

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

    } catch (e) {

        spinner?.classList.add("hidden");

        text.innerHTML = "Error while saving document...";
    }

    setTimeout(async () => await PeriodicSave(file, content, encoding, address), TIMEOUTMS);
}

async function UserSave(file, content, encoding, address) {

    const spinner = document.getElementById("usersavespinner");
    const btnSave = document.getElementById("usersavebtn");

    spinner?.classList.remove("hidden");
    btnSave.disabled = true;

    try {
        let response = await SaveData(file, content, encoding, address);

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
    } catch (e) {
        spinner?.classList.add("hidden");
        btnSave.disabled = false;

        ShowErrorToast("Error", response.message);
    }
}

async function SaveData(file, content, encoding, address) {

    const forgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    try {

        var response = await fetch(address, {
            method: "POST",
            headers: {
                "Content-Type": 'application/json',
                "X-CSRF-TOKEN": forgeryToken
            },
            body: JSON.stringify({
                file: file,
                content: content,
                encoding: encoding
            }),
            signal: AbortSignal.timeout(10000)
        });

        return response;

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
}