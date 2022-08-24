using System.Drawing;
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
        private VertexPositionColor[] _vertices;

        private readonly Camera _cam;
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
        
        _vertices = new VertexPositionColor[2000];
        /*new(new(-.5f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //front plane
        new(new( .0f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
        new(new( .0f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
        new(new(-.5f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
        
        new(new(-.5f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //back plane
        new(new( .0f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
        new(new( .0f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
        new(new(-.5f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f))*/
        int k = 0;
        for (float i = 0; i < 20; i += 0.001f)
        {
            Vector3 position = Vector3.Zero;
            exp.Parameters["x"] = i;
            position.X = i;
            for (float j = 0; j < 20; j += 0.001f)
            {
                exp.Parameters["y"] = j;
                position.Z = j;
                position.Y = (float) Convert.ToDouble(exp.Evaluate().ToString());
                _vertices[k++] = new VertexPositionColor(position,//BUG: one right here!
                    new Color4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f));
            }

        }

        /*Expression e = new Expression(f, EvaluateOptions.IgnoreCase);
        vertices = new VertexPositionColor[4000];
        for (int i = 0, j = -2000; i < 2000; i++, j++)
        {
            e.Parameters["x"] = j;
            vertices[i] = new VertexPositionColor(new Vector3(j, (float)Convert.ToDouble(e.Evaluate()), 0.0f), Color4.Firebrick);
        }*/

        int[] indices = {
            0, 1, 2, //front
            2, 3, 0,
            
            1, 5, 6, //right
            6, 2, 1,
            
            5, 4, 7, //back
            7, 6, 5,
            
            4, 0, 3, //left
            3, 7, 4,
            
            4, 5, 1, //up
            1, 0, 4,
            
            3, 2, 6, //bottom
            6, 7, 3
        };

        _vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, _vertices.Length);
        _vertexBuffer.SetData(_vertices, _vertices.Length);

        _indexBuffer = new IndexBuffer(indices.Length);
        _indexBuffer.SetData(indices, indices.Length);

        _vertexArray = new VertexArray(_vertexBuffer);
        _shaderProgram = new ShaderProgram(File.ReadAllText("vert.glsl"), File.ReadAllText("frag.glsl"));
    }

    protected override void OnUnload()
    {
        _vertexArray.Dispose();
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();
        _shaderProgram.Dispose();
        
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
        GL.UseProgram(_shaderProgram.ShaderProgramHandle);
        GL.BindVertexArray(_vertexArray.VertexArrayHandle);
        //GL.BindBuffer(BufferTarget.ElementArrayBuffer,indexBuffer.IndexBufferHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer,_vertexBuffer.VertexBufferHandle);
        Matrix4 model = Matrix4.Identity;
        Matrix4 projection = _cam.GetProjectionMatrix();
        Matrix4 view = _cam.GetViewMatrix();

        _shaderProgram.SetUniform("view", view);
        _shaderProgram.SetUniform("model", model);
        _shaderProgram.SetUniform("projection", projection);
        
        //GL.DrawElements(PrimitiveType.Triangles, 36,DrawElementsType.UnsignedInt, 0);
        //GL.PointSize(8);
        GL.DrawArrays(PrimitiveType.Points, 0, _vertices.Length);
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