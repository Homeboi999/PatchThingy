using System.Diagnostics;
using PatchThingy.Data;
using PatchThingy.Operations;

namespace PatchThingy.Pages.Operations;

class ApplyPatchesPage : OperationPage
{
    ApplyPatches operation;
    protected override string modeText => "Applying Patches";

    public ApplyPatchesPage(int chapter, bool allChapters = false) : base(chapter, allChapters)
    {
        ApplyPatchesBridge operationBridge = new ApplyPatchesBridge(this);
        operation = new(operationBridge);
    }

    protected override void OnInitialize()
    {
        if (allChapters)
        {
            for (int i = 1; i <= ChapterPage.chapterCount; i++)
            {
                operation.ApplyAllToChapter(i);

                // Add a space between chapters,
                // excluding the final one
                if (i < ChapterPage.chapterCount)
                {
                    mainLog.Add("");
                }
            }
        }
        else
        {
            operation.ApplyAllToChapter(chapter);
        }

        // Show the ResultGroup after everything
        resultGroup.visible = true;
        Draw();
    }

    class ApplyPatchesBridge(ApplyPatchesPage page) : IOperation
    {
        public bool TryLoadData(DataType type, int chapter, out DataFile dataFile)
        {
            string loadText = $"Loading {DataFile.GetFileName(type)} for Chapter {chapter}...";

            if (Debugger.IsAttached)
            {
                AddLog(loadText);
            }
            else if (page.loadingGroup.visible)
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
            # if DEBUG
            if (Debugger.IsAttached)
            {
                switch (type)
                {
                    case MessageType.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case MessageType.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case MessageType.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    default:
                        Console.ResetColor();
                        break;
                }

                Console.WriteLine(message);
                return;
            }
            # endif

            page.mainLog.Add(message, type);
            page.Draw();
        }
        
        public void ErrorMessage(string message)
        {
            # if DEBUG
            if (Debugger.IsAttached)
            {
                AddLog(message, MessageType.Error);
                return;
            }
            # endif

            // Make Error Page
            page.CreateMessage(message, MessageType.Error);
            AddLog(message, MessageType.Error);
        }
        public void ErrorMessage(IReadOnlyList<string> messages)
        {
            # if DEBUG
            if (Debugger.IsAttached)
            {
                AddLog(messages[0], MessageType.Error);
                return;
            }
            # endif

            // Make Error Page
            page.CreateMessage(messages, MessageType.Error);
            AddLog(messages[0], MessageType.Error);
        }
        
        public bool WarningMessage(string message)
        {
            # if DEBUG
            if (Debugger.IsAttached)
            {
                AddLog(message, MessageType.Warning);
                return true;
            }
            # endif

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
            # if DEBUG
            if (Debugger.IsAttached)
            {
                AddLog(messages[0], MessageType.Warning);
                return true;
            }
            # endif

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
            # if DEBUG
            if (Debugger.IsAttached)
            {
                return;
            }
            # endif

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