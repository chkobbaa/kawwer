using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class GroupsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public GroupsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Groups";
    }

    public ObservableCollection<GroupDto> Groups { get; } = new();

    [ObservableProperty] private string _newGroupName = string.Empty;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var groups = await _api.GetGroupsAsync();
        Groups.Clear();
        foreach (var g in groups)
        {
            Groups.Add(g);
        }
    });

    [RelayCommand]
    private Task CreateAsync() => RunAsync(async () =>
    {
        if (NewGroupName.Trim().Length < 2)
        {
            ErrorMessage = "Group name must be at least 2 characters.";
            return;
        }

        await _api.CreateGroupAsync(NewGroupName.Trim(), null);
        NewGroupName = string.Empty;
        await LoadAsync();
    });

    [RelayCommand]
    private Task DeleteAsync(Guid id) => RunAsync(async () =>
    {
        await _api.DeleteGroupAsync(id);
        await LoadAsync();
    });
}
