using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace DocAsCode.EntityModel
{
    public static class TreeIterator
    {
        public static async Task PreorderAsync<T>(T current, T parent, Func<T, IEnumerable<T>> childrenGetter, Func<T, T, Task<bool>> action)
        {
            if (current == null || action == null)
            {
                return;
            }

            if (!await action(current, parent))
            {
                return;
            }

            if (childrenGetter == null)
            {
                return;
            }

            var children = childrenGetter(current);
            if (children != null)
            {
                foreach (var child in children)
                {
                    await PreorderAsync(child, current, childrenGetter, action);
                }
            }
        }
    }

    public static class LinkParser
    {
        const string idSelector = @"(?![0-9])[\w_])+[\w\(\)\.\{\}\[\]\|\*\^~#@!`,_<>:]*";
        public static Regex CommentIdRegex = new Regex(@"^(?<type>N|T|M|P|F|E):(?<id>(" + idSelector + ")$", RegexOptions.Compiled);

        // link from cref is in @T:System.Object- format
        public static Regex LinkFromCrefRegex = new Regex(@"@(?<content>((?<type>N|T|M|P|F|E):" + idSelector + @")\-", RegexOptions.Compiled);

        // self written link should be ended with a whitespace
        public static Regex LinkFromSelfWrittenRegex = new Regex(@"@(?<content>(" + idSelector + @")", RegexOptions.Compiled);

        public static string ResolveToMarkdownLink(Dictionary<string, IndexYamlItemViewModel> dict, string input)
        {
            return LinkParser.ResolveText(dict, input, s =>
                 string.Format("[{0}](#/{1})", s.Name, s.Href), s => string.Format("[{0}](#)", s)
                );
        }

        public static string ResolveText<T>(Dictionary<string, T> dict, string input, Func<T, string> linkGenerator, Func<string, string> failureHandler = null)
        {
            if (dict == null) return input;
            return LinkParser.Resolve(input, s =>
            {
                T item;
                if (dict.TryGetValue(s, out item))
                {
                    if (linkGenerator != null)
                    {
                        return linkGenerator(item);
                    }
                    else
                    {
                        Debug.Assert(linkGenerator == null);
                        return item.ToString();
                    }
                }
                else
                {
                    if (failureHandler != null)
                    {
                        return failureHandler(s);
                    }

                    return null;
                }
            });
        }

        public static string Resolve(string input, Func<string, string> replaceHandler)
        {
            if (string.IsNullOrEmpty(input)) return null;
        
            // 1. Self written @System.Object is also supported
            // 2. Generated from triple slash comments: @T:System.Object_, _ stands for a whitespace
            input = LinkFromCrefRegex.Replace(input, new MatchEvaluator(s => LinkResolver(s, replaceHandler)));

            if (string.IsNullOrEmpty(input)) return null;
            input = LinkFromSelfWrittenRegex.Replace(input, new MatchEvaluator(s => LinkResolver(s, replaceHandler)));
            return input;
        }

        private static string LinkResolver(Match match, Func<string, string> replaceHandler)
        {
            string filePath;
            string id = match.Groups["content"].Value;
            // For a valid commentid, remove the first 2 characters
            if (CommentIdRegex.IsMatch(id))
            {
                id = id.Substring(2);

                string replacement = replaceHandler(id);
                if (!string.IsNullOrEmpty(replacement))
                {
                    return replacement;
                }
            }
            else
            {
                string replacement = replaceHandler(id);
                if (!string.IsNullOrEmpty(replacement))
                {
                    // For manually written link, append a whitespace???
                    return replacement + " ";
                }
            }

            return match.Value;
        }
    }

    public static class TripleSlashCommentParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public static string GetSummary(string xml, bool normalize)
        {
            // Resolve <see cref> to @ syntax
            // Also support <seealso cref>
            string selector = "/member/summary";
            xml = ResolveSeeCref(xml, selector);
            xml = ResolveSeeAlsoCref(xml, selector);

            // Trim each line as a temp workaround
            var summary = GetSingleNode(xml, selector, normalize, (e) => null);
            return summary;
        }

        public static string GetReturns(string xml, bool normalize)
        {
            // Resolve <see cref> to @ syntax
            // Also support <seealso cref>
            string selector = "/member/returns";
            xml = ResolveSeeCref(xml, selector);
            xml = ResolveSeeAlsoCref(xml, selector);

            return GetSingleNode(xml, selector, normalize, (e) => null);
        }

        public static string GetParam(string xml, string param, bool normalize)
        {
            if (string.IsNullOrEmpty(xml)) return null;
            Debug.Assert(!string.IsNullOrEmpty(param));
            if (string.IsNullOrEmpty(param))
            {
                return null;
            }

            // Resolve <see cref> to @ syntax
            // Also support <seealso cref>
            string selector = "/member/param[@name='" + param + "']";
            xml = ResolveSeeCref(xml, selector);
            xml = ResolveSeeAlsoCref(xml, selector);

            return GetSingleNode(xml, selector, normalize, (e) => null);
        }

        /// <summary>
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="nodeSelector"></param>
        /// <returns></returns>
        public static string ResolveSeeAlsoCref(string xml, string nodeSelector)
        {
            // Resolve <see cref> to @ syntax
            return ResolveCrefLink(xml, nodeSelector + "/seealso");
        }

        public static string ResolveSeeCref(string xml, string nodeSelector)
        {
            // Resolve <see cref> to @ syntax
            return ResolveCrefLink(xml, nodeSelector + "/see");
        }

        public static string ResolveCrefLink(string xml, string nodeSelector)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                var nav = doc.CreateNavigator();
                var iter = nav.Select(nodeSelector + "[@cref]");
                List<XPathNavigator> sees = new List<XPathNavigator>();
                foreach (XPathNavigator i in iter)
                {
                    var node = i.SelectSingleNode("@cref");
                    if (node != null)
                    {
                        var currentNode = i.Clone();
                        var value = node.Value;

                        // Current: Always append a -, remove when resolve

                        // TODO: need more discussion on @ syntax
                        // what if <see cref="book">s intentionally want no space in between
                        // Append a space to the link 
                        //i.MoveToNext();
                        //if (i.NodeType == XPathNodeType.Text)
                        //{
                        //    if (!string.IsNullOrWhiteSpace(i.Value.Substring(0, 1)))
                        //    {
                        //        i.ReplaceSelf(" " + i.Value);
                        //    }
                        //}

                        currentNode.InsertAfter("@" + value + "-");

                        sees.Add(currentNode);
                    }
                }

                // on successful deleteself, i would point to its parent
                foreach (XPathNavigator i in sees)
                {
                    i.DeleteSelf();
                }

                xml = doc.InnerXml;
            }
            catch
            {
            }

            return xml;
        }

        public static string GetSingleNode(string xml, string selector, bool normalize, Func<Exception, string> errorHandler)
        {
            try
            {
                using (StringReader reader = new StringReader(xml))
                {
                    XPathDocument doc = new XPathDocument(reader);
                    var nav = doc.CreateNavigator();
                    var node = nav.SelectSingleNode(selector);
                    if (node == null)
                    {
                        throw new ArgumentException(selector + " is not found");
                    }

                    var output = node.Value;
                    if (normalize) output = NormalizeContentFromTripleSlashComment(output);
                    return output;
                }
            }
            catch (Exception e)
            {
                if (errorHandler != null)
                {
                    return errorHandler(e);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// The issue with GetXmlDocumentationXML is that it append /r/n and 4 spaces to the new line,
        /// which is considered as code in Markdown syntax
        /// </summary>
        /// <param name="content">The content from triple slash comment</param>
        /// <returns></returns>
        private static string NormalizeContentFromTripleSlashComment(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;
            StringBuilder builder = new StringBuilder();
            using (StringReader reader = new StringReader(content))
            {
                string line;
                // Trim spaces for each line, thus actually Tab to indent is not supported...while new line is kept
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    builder.AppendLine(line);
                }
            }

            // Trim again, e.g. <summary> always starts a new line and thus a \r\n is the first line
            return builder.ToString().Trim();
        }
    }

    public static class YamlViewModelExtension
    {
        public static bool IsPageLevel(this MemberType type)
        {
            return type == MemberType.Namespace || type == MemberType.Class || type == MemberType.Enum || type == MemberType.Delegate || type == MemberType.Interface || type == MemberType.Struct;
        }

        public static YamlItemViewModel Shrink(this YamlItemViewModel item)
        {
            YamlItemViewModel shrinkedItem = new YamlItemViewModel();
            shrinkedItem.Name = item.Name;
            shrinkedItem.Href = item.Href;
            shrinkedItem.Summary = item.Summary;
            shrinkedItem.Type = item.Type;
            shrinkedItem.YamlPath = item.YamlPath;
            return shrinkedItem;
        }
        public static YamlItemViewModel ShrinkToSimpleToc(this YamlItemViewModel item)
        {

            YamlItemViewModel shrinkedItem = new YamlItemViewModel();
            shrinkedItem.Name = item.Name;
            shrinkedItem.Href = item.Href;
            shrinkedItem.Type = item.Type;
            shrinkedItem.YamlPath = item.YamlPath;
            shrinkedItem.DisplayNames = item.DisplayNames;
            shrinkedItem.Items = null;

            if (item.Items == null)
            {
                return shrinkedItem;
            }

            if (item.Type == MemberType.Toc || item.Type == MemberType.Namespace)
            {
                foreach (var i in item.Items)
                {
                    if (shrinkedItem.Items == null)
                    {
                        shrinkedItem.Items = new List<YamlItemViewModel>();
                    }

                    if (i.IsInvalid) continue;
                    var shrinkedI = i.ShrinkToSimpleToc();
                    shrinkedItem.Items.Add(shrinkedI);
                }

            }

            return shrinkedItem;
        }

        public static YamlItemViewModel ShrinkChildren(this YamlItemViewModel item)
        {
            if (item.Items == null)
            {
                return item;
            }
            YamlItemViewModel shrinkedItem = (YamlItemViewModel)item.Clone();
            shrinkedItem.Items = new List<YamlItemViewModel>();
            foreach(var i in item.Items)
            {
                if (i.IsInvalid) continue;

                if (item.Type == MemberType.Namespace)
                {
                    shrinkedItem.Items.Add(i.Shrink());
                }
                else
                {
                    shrinkedItem.Items.Add(i);
                }
            }

            return shrinkedItem;
        }
    }
}
