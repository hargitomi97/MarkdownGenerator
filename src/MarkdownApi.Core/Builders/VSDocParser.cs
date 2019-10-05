using igloo15.MarkdownApi.Core.Themes.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace igloo15.MarkdownApi.Core.Builders
{
    /// <summary>
    /// Type of Xml Comment
    /// </summary>
    public enum MemberType
    {
        /// <summary>
        /// Xml comment for field
        /// </summary>
        Field = 'F',

        /// <summary>
        /// Xml Comment for Property
        /// </summary>
        Property = 'P',

        /// <summary>
        /// Xml Comment for Type
        /// </summary>
        Type = 'T',

        /// <summary>
        /// Xml comment for Event
        /// </summary>
        Event = 'E',

        /// <summary>
        /// Xml comment for Method
        /// </summary>
        Method = 'M',

        /// <summary>
        /// Xml comment for none
        /// </summary>
        None = 0
    }

    /// <summary>
    /// Xml Comment in Xml Document
    /// </summary>
    public class XmlDocumentComment
    {
        /// <summary>
        /// The type of comment
        /// </summary>
        public MemberType MemberType { get; internal set; }

        /// <summary>
        /// The class name for the comment
        /// </summary>
        public string ClassName { get; internal set; }

        /// <summary>
        /// The Member Name for this comment
        /// </summary>
        public string MemberName { get; internal set; }

        /// <summary>
        /// The Summary comment
        /// </summary>
        public string Summary { get; internal set; }

        /// <summary>
        /// The Remarks of the comment
        /// </summary>
        public string Remarks { get; internal set; }

        /// <summary>
        /// Any parameter summaries of comment
        /// </summary>
        public Dictionary<string, string> Parameters { get; internal set; }

        /// <summary>
        /// The summary of the return
        /// </summary>
        public string Returns { get; internal set; }

        /// <summary>
        /// Converts comment to a single string summary
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MemberType + ":" + ClassName + "." + MemberName;
        }
    }

    internal static class VSDocParser
    {
        public static XmlDocumentComment[] ParseXmlComment(XDocument xDocument, Dictionary<string, string> myDictionary, HashSet<string> hs, Dictionary<string, string> myDictionary2, HashSet<string> hs2)
        {
            return ParseXmlComment(xDocument, null, myDictionary, hs, myDictionary2, hs2);
        }

        // cheap, quick hack parser:)
        internal static XmlDocumentComment[] ParseXmlComment(XDocument xDocument, string namespaceMatch, Dictionary<string, string> myDictionary, HashSet<string> hs, Dictionary<string, string> myDictionary2, HashSet<string> hs2)
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
                    summaryXml = Regex.Replace(summaryXml, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, namespaceMatch, myDictionary, hs, myDictionary2, hs2));

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

        private static int ParseParamType(string value, string[] tokens, int currIndex, List<string> newTypes)
        {
            var paramType = value;
            if (paramType.Contains("{"))
            {
                var index = paramType.IndexOf("{");
                var newType = paramType.Substring(0, index);
                var nextType = paramType.Substring(index + 1, paramType.Length - index - 1);

                List<string> innerTypes = new List<string>();
                currIndex = ParseParamType(nextType, tokens, currIndex, innerTypes);
                if (currIndex < tokens.Length && !nextType.Contains("}"))
                {
                    do
                    {
                        paramType = tokens[currIndex];
                        currIndex = ParseParamType(paramType, tokens, currIndex, innerTypes);
                    }
                    while (!paramType.Contains("}") && currIndex < tokens.Length);
                }

                var innerTypeValue = string.Join(",", innerTypes);

                newTypes.Add($"{newType}{{{innerTypeValue}}}");
            }
            else if (paramType.Contains("}"))
            {
                newTypes.Add(paramType.Replace("}", ""));
                currIndex++;
            }
            else
            {
                newTypes.Add(paramType);
                currIndex++;
            }

            return currIndex;
        }

        private static string ResolveSeeElement(Match m, string ns, Dictionary<string, string> myDictionary, HashSet<string> hs, Dictionary<string, string> myDictionary2, HashSet<string> hs2)
        {
            var returned = "";
            

            Assembly assembly = Assembly.LoadFrom(@"C:/Users/Tomi/Desktop/sigstat/src/SigStat.Common/bin/Debug/net461/SigStat.Common.dll");
            var Classes = "";
            var Fields = "";
            var Properties = "";
            var Interfaces = "";
            var Generic = "";
            var webLink = "";
            var Events = "";
            var Methods = "";
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass)
                {
                    if (!(type.ToString().Contains("+") || type.ToString().Contains(">") || type.ToString().Contains("<")))
                    {
                        Classes = type.ToString();
                        int index = Classes.IndexOf("[");
                        if (index > 0)
                            Classes = Classes.Substring(0, index);
                        //Console.WriteLine(Classes.Substring(Classes.IndexOf('.') + 1).Substring(Classes.IndexOf('.')).Replace('`', '-') + " " + Classes.Replace(".", "/"));
                        myDictionary[Classes.Substring(Classes.IndexOf('.') + 1).Substring(Classes.IndexOf('.')).Replace('`', '-')] = Classes.Replace(".", "/");
                    }
                }
                if (type.IsInterface)
                {
                    var cc = "";
                    Interfaces = type.ToString();
                    cc = type.ToString();
                    if (cc.Contains('.'))
                    {
                        cc = cc.Substring(cc.IndexOf('.') + 1);
                    }
                    if (cc.Contains('.'))
                    {
                        cc = cc.Substring(cc.IndexOf('.') + 1);
                    }
                    myDictionary[cc] = Interfaces.Replace(".", "/");
                }
                if (type.ToString().Contains("`1") && !type.ToString().Contains("<>") && !type.ToString().Contains("+"))
                {
                    Generic = type.ToString();
                    int index = Generic.IndexOf("[");
                    if (index > 0)
                        Generic = Generic.Substring(0, index);

                    myDictionary[Generic] = Generic.Replace(".", "/").Replace('`', '-');
                }
                foreach (FieldInfo field in type.GetFields())
                {
                    if (!field.Name.Contains("<>") && !field.Name.Contains("<>") && !field.Name.Contains("Value") && !field.Name.Contains("+"))
                    {
                        Fields = field.Name;
                        //Console.WriteLine(field.DeclaringType.ToString().Split('.').Last() + "." + Fields + " " + field.DeclaringType.ToString().Replace(".", "/"));
                        myDictionary[field.DeclaringType.ToString().Split('.').Last() + "." + Fields] = field.DeclaringType.ToString().Replace(".", "/");
                    }
                }
                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (!property.Name.Contains("+"))
                    {
                        var xx = "";
                        Properties = property.Name;
                        xx = property.DeclaringType.ToString();
                        xx = xx.Substring(xx.IndexOf('.') + 1);
                        if(xx.Contains('.'))
                        {
                            xx = xx.Substring(xx.IndexOf('.') + 1);
                        }
                        //Console.WriteLine(xx);
                        //Console.WriteLine(xx +  "." + Properties + " " + property.DeclaringType.ToString().Replace(".", "/"));
                        myDictionary[xx + "." + Properties] = property.DeclaringType.ToString().Replace(".", "/");
                    }
                }
                foreach (EventInfo event_ in type.GetEvents())
                {
                    var zz = "";
                    Events = event_.Name;
                    zz = event_.DeclaringType.ToString();
                    zz = zz.Substring(zz.IndexOf('.') + 1);
                    if (zz.Contains('.'))
                    {
                        zz = zz.Substring(zz.IndexOf('.') + 1);
                    }
                    myDictionary[zz + "." + Events] = event_.DeclaringType.ToString().Replace(".", "/");
                }
                foreach (MethodInfo method in type.GetMethods())
                {
                    var yy = "";
                    Methods = method.Name;
                    yy = method.DeclaringType.ToString();
                    yy = yy.Substring(yy.IndexOf('.') + 1);
                    if (yy.Contains('.'))
                    {
                        yy = yy.Substring(yy.IndexOf('.') + 1);
                    }
                    //Console.WriteLine(yy + "." + Methods + " " + method.DeclaringType.ToString().Replace(".", "/"));
                    myDictionary[yy + "." + Methods] = method.DeclaringType.ToString().Replace(".", "/");
                }
            }

            foreach (KeyValuePair<string, string> pair in myDictionary) // osztályok benne
            {
                //Console.WriteLine(pair.Key + "\t" + pair.Value);
            }
            var typeName = m.Groups[1].Value;
            if (typeName.Equals("Microsoft.Extensions.Logging.ILogger"))
            {
                returned = $"[{typeName}]" + "(https://docs.microsoft.com/en-us/dotnet/api/Microsoft.Extensions.Logging.ILogger)";
                return returned;
            }

            if (typeName.Equals("System.Drawing.RectangleF"))
            {
                returned = $"[{typeName}]" + "(https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.RectangleF)";
                return returned;
            }
            if (typeName.Equals("System.Collections.Generic.KeyNotFoundException"))
            {
                returned = $"[{typeName}]" + "(https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.KeyNotFoundException)";
                return returned;
            }
            typeName = typeName.Substring(typeName.IndexOf('.') + 1);
            typeName = typeName.Substring(typeName.IndexOf('.') + 1);
            typeName = typeName.Replace("`", "-");
            //special case for HSCP
            int hereIndex = typeName.IndexOf("(");
            if (hereIndex > 0)
                typeName = typeName.Substring(0, hereIndex);
            //Console.WriteLine(typeName);
            webLink = myDictionary.FirstOrDefault(x => x.Key == typeName).Value;
            //Console.WriteLine(webLink);
            if (webLink != null && webLink.Contains("`"))
            {
                webLink = webLink.Replace('`', '-');
            }
            returned = $"[{typeName}]" + "(https://github.com/hargitomi97/sigstat/blob/master/docs/md/" + webLink + ".md)";
            Console.WriteLine(returned);
            return returned;
        }

        private class Item1EqualityCompaerer<T1, T2> : EqualityComparer<Tuple<T1, T2>>
        {
            public override bool Equals(Tuple<T1, T2> x, Tuple<T1, T2> y)
            {
                return x.Item1.Equals(y.Item1);
            }

            public override int GetHashCode(Tuple<T1, T2> obj)
            {
                return obj.Item1.GetHashCode();
            }
        }

        /// <summary>
        /// Parse Xml Parameters with support from C# Discord and Nox#8248
        /// </summary>
        /// <param name="parameterString"></param>
        /// <returns>an array of parsed parameters</returns>
        private static string[] ParseXmlParameters(string parameterString)
        {
            var newParameterList = new List<string>();
            var nestCount = 0;
            var startIdx = 0;
            for (int i = 0; i < parameterString.Length; i++)
            {
                if (parameterString[i] == ',' && nestCount == 0)
                {
                    newParameterList.Add(parameterString.Substring(startIdx, i - startIdx));
                    startIdx = i + 1;
                }
                else if (i == parameterString.Length - 1)
                {
                    newParameterList.Add(parameterString.Substring(startIdx, parameterString.Length - startIdx));
                }
                else if (parameterString[i] == '[' || parameterString[i] == '{')
                {
                    nestCount++;
                }
                else if (parameterString[i] == ']' || parameterString[i] == '}')
                {
                    nestCount--;
                }
            }

            return newParameterList.ToArray();
        }
    }
}