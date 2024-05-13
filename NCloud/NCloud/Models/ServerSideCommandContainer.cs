namespace NCloud.Models
{
    public class ServerSideCommandContainer
    {
        public string Command { get; private set; }
        public int Parameters { get; private set; }
        public bool PrintResult { get; private set; }
        public bool CanUseInSingleLineMode { get; private set; }
        public Func<List<string>, Task<object>> ExecutionAction { get; private set; }

        public ServerSideCommandContainer(string command, int parameters, bool printResult,bool canUseInSingleLineMode, Func<List<string>, Task<object>> executionAction)
        {
            Command = command;
            Parameters = parameters;
            PrintResult = printResult;
            CanUseInSingleLineMode = canUseInSingleLineMode;
            ExecutionAction = executionAction;
        }
    }
}
