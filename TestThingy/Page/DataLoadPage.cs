using TestThingy.Data;
using TestThingy.Widget;

namespace TestThingy.Page;

class DataLoadPage : Page
{
    override public int MaxWidth => 60;

    WidgetGroup loadingGroup = new WidgetGroup();
    public WidgetGroup messageGroup = new WidgetGroup();
    WidgetGroup choicerGroup = new WidgetGroup();

    ChoicerWidget missingChoicer;
    public TextWidget missingMessage = new TextWidget("", Alignment.Center);

    public DataFile? data;
    int chapter;
    DataType type;
    bool required;

    public DataLoadPage(DataType type, int chapter, bool required = true)
    {
        this.chapter = chapter;
        this.type = type;
        this.required = required;

        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        loadingGroup.AddWidget(new TextWidget($"Loading {DataFile.GetFileName(type)} for Chapter {chapter}...", Alignment.Center));
        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        loadingGroup.visible = true;
        AddWidget(loadingGroup);

        // Error Message
        messageGroup.AddWidget(new SeparatorWidget(visible: false));
        
        if (required)
        {
            messageGroup.AddWidget(new TextWidget("! ERROR !", Alignment.Center, ConsoleColor.Red));
        }
        else
        {
            messageGroup.AddWidget(new TextWidget("! WARNING !", Alignment.Center, ConsoleColor.Yellow));
        }

        missingMessage.content = $"Unable to locate {DataFile.GetFileName(type)} for Chapter {chapter}.";
        messageGroup.AddWidget(missingMessage);
        AddWidget(messageGroup);

        // Choicer
        if (required)
        {
            missingChoicer = new(["Return to Start", "Exit PatchThingy"]);
        }
        else
        {
            missingChoicer = new(["Confirm", "Cancel"]);
        }

        choicerGroup.AddWidget(new SeparatorWidget(visible: false));
        choicerGroup.AddWidget(missingChoicer);
        choicerGroup.AddWidget(new SeparatorWidget(visible: false));
        SetFocusedWidget(missingChoicer);
        AddWidget(choicerGroup);

        // event setup
        missingChoicer.Confirmed += OnChosen;
        missingChoicer.Cancelled += OnCancelled;
    }

    public override PageControl RunLoop()
    {
        // try to load data
        try
        {
            // Show a loading screen
            Draw();
            data = new(type, chapter);

            // Return to last page if successful
            GoToPrevious();
            return PageControl.GoToPrevious;
        }
        catch (FileNotFoundException)
        {
            loadingGroup.visible = false;
            messageGroup.visible = true;
            choicerGroup.visible = true;
            return base.RunLoop();
        }
    }

    // important = true
    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        if (required)
        {
            switch (e.choice)
            {
                // Return to Start
                case 0:
                    GoToFirst();
                    break;

                // Exit Patchthingy
                case 1:
                    ExitAll();
                    break;
            }
        }
        else
        {
            switch (e.choice)
            {
                // Confirm
                case 0:
                    GoToPrevious();
                    break;

                // Cancel
                case 1:
                    GoToFirst();
                    break;
            }
        }
    }

    // important = true
    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToFirst();
    }
}