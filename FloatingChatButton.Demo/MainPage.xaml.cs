using System.Windows.Input;

namespace FloatingChatButton.Demo
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public ICommand MessageSentCommand { get; }

        public MainPage()
        {
            try
            {
                MessageSentCommand = new Command<string>(OnMessageSent);

                InitializeComponent();

                BindingContext = this;

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

        private async void OnMessageSent(string message)
        {
            await Task.Delay(1000);

            myfloatingButton.Messages.Add(new()
            {
                Text = $"You said: {message}",
                IsIncoming = true
            });
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
