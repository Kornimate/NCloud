class TerminalCommand {
    constructor() {
        this.clientSideCommands = ["download-file", "download-dir"];
    }


    async eval(command, address) {

        var response = await AjaxCall(address, command);

        return await response.json();
    }

    ExecuteClientSideCommand(command) {
        if (!this.clientSideCommands.includes(command)) {
            return false;
        }

        return true;
        //execute command
    }
}