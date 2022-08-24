using NCalc;

namespace opentk.organised;

public class Functions
{
    private string functionStr;
    private Range functionRange;
    
    public Functions(string str, Range range)
    {
        functionStr = str;
        functionRange = range;
    }

    public Functions()
    {
        Expression e = new Expression(Console.ReadLine(), EvaluateOptions.IgnoreCase);
        e.Parameters["x"] = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine(e.Evaluate());
    }

    private void SampleFunction()
    {
        float i = -10;
        for (i = -10; i < 10; i += 0.0001f)
        {
            
        }
        
        
    }
}