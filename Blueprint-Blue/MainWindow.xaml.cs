
namespace Blueprint_Blue
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var svc = new BlueprintSVC();
            svc.Run();
        }
    }
}
