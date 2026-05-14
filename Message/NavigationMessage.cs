namespace HakatonApplication.Message
{
    public class NavigationMessage
    {
        public Type ViewModelType { get; }
        public int? Id { get; }

        public NavigationMessage(Type viewModelType, int? id = null)
        {
            ViewModelType = viewModelType;
            Id = id;
        }
    }
}