using NLog;

namespace NLog6;

internal static class Program
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    internal static void Main()
    {
        Console.WriteLine("Hello, World!");

        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterMinLevel(LogLevel.Trace).WriteToColoredConsole();
        });

        try
        {
            // Correct
            Log.Info("Correct:");
            Log.Info("My name is {name}",                                "Alex");                                                // My name is Alex
            Log.Info("I am {age} years old",                             30);                                                    // I am 30 years old
            Log.Info("I have {count} cards. All have the same ID: {id}", 3, Guid.Parse("be896de0-7802-47ad-97e2-e622181d4879")); // I have 3 cards. All have the same ID: be896de0-7802-47ad-97e2-e622181d4879
            Log.Info("My ID is {id}",                                    Guid.Parse("502f088f-1c1f-4f1e-8401-a2528211086a"));    // My ID is 502f088f-1c1f-4f1e-8401-a2528211086a

            Log.Info("");
            Log.Info("");

            // Wrong
            Log.Info("Wrong:");
            Log.Info("My name is {name:l}",    "Alex"); // My name is Alex
            Log.Info("I am {age:l} years old", 30);     // I am {age:l} years old

            try
            {
                Console.WriteLine("I have 3 cards:");

                // Fails when {count:l} is combined with a GUID
                // System.FormatException: Format specifier was invalid.
                Log.Info("I have {count:l} cards. All have the same ID: {id}", 3, Guid.Parse("be896de0-7802-47ad-97e2-e622181d4879"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            try
            {
                Console.WriteLine("My ID is:");

                // System.FormatException: Format string can be only "D", "d", "N", "n", "P", "p", "B", "b", "X" or "x".
                Log.Info("My ID is {id:l}", Guid.Parse("502f088f-1c1f-4f1e-8401-a2528211086a"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            throw new Exception("Master Exception");
        }
        catch (Exception e)
        {
            Console.WriteLine("Catch block:");

            // This is really dangerous. The real exception is now gone and hidden by the System.FormatException.
            // Unhandled exception. System.FormatException: Format string can be only "D", "d", "N", "n", "P", "p", "B", "b", "X" or "x".
            Log.Error(e, "This exception has the ID {id:l}", Guid.Parse("8816da7b-9aa1-427a-91f1-24e34e77bb8e"));
        }
    }
}