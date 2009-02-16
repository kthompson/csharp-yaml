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
        private List<Document> _documents = new List<Document>();

        public Document[] Documents
        {
            get
            {
                return _documents.ToArray();
            }
        }

        private YamlReader()
        {
        }

        private Token ReadNextToken()
        {
            Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString());
            this.NextRawToken();
            return this.SkipIgnorable();
        }

        private Node ReadNode()
        {
            while (true)
            {
                switch (this.Token)
                {
                    case Token.Anchor:
                        return ReadAnchor();
                    case Token.Alias:
                        return ReadAlias();
                    case Token.BlockMappingBegin:
                        return this.ReadBlockMapping();
                    case Token.DocumentSeperator:
                        return this.ReadDocument();
                    case Token.OpenBrace:
                        return this.ReadFlowMapping();
                    case Token.OpenBracket:
                        return this.ReadFlowSequence();
                    case Token.BlockSeqBegin:
                        return this.ReadBlockSequence();
                    case Token.PlainScalar:
                        return this.ReadPlainScalar();
                    case Token.DoubleQuote:
                        return this.ReadDoubleQuotedScalar();
                    case Token.SingleQuote:
                        return this.ReadSingleQuotedScalar();
                    case Token.Literal:
                        return this.ReadBlockScalar();
                    case Token.Folded:
                        return this.ReadFoldedScalar();
                    case Token.Tag:
                        return this.ReadTaggedNode();
                    case Token.EOS:
                        return null;
                    case Token.IndentSpaces:
                    case Token.Comment:
                    case Token.Newline:
                        this.ReadNextToken();
                        continue;
                    default:
                        Console.WriteLine("NotImplemented: " + this.Token.ToString());
                        this.ReadNextToken();
                        continue;
                }
            }
        }

        private Node ReadTaggedNode()
        {
            var tag = this.TokenText.Substring(1);
            SkipToken(Token.Tag);
            var node = ReadNode();
            node.Tag = tag;
            return node;
        }

        private Node ReadBlockScalar()
        {
            return ReadScalar(Token.Literal, Token.Outdent, ReadFoldedContent);
        }

        private Node ReadFoldedScalar()
        {
            return ReadScalar(Token.Folded, Token.Outdent, ReadFoldedContent);
        }

        private Node ReadAlias()
        {
            var alias = this.TokenText.Substring(1);
            SkipToken(Token.Alias);
            return _anchors[alias];
        }

        private Node ReadAnchor()
        {
            var anchor = this.TokenText.Substring(1);
            SkipToken(Token.Anchor);
            var node = this.ReadNode();
            _anchors.Add(anchor, node);
            return node;
        }

        private Document ReadDocument()
        {
            
            var document = new Document();
            SkipToken(Token.Newline, false);
            SkipToken(Token.DocumentSeperator, false);

            while (this.Token != Token.PauseStream && this.Token != Token.EOS)
                document.Add(this.ReadNode());

            _documents.Add(document);
            return document;
        }

        private Mapping ReadFlowMapping()
        {
            var map = new Mapping();

            SkipToken(Token.OpenBrace);

            Node key = null;

            while (true)
            {
                switch (this.Token)
                {
                    case Token.Newline:
                        SkipToken();
                        continue;
                    case Token.Comma:
                        SkipToken();
                        key = null;
                        continue;
                    case Token.SimpleKey:
                    case Token.KeyIndicator:
                        SkipToken();
                        key = ReadNode();
                        continue;
                    case Token.ValueIndicator:
                        SkipToken();
                        map.Add(key, this.ReadNode());
                        continue;
                    case Token.CloseBrace:
                        SkipToken();
                        return map;
                    default:
                        throw new InvalidDataException("Unexpected token: " + this.Token.ToString());
                }
            }
        }

        private string ReadComment()
        {
            return this.TokenText;
        }



        private string ReadBlockContent()
        {
            var literal = "";
            Token lastToken = this.Token;
            while (true)
            {
                switch (this.Token)
                {
                    case Token.EmptyLine:
                        SkipToken();
                        continue;
                    case Token.Escape:
                    case Token.TextContent:
                    case Token.Newline:
                        literal += this.TokenText;
                        SkipToken();
                        continue;
                    default:
                        break;
                }
                break;
            }

            return literal;
        }

        private string ReadFoldedContent()
        {
            SkipToken(Token.Newline);
            return ReadBlockContent();
        }

        private Scalar ReadSingleQuotedScalar()
        {
            return ReadScalar(Token.SingleQuote, Token.SingleQuote, ReadBlockContent);
        }

        private Scalar ReadDoubleQuotedScalar()
        {
            return ReadScalar(Token.DoubleQuote, Token.DoubleQuote, ReadBlockContent);
        }

        private Scalar ReadPlainScalar()
        {
            return ReadScalar(Token.PlainScalar, Token.PlainEnd, ReadBlockContent);
        }

        private Scalar ReadScalar(Token start, Token end, Func<string> content)
        {
            SkipToken(start);

            var scalar = new Scalar(content.Invoke());

            SkipToken(end);
            return scalar;
            
        }

        private Sequence ReadBlockSequence()
        {
            SkipToken(Token.BlockSeqBegin);
            var seq = new Sequence();

            while (SkipToken(Token.BlockSeqIndicator, false))
            {
                seq.Add(this.ReadNode());
                SkipToken(Token.Newline, false);
            }

            if (!SkipToken(Token.Outdent, false))
                SkipToken(Token.Newline);

            return seq;
        }

        private Token SkipIgnorable()
        {
            while (true)
            {
                switch (this.Token)
                {
                    case Token.IndentSpaces:
                    case Token.Comment:
                    //case Token.Newline:
                        Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString());
                        this.NextRawToken();
                        continue;
                    default:
                        break; ;
                }
                break;
            }
            return this.Token;
        }

        private void SkipToken()
        {
            this.ReadNextToken();
        }

        private bool SkipToken(Token token)
        {
            return SkipToken(token, true);
        }

        private bool SkipToken(Token token, bool throwOnInvalidToken)
        {
            SkipIgnorable();
            if (this.Token == token)
            {
                this.ReadNextToken();
                return true;
            }

            if (throwOnInvalidToken)
                throw new InvalidDataException(token.ToString() + " expected but not found.");

            return false;
        }

        private Sequence ReadFlowSequence()
        {
            var seq = new Sequence();
            SkipToken(Token.OpenBracket);

            seq.Add(this.ReadNode());

            while (SkipToken(Token.Comma, false))
                seq.Add(this.ReadNode());

            SkipToken(Token.CloseBracket);
            SkipToken(Token.Newline, false);

            return seq;
        }

        private Mapping ReadBlockMapping()
        {
            var map = new Mapping();
            SkipToken(Token.BlockMappingBegin);

            Node key = null;

            while (true)
            {
                switch (this.Token)
                {
                    case Token.Newline:
                        SkipToken();
                        continue;
                    case Token.SimpleKey:
                    case Token.KeyIndicator:
                        SkipToken();
                        key = ReadNode();
                        continue;
                    case Token.ValueIndicator:
                        SkipToken();
                        map.Add(key, this.ReadNode());
                        continue;
                    case Token.Outdent:
                        SkipToken();
                        return map;
                    default:
                        throw new InvalidDataException("Unexpected token: " + this.Token.ToString());
                }
            }
        }

        public void TestRead()
        {
            while (true)
            {
                if (this.Token == Token.EOS) break;

                this.ReadNode();
            }
        }

        #region "static methods"
        public static YamlReader Load(string yaml)
        {
            var reader = new YamlReader();
            reader.SetSource(yaml);
            reader.ReadDocument();
            return reader;
        }

        public static YamlReader LoadFile(string path)
        {
            using (var streamReader = File.OpenText(path))
            {
                return YamlReader.Load(streamReader.ReadToEnd());
            }
        }
        #endregion
    }
}
