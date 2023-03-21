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
        public void ComplexTryCatchTestsWillFail()
        {
            var program = SmolCompiler.Compile("class ValidationError extends Error {\n  printCustomerMessage() {\n    return `Validation failed :-( (details: ${this.message})`;\n  }\n}\n\ntry {\n  throw new ValidationError(\"Not a valid phone number\");\n} catch (error) {\n  if (error instanceof ValidationError) {\n    console.log(error.name); // This is Error instead of ValidationError!\n    console.log(error.printCustomerMessage());\n  } else {\n    console.log(\"Unknown error\", error);\n    throw error;\n  }\n}");

            var vm = new SmolVM(program);

            vm.Run();

            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

    }
}

