namespace FloatingChatButton.Demo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            try
            {
                InitializeComponent();
                myfloatingButton.Messages = new System.Collections.ObjectModel.ObservableCollection<FloatingChatButton.Models.ChatMessage>
            {
                new FloatingChatButton.Models.ChatMessage { Text = "Hello, how can I help you?", IsIncoming = true },
                new FloatingChatButton.Models.ChatMessage { Text = "I have a question about your product.", IsIncoming = false },
                new FloatingChatButton.Models.ChatMessage { Text = "Sure! What would you like to know?", IsIncoming = true }
            };
            }
            catch (Exception ex)
            {

            }
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
