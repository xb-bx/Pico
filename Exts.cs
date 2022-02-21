using System;
namespace Pico;

public static class Exts
{
    public static void Clamp(ref this int x, int min, int max)
    {
        if (x < min)
            x = min;
        else if (x >= max)
            x = max - 1;
    }
    public static int ZeroOrHigher(this int x) =>
        x < 0 ? 0 : x;
    public static void Clean(ref this Span<char> span)
    {
        for (int i = 0; i < span.Length; i++)
            span[i] = ' ';
    }
}

