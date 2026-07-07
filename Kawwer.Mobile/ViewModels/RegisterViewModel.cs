using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    public RegisterViewModel(AuthService auth)
    {
        _auth = auth;
        Title = "Create account";
    }

    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [RelayCommand]
    private Task RegisterAsync() => RunAsync(async () =>
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        await _auth.RegisterAsync(new
        {
            username = Username.Trim(),
            email = Email.Trim(),
            password = Password,
            firstName = NameFormat.Capitalize(FirstName),
            lastName = NameFormat.Capitalize(LastName),
            phoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim()
        });

        // A brand-new account has not been onboarded yet, so it always starts in the onboarding flow.
        await Shell.Current.GoToAsync(_auth.RequiresOnboarding ? "//onboarding" : "//main/hometab");
    });

    [RelayCommand]
    private Task BackToLoginAsync() => Shell.Current.GoToAsync("..");
}
