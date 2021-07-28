using System;
using System.Collections.Generic;
using System.Text;

using Mapalyzer.Common;
using Mapalyzer.Data;

namespace Mapalyzer.Sources.Abstracts
{
    abstract class AAttributeSource
    {
        #region Meta

        internal abstract string AttributeName { get; }
        internal virtual string AttributeClassName { get => AttributeName + "Attribute"; }
        internal virtual string FullyQualifiedName {get => Constants.SystemNamespace + "." + AttributeClassName; }

        #endregion

        internal abstract string ProvideSource();

        internal SourceCode Build() => new SourceCode(ProvideSource(), AttributeName + "Attribute.generated.cs");
    }
}