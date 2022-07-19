﻿using Peak.PeakC;
using Peak.PeakC.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peak.AsmGeneration
{
    static class CodeGeneration
    {

        public static AsmModel GetAsmAssembly(ProgramNode node)
        {
            // TODO: make asm builder
            var table = new GlobalSymbolTable();
            generateForProgramNode(node, table);

            return table.MainAssembly;
        }

      

        private static void generateForProgramNode(ProgramNode node, GlobalSymbolTable st)
        {
            var rbpSizeOperand = GenMethodPrologueAndGet_rbp(st);
            foreach (Node n in node.Node)
            {
                if (n is LoadNode)
                {
                    applyLoadNode((LoadNode)n, st);
                }
                else
                    CodeBlock.Generate((CodeBlockNode)n, st);
            }
            int frameSize = MemoryAllocator.AlignUpAbsolute(st.MemoryAllocator.GetFrameSize(), 16);
            rbpSizeOperand.Offset = st.MemoryAllocator.GetFrameSize();
        }

        public static Operand GenMethodPrologueAndGet_rbp(GlobalSymbolTable st)
        {
            var frameSizeOperand = new Operand() { Offset = 0 };
            st.MethodCode.Emit(InstructionName.Push, RegisterName.RBP);
            st.MethodCode.Emit(InstructionName.Mov, RegisterName.RBP, RegisterName.RSP);
            st.MethodCode.Emit(InstructionName.Sub, RegisterName.RSP, frameSizeOperand);

            return frameSizeOperand;
        }

        private static void applyLoadNode(LoadNode node, GlobalSymbolTable st)
        {
            string fileName = (node as LoadNode).LoadFileName.Content;

            var paths = new string[6]
            {
                Directory.GetCurrentDirectory() + "\\" + fileName,
                Directory.GetCurrentDirectory() + "\\" + fileName + ".p",
                Directory.GetCurrentDirectory() + "\\lib\\" + fileName,
                Directory.GetCurrentDirectory() + "\\lib\\" + fileName + ".p",
                node.MetaInf.File + "\\" + fileName,
                node.MetaInf.File + "\\" + fileName + ".p"
            };
            /*
            
            if (Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName + ".p")
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName + ".p")
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName)
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName + ".p")
                )*/

            foreach (string path in paths)
                if (File.Exists(path))
                    if (st.IsNewFile(fileName))
                    {
                        st.RegisterFile(fileName);
                        var p = new Parser();
                        generateForProgramNode(p.GetNode(path), st);
                        return;
                    }
                    else
                        Error.WarningMessage(node.MetaInf, "file already loadet");

            Error.FileNotFoundErrMessage(node.LoadFileName);

        }
        

        
    }
}