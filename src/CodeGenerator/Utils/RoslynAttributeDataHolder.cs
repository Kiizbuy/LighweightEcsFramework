using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator
{
     internal sealed class RoslynAttributeDataHolder
    {
        //Singleton, Non-cancer, but cancer :)
        internal static readonly Lazy<RoslynAttributeDataHolder> GetOrCreateInstance =
            new Lazy<RoslynAttributeDataHolder>(() => new RoslynAttributeDataHolder()); 

        private readonly Dictionary<string, RoslynAttributeData> _attributeData;
        private readonly string _attributeNamePostfix = "Attribute";
        
        internal RoslynAttributeDataHolder()
        {
            _attributeData = new Dictionary<string, RoslynAttributeData>();
        }
        
        internal RoslynAttributeDataHolder(SeparatedSyntaxList<AttributeSyntax> attributeListSyntax) 
        : this()
        {
            Update(attributeListSyntax);
        }

        
        internal RoslynAttributeDataHolder Update(SeparatedSyntaxList<AttributeSyntax> attributes, 
            bool clearOldData = false)
        {
            if (clearOldData)
            {
                Clear();
            }
            
            foreach (var attribute in attributes)
            {
                _attributeData[attribute.Name.NormalizeWhitespace().ToFullString()] = new RoslynAttributeData(attribute);
            }

            return this;
        }

        public void Clear()
        {
            _attributeData.Clear();
        }

        internal bool TryGet(string attributeName, out RoslynAttributeData value)
        {
            var newAtrName = attributeName.RemovePartOfString(_attributeNamePostfix);
            return _attributeData.TryGetValue(newAtrName, out value);
        }

        internal RoslynAttributeData Get(string attributeName) 
            => _attributeData[attributeName];
    }

    internal class RoslynAttributeData
    {
        private readonly AttributeSyntax _attribute;

        public RoslynAttributeData(AttributeSyntax attribute)
        {
            _attribute = attribute;
        }

        internal bool TryGetFirstArgument(out AttributeArgumentSyntax argumentSyntax)
        {
            argumentSyntax = null;
            return _attribute.ArgumentList != null && TryGetArgumentViaIndex(0, out argumentSyntax);
        }
        
        internal bool TryGetLastArgument(out AttributeArgumentSyntax argumentSyntax)
        {
            argumentSyntax = null;
            return _attribute.ArgumentList != null &&
                   TryGetArgumentViaIndex(_attribute.ArgumentList.Arguments.Count - 1,
                       out argumentSyntax);
        }

        internal bool TryGetArgumentViaIndex(int index, out AttributeArgumentSyntax argumentSyntax)
        {
            argumentSyntax = null;
            
            if (_attribute.ArgumentList == null)
                return false;
            
            argumentSyntax = _attribute.ArgumentList.Arguments[index];
            return argumentSyntax != null;
        }

        internal AttributeArgumentSyntax GetArgumentViaIndex(int index)
        {
            return _attribute.ArgumentList.Arguments[index];
        }

        internal AttributeArgumentSyntax GetFirstArgument()
        {
            return _attribute.ArgumentList.Arguments.First();
        }

        internal AttributeArgumentSyntax GetLastArgument()
        {
            return _attribute.ArgumentList.Arguments.Last();

        }
    }
}