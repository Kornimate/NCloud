class TerminalCommand {
    constructor(clientSideCommands) {
        this.clientSideCommands = clientSideCommands;
    }

    async ExecuteServerSideCommand(command, address) {

        var response = await AjaxCall(address, command);

        return await response.json();
    }

    get Commands() {

        return this.clientSideCommands;
    }

    //returns errorMessage (string) and stopExecution (bool) value

    async ExecuteClientSideCommand(command, address, terminal, errorAction) {

        let response = await AjaxCall(address, command);

        response = await response.json();

        console.log(response);

        terminal.resume();

        if (response.closeTerminal) {
            terminal.pause();

            setTimeout(() => window.location.href = errorAction, 3000);

            return [`[[b;red;black]${response.message}, redirected to dashboard]`, true];
        }

        if (!response.isClientSideCommand)
            return ["", false];

        if (!response.noErrorWithSyntax)
            return [response.errorMessage, true];

        document.getElementById("addElement").innerHTML = response.actionHTMLElement;

        document.getElementById(response.actionHTMLElementId).click();

        document.getElementById("addElement").innerHTML = "";

        return ["command executed successfully", true];
    }
}