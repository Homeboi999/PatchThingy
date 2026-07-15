using TestThingy.Data;

namespace TestThingy.Operations;

interface IOperation
{
    public bool TryLoadData(DataType type, int chapter, out DataFile dataFile);
    public void AddLog(string message, MessageType type = MessageType.None);
    public void ErrorMessage(string message);
    public void ErrorMessage(IReadOnlyList<string> messages);
    public bool WarningMessage(string message);
    public bool WarningMessage(IReadOnlyList<string> messages);
    public void OnComplete();
}