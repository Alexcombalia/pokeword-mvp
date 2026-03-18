using UnityEngine;

public static class BakerIntentParser
{
    public static bool WantsToBuyBread(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        string text = message.ToLower().Trim();

        bool mentionsBread =
            text.Contains("pain") ||
            text.Contains("baguette");

        bool buyingIntent =
            text.Contains("je veux") ||
            text.Contains("je voudrais") ||
            text.Contains("je prends") ||
            text.Contains("donnez-moi") ||
            text.Contains("un ") ||
            text.Contains("une ");

        return mentionsBread && buyingIntent;
    }
}