using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static igloo15.MarkdownApi.Core.Builders.VSDocParser;

namespace igloo15.MarkdownApi.Core.Themes.Default
{
    /// <summary>
    /// Used as a utility class for creating clean links and names for MarkdownItems
    /// </summary>
    public static class Cleaner
    {
        /// <summary>
        /// Create a Link to the target from the MarkadownItem with full type information
        /// </summary>
        /// <param name="currentItem">The current location to create a link from</param>
        /// <param name="target">The target to link to </param>
        /// <param name="useFullName">Use the full name of the type with namespace</param>
        /// <param name="useSpecialText">Use special code quote on link title to make it look nicer</param>
        /// <returns>The markdown string</returns>
        public static string CreateFullTypeWithLinks(IMarkdownItem currentItem, Type target, bool useFullName, bool useSpecialText)
        {
            StringBuilder sb = new StringBuilder();
            var genericArray = target.GetGenericArguments();
            for(var i = 0; i < genericArray.Length; i++)
            {
                var link = CreateFullTypeWithLinks(currentItem, genericArray[i], useFullName, useSpecialText);
                sb.Append(link);

                if (i + 1 != genericArray.Length)
                    sb.Append(", ");
            }

            var actualName = currentItem.GetNameOrNameLink(target, useFullName, useSpecialText);

            if(genericArray.Length > 0)
                return $"{actualName}\\<{sb.ToString()}>";
            return actualName;
        }

        /// <summary>
        /// Create a constructor with links and parameters
        /// </summary>
        /// <param name="currentItem">The current location to create this Constructor name with links</param>
        /// <param name="constructor">The constructor to clean up</param>
        /// <param name="useFullName">Use full name of the constructor</param>
        /// <param name="useParameterNames">Use parameter names if set to false only type will be shown</param>
        /// <returns>The markdown string</returns>
        public static string CreateFullConstructorsWithLinks(IMarkdownItem currentItem, MarkdownConstructor constructor, bool useFullName, bool useParameterNames)
        {
            var parameters = constructor.InternalItem.GetParameters();
            MarkdownBuilder mb = new MarkdownBuilder();

            string name = useFullName ? CleanFullName(constructor.ParentType.InternalType, false, false) : CleanName(constructor.ParentType.Name, false, false);

            if (constructor.FileName != null)
                mb.Link(name, currentItem.To(constructor));
            else
                mb.Append(name);
            mb.Append(" ( ");
            if (parameters.Length > 0)
            {

                StringBuilder sb = new StringBuilder();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var link = CreateFullTypeWithLinks(currentItem, type, useFullName, true);
                    sb.Append(link);

                    if (useParameterNames)
                        sb.Append($" {parameters[i].Name}");

                    if (i + 1 != parameters.Length)
                        sb.Append(", ");
                }
                mb.Append(sb.ToString());
            }
            mb.Append(" )");
            return mb.ToString();
        }

