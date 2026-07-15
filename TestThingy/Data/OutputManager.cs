namespace TestThingy.Data;

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
    }

    public bool QueueFile(TempFile queueFile)
    {
        // check for duplicate entries
        if (tempFiles.Exists(file => (file.name == queueFile.name && file.type == queueFile.type)))
        {
            return false;
        }

        string path = Path.Combine(GetChapterPath(queueFile.chapter), GetTypeFolder(queueFile.type));
        bool isGlobal = File.Exists(Path.Combine(GetChapterPath(0), GetTypeFolder(queueFile.type))) && !File.Exists(path);

        // save if the chapter is global or not
        if (isGlobal)
        {
            queueFile.chapter = 0;
            path = Path.Combine(GetChapterPath(queueFile.chapter), GetTypeFolder(queueFile.type));
        }

        // add to queue
        tempFiles.Add(queueFile);
        return true;
    }

    void ResetFolder(int chapter, FileType type, bool keepFiles = false)
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