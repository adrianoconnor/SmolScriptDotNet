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
        public void Getters()
        {
            var source = @"var a = x.y().b.c;";

            var s = new Scanner(source);
            var tokens = s.ScanTokens();
            var p = new Parser(tokens.tokens);
            var dump = new SmolScript.Internals.Ast.AstDump().Print(p.Parse());
            Console.WriteLine(source);
            Console.WriteLine(dump);

            var vm = SmolVM.Compile(source);
            Assert.ThrowsException<NullReferenceException>(vm.RunInDebug);
        }


            [TestMethod]
        public void DefineSimpleClassWithFunc()
        {
            var source = @"

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
";

            var s = new Scanner(source);
            var tokens = s.ScanTokens();
            var p = new Parser(tokens.tokens);
            var dump = new SmolScript.Internals.Ast.AstDump().Print(p.Parse());
            Console.WriteLine(dump);


            var program = SmolCompiler.Compile(source);
            var vm = new SmolVM(program);

            //Console.WriteLine(vm.Decompile());
            //Console.WriteLine(vm.DumpAst());
            vm.RunInDebug();

            var t = (SmolValue)vm.globalEnv.Get("t")!;

            Assert.AreEqual(SmolValueType.ObjectRef, t.type);

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void DefineClassAndCallProps()
        {
            var source = @"

class testClass {
    constructor() {
        this.test = 1;
    }

    addOne() {
        this.test += 1;
    }

    subOne() {
        this.test -= 1;
    }

    square() {
        this.test **= 2;
    }

    div() {
        this.test /= 2;
    }

    mul() {
        this.test *= 2;
    }

    getTest() {
        return this.test;
    }
}

var c = new testClass();
c.addOne();
c.addOne();
c.subOne();
c.square();
c.div();
c.mul();

var a = c.getTest();

";
            /*
            var s = new Scanner(source);
            var t = s.ScanTokens();
            var p = new Parser(t.tokens);
            var dump = new SmolScript.Internals.Ast.AstDump().Print(p.Parse());
            Console.WriteLine(dump);
            */

            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(4.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void DefineClassAndCallNested()
        {
            var source = @"

class testClass1 {
    constructor() {
        this.testClass = new testClass2();
    }

    getTestClass() {
        return this.testClass;
    }
}

class testClass2 {
    constructor() {
        this.test2 = 2;
    }

    getTestValue() {
        return this.test2;
    }
}

var c = new testClass1();

var a = c.testClass;
var b = a.test2;

debugger;

var d = c.testClass.test2;

var e = c.getTestClass().test2;

var f = c.getTestClass().getTestValue();

var g = c.testClass.getTestValue();

";
            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.RunInDebug();

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
            Assert.AreEqual(0, vm.stack.Count);

            vm.RunInDebug();

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("d")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("e")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("f")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("g")!).value);
        }

        [TestMethod]
        public void EmptyStackAfterNew()
        {
            var source = @"

class testClass1 {
  constructor()
  {
    var a = 1;
    1 + 2;
    return a;
  }
}

3 + 4;
var c = new testClass1();

";
            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.RunInDebug();

            Assert.IsNotNull(vm.globalEnv.Get("c"));
            Assert.AreEqual(0, vm.stack.Count);
        }
    }
}

