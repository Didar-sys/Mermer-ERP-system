using Autofac;
using AutoMapper;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Authentication.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.Data.Synchronizer.Core.Tools;
using System.Reflection;

namespace Mermer.Core.Couch;

public class BinyatCouchModule : BinyatCoreModule
{
    private readonly string _url;
    private readonly string _bucket;
    private readonly string _username;
    private readonly string _password;

    public BinyatCouchModule(string url, string bucket, string username, string password)
    {
        // Присвоюємо ті значення, які приходять у параметрах
        _url = url;
        _bucket = bucket;
        _username = username;
        _password = password;
    }

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        Assembly assembly = typeof(BinyatCouchModule).Assembly;

        builder.RegisterModule(new CouchModule(_url, _bucket, _username, _password));

        // Очищений синтаксис сканування Autofac
        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Name.EndsWith("Service") && t.Name != "CouchLoginService")
               .AsImplementedInterfaces();

        builder.RegisterType<CouchLoginService>().As<ILoginService>().SingleInstance();

        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Name.EndsWith("Mapper"))
               .As<Profile>();

        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Name.EndsWith("Repository"))
               .AsImplementedInterfaces();

        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Name.EndsWith("IndexCreator") || t.Name.EndsWith("ViewsCreator"))
               .As<IInitialSchemaCreator>().SingleInstance();

        builder.RegisterAssemblyTypes(assembly)
               .Where(t => t.Name.EndsWith("InitialDataCreator"))
               .As<IInitialDataCreator>().SingleInstance();

        builder.RegisterType<CouchDocumentChangeListener>().As<IDocumentChangeListener>().SingleInstance();
        builder.RegisterType<Mermer.Data.Patcher.Patcher>().As<IPatcher>();
        builder.RegisterType<ChangeHelper>().As<IChangeHelper>();

        // Просто створюємо об'єкти, а Visual Studio сама знайде їхній шлях
        var instance = new CouchCluster();
        instance.Initialize(_url, _bucket, _username, _password);
        builder.RegisterInstance(instance).As<ICouchCluster>().SingleInstance();
    }
}