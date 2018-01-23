using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Grace.DependencyInjection;
using RouteHelper.ViewModels;

namespace RouteHelper.Bootstrapping
{
  public class Bootstrapper : BootstrapperBase
  {
    private DependencyInjectionContainer kernel;

    public Bootstrapper()
    {
      Initialize();
    }

    protected override void Configure()
    {
      kernel = new DependencyInjectionContainer();
      kernel.Configure(x=>x.Export<WindowManager>().As<IWindowManager>().Lifestyle.Singleton());
      kernel.Configure(x=>x.Export<EventAggregator>().As<IEventAggregator>().Lifestyle.Singleton());
    }

    protected override object GetInstance(Type service, string key)
    {
      return kernel.Locate(service);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
      return kernel.LocateAll(service);
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
      DisplayRootViewFor<RouteHelperViewModel>();
    }
  }
}