using System;
namespace SmolScript.Tests.Comments
{
    [TestClass]
    public class BasicComments
    {
        [TestMethod]
        public void SingleLineComments()
        {
            var code = @"

var a = 1;
//function f(x) {
// x(10);
//}
function f(x) { // ?
  x(10); // ?
} //?
f(function(z) { a += 10; });
//
";

            ISmolRuntime vm = SmolVM.Compile(code);

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.Run();

            var a = vm.GetGlobalVar<int>("a");

            Assert.AreEqual(11, a);
        }


        [TestMethod]
        public void MultiLineComments()
        {
            var code = @"

var a = 1;
/*
//* * //
function f(x) {
  x(10);
}
*/
function f(x) {
  x(10);
}
/* 
' "" @
* This breaks?
*/

f(function(z) { a += 10; });
";

            ISmolRuntime vm = SmolVM.Compile(code);

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.Run();

            var a = vm.GetGlobalVar<int>("a");

            Assert.AreEqual(11, a);
        }
    }
}

