using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YaTools.Yaml;

namespace Yaml
{
    public class YamlReader : YamlScanner
    {
        #region properties
        #endregion
        private Dictionary<string, Node> _anchors = new Dictionary<string, Node>();

        private YamlReader()
        {
        }

        private Token Read()
        {
            while (true)
            {
                
                switch (this.NextRawToken())
                {
                    case Token.DocumentSeperator:
                    case Token.PauseStream:
                        Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString() + "  NOT SUPPORTED");
                        continue;
                    case Token.IndentSpaces:
                    case Token.Comment:
                    case Token.Newline:
                        Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString());
                        continue;
                    default:
                        Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString());
                        break;
                }
                break;
            }
            return this.Token;
        }

        private Node ReadNode()
        {
            while (true)
            {
                switch (this.Read())
                {
                    case Token.Anchor:
                        var anchor = this.TokenText.Substring(1);
                        var node = this.ReadNode();
                        _anchors.Add(anchor, node);
                        return node;
                    case Token.Alias:
                        var alias = this.TokenText.Substring(1);
                        return _anchors[alias];
                    case Token.BlockMappingBegin:
                        return this.ReadBlockMapping();
                    case Token.OpenBrace:
                        return this.ReadFlowMapping();
                    case Token.OpenBracket:
                        return this.ReadFlowSequence();
                    case Token.BlockSeqBegin:
                        return this.ReadBlockSequence();
                    case Token.PlainScalar:
                        return this.ReadScalar(Token.PlainEnd);
                    case Token.DoubleQuote:
                        return this.ReadScalar(Token.DoubleQuote);
                    case Token.EOS:
                        return null;
                    default:
                        break;
                }
                break;
            }
            return null;
        }

        private Node ReadFlowMapping()
        {
            var map = new Mapping();

            Token token;
            while ((token = this.Read()) == Token.SimpleKey || token == Token.KeyIndicator)
            {
                var key = ReadNode();
                if (this.Read() != Token.ValueIndicator)
                    throw new InvalidDataException();
                var value = ReadNode();
                map.Add(key, value);
                if (this.Read() != Token.Comma)
                    break;
            }

            return map;
        }

        public string ReadComment()
        {
            return this.TokenText;
        }

        private string ReadTextContent()
        {
            this.Read();
            var text = this.TokenText;
            return text;
        }

        public Scalar ReadScalar(Token tail)
        {
            
            var scalar = new Scalar(ReadTextContent());
            if (this.Read() != tail)
                throw new InvalidDataException();

            return scalar;
            
        }

        public Sequence ReadBlockSequence()
        {
            var seq = new Sequence();

            while (this.Read() == Token.BlockSeqIndicator)
                seq.Add(this.ReadNode());
            

            return seq;
        }

        public Sequence ReadFlowSequence()
        {
            var seq = new Sequence();

            seq.Add(this.ReadNode());

            while (this.Read() == Token.Comma)
                seq.Add(this.ReadNode());

            if (this.Token != Token.CloseBracket)
                throw new InvalidDataException();

            return seq;
        }

        public Mapping ReadBlockMapping()
        {
            var map = new Mapping();

            Token token;
            while ((token = this.Read()) == Token.SimpleKey || token == Token.KeyIndicator)
            {
                var key = ReadNode();
                if (this.Read() != Token.ValueIndicator)
                    throw new InvalidDataException();
                var value = ReadNode();
                map.Add(key, value);
            }

            return map;
        }

        public void TestRead()
        {
            while (true)
            {
                if (this.Token == Token.EOS) break;

                this.ReadNode();
            }
            //Token token;
            //while ()
            //{
            //    Console.WriteLine(new string(' ', this.Depth) + CurrentType);
            //    //switch (CurrentType)
            //    //{
            //    //    case Token.PlainScalar:
            //    //        this.ReadScalar();
            //    //        break;
            //    //    case Token.Alias:
            //    //        break;
            //    //    case Token.Anchor:
            //    //        break;
            //    //    case Token.At:
            //    //        break;
            //    //    case Token.Backtick:
            //    //        break;
            //    //    case Token.BlockMappingBegin:
            //    //        this.ReadMap();
            //    //        break;
            //    //    case Token.BlockSeqBegin:
            //    //        this.ReadSequence();
            //    //        break;
            //    //    case Token.BlockSeqIndicator:
            //    //        this.ReadSequence();
            //    //        break;
            //    //    case Token.CloseBrace:
            //    //        break;
            //    //    case Token.CloseBracket:
            //    //        break;
            //    //    case Token.Comma:
            //    //        break;
            //    //    case Token.Directive:
            //    //        break;
            //    //    case Token.DocumentSeperator:
            //    //        break;
            //    //    case Token.EmptyLine:
            //    //        break;
            //    //    case Token.Escape:
            //    //        break;
            //    //    case Token.EscapedLineBreak:
            //    //        break;
            //    //    case Token.InconsistantIndent:
            //    //        break;
            //    //    case Token.KeyIndicator:
            //    //        break;
            //    //    case Token.OpenBrace:
            //    //        break;
            //    //    case Token.OpenBracket:
            //    //        break;
            //    //    case Token.Outdent:
            //    //        break;
            //    //    case Token.PauseStream:
            //    //        break;
            //    //    case Token.SeperationSpace:
            //    //        break;
            //    //    case Token.SimpleKey:
            //    //        break;
            //    //    case Token.SpecificEndOfLine:
            //    //        break;
            //    //    case Token.Tag:
            //    //        break;
            //    //    case Token.TextContent:
            //    //        break;
            //    //    case Token.Unexpected:
            //    //        break;
            //    //    case Token.ValueIndicator:
            //    //        break;
            //    //    default:
            //    //        break;
            //    //}
            //}
        }

        #region "static methods"
        public static YamlReader OpenText(string path)
        {
            YamlReader reader = new YamlReader();
            using (StreamReader streamReader = File.OpenText(path))
            {
                reader.SetSource(streamReader.ReadToEnd());
            }
            return reader;
        }
        #endregion
    }
}
