# WARNING: This is a learning project written by someone who knows nothing about building this kind of software. It is not suitable for any kind of production use, and possibly never will be. Use at your own risk.

## SmolScript

SmolScript is a JS-like language that runs inside a tiny stack based VM. It is designed to be relatively easy to port to other languages (so far we've got .net and a half-finished TypeScript version), but each version is going to be different because we use the host language's features very heavily (e.g., we are totally dependent on the .net runtime's implementation of strings, regexes, serialization routines, networking, JSON etc). This choice allows SmolScript itself be *very* smol.

The primary goals of this project are:

1. For me (Adrian O'Connor) to learn how programming languages are implemented, and get a deeper understanding of how compilers and virtual machines work. It's just an itch I've been wanting to scratch since I started programming over 20 years ago.
2. Build a useful Javascript-like embedded runtime that might one day be used as scripting language inside other applications. For example, it could be used for adding user-scripts to an existing application, or running code in a zero trust environment, or building an educational IDE for teaching programming concepts in an interactive way.
3. It is designed to be secure. The run-time is intended to be completely sandboxed from the host application, making it somewhat suitable for running untrusted code. Smol scripts can't access any of the host language's features or resources, but you can create custom bridges using functions and delegates, so you can expose whatever you need. Please note that we make no guarantees about security, it has not been battle tested.

Even though we've based SmolScript on Javascript, it is not anywhere near feature parity and probably never will be. I'm not at all convinced that I want to implement Javascript, this is just a little side project.

What is built so far:

* A working scanner, parser and a 'byte-code' compiler that supports most of the basics that you'd expect in a mini Javascript-like language
* A stack-based Virtual Machine

The byte-code compiler does not produce byte-code at all, all of the instructions are actually objects in the host language, but it doesn't matter because it doesn't generate binaries, they're just an intermediate step.

In terms of language features we have:

* Var (but works more like let!)
* Basic flow control
* Arrays
* Dictionarys (plain Objects, no inheritence)
* Classes (no inheritence)
* First class functions
* Try/Catch
* Basic interop with the host language (call custom defined functions both ways, get values out of the VM)

## I want to use it!

No, it is not ready. I won't get time to support this, it is not a commercial project.

Also, it is incredibly slow -- probably the least optimised language you've ever used. I really have made no effort to optimise, because I want to keep the code simple.

Also, it is a bit messy. Unit tests prove it works as designed, but I made some questionable choices due to lack of experience.

I do hope somebody will find this interesting at least. I've certainly enjoyed creating it, and I've learned so much. Maybe one day it will be actually useful.
