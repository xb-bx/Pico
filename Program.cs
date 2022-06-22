using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace Pico;
public class Program
{
    public static void Main(string[] args)
    {
        var buffers = new List<BufferBase>();
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        if (args.Length < 1)
        {
            var helpbuf = new TextBuffer("help", "help", 0, 0, false);
            helpbuf.Lines.AddRange(TabsBuffer.Help);
            buffers.Add(new TextBufferWithLineNumbers(helpbuf, 0, 0));
        }
        else
        {
            foreach (var arg in args)
            {
                var fileBuf = new TextBuffer(arg, Path.GetFileName(arg), 0, 0, true);
                if (File.Exists(arg))
                {
                    fileBuf.Lines.AddRange(File.ReadAllLines(arg));
                }
                else
                {
                    fileBuf.Lines.Add("");
                }
                buffers.Add(new TextBufferWithLineNumbers(fileBuf, 0, 0));
            }
        }
        Console.Clear();
        int w = (Console.WindowWidth);
        int h = Console.WindowHeight;

        //var border = new Border(buffer, w, h);
        var tabs = new TabsBuffer(w, h - 1);
        tabs.Buffers.AddRange(buffers);
        Span<char> b = stackalloc char[(w + 1) * h];
        var nls = new CharBuffer(b, w, 0, w, h, w);
        for (int i = 0; i < h - 1; i++)
            nls[0, i] = '\n';
        var main = new CharBuffer(b, 0, 0, w, h, w);
        DebugLine.Instance.Width = w - 1;
        DebugLine.Instance.Height = 1;
        var buf = new CharBuffer(b, 0, 0, w, h - 1, w);
        var debug = new CharBuffer(b, 0, h - 1, w - 1, 1, w);
        Console.CursorLeft = Console.CursorTop = 0;
        tabs.RerenderNeeded = true;
        var ds = ((Span<char>)debug);
        var bs = ((Span<char>)buf);
        Console.CancelKeyPress += (o, e) =>
        {
            e.Cancel = true;
            if (tabs.Buffers.OfType<TextBufferWithLineNumbers>().Any(x => x.Edited))
            {
                Console.Clear();
                Console.WriteLine("There're unsaved buffers? Save all and quit[Y], Discard changes[N], Cancel[C]?");
                var key = Console.ReadLine();
                switch (key)
                {
                    case "Y":
                        foreach (var buf in tabs.Buffers.OfType<TextBufferWithLineNumbers>().Where(x => x.Child.Editable && x.Edited).Select(x => x.Child))
                        {
                            File.WriteAllLines(buf.Path, buf.Lines);
                        }
                        e.Cancel = false;
                        break;
                    case "N": e.Cancel = false; break;
                    default:
                        Console.Clear();
                        Span<char> b = stackalloc char[(w + 1) * h];
                        var main = new CharBuffer(b, 0, 0, w, h, w);
                        var buff = new CharBuffer(b, 0, 0, w, h - 1, w);
                        var debug = new CharBuffer(b, 0, h - 1, w - 1, 1, w);
                        tabs.RerenderNeeded = true;
                        tabs.Render(buff);
                        DebugLine.Instance.Render(debug);
                        Console.Out.Write(main.Buffer);
                        Console.CursorLeft = Console.CursorTop = 0;
                        return;
                }
            }
            Console.Clear();
            Console.CursorVisible = true;
        };
        while (true)
        {
            try
            {
                int cursorX = Console.CursorLeft;
                int cursorY = Console.CursorTop;
                Console.CursorVisible = false;
                if (tabs.RerenderNeeded)
                {
                    bs.Clean();
                    tabs.Render(buf);
                    Console.CursorLeft = 0;
                    Console.CursorTop = 0;
                    Console.Out.Write(bs);
                    Console.CursorLeft = cursorX;
                    Console.CursorTop = cursorY;

                }
                if (DebugLine.Instance.RerenderNeeded)
                {
                    ds.Clean();
                    DebugLine.Instance.Render(debug);
                    Console.CursorLeft = 0;
                    Console.CursorTop = debug.OffsetY;
                    Console.Out.Write(ds);
                    Console.CursorLeft = cursorX;
                    Console.CursorTop = cursorY;
                }
                Console.CursorVisible = true;
                tabs.Input(buf, Console.ReadKey(true));
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(DebugLine.Instance.Debug);
                return;
            }
        }
    }


}
