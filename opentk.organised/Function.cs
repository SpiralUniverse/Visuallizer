using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

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
        public bool IsGradientColor { get; set; }

        private Enum? dimension;
        public Enum? Dimension { get; set; }
        
        private Position2D[] _vertices = null!;
        public Position2D[] Vertices => _vertices;

        private Color4 _color;
        public Color4 Color => _color;

    #endregion

    #region -> Fields <-

        public int lineThicness;
        public double resolution;
        public Vector2d extents;
        public long verticesCount;
        public VertexBuffer _vertexBuffer = null!;
        public ShaderProgram _shaderProgram = null!;
        public VertexArray _vertexArray = null!;
        
        private float _minHeight;
        private float _maxHeight;
        public float MinHeight => _minHeight;
        public float MaxHeight => _maxHeight;
        
        // Adaptive resolution fields
        private bool useAdaptiveResolution = false;
        private double stepSize;
        private double maxResolution;
        private double minResolution;

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
        
        // Initialize adaptive resolution parameters
        useAdaptiveResolution = true;
        stepSize = resolution * 2.0; // Start with larger steps
        maxResolution = resolution * 0.05;  // Much higher detail for complex areas
        minResolution = resolution * 4.0;   // Lower detail for flat areas
        
        functionsList.Add(_guid, this);
        UpdateVertices();
        CreateBuffer();
        Console.WriteLine($"Function created: {guid}");
        Console.WriteLine($"Generated {verticesCount} vertices with {(useAdaptiveResolution ? "adaptive" : "uniform")} resolution");
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
    
    public void SetShaderUniforms(Matrix4 model, Matrix4 view, Matrix4 projection)
    {
        _shaderProgram.SetUniform("view", view);
        _shaderProgram.SetUniform("model", model);
        _shaderProgram.SetUniform("projection", projection);
        _shaderProgram.SetUniform("minHeight", _minHeight);
        _shaderProgram.SetUniform("maxHeight", _maxHeight);
    }
    
    public void UpdateVertices()
    {
        // Try to load from cache first
        if (VertexCache.TryLoadVertices(_expression, resolution, extents, useAdaptiveResolution, out Position2D[] cachedVertices, out float cachedMinHeight, out float cachedMaxHeight))
        {
            _vertices = cachedVertices;
            _minHeight = cachedMinHeight;
            _maxHeight = cachedMaxHeight;
            verticesCount = _vertices.Length;
            return;
        }

        // Generate vertices if not cached
        Console.WriteLine("🔄 Generating vertices...");
        var vertices = new List<Position2D>();
        
        if (useAdaptiveResolution)
        {
            GenerateAdaptiveVertices(vertices);
        }
        else
        {
            GenerateUniformVertices(vertices);
        }
        
        _vertices = vertices.ToArray();
        verticesCount = _vertices.Length;
        
        // Calculate height range for heat map coloring
        CalculateHeightRange();
        
        // Cache the generated vertices
        VertexCache.SaveVertices(_expression, resolution, extents, useAdaptiveResolution, _vertices, _minHeight, _maxHeight);
    }
    
    private void GenerateAdaptiveVertices(List<Position2D> vertices)
    {
        // Calculate total steps for progress tracking
        var totalStepsX = (int)((extents.X * 2) / stepSize);
        var totalStepsY = (int)((extents.Y * 2) / stepSize);
        var totalSteps = totalStepsX * totalStepsY;
        var currentStep = 0;
        var lastProgressPercent = -1;

        for (var i = -extents.X; i < extents.X; i += stepSize)
        {
            for (var j = -extents.Y; j < extents.Y; j += stepSize)
            {
                // Modern C# 13: Target-typed new expressions and collection expressions
                Vector2[] corners = [
                    new((float)i, (float)j),                    // topLeft
                    new((float)(i + stepSize), (float)j),       // topRight
                    new((float)i, (float)(j + stepSize)),       // bottomLeft
                    new((float)(i + stepSize), (float)(j + stepSize)) // bottomRight
                ];
                
                // Subdivide this quad recursively based on function complexity
                SubdivideQuad(corners[0], corners[1], corners[2], corners[3], 4, vertices);
                
                // Progress indication with modern string interpolation
                currentStep++;
                var progressPercent = (int)((double)currentStep / totalSteps * 100);
                if (progressPercent != lastProgressPercent && progressPercent % 10 == 0)
                {
                    Console.Write($"\r🔄 Generating vertices... {progressPercent}% ({vertices.Count:N0} vertices so far)");
                    lastProgressPercent = progressPercent;
                }
            }
        }
        Console.WriteLine(); // New line after progress
    }
    
    private void GenerateUniformVertices(List<Position2D> vertices)
    {
        for (var i = -extents.X; i < extents.X; i += resolution)
        {
            for (var j = -extents.Y; j < extents.Y; j += resolution)
            {
                AddQuad(vertices, (float)i, (float)j, (float)resolution);
            }
        }
    }
    
    private void CalculateHeightRange()
    {
        _minHeight = float.MaxValue;
        _maxHeight = float.MinValue;
        
        // Sample the function to find height range
        for (var i = -extents.X; i < extents.X; i += resolution * 2)
        {
            for (var j = -extents.Y; j < extents.Y; j += resolution * 2)
            {
                float height = EvaluateFunction((float)i, (float)j);
                _minHeight = Math.Min(_minHeight, height);
                _maxHeight = Math.Max(_maxHeight, height);
            }
        }
    }
    
    private double GetAdaptiveResolution(double distance)
    {
        // Logarithmic scaling: closer = higher resolution
        double factor = Math.Log(1 + distance / 10.0);
        return Math.Max(maxResolution, Math.Min(minResolution, resolution * factor));
    }
    
    private double CheckGradientResolution(double x, double y)
    {
        try
        {
            // Sample function at current point and neighbors to check gradient
            float center = EvaluateFunction((float)x, (float)y);
            float right = EvaluateFunction((float)(x + resolution), (float)y);
            float up = EvaluateFunction((float)x, (float)(y + resolution));
            
            float gradientMagnitude = Math.Abs(right - center) + Math.Abs(up - center);
            
            // Higher gradient = need more resolution
            if (gradientMagnitude > 1.0f)
                return maxResolution; // High detail
            else if (gradientMagnitude > 0.5f)
                return resolution * 0.5; // Medium detail
            else
                return resolution; // Normal detail
        }
        catch
        {
            return resolution; // Default if evaluation fails
        }
    }
    
    private float EvaluateFunction(float x, float y)
    {
        try
        {
            var expr = new NCalc.Expression(_expression);
            expr.Parameters["x"] = (double)x;
            expr.Parameters["y"] = (double)y;
            
            // C# 13: Modern mathematical function handler with pattern matching
            expr.EvaluateFunction += (name, args) =>
            {
                var param1 = (double)args.Parameters[0].Evaluate();
                var param2 = args.Parameters.Length > 1 ? (double)args.Parameters[1].Evaluate() : 0.0;
                
                args.Result = name.ToLower() switch
                {
                    "sin" => Math.Sin(param1),
                    "cos" => Math.Cos(param1),
                    "tan" => Math.Tan(param1),
                    "sqrt" => Math.Sqrt(param1),
                    "abs" => Math.Abs(param1),
                    "log" => Math.Log(param1),
                    "exp" => Math.Exp(param1),
                    "pow" => Math.Pow(param1, param2),
                    "atan2" => Math.Atan2(param1, param2),
                    "min" => Math.Min(param1, param2),
                    "max" => Math.Max(param1, param2),
                    _ => throw new ArgumentException($"Unknown function: {name}")
                };
            };
            
            var result = expr.Evaluate();
            if (result != null && double.TryParse(result.ToString(), out double height))
            {
                return (float)height;
            }
            return 0f;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error evaluating function '{_expression}' at ({x}, {y}): {ex.Message}");
            return 0f;
        }
    }
    
    private void SubdivideQuad(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, int depth, List<Position2D> vertices)
    {
        // Base case: if we've reached max depth or quad is too small
        if (depth <= 0 || Vector2.Distance(topLeft, bottomRight) < maxResolution)
        {
            // Add the quad as two triangles
            AddTriangle(vertices, topLeft, topRight, bottomLeft);
            AddTriangle(vertices, topRight, bottomRight, bottomLeft);
            return;
        }
        
        // Calculate function values at corners
        float tlHeight = EvaluateFunction(topLeft.X, topLeft.Y);
        float trHeight = EvaluateFunction(topRight.X, topRight.Y);
        float blHeight = EvaluateFunction(bottomLeft.X, bottomLeft.Y);
        float brHeight = EvaluateFunction(bottomRight.X, bottomRight.Y);
        
        // Calculate center point
        Vector2 center = (topLeft + topRight + bottomLeft + bottomRight) * 0.25f;
        float centerHeight = EvaluateFunction(center.X, center.Y);
        
        // Check if we need subdivision based on height variation
        float minHeight = Math.Min(Math.Min(tlHeight, trHeight), Math.Min(blHeight, brHeight));
        float maxHeight = Math.Max(Math.Max(tlHeight, trHeight), Math.Max(blHeight, brHeight));
        float heightVariation = maxHeight - minHeight;
        
        // Also check how much the center differs from the average of corners
        float avgCornerHeight = (tlHeight + trHeight + blHeight + brHeight) * 0.25f;
        float centerDeviation = Math.Abs(centerHeight - avgCornerHeight);
        
        // Subdivide if there's significant height variation or center deviation
        if (heightVariation > 0.2f || centerDeviation > 0.1f)
        {
            // Calculate midpoints
            Vector2 topMid = (topLeft + topRight) * 0.5f;
            Vector2 bottomMid = (bottomLeft + bottomRight) * 0.5f;
            Vector2 leftMid = (topLeft + bottomLeft) * 0.5f;
            Vector2 rightMid = (topRight + bottomRight) * 0.5f;
            
            // Recursively subdivide into 4 smaller quads
            SubdivideQuad(topLeft, topMid, leftMid, center, depth - 1, vertices);
            SubdivideQuad(topMid, topRight, center, rightMid, depth - 1, vertices);
            SubdivideQuad(leftMid, center, bottomLeft, bottomMid, depth - 1, vertices);
            SubdivideQuad(center, rightMid, bottomMid, bottomRight, depth - 1, vertices);
        }
        else
        {
            // No need for further subdivision, add the quad
            AddTriangle(vertices, topLeft, topRight, bottomLeft);
            AddTriangle(vertices, topRight, bottomRight, bottomLeft);
        }
    }
    
    private void AddTriangle(List<Position2D> vertices, Vector2 a, Vector2 b, Vector2 c)
    {
        vertices.Add(new Position2D(a));
        vertices.Add(new Position2D(b));
        vertices.Add(new Position2D(c));
    }
    
    private void AddQuad(List<Position2D> vertices, float x, float y, float size)
    {
        // Create two triangles to form a quad
        vertices.Add(new Position2D(new Vector2(x, y)));
        vertices.Add(new Position2D(new Vector2(x + size, y)));
        vertices.Add(new Position2D(new Vector2(x, y + size)));

        vertices.Add(new Position2D(new Vector2(x + size, y)));
        vertices.Add(new Position2D(new Vector2(x + size, y + size)));
        vertices.Add(new Position2D(new Vector2(x, y + size)));
    }
}