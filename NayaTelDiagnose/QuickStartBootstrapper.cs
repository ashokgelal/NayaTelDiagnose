
using Prism.Mef;
using Prism.Modularity;
using ViewSwitchingNavigation.Infrastructure;
using System.ComponentModel.Composition.Hosting;
using System.Threading.Tasks;
using System.Windows;
 
namespace NayaTelDiagnose
{
     public class QuickStartBootstrapper : MefBootstrapper
    {
        private const string ModuleCatalogUri = "/ViewSwitchingNavigation;component/ModulesCatalog.xaml";

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(QuickStartBootstrapper).Assembly));
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new ConfigurationModuleCatalog();
        }
        
        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }
        
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
           
        }
    }

}
