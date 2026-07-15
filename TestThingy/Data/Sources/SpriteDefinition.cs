using UndertaleModLib;
using UndertaleModLib.Models;

namespace TestThingy.Data.Sources;

public record SpriteDefinition
(
    string Name,
    int index,
    string ImageFile,
    int FrameCount,

    uint[] Size,
    int[] Margins,
    uint BoundingBoxMode,
    int[] Origin,

    float playbackSpeed,
    AnimSpeedType playbackType = AnimSpeedType.FramesPerGameFrame
) : IComparable<SpriteDefinition> // so that I can sort by Index
{
    // EVERY SPRITE IN DELTARUNE HAS
    // THE FOLLOWING PROPERTIES:
    //
    // Is special type?     -   true
    // Version              -   3
    // Type =               -   Normal
    //
    // (as seen in UndertaleModTool)

    public static SpriteDefinition Load (UndertaleSprite sprite, int spriteIndex)
    {
        string fileName;

        // generate the file name that the sprite
        // will load from when applying
        if (sprite.Textures.Count > 1)
        {
            // im only using the strip format for animated sprites :P
            fileName = $"{sprite.Name.Content}_strip{sprite.Textures.Count}.png";
        }
        else
        {
            fileName = $"{sprite.Name.Content}.png";
        }

        // override default behavior to be
        // FramesPerGameFrame
        AnimSpeedType speedType;
        if (sprite.IsSpecialType == false)
        {
            speedType = AnimSpeedType.FramesPerGameFrame;
        }
        else
        {
            speedType = sprite.GMS2PlaybackSpeedType;
        }

        // makes it easier to load the required data
        // from an UndertaleSprite bc theres just SO MUCH
        return new SpriteDefinition
        (
            sprite.Name.Content,
            spriteIndex,
            fileName,
            sprite.Textures.Count,

            [sprite.Width, sprite.Height],
            [sprite.MarginLeft, sprite.MarginRight, sprite.MarginBottom, sprite.MarginTop],
            sprite.BBoxMode,
            [sprite.OriginX, sprite.OriginY],
            
            sprite.GMS2PlaybackSpeed,
            speedType
        );
    }

    public int CompareTo(SpriteDefinition? compareSprite)
    {
        if (compareSprite is null)
        {
            return 1;
        }
        else
        {
            return (this.index.CompareTo(compareSprite.index));
        }
    }

    UndertaleSimpleList<UndertaleSprite.TextureEntry> Textures = [];

    public UndertaleSprite Save (UndertaleData data)
    {
        // initialize variables
        UndertaleString spriteUTString = data.Strings.MakeString(Name);
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

        // texture list prepared in advance
        newSprite.Textures = Textures;

        return newSprite;
    }

    public void AddFrames (TextureAtlas atlas, UndertaleData data)
    {
        // should list through atlas sprites where the name is the same
        foreach (TextureAtlas.AtlasSprite frame in atlas.Content)
        {
            if (frame.Name != this.Name)
            {
                continue;
            }

            // Initalize values of this texture
            UndertaleTexturePageItem texturePageItem = new UndertaleTexturePageItem();
            texturePageItem.Name = new UndertaleString($"PageItem {data.TexturePageItems.Count}");

            texturePageItem.SourceX = (ushort)frame.Pos[0];
            texturePageItem.SourceY = (ushort)frame.Pos[1];
            texturePageItem.SourceWidth = (ushort)frame.Size[0];
            texturePageItem.SourceHeight = (ushort)frame.Size[1];

            texturePageItem.TargetX = 0;
            texturePageItem.TargetY = 0;
            texturePageItem.TargetWidth = (ushort)frame.Size[0];
            texturePageItem.TargetHeight = (ushort)frame.Size[1];

            texturePageItem.BoundingWidth = (ushort)frame.Size[0];
            texturePageItem.BoundingHeight = (ushort)frame.Size[1];
            texturePageItem.TexturePage = atlas.TexturePage;

            // Add this texture to UMT
            data.TexturePageItems.Add(texturePageItem);

            // Create TextureEntry object
            UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
            texentry.Texture = texturePageItem;
            Textures.Add(texentry);
        }
    }
}