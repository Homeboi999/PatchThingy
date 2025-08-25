using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using Underanalyzer.Decompiler;

public record ScriptDefinition (string Name, string Code)
{
    // This is all the necessary data to define a script.
}

public record GameObjectDefinition (string Name)
{
    // NO ACTUAL DATA DEFINED HERE!
    //
    // The only game objects I add are obj_partymenu and obj_configmenu,
    // both of which use whatever default settings UndertaleGameObject has.
    // I'll add the necessary definitions as I need them, but only default works for now.
}