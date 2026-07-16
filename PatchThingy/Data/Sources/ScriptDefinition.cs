using UndertaleModLib;
using UndertaleModLib.Models;

namespace PatchThingy.Data.Sources;

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
        // ensure code entry exists before proceeding
        UndertaleCode codeEntry = Data.Code.ByName(this.Code);

        if (codeEntry is null)
        {
            // add scripts before everything else to try
            // to stop weirdly generating script defs
            codeEntry = UndertaleCode.CreateEmptyEntry(Data, this.Code);
        }

        // Add script definition to UndertaleData,
        // and defining a string for the script name.
        return new UndertaleScript() { Name = Data.Strings.MakeString(Name), Code = codeEntry };
    }
}