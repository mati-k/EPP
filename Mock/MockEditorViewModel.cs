using EPP.ViewModels;

namespace EPP.Mock
{
    public partial class MockEditorViewModel : EditorViewModel
    {
        public MockEditorViewModel() : base()
        {
            EventFile = new();
            EventFile.Events.Add(new() { Title = "Event 1" });
            EventFile.Events.Add(new() { Title = "Event 2" });
            EventFile.Events.Add(new() { Title = "Event with longer name" });
            EventFile.Events.Add(new() { Title = "Event with very long and even longer name" });

            SelectedEvent = EventFile.Events[0];
        }
    }
}
