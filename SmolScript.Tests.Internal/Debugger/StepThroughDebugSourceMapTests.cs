using System;
namespace SmolScript.Tests.Internal.Debugger
{
	[TestClass]
	public class StepThroughDebugSourceMapTests
	{
        public string getPendingInstr(SmolVM vm)
        {
            var pending_instr = vm.program.code_sections[vm.code_section][vm.PC];

            var pending_instr_first_token = vm.program.tokens[pending_instr.token_map_start_index!.Value];
            var pending_instr_last_token = vm.program.tokens[pending_instr.token_map_end_index!.Value];

            return vm.program.source.Substring(pending_instr_first_token.start_pos, pending_instr_last_token.end_pos - pending_instr_first_token.start_pos);

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

                var vm = SmolVM.Compile(source);
                vm.Run();
                //console.log(vm.program.decompile());

                //vm.Step(); // Step into the program
                //Assert.IsNull(vm.GetGlobalVar<int>("y")); // Can't do this..
                Assert.AreEqual("var y = 2", getPendingInstr((SmolVM)vm));
                
                vm.Step(); // var y = 2
                Assert.AreEqual(2, vm.GetGlobalVar<int>("y"));

                Assert.AreEqual("var x = 0", getPendingInstr((SmolVM)vm));

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

