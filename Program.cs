// See https://aka.ms/new-console-template for more information
using ChessGame;

try
{
    Console.WriteLine("Welcome to Chess Game!");
    Console.WriteLine("1. Play against AI");
    Console.WriteLine("2. Two-player mode");
    Console.Write("Select mode (1 or 2): ");

    // Default to two-player mode
    bool useAI = false;
    int aiDepth = 7; // Default depth
    
    // Try to read input, but have fallbacks for debugging environments
    try
    {
        // Use a timeout to prevent hanging in debugging environments
        var inputTask = Task.Run(() => Console.ReadLine());
        bool completed = inputTask.Wait(TimeSpan.FromSeconds(5));
        
        if (completed && inputTask.Result == "1")
        {
            useAI = true;
            
            Console.Write("Enter AI search depth (3-10, higher is stronger but slower): ");
            
            var depthTask = Task.Run(() => Console.ReadLine());
            bool depthCompleted = depthTask.Wait(TimeSpan.FromSeconds(5));
            
            if (depthCompleted && !string.IsNullOrEmpty(depthTask.Result) && 
                int.TryParse(depthTask.Result, out int parsedDepth))
            {
                aiDepth = Math.Max(3, Math.Min(10, parsedDepth)); // Clamp between 3 and 10
            }
            else
            {
                Console.WriteLine("Using default AI depth (7).");
            }
        }
        else if (!completed)
        {
            Console.WriteLine("Input timeout. Defaulting to two-player mode.");
        }
    }
    catch (Exception ex) when (ex is System.IO.IOException || ex is AggregateException)
    {
        Console.WriteLine($"Input error: {ex.Message}");
        Console.WriteLine("Defaulting to two-player mode.");
    }
    
    if (useAI)
    {
        Console.WriteLine($"Starting game with AI (search depth: {aiDepth})");
        Game game = new Game(useAI: true, aiSearchDepth: aiDepth);
        game.Play();
    }
    else
    {
        Console.WriteLine("Starting two-player mode");
        Game game = new Game(useAI: false);
        game.Play();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
    Console.WriteLine("Press any key to exit...");
    try
    {
        Console.ReadKey();
    }
    catch
    {
        // Ignore any errors when trying to read a key at exit
    }
}
