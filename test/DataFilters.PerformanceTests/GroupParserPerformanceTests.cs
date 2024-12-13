using System.Diagnostics.CodeAnalysis;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DataFilters.Grammar.Parsing;
using Superpower;
using Superpower.Model;

namespace DataFilters.PerformanceTests
{
    [MemoryDiagnoser]
    //[SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    [SuppressMessage("Performance", "CA1822:Marquer les membres comme étant static")]
    public class GroupParserPerformanceTests
    {
        [Params(1, 2, 3, 5)] // Paramètres pour le nombre de parenthèses
        public int ParenthesesCount;

        private string _constructedString;
        private const string FixedText = "12:34:56.789";
        private FilterTokenizer _tokenizer;
        private TokenList<FilterToken> _tokens;

        [GlobalSetup]
        public void Setup()
        {
            _tokenizer = new FilterTokenizer();
            StringBuilder sb = new StringBuilder();
            
            // Ajout des parenthèses ouvrantes
            for (int i = 0; i < ParenthesesCount; i++)
            {
                sb.Append('(');
            }

            // Ajout du texte fixe
            sb.Append(FixedText);

            // Ajout des parenthèses fermantes
            for (int i = 0; i < ParenthesesCount; i++)
            {
                sb.Append(')');
            }

            _constructedString = sb.ToString();
            
            _tokens = _tokenizer.Tokenize(_constructedString);
        }
        
        [Benchmark]
        public void Parse() => FilterTokenParser.Group.Parse(_tokens);
    
    }
}