using UndertaleModLib;
using UndertaleModLib.Compiler;
using UndertaleModLib.Models;

namespace PatchThingy.Data.Sources;

public record EventDefinition (string Code, EventType Type, uint Subtype)
{
    public static EventDefinition Load(UndertaleGameObject.Event objectEvent, int typeIndex)
    {
        return new EventDefinition(objectEvent.Actions[0].CodeId.Name.Content, (EventType)typeIndex, objectEvent.EventSubtype);
    }

    public void Save(UndertaleGameObject gameObject, UndertaleData data)
    {
        // ensure code entry exists before proceeding
        UndertaleCode codeEntry = data.Code.ByName(this.Code);

        if (codeEntry is null)
        {
            // gameObjects are added before code files, so
            // we need to make an empty file to link to.
            codeEntry = UndertaleCode.CreateEmptyEntry(data, this.Code);

            // the QueueReplace used for source code will
            // still work regardless i think
        }

        // properly link the code and the events
        CodeImportGroup.LinkEvent(gameObject, codeEntry, this.Type, this.Subtype);
    }
}