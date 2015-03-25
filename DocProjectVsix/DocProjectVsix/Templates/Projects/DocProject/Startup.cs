using Microsoft.AspNet.Builder;

namespace ConsoleApp1
{
    //public class Program
    //{
    //    public void Main(string[] args)
    //    {
    //        Console.WriteLine("Hello World");
    //        Console.ReadLine();
    //    }
    //}
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseWelcomePage();
        }
    }
}
