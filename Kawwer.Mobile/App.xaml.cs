using Kawwer.Mobile.Services;

namespace kawwer;

public partial class App : Application
{
    private readonly AppShell _shell;

    // Held so the eager singleton stays alive for the app's lifetime: it subscribes to the
    // real-time stream at startup and turns important updates into a simulated call in Call mode.
    private readonly CallSimulationService _callSimulation;

    public App(AppShell shell, CallSimulationService callSimulation)
    {
        InitializeComponent();
        _shell = shell;
        _callSimulation = callSimulation;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(_shell);
}
