namespace SmolScript.Tests.String
{
    [TestClass]
    public class StringNativeMethodTests
    {
        [TestMethod]
        public void StringLength()
        {
            var src = @"
            var a = 'Test String';
            var b = a.length;
        ";

            var vm = SmolVM.Compile(src);

            vm.Run();

            var a = vm.GetGlobalVar<int>("b");

            Assert.AreEqual(11, a);
        }

        [TestMethod]
        public void StringIndexOf()
        {
            var src = @"
            var a = 'Test String';
            var b = a.indexOf('Str');
        ";

            var vm = SmolVM.Compile(src);

            vm.Run();

            var a = vm.GetGlobalVar<int>("b");

            Assert.AreEqual(5, a);
        }
    }
}

