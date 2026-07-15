using ImageMagick;
using RectpackSharp;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;

namespace TestThingy.Data.Sources;

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