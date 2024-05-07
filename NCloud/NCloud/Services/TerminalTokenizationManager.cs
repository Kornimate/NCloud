using Castle.Core;
using NCloud.ConstantData;

namespace NCloud.Services
{
    public class TerminalTokenizationManager
    {
        public static Pair<string,List<string>> Tokenize(string command)
        {
            int commandParameterSeparator = command.IndexOf(Constants.TerminalWhiteSpace);

            string commandWord = command.Substring(0, commandParameterSeparator).Trim().TrimStart(Constants.SingleLineCommandMarker);
            string parametersString = command.Substring(commandParameterSeparator + 1).Trim();

            List<string> parameters = parametersString.Split(Constants.TerminalStringMarker,StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x != String.Empty).ToList();

            return new Pair<string,List<string>>(commandWord, parameters);
        }

        public static void CheckCorrectnessOfCommand(string command)
        {
            if (command.IndexOf(Constants.TerminalWhiteSpace) == -1)
            {
                throw new InvalidDataException("No space after command");
            }

            if (command.Count(x => x == Constants.TerminalStringMarker) % 2 != 0)
            {
                throw new InvalidDataException("String markers incorrect");
            }
        }

        public static void CheckCorrectnessOfSingleLineCommand(string command)
        {
            if (command.StartsWith(Constants.SingleLineCommandMarker))
            {
                throw new InvalidDataException("command should start with '@'");
            }

            CheckCorrectnessOfCommand(command);
        }
    }
}
