using UndertaleModLib;
using UndertaleModLib.Models;

namespace TestThingy.Data.Sources;

public record GameObjectDefinition
(
    string Name,
    string? Sprite,
    string? Parent,
    CollisionShapeFlags CollisionShape,
    bool IsAwake,
    List<EventDefinition> Events
)
{
    public static GameObjectDefinition Load(UndertaleGameObject gameObject)
    {
        // get name
        string name = gameObject.Name.Content;

        // find name of sprite if it exists
        string? spriteName = null;
        if (gameObject.Sprite is not null)
        {
            spriteName = gameObject.Sprite.Name.Content;
        }

        // find name of parent object if it exists
        UndertaleGameObject parentObject = gameObject._parentId.Resource;
        string? parentName = null;

        if (parentObject is not null)
        {
            parentName = parentObject.Name.Content;
        }

        CollisionShapeFlags collisionShape = gameObject.CollisionShape;
        bool awake = gameObject.Awake;
        List<EventDefinition> events = [];

        // first layer of lists is for event types.
        // (explains the empty entries in the json)
        for (int i = 0; i <= 14; i++)
        {
            foreach (UndertaleGameObject.Event eventAction in gameObject.Events[i])
            {
                events.Add(EventDefinition.Load(eventAction, i));
            }
        }

        return new GameObjectDefinition(name, spriteName, parentName, collisionShape, awake, events);
    }

    public UndertaleGameObject Save(UndertaleData data)
    {
        var gameObject = new UndertaleGameObject();
        gameObject.Name = data.Strings.MakeString(this.Name);

        if (this.Sprite is not null)
        gameObject.Sprite = data.Sprites.ByName(this.Sprite);

        if (this.Parent is not null)
        gameObject.ParentId = data.GameObjects.ByName(this.Parent);

        gameObject.CollisionShape = this.CollisionShape;
        gameObject.Awake = this.IsAwake;

        // convert events
        foreach (EventDefinition eventDef in this.Events)
        {
            eventDef.Save(gameObject, data);
        }

        return gameObject;
    }
}