using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Web.Http;
using ApiService.EventHandlers;
using ApiService.EventHandlers.Hierarchy;
using ApiService.EventHandlers.Incidents;
using ApiService.EventHandlers.Personnel;
using ApiService.EventHandlers.Positions;
using ApiService.EventHandlers.Radios;
using ApiService.EventHandlers.Units;
using ApiService.Models;
using TriTech.VisiCAD;
using ILogger = Serilog.ILogger;

namespace ApiService
{
    public static class DependencyRegistration
    {
        internal static IContainer Register(HttpConfiguration config, IConfigurationRoot configuration, ILogger logger)
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var serviceSettings = new ServiceSettings();
            configuration.Bind(serviceSettings);
            builder.RegisterInstance(serviceSettings).SingleInstance();

            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();
            
            builder.Register(c =>
            {
                var settings = c.Resolve<ServiceSettings>();
                var cadManager = new CADManager();
                cadManager.LoginAsServiceAccount(settings.ServiceAccountName);
                return cadManager;
            }).SingleInstance();
            
            // Register services here
            builder.RegisterType<Validation>().InstancePerDependency();
            
            // Hierarchy Services
            builder.RegisterType<GetAgencyIdByName>().InstancePerDependency();
            
            // Incident Services
            builder.RegisterType<CreateComment>().InstancePerDependency();
            builder.RegisterType<CreateIncident>().InstancePerDependency();
            builder.RegisterType<CreateIncidentFieldsParam>().InstancePerDependency();
            builder.RegisterType<CreateIncidentHierarchyParams>().InstancePerDependency();
            builder.RegisterType<GetCallerTypesByAgency>().InstancePerDependency();
            builder.RegisterType<GetCallMethods>().InstancePerDependency();
            builder.RegisterType<GetIncident>().InstancePerDependency();
            builder.RegisterType<GetIncidents>().InstancePerDependency();
            builder.RegisterType<GetUserDefinedFields>().InstancePerDependency();
            builder.RegisterType<UpdateCallerType>().InstancePerDependency();
            builder.RegisterType<UpdateCallMethod>().InstancePerDependency();
            builder.RegisterType<UpdateCallTakingPerformedBy>().InstancePerDependency();
            builder.RegisterType<UpdateUserDefinedField>().InstancePerDependency();
            
            // Personnel Services
            builder.RegisterType<GetPerson>().InstancePerDependency();
            builder.RegisterType<GetPersonnel>().InstancePerDependency();
            
            // Position Services
            builder.RegisterType<GetPositionsFromDatabase>().InstancePerDependency();
            builder.RegisterType<UpdatePositions>().InstancePerDependency();
            builder.RegisterType<UpdatePositionsToDatabase>().InstancePerDependency();
            
            // Radio Services
            builder.RegisterType<CreatePersonnelRadioFields>().InstancePerDependency();
            builder.RegisterType<DeletePersonnelRadio>().InstancePerDependency();
            builder.RegisterType<GetPersonnelRadiosByRadioCode>().InstancePerDependency();
            builder.RegisterType<GetPersonnelRadiosIncludingTemporary>().InstancePerDependency();
            builder.RegisterType<SavePersonnelRadio>().InstancePerDependency();
            
            // Unit Services
            builder.RegisterType<GetUnit>().InstancePerDependency();
            builder.RegisterType<GetUnits>().InstancePerDependency();
            
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            
            // Start the CADManager to ensure it is ready for use
            container.Resolve<CADManager>();
            
            return container;
        }
    }
}