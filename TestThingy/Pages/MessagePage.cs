using TestThingy.Widget;

namespace TestThingy.Pages;

class MessagePage : Page
{
    override public int MaxWidth => 60;

    // Text
    TextWidget header = new TextWidget([], Alignment.Center);
    TextWidget message = new TextWidget([], Alignment.Center);

    // Choicer
    WidgetGroup confirmGroup = new WidgetGroup();
    ChoicerWidget confirmChoicer;
    List<PageControl> choiceResults = [];
    PageControl cancelResult;

    public MessagePage(string message, MessageType type = MessageType.None)
    {
        // Header Setup
        SetHeaderType(type);

        // Default Choicer Setup
        switch (type)
        {
            case MessageType.Error:
                SetConfirmChoices(["Return to Start", "Exit PatchThingy"], [PageControl.GoToFirst, PageControl.ExitAll]);
                SetCancelResult(PageControl.GoToFirst);
                break;

            case MessageType.Warning:
                SetConfirmChoices(["Confirm", "Cancel"], [PageControl.GoToPrevious, PageControl.GoToFirst]);
                SetCancelResult(PageControl.GoToFirst);
                break;

            case MessageType.Success:
                SetConfirmChoices(["Return to Start", "Exit PatchThingy"], [PageControl.GoToFirst, PageControl.ExitAll]);
                SetCancelResult(PageControl.GoToFirst);
                break;

            // None
            default:
                SetConfirmChoices(["Return to Start", "Exit PatchThingy"], [PageControl.GoToFirst, PageControl.ExitAll]);
                SetCancelResult(PageControl.GoToFirst);
                break;
        }

        // Message Setup
        this.message.AddLine(message);

        // Add header + message
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(header);
        AddWidget(this.message);
        AddWidget(new SeparatorWidget(visible: false));

        // Add confirm choicer
        confirmGroup.AddWidget(new SeparatorWidget(visible: true));
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        confirmGroup.AddWidget(confirmChoicer!);
        SetFocusedWidget(confirmChoicer!);
        confirmGroup.AddWidget(new SeparatorWidget(visible: false));
        confirmGroup.visible = true;
        AddWidget(confirmGroup);


        confirmChoicer!.Confirmed += OnChosen;
        confirmChoicer.Cancelled += OnCancelled;
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        CallControlFunction(choiceResults[confirmChoicer.curSelection]);
    }
    private void OnCancelled(object? sender, EventArgs e)
    {
        CallControlFunction(cancelResult);
    }

    void CallControlFunction(PageControl control)
    {
        switch (control)
        {
            case PageControl.GoToPrevious:
                GoToPrevious();
                break;

            case PageControl.GoToFirst:
                GoToFirst();
                break;
                
            case PageControl.ExitAll:
                ExitAll();
                break;
        }
    }

    // Functions for changing the choices in the
    // confirmChoicer and setting PageControl
    public void SetConfirmChoices(IReadOnlyList<string> choices, IReadOnlyList<PageControl> results)
    {
        confirmChoicer = new ChoicerWidget(choices);
        choiceResults = results.ToList();
    }
    public void SetConfirmChoices(IReadOnlyList<string> choices, PageControl result)
    {
        confirmChoicer = new ChoicerWidget(choices);
        choiceResults = [result];
    }

    // Same as above but for cancelling the Choicer.
    public void SetCancelResult(PageControl result)
    {
        cancelResult = result;
    }

    // Change the header based on type
    public void SetHeaderType(MessageType type)
    {
        header.Clear();

        switch (type)
        {
            case MessageType.Error:
                header.color = ConsoleColor.Red;
                header.AddLine("! ERROR !");
                break;

            case MessageType.Warning:
                header.color = ConsoleColor.Yellow;
                header.AddLine("! WARNING !");
                break;

            case MessageType.Success:
                header.color = ConsoleColor.Green;
                header.AddLine("Success!");
                break;

            // None
            default:
                header.color = ConsoleColor.White;
                break;
        }
    }
}