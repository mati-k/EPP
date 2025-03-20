using EPP.ViewModels;

namespace EPP.Mock
{
    public partial class MockConfigurationViewModel : ConfigurationViewModel
    {
        public MockConfigurationViewModel()
        {
            EventPath = "C:\\Users\\User\\Documents\\EventPath";
            LocalizationPath = "C:\\Users\\User\\Documents\\LocalizationPath";
            SourceDirectories.Add("C:\\Users\\User\\Documents\\SourceDirectories");
            SourceDirectories.Add("C:\\Users\\User\\Documents\\SourceDirectories2");
        }
    }
}
