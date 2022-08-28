using OpenTK.Mathematics;

namespace opentk.organised;

public enum FunctionDimension
{
    ZeroDimension,
    OneDimension,
    twoDimensions,
    threeDimensions
}

public class Function : IDisposable
{
    public static Dictionary<Guid, Function> functionsList = new Dictionary<Guid, Function>();


    #region -> Getters <-

        private string _expression;
        public string Expression => _expression;

        private Guid _guid;
        public Guid Id => _guid;
        
        private bool isActive;
        public bool IsActive => isActive;

        private bool isGradientColor;
        public bool IsGradientColor;

        private Enum dimension;
        public Enum Dimension;
        
        private Position2D[] _vertices;
        public Position2D[] Vertices => _vertices;

        private Color4 _color;
        public Color4 Color => _color;

    #endregion

    #region -> Fields <-

        public int lineThicness;
        public double resolution;
        public Vector2d extents;
        public long verticesCount;
        public VertexBuffer _vertexBuffer;
        public ShaderProgram _shaderProgram;
        public VertexArray _vertexArray;

        #endregion

    public Function(Guid guid, string expression, double resolution, Vector2d extents, Color4 color)
    {
        _expression = expression;
        isActive = true;
        lineThicness = 1;
        this.resolution = resolution;
        this.extents = extents;
        _guid = guid;
        _color = color;
        functionsList.Add(_guid, this);
        UpdateVertices();
        CreateBuffer();
        Console.WriteLine(guid.ToString());
    }

    public void SetActive(bool boolean)
    {
        isActive = boolean;
    }

    public void Dispose()
    {
        _vertexArray.Dispose();
        _vertexBuffer.Dispose();
        _shaderProgram.Dispose();
        functionsList.Remove(_guid);
    }

    public void CreateBuffer()
    {
        _vertexBuffer = new VertexBuffer(Position2D.VertexInfo, _vertices.Length);
        _vertexBuffer.SetData(_vertices, _vertices.Length);

        _vertexArray = new VertexArray(_vertexBuffer);
        _shaderProgram = new ShaderProgram(File.ReadAllText("vert.glsl").Replace("/*return function*/", _expression),
            File.ReadAllText("frag.glsl"));//.Replace("/*aColor = vColor*/", $"aColor = vec4({_color.R}, {_color.G}, {_color.B}, {_color.A});"));
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