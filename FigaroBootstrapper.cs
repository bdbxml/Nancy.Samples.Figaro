﻿using System;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.TinyIoc;

namespace Nancy.Demos.Figaro
{
    public class FigaroBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            
        }

        public override void Configure(INancyEnvironment environment)
        {
            environment.Diagnostics(true,"pass");
            environment.StaticContent("/Content");
            environment.Tracing(true, true);
            base.Configure(environment);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(new FigaroDataContext(RootPathProvider.GetRootPath()));
        }

        
        //protected override DiagnosticsConfiguration DiagnosticsConfiguration => 
        //    new DiagnosticsConfiguration() { Password = "pass"};

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(
                StaticContentConventionBuilder.AddDirectory("content", "Content", "*.*"));
        }
    }
}
