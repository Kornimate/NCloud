using NCloud.Users;

namespace NCloud.Models
{
    /// <summary>
    /// Class to store information about server side executed commands in cloud terminal
    /// </summary>
    public class ServerSideCommandContainer
    {
        public string Command { get; private set; }
        public int Parameters { get; private set; }
        public bool PrintResult { get; private set; }
        public bool CanUseInSingleLineMode { get; private set; }
        public Func<List<string>,CloudPathData,CloudUser, Task<object>> ExecutionAction { get; private set; }

        public ServerSideCommandContainer(string command, int parameters, bool printResult,bool canUseInSingleLineMode, Func<List<string>, CloudPathData, CloudUser, Task<object>> executionAction)
        {
            Command = command;
            Parameters = parameters;
            PrintResult = printResult;
            CanUseInSingleLineMode = canUseInSingleLineMode;
            ExecutionAction = executionAction;
        }
    }
}
