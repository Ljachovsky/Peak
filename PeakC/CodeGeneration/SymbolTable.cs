﻿using Peak.PeakC;
using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Peak.CodeGeneration
{
    partial class SymbolTable
    {
        private List<string> loadetFiles = new List<string>(); //only for global scope
        public bool IsGlobalScope { get; set; } = false; // global memory-scope, not var-definition scope 
        public bool IsMethodDefTable { get; set; } = false;
        public SymbolTable Prev { get; set; } // only for if-else-while

        public List<TableElement> Data = new List<TableElement>();
        public List<RuntimeEnvironment.RuntimeModule.Constant> ConstandData = new List<RuntimeEnvironment.RuntimeModule.Constant>();
        public int MemorySize { get; set; }


        public MethodDescription CurrentMethod { get; set; }
        public RuntimeModule CurrentRuntimeModule { get; set; }
        
        public int MethodContextIndex { get; set; } // to equals refenerences on methods-context 

        public SymbolTable()
        {

        }

        public bool IsNewFile(string file)
        {

            if (this.loadetFiles.Contains(Path.GetFullPath(file)))
                return true;
            return false;
        }

        public void RegisterFile(string file)
        {
            this.loadetFiles.Add(file);
        }

        private int calculateNewOffsetAddress() // for last element throught if-else-while context
        {
            var table = this;

            while (true)
            {
                for (int i = Data.Count - 1; i >= 0; i--)
                {
                    if (Data[i].OffsetAddress != -1)
                    {
                        expandMemorySizeByLastAddress(Data[i].OffsetAddress + 1);
                        return Data[i].OffsetAddress + 1;
                    }

                }

                if (this.Prev == null || this.IsMethodDefTable)
                {
                    expandMemorySizeByLastAddress(0);
                    return 0;
                }
                else
                    table = table.Prev;
            }
        }

     
       
        public void RegisterSymbol(TableElement tableElement)
        {
            tableElement.Ref = this;
            tableElement.OffsetAddress = calculateNewOffsetAddress();
            this.Data.Add(tableElement);
            //if ()
        }


        private void expandMemorySizeByLastAddress(int size)
        {
            if (this.MemorySize < size + 1)
                MemorySize = size + 1;
        }
        public int GetConstantAddress(ConstValueNode node)
        {
            if (node.Value.Type == type.IntValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Int,
                    IntValue = int.Parse(node.Value.Content)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.DoubleValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Double,
                    DoubleValue = double.Parse(node.Value.Content, CultureInfo.InvariantCulture)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.BoolValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Bool,
                    BoolValue = bool.Parse(node.Value.Content)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.StrValue)
            {
                return GetConstantAddress(node.Value.Content);
            }
            else
                throw new Exception();
        }
        public int GetConstantAddress(string S)
        {
            this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
            {
                Type = RuntimeEnvironment.RuntimeModule.ConstantType.Str,
                StrValue = S
            });
            return ConstandData.Count - 1;
        }
    }
}
