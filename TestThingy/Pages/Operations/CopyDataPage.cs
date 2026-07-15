using TestThingy.Widget;
using TestThingy.Data;

namespace TestThingy.Pages.Operations;

class CopyDataPage : OperationPage
{
    DataType sourceType;
    DataType destType;
    protected override string modeText { get; }

    string confirmMessage1;
    string confirmMessage2;

    public CopyDataPage(OperationType mode, int chapter, bool allChapters = false) : base(chapter, allChapters)
    {
        switch (mode)
        {
            case OperationType.LoadVanilla:
                sourceType = DataType.Vanilla;
                destType = DataType.Active;
                modeText = $"Loading {DataFile.GetFileName(sourceType)}";
                break;

            case OperationType.SaveVanilla:
                sourceType = DataType.Active;
                destType = DataType.Vanilla;
                modeText = $"Saving {DataFile.GetFileName(destType)}";
                break;

            case OperationType.LoadBackup:
                sourceType = DataType.Backup;
                destType = DataType.Active;
                modeText = $"Loading {DataFile.GetFileName(sourceType)}";
                break;

            case OperationType.SaveBackup:
                sourceType = DataType.Active;
                destType = DataType.Backup;
                modeText = $"Saving {DataFile.GetFileName(destType)}";
                break;

            default:
                sourceType = DataType.Active;
                destType = DataType.Active;
                modeText = "Failsafe";
                break;
        }
        
        headerText.Clear();
        headerText.AddLine(fullHeader);
        headerGroup.visible = allChapters;

        confirmMessage1 = $"This will overwrite the {DataFile.GetFileName(destType)} with";
        confirmMessage2 = $"{DataFile.GetFileName(sourceType)} for";

        if (allChapters)
        {
            confirmMessage2 += $" All Chapters. Continue?";
        }
        else
        {
            confirmMessage2 += $" Chapter {chapter}. Continue?";
        }

        mainLog.height = ChapterPage.chapterCount;
    }

    protected override void OnInitialize()
    {
        // Create MessagePage as placeholder
        MessagePage confirmPage = new MessagePage(confirmMessage1, MessageType.Warning);
        confirmPage.message.AddLine(confirmMessage2);
        SwitchPage(confirmPage);

        // TODO: make a better way to read the output
        // of another page's choicer
        if (CheckPageControl(out PageControl result))
        {
            GoToPrevious();
            return;
        }

        if (allChapters)
        {
            // Page Setup
            loadingGroup.visible = false;
            mainGroup.visible = true;

            string message;

            for (int i = 1; i <= ChapterPage.chapterCount; i++)
            {
                if (MoveData(i))
                {
                    // Success Log
                    message = $"Chapter {i} - Sucessfully replaced {DataFile.GetFileName(destType)} with {DataFile.GetFileName(sourceType)}!";
                    mainLog.Add(message, MessageType.Success);
                    Draw();
                }
                else
                {
                    // Error Log
                    message = $"Chapter {i} - Failed to load {DataFile.GetFileName(sourceType)}.";
                    mainLog.Add(message, MessageType.Error);
                    Draw();
                }

                // Update Header
                chaptersDone++;
                headerText.Clear();
                headerText.AddLine($"Deltarune - {modeText}... ({chaptersDone}/{ChapterPage.chapterCount} Complete)");
            }

            // show resultChoicer and
            // continue to RunLoop()
            resultGroup.visible = true;
            return;
        }
        else
        {
            if (MoveData(chapter))
            {
                // Success Page
                MessagePage successMessage = new MessagePage(confirmMessage1, MessageType.Success);
                successMessage.message.AddLine(confirmMessage2);
                SwitchPage(successMessage);
            }
            else
            {
                // Error Page
                string errorMessage = $"Could not find {DataFile.GetFileName(sourceType)} for Chapter {chapter}.";
                MessagePage errorPage = new MessagePage(errorMessage, MessageType.Error);
                SwitchPage(errorPage);
            }
        }
    }

    bool MoveData(int chapter)
    {
        string sourcePath = Path.Combine(Config.current.GamePath, $"./chapter{chapter}_windows", DataFile.GetFileName(sourceType));
        string destPath = Path.Combine(Config.current.GamePath, $"./chapter{chapter}_windows", DataFile.GetFileName(destType));

        // try to copy, otherwise make an error for the menu
        try
        {
            File.Copy(sourcePath, destPath, true);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }
}