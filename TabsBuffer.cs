using System.Collections.Generic;
using System.Linq;
using System;
namespace Pico;

public class TabsBuffer : BufferBase
{
    public static string[] Help = 
            new string[]
            {
                "Use arrows to move",
                "Backspace to delete",
                "Ctrl + T - next buffer",
                "Ctrl + Q - close current buffer",
                "Ctrl + S - save buffer",
                "Ctrl + H - show help",
                "Ctrl + N - Toggle line numbers",
                "Ctrl + R - Reload current buffer",
            };


    public List<BufferBase> Buffers { get; set; }
    public int CurrentTab;
    public TabsBuffer(int w, int h)
    {
        Buffers = new();
        (Width, Height) = (w, h);
    }
    public override void Render(CharBuffer buf)
    {
        if (Buffers.Count == 0)
            Environment.Exit(0);
        var header = string.Join("", Buffers.Select((x, i) => $"| {(i == CurrentTab ? "[" : "")}{ x.Name }{(i == CurrentTab ? "]" : "")} "));
        for (int i = 0; i < header.Length && i < buf.Width; i++)
            buf[i, 0] = header[i];
        for (int i = 0; i < Width; i++)
            buf[i, 1] = '\u203e';
        CurrentTab.Clamp(0, Buffers.Count);
        Buffers[CurrentTab].Height = Height - 2;
        Buffers[CurrentTab].Width = Width;
        Buffers[CurrentTab].Render(new CharBuffer(buf.Buffer, buf.OffsetX, buf.OffsetY + 2, buf.Width, buf.Height - 2, buf.ScreenWidth));
        RerenderNeeded = false;
    }
    public override void Input(CharBuffer buf, ConsoleKeyInfo keyinfo)
    {
        if (keyinfo.Key == ConsoleKey.T && (keyinfo.Modifiers & ConsoleModifiers.Control) > 0)
        {
            CurrentTab = (CurrentTab + 1) % Buffers.Count;
            RerenderNeeded = true;
        }
        else if (keyinfo.Key == ConsoleKey.Q && (keyinfo.Modifiers & ConsoleModifiers.Control) > 0)
        {
            Buffers.Remove(Buffers[CurrentTab--]);
            if (Buffers.Count > 0)
                CurrentTab %= Buffers.Count;
            RerenderNeeded = true;
        }
        else if (keyinfo.Key == ConsoleKey.H && (keyinfo.Modifiers & ConsoleModifiers.Control) > 0)
        {
            var helpTextBufer = new TextBuffer("help", "help", 0, 0, false);
            helpTextBufer.Lines.AddRange(Help);
            Buffers.Add(new TextBufferWithLineNumbers(helpTextBufer, 0, 0));
            CurrentTab = Buffers.Count - 1;
            RerenderNeeded = true;
        }
        else
        {
            Buffers[CurrentTab].Input(new CharBuffer(buf.Buffer, buf.OffsetX, buf.OffsetY + 2, buf.Width, buf.Height - 2, buf.ScreenWidth), keyinfo);
            RerenderNeeded = Buffers[CurrentTab].RerenderNeeded;
        }
    }
}

