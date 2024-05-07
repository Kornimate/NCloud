using Castle.Core;
using NCloud.ConstantData;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NCloud.Services
{
    public class CloudTerminalTokenizationManager
    {
        public static Pair<string, List<string>> Tokenize(string command, string userId)
        {

            string commandWord = command;
            string parametersString = String.Empty;

            int commandParameterSeparator = command.IndexOf(Constants.TerminalWhiteSpace);

            if (commandParameterSeparator != -1)
            {
                commandWord = command.Substring(0, commandParameterSeparator).Trim().TrimStart(Constants.SingleLineCommandMarker);
                parametersString = command.Substring(commandParameterSeparator + 1).Trim(); 
            }

            List<string> parameters = parametersString.Split(Constants.TerminalStringMarker, StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    x = x.Trim();
                    
                    if (x.StartsWith(Constants.AbsolutePathMarker))
                    {
                        x = x.Insert((x.Contains(Constants.PathSeparator) ? x.IndexOf(Constants.PathSeparator) + 1 : x.Length), $"{userId}/");
                    }

                    x = x.Replace(Constants.PathSeparator, Path.DirectorySeparatorChar);

                    return x;

                }).Where(x => x != String.Empty).ToList();

            return new Pair<string, List<string>>(commandWord, parameters);
        }

        public static void CheckCorrectnessOfCommand(string command)
        {
            if (command.Count(x => x == Constants.TerminalStringMarker) % 2 != 0)
            {
                throw new InvalidDataException("String markers incorrect");
            }

        }

        public static void CheckCorrectnessOfSingleLineCommand(string command)
        {
            if (!command.StartsWith(Constants.SingleLineCommandMarker))
            {
                throw new InvalidDataException("command should start with '@'");
            }

            CheckCorrectnessOfCommand(command);
        }
    }
}
