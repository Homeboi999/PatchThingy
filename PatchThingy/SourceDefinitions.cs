using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;
using RectpackSharp;
using ImageMagick;
using UndertaleModLib.Util;

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
    string ImageFile,
    int FrameCount,

    uint[] Size,
    int[] Margins,
    uint BoundingBoxMode,
    int[] Origin,

    float playbackSpeed,
    AnimSpeedType playbackType = AnimSpeedType.FramesPerGameFrame
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

public record TextureAtlas
{
    // store extra data to help with importing
    public record AtlasSprite
    {
        public string Name;
        public MagickImage Texture;
        public uint[] Size;

        public uint[] Pos = [0, 0];

        public AtlasSprite(SpriteDefinition sprite, MagickImage Texture)
        {
            this.Name = sprite.Name; // sprite name for loading
            this.Texture = Texture; // texture data

            this.Size = sprite.Size;
        }
    }

    public List<AtlasSprite> Content = [];
    public UndertaleEmbeddedTexture? TexturePage;

    public void Add(SpriteDefinition sprite, string filePath)
    {
        List<MagickImage> frames = [];

        if (sprite.FrameCount > 1)
        {
            // load image and split the frames
            MagickImage image = new(File.Open(filePath, FileMode.Open));

            for (uint i = 0; i < sprite.FrameCount; i++)
            {
                // add the frames to a list of images.
                // this is so i can pack them all at once later.
                frames.Add((MagickImage)image.CloneArea((int)(sprite.Size[0] * i), 0, sprite.Size[0], sprite.Size[1]));
            }
        }
        else
        {
            // load image and add to list
            MagickImage image = new(File.Open(filePath, FileMode.Open));
            frames.Add(image);
        }

        foreach (MagickImage frame in frames)
        {
            // add to TexturePage
            Content.Add(new TextureAtlas.AtlasSprite(sprite, frame));
        }
    }

    public void Save(UndertaleData data)
    {
        // get coords and create image of the right size
        MagickImage pageImage = CreatePage();

        // Add sprites to the page image
        foreach (AtlasSprite sprite in Content)
        {
            pageImage.Composite(sprite.Texture, (int)sprite.Pos[0], (int)sprite.Pos[1], CompositeOperator.Replace);
        }

        // numbers in the names of TextureEntries
        // mayb these can be changed, but im not
        // gonna bother testing it for now

        // add the texture page to the data
        TexturePage = new UndertaleEmbeddedTexture(); // saved so added frames can use it
        TexturePage.Name = new UndertaleString($"Texture {data.EmbeddedTextures.Count}");
        TexturePage.TextureData.Image = GMImage.FromMagickImage(pageImage).ConvertToPng(); // TODO: other formats?
        data.EmbeddedTextures.Add(TexturePage);
    }

    // create the image for the texture page
    MagickImage CreatePage()
    {
        // get area from SpriteFrame
        PackingRectangle[] rectList = new PackingRectangle[Content.Count];
        for (int i = 0; i < Content.Count; i++)
        {
            rectList[i].Width = Content[i].Size[0];
            rectList[i].Height = Content[i].Size[1];
            rectList[i].Id = i;
        }

        // pack the frames into a single image,
        // giving each an X and Y value.
        RectanglePacker.Pack(rectList, out PackingRectangle bounds);

        // update X & Y values 
        foreach (PackingRectangle rect in rectList)
        {
            Content[rect.Id].Pos = [rect.X, rect.Y];
        }

        // Create empty image of the required dimensions
        return new MagickImage(MagickColors.Transparent, bounds.Width, bounds.Height);
    }
}

public record GameObjectDefinition
{
    public string Name = "";
    CollisionShapeFlags CollisionShape;

    // no way this works, right??
    UndertalePointerList<UndertalePointerList<UndertaleGameObject.Event>> Events = [];

    public static GameObjectDefinition Load(UndertaleGameObject gameObject)
    {
        var objectDef = new GameObjectDefinition();

        objectDef.Name = gameObject.Name.Content;
        objectDef.CollisionShape = gameObject.CollisionShape;

        objectDef.Events = gameObject.Events;
        return objectDef;
    }

    public UndertaleGameObject Save(UndertaleData data)
    {
        var gameObject = new UndertaleGameObject();
        gameObject.Name = data.Strings.MakeString(this.Name);
        gameObject.CollisionShape = this.CollisionShape;
        gameObject.Events = this.Events;

        return gameObject;
    }
}