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

    public ChaptersPage(PageManager manager, bool onlyChapters = false) : base(manager)
    {
        this.onlyChapters = onlyChapters;
        
        chapterChoicer = new ChoicerWidget(chapterArray);

        // title
        AddWidget(new TextWidget(manager.mainTitle, Alignment.Center));

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

    override public void OnKeyInput(ConsoleKey inputKey)
    {
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
                    TestPage newPage = new TestPage(manager);
                    newPage.bottomText.content = "(Will start generating patches for all chapters)";
                    manager.AddPage(newPage);
                }
                else
                {
                    manager.AddPage(new ActionPage(manager, chapterChoicer.curSelection + (onlyChapters ? 1 : 0)));
                }

                break;
        }
    }
}