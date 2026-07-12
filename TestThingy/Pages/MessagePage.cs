using TestThingy.Widget;

namespace TestThingy.Pages;

class MessagePage : Page
{
    override public int MaxWidth => 60;

    // Text
    TextWidget header = new TextWidget([], Alignment.Center);
    public TextWidget message = new TextWidget([], Alignment.Center);

    // Choicer
    WidgetGroup confirmGroup = new WidgetGroup();
    ChoicerWidget confirmChoicer;
    List<PageControl> choiceResults = [];
    PageControl cancelResult;

    public MessagePage(string message, MessageType type = MessageType.None)
    {
        // Header + Choicer Setup
        SetHeaderType(type);
        SetConfirmChoices(GetDefaultChoicesByType(type));
        SetCancelResult(PageControl.GoToFirst);

        // Message Setup
        if (message.Length > 0)
        {
            this.message.AddLine(message);
        }

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
    public void SetConfirmChoices(IReadOnlyList<(string text, PageControl result)> choices)
    {
        List<string> texts = [];
        List<PageControl> results = [];

        foreach ((string text, PageControl result) choice in choices)
        {
            texts.Add(choice.text);
            results.Add(choice.result);
        }

        confirmChoicer = new ChoicerWidget(texts);
        choiceResults = results;
    }
    IReadOnlyList<(string text, PageControl result)> GetDefaultChoicesByType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Success:
            case MessageType.Error:
                return [("Return to Start", PageControl.GoToFirst), ("Exit PatchThingy", PageControl.ExitAll)];

            case MessageType.Warning:
                return [("Confirm", PageControl.GoToPrevious), ("Cancel", PageControl.GoToFirst)];

            // None
            default:
                return [("Return to Start", PageControl.GoToFirst), ("Exit PatchThingy", PageControl.ExitAll)];
        }
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