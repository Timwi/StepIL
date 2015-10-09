Instructions
=

* Compile your program normally using Visual Studio. This will create a compiled binary (`.exe` or `.dll`) and a symbol file (`.pdb`).

* Run StepIL and pass it the compiled binary on the command-line. For example:

        StepIL.exe MyProgram.exe

    This command will overwrite the `.pdb` file with a new one, and it will also create a `.il` file (in the same directory) containing all the IL code from your binary.

* Hit F10 in Visual Studio to start debugging your program. If it prompts you for the `.il` file, browse to it.

* You will now step through the IL code. You can use Step Over (F10), Step Into (F11), Run to Cursor, everything. All the normal debugging mechanisms (Locals, Watch, Call stack, etc.) should work.

* If your code calls code in another assembly (usually a library) whose source is also in your project’s solution, the debugger will jump into C# code when you Step Into (F11) the library code. You can run StepIL separately on any assembly for which you wish to step through IL instead of C#.

* If you need to go back from stepping through IL to stepping through C# code normally, just recompile the project. If you wish to step through IL all the time, you will have to run StepIL every time after compiling. You could, if you were so inclined, set it up as a “Post-build event” in the Build Events tabs of the project’s properties.
