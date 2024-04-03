class TerminalCommand {
    constructor() { }

    async eval(command, address, forgeryToken) {
        var response = await fetch(address, {
            method: "POST",
            headers: {
                "Content-Type": 'application/json',
                "X-CSRF-TOKEN": forgeryToken
            },
            body: JSON.stringify(command)
        });
        return response.text();
    }
}