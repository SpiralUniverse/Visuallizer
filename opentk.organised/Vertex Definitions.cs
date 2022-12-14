using OpenTK.Mathematics;

namespace opentk.organised;

public readonly struct VertexAttribute
{
    public readonly string Name;
    public readonly int Index;
    public readonly int ComponentCount;
    public readonly int Offset;

    public VertexAttribute(string name, int index, int componentCount, int offset)
    {
        Name = name;
        Index = index;
        ComponentCount = componentCount;
        Offset = offset;
    }
}

public sealed class VertexInfo
{
    public readonly Type Type;
    public readonly int SizeInBytes;
    public readonly VertexAttribute[] VertexAttributes;

    public VertexInfo(Type type, params VertexAttribute[] attributes)
    {
        Type = type;
        SizeInBytes = 0;

        VertexAttributes = attributes;

        for (int i = 0; i < VertexAttributes.Length; i++)
        {
            VertexAttribute attribute = VertexAttributes[i];
            SizeInBytes += attribute.ComponentCount * sizeof(float);
        }
    }
}


public readonly struct VertexPositionColor
{
    public readonly Vector3 Position;
    public readonly Color4 Color;

    public static readonly VertexInfo VertexInfo = new(
        typeof(VertexPositionColor),
        new VertexAttribute("Position", 0, 3, 0),
        new VertexAttribute("Color", 1, 4, 3 * sizeof(float))
    );

    public VertexPositionColor(Vector3 position, Color4 color)
    {
        Position = position;
        Color = color;
    }
}

public readonly struct Position2D
{
    public readonly Vector2 Position;
    public static readonly VertexInfo VertexInfo = new(
        typeof(Position2D),
        new VertexAttribute("Position", 0, 2, 0)
    );

    public Position2D(Vector2 position)
    {
        Position = position;
    }
}



public readonly struct VertexPositionTexture
{
    public readonly Vector3 Position;
    public readonly Vector2 TexCoord;

    public static readonly VertexInfo VertexInfo = new(
        typeof(VertexPositionTexture),
        new VertexAttribute("Position", 0, 3, 0),
        new VertexAttribute("TexCoord", 1, 2, 2 * sizeof(float))
    );

    public VertexPositionTexture(Vector3 position, Vector2 texCoord)
    {
        Position = position;
        TexCoord = texCoord;
    }
}