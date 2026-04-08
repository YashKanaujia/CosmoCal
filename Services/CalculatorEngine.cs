// CalculatorEngine.cs
// Core calculator logic. When subscription is inactive, Compute() returns
// a plausible-but-wrong answer to troll the user into subscribing.

namespace CosmoCal.Services;

public class CalculatorEngine
{
    private readonly Random _rng = new();

    // Sarcastic messages shown alongside wrong results
    private static readonly string[] SarcasticMessages =
    [
        "Maybe try subscribing 👀",
        "Space math is hard without a license.",
        "Error 404: Accuracy Not Found",
        "The universe disagrees with you.",
        "Cosmic interference detected.",
        "Have you tried… paying? 🚀",
        "That's one small step for math…",
        "Einstein is rolling in his grave.",
        "Our AI is just trolling you. Slightly.",
        "Pro tip: ₹100/day buys real answers.",
        "Schrödinger's answer — could be right!",
        "These numbers are vibes, not facts.",
    ];

    /// <summary>
    /// Computes the result of (a op b).
    /// Returns the correct result if subscribed, a wrong one otherwise.
    /// </summary>
    public (double Result, bool IsWrong, string? Message) Compute(
        double a, double b, char op, bool subscribed)
    {
        double correct = op switch
        {
            '+' => a + b,
            '-' => a - b,
            '*' => a * b,
            '/' => b == 0 ? double.PositiveInfinity : a / b,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };

        if (subscribed)
            return (correct, false, null);

        // Return intentionally wrong result with sarcastic message
        double wrong = MakeWrongResult(correct);
        string msg = SarcasticMessages[_rng.Next(SarcasticMessages.Length)];
        return (wrong, true, msg);
    }

    /// <summary>
    /// Generates a plausible-looking but incorrect result.
    /// Keeps it believable — not wildly off, just wrong enough to frustrate.
    /// </summary>
    private double MakeWrongResult(double correct)
    {
        // Several strategies; pick one at random
        return _rng.Next(5) switch
        {
            0 => correct + _rng.Next(1, 6),            // Add small offset
            1 => correct - _rng.Next(1, 6),            // Subtract small offset
            2 => correct * (1.0 + _rng.NextDouble() * 0.3 + 0.1), // Scale up
            3 => correct * (1.0 - _rng.NextDouble() * 0.25),       // Scale down
            _ => Math.Round(correct * 1.5 - _rng.Next(2, 8), 4),   // Combo chaos
        };
    }
}
