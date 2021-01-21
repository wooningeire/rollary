using Xamarin.Forms;

namespace rollary
{
    public partial class App : Application
    {
        private readonly ICaptureControlService captureControl = DependencyService.Get<ICaptureControlService>();
        private readonly ResultStateManager resultStateManager = new ResultStateManager();

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new CameraPage(captureControl, resultStateManager));
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
            //System.Diagnostics.Debug.WriteLine("Now I sleep");

            resultStateManager.Pause();

            captureControl.TerminateMediaCapture();
        }

        protected override void OnResume()
        {
            //System.Diagnostics.Debug.WriteLine("I resumed!!!!!");

            var navigationStack = MainPage.Navigation.NavigationStack;
            var lastPage = navigationStack[navigationStack.Count - 1];

            if (!(lastPage is CameraPage cameraPage)) return;

            cameraPage.LoadMediaCapture();
        }
    }
}
