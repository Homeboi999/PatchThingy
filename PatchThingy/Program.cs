// See https://aka.ms/new-console-template for more information
// run this with `dotnet run` or pressing F5

using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Decompiler;
using CodeChicken.DiffPatch;
using System.Text.Json;

DataFile vanilla = new("data-vanilla.win");
DataFile modded = new("data.win");

DataHandler.GeneratePatches(vanilla, modded);