﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using Suteki.Common.Binders;
using Suteki.Common.Models;
using Suteki.Common.Windsor;
using Suteki.Shop.IoC;
using Suteki.Shop.Routes;

namespace Suteki.Shop
{
    public class GlobalApplication : HttpApplication, IContainerAccessor
    {
        private static IWindsorContainer container;

        public IWindsorContainer Container
        {
            get { return container; }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            RouteManager.RegisterRoutes(RouteTable.Routes);
            InitializeWindsor();
            InitializeBinders();
        }

        private static void InitializeBinders()
        {
            ModelBinders.Binders.DefaultBinder = container.Resolve<IModelBinder>();
            ModelBinders.Binders.Add(typeof(Money), new MoneyBinder());
        }

        protected void Application_End(object sender, EventArgs e)
        {
            container.Dispose();
        }

        /// <summary>
        /// This web application uses the Castle Project's IoC container, Windsor see:
        /// http://www.castleproject.org/container/index.html
        /// </summary>
        protected virtual void InitializeWindsor()
        {
            if (container == null)
            {
                // create a new Windsor Container
				container = ContainerBuilder.Build("Configuration\\Windsor.config"); 

                // set up the static IoC locator (this is an anti-pattern, only use in dire need!)
                IocContainer.SetResolveFunction(container.Resolve);
                IocContainer.SetReleaseAction(container.Release);

                // set the controller factory to the Windsor controller factory (in MVC Contrib)
                ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
            }
        }

        /* /// <summary>
        /// We want to replace the IPriciple generated by the ASP.NET runtime with our
        /// own User instance. This allows us to control access by role anwhere in 
        /// our application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_OnAuthenticateRequest(Object sender, EventArgs e)
        {
            if (Context.User != null)
            {
                if (Context.User.Identity.IsAuthenticated)
                {
                    string email = Context.User.Identity.Name;

                    IRepository<User> userRepository = container.Resolve<IRepository<User>>();
                    User user = userRepository.GetAll().WhereEmailIs(email);

                    if (user == null)
                    {
                        FormsAuthentication.SignOut();
                    }
                    else
                    {
                        System.Threading.Thread.CurrentPrincipal = Context.User = user;
                        return;
                    }
                }
            }

            System.Threading.Thread.CurrentPrincipal =
                Context.User = Suteki.Shop.User.Guest;
        }*/
    }
}