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
        SetFocusedWidget(testChoicer);
        AddWidget(new SeparatorWidget(visible: false));
        AddWidget(bottomText);
        AddWidget(new SeparatorWidget(visible: false));

        testChoicer.Confirmed += OnChosen;
        testChoicer.Cancelled += OnCancelled;
    }

    private void OnChosen(object? sender, ChoicerEventArgs e)
    {
        switch(e.choice)
        {
            // "Yes"
            case 0:
            case 1:
                TestPage newPage = new TestPage();
                newPage.pageNum = pageNum + 1;
                newPage.bottomText.content = "Page: " + newPage.pageNum.ToString();
                SwitchPage(newPage);
                testChoicer.chosen = false;
                break;

            // "No"
            case 2:
                GoToFirst();
                break;

            // "Fuck You!!!!"
            case 3:
                ExitAll();
                break;
        }
    }

    private void OnCancelled(object? sender, EventArgs e)
    {
        GoToPrevious();
    }
}