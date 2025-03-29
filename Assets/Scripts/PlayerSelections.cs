using UnityEngine;

// Static class to store player selections between scenes
public static class PlayerSelections
{
    public static string Player1Character { get; set; } = "Hakurei Reimu";  // Default value
    public static string Player2Character { get; set; } = "Kirisame Marisa"; // Default value
    
    // Adding player names
    public static string Player1Name { get; set; } = "Player 1";  // Default value
    public static string Player2Name { get; set; } = "Player 2"; // Default value
} 