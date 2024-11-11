using System;
namespace SmolScript.Tests.Internal.Debugger
{
	[TestClass]
	public class StepThroughDebugSourceMapTests
	{
        private string getPendingInstr(SmolVm vm)
        {
            var pending_instr = vm.Program.CodeSections[vm.code_section][vm.PC];

            var pending_instr_first_token = vm.Program.Tokens[pending_instr.token_map_start_index!.Value];
            var pending_instr_last_token = vm.Program.Tokens[pending_instr.token_map_end_index!.Value];

            return vm.Program.Source.Substring(pending_instr_first_token.StartPosition, pending_instr_last_token.EndPosition - pending_instr_first_token.StartPosition);

        }

        [TestMethod]
		public void SimpleTest()
		{

                var source = @"
    debugger;
    var y = 2;
    var x = 0
    if (y == 2)
                {
                    x = 1
    }
                else
                {
                    x = 2
                }
                x = 3";

                var vm = SmolVm.Compile(source);
                vm.Run();
                //console.log(vm.program.decompile());

                //vm.Step(); // Step into the program
                //Assert.IsNull(vm.GetGlobalVar<int>("y")); // Can't do this..
                Assert.AreEqual("var y = 2", getPendingInstr((SmolVm)vm));
                
                vm.Step(); // var y = 2
                Assert.AreEqual(2, vm.GetGlobalVar<int>("y"));

                Assert.AreEqual("var x = 0", getPendingInstr((SmolVm)vm));

                vm.Step(); // var x = 0
            Assert.AreEqual(0, vm.GetGlobalVar<int>("x"));
            /*
            expect(getPendingInstr(vm)).toBe('if (y == 2)');
            vm.step(); // if (y == 2)
            expect(getPendingInstr(vm)).toBe('{');
            vm.step();
            expect(vm.getGlobalVar('x')).toBe(0);
            expect(getPendingInstr(vm)).toBe('x = 1');
            vm.step(); // x = 1 (inside if)
            expect(vm.getGlobalVar('x')).toBe(1);
            vm.step(); // x = 3
            expect(getPendingInstr(vm)).toBe('x = 3');
            vm.step();
            expect(vm.getGlobalVar('x')).toBe(3);
            */
        }
    }
}

