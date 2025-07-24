using FloatingChatButton.Demo.Models;
using FloatingChatButton.Demo.PageModels;

namespace FloatingChatButton.Demo.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}