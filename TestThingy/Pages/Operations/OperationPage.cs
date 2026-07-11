using TestThingy.Widget;
using TestThingy.Data;

namespace TestThingy.Pages.Operations;

abstract class OperationPage : Page
{
    override public int MaxWidth => 80;

    protected int chapter;
    protected bool allChapters;

    // Header
    protected WidgetGroup headerGroup = new WidgetGroup(visible: true);
    protected TextWidget headerText = new TextWidget([], Alignment.Center);

    // DataFile loading screen
    protected WidgetGroup loadingGroup = new WidgetGroup(visible: true);
    protected TextWidget loadingText = new TextWidget(["Loading..."], Alignment.Center);

    // Output Log
    protected WidgetGroup mainGroup = new WidgetGroup(visible: false);
    protected LogWidget mainLog = new LogWidget(8);

    // Results Choicer
    protected WidgetGroup resultGroup = new WidgetGroup(visible: false);
    protected ChoicerWidget resultChoicer = new ChoicerWidget(["Confirm"]);
    protected List<PageControl> resultOutcome = [PageControl.GoToPrevious];
    
    public OperationPage(int chapter, bool allChapters = false)
    {
        this.chapter = chapter;
        this.allChapters = allChapters;

        // Header
        headerGroup.AddWidget(headerText);
        headerGroup.AddWidget(new SeparatorWidget(visible: true));
        AddWidget(headerGroup);

        // Loading screen
        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        loadingGroup.AddWidget(loadingText);
        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(loadingGroup);

        // Loading screen
        mainGroup.AddWidget(mainLog);
        AddWidget(mainGroup);

        // Results Choicer
        resultGroup.AddWidget(new SeparatorWidget(visible: true));
        resultGroup.AddWidget(new SeparatorWidget(visible: false));
        resultGroup.AddWidget(resultChoicer);
        resultGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(resultGroup);
        SetFocusedWidget(resultChoicer);
    }

    protected bool TryLoadData(DataType type, int chapter, out DataFile? data)
    {
        loadingText.Clear();
        loadingText.AddLine($"Loading {DataFile.GetFileName(type)} for Chapter {chapter}...");
        Draw();

        bool loaded = DataFile.TryLoad(type, chapter, out data);

        if (!loaded)
        {
            // Make Error Page
            string errorMessage = $"Unable to locate {DataFile.GetFileName(type)} for Chapter {chapter}.";
            MessagePage errorPage = new MessagePage(errorMessage, MessageType.Error);
            SwitchPage(errorPage);
        }

        return loaded;
    }
}