class TerminalCommand {
    constructor() { }

    async eval(command, address) {

        var response = await AjaxCall(address, command);

        return await response.json();
    }
}