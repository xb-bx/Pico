using System;
namespace Pico;

public class DebugLine : BufferBase
{
    private string debug = "";
    public string Debug { get => debug; set { RerenderNeeded = true; debug = value; } }

    public static DebugLine Instance = new();
    public override void Render(CharBuffer buf)
    {
        for (int i = 0; i < buf.Width && i < debug.Length; i++)
            buf[i, 0] = debug[i];
        Console.CursorLeft = Console.CursorTop = 0;
        RerenderNeeded = false;
    }
    public override void Input(CharBuffer buf, ConsoleKeyInfo info) { }
}

