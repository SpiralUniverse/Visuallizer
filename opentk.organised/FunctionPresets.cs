using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenTK.Mathematics;

namespace opentk.organised;

public static class FunctionPresets
{
    public class PresetFunction
    {
        public string Name { get; set; } = string.Empty;
        public string Expression { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public static readonly Dictionary<string, List<PresetFunction>> Categories = new()
    {
        ["Wave Functions"] = new List<PresetFunction>
        {
            new() { Name = "Simple Wave", Expression = "sin(x)*cos(y)", Description = "Basic sine-cosine wave pattern" },
            new() { Name = "Ripple Effect", Expression = "sin(sqrt(x*x + y*y)) * exp(-sqrt(x*x + y*y)/10)", Description = "Circular ripples with decay" },
            new() { Name = "Interference", Expression = "sin(x) + sin(y) + 0.5*sin(x+y)", Description = "Wave interference pattern" }
        },
        
        ["Mathematical Classics"] = new List<PresetFunction>
        {
            new() { Name = "Mexican Hat", Expression = "(2 - (x*x + y*y)) * exp(-(x*x + y*y)/2)", Description = "Classic Ricker wavelet" },
            new() { Name = "Gaussian Bell", Expression = "exp(-(x*x + y*y)/4)", Description = "2D Gaussian distribution" },
            new() { Name = "Saddle Point", Expression = "x*x - y*y", Description = "Hyperbolic paraboloid" }
        },
        
        ["Complex Patterns"] = new List<PresetFunction>
        {
            new() { Name = "Vortex", Expression = "sin(atan2(y,x)*3 + sqrt(x*x + y*y)) * (10/(1 + x*x + y*y))", Description = "Spiral vortex pattern" },
            new() { Name = "Complex Wave", Expression = "sin(x*x + y*y) + 0.5*cos(x*3)*sin(y*3)", Description = "Complex interference pattern" },
            new() { Name = "Fractal-like", Expression = "sin(x) + sin(x*2)/2 + sin(x*4)/4 + cos(y) + cos(y*2)/2 + cos(y*4)/4", Description = "Multi-frequency composition" }
        },
        
        ["Exotic Functions"] = new List<PresetFunction>
        {
            new() { Name = "Rose Pattern", Expression = "cos(3*atan2(y,x)) * sqrt(x*x + y*y) * exp(-sqrt(x*x + y*y)/5)", Description = "Rose-like pattern" },
            new() { Name = "Lattice", Expression = "sin(x*3)*sin(y*3) + 0.3*sin(x*9)*sin(y*9)", Description = "Crystal lattice structure" },
            new() { Name = "Tornado", Expression = "exp(-((x-sin(y/2)*2)*(x-sin(y/2)*2) + (y*0.5)*(y*0.5))/3) * sin(y*2)", Description = "Tornado-like spiral" }
        }
    };

    public static void DisplayPresets()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                    📚 Function Library                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.WriteLine();

        int index = 1;
        foreach (var category in Categories)
        {
            Console.WriteLine($"🎨 {category.Key}:");
            foreach (var preset in category.Value)
            {
                Console.WriteLine($"   {index,2}. {preset.Name}");
                Console.WriteLine($"       f(x,y) = {preset.Expression}");
                Console.WriteLine($"       {preset.Description}");
                Console.WriteLine();
                index++;
            }
        }
    }

    public static PresetFunction? GetPresetByIndex(int index)
    {
        int currentIndex = 1;
        foreach (var category in Categories)
        {
            foreach (var preset in category.Value)
            {
                if (currentIndex == index)
                    return preset;
                currentIndex++;
            }
        }
        return null;
    }

    public static int GetTotalPresets()
    {
        return Categories.Values.Sum(list => list.Count);
    }
}

// Vertex data caching system
public static class VertexCache
{
    private static readonly string CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MathVisualizer", "Cache");
    
    static VertexCache()
    {
        Directory.CreateDirectory(CacheDirectory);
    }

    private static string GetCacheKey(string expression, double resolution, Vector2d extents, bool useAdaptive)
    {
        using var sha256 = SHA256.Create();
        var input = $"{expression}_{resolution}_{extents.X}_{extents.Y}_{useAdaptive}";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16]; // Use first 16 chars
    }

    public static string GetCacheFilePath(string expression, double resolution, Vector2d extents, bool useAdaptive)
    {
        var key = GetCacheKey(expression, resolution, extents, useAdaptive);
        return Path.Combine(CacheDirectory, $"vertices_{key}.cache");
    }

    public static bool TryLoadVertices(string expression, double resolution, Vector2d extents, bool useAdaptive, out Position2D[] vertices, out float minHeight, out float maxHeight)
    {
        vertices = Array.Empty<Position2D>();
        minHeight = 0f;
        maxHeight = 0f;

        try
        {
            var cacheFile = GetCacheFilePath(expression, resolution, extents, useAdaptive);
            if (!File.Exists(cacheFile))
                return false;

            using var reader = new BinaryReader(File.OpenRead(cacheFile));
            
            // Read metadata
            var version = reader.ReadInt32();
            if (version != 1) return false; // Version mismatch
            
            minHeight = reader.ReadSingle();
            maxHeight = reader.ReadSingle();
            var count = reader.ReadInt32();

            // Read vertices
            vertices = new Position2D[count];
            for (int i = 0; i < count; i++)
            {
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                vertices[i] = new Position2D(new Vector2(x, y));
            }

            Console.WriteLine($"📦 Loaded {count:N0} vertices from cache");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void SaveVertices(string expression, double resolution, Vector2d extents, bool useAdaptive, Position2D[] vertices, float minHeight, float maxHeight)
    {
        try
        {
            var cacheFile = GetCacheFilePath(expression, resolution, extents, useAdaptive);
            using var writer = new BinaryWriter(File.OpenWrite(cacheFile));
            
            // Write metadata
            writer.Write(1); // Version
            writer.Write(minHeight);
            writer.Write(maxHeight);
            writer.Write(vertices.Length);

            // Write vertices
            foreach (var vertex in vertices)
            {
                writer.Write(vertex.Position.X);
                writer.Write(vertex.Position.Y);
            }

            Console.WriteLine($"💾 Cached {vertices.Length:N0} vertices");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to cache vertices: {ex.Message}");
        }
    }
}
