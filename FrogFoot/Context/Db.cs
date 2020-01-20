using System.Web;

namespace FrogFoot.Context
{
    public static class Db
    {
        public static ApplicationDbContext GetInstance()
        {
            if (HttpContext.Current != null)
            {
                if (!HttpContext.Current.Items.Contains("_db_context"))
                {
                    HttpContext.Current.Items.Add("_db_context", new ApplicationDbContext());
                }
                return HttpContext.Current.Items["_db_context"] as ApplicationDbContext;
            }
            return new ApplicationDbContext();
        }

        public static ApplicationDbContext GetReadOnlyInstance()
        {
            if (!HttpContext.Current.Items.Contains("_db_context_read_only"))
            {
                ApplicationDbContext entities = new ApplicationDbContext();
                entities.Configuration.AutoDetectChangesEnabled = false;
                HttpContext.Current.Items.Add("_db_context_read_only", entities);
            }

            return HttpContext.Current.Items["_db_context_read_only"] as ApplicationDbContext;
        }
    }
}