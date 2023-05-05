using System;
using SmolScript;
using SmolScript.Internals;
using SmolScript.Internals.SmolStackTypes;

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
            Assert.ThrowsException<Exception>(vm.RunInDebug);
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

            var t = vm.globalEnv.Get("t")!;

            Assert.AreEqual(typeof(SmolObject), t.GetType());

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
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

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("a"));
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

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
            Assert.AreEqual(0, vm.stack.Count);

            vm.RunInDebug();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("d"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("e"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("f"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("g"));
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

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.RunInDebug();

            Assert.IsNotNull(vm.globalEnv.Get("c"));
            Assert.AreEqual(0, vm.stack.Count);
        }

        [TestMethod]
        public void ClassWithCtorArgs()
        {
            var source = @"

class testClass {
    constructor(y) {
        this.x = y + 2;
    }
}

var t = new testClass(1);
var a = t.x;
";
            var vm = SmolVM.Compile(source);

            vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
        }
    }
}

