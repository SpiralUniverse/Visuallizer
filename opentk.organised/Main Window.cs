using System.Drawing;
using NCalc;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ImGuiNET;

namespace opentk.organised;

public class MainWindow : GameWindow
{
    
    #region -> Variables <-
    

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

        IsVisible = true;
        CursorState = CursorState.Grabbed;
        
        GL.ClearColor(Color.Gray);
        GL.Enable(EnableCap.DepthTest);
        
        const double resolution = 0.1;
        const double extents = 50;

        //Function fun1 = new Function(Guid.NewGuid(), "x - y", resolution, new Vector2d(extents, extents), Color4.Aqua);
        int nb;
        do
        {
            Console.WriteLine("pls enter a positive non zero integer that represent the nb of functions to draw!\nThe nb should be less than 10");
            nb = Convert.ToInt32(Console.ReadLine());
        } while (nb <= 0 || nb >= 10);

        for (int i = 0; i < nb; i++)
        {
            Console.WriteLine("pls input the function expression:");
            Function function = new Function(Guid.NewGuid(), Console.ReadLine(), resolution, new Vector2d(extents, extents), Color4.Aqua);
            //Note: the function ctr doesnt yet work with the color and similar fields
        }
    }

    protected override void OnUnload()
    {
        foreach (Function function in Function.functionsList.Values)
        {
            function.Dispose();
        }
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;
        if(IsKeyDown(Keys.Escape)) Close();
        
        float camSpeed = 2f;
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
        foreach (Function function in Function.functionsList.Values)
        {
            GL.UseProgram(function._shaderProgram.ShaderProgramHandle);
            GL.BindVertexArray(function._vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, function._vertexArray.VertexArrayHandle);
            function._shaderProgram.SetUniform("view", view);
            function._shaderProgram.SetUniform("model", model);
            function._shaderProgram.SetUniform("projection", projection);
            GL.DrawArrays(PrimitiveType.Triangles, 0, function.Vertices.Length);
        }
        
        GL.PointSize(7);
        GL.LineWidth(2);
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