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

    async ExecuteClientSideCommand(command, address, terminal) {

        let response = await AjaxCall(address, command);

        response = await response.json();

        terminal.resume();

        if (!response.isClientSideCommand)
            return ["", false];

        if (!response.noErrorWithSyntax)
            return [response.errorMessage, true];

        //execute command

        return ["command executed successfully", true];
    }
}