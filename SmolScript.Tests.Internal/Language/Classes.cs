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

            vm.RunInDebug();

            Assert.AreEqual(4.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void DefineClassAndCallNested()
        {
            var source = @"

class testClass1 {
    constructor() {
        this.x = 2;

        //debugger;
        var m = new testClass2();

        this.testClass = m;
        //print(m);
        //print(this.testClass);
        //print('leaving ctor');
    }

    getTestClass() {
        //print('getTestClass()');
        //print(this.testClass);
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

function x() {
   var t = new testClass1();
    //print ('?');
   return t;
}

var c = new testClass1(); // x();
//print ('?');
var a = c.testClass;
var b = a.test2;
//var c = c.testClass.test2; // Above works, this does not :(

";
            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.RunInDebug();

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
            Assert.AreEqual(0, vm.stack.Count);
        }

        [TestMethod]
        public void EmptyStackAfterNew()
        {
            var source = @"

class testClass1 {
constructor()
{
}
}


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

