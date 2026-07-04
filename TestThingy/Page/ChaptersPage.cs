using TestThingy.Widget;

namespace TestThingy.Page;

class ChaptersPage : Page
{
    override public int MaxWidth => 60;
    
    public TextWidget chapterPrompt = new TextWidget("Select a Deltarune chapter to patch", Alignment.Center);
    bool onlyChapters;
    
    ChoicerWidget chapterChoicer;
    string[] chapterArray
    {
        get
        {
            List<string> chapterList = ["Chapter 1", "Chapter 2", "Chapter 3", "Chapter 4", "Chapter 5"];

            if (!onlyChapters)
            {
                chapterList.Insert(0, "All Chapters");
            }

            return chapterList.ToArray();
        }
    }

    public ChaptersPage(bool onlyChapters = false)
    {
        this.onlyChapters = onlyChapters;
        
        chapterChoicer = new ChoicerWidget(chapterArray);

        // main label
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(chapterPrompt);
        AddWidget(new SeparatorWidget(visible: false));
        
        AddWidget(new SeparatorWidget(visible: true));

        // choicer
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(chapterChoicer);
        AddWidget(new SeparatorWidget(visible: false));
    }

    override public PageControl OnKeyInput(ConsoleKey inputKey)
    {
        PageControl result = PageControl.Continue;

        switch (inputKey)
        {
            // Choicer Selection
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
                chapterChoicer.ChangeSelection(inputKey);
                break;

            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                // TODO: make this better (prob a diff kind of page tbh)
                if (onlyChapters)
                {
                    TestPage newPage = new TestPage();
                    newPage.bottomText.content = "(Will start generating patches for all chapters)";
                    result = SwitchPage(newPage);
                }
                else
                {
                    result = SwitchPage(new ActionPage(chapterChoicer.curSelection + (onlyChapters ? 1 : 0)));
                }

                break;

            // Cancel
            case ConsoleKey.X:
            case ConsoleKey.Escape:
                result = PageControl.GoToPrevious;
                break;
        }

        return result;
    }
}