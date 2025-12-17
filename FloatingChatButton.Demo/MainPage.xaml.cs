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
                myfloatingButton.Messages =
                [
                    new() { Text = "Hello, how can I help you?", IsIncoming = true },
                    new() { Text = "I have a question about your product.", IsIncoming = false },
                    new() { Text = "Sure! What would you like to know?", IsIncoming = true }
                ];
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
