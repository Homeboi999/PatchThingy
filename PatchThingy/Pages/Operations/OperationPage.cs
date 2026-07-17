using System.Diagnostics;
using PatchThingy.Widgets;
using PatchThingy.Data;
using PatchThingy.Operations;

namespace PatchThingy.Pages.Operations;

abstract class OperationPage : Page
{
    override public int MaxWidth => mainGroup.visible ? 80 : 60;

    protected int chapter;
    protected bool allChapters;
    protected int chaptersDone;

    // Header
    protected WidgetGroup headerGroup = new WidgetGroup(visible: true);
    protected TextWidget headerText;
    protected virtual string modeText => "MODE TEXT";
    protected string fullHeader
    {
        get
        {
            if (allChapters)
            {
                return $"Deltarune - {modeText}... ({chaptersDone}/{ChapterPage.chapterCount} Complete)";
            }
            else
            {
                return $"Deltarune Chapter {chapter} - {modeText}...";
            }
        }
    }

    // DataFile loading screen
    protected WidgetGroup loadingGroup = new WidgetGroup(visible: true);
    protected TextWidget loadingText = new TextWidget(["Initializing..."], Alignment.Center);

    // Output Log
    protected WidgetGroup mainGroup = new WidgetGroup(visible: false);
    protected LogWidget mainLog = new LogWidget(8);

    // Results Choicer
    protected WidgetGroup resultGroup = new WidgetGroup(visible: false);
    protected ChoicerWidget resultChoicer = new ChoicerWidget(["Return to Start", "Exit PatchThingy"]);
    
    public OperationPage(int chapter, bool allChapters = false)
    {
        this.chapter = chapter;
        this.allChapters = allChapters;

        if (allChapters)
        {
            loadingGroup.visible = false;
            mainGroup.visible = true;
        }

        // Loading screen
        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        loadingGroup.AddWidget(loadingText);
        loadingGroup.AddWidget(new SeparatorWidget(visible: false));
        loadingGroup.AddWidget(new SeparatorWidget(visible: true));
        AddWidget(loadingGroup);

        // Loading screen
        mainGroup.AddWidget(mainLog);
        mainGroup.AddWidget(new SeparatorWidget(visible: true));
        AddWidget(mainGroup);

        // Header
        // headerGroup.AddWidget(new SeparatorWidget(visible: false));
        headerText = new TextWidget([fullHeader], Alignment.Center);
        headerGroup.AddWidget(headerText);
        // headerGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(headerGroup);

        // Results Choicer
        resultGroup.AddWidget(new SeparatorWidget(visible: true));
        resultGroup.AddWidget(new SeparatorWidget(visible: false));
        resultGroup.AddWidget(resultChoicer);
        resultGroup.AddWidget(new SeparatorWidget(visible: false));
        AddWidget(resultGroup);
        SetFocusedWidget(resultChoicer);

        // Event Setup
        resultChoicer.Confirmed += OnConfirm;
        resultChoicer.Cancelled += OnCancelled;
    }

    // Allow scrolling the log while choicer
    // is focused
    public override void OnKeyInput(ConsoleKey inputKey)
    {
        switch(inputKey)
        {
            case ConsoleKey.UpArrow:
                mainLog.Scroll(1);
                Draw();
                break;

            case ConsoleKey.DownArrow:
                mainLog.Scroll(-1);
                Draw();
                break;
                
            default:
                base.OnKeyInput(inputKey);
                break;
        }
    }

    // basic resultChoicer functions to be
    // changed if needed for specific cases
    protected virtual void OnConfirm(object? sender, ChoicerEventArgs e)
    {
        switch (resultChoicer.curSelection)
        {
            case 0:
                GoToFirst();
                break;

            case 1:
                ExitAll();
                break;
        }
    }
    protected virtual void OnCancelled(object? sender, EventArgs e)
    {
        GoToFirst();
    }

    // Convenient MessagePage creator
    // to help with error/warning popups
    protected bool CreateMessage(IReadOnlyList<string> messages, MessageType type)
    {
        // if debugger, don't
        if (Debugger.IsAttached)
        {
            return true;
        }

        // Make Message Page
        MessagePage messagePage = new MessagePage("", type);

        // Add message lines
        foreach (string message in messages)
        {
            messagePage.message.AddLine(message);
        }

        SwitchPage(messagePage);
        return messagePage.result;
    }
    protected bool CreateMessage(string message, MessageType type)
    {
        // if debugger, don't
        if (Debugger.IsAttached)
        {
            return true;
        }

        // Make Message Page
        MessagePage messagePage = new MessagePage(message, type);
        SwitchPage(messagePage);
        return messagePage.result;
    }
}