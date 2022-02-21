using System;
namespace Pico;

public ref struct CharBuffer
{
    public Span<char> Buffer;
    public int OffsetX, OffsetY, Width, Height, ScreenWidth;
    public char this[int x, int y]
    {
        get => Buffer[(y + OffsetY) * ScreenWidth + (x + OffsetX)];
        set => Buffer[(y + OffsetY) * ScreenWidth + (x + OffsetX)] = value;
    }
    public CharBuffer(Span<char> buf, int ox, int oy, int w, int h, int sw)
    {
        (OffsetX, OffsetY, Width, Height, ScreenWidth) = (ox, oy, w, h, sw);
        Buffer = buf;
    }
    public static implicit operator Span<char>(CharBuffer buf) => buf.Buffer.Slice(buf.OffsetY * buf.ScreenWidth + buf.OffsetX, ((buf.Height - 1 + buf.OffsetY) * buf.ScreenWidth + (buf.Width - 0 + buf.OffsetX) + 0) - (buf.OffsetY * buf.ScreenWidth + buf.OffsetX));
}

