using System.Diagnostics;

namespace PatchThingy.Data;

public class OutputManager
{
    public static string GetChapterPath(int chapter)
    {
        switch (chapter)
        {
            // Global Patches
            case 0:
                return Path.Combine(Config.current.OutputPath, "./Global");
                
            // Chapter-Specific
            default:
                return Path.Combine(Config.current.OutputPath, $"./Chapter{chapter}");
        }
    }

    public static string GetTypeFolder(FileType type)
    {
        switch (type)
        {
            // Source Code
            case FileType.Code:
                return "./Code";
                
            // Scripts
            case FileType.Script:
                return "./Scripts";
                
            // Sprites
            case FileType.Sprite:
                return "./Sprites";
                
            // Patches
            case FileType.Patch:
                return "./Patches";
                
            // Game Objects
            case FileType.GameObject:
                return "./GameObjects";
                
            // Failsafe
            default:
                return "";
        }
    }

    public static string GetFileExtension(FileType type)
    {
        switch (type)
        {
            // Source Code
            case FileType.Code:
                return $".gml";
                
            // Scripts
            case FileType.Script:
                return $".json";
                
            // Sprites
            case FileType.Sprite:
                return $".json";
                
            // Patches
            case FileType.Patch:
                return $".gml.patch";
                
            // Game Objects
            case FileType.GameObject:
                return $".json";
                
            // Failsafe
            default:
                return "";
        }
    }

    List<TempFile> tempFiles = [];

    public record TempFile
    {
        public string name;
        public string text;
        public int chapter;
        public FileType type;

        public TempFile (string name, string text, int chapter, FileType type)
        {
            this.name = name;
            this.text = text;
            this.chapter = chapter;
            this.type = type;
        }

        public override string ToString()
        {
            return $"[CH{chapter}] {type} - {name}";
        }
    }

    public bool QueueFile(bool makeGlobal, TempFile queueFile)
    {
        string fileName = queueFile.name + GetFileExtension(queueFile.type);
        string path = Path.Combine(GetChapterPath(queueFile.chapter), GetTypeFolder(queueFile.type), fileName);
        string globalPath = Path.Combine(GetChapterPath(0), GetTypeFolder(queueFile.type), fileName);
        
        bool isGlobal = File.Exists(globalPath) && !File.Exists(path);

        // save if the chapter is global or not
        if (isGlobal)
        {
            queueFile.chapter = 0;
            
            if (!makeGlobal)
            {
                return false;
            }
            // // idk why i was doing this
            // path = Path.Combine(globalPath, fileName);
        }

        // add to queue
        tempFiles.Add(queueFile);
        return true;
    }

    public void SaveModFiles(int chapter, bool makeGlobal)
    {
        // Reset output folder structure.
        ResetAllFolders(chapter);

        // Write file to the correct folder
        // based on the file type.
        foreach (TempFile queueFile in tempFiles)
        {
            // Skip global patches if not chosen chapter
            if (queueFile.chapter == 0 && !makeGlobal)
            {
                continue;
            }

            string chapterPath = GetChapterPath(queueFile.chapter);
            string typeFolder = GetTypeFolder(queueFile.type);
            string fileName = queueFile.name + GetFileExtension(queueFile.type);

            string path = Path.Combine(chapterPath, typeFolder, fileName);

            // Don't replace global source code.
            if (queueFile.type == FileType.Code && queueFile.chapter == 0 && File.Exists(path))
            {
                continue;
            }

            File.WriteAllText(path, queueFile.text);
        }

        // after done, clear fileQueue so it doesnt
        // carry over to the next chapter
        // keep global patches in queue maybe?
        tempFiles.RemoveAll(file => (file.chapter != 0));
    }

    void ResetAllFolders(int chapter)
    {
        ResetFolder(chapter, FileType.Code, keepFiles: false);
        ResetFolder(chapter, FileType.Script);
        ResetFolder(chapter, FileType.Sprite);
        ResetFolder(chapter, FileType.Patch);
        ResetFolder(chapter, FileType.GameObject);
    }

    void ResetFolder(int chapter, FileType type, bool keepFiles = true)
    {
        string fullPath = Path.Combine(GetChapterPath(chapter), GetTypeFolder(type));

        // if folder doesnt exist, dont need to delete
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        // Empty folder of all files of a given type
        foreach (string file in Directory.EnumerateFiles(fullPath))
        {
            if (file.EndsWith(GetFileExtension(type)))
            {
                if (keepFiles)
                {
                    // only delete files that arent in the queue so
                    // we dont keep anything i got rid of or smthn
                    if (!tempFiles.Exists(queueFile => (queueFile.name == Path.GetFileNameWithoutExtension(file))))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    File.Delete(file);
                }
            }
        }
    }
}