using System;
namespace SmolScript.Tests.String
{
    [TestClass]
    public class BasicStringTests
    {
        [TestMethod]
        public void CreateStringWithNew()
        {
            var code = @"
var a1 = new String(123);
var a2 = new String('123 456');
var a3 = new String(a1);
var a1_len = a1.length;
var a2_index = a2.indexOf('456');
var a4 = new String(); // In node this gives an empty string
";

            var vm = SmolVM.Compile(code);

            vm.Run();

            Assert.AreEqual("123", vm.GetGlobalVar<string>("a1"));
            Assert.AreEqual("123 456", vm.GetGlobalVar<string>("a2"));
            Assert.AreEqual("123", vm.GetGlobalVar<string>("a3"));
            Assert.AreEqual("", vm.GetGlobalVar<string>("a4"));

            Assert.AreEqual(3, vm.GetGlobalVar<int>("a1_len"));
            Assert.AreEqual(4, vm.GetGlobalVar<int>("a2_index"));
        }

        [TestMethod]
        public void ConcatStrings()
        {
            var code = @"
var a1 = new String(123);
var a2 = ""456"";
var a3 = '789';
var b = a1 + a2 + a3;
";

            var vm = SmolVM.Init(code);

            Assert.AreEqual("123456789", vm.GetGlobalVar<string>("b"));
            Assert.AreEqual(123456789, vm.GetGlobalVar<int>("b"));
        }
    }
}