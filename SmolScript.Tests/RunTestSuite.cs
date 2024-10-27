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

                foreach (var f in testSuiteDirectory.EnumerateFiles("*.test.smol", SearchOption.AllDirectories))
                {
                    tests.Add(new object[] { f.FullName, false });
                    tests.Add(new object[] { f.FullName, true });
                }

                return tests;
            }
        }

        public static string GetCustomDynamicDataDisplayName(MethodInfo methodInfo, object[] data)
        {
            var f = (string)data[0];

            return string.Format("{0}", f.Substring(f.IndexOf("SmolScriptTests") + 16, f.Length - (f.IndexOf("SmolScriptTests") + 16 + 10)));
        }

        Regex _regexTestFileHeader = new Regex(@"\/\*(.*?)(Steps:.*?\n)(.*?)\*\/", RegexOptions.Singleline);
        Regex _regexStepMatcher = new Regex(@"^- (.*?)$", RegexOptions.Multiline);
        Regex _runStepRegex = new Regex(@"- run$", RegexOptions.IgnoreCase);
        Regex _expectGlobalNumberRegex = new Regex(@"- expect global (.*?) to be number (-{0,1}[0-9]+(\.{0,1}[0-9]*))", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
        Regex _expectGlobalStringRegex = new Regex(@"- expect global (.*?) to be string (.*)", RegexOptions.IgnoreCase);
        Regex _expectGlobalBoolRegex = new Regex(@"- expect global (.*?) to be boolean (.*)", RegexOptions.IgnoreCase);
        Regex _expectGlobalUndefinedRegex = new Regex(@"- expect global (.*?) to be undefined", RegexOptions.IgnoreCase);

        [TestMethod]
        [DynamicData(nameof(AllTestData), DynamicDataDisplayName = nameof(GetCustomDynamicDataDisplayName))]
        public void ParseExecuteAndEvaluate(string testFile, bool removeSemicolons)
        { 
            var source = File.ReadAllText(testFile).ReplaceLineEndings("\n");

            if (removeSemicolons)
            {
                var rem = new Regex("(?<!(for\\(.*?;.*?)|for\\(.*?);", RegexOptions.Multiline);
                source = rem.Replace(source, "");
            }

            var headerMatch = _regexTestFileHeader.Matches(source);

            if (headerMatch.Any())
            {
                var stepsBlock = headerMatch[0].Groups[3].Value;
                var matchedSteps = _regexStepMatcher.Matches(stepsBlock);

                if (matchedSteps.Any())
                {
                    var vm = SmolVm.Compile(source);

                    foreach (Match matchedStep in matchedSteps!)
                    {
                        string step = matchedStep.Value;

                        if (_runStepRegex.IsMatch(step))
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
                        else if (_expectGlobalNumberRegex.IsMatch(step))
                        {
                            var m = _expectGlobalNumberRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(Double.Parse(m[0].Groups[2].Value), vm.GetGlobalVar<double>(m[0].Groups[1].Value), step);
                        }
                        else if (_expectGlobalStringRegex.IsMatch(step))
                        {
                            var m = _expectGlobalStringRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(m[0].Groups[2].Value.ReplaceLineEndings(), vm.GetGlobalVar<string>(m[0].Groups[1].Value), step);
                        }
                        else if (_expectGlobalBoolRegex.IsMatch(step))
                        {
                            var m = _expectGlobalBoolRegex.Matches(step);

                            if (!m.Any())
                            {
                                throw new Exception($"Could not parse {step}");
                            }

                            Assert.AreEqual(Boolean.Parse(m[0].Groups[2].Value), vm.GetGlobalVar<bool>(m[0].Groups[1].Value), step);
                        }
                        else if (_expectGlobalUndefinedRegex.IsMatch(step))
                        {
                            var m = _expectGlobalUndefinedRegex.Matches(step);

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

