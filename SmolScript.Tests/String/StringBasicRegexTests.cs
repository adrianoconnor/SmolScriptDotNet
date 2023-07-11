namespace SmolScript.Tests.String
{
    [TestClass]
    public class StringBasicRegexTests
    {
        [TestMethod]
        public void StringMatch()
        {
            var src = @"
            var r = new RegExp(""a+bc""); 
            var a = 'Test abc String';
            var b = a.search(r);
        ";

            var vm = SmolVM.Compile(src);

            vm.Run();

            var b = vm.GetGlobalVar<int>("b");

            Assert.AreEqual(5, b);
        }
    }
}

