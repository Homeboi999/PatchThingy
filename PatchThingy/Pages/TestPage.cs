using PatchThingy.Widgets;

namespace PatchThingy.Pages;

class TestPage : Page
{
    override public int MaxWidth => 80;
    LogWidget testLog = new LogWidget(8);
    ChoicerWidget testChoicer = new ChoicerWidget(["Add Entries", "Clear"]);

    public TestPage()
    {
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new TextWidget(["Test Page"]));
        AddWidget(new TextWidget(["LogWidget"]));
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new SeparatorWidget(visible: true));
        AddWidget(testLog);
        AddWidget(new SeparatorWidget(visible: true));
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(testChoicer);
        SetFocusedWidget(testChoicer);
        AddWidget(new SeparatorWidget(visible: false));

        // Event Setup
        testChoicer.Confirmed += OnChosen;
        testChoicer.Cancelled += OnCancelled;
    }

    public override void OnKeyInput(ConsoleKey inputKey)
    {
        switch(inputKey)
        {
            case ConsoleKey.UpArrow:
                testLog.Scroll(1);
                Draw();
                break;

            case ConsoleKey.DownArrow:
                testLog.Scroll(-1);
                Draw();
                break;
                
            default:
                base.OnKeyInput(inputKey);
                break;
        }
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        testChoicer.chosen = false;
        switch(testChoicer.curSelection)
        {
            // Add Entries
            case 0:
                testLog.Add($"#{testLog.logLines} - None", MessageType.None);
                testLog.Add($"#{testLog.logLines} - Warning", MessageType.Warning);
                testLog.Add($"#{testLog.logLines} - Error", MessageType.Error);
                testLog.Add($"#{testLog.logLines} - Success", MessageType.Success);
                break;

            // Clear
            case 1:
                testLog.Clear();
                break;
        }
    }
    private void OnCancelled(object? sender, EventArgs e)
    {
        ExitAll();
    }

}