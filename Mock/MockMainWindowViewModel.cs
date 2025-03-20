using EPP.ViewModels;

namespace EPP.Mock
{
    public partial class MockMainWindowViewModel : MainWindowViewModel
    {
        public MockMainWindowViewModel()
        {
            CurrentPage = new MockConfigurationViewModel();
        }
    }
}
