﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Peak.CodeGeneration
{
    class SymbolTable
    {
        private List<string> loadetFiles = new List<string>();
        public bool IsGlobalScopeTable { get; set; } = false;
        public SymbolTable Prev { get; set; }
        public SymbolTable Next { get; set; }

        public List<TableElement> Data = new List<TableElement>();
        public List<RuntimeEnvironment.RuntimeModule.Constant> ConstandData = new List<RuntimeEnvironment.RuntimeModule.Constant>(); 
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

        private int calculateNewOffsetAddress()
        {
            for (int i = Data.Count-1; i>0; i--)
            {
                if (Data[i].OffsetAddress != -1)
                    return Data[i].OffsetAddress + 1;
            }
            return 0;
        }

        public bool ContainsSymbol(Token name)
        {
            foreach (TableElement t in Data)
            {
                if (t.Name == name.Content)
                    return true;
            }
            if (Prev != null)
                return Prev.ContainsSymbol(name);
            else
                return false;
        }

        public void RegisterSymbol(TableElement tableElement)
        {
            tableElement.Ref = this;
            tableElement.OffsetAddress = calculateNewOffsetAddress();
            this.Data.Add(tableElement);
        }

        public int GetConstantAddress(int content)
        {
            /*foreach(RuntimeEnvironment.RuntimeModule.Constant c in ConstandData)
            {
                if (c.Type == RuntimeEnvironment.RuntimeModule.ConstantType.Int)
            }*/
            this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
            {
                Type = RuntimeEnvironment.RuntimeModule.ConstantType.Int,
                IntValue = content
            });
            return ConstandData.Count - 1;
        }
    }
}
