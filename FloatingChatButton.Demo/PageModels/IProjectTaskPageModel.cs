using CommunityToolkit.Mvvm.Input;
using FloatingChatButton.Demo.Models;

namespace FloatingChatButton.Demo.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}