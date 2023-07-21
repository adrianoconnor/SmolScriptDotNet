> WARNING: This is a learning project written by someone who knows nothing about building this kind of software. It is not suitable for any kind of production use, and probably never will be. Use at your own risk.

# SmolScript

SmolScript is a JS-like language that runs inside a tiny stack based VM. It is designed to be relatively easy to implement in other languages (there is also a TypeScript version currently), but each version is going to be different because we use the host language's features very heavily (e.g., we are totally dependent on the .net implementation of strings, regexes, serialization routines, networking, JSON etc). This choice allows SmolScript itself be *very* smol.

A demo website (that runs the type script version in the browser) is coming soon-ish, but it's not ready yet -- it will let you visualise the compiled program along with the VM internal state and we're quite exicited about it :) 

The primary goals of this project are:

1. For me (Adrian O'Connor) to learn how programming languages are implemented, and get a deeper understanding of how compilers and virtual machines work. It's just an itch I've been wanting to scratch since I started programming many years ago.
2. Build a potentially useful Javascript-like runtime that could be used as embedded scripting language inside other applications. For example, it could be used for adding macros to an existing application, or running code in a zero trust environment, or building an educational IDE for teaching programming concepts in an interactive way.
3. It is designed to be secure (but there are no guarantees). The run-time is intended to be completely sandboxed from the host application, making it somewhat suitable for running untrusted code. Smol scripts can't access any of the host language's features or resources, but you can create custom bridges using functions and delegates, so you can expose whatever you need. Please note that we make no guarantees about security.

One goal we do not have is performance. This is probably the slowest Javascript-like engine you will ever use! We have absolutely prioritised simplicity of code over high performance.

Even though we've based SmolScript on Javascript, it is not anywhere near feature parity and never will be, this really is just a little side project. However, whenever we add new features we always start by looking at how Javascript does it, and over time that has become our defacto approach.

What is built so far:

* A working 'byte-code' compiler (including hand rolled scanner and parser) that supports most of the basic language features you'd expect in a tiny Javascript-like language
* A stack-based Virtual Machine that supports break points and step through and a small degree of observability (with more to come)
* An easy way to expose custom functions from .net using lambdas to wrap the native code (no need for adding new types/interfaces) that automatically deal with type coercion
* A test suite that I think almost covers the entire language -- we've built this in a way that the test files can be shared across all smol implementations (but I'm still moving stuff from the early .net development unit tests to the shared suite so that's not complete)

The byte-code compiler does not produce byte-code at all, all of the instructions are actually objects in the host language, but it doesn't matter because it doesn't generate binaries, they're just an intermediate step.

In terms of language features, we currently have:

* Var only for variables (but in smol var works more like let)
* Flow control
* Basic Arrays
* Basic Dictionarys (plain Objects, no inheritence)
* Classes (no inheritence)
* First class functions
* Basic Try/Catch
* Regex

On our list of things that we think we want to add:

* for ... in
* JSON
* Pass .net objects in as paramters and use reflection to access them dynamicly
* => syntax
* Better try/catch
* Exposing tokens/line numbers for compiled source
* Much better compiler errors
* Support for code without semicolons

What is not on our roadmap to support:

* Modules (except maybe basic include statements, equivalent to multiple script tags on a webpage)
* Async/await
* file, network, database etc -- for that we expect you to use native custom functions exposed to your VM (this is how we make it secure!)

## Try it out

To try it out, add the SmolScript nuget package to your project (currently version 0.0.2) and do something like this:

```csharp
using SmolScript;

var exampleSmolScriptCode = @"

  // This is the smol script program, not .net
  var a = 10;

  function multiplyA(valueToMultiplyBy) {
    a = a * valueToMultiplyBy;
  }

  multiplyA(10);
";

var vm = SmolVM.Compile(exampleSmolScriptCode);

vm.Run(); // This executes the script we passed in

var valueOfA = vm.GetGlobalVar<int>("a"); // Fetch the value of variable 'a' from the VM, will be 100
```

That's it! If you want to call the function from .net:

```csharp
// You can only call after vm.Run or vm.Init has been called -- so the VM is primed with initial state...

vm.Call("multiplyA", 2);

var newValueOfA = vm.GetGlobalVar<int>("a"); // 200
```

## I want to use it in my project!

No, it is not ready, it is not a commercial project and I don't have time to support it.

Also, it is incredibly slow -- probably the least optimised language you've ever used. I really have made no effort to optimise, because I want to keep the code simple.

Also, it is a bit messy. Unit tests prove it works as designed, but I made some questionable choices due to lack of experience.

I do hope somebody will find this interesting at least. I've certainly enjoyed creating it, and I've learned so much. Maybe one day it will be actually useful.

This version is MIT licensed.
