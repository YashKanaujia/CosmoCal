namespace CosmoCal.Services
{
    /// <summary>
    /// Bridges the custom Blazor title bar to native WinForms window controls.
    /// The Form1 constructor subscribes to these events and performs the actual window operation.
    /// </summary>
    public class WindowControlService
    {
        public event Action? CloseRequested;
        public event Action? MinimizeRequested;
        public event Action? MaximizeRestoreRequested;

        public void RequestClose()           => CloseRequested?.Invoke();
        public void RequestMinimize()        => MinimizeRequested?.Invoke();
        public void RequestMaximizeRestore() => MaximizeRestoreRequested?.Invoke();
    }
}
