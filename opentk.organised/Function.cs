using OpenTK.Mathematics;

namespace opentk.organised;

public enum FunctionDimension
{
    ZeroDimension,
    OneDimension,
    twoDimensions,
    threeDimensions
}

public class Function
{
    private string _expression;
    public string Expression => _expression;

    private bool isActive;
    public bool IsActive => isActive;

    private bool isGradientColor;
    public bool IsGradientColor;

    private Enum dimension;
    public Enum Dimension;
    
    public int lineThicness;
    public double resolution;
    public Vector2d extents;
    public long verticesCount;
    
    private Position2D[] _vertices;
    public Position2D[] Vertices => _vertices;

    public Function(string expression, double resolution, Vector2d extents)
    {
        _expression = expression;
        isActive = true;
        lineThicness = 1;
        this.resolution = resolution;
        this.extents = extents;
    }

    public void SetActive(bool boolean)
    {
        isActive = boolean;
    }

    public void UpdateVertices()
    {
        verticesCount = 0;
        for (var i = -extents.X; i < extents.X; i += resolution)
        {
            for (var j = -extents.Y; j < extents.Y; j += resolution)
            {
                verticesCount += 6;
            }
        }
        _vertices = new Position2D[verticesCount];
        
        var k = 0;
        for (var i = -extents.X; i < extents.Y; i += resolution)
        {
            for (var j = -extents.X; j < extents.Y; j += resolution)
            {
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j + 0.1f));

                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j + 0.1f));
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j + 0.1f));
            }
        }
    }
}