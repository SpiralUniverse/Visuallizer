using System.Drawing;
using NCalc;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace opentk.organised;

public class Main_Window : GameWindow
{
    
    #region -> Variables <-

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private ShaderProgram shaderProgram;
        private VertexPositionColor[] vertices;

        private Camera cam;
        private Vector2 _lastPos;
        private bool _firstMove = true;
        private double _time;

        #endregion
    
    public Main_Window() : base(
        new()
        {
            RenderFrequency = 60,
            UpdateFrequency = 60
        },
        new()
        {
            Location = new Vector2i(910,540),
            Size = new(1720,880),
            Title = "Main Window",
            WindowBorder = WindowBorder.Hidden,
            WindowState = WindowState.Normal,
            StartVisible = false,
            StartFocused = true,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new(4, 3)
        })
    { 
        CenterWindow();
        cam = new Camera(Vector3.UnitY * 2, ClientSize.X / (float)ClientSize.Y);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        
        GL.Viewport(0, 0, e.Width, e.Height);
        cam.AspectRatio = Size.X / (float)Size.Y;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        string f = "sin(x)";//Console.ReadLine();
        IsVisible = true;
        CursorState = CursorState.Grabbed;
        
        GL.ClearColor(Color.Gray);
        GL.Enable(EnableCap.DepthTest);
        Random rand = new();

        vertices = GridGen();//new VertexPositionColor[] {
            /*new(new(-.5f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //front plane
            new(new( .0f,  .5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new( .0f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new(-.5f, -.5f, .0f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            
            new(new(-.5f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),  //back plane
            new(new( .0f,  .5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new( .0f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f)),
            new(new(-.5f, -.5f, -.5f), new((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1.0f))*/
            
            /*new (new Vector3(-1f,  1f, 0.0f), new Color4(  1.0f, 0.0f, 0.0f, 1.0f)),
            new (new Vector3( 1f,  1f, 0.0f), new Color4( 0.0f, 1.0f, 0.0f, 1.0f)),
            new (new Vector3(-1f, -1f, 0.0f), new Color4(0.0f, 0.0f, 1.0f, 1.0f)),
            
            new (new Vector3(-1f, -1f, 0.0f), new Color4(0.0f, 0.0f, 1.0f, 1.0f)),
            new (new Vector3(1f,  1f, 0.0f), new Color4( 0.0f, 1.0f, 0.0f, 1.0f)),
            new (new Vector3(1f, -1f, 0.0f), new Color4(1.0f, 1.0f, 1.0f, 1.0f))*/

        //};

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

        vertexBuffer = new(VertexPositionColor.VertexInfo, vertices.Length);
        vertexBuffer.SetData(vertices, vertices.Length);

        indexBuffer = new(indices.Length);
        indexBuffer.SetData(indices, indices.Length);

        vertexArray = new(vertexBuffer);
        shaderProgram = new(File.ReadAllText("testvert.glsl"), File.ReadAllText("testfrag.glsl"));
    }

    protected override void OnUnload()
    {
        vertexArray.Dispose();
        indexBuffer.Dispose();
        vertexBuffer.Dispose();
        
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
        _time += 4.0 * args.Time;
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.UseProgram(shaderProgram.ShaderProgramHandle);
        GL.BindVertexArray(vertexArray.VertexArrayHandle);
        //GL.BindBuffer(BufferTarget.ElementArrayBuffer,indexBuffer.IndexBufferHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer,vertexBuffer.VertexBufferHandle);
        Matrix4 model = Matrix4.Identity;
        Matrix4 projection = cam.GetProjectionMatrix();
        Matrix4 view = cam.GetViewMatrix();

        shaderProgram.SetUniform("view", view);
        shaderProgram.SetUniform("model", model);
        shaderProgram.SetUniform("projection", projection);
        
        //GL.DrawElements(PrimitiveType.Triangles, 36,DrawElementsType.UnsignedInt, 0);
        //GL.PointSize(8);
        GL.DrawArrays(PrimitiveType.Lines, 0, vertices.Length);
        Context.SwapBuffers();
    }
    
    private void ProcessInput(ref float camSpeed, FrameEventArgs e)
    {
 
        if (IsKeyDown(Keys.W))
        {
            cam.Position += cam.Front * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.S))
        {
            cam.Position -= cam.Front * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.A))
        {
            cam.Position -= cam.Right * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.D))
        {
            cam.Position += cam.Right * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.Space))
        {
            cam.Position += cam.Up * camSpeed * (float)e.Time;
        }
 
        if (IsKeyDown(Keys.LeftShift))
        {
            cam.Position -= cam.Up * camSpeed * (float)e.Time;
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
            cam.Yaw += deltaX * sensitivity;
            cam.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
    
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        cam.Fov -= e.OffsetY;
    }


    private VertexPositionColor[] GridGen()
    {
        const int gridSize = 1024;
        var gridLines = new List<VertexPositionColor>();
        for (var y = -gridSize; y <= gridSize; y++)
        {
            var color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            if (y % 10 == 0)
            {
                color = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            }
            
            if (y == 0)
            {
                color = new Color4(0.8f, 0.2f, 0.2f, 1);
            }
            
            gridLines.Add(new VertexPositionColor(new Vector3(y, 0, -gridSize), color));
            gridLines.Add(new VertexPositionColor(new Vector3(y, 0, gridSize), color));
        }
        
        for (var x = -gridSize; x <= gridSize; x++)
        {
            var color = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            if (x % 10 == 0)
            {
                color = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            }

            if (x == 0)
            {
                color = new Color4(0.2f, 0.2f, 0.8f, 1);
            }
            
            gridLines.Add(new VertexPositionColor(new Vector3(-gridSize, 0, x), color));
            gridLines.Add(new VertexPositionColor(new Vector3(gridSize, 0, x), color));
        }

        return gridLines.ToArray();
    }
}