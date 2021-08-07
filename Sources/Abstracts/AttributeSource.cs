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

        protected abstract string AttributeName { get; }
        protected virtual string AttributeClassName => AttributeName + "Attribute";
        internal virtual string FullyQualifiedName => Constants.SystemNamespace + "." + AttributeClassName;

        #endregion

        protected abstract string ProvideSource();

        internal SourceCode Build() => new SourceCode(ProvideSource(), AttributeName + "Attribute.generated.cs");
    }
}