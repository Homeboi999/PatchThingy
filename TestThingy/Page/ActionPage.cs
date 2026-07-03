using TestThingy.Widget;

namespace TestThingy.Page;

class ActionPage : Page
{
    override public int MaxWidth => 60;

    int chapter = 0;
    bool allChapters => chapter == 0;

    string actionPrompt = "Please select an action for ";
    string singleChapterText = "Deltarune Chapter ";
    string allChapterText = "all chapters of Deltarune";

    ChoicerWidget actionChoicer = new ChoicerWidget(["Generate new patches", "Apply existing patches", "Manage Data Files"], ChoicerType.List);

    public ActionPage(PageManager manager, int chapter) : base(manager)
    {
        // check chapter number
        this.chapter = chapter;

        // change prompt accordingly
        if (allChapters)
        {
            actionPrompt = actionPrompt + allChapterText;
        }
        else
        {
            actionPrompt = actionPrompt + singleChapterText + chapter;
        }

        // title
        AddWidget(new TextWidget(manager.mainTitle, Alignment.Center));

        // main prompt
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new TextWidget(actionPrompt, Alignment.Center));
        AddWidget(new SeparatorWidget(visible: false));
        
        AddWidget(new SeparatorWidget(visible: true));

        // choicer
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(actionChoicer);
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
                actionChoicer.ChangeSelection(inputKey);
                break;

            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                switch(actionChoicer.curSelection)
                {
                    // Generate
                    case 0:
                        if (allChapters)
                        {
                            ChaptersPage newPage = new ChaptersPage(manager, true);
                            newPage.chapterPrompt.content = "Which chapter should Global Patches be generated from?";
                            manager.AddPage(newPage);
                        }
                        else
                        {
                            TestPage newPage = new TestPage(manager);
                            newPage.bottomText.content = $"(Will start generating patches for Ch. {chapter})";
                            manager.AddPage(newPage);
                        }
                        break;

                    // Apply
                    case 1:
                        TestPage newPage2 = new TestPage(manager);
                        newPage2.bottomText.content = $"(Will apply patches to Ch. {chapter})";
                        manager.AddPage(newPage2);
                        break;

                    // Manage Data
                    case 2:
                        ManageDataPage dataPage = new ManageDataPage(manager, chapter);
                        manager.AddPage(dataPage);
                        break;
                }
                break;
        }
    }
}