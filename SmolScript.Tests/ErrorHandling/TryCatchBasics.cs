using System;
namespace SmolScript.Tests.ErrorHandling
{
    [TestClass]
    public class TryCatchBasics
    {
        [TestMethod]
        public void TryCatchOnly()
        {
            var code = @"
var a = 1;

try {
  a = b * 2; // Undefined * 2 will throw an error
}
catch {
  a = 2;
}
";
            var vm = SmolVM.Compile(code);

            vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("a"));
        }

        [TestMethod]
        public void TryFinallyOnly()
        {
            var code = @"
var a = 1;

try {
  a = b * 2;
}
finally {
  a = 4;
}
";
            var vm = SmolVM.Compile(code);

            Assert.ThrowsException<SmolRuntimeException>(() => vm.Run());

            Assert.AreEqual(4.0, vm.GetGlobalVar<int>("a"));
        }

        [TestMethod]
        public void TryCatchAndFinallyWithException()
        {
            var code = @"
var a = 1;
var b = 1;

try {
  a = c * 2; // Undefined * 2 will throw an error
}
catch {
  a = 2;
}
finally {
  b = 2;
}
";
            var vm = SmolVM.Compile(code);

            vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("b"));
        }

        [TestMethod]
        public void TryCatchAndFinallyWithExceptionInCatch()
        {
            var code = @"
var a = 1;
var b = 1;

try {
    try {
      a = c * 2; // Undefined * 2 will throw an error
    }
    catch {
      a = c * 3;
    }
    finally {
      b = 2;
    }
}
catch(e) {
  throw e;
}
";
            var vm = SmolVM.Compile(code);

            vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(1.0, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("b"));
        }

        [TestMethod]
        public void TryCatchAndFinallyWithoutException()
        {
            var code = @"
var a = 1;
var b = 1;

try {
  a = 2;
}
catch {
  a = 3;
}
finally {
  b = 2;
}
";
            var vm = SmolVM.Compile(code);

            vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<int>("b"));
        }

        [TestMethod]
        public void TryCatchWithExplicitError()
        {
            var source = @"
var a = 1;
var err = '';

try {
  a = 2;
  throw 'error';
}
catch(e) {
  a = 3;
  err = e;
}

var b = 4;
";

            var vm = new SmolVM(source);

            vm.OnDebugLog = Console.WriteLine;

            vm.Run();

            //Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("b"));

            Assert.AreEqual("error", vm.GetGlobalVar<string>("err"));
        }

        [TestMethod]
        public void TryCatchWithImplicitError()
        {
            var source = @"
var a = 1;
var err = '';

try {
  a = 2 * b;
}
catch(e) {
  a = 3;
  err = e;
}

var b = 4;
";

            var vm = new SmolVM(source);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("b"));

            Assert.AreEqual("Unable to multiply Number and Undefined", vm.GetGlobalVar<string>("err"));
        }
    }
}

