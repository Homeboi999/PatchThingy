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

    public UndertaleSprite Save (UndertaleData Data)
    {
        // initialize variables
        UndertaleString spriteUTString = Data.Strings.MakeString(Name);
        UndertaleSprite newSprite = new UndertaleSprite();

        // set variables
        newSprite.Name = spriteUTString;

        newSprite.Width = Size[0];
        newSprite.Height = Size[1];

        newSprite.MarginLeft = Margins[0];
        newSprite.MarginRight = Margins[1];
        newSprite.MarginBottom = Margins[2];
        newSprite.MarginTop = Margins[3];

        newSprite.BBoxMode = BoundingBoxMode;
        newSprite.OriginX = Origin[0];
        newSprite.OriginY = Origin[1];

        newSprite.GMS2PlaybackSpeedType = playbackType;
        newSprite.GMS2PlaybackSpeed = playbackSpeed;

        // hardcoded values
        newSprite.IsSpecialType = true;
        newSprite.SVersion = 3;

        // placeholder sprite (funni)
        // Create TextureEntry object
        UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
        texentry.Texture = Data.TexturePageItems.ByName("PageItem 6334"); // spr_checkers_milk
        newSprite.Textures.Add(texentry);

        return newSprite;
    }
}