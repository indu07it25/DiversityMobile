using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;

namespace DiversityPhone.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            await Task.Delay(1000);
            Assert.IsTrue(false);
        }
    }
}
