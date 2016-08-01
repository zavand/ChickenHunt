using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving;
using log4net;
using log4net.Appender;
using log4net.Config;
using Module = Autofac.Module;

namespace ChickenHunt.Website.Common
{
    public class LoggingModule : Module
    {
        static LoggingModule()
        {
            XmlConfigurator.Configure();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register((c, p) =>
            {
                Exception exception = null;
                var loggerName = "Global";
                try
                {
                    var namedParam = p.OfType<NamedParameter>().FirstOrDefault(m => m.Name == "name");
                    if (namedParam != null)
                        loggerName = (string)namedParam.Value;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                var logger = GetLogger(loggerName, c);
                if (exception != null)
                {
                    logger?.Error("Error during logger creation.", exception);
                }
                return logger;
            }).As<ILog>();
        }

        private void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            e.Parameters = e.Parameters
                .Union(
                    new[]
                    {
                        new ResolvedParameter(
                            (p, i) => p.ParameterType == typeof (ILog),
                            (p, i) => GetLogger(p.Member.DeclaringType.Name, (e.Context as IInstanceLookup)?.ActivationScope)
                            )
                    })
                .ToArray();
        }

        private ILog GetLogger(string name, IComponentContext scope)
        {
            var logger = LogManager.GetLogger(name);
            
            return logger;
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            // Handle constructor parameters.
            registration.Preparing += OnComponentPreparing;

            // Handle properties.
            registration.Activated += InjectLoggerProperties;
        }

        private void InjectLoggerProperties(object sender, ActivatedEventArgs<object> e)
        {
            var instanceType = e.Instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToArray();

            // Set the properties located.
            foreach (var propToSet in properties)
            {
                var v = propToSet.GetValue(e.Instance);
                if (v == null)
                {
                    propToSet.SetValue(e.Instance, GetLogger(instanceType.Name, (e.Context as IInstanceLookup)?.ActivationScope), null);
                }
            }
        }
    }
}