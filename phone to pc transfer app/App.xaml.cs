using Microsoft.Extensions.DependencyInjection;

namespace phone_to_pc_transfer_app
{
    public partial class App : Application
    {
        private readonly MainPage _mainPage;

        public App(MainPage mainPage)
        {
            InitializeComponent();
            _mainPage = mainPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_mainPage);
        }
    }
}