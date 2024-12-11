namespace CodeGenerator.Rewriters
{
    internal static class RewritersProvider
    {
        internal static readonly AttributeRemoveRewriter AttributeRemoveRewriter;
        internal static readonly RemoveCommentsRewriter RemoveCommentsRewriter;
        internal static readonly ListFieldRemoveRewriter ListFieldRemoveRewriter;
        internal static readonly FieldWIthAttributesRemoveRewriter FieldWIthAttributesRemoveRewriter;
        internal static readonly RemoveAllAttributesFromNodeRewriter RemoveAllAttributesFromNodeRewriter;
        
        static RewritersProvider()
        {
            RemoveCommentsRewriter = new RemoveCommentsRewriter();
            ListFieldRemoveRewriter = new ListFieldRemoveRewriter();
            RemoveAllAttributesFromNodeRewriter = new RemoveAllAttributesFromNodeRewriter();
            FieldWIthAttributesRemoveRewriter = new FieldWIthAttributesRemoveRewriter("IgnoreSerialization"); //TODO FIll AttributeName
            AttributeRemoveRewriter = new AttributeRemoveRewriter("IgnoreSerialization", 
                "NonSerialized", 
                "FixedArrayGeneration", 
                "MinMaxValue",
                "FloatMinMaxValue"); //TODO Fill AttributeNames
        }
    }
}