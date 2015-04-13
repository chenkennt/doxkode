using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace DocumentationWebsiteTmpl
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}
