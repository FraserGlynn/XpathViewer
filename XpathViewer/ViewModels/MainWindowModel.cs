namespace XpathViewer.ViewModels
{
    internal class MainWindowModel : ViewModel
    {
        public MainViewModel MainView { get; }

        public MainWindowModel()
        {
            MainView = new MainViewModel();
        }

    }
}
