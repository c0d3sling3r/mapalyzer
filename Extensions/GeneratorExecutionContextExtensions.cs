using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

using Mapalyzer.Data;

namespace Mapalyzer
{
    internal static class GeneratorExecutionContextExtensions
    {
        internal static void AddSource(this GeneratorExecutionContext context, SourceCode sourceCode)
        {
            context.AddSource(sourceCode.Filename, sourceCode.Body);
        }
    }
}
