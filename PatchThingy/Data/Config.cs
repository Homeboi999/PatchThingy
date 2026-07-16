namespace PatchThingy.Data;

public record Config (string GamePath, string OutputPath, string ReleasePath)
{
    public static Config current = null!;
}