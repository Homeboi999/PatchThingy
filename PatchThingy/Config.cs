using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

public record Config (string GamePath, string OutputPath, string ReleasePath)
{
    public static Config current = null!;
}