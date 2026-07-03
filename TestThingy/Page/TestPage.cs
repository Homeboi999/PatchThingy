using TestThingy.Widget;

namespace TestThingy.Page;

class TestPage : Page
{
    override public int MaxWidth => 60;

    TextWidget topText = new TextWidget("Test Page!!", Alignment.Center);
    ChoicerWidget testChoicer = new ChoicerWidget(["Yes", "Also Yes", "No", "Fuck You!!!!"]);
    public TextWidget bottomText = new TextWidget("wowie!!", Alignment.Center);

    public TestPage(PageManager manager) : base(manager)
    {
        AddWidget(new TextWidget(manager.mainTitle, Alignment.Center));
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(topText);
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(new SeparatorWidget(visible: true));
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(testChoicer);
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(bottomText);
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
                testChoicer.ChangeSelection(inputKey);
                break;

            // Confirm
            case ConsoleKey.Z:
            case ConsoleKey.Enter:
                switch(testChoicer.curSelection)
                {
                    // "Yes"
                    case 0:
                    case 1:
                        TestPage newPage = new TestPage(manager);
                        newPage.bottomText.content = "Page: " + (manager.pageCount + 1).ToString();
                        manager.AddPage(newPage);
                        break;

                    // "No"
                    case 2:
                        manager.RemovePage();
                        break;

                    // "Fuck You!!!!"
                    case 3:
                        manager.Exit();
                        break;
                }
                break;
        }
    }
}