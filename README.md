# WARNING: This is a learning project written by someone who knows nothing about building this kind of software. It is not suitable for any kind of production use and possibly never will be. Use at your own risk.

## SmolScript

SmolScript is a JS-like language that runs inside a tiny stack based VM. It is designed to be relatively easy to port to other languages (so far we've got .net and a half-finished TypeScript version), but each version is going to be different because we use the host language's features very heavily (e.g., we are totally dependent on the .net runtime's implementation of strings, regexes, serialization routines, networking, JSON etc). This choice allows SmolScript itself be *very* smol.

The primary goals of this project are:

1. For me (Adrian O'Connor) to learn programming language fundamentals, and get a deeper understanding of how compilers and virtual machines work. It's just an itch I've been wanting to scratch since I started programming.
2. Build a small JS-like langauge that might one day be useful as a pluggable/embedded scripting system for various uses (like running custom user scripts in a zero trust environment, or building an educational runtime for teaching programming concepts in an interactive way).
3. Be *very* secure. The run-time is intended to be completely sandboxed from the host application, making it suitable for running untrusted code. By default there's no bridge between the VM and the host, but you can register custom functions as callbacks and then call them from your smol scripts, giving you a nice way to expose just the bare minimum surface area of integration that you need.

Even though we've based SmolScript on JS, it is not anywhere near feature parity and never will be. In this first alpha version we've made some decisions that we might change later, but right now you need to declare all variables before use, and var works like let. I'm not at all convinced that I want to implement an actual JS, this is just a little side project.

What is built so far:

* A working simple stack-based Virtual Machine with ZERO optimizations. We can branch, loop, assign, evaluate etc.
* A working scanner, parser (builds an AST) and a byte-code compiler that supports variable and function declarations, assignment, comparison, boolean logic, if/then support.

What is still left on our todo list, in rough priority order:

* Arrays
* Dictionarys
* JS-like objects with prototypical inheritance (maybe)
* Classes
* Function expressions
* Try/Catch
