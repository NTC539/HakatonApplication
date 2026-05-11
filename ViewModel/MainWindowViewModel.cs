using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.DTO;
using HakatonApplication.Models;
using HakatonApplication.Service;
using System.Collections.ObjectModel;

namespace HakatonApplication.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IHakatonService _hakatonService;

    public MainWindowViewModel(IHakatonService hakatonService)
    {
        _hakatonService = hakatonService;
    }

    [ObservableProperty]
    private ObservableCollection<HakatonListItemDto> _hakatons = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadHakatonsAsync() => await LoadDataAsync(null);

    [RelayCommand]
    private async Task SearchAsync() => await LoadDataAsync(SearchText);

    private async Task LoadDataAsync(string? searchText)
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var items = await _hakatonService.GetAllHakatonsAsync(searchText);
            Hakatons = new ObservableCollection<HakatonListItemDto>(items);
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task OpenHakatonAsync(int id)
    {
        //// Открытие окна деталей
        //var detailViewModel = new HakatonDetailViewModel(id, ...);
        //var detailWindow = new HakatonDetailView { DataContext = detailViewModel };
        //detailWindow.ShowDialog();
        //// Опционально: обновить список
        //await LoadHakatonsAsync();
    }
}