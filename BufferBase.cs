using System;
namespace Pico;

public abstract class BufferBase
{
    public int Width, Height;
    public abstract string Name { get; }
    public bool RerenderNeeded = true;
    public abstract void Input(CharBuffer buf, ConsoleKeyInfo keyinfo);
    public abstract void Render(CharBuffer buf);
}

