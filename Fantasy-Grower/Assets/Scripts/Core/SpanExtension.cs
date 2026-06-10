using System;

public static class SpanExtension
{
    public static bool TryAppend(this Span<char> target, ref int offset, ReadOnlySpan<char> value)
    {
        if (offset + value.Length > target.Length)
            return false;

        value.CopyTo(target[offset..]);
        offset += value.Length;
        return true;
    }
}
