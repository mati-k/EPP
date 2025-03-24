using EPP.Models;
using EPP.ViewModels;

namespace EPP.Mock
{
    public partial class MockEditorViewModel : EditorViewModel
    {
        public MockEditorViewModel() : base()
        {
            EventPicture picture = new EventPicture("");

            EventFile = new();
            EventFile.Events.Add(new() { Title = "Event 1", SelectedPicture = picture });
            EventFile.Events.Add(new() { Title = "Event 2", SelectedPicture = picture });
            EventFile.Events.Add(new() { Title = "Event with longer name", SelectedPicture = picture });
            EventFile.Events.Add(new() { Title = "Event with very long and even longer name", SelectedPicture = picture });

            SelectedEvent = EventFile.Events[0];
        }
    }
}
