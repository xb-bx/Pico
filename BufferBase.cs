using System;
namespace Pico;

public abstract class BufferBase
{
    public int Width, Height;
    public string Name;
    public bool RerenderNeeded = false;
    public abstract void Input(CharBuffer buf, ConsoleKeyInfo keyinfo);
    public abstract void Render(CharBuffer buf);
}

