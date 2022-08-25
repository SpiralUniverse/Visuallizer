using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace opentk.organised;

public class Grid
{
    
    #region -> Fields and Getters <-

        private readonly uint _gridSize;
        public uint GridSize => _gridSize;
        
        private Color4 _color;
        public Color4 Color => _color;
        
        private readonly VertexPositionColor[] _gridVertices;
        private VertexArray _vertexArray;
        private VertexBuffer _vertexBuffer;
        private ShaderProgram _shaderProgram;

    #endregion

    public Grid(uint gridSize)
    {
        _gridSize = gridSize;
        _gridVertices = new VertexPositionColor[_gridSize * 4];
        _color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        GridGen();
        Initiate();
        DrawGrid();
    }

    private void Initiate()
    {
        _vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, _gridVertices.Length);
        _vertexBuffer.SetData(_gridVertices, _gridVertices.Length);
        _vertexArray = new VertexArray(_vertexBuffer);
        _shaderProgram = new ShaderProgram(File.ReadAllText("Grid Vert.glsl"), File.ReadAllText("Grid Frag.glsl"));
    }

    ~Grid()
    {
        Dispose();
    }

    private void Dispose()
    {
        _vertexBuffer.Dispose();
        _vertexArray.Dispose();
        _shaderProgram.Dispose();
    }

    private void GridGen()
    {
        _color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        for (var y = -_gridSize; y <= _gridSize; y++)
        {
            if (y % 10 == 0)
            {
                _color = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            }
            
            if (y == 0)
            {
                _color = new Color4(0.8f, 0.2f, 0.2f, 1);
            }
            _gridVertices[y + _gridSize] = new VertexPositionColor(new Vector3(y, 0, -_gridSize), _color);
            _gridVertices[y + _gridSize + 1] = new VertexPositionColor(new Vector3(y, 0, _gridSize), _color);
        }

        for (var x = -_gridSize; x < _gridSize; x++)
        {
            _color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            if (x % 10 == 0)
            {
                _color = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            }

            if (x == 0)
            {
                _color = new Color4(0.2f, 0.2f, 0.8f, 1);
            }
            
            _gridVertices[x + _gridSize * 2] = new VertexPositionColor(new Vector3(-_gridSize, 0, x), _color);
            _gridVertices[x + _gridSize * 2 + 1] = new VertexPositionColor(new Vector3(_gridSize, 0, x), _color);
        }
    }

    public void DrawGrid()
    {
        GL.UseProgram(_shaderProgram.ShaderProgramHandle);
        GL.BindVertexArray(_vertexArray.VertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.VertexBufferHandle);
        GL.DrawArrays(PrimitiveType.Lines, 0, _gridVertices.Length);
    }
}