using Microsoft.CodeAnalysis.Diagnostics;

namespace Mapalyzer.Core.Analyzers
{
    public interface IAnalyzer
    {
        void RegisterAction(SyntaxNodeAnalysisContext context);
    }
}