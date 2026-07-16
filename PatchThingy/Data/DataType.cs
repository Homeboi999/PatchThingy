namespace PatchThingy.Data;

public enum DataType
{
    // Active Data: the data.win that Deltarune loads, and that Steam would replace
    // Vanilla Data: the version of data.win that the patches were based on
    // Backup Data: a second copy of the patched data.win, in case of an update
    Active,
    Vanilla,
    Backup,
}