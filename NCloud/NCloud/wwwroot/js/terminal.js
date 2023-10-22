class TerminalCommand {
    constructor() { }

    async eval(command, address) {
        address = address.replace("@", command);
        var response = await fetch(address);
        return response.text();
    }
}