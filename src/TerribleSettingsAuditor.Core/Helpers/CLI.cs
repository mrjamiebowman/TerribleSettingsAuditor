namespace TerribleSettingsAuditor.Core.Helpers;

internal class CLI
{
    public static void WriteLineGreen(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    public static void WriteLinLineYellow(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    public static void WriteLineRed(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    public static void WriteLineError(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    public static void WriteGreen(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(message);
        Console.ForegroundColor = previousColor;
    }

    public static void WriteRed(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(message);
        Console.ForegroundColor = previousColor;
    }

    public static string GenerateRandomScreeningReportMessage()
    {
        string[] tsaLines = new[]
        {
            "Ma’am, your connection string is over 3.4 ounces. Please place it in a clear, quart-sized zip folder or it will be confiscated.",
            "Sir, this unencrypted password has been randomly selected for additional screening. Please step aside.",
            "Sir, we detected a minor syntax error. Please remove your shoes, belt, and light jacket before attempting to compile again.",
            "Ma’am, your configuration file is currently in a boarding group that does not exist. Please wait at the gate.",
            "Sir, your bag just made a noise we’ve only heard in spy movies. Mind stepping over here before it takes off on its own?",
            "Sir, we saw you sweating like you're smuggling fireworks on the Fourth of July. Quick chat over here?",
            "You’ve been randomly selected by the Wheel of Misfortune™. Please step aside. Yes, again.",
            "Ma’am, you’re too calm for a 6 a.m. Step over here. We need to know your secrets. And maybe your skincare routine.",
            "Sir, you’re wearing Gucci slides, a Rolex, and 17 gold chains… on Spirit Airlines. Just a quick check to make sure you’re not the plane's new owner.",
            "Sir, I get it — barefoot is freeing. But this is not a yoga retreat. Let’s talk about hygiene over here.",
            "Sir, your laptop has 47 open tabs, 3 mining scripts, and a crypto wallet. We're just gonna make sure it's not trying to fly itself.",
            "Ma’am, you whispered something about lizard people and 5G at Gate 12. We’re just gonna check your carry-on for tinfoil hats.",
            "Sir, you’ve been standing barefoot in security humming Jimmy Buffett songs for 30 minutes. Come with us before you start grilling.",
            "Sir, the entire terminal heard your group chat read-aloud. We just want to know how your cousin’s rash turned out.",
            "Ma’am, you’ve livestreamed every step of the TSA line. We just need a moment to sign the media release.",
            "Young man, we counted 42 questions about plane crashes in 6 minutes. Let's take a break from Google, okay?",
            "Sir, you loudly explained TSA procedure to 3 people and made up 12 rules. We’re just gonna ask you to stop helping.",
            "Ma’am, your purse contains a rotisserie chicken, 3 pickles, and a whole melon. We admire your commitment to in-flight dining. Let’s chat.",
            "Sir, you’re wearing snakeskin pants, sunglasses at night, and a cape. We’re not mad — we just want a selfie and a quick bag check.",
            "Sir, when we asked if you packed your own bag, you whispered ‘define packed.’ Please step this way."
        };

        Random rnd = new Random();
        int index = rnd.Next(tsaLines.Length);
        string selectedLine = tsaLines[index];

        return selectedLine;
    }

    public static class Icons
    {
        public const string Success = "✅";

        public const string Warning = "⚠️";

        public const string Failure = "❌";

        public const string Luggage = "🧳";

        public const string Pin = "📌";

        public const string Wrench = "🔧";

        public const string Camera = "📸";

        public const string AirplaneLiftOff = "🛫";
    }
}
