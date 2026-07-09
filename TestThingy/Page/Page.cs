using System.Reflection;
using TestThingy.Widget;

namespace TestThingy.Page;

abstract class Page
{
    // Title bar that's the same for
    // no matter what page we're on
    public readonly static string versionNum = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";
    public readonly string mainTitle = $"╾─╴╴╴  PatchThingy Rewrite Test  ╶╶╶─╼";

    // Variables that each page will need
    List<Widget.Widget> widgets = [];
    // TODO: base type for focusable widgets
    protected ChoicerWidget? FocusedWidget { get; private set; } = null;
    Page? lastPage;
    PageControl nextPageControl = PageControl.Continue;

    public Page(Page? lastPage = null)
    {
        this.lastPage = lastPage;
    }

    abstract public int MaxWidth { get; }

    public virtual PageControl RunLoop()
    {
        while (true)
        {
            Draw();

            ConsoleKeyInfo input = Console.ReadKey(true);    
            OnKeyInput(input.Key);

            PageControl result = nextPageControl;
            nextPageControl = PageControl.Continue;

            switch(result)
            {
                case PageControl.GoToPrevious:
                    return PageControl.Continue;

                case PageControl.ExitAll:
                    return result;

                case PageControl.GoToFirst:
                    if (lastPage is not null)
                    {
                        return result;
                    }
                    break;
            }
        }
    }

    public virtual void OnKeyInput(ConsoleKey inputKey)
    {
        FocusedWidget?.OnKeyInput(inputKey);
    }

    public void SetFocusedWidget(ChoicerWidget newWidget)
    {
        if (FocusedWidget is not null)
        {
            FocusedWidget.focused = false;
        }

        FocusedWidget = newWidget;
        FocusedWidget.focused = true;
    }

    protected void SwitchPage(Page nextPage)
    {
        nextPage.lastPage = this;
        nextPageControl = nextPage.RunLoop();
    }

    protected void GoToPrevious()
    {
        nextPageControl = PageControl.GoToPrevious;
    }
    protected void GoToFirst()
    {
        nextPageControl = PageControl.GoToFirst;
    }
    protected void ExitAll()
    {
        nextPageControl = PageControl.ExitAll;
    }
    
    public void Draw()
    {
        Console.Write("\x1b[?2026h"); // stop display
        Console.Clear();
        var boxWidth = Math.Min(MaxWidth, Console.BufferWidth - 2);
        var boxHeight = Math.Max(0, widgets.Select(widget => widget.LineCount).Sum());
        var boxX = Math.Clamp((Console.BufferWidth - boxWidth) / 2 - 1, 0, Math.Max(Console.BufferWidth - 1, 0));
        var boxY = Math.Max((Console.BufferHeight - boxHeight) / 2 - 1, 0);

        // update box position, without going OoB
        DrawContext box = new DrawContext(boxX, boxY, boxWidth, boxHeight);

        // assemble box as single string
        // so it writes instantly
        DrawContext.MoveCursor(0, box.y);
        
        string boxString = "";
        boxString += box.AssembleRow('┏', '━', '┓');
        for (int i = 0; i < box.height; i++)
        {
            boxString += box.AssembleRow('┃', ' ', '┃');
        }
        boxString += box.AssembleRow('┗', '━', '┛');

        // draw box to screen
        Console.Write(boxString);

        // write title on top of box
        DrawContext.MoveCursor(box.AlignPosition(Alignment.Center) - (mainTitle.Length / 2), box.y);
        Console.Write(mainTitle);

        // Potential future version
        int line = 1;
        foreach (Widget.Widget widget in widgets)
        {
            widget.Draw(box, line);
            line += widget.LineCount;
        }

        Console.Write("\x1b[?2026l"); // start display
    }

    // Add a text widget to the menu
    public void AddWidget (Widget.Widget newWidget)
    {
        widgets.Add(newWidget);
    }
    public void InsertWidget (int index, Widget.Widget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets.Insert(index, newWidget);
    }
    public void ReplaceWidget(int index, Widget.Widget newWidget)
    {
        if (index > widgets.Count || index < 0)
        {
            return;
        }
        widgets[index] = newWidget;
    }
}