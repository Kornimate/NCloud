namespace NCloud.Services
{
    public interface ICloudTerminalService
    {
        string ExecuteCommand(string? input);
        void ExecuteSingleLineCommand(string? command);
    }
}
