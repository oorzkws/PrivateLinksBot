namespace PrivateLinksBot;
// Note: actual namespace depends on the project name.

/// <summary>
/// Entry point
/// </summary>
public static class Program {
    // ReSharper disable once UnusedParameter.Global
    public static Task Main(string[] args) {
        try {
            return new PrivateLinksBot().RunAsync();
        }
        catch (Exception e) {
            Console.WriteLine($"Unhandled exception: {e}");
            Environment.Exit(e.HResult);
            throw;
        }
    }
}