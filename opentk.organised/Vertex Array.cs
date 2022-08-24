using OpenTK.Graphics.OpenGL;

namespace opentk.organised;

public sealed class VertexArray : IDisposable
{
    private bool _disposed;

    public readonly int VertexArrayHandle;
    public readonly VertexBuffer VertexBuffer;

    public VertexArray(VertexBuffer vertexBuffer)
    {
        _disposed = false;

        if(vertexBuffer is null)
        {
            throw new ArgumentNullException(nameof(vertexBuffer));
        }

        VertexBuffer = vertexBuffer;

        int vertexSizeInBytes = VertexBuffer.VertexInfo.SizeInBytes;
        VertexAttribute[] attributes = VertexBuffer.VertexInfo.VertexAttributes;

        VertexArrayHandle = GL.GenVertexArray();
        GL.BindVertexArray(VertexArrayHandle);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer.VertexBufferHandle);

        foreach (var attribute in attributes)
        {
            GL.VertexAttribPointer(attribute.Index, attribute.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, attribute.Offset);
            GL.EnableVertexAttribArray(attribute.Index);
        }

        GL.BindVertexArray(0);
    }

    ~VertexArray()
    {
        Dispose();
    }

    public void Dispose()
    {
        if(_disposed)
        {
            return;
        }

        GL.BindVertexArray(0);
        GL.DeleteVertexArray(VertexArrayHandle);

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}