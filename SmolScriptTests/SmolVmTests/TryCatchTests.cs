using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class ComplexTryCatchTests
	{
        public ComplexTryCatchTests()
		{


		}

        [TestMethod]
        public void SimpleTryCatchSyntaxTest()
        {
            //            var source = "try { var a = 1; } catch { /* nothing */ }"; // Inline comment breaks the scanner :(
            var source = "try { var a = 1; } catch { var b = 2; }";

            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);
            
            vm.Run();

            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void SimpleTryCatchSyntaxTest2()
        {
            var source = "var a = 1; try { a = 2; } catch(e) { a = 3; }";

            var s = new Scanner(source);

            var tokens = s.ScanTokens();

            foreach (var t in tokens.tokens)
            {
                Console.WriteLine(t);
            }

            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void SimpleTryCatchSyntaxTest3()
        {
            var source = "var a = 1; try { a = 2; } catch(e) { } finally { a = 3; } ";

            var s = new Scanner(source);

            var tokens = s.ScanTokens();

            foreach (var t in tokens.tokens)
            {
                Console.WriteLine(t);
            }

            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.Run();

            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void ComplexTryCatchTestsWillFail()
        {
            var source = "class ValidationError extends Error {\n  printCustomerMessage() {\n    return `Validation failed :-( (details: ${this.message})`;\n  }\n}\n\ntry {\n  throw new ValidationError(\"Not a valid phone number\");\n} catch (error) {\n  if (error instanceof ValidationError) {\n    console.log(error.name); // This is Error instead of ValidationError!\n    console.log(error.printCustomerMessage());\n  } else {\n    console.log(\"Unknown error\", error);\n    throw error;\n  }\n}";

            var s = new Scanner(source);

            var tokens = s.ScanTokens();

            foreach (var t in tokens.tokens)
            {
                Console.WriteLine(t);
            }

            var program = SmolCompiler.Compile(source);

            var vm = new SmolVM(program);

            vm.Run();

            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

    }
}

