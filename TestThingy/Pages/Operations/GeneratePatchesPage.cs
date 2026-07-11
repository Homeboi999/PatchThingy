using TestThingy.Widget;
using TestThingy.Data;

namespace TestThingy.Pages.Operations;

class GeneratePatchesPage : OperationPage
{    
    DataFile? active;
    DataFile? vanilla;

    public GeneratePatchesPage(int chapter, bool allChapters = false) : base(chapter, allChapters)
    {
        headerText.AddLine($"Deltarune Chapter {chapter} - Generating Patches...");
    }

    protected override void OnInitialize()
    {
        if (allChapters)
        {
            // TODO: this
            // Create MessagePage as placeholder
            string placeholderMessage = $"(Will generate patches for All Chapters)";
            MessagePage placeholderPage = new MessagePage(placeholderMessage);
            SwitchPage(placeholderPage);
        }
        else
        {
            // Load Active Data
            if (!TryLoadData(DataType.Active, chapter, out active))
            {
                return;
            }

            // Load Vanilla Data
            if (!TryLoadData(DataType.Vanilla, chapter, out vanilla))
            {
                return;
            }

            // Create MessagePage as placeholder
            string placeholderMessage = $"(Will generate patches for Ch. {chapter})";
            MessagePage placeholderPage = new MessagePage(placeholderMessage);
            SwitchPage(placeholderPage);
        }
    }
}