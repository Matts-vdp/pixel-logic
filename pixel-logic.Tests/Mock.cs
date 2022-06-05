using Game.Components;

namespace Pixel_logic.Tests;

public class File : IFile
{
    private readonly string _txt;
    public string Result = "";
    public File() : this("")
    {
    }
    public File(string txt)
    {
        _txt = txt;
    }
    public string ReadAllText(string filename)
    {
        return _txt;
    }
    public void WriteAllTextAsync(string filename, string txt)
    {
        Result = txt;
    }
}