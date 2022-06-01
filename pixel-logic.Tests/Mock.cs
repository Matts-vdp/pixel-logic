using Game.Components;

namespace pixel_logic.Tests;

public class File : IFile
{
    private string txt;
    public string result = "";
    public File() : this("")
    {
    }
    public File(string txt)
    {
        this.txt = txt;
    }
    public string ReadAllText(string filename)
    {
        return txt;
    }
    public void WriteAllTextAsync(string filename, string txt)
    {
        result = txt;
    }
}