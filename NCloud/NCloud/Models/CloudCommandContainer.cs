namespace NCloud.Models
{
    public class CloudCommandContainer
    {
        public string Command { get; private set; }
        public int Parameters { get; private set; }
        public bool PrintResult { get; private set; }
        public Func<List<string>, Task<string>> ExecutionAction { get; private set; }

        public CloudCommandContainer(string command, int parameters, bool printResult, Func<List<string>, Task<string>> executionAction)
        {
            Command = command;
            Parameters = parameters;
            PrintResult = printResult;
            ExecutionAction = executionAction;
        }
    }
}
