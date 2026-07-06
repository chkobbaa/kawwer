using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;
using Microsoft.Maui.Storage;

namespace Kawwer.Mobile.ViewModels;

/// <summary>
/// Drives the full-screen, first-run onboarding flow: date of birth, preferred position and
/// preferred foot, followed by a "find friends / skip" screen. Answers are drafted to
/// <see cref="Preferences"/> so the flow resumes with the same values if the app is killed midway,
/// and are only marked complete once the backend has saved them.
/// </summary>
public sealed partial class OnboardingViewModel : BaseViewModel
{
    // Draft keys let the flow resume with the same answers if the app is closed mid-onboarding.
    private const string DraftStepKey = "onboarding_draft_step";
    private const string DraftBirthDateKey = "onboarding_draft_birthdate";
    private const string DraftPositionKey = "onboarding_draft_position";
    private const string DraftFootKey = "onboarding_draft_foot";

    private const int FirstStep = 1;
    private const int LastDataStep = 3;
    private const int DoneStep = 4;

    private readonly KawwerApiClient _api;
    private readonly AuthService _auth;

    private bool _hasDraftBirthDate;

    public OnboardingViewModel(KawwerApiClient api, AuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = "Welcome";

        // A plausible default so the picker never opens on "today"; bounds mirror the API validator.
        MaximumBirthDate = DateTime.Today.AddYears(-10);
        MinimumBirthDate = DateTime.Today.AddYears(-100);
        _birthDate = DateTime.Today.AddYears(-20);

        // Resume any draft left behind by a killed session. Done here (in the constructor) so the
        // backing fields can be seeded directly, without firing change notifications or re-saving
        // the draft mid-restore.
        try
        {
            var step = Preferences.Default.Get(DraftStepKey, FirstStep);
            _currentStep = step is >= FirstStep and <= LastDataStep ? step : FirstStep;

            var dobRaw = Preferences.Default.Get(DraftBirthDateKey, string.Empty);
            if (!string.IsNullOrEmpty(dobRaw)
                && DateTime.TryParse(dobRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dob))
            {
                _birthDate = dob;
                _hasDraftBirthDate = true;
            }

            var position = Preferences.Default.Get(DraftPositionKey, 0);
            if (position != 0 && Enum.IsDefined(typeof(PreferredPosition), position))
            {
                _selectedPosition = (PreferredPosition)position;
            }

            var foot = Preferences.Default.Get(DraftFootKey, 0);
            if (foot != 0 && Enum.IsDefined(typeof(PreferredFoot), foot))
            {
                _selectedFoot = (PreferredFoot)foot;
            }
        }
        catch
        {
            // A corrupt draft should never block onboarding; fall back to defaults.
        }
    }

    /// <summary>Raised whenever the visible step changes, so the view can animate the transition.</summary>
    public event EventHandler<int>? StepChanged;

