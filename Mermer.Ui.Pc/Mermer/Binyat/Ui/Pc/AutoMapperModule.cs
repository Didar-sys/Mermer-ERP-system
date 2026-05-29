using Autofac;
using AutoMapper;
using System.Collections.Generic;

namespace Mermer.Ui.Pc // Або інший простір імен, де ти його створиш
{
    public class AutoMapperModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Реєструємо всі профілі мапінгу з поточних збірок
            builder.Register(context =>
            {
                var profiles = context.Resolve<IEnumerable<Profile>>();
                var config = new MapperConfiguration(cfg =>
                {
                    foreach (var profile in profiles)
                    {
                        cfg.AddProfile(profile);
                    }
                });
                return config;
            }).SingleInstance().AutoActivate().AsSelf();

            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper(c.Resolve)).As<IMapper>().InstancePerLifetimeScope();
        }
    }
}