using CommunityToolkit.Mvvm.ComponentModel;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.ViewModels;

/// <summary>Wraps a user with a selection flag for multi-select invite lists.</summary>
public sealed partial class SelectableUser : ObservableObject
{
    public SelectableUser(UserSummaryDto user) => User = user;

    public UserSummaryDto User { get; }

    [ObservableProperty]
    private bool _isSelected;
}
