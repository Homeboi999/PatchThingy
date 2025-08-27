using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

public record ScriptDefinition (string Name, string Code)
{
    // This is all the necessary data to define a script.

    public static ScriptDefinition Load (UndertaleScript script)
    {
        // makes it easier to load the required data
        // from an UndertaleScript, mainly for consistency
        return new ScriptDefinition( script.Name.Content, script.Code.Name.Content);
    }
    
    public UndertaleScript Save (UndertaleData Data)
    {
        // Add script definition to UndertaleData,
        // and defining a string for the script name.
        return new UndertaleScript() { Name = Data.Strings.MakeString(Name), Code = Data.Code.ByName(Code) };
    }
}

public record SpriteDefinition
(
    string Name,

    uint[] Size,
    int[] Margins,
    uint BoundingBoxMode,
    int[] Origin,

    float playbackSpeed,
    AnimSpeedType playbackType
)
{
    // EVERY SPRITE IN DELTARUNE HAS
    // THE FOLLOWING PROPERTIES:
    //
    // Is special type?     -   true
    // Version              -   3
    // Type =               -   Normal
    //
    // (as seen in UndertaleModTool)

    public static SpriteDefinition Load (UndertaleSprite sprite)
    {
        // makes it easier to load the required data
        // from an UndertaleSprite bc theres just SO MUCH
        return new SpriteDefinition
        (
            sprite.Name.Content,
            [sprite.Width, sprite.Height],
            [sprite.MarginLeft, sprite.MarginRight, sprite.MarginBottom, sprite.MarginTop],
            sprite.BBoxMode,
            [sprite.OriginX, sprite.OriginY],
            sprite.GMS2PlaybackSpeed,
            sprite.GMS2PlaybackSpeedType
        );
    }
}