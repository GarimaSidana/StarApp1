using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarApp1.Models;

namespace StarApp1.Models
{
    public class StarUserDbContext : IdentityDbContext
    {
        //private readonly IConfiguration _configuration;

        public StarUserDbContext(DbContextOptions<StarUserDbContext> options /*,IConfiguration Config*/) : base(options)
        {
            //_configuration = Config; 
        }
        public  void ExecuteResult(ActionContext context)
        {
            var Response = context.HttpContext.Response;
           
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {

            //        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("MYConnector"),
            //        sqlServerOptions => sqlServerOptions.CommandTimeout(
            //        Convert.ToInt32(_configuration.GetConnectionString("ConnectionTimeOut"))
            //        ));

            //    }
            //}
        public DbSet<StarApp1.Models.LoginViewModel> LoginViewModel { get; set; }
        public DbSet<StarApp1.Models.RegisterViewModel> RegisterViewModel { get; set; }
        public DbSet<StarApp1.Models.UserAdmin> UserAdmin { get; set; }
        
    }

}