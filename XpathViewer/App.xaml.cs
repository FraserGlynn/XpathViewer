using System;
using System.IO;
using System.Reflection;
using System.Windows;
using XpathViewer.ViewModels;

namespace XpathViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private readonly MainWindowModel viewModel;

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            viewModel = new MainWindowModel();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.GetName().Name == assemblyName.Name)
                {
                    return assembly;
                }
            }

            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", assemblyName.Name + ".dll");
            if (File.Exists(resourcesPath))
            {
                return Assembly.LoadFrom(resourcesPath);
            }

            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow = new MainWindow()
            {
                DataContext = viewModel
            };
            MainWindow.Show();
            base.OnStartup(e);
        }


    }
}
