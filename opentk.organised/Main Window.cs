﻿using System.Drawing;
using NCalc;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace opentk.organised;

public class MainWindow : GameWindow
{
    
    #region -> Variables <-

        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private VertexArray _vertexArray;
        private ShaderProgram _shaderProgram;
        private Position2D[] _vertices;

        private readonly Camera _cam;
        private Grid _grid;
        private Vector2 _lastPos;
        private bool _firstMove = true;

        #endregion
    
    public MainWindow() : base(
        new GameWindowSettings
        {
            RenderFrequency = 60,
            UpdateFrequency = 60
        },
        new NativeWindowSettings
        {
            Location = new Vector2i(910,540),
            Size = new Vector2i(1720,880),
            Title = "Main Window",
            WindowBorder = WindowBorder.Hidden,
            WindowState = WindowState.Normal,
            StartVisible = false,
            StartFocused = true,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 3)
        })
    { 
        CenterWindow();
        _cam = new Camera(Vector3.UnitY * 2, ClientSize.X / (float)ClientSize.Y);
        _grid = new Grid(1024);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(0, 0, e.Width, e.Height);
        _cam.AspectRatio = Size.X / (float)Size.Y;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        string f = "sin(x) + cos(y)";//Console.ReadLine();
        IsVisible = true;
        CursorState = CursorState.Grabbed;
        
        GL.ClearColor(Color.Gray);
        GL.Enable(EnableCap.DepthTest);
        Random rand = new();

        Expression exp = new Expression(f, EvaluateOptions.IgnoreCase);

        const double resolution = 0.01;
        const double extents = 5;

        long verticesCount = 0;

        for (var i = -extents; i < extents; i += resolution)
        {
            for (var j = -extents; j < extents; j += resolution)
            {
                verticesCount += 6;
            }
        }

        _vertices = new Position2D[verticesCount];

        #region -> RecVerts <-

            /*new(new(-.5f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //front plane
            new(new( .0f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new( .0f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new(-.5f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            
            new(new(-.5f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //back plane
            new(new( .0f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new( .0f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new(-.5f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f))*/
        

        #endregion
        
        var k = 0;
        for (var i = -extents; i < extents; i += resolution)
        {
            for (var j = -extents; j < extents; j += resolution)
            {
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j + 0.1f));

                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j));
                _vertices[k++] = new Position2D(new Vector2((float)i + 0.1f, (float)j + 0.1f));
                _vertices[k++] = new Position2D(new Vector2((float)i, (float)j + 0.1f));
            }
        }
        

        _vertexBuffer = new VertexBuffer(Position2D.VertexInfo, _vertices.Length);
        _vertexBuffer.SetData(_vertices, _vertices.Length);

        _vertexArray = new VertexArray(_vertexBuffer);
        _shaderProgram = new ShaderProgram(File.ReadAllText("vert.glsl"), File.ReadAllText("frag.glsl"));
    }

    protected override void OnUnload()
    {
        _vertexArray.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();
        
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;
        if(IsKeyDown(Keys.Escape)) Close();
        
        float camSpeed = 1.5f;
        float sensitivity = 0.2f;
        
        base.OnUpdateFrame(args);

        ProcessInput(ref camSpeed, args);
        ProcessMouse(ref sensitivity);

    }

    protected override void OnFocusedChanged(FocusedChangedEventArgs e)
    {
        base.OnFocusedChanged(e);
        _lastPos = new Vector2(MouseState.X, MouseState.Y);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        var model = Matrix4.Identity;
        var projection = _cam.GetProjectionMatrix();
        var view = _cam.GetViewMatrix();

        _grid.DrawGrid(new Matrix4[] {view, model, projection});
        
        GL.UseProgram(_shaderProgram.ShaderProgramHandle);
        GL.BindVertexArray(_vertexArray.VertexArrayHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer.VertexBufferHandle);
        
        _shaderProgram.SetUniform("view", view);
        _shaderProgram.SetUniform("model", model);
        _shaderProgram.SetUniform("projection", projection);
        
        GL.PointSize(7);
        GL.LineWidth(2);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);
        Context.SwapBuffers();
    }
    
    private void ProcessInput(ref float camSpeed, FrameEventArgs e)
    {
 
        if (IsKeyDown(Keys.W))
        {
            _cam.Position += _cam.Front * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.S))
        {
            _cam.Position -= _cam.Front * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.A))
        {
            _cam.Position -= _cam.Right * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.D))
        {
            _cam.Position += _cam.Right * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.Space))
        {
            _cam.Position += _cam.Up * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.LeftShift))
        {
            _cam.Position -= _cam.Up * camSpeed * (float)e.Time;
        }

        if (IsKeyDown(Keys.Escape))
        {
            Environment.Exit(0);
        }
    }

    private void ProcessMouse(ref float sensitivity)
    {
        if (_firstMove)
        {
            _lastPos = new Vector2(MouseState.X, MouseState.Y);
            _firstMove = false;
        }
        else
        {
            // Calculate the offset of the MouseState position
            var deltaX = MouseState.X - _lastPos.X;
            var deltaY = MouseState.Y - _lastPos.Y;
            _lastPos = new Vector2(MouseState.X, MouseState.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            _cam.Yaw += deltaX * sensitivity;
            _cam.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
    
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        _cam.Fov -= e.OffsetY;
    }

}