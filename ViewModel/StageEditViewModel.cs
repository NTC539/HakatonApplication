using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.Models;
namespace HakatonApplication.ViewModel
{


    public partial class StageEditViewModel : ObservableObject
    {
        private readonly Stage? _existingStage;
        [ObservableProperty] private string _description = "";
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private int? _orderNumber;
        [ObservableProperty] private int? _locationId;
        [ObservableProperty] private int? _stageTypeId;

        public Stage? ResultStage { get; private set; }
        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public StageEditViewModel(Stage? existingStage = null)
        {
            _existingStage = existingStage;
            if (existingStage != null)
            {
                Description = existingStage.Description ?? "";
                StartDate = existingStage.StartDate;
                EndDate = existingStage.EndDate;
                OrderNumber = existingStage.OrderNumber;
                LocationId = existingStage.LocationId;
                StageTypeId = existingStage.StageTypeId;
            }
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private void Ok()
        {
            ResultStage = _existingStage ?? new Stage();
            ResultStage.Description = Description;
            ResultStage.StartDate = StartDate;
            ResultStage.EndDate = EndDate;
            ResultStage.OrderNumber = OrderNumber;
            ResultStage.LocationId = LocationId;
            ResultStage.StageTypeId = StageTypeId;
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}