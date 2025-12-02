namespace PseudoRun.Desktop.Services
{
    public interface IFileIOService
    {
        void OpenFile(string filename, string mode); // mode: "READ", "WRITE", "APPEND"
        void CloseFile(string filename);
        string ReadLine(string filename);
        void WriteLine(string filename, string data);
        bool IsEndOfFile(string filename);
        void ClearSandbox();
        string GetSandboxPath();
    }
}
