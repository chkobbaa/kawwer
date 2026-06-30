using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
        Title = "Sign in";
    }

    [ObservableProperty]
    private string _usernameOrEmail = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [RelayCommand]
    private Task LoginAsync() => RunAsync(async () =>
    {
        if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Enter your username/email and password.";
            return;
        }

        await _auth.LoginAsync(UsernameOrEmail.Trim(), Password);
        await Shell.Current.GoToAsync("//main");
    });

    [RelayCommand]
    private Task GoToRegisterAsync() => Shell.Current.GoToAsync("register");
}
