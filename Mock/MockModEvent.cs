using EPP.Models;

namespace EPP.Mock
{
    public partial class MockModEvent : ModEvent
    {
        public MockModEvent() : base()
        {
            var picture = new EventPicture("ANGRY_MOB_eventPicture");

            Id = "Event.1";
            Title = "Event title";
            Description = "Event Description";
            SelectedPicture = picture;
            Pictures = [picture, new EventPicture("ADVISOR_eventPicture")];
            Options = [
                new() { Name = "Option 1" },
                new() { Name = "Option 2" },
                new() { Name = "Option 3" },
            ];
        }
    }
}
