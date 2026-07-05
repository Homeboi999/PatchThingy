using TestThingy.Widget;

namespace TestThingy.Page;

class GlobalChapterPage : ChapterPage
{
    protected override string chapterPrompt => "Which chapter should Global Patches be generated from?";

    public GlobalChapterPage() : base(onlyChapters: true)
    {
    }

    protected override void OnChapterSelected(int chapter)
    {
        TestPage newPage = new TestPage();
        newPage.bottomText.content = "(Will start generating patches for all chapters)";
        SwitchPage(newPage);
    }
}