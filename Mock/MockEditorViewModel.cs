using CommunityToolkit.Mvvm.DependencyInjection;
using EPP.Models;
using EPP.Services;
using EPP.ViewModels;
using System.Collections.ObjectModel;

namespace EPP.Mock
{
    public partial class MockEditorViewModel : EditorViewModel
    {
        public MockEditorViewModel() : base()
        {
            EventPicture picture = new("");

            EventFile = new();
            EventFile.Events.Add(new MockModEvent());
            EventFile.Events.Add(new() { Title = "Event 2", SelectedPicture = picture });
            EventFile.Events.Add(new() { Title = "Event with longer name", SelectedPicture = picture });
            EventFile.Events.Add(new() { Title = "Event with very long and even longer name", SelectedPicture = picture });

            SelectedEvent = EventFile.Events[0];

            var gfxService = Ioc.Default.GetService<IGfxService>();
            if (gfxService is not null)
            {
                Pictures = new ObservableCollection<string>(gfxService.GetPictureNames());
            }
        }
    }
}
