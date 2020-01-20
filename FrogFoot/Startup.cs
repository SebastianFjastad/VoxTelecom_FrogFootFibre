using System.Configuration;
using FrogFoot.Utilities;
using Hangfire;
using Microsoft.Owin;
using Owin;
using WebGrease.Css.Ast;

[assembly: OwinStartupAttribute(typeof(FrogFoot.Startup))]
namespace FrogFoot
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("FrogFootDb");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ConfigureAuth(app);

            string emailRemonderCronTime = ConfigurationManager.AppSettings["EmailReminderCronTime"];
            RecurringJob.AddOrUpdate(() => EmailSender.SendReminderToISPs(), emailRemonderCronTime);

            string zonesChangedCheckTime = ConfigurationManager.AppSettings["ZonesChangedCheckTime"];
            RecurringJob.AddOrUpdate(() => ZoneSync.CheckLastModDate(), zonesChangedCheckTime);
        }
    }
}
