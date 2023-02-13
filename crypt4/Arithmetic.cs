using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crypt4
{
    class Arithmetic
    {
        public int bitsPerSymbol = 0;
        public Dictionary<char, int> SymbolsFrequency = new Dictionary<char, int> { };
        public Dictionary<char, decimal> SymbolsProbability = new Dictionary<char, decimal> { };
        public string FileContent = "";
        public Arithmetic(string FileContent)
        {
            this.FileContent = FileContent;
            FindSymbolsFrequency(FileContent);
        }
        public Arithmetic() { }

        // ---------- COMPRESSION ---------- //
        private Dictionary<char, decimal> CreateProbabilityTable(Dictionary<char, int> symbolsFrequency, int fileLength)
        {
            decimal fileLen = Convert.ToDecimal(fileLength);
            Dictionary<char, decimal> pt = new Dictionary<char, decimal> { };
            foreach (var symb in symbolsFrequency)
                pt.Add(symb.Key, Convert.ToDecimal(symb.Value)/fileLen);

            return pt;
        }

        private Dictionary<char, Symbol> SetSymbolBounds(Dictionary<char, decimal> pt)
        {
            Dictionary<char, Symbol> symbols = new Dictionary<char, Symbol> { };
            decimal low = 0.0m;
            decimal high = 0.0m;
            foreach (var symb in pt)
            {
                high += symb.Value;
                symbols.Add(symb.Key, new Symbol(symb.Key, low, high));
                low = high;
            }

            return symbols;
        }


        private string EncodeText(string file, Dictionary<char, Symbol> symbols)
        {
            string text = "";
            foreach (var symb in SymbolsFrequency)
                text += $"{symb.Key}{symb.Value} ";
            text += "\n\n";
            decimal low = 0.0m;
            decimal high = 1.0m;
            decimal currentRange = high - low;
            Symbol s = null;
            for (int i = 0; i < file.Length; i++)
            {
                s = symbols[file[i]];
                high = low + (currentRange * s.High);
                low = low + (currentRange * s.Low);
                currentRange = high - low;
                //Debug.WriteLine($"({low}:{high})");
            }
            text += ((high + low) / 2.0m).ToString();

            return text;
        }

        public string Compress(string file)
        {
            if (this.SymbolsFrequency.Count == 0)
                FindSymbolsFrequency(file);
            // create probability table
            SymbolsProbability = CreateProbabilityTable(this.SymbolsFrequency, file.Length);
            // set bounds to symbols
            Dictionary<char, Symbol> symbols = SetSymbolBounds(SymbolsProbability);
            // encode the file contents
            string encodedFile = EncodeText(file, symbols);

            return encodedFile;
        }
        // -------------------- //

        // ---------- DECOMPRESSION ---------- //
        private Dictionary<char, int> CreateFreqDictionary(string header)
        {
            Dictionary<char, int> sf = new Dictionary<char, int> { };
            List<string> symbols = (header.Split(' ')).ToList();
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] != "")
                {
                    string s = symbols[i];
                    string value = "";
                    for (int j = 1; j < s.Length; j++)
                        value += s[j];
                    sf.Add(s[0], Convert.ToInt32(value));
                }
                else
                {
                    if (i < symbols.Count - 1)
                    {
                        symbols[i] = $" {symbols[i + 1]}";
                        symbols.RemoveAt(i + 1);
                        i--;
                    }
                    else
                        symbols.RemoveAt(i);
                }
            }

            return sf;
        }
        private string DecodeText(string file, int fileLength, Dictionary<char, Symbol> symbols)
        {
            string text = "";
            decimal value = Convert.ToDecimal(file);
            //Debug.WriteLine(file);
            //Debug.WriteLine(value.ToString());
            decimal low = 0.0m;
            decimal high = 1.0m;
            decimal currentRange = high - low;
            for(int i = 0; i < fileLength; i++)
            {
                foreach (var symb in symbols)
                {
                    if (symb.Value.InRange(value))
                    {
                        text += symb.Key;
                        currentRange = symb.Value.High - symb.Value.Low;
                        value = (value - symb.Value.Low) / currentRange;
                        //Debug.WriteLine(value.ToString());
                        break;
                    }
                }
            }
            return text;
        }

        public string Decompress(string file)
        {
            // split the header and encoded text
            string header = "";
            string content = "";
            int i = 0;
            while ($"{file[i]}{file[i + 1]}" != "\n\n")
            {
                header += file[i];
                i++;
            }
            i += 2;
            for (int j = i; j < file.Length; j++)
                content += file[j];
            // create symbols frequency dictionary
            Dictionary<char, int> symbFreq = CreateFreqDictionary(header);
            for (int j = 0; j < symbFreq.Count; j++)
            {
                Debug.WriteLine($"({symbFreq.ElementAt(j).Key} {symbFreq.ElementAt(j).Value}) - [{SymbolsFrequency.ElementAt(j).Key} {SymbolsFrequency.ElementAt(j).Value}]");
            }
            // create probability table
            int fileLength = 0;
            foreach (var s in symbFreq)
                fileLength += s.Value;
            Dictionary<char, decimal> SymbolsProb = CreateProbabilityTable(this.SymbolsFrequency, fileLength);
            // set bounds to symbols
            Dictionary<char, Symbol> symbols = SetSymbolBounds(SymbolsProb);
            // encode the file contents
            string decodedText = DecodeText(content, fileLength, symbols);

            return decodedText;
        }
        // -------------------- //

        private void FindSymbolsFrequency(string file)
        {
            for (int i = 0; i < file.Length; i++)
            {
                if (SymbolsFrequency.ContainsKey(file[i]))
                    SymbolsFrequency[file[i]] += 1;
                else
                    SymbolsFrequency[file[i]] = 1;
            }
            bitsPerSymbol = Convert.ToInt32(Math.Ceiling(Math.Log(SymbolsFrequency.Count, 2)));
        }
    }
}
