// SubscriptionService.cs
// Manages mock subscription state, activation, expiry, and 24-hour countdown.
// In a real app this would talk to a payment gateway (Razorpay, Stripe, etc.)

namespace CosmoCal.Services;

public class SubscriptionService
{
    // --- State ---
    public bool IsActive => _expiryTime.HasValue && DateTime.UtcNow < _expiryTime.Value;
    public TimeSpan TimeRemaining => IsActive ? _expiryTime!.Value - DateTime.UtcNow : TimeSpan.Zero;

    private DateTime? _expiryTime;

    // Raised whenever subscription status changes (activates or expires)
    public event Action? OnStateChanged;

    // Subscription cost in Rupees (display only — no real payment)
    public const decimal PricePerDay = 100m;

    // Duration of one subscription period
    private static readonly TimeSpan SubscriptionDuration = TimeSpan.FromHours(24);

    // --- Mock Purchase ---
    /// <summary>
    /// Simulates purchasing a subscription.
    /// Call this when the user presses "Buy Subscription".
    /// </summary>
    public Task<bool> PurchaseAsync()
    {
        // Simulate a brief network/payment processing delay
        return Task.Run(async () =>
        {
            await Task.Delay(1500); // 1.5s mock payment processing

            // Mock: always succeeds. Replace with real gateway call here.
            _expiryTime = DateTime.UtcNow.Add(SubscriptionDuration);

            // Notify components that state has changed
            OnStateChanged?.Invoke();
            return true;
        });
    }

    /// <summary>
    /// Should be called periodically (e.g. every second from the UI timer)
    /// to check if the subscription has lapsed.
    /// </summary>
    public void Tick()
    {
        if (_expiryTime.HasValue && !IsActive)
        {
            // Just expired — clear and notify
            _expiryTime = null;
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Formatted time remaining string for display: HH:MM:SS
    /// </summary>
    public string FormattedTimeRemaining()
    {
        var t = TimeRemaining;
        return $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
    }
}
