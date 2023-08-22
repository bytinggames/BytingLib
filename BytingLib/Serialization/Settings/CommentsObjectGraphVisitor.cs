using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;

namespace BytingLib
{
    class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        private string? lastComment;
        private readonly Dictionary<string, string> comments;

        public CommentsObjectGraphVisitor(Dictionary<string, string> comments, IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
            this.comments = comments;
        }

        public override void VisitMappingStart(IObjectDescriptor mapping, Type keyType, Type valueType, IEmitter context)
        {
            base.VisitMappingStart(mapping, keyType, valueType, context);
        }

        public override void VisitMappingEnd(IObjectDescriptor mapping, IEmitter context)
        {
            WriteComment(context, null);

            base.VisitMappingEnd(mapping, context);
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            comments.TryGetValue(key.Name, out var _comment);

            WriteComment(context, _comment);

            return base.EnterMapping(key, value, context);
        }

        private void WriteComment(IEmitter context, string? comment)
        {
            if (lastComment != null)
            {
                context.Emit(new Comment(lastComment, true, Mark.Empty, Mark.Empty));
            }
            lastComment = comment;
        }
    }
}