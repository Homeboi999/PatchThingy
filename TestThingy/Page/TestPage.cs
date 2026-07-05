using TestThingy.Widget;

namespace TestThingy.Page;

class TestPage : Page
{
    override public int MaxWidth => 60;
    public int pageNum = 1;

    TextWidget topText = new TextWidget("Test Page!!", Alignment.Center);
    ChoicerWidget testChoicer = new ChoicerWidget(["Yes", "Also Yes", "No", "Fuck You!!!!"]);
    public TextWidget bottomText = new TextWidget("wowie!!", Alignment.Center);

    public TestPage()
    {
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

    override public PageControl OnKeyInput(ConsoleKey inputKey)
    {
        PageControl result = PageControl.Continue;

        switch (testChoicer.OnKeyInput(inputKey))
        {
            // Confirm
            case ChoicerResult.Confirm:
                switch(testChoicer.curSelection)
                {
                    // "Yes"
                    case 0:
                    case 1:
                        TestPage newPage = new TestPage();
                        newPage.pageNum = pageNum + 1;
                        newPage.bottomText.content = "Page: " + newPage.pageNum.ToString();
                        result = SwitchPage(newPage);
                        break;

                    // "No"
                    case 2:
                        result = PageControl.GoToFirst;
                        break;

                    // "Fuck You!!!!"
                    case 3:
                        result = PageControl.ExitAll;
                        break;
                }
                break;

            // Cancel
            case ChoicerResult.Cancel:
                result = PageControl.GoToPrevious;
                break;
        }

        return result;
    }
}