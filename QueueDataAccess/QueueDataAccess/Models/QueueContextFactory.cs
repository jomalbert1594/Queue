using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QueueDataAccess.Models
{
    public class QueueContextFactory: IDesignTimeDbContextFactory<QueueDbContext>
    {
        public QueueDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueueDbContext>();

            optionsBuilder.UseSqlServer(
                "Data Source=.\\SQLEXPRESS;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.1.132\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.1.100\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=192.168.2.132\\SQLEXPRESS,1433;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            //optionsBuilder.UseSqlServer(
            //    "Data Source=.\\SQLEXPRESS;initial catalog=QueueDb2;user id=Jomari;password=admin123");

            return new QueueDbContext(optionsBuilder.Options);
        }
    }
}
