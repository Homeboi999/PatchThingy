using PatchThingy.Widgets;
using PatchThingy.Data;
using PatchThingy.Operations;

namespace PatchThingy.Pages.Operations;

class GeneratePatchesPage : OperationPage
{
    protected override string modeText => "Generating Patches";
    GeneratePatches operation;

    public GeneratePatchesPage(int chapter, bool allChapters = false) : base(chapter, allChapters)
    {
        GeneratePatchesBridge bridge = new(this);
        operation = new(bridge);
    }

    protected override void OnInitialize()
    {
        if (allChapters)
        {
            // for loop is handled by the operation
            // so that the list of things carries over
            operation.AllChapters(globalChapter: chapter);
        }
        else
        {
            operation.SingleChapter(chapter);
        }

        // Show the ResultGroup after everything
        resultGroup.visible = true;
        Draw();
    }

    class GeneratePatchesBridge(GeneratePatchesPage page) : IOperation
    {
        public bool TryLoadData(DataType type, int chapter, out DataFile dataFile)
        {
            string loadText = $"Loading {DataFile.GetFileName(type)} for Chapter {chapter}...";

            if (page.loadingGroup.visible)
            {
                // Change Loading Text
                page.loadingText.Clear();
                page.loadingText.AddLine(loadText);
                page.Draw();

                // Show MainLog
                page.mainLog.Add(loadText);
                page.mainGroup.visible = true;
                page.loadingGroup.visible = false;
            }
            else
            {
                // Add Loading Text
                AddLog(loadText);
            }

            // if i try to read the null DataFile,
            // its my fault lmao
            bool loaded = DataFile.TryLoad(type, chapter, out dataFile);

            return loaded;
        }
        
        public void AddLog(string message, MessageType type = MessageType.None)
        {
            page.mainLog.Add(message, type);
            page.Draw();
        }
        
        public void ErrorMessage(string message)
        {
            if (!page.allChapters)
            {
                // Make Error Page
                page.CreateMessage(message, MessageType.Error);
            }

            // Add to log
            AddLog(message, MessageType.Error);
        }
        public void ErrorMessage(IReadOnlyList<string> messages)
        {
            if (!page.allChapters)
            {
                // Make Error Page
                page.CreateMessage(messages, MessageType.Error);
            }

            // Add to log
            AddLog(messages[0], MessageType.Error);
        }
        
        public bool WarningMessage(string message)
        {
            // Make Warning Page
            page.mainLog.Add(message, MessageType.Warning);
            bool result = page.CreateMessage(message, MessageType.Warning);

            if (!result)
            {
                page.mainLog.Add("Operation Cancelled", MessageType.Error);
                OnComplete();
                page.Continue();
            }
            
            page.Draw();
            return result;
        }
        public bool WarningMessage(IReadOnlyList<string> messages)
        {
            // Make Warning Page
            bool result = page.CreateMessage(messages, MessageType.Warning);
            AddLog(messages[0], MessageType.Warning);

            if (!result)
            {
                page.mainLog.Add("Operation Cancelled", MessageType.Error);
                OnComplete();
                page.Continue();
            }
            
            page.Draw();
            return result;
        }
        
        public void OnComplete()
        {
            if (page.allChapters)
            {
                // Update Header
                page.chaptersDone++;
                page.headerText.Clear();
                page.headerText.AddLine($"Deltarune - {page.modeText}... ({page.chaptersDone}/{ChapterPage.chapterCount} Complete)");
            }
        }
    }
}