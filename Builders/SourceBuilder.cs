using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasyMapper.Builders
{
    internal sealed class SourceBuilder : IDisposable
    {
        #nullable enable
        #region Fields

        private readonly StringWriter _writer;
        private readonly IndentedTextWriter _indentedWriter;

        #endregion

        public SourceBuilder()
        {
            _writer = new StringWriter();
            _indentedWriter = new IndentedTextWriter(_writer, new string(' ', 4));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _writer.Dispose();
            _indentedWriter.Dispose();
        }
        
        public SourceBuilder Write(string text)
        {
            if (!string.IsNullOrEmpty(text))
                _indentedWriter.Write(text);

            return this;
        }

        public SourceBuilder Write(string format, params object[] args)
        {
            if (!string.IsNullOrEmpty(format))
                _indentedWriter.Write(format, args);

            return this;
        }

        public SourceBuilder WriteIndented(string format, params object[] args)
        {
            _indentedWriter.Indent++;

            Write(format, args);

            _indentedWriter.Indent--;
            return this;
        }

        public SourceBuilder WriteIndented(string format)
        {
            _indentedWriter.Indent++;

            Write(format);

            _indentedWriter.Indent--;
            return this;
        }

        public SourceBuilder WriteLine(string? text = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                _indentedWriter.WriteLineNoTabs(text);
            else
                _indentedWriter.WriteLine(text);

            return this;
        }

        public SourceBuilder WriteIndentedLine(string text)
        {
            _indentedWriter.Indent++;

            WriteLine(text);

            _indentedWriter.Indent--;
            return this;
        }

        public SourceBuilder WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
            return this;
        }

        public SourceBuilder WriteIndentedLine(string format, params object[] args)
        {
            _indentedWriter.Indent++;
            
            WriteLine(format, args);

            _indentedWriter.Indent--;
            return this;
        }

        public SourceBuilder WriteLineIf(bool condition, string text)
        {
            if (condition)
                WriteLine(text);

            return this;
        }

        public SourceBuilder WriteOpeningBracket()
        {
            _indentedWriter.WriteLine("{");
            _indentedWriter.Indent++;

            return this;
        }

        public SourceBuilder WriteClosingBracket()
        {
            _indentedWriter.Indent--;
            _indentedWriter.WriteLine("}");

            return this;
        }

        /// <inheritdoc />
        public override string ToString() => _writer.ToString();
    }
}
