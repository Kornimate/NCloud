class TerminalCommand {
    constructor() { }


    async ExecuteServerSideCommand(command, address) {

        var response = await AjaxCall(address, command);

        return await response.json();
    }

    async ExecuteClientSideCommand(command, address, terminal) {

        let response = await AjaxCall(address), command);

        response = await response.json();

        terminal.resume();

        if (!response.isClientSideCommand)
            return ["",false];

        if (!response.noErrorWithSyntax)
            return [response.errorMessage,true];

         //execute command

        return ["command executed successfully", true];
    }
}