        /// <summary>
        /// Cleans a method and adds the appropiate links
        /// </summary>
        /// <param name="currentItem">The current markdown item containing the method to be cleaned</param>
        /// <param name="method">The method to be cleaned</param>
        /// <param name="useFullName">Determine if full name of method should be shown</param>
        /// <param name="useParameterNames">Determines if parameter names should be shown</param>
        /// <returns>The cleaned string</returns>
        public static string CreateFullMethodWithLinks(IMarkdownItem currentItem, MarkdownMethod method, bool useFullName, bool useParameterNames)
        {
            var parameters = method.InternalItem.GetParameters();
            MarkdownBuilder mb = new MarkdownBuilder();
            if (method.FileName != null)
                mb.Link(method.Name, currentItem.To(method));
            else
                mb.Append(method.Name);
            mb.Append(" ( ");
            if (parameters.Length > 0)
            {
                
                StringBuilder sb = new StringBuilder();
                for(var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var link = CreateFullTypeWithLinks(currentItem, type, useFullName, true);

                    if (link.IndexOf('&') > 0)
                    {
                        link = link.Replace("&", "");
                        sb.Append("out ");
                    }

                    sb.Append(link);

                    if(useParameterNames)
                        sb.Append($" {parameters[i].Name}");

                    
                        

                    if (i + 1 != parameters.Length)
                        sb.Append(", ");
                }
                mb.Append(sb.ToString());
            }
            mb.Append(" )");
            return mb.ToString();
        }
        public static string CreateFullParametersInMethods(IMarkdownItem currentItem, MarkdownMethod method, bool useFullName, bool useParameterNames)
        {
            var parameters = method.InternalItem.GetParameters();
            //Console.WriteLine(parameters[1]);
            foreach(var k in parameters)
            {
               //Console.WriteLine(k.Name); // paraméter neve
            }

            
            MarkdownBuilder mb = new MarkdownBuilder();
            if (parameters.Length > 0)
            {

                StringBuilder sb = new StringBuilder();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var link = CreateFullTypeWithLinks(currentItem, type, useFullName, true);

                    if (link.IndexOf('&') > 0)
                    {
                        link = link.Replace("&", "");
                        sb.Append("out ");
                    }

                    if (useParameterNames)
                        sb.Append($" {parameters[i].Name}  ");

                    sb.Append(link);

                    if (i + 1 != parameters.Length)
                    {
                        sb.Append("<br>");
                    }
                }
                mb.Append(sb.ToString());
            }
            return mb.ToString();
        }

        internal static XmlDocumentComment[] ParseXmlComment(XDocument xDocument, string namespaceMatch, Dictionary<string, string> myDictionary = null)
        {
            return xDocument.Descendants("member")
                .Select(x =>
                {
                    var match = Regex.Match(x.Attribute("name").Value, @"(.):(.+)\.([^.()]+)?(\(.+\)|$)");
                    if (!match.Groups[1].Success) return null;

                    var memberType = (MemberType)match.Groups[1].Value[0];
                    if (memberType == MemberType.None) return null;

                    var summaryXml = x.Elements("summary").FirstOrDefault()?.ToString()
                        ?? x.Element("summary")?.ToString()
                        ?? "";
                    //Console.WriteLine(summaryXml);
                    summaryXml = Regex.Replace(summaryXml, @"<\/?summary>", string.Empty); // a summary tageket üres sztringgel helyettesítjük
                                                                                           // Console.WriteLine(summaryXml);
                    summaryXml = Regex.Replace(summaryXml, "<para>", "<br>"); // a para tageket egy új sorral helyettesítjük
                    summaryXml = Regex.Replace(summaryXml, "</para>", string.Empty); // a vége parákat meg eltüntetjük
                                                                                     // Console.WriteLine(summaryXml);
                    summaryXml = Regex.Replace(summaryXml, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, namespaceMatch, myDictionary));

                    var parsed = Regex.Replace(summaryXml, @"<(type)*paramref name=""([^\""]*)""\s*\/>", e => $"`{e.Groups[2].Value}`");
                    // Console.WriteLine(parsed);

                    var summary = parsed;

                    if (summary != "")
                    {
                        summary = string.Join("  ", summary.Split(new[] { "\r", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()));
                    }
                    // Console.WriteLine(summary);

                    var returns = ((string)x.Element("returns")) ?? "";

                    var remarks = ((string)x.Element("remarks")) ?? "";

                    var parameters = x.Elements("param")
                        .Select(e => Tuple.Create(e.Attribute("name").Value, e))
                        .Distinct(new Item1EqualityCompaerer<string, XElement>())
                        .ToDictionary(e => e.Item1, e => e.Item2.Value);

                    if (memberType == MemberType.Method && match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[4].Value))
                    {
                        int index = 0;
                        Dictionary<string, string> methodParams = new Dictionary<string, string>();
                        var paramTypes = ParseXmlParameters(match.Groups[4].Value.Replace("(", "").Replace(")", ""));

                        foreach (var paramElem in x.Elements("param"))
                        {
                            string newName = paramElem.Attribute("name").Value;

                            if (index < paramTypes.Length)
                            {
                                newName = newName + ":" + paramTypes[index].Replace("0:", "");
                            }
                            methodParams.Add(newName, paramElem.Value);
                            index++;
                        }
                        parameters = methodParams;
                    }

                    var className = (memberType == MemberType.Type)
                        ? match.Groups[2].Value + "." + match.Groups[3].Value
                        : match.Groups[2].Value;

                    return new XmlDocumentComment
                    {
                        MemberType = memberType,
                        ClassName = className,
                        MemberName = match.Groups[3].Value,
                        Summary = summary.Trim(),
                        Remarks = remarks.Trim(),
                        Parameters = parameters,
                        Returns = returns.Trim()
                    };
                })
                .Where(x => x != null)
                .ToArray();
        }

        /// <summary>
        /// Create a full parameter name with links
        /// </summary>
        /// <param name="currentItem">The current markdown item with the property to be rendered</param>
        /// <param name="property">The markdown property</param>
        /// <param name="useFullName">Determines if the fullName of Markdown property should be used</param>
        /// <param name="useParameterNames">Determines if parameter names should be shown on property</param>
        /// <returns>Returns the full parameter name with links rendered in markdown</returns>
        public static string CreateFullParameterWithLinks(IMarkdownItem currentItem, MarkdownProperty property, bool useFullName, bool useParameterNames)
        {
            var fullParameterName = property.InternalItem.ToString();
            MarkdownBuilder mb = new MarkdownBuilder();
            if (property.FileName != null)
                mb.Link(property.Name, currentItem.To(property));
            else
                mb.Append(property.Name);

            var parts = fullParameterName.Split('[', ']');

            var index = property.InternalItem.GetPropertyKeyType();

            if (index.Key != null)
            {
                mb.Append(" [ ");

                var link = CreateFullTypeWithLinks(currentItem, index.Key, useFullName, true);

                mb.Append(link);

                if (useParameterNames)
                    mb.Append($" {index.Name}");

                mb.Append(" ]");
            }
            
            return mb.ToString();
        }

        private static (Type Key, String Name) GetPropertyKeyType(this PropertyInfo info)
        {
            var methodInfo = info.GetSetMethod();
            if (methodInfo != null)
            {
                foreach (var param in methodInfo.GetParameters())
                {
                    if (param.ParameterType != info.PropertyType)
                    {
                        return (param.ParameterType, param.Name);
                    }
                        
                }

            }

            return (null, null);
        }

        /// <summary>
        /// Cleans a type and returns the full name
        /// </summary>
        /// <param name="t">The type to clean</param>
        /// <param name="keepGenericNumber">Determines if the type's generic number should be kept and shown</param>
        /// <param name="specialText">Returns if clean name should be rendered as special text</param>
        /// <returns>The clean name returned</returns>
        public static string CleanFullName(Type t, bool keepGenericNumber, bool specialText)
        {
            if (t == null) return "";
            if (t == typeof(void))
                return "void";


            var name = t.FullName;

            return CleanName(t.FullName, keepGenericNumber, specialText);            
        }

        /// <summary>
        /// Cleans a name removing bad characters and other items
        /// </summary>
        /// <param name="name">The name to clean</param>
        /// <param name="keepGenericNumber">If a generic number exists this will determine if it should be kept</param>
        /// <param name="specialText">Render cleaned name with special text</param>
        /// <returns>The cleaned name</returns>
        public static string CleanName(string name, bool keepGenericNumber, bool specialText)
        {
            if (String.IsNullOrEmpty(name))
                return name;

            var indexBracket = name.IndexOf("[");

            if (indexBracket > 0)
                name = name.Substring(0, indexBracket);

            var indexDash = name.IndexOf('`');

            if (indexDash > 0)
            {
                if (keepGenericNumber)
                {
                    name = name.Replace('`', '-');
                }
                else
                {
                    name = name.Substring(0, indexDash);
                }
            }

            if (specialText)
                return $"`{name}`";
            return name;
        }

        /// <summary>
        /// Remove generics from name basically the `1 text
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>GenericLess string</returns>
        public static string RemoveGenerics(string name)
        {
            var indexDash = name.IndexOf('`');
            if (indexDash < 0)
                return name;

            var endPartOfString = name.Substring(indexDash, name.Length - indexDash);

            var bracketDash = endPartOfString.IndexOf('[');
            if (bracketDash < 0)
                return name;

            var genericString = name.Substring(indexDash, bracketDash);
            name = name.Replace(genericString, "");

            return RemoveGenerics(name);
        }

    }
}
