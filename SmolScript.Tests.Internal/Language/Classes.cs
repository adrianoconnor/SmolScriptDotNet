using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class Classes
	{
        public Classes()
		{


		}

        [TestMethod]
        public void DefineSimpleClassWithFunc()
        {
            var program = SmolCompiler.Compile(@"

class testClass {
    constructor() {
        // this.x = 1;
        print('In the CTOR!');
    }

    addOne(n) {
        return n + 1;
    }

    #addTwo(n) {
        return n + 2;
    }
}

var t = new testClass();
var a = t.addOne(1);
");

            var vm = new SmolVM(program);

            //Console.WriteLine(vm.Decompile());
            //Console.WriteLine(vm.DumpAst());
            vm.RunInDebug();

            var t = (SmolValue)vm.globalEnv.Get("t")!;

            Assert.AreEqual(SmolValueType.ObjectRef, t.type);

            //Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void DefineClassAndCallProps()
        {
            var program = SmolCompiler.Compile(@"

class testClass {
    constructor() {
        this.test = 1;
    }

    addOne() {
        this.test += 1;
    }

    getTest() {
        return this.test;
    }
}

var c = new testClass();
c.addOne();

var a = c.getTest();

");

            var vm = new SmolVM(program);

            Console.WriteLine(vm.DumpAst());

            vm.Run();

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }
    }
}

