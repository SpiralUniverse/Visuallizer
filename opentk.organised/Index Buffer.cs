using OpenTK.Graphics.OpenGL;

namespace opentk.organised;

public sealed class IndexBuffer : IDisposable
{
    public static readonly int MinIndexCount = 1;
    public static readonly int MaxIndexCount = 250_000;

    private bool _disposed;

    public readonly int IndexBufferHandle;
    public readonly int IndexCount;
    public readonly bool IsStatic;

    public IndexBuffer(int indexCount, bool isStatic = true)
    {
        if (indexCount < MinIndexCount ||
            indexCount > MaxIndexCount)
        {
            throw new ArgumentOutOfRangeException(nameof(indexCount));
        }

        IndexCount = indexCount;
        IsStatic = isStatic;

        BufferUsageHint hint = BufferUsageHint.StaticDraw;
        if (!IsStatic)
        {
            hint = BufferUsageHint.StreamDraw;
        }

        IndexBufferHandle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
        GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(int), IntPtr.Zero, hint);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    ~IndexBuffer()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.DeleteBuffer(IndexBufferHandle);

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public void SetData(int[] data, int count)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.Length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(data));
        }

        if (count <= 0 ||
            count > IndexCount ||
            count > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, count * sizeof(int), data);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }
}