    // ----- Step machine -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStep1))]
    [NotifyPropertyChangedFor(nameof(IsStep2))]
    [NotifyPropertyChangedFor(nameof(IsStep3))]
    [NotifyPropertyChangedFor(nameof(IsDoneStep))]
    [NotifyPropertyChangedFor(nameof(IsDataStep))]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    [NotifyPropertyChangedFor(nameof(Progress))]
    [NotifyPropertyChangedFor(nameof(StepTitle))]
    [NotifyPropertyChangedFor(nameof(StepSubtitle))]
    [NotifyPropertyChangedFor(nameof(PrimaryLabel))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private int _currentStep = FirstStep;

    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;
    public bool IsDoneStep => CurrentStep == DoneStep;
    public bool IsDataStep => CurrentStep is >= FirstStep and <= LastDataStep;
    public bool CanGoBack => CurrentStep is > FirstStep and <= LastDataStep;
    public double Progress => IsDataStep ? (double)CurrentStep / LastDataStep : 1d;

    public string StepTitle => CurrentStep switch
    {
        1 => "When's your birthday?",
        2 => "Where do you play?",
        3 => "Your stronger foot?",
        _ => $"You're all set{NameSuffix}"
    };

    public string StepSubtitle => CurrentStep switch
    {
        1 => "Your age helps us match you with the right games.",
        2 => "Pick the position you play most often.",
        3 => "This helps teammates set up the play.",
        _ => "Your profile is ready. Want to bring your crew along?"
    };

    public string PrimaryLabel => CurrentStep == LastDataStep ? "Finish" : "Continue";

    private string NameSuffix
    {
        get
        {
            var name = _auth.Session.CurrentUser?.DisplayFirstName;
            return string.IsNullOrWhiteSpace(name) ? "!" : $", {name}!";
        }
    }

    // ----- Step 1: date of birth -----
    [ObservableProperty] private DateTime _birthDate;

    public DateTime MinimumBirthDate { get; }
    public DateTime MaximumBirthDate { get; }

    partial void OnBirthDateChanged(DateTime value) => SaveDraft();

    // ----- Step 2: preferred position -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPositionValue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private PreferredPosition? _selectedPosition;

    // Bound by the view's DataTriggers to highlight the chosen card (0 = nothing selected yet).
    public int SelectedPositionValue => (int)(SelectedPosition ?? 0);

    // ----- Step 3: preferred foot -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedFootValue))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private PreferredFoot? _selectedFoot;

    public int SelectedFootValue => (int)(SelectedFoot ?? 0);

    [RelayCommand]
    private void SelectPosition(string value)
    {
        if (Enum.TryParse<PreferredPosition>(value, out var position))
        {
            SelectedPosition = position;
            SaveDraft();
        }
    }

    [RelayCommand]
    private void SelectFoot(string value)
    {
        if (Enum.TryParse<PreferredFoot>(value, out var foot))
        {
            SelectedFoot = foot;
            SaveDraft();
        }
    }

    private bool CanContinue() => CurrentStep switch
    {
        2 => SelectedPosition is not null,
        3 => SelectedFoot is not null,
        _ => true
    };

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private Task ContinueAsync() => RunAsync(async () =>
    {
        if (CurrentStep < LastDataStep)
        {
            GoToStep(CurrentStep + 1);
            return;
        }

        // Final data step: persist to the backend, which stamps the "onboarding completed" marker,
        // then reveal the "find friends" screen.
        var updated = await _api.CompleteOnboardingAsync(new
        {
            birthDate = DateOnly.FromDateTime(BirthDate),
            preferredPosition = SelectedPosition,
            preferredFoot = SelectedFoot
        });

        _auth.Session.CurrentUser = updated; // flips the persisted onboarding flag to true
        ClearDraft();
        GoToStep(DoneStep);
    });

    [RelayCommand]
    private void Back()
    {
        if (CanGoBack)
        {
            GoToStep(CurrentStep - 1);
        }
    }

    [RelayCommand]
    private Task FindFriendsAsync() => Shell.Current.GoToAsync("//main/friendstab");

    [RelayCommand]
    private Task SkipAsync() => Shell.Current.GoToAsync("//main/hometab");

    /// <summary>
    /// Runs when the page appears. Guards against a stale "needs onboarding" flag by asking the
    /// server: if onboarding is already done (e.g. completed on another device) we skip straight
    /// into the app; otherwise we pre-fill anything already known so the flow feels continuous.
    /// </summary>
    [RelayCommand]
    public async Task AppearingAsync()
    {
        try
        {
            var me = await _api.GetMeAsync();
            _auth.Session.CurrentUser = me;

            if (me.OnboardingCompleted)
            {
                ClearDraft();
                await Shell.Current.GoToAsync("//main/hometab");
                return;
            }

            if (SelectedPosition is null && me.PreferredPosition is { } position)
            {
                SelectedPosition = position;
            }

            if (SelectedFoot is null && me.PreferredFoot is { } foot)
            {
                SelectedFoot = foot;
            }

            if (!_hasDraftBirthDate && me.BirthDate is { } dob)
            {
                BirthDate = dob.ToDateTime(TimeOnly.MinValue);
            }
        }
        catch
        {
            // Offline or a transient failure: continue with the local draft/defaults so the user is
            // never blocked from onboarding.
        }
    }

    private void GoToStep(int step)
    {
        CurrentStep = step;
        SaveDraft();
        StepChanged?.Invoke(this, step);
    }

    // ----- Draft persistence (resume-if-killed) -----
    private void SaveDraft()
    {
        try
        {
            Preferences.Default.Set(DraftStepKey, CurrentStep);
            Preferences.Default.Set(DraftBirthDateKey, BirthDate.ToString("O", CultureInfo.InvariantCulture));
            Preferences.Default.Set(DraftPositionKey, (int)(SelectedPosition ?? 0));
            Preferences.Default.Set(DraftFootKey, (int)(SelectedFoot ?? 0));
        }
        catch
        {
            // Best effort: losing a draft only means the flow restarts from step one.
        }
    }

    private static void ClearDraft()
    {
        try
        {
            Preferences.Default.Remove(DraftStepKey);
            Preferences.Default.Remove(DraftBirthDateKey);
            Preferences.Default.Remove(DraftPositionKey);
            Preferences.Default.Remove(DraftFootKey);
        }
        catch
        {
            // Ignored.
        }
    }
}
