using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace SmolScript.Tests
{
	[TestClass]
	public class RunTestSuite
	{
        public static IEnumerable<object[]> AllTestData
        {
            get
            {
                var config = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.test.json", true)
                   .Build();
                
                var testSuiteFolderPath = config["TestSuiteFolder"];
                DirectoryInfo? testSuiteDirectory;

                if (!string.IsNullOrEmpty(testSuiteFolderPath))
                {
                    Console.WriteLine($"Got test suite directory '{testSuiteFolderPath}' from AppConfig");
                    testSuiteDirectory = new DirectoryInfo(testSuiteFolderPath);
                }
                else
                {
                    // Assume we're running in the SmolScript.Tests directory (in bin/Debug/net6.0 probably)

                    var pwd = Environment.CurrentDirectory;

                    // We just need to switch from SmolScripts.Tests to the git submodile folder SmolScriptTests
                    testSuiteFolderPath = Path.Combine(pwd.Substring(0, pwd.IndexOf("SmolScript.Tests")), "SmolScriptTests");

                    testSuiteDirectory = new DirectoryInfo(testSuiteFolderPath);
                }

                var tests = new List<object[]>();

                foreach(var f in testSuiteDirectory.EnumerateFiles("*.test.smol", SearchOption.AllDirectories))
                {
                    tests.Add(new object[] { f.FullName });
                }

                return tests;
            }
        }

        public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
        {
            var f = (string)data[0];

            return string.Format("{0}", f.Substring(f.IndexOf("SmolScriptTests") + 16, f.Length - (f.IndexOf("SmolScriptTests") + 16 + 10)));
        }

        Regex regexTestFileHeader = new Regex(@"\/\*(.*?)(Steps:.*?\n)(.*?)\*\/", RegexOptions.Singleline);
        Regex regexStepMatcher = new Regex(@"^- (.*?)$", RegexOptions.Multiline);
        Regex runStepRegex = new Regex(@"- run$", RegexOptions.IgnoreCase);
        Regex expectGlobalNumberRegex = new Regex(@"- Expect global (.*?) to be number ([0-9]+(\.{0,1}[0-9]*))", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
        Regex expectGlobalStringRegex = new Regex(@"- expect global (.*?) to be string (.*)", RegexOptions.IgnoreCase);
        Regex expectGlobalBoolRegex = new Regex(@"- expect global (.*?) to be boolean (.*)", RegexOptions.IgnoreCase);
        Regex expectGlobalUndefinedRegex = new Regex(@"- expect global (.*?) to be undefined", RegexOptions.IgnoreCase);

        [TestMethod]
        [DynamicData(nameof(AllTestData), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void ParseExecuteAndEvaluate(string testFile)
        {
            var fileData = File.ReadAllText(testFile);

            var headerMatch = regexTestFileHeader.Matches(fileData);

            if (headerMatch.Any())
            {
                var stepsBlock = headerMatch[0].Groups[3].Value;
                var matchedSteps = regexStepMatcher.Matches(stepsBlock);

                if (matchedSteps.Any())
                {
                    var vm = SmolVM.Compile(fileData);

                    foreach(Match matchedStep in matchedSteps!)
                    {
                        string step = matchedStep.Value;

                        if (runStepRegex.IsMatch(step))
                        {
                            try
                            {
                                vm.Run();
                            }
                            catch (Exception)
                            {
                                //console.log(test.fileData);
                                //console.log(vm.decompile());
                                //console.log(debugLog);
                                throw;
                            }
                        }
                        else if (expectGlobalNumberRegex.IsMatch(step))
                        {
                            var m = expectGlobalNumberRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(vm.GetGlobalVar<double>(m[0].Groups[1].Value), Double.Parse(m[0].Groups[2].Value), step);
                        }
                        else if (expectGlobalStringRegex.IsMatch(step))
                        {
                            var m = expectGlobalStringRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(vm.GetGlobalVar<string>(m[0].Groups[1].Value), m[0].Groups[2].Value, step);
                        }
                        else if (expectGlobalBoolRegex.IsMatch(step))
                        {
                            var m = expectGlobalBoolRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(vm.GetGlobalVar<bool>(m[0].Groups[1].Value), Boolean.Parse(m[0].Groups[2].Value), step);
                        }
                        else if (expectGlobalUndefinedRegex.IsMatch(step))
                        {
                            var m = expectGlobalUndefinedRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.IsNull(vm.GetGlobalVar<string>(m[0].Groups[1].Value), step);
                        }
                        else
                        {
                            throw new Exception($"Could not parse step: {step}");
                        }
                    }
                }
            }
            else
            {
                throw new Exception("No steps :(");
            }
        }
	}
}

