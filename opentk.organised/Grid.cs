using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.IO;

namespace opentk.organised;

public class Grid
{
    
    #region -> Fields and Getters <-

        private readonly uint _gridSize;
        public uint GridSize => _gridSize;
        
        private Color4 _color;
        public Color4 Color => _color;
        
        private readonly VertexPositionColor[] _gridVertices;
        private VertexArray _vertexArray = null!;
        private VertexBuffer _vertexBuffer = null!;
        private ShaderProgram _shaderProgram = null!;

    #endregion

    public Grid(uint gridSize)
    {
        _gridSize = gridSize;
        _gridVertices = new VertexPositionColor[_gridSize * 8 + 2];
        _color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
        GridGen();
        Initiate();
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
    }

    private void GridGen()
    {
        var vert = 0;
        for (var y = -_gridSize; y <= _gridSize; y++)
        {
            // Subtle grid lines - much easier on the eyes
            _color = new Color4(0.15f, 0.18f, 0.22f, 0.6f); // Subtle blue-gray
            if (y % 10 == 0)
            {
                _color = new Color4(0.25f, 0.28f, 0.32f, 0.8f); // Slightly brighter for major lines
            }
            
            if (y == 0)
            {
                // Elegant X-axis in soft red
                _color = new Color4(0.6f, 0.3f, 0.3f, 0.9f);
            }
            
            _gridVertices[vert++] = new VertexPositionColor(new Vector3(y, 0, -_gridSize), _color);
            _gridVertices[vert++] = new VertexPositionColor(new Vector3(y, 0, _gridSize), _color);
        }

        for (var x = -_gridSize; x < _gridSize; x++)
        {
            _color = new Color4(0.15f, 0.18f, 0.22f, 0.6f);
            if (x % 10 == 0)
            {
                _color = new Color4(0.25f, 0.28f, 0.32f, 0.8f);
            }

            if (x == 0)
            {
                // Elegant Z-axis in soft blue
                _color = new Color4(0.3f, 0.4f, 0.6f, 0.9f);
            }
            
            _gridVertices[vert++] = new VertexPositionColor(new Vector3(-_gridSize, 0, x), _color);
            _gridVertices[vert++] = new VertexPositionColor(new Vector3(_gridSize, 0, x), _color);
        }
    }

    public void DrawGrid(Matrix4[] mat)
    {
        GL.UseProgram(_shaderProgram.ShaderProgramHandle);
        GL.BindVertexArray(_vertexArray.VertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.VertexBufferHandle);
        
        _shaderProgram.SetUniform("view", mat[0]);
        _shaderProgram.SetUniform("model", mat[1]);
        _shaderProgram.SetUniform("projection", mat[2]);

        GL.DrawArrays(PrimitiveType.Lines, 0, _gridVertices.Length);
    }
}