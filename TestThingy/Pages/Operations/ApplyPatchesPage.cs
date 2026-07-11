using TestThingy.Widget;
using TestThingy.Data;

namespace TestThingy.Pages.Operations;

class ApplyPatchesPage : OperationPage
{
    DataFile? vanilla;

    public ApplyPatchesPage(int chapter, bool allChapters = false) : base(chapter, allChapters)
    {
        headerText.AddLine($"Deltarune Chapter {chapter} - Applying Patches...");
    }

    protected override void OnInitialize()
    {
        if (allChapters)
        {
            // TODO: this
            // Create MessagePage as placeholder
            string placeholderMessage = $"(Will apply patches to All Chapters)";
            MessagePage placeholderPage = new MessagePage(placeholderMessage);
            SwitchPage(placeholderPage);
        }
        else
        {
            // Load Vanilla Data
            if (!TryLoadData(DataType.Vanilla, chapter, out vanilla, customMessage: true))
            {
                // Add warning to log
                mainLog.Add($"Unable to locate {DataFile.GetFileName(DataType.Vanilla)}", MessageType.Warning);

                // Make Warning Page instead
                // of default Error page
                string errorMessage = $"Unable to locate {DataFile.GetFileName(DataType.Vanilla)} for Chapter {chapter}.";
                MessagePage errorPage = new MessagePage(errorMessage, MessageType.Warning);
                errorPage.message.AddLine($"Create Vanilla Data from Active Data? ({DataFile.GetFileName(DataType.Active)})");
                SwitchPage(errorPage);

                // TODO: make a better way to read the output
                // of another page's choicer
                if (CheckPageControl(out PageControl result))
                {
                    GoToFirst();
                    return;
                }

                // Load Active Data instead
                if (!TryLoadData(DataType.Active, chapter, out vanilla))
                {
                    return;
                }
            }

            // Create MessagePage as placeholder
            string placeholderMessage = $"(Will apply patches to Ch. {chapter})";
            MessagePage placeholderPage = new MessagePage(placeholderMessage);
            SwitchPage(placeholderPage);
        }
    }
}