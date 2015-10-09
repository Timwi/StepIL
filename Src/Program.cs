using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;
using Mono.Cecil.Pdb;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace StepIL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("One argument expected: Input file (DLL or EXE).");
                Console.WriteLine("A .pdb is expected to exist in the same directory.");
                Console.WriteLine("A .il file will be created (and overwritten if it exists).");
                return;
            }

            var assemblyFile = args[0];
            var ilPath = Path.Combine(Path.GetDirectoryName(assemblyFile), Path.GetFileNameWithoutExtension(assemblyFile) + ".il");
            var pdbPath = Path.Combine(Path.GetDirectoryName(assemblyFile), Path.GetFileNameWithoutExtension(assemblyFile) + ".pdb");

            var assembly = AssemblyDefinition.ReadAssembly(assemblyFile);
            assembly.MainModule.ReadSymbols();
            var doc = new Document(ilPath) { Language = DocumentLanguage.Cil };

            using (var ilFile = File.Open(ilPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var il = new StreamWriter(ilFile))
            {
                var curLine = 1;
                Action<string> writeLine = line =>
                {
                    curLine += line.Count(ch => ch == '\n') + 1;
                    il.WriteLine(line);
                };

                foreach (var type in assembly.Modules.SelectMany(m => m.Types))
                {
                    writeLine("Type: " + type.FullName);
                    foreach (var method in type.Methods)
                    {
                        writeLine("    Method: " + method.FullName);
                        if (method.Body != null)
                        {
                            foreach (var local in method.Body.Variables)
                                writeLine("        Local: " + local.VariableType + " " + local.Name);
                            writeLine("");
                            var newInstructions = new Collection<Instruction>();
                            foreach (var instruction in method.Body.Instructions)
                            {
                                if (newInstructions.Count > 0)
                                    newInstructions.Add(Instruction.Create(OpCodes.Nop));
                                newInstructions.Add(instruction);
                                var prevLine = curLine + 1;
                                var instrStr = instruction.ToString();
                                writeLine("        " + instrStr);
                                instruction.SequencePoint = new SequencePoint(doc)
                                {
                                    StartLine = prevLine,
                                    StartColumn = 9,
                                    EndLine = curLine,
                                    EndColumn = (instrStr.Contains('\n') ? instrStr.Length - instrStr.LastIndexOf('\n') : instrStr.Length) + 9
                                };
                            }
                            method.Body.Instructions.Clear();
                            foreach (var instr in newInstructions)
                                method.Body.Instructions.Add(instr);
                            writeLine("");
                        }
                        writeLine("");
                    }
                }
            }

            assembly.Write(assemblyFile, new WriterParameters { WriteSymbols = true });
        }
    }
}
