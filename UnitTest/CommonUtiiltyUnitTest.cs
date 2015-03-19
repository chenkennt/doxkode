using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocAsCode.BuildMeta;
using System.IO;
using System.Threading.Tasks;
using DocAsCode.Utility;
using DocAsCode.EntityModel;
using System.Collections.Generic;

namespace UnitTest
{
    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    public class UtilityUnitTest
    {
        [TestMethod]
        public async Task TestTripleSlashParser()
        {
            string input = @"
      <member name='T:TestClass1.Partial1'>

          <summary>
          Parital classes <see cref='T:System.AccessViolationException'/><see cref='T:System.AccessViolationException'/>can not cross assemblies, ```Classes in assemblies are by definition complete.```

          </summary>
          <returns>Task<see cref='T:System.AccessViolationException'/> returns</returns>

              <param name='input'>This is <see cref='T:System.AccessViolationException'/>the input</param>

              <param name = 'output' > This is the output </param >
     
           </member>";
            var summary = TripleSlashCommentParser.GetSummary(input, true);
            Assert.AreEqual("Parital classes @T:System.AccessViolationException-@T:System.AccessViolationException-can not cross assemblies, ```Classes in assemblies are by definition complete.```\r\n", summary);

            var returns = TripleSlashCommentParser.GetReturns(input, true);
            Assert.AreEqual("Task@T:System.AccessViolationException- returns", returns);

            var paramInput = TripleSlashCommentParser.GetParam(input, "input", true);
            Assert.AreEqual("This is @T:System.AccessViolationException-the input", paramInput);

            var invalidParam = TripleSlashCommentParser.GetParam(input, "invalid", true);
            Assert.IsNull(invalidParam);
        }

        [TestMethod]
        public void TestLinkParser()
        {
            Dictionary<string, string> index = new Dictionary<string, string>
            {
                {"link", "href" },
            };
            string input = "a@T:link-@invalid";
            string output = LinkParser.ResolveText(index, input, s => s);
            Assert.AreEqual("ahref@invalid", output);
            input = "a@link @T:link-";
            output = LinkParser.ResolveText(index, input, s => "[link](" + s + ")");
            Assert.AreEqual("a[link](href)  [link](href)", output);
        }

        [TestMethod]
        public void TestGitUtility()
        {
            var output = GitUtility.GetGitDetail(Environment.CurrentDirectory);
            Assert.AreEqual("https://capservice.visualstudio.com/DefaultCollection/CAPS/_git/DocAsCode", output.RemoteRepositoryUrl);
        }
    }
}
