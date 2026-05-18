using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.Models;
using HakatonApplication.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{
    public partial class StageEditViewModel : ObservableObject
    {
        private readonly IHakatonService? _service;
        private readonly Stage? _existingStage;

        [ObservableProperty] private string _description = "";
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private int? _orderNumber;

        [ObservableProperty] private ObservableCollection<Location> _availableLocations = new();
        [ObservableProperty] private Location? _selectedLocation;
        [ObservableProperty] private string _newLocationName = "";
        [ObservableProperty] private string _newLocationAddress = "";
        [ObservableProperty] private bool _isAddingNewLocation;

        [ObservableProperty] private ObservableCollection<StageType> _availableStageTypes = new();
        [ObservableProperty] private StageType? _selectedStageType;
        [ObservableProperty] private string _newStageTypeName = "";

        public Stage? ResultStage { get; private set; }
        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public StageEditViewModel(IHakatonService? service = null, Stage? existingStage = null)
        {
            _service = service;
            _existingStage = existingStage;

            if (existingStage != null)
            {
                Description = existingStage.Description ?? "";
                StartDate = existingStage.StartDate;
                EndDate = existingStage.EndDate;
                OrderNumber = existingStage.OrderNumber;
            }

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (_service != null)
            {
                var locations = await _service.GetAllLocationsAsync();
                AvailableLocations = new ObservableCollection<Location>(locations);
                var stageTypes = await _service.GetAllStageTypesAsync();
                AvailableStageTypes = new ObservableCollection<StageType>(stageTypes);
            }

            if (_existingStage != null)
            {
                if (_existingStage.LocationId.HasValue)
                {
                    SelectedLocation = AvailableLocations.FirstOrDefault(l => l.Id == _existingStage.LocationId.Value);
                    if (SelectedLocation != null) NewLocationName = SelectedLocation.Name;
                }
                if (_existingStage.StageTypeId.HasValue)
                {
                    SelectedStageType = AvailableStageTypes.FirstOrDefault(st => st.Id == _existingStage.StageTypeId.Value);
                    if (SelectedStageType != null) NewStageTypeName = SelectedStageType.StageType1;
                }
            }
        }

        partial void OnNewLocationNameChanged(string value)
        {
            if (!string.IsNullOrEmpty(value) && (SelectedLocation == null || SelectedLocation.Name != value))
                IsAddingNewLocation = true;
            else
            {
                IsAddingNewLocation = false;
                NewLocationAddress = "";
            }
        }

        private async Task<int?> GetOrCreateLocationId()
        {
            if (SelectedLocation != null && (string.IsNullOrEmpty(NewLocationName) || SelectedLocation.Name == NewLocationName))
                return SelectedLocation.Id;
            if (!string.IsNullOrEmpty(NewLocationName))
            {
                var newLocation = new Location { Name = NewLocationName, Address = NewLocationAddress };
                if (_service != null) await _service.AddLocationAsync(newLocation);
                return newLocation.Id;
            }
            return null;
        }

        private async Task<int?> GetOrCreateStageTypeId()
        {
            if (SelectedStageType != null && (string.IsNullOrEmpty(NewStageTypeName) || SelectedStageType.StageType1 == NewStageTypeName))
                return SelectedStageType.Id;
            if (!string.IsNullOrEmpty(NewStageTypeName))
            {
                var newStageType = new StageType { StageType1 = NewStageTypeName };
                if (_service != null) await _service.AddStageTypeAsync(newStageType);
                return newStageType.Id;
            }
            return null;
        }

        private async void Ok()
        {
            var locationId = await GetOrCreateLocationId();
            if (locationId == null && !string.IsNullOrEmpty(NewLocationName))
            {
                MessageBox.Show("Не удалось создать или выбрать место проведения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var stageTypeId = await GetOrCreateStageTypeId();
            if (stageTypeId == null && !string.IsNullOrEmpty(NewStageTypeName))
            {
                MessageBox.Show("Не удалось создать или выбрать тип этапа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultStage = _existingStage ?? new Stage();
            ResultStage.Description = Description;
            ResultStage.StartDate = StartDate;
            ResultStage.EndDate = EndDate;
            ResultStage.OrderNumber = OrderNumber;
            ResultStage.LocationId = locationId;
            ResultStage.StageTypeId = stageTypeId;

            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}