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

    public static bool TryAppend(
        this Span<char> target,
        ref int offset,
        uint value,
        ReadOnlySpan<char> format = default
    )
    {
        if (!value.TryFormat(target[offset..], out int charsWritten, format))
            return false;

        offset += charsWritten;
        return true;
    }
}
