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
#if DEBUG
            Console.WriteLine(new string(' ', this.Depth) + this.Token.ToString());
#endif
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
                        return this.ReadLiteralScalar();
                    case Token.Folded:
                        return this.ReadFoldedScalar();
                    case Token.Tag:
                        return this.ReadTaggedNode();
                    case Token.Outdent:
                        return null;
                    case Token.EOS:
                        return null;
                    case Token.IndentSpaces:
                    case Token.Comment:
                    case Token.Newline:
                        this.ReadNextToken();
                        continue;
                    default:
#if DEBUG
                        System.Diagnostics.Debugger.Break();
                        Console.WriteLine("NotImplemented: " + this.Token.ToString());
                        this.ReadNextToken();
                        continue;
#else
                        throw new NotImplementedException("Token not implemented: " + this.Token.ToString());
#endif
                }
            }
        }

        private Node ReadTaggedNode()
        {
            var tag = new Tag(this.TokenText.Substring(1));
            SkipToken(Token.Tag);
            tag.Value = ReadNode();
            return tag;
        }

        private Node ReadLiteralScalar()
        {
            return ReadScalar(Token.Literal, Token.Outdent, ReadBlockContent);
        }

        private Node ReadFoldedScalar()
        {
            return ReadScalar(Token.Folded, Token.Outdent, ReadBlockContent);
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

        private void Read()
        {
            var document = new Document();
            _documents.Add(document);
            while (true)
            {
                switch (this.Token)
                {
                    case Token.DocumentSeperator:
                        SkipToken();
                        if (document.Count == 0)
                            continue;
                        document = new Document();
                        _documents.Add(document);
                        continue;
                    case Token.PauseStream:
                    case Token.IndentSpaces:
                    case Token.Comment:
                    case Token.Newline:
                        SkipToken();
                        continue;
                    case Token.EOS:
                        return;
                    default:
                        document.Add(this.ReadNode());
                        continue;
                }
            }
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
                        throw new SyntaxErrorException(string.Empty, "Unexpected token: " + this.Token.ToString(), this.TokenSpan, string.Empty);
                }
            }
        }

        private string ReadComment()
        {
            return this.TokenText;
        }



        private string ReadFlowContent()
        {
            var literal = "";
            Token lastToken = this.Token;
            while (true)
            {
                switch (this.Token)
                {
                    case Token.Newline:
                        SkipToken();
                        continue;
                    case Token.EmptyLine:
                        SkipToken();
                        if (this.Token == Token.Newline)
                        {
                            literal += this.TokenText;
                            SkipToken();
                        }
                        continue;
                    case Token.Escape:
                    case Token.TextContent:
                        literal += this.TokenText;
                        SkipToken();
                        if (SkipToken(Token.Newline, false))
                        {
                            if (this.Token == Token.TextContent || this.Token == Token.Escape)
                                literal += " ";
                        }
                        continue;
                    default:
                        break;
                }
                break;
            }

            return literal;
        }

        private string ReadBlockContent()
        {
            this.NextRawToken(); //Token.Newline

            var literal = "";
            Token lastToken = this.Token;
            bool dontFoldLine = false;
            while (true)
            {
                switch (this.Token)
                {
                    case Token.IndentSpaces:
                        this.NextRawToken();
                        dontFoldLine = true;
                        continue;
                    case Token.Newline:
                        this.NextRawToken();
                        dontFoldLine = false;
                        continue;
                    case Token.EmptyLine:
                        this.NextRawToken();
                        if (this.Token == Token.Newline)
                        {
                            literal += this.TokenText;
                            this.NextRawToken();
                        }
                        continue;
                    case Token.Escape:
                    case Token.TextContent:
                        literal += this.TokenText;
                        this.NextRawToken();
                        if (this.Token == Token.Newline)
                        {
                            //save the new line incase we are at a situation where we may need to fold.

                            var newLine = this.TokenText;

                            this.NextRawToken();

                            //if we are folding add a space otherwise add the Newline
                            switch (this.Token)
                            {
                                case Token.IndentSpaces:
                                case Token.TextContent:
                                case Token.Escape:
                                    literal += dontFoldLine ? newLine : " ";
                                    break;
                            }
                                
                        }
                        continue;
                    default:
                        break;
                }
                break;
            }

            return literal;
            
        }

        private Scalar ReadSingleQuotedScalar()
        {
            return ReadScalar(Token.SingleQuote, Token.SingleQuote, ReadFlowContent);
        }

        private Scalar ReadDoubleQuotedScalar()
        {
            return ReadScalar(Token.DoubleQuote, Token.DoubleQuote, ReadFlowContent);
        }

        private Scalar ReadPlainScalar()
        {
            return ReadScalar(Token.PlainScalar, Token.PlainEnd, ReadFlowContent);
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
                throw new SyntaxErrorException(string.Empty, token.ToString() + " expected but not found.", this.TokenSpan, string.Empty);

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
                        throw new SyntaxErrorException(string.Empty, "Unexpected token: " + this.Token.ToString(), this.TokenSpan, string.Empty);
                }
            }
        }

        

        #region "static methods"
        public static YamlReader Load(string yaml)
        {
            var reader = new YamlReader();
            reader.SetSource(yaml);
            reader.Read();
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
