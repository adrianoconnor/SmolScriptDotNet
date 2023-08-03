using System;
namespace SmolScript.Tests.ErrorHandling
{
    [TestClass]
    public class TryCatchBasics2
    {

        [TestMethod]
        public void SimpleTryCatchSyntaxTest()
        {
            var source = "var a = 0; try { a = 1; } catch { a = 2; }";

            var vm = SmolVM.Compile(source);
            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void SimpleTryCatchSyntaxTest2()
        {
            var source = "var a = 1; try { a = 2; } catch(e) { a = 3; }";

            var vm = SmolVM.Compile(source);

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void SimpleTryCatchSyntaxTest3()
        {
            var source = "var a = 1; try { a = 2; } catch(e) { } finally { a = 3; } ";

            var vm = SmolVM.Compile(source);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void ComplexTryCatchTestsWillFail()
        {
            var source = "var a = 1; try { a = 2; throw 'Error'; } catch(e) { a = 3; debugger; } finally { a = 4; } var b = 4;";

            var vm = SmolVM.Compile(source);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));

            vm.Run();

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("a"));

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("b"));
        }
    }
}

