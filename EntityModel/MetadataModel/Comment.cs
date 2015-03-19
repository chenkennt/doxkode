using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityModel
{
    public class TripleSlashComment : CommentBase
    {
    }

    public class MarkdownComment : CommentBase
    {
    }

    public class PlainTextComment : CommentBase
    {
    }

    public class CommentBase : IComment
    {
        public string Raw { get; set; }
        public int StartLine { get; set; }
    }

    public interface IComment
    {
        string Raw { get; set; }
        int StartLine { get; set; }
    }

    public enum CommentType
    {
        TripleSlash,
        Markdown,
        PlainText
    }
}
