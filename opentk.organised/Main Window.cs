using System;
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
            Title = "Mathematical Function Visualizer - Professional Edition",
            WindowBorder = WindowBorder.Resizable,
            WindowState = WindowState.Normal,
            StartVisible = false,
            StartFocused = true,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 3)
        })
    { 
        CenterWindow();
        // Position camera to look down at the function from a good viewing angle
        _cam = new Camera(new Vector3(20, 15, 20), ClientSize.X / (float)ClientSize.Y);
        _cam.Pitch = -25f; // Look down at the function
        _cam.Yaw = -135f;  // Angle to see the function nicely
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
        
        // Professional dark theme background
        GL.ClearColor(0.08f, 0.08f, 0.12f, 1.0f); // Dark blue-gray
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.LineSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        
        // Additional anti-flickering settings
        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.PolygonOffset(1.0f, 1.0f); // Prevent z-fighting
        GL.DepthRange(0.0, 1.0);
        
        const double resolution = 0.1;
        const double extents = 200; // Extended for more "infinite" feel

        //Function fun1 = new Function(Guid.NewGuid(), "x - y", resolution, new Vector2d(extents, extents), Color4.Aqua);
        int nb;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Mathematical Function Visualizer v2.0              ║");
        Console.WriteLine("║        Professional 3D Function Visualization             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        // Ask for input method
        Console.WriteLine("Choose input method:");
        Console.WriteLine("1. 📚 Use built-in function presets");
        Console.WriteLine("2. ✏️  Enter custom mathematical expressions");
        Console.WriteLine("3. 🧪 Test all preset functions (diagnostic mode)");
        Console.Write("► Select option (1, 2, or 3): ");
        
        string choice = Console.ReadLine() ?? "2";
        bool usePresets = choice == "1";
        bool testMode = choice == "3";
        
        if (testMode)
        {
            TestAllPresets();
            return;
        }
        
        do
        {
            Console.WriteLine("\n📊 Enter the number of functions to visualize (1-9):");
            Console.Write("► ");
            nb = Convert.ToInt32(Console.ReadLine());
            if (nb <= 0 || nb >= 10)
            {
                Console.WriteLine("❌ Please enter a valid number between 1 and 9.\n");
            }
        } while (nb <= 0 || nb >= 10);

        Console.WriteLine();
        
        if (usePresets)
        {
            FunctionPresets.DisplayPresets();
            Console.WriteLine($"Choose {nb} function(s) from the library above:");
        }

        for (int i = 0; i < nb; i++)
        {
            string expression;
            
            if (usePresets)
            {
                int totalPresets = FunctionPresets.GetTotalPresets();
                int presetChoice;
                do
                {
                    Console.Write($"► Select preset #{i + 1} (1-{totalPresets}): ");
                    presetChoice = Convert.ToInt32(Console.ReadLine());
                    if (presetChoice < 1 || presetChoice > totalPresets)
                    {
                        Console.WriteLine($"❌ Please enter a number between 1 and {totalPresets}");
                    }
                } while (presetChoice < 1 || presetChoice > totalPresets);
                
                var preset = FunctionPresets.GetPresetByIndex(presetChoice);
                expression = preset?.Expression ?? "sin(x)*cos(y)";
                Console.WriteLine($"✨ Selected: {preset?.Name} - {preset?.Description}");
            }
            else
            {
                Console.WriteLine($"🔢 Enter mathematical expression for function #{i + 1}:");
                Console.WriteLine("   Examples: sin(x)*cos(y), x^2 + y^2, exp(-x^2-y^2)*cos(x*y)");
                Console.Write("► f(x,y) = ");
                expression = Console.ReadLine() ?? "sin(x)*cos(y)";
            }
            
            Function function = new Function(Guid.NewGuid(), expression, resolution, new Vector2d(extents, extents), Color4.Aqua);
            Console.WriteLine($"✅ Function #{i + 1} loaded successfully!\n");
        }
        
        Console.WriteLine("🚀 Launching 3D visualization...");
        Console.WriteLine("🎮 Controls: WASD to move, Mouse to look around, ESC to exit");
        Console.WriteLine("💾 Vertex data is automatically cached for faster loading next time!");
        Console.WriteLine();
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

        // Draw functions first (solid objects)
        GL.Disable(EnableCap.Blend);
        foreach (Function function in Function.functionsList.Values)
        {
            GL.UseProgram(function._shaderProgram.ShaderProgramHandle);
            GL.BindVertexArray(function._vertexArray.VertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, function._vertexBuffer.VertexBufferHandle);
            function.SetShaderUniforms(model, view, projection);
            GL.DrawArrays(PrimitiveType.Triangles, 0, function.Vertices.Length);
        }
        
        // Draw grid last (transparent overlay) with stronger depth offset
        GL.Enable(EnableCap.Blend);
        GL.DepthMask(false); // Don't write to depth buffer for grid
        GL.PolygonOffset(-2.0f, -2.0f); // Stronger negative offset to push grid behind
        _grid.DrawGrid(new Matrix4[] {view, model, projection});
        GL.DepthMask(true); // Re-enable depth writing
        GL.PolygonOffset(1.0f, 1.0f); // Reset to default
        
        // Professional rendering settings
        GL.PointSize(5);
        GL.LineWidth(1.5f);
        
        // Enable multisampling for smoother edges if available
        GL.Enable(EnableCap.Multisample);
        
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
    
    private void TestAllPresets()
    {
        Console.WriteLine("\n🧪 TESTING ALL PRESET FUNCTIONS");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        
        var presets = FunctionPresets.GetAllPresets();
        var results = new List<(string Name, string Expression, bool Success, string? Error, long VertexCount, TimeSpan Duration)>();
        
        foreach (var (name, expression, description) in presets)
        {
            Console.WriteLine($"\n🔍 Testing: {name}");
            Console.WriteLine($"   Expression: {expression}");
            Console.WriteLine($"   Description: {description}");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // Create test function with smaller extents for faster testing
                var testFunction = new Function(
                    Guid.NewGuid(), 
                    expression, 
                    resolution: 0.2, // Coarser resolution for testing
                    extents: new Vector2d(50, 50), // Smaller area
                    Color4.White
                );
                
                stopwatch.Stop();
                var vertexCount = testFunction.verticesCount;
                
                results.Add((name, expression, true, null, vertexCount, stopwatch.Elapsed));
                Console.WriteLine($"   ✅ Success: {vertexCount:N0} vertices in {stopwatch.Elapsed.TotalSeconds:F2}s");
                
                // Clean up
                testFunction.Dispose();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.Add((name, expression, false, ex.Message, 0, stopwatch.Elapsed));
                Console.WriteLine($"   ❌ Failed: {ex.Message}");
            }
        }
        
        // Summary report
        Console.WriteLine("\n📊 TEST SUMMARY REPORT");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        
        var successful = results.Where(r => r.Success).ToList();
        var failed = results.Where(r => !r.Success).ToList();
        
        Console.WriteLine($"✅ Successful: {successful.Count}/{results.Count}");
        Console.WriteLine($"❌ Failed: {failed.Count}/{results.Count}");
        Console.WriteLine($"⚡ Average time: {results.Where(r => r.Success).Average(r => r.Duration.TotalSeconds):F2}s");
        Console.WriteLine($"📊 Total vertices: {successful.Sum(r => r.VertexCount):N0}");
        
        if (failed.Any())
        {
            Console.WriteLine("\n❌ FAILED FUNCTIONS:");
            foreach (var (name, expression, _, error, _, duration) in failed)
            {
                Console.WriteLine($"   • {name}: {error}");
            }
        }
        
        Console.WriteLine("\n⚡ PERFORMANCE RANKING (by generation time):");
        foreach (var (name, _, _, _, vertexCount, duration) in successful.OrderBy(r => r.Duration))
        {
            Console.WriteLine($"   • {name}: {duration.TotalSeconds:F2}s ({vertexCount:N0} vertices)");
        }
        
        Console.WriteLine($"\n💾 Cache location: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/MathVisualizer/Cache/");
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }

}