using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static igloo15.MarkdownApi.Core.Builders.VSDocParser;

namespace igloo15.MarkdownApi.Core.Themes.Default
{
    /// <summary>
    /// Default markdown method page builder - Warning this is not yet implemented
    /// </summary>
    public class DefaultMethodBuilder
    {
        private DefaultOptions _options;
        private string foundReturn;


        /// <summary>
        /// Constructs a markdown method page builder
        /// </summary>
        /// <param name="options">The default options to be constructed with</param>
        public DefaultMethodBuilder(DefaultOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Builds the markdown method page content
        /// </summary>
        /// <param name="item">The markdown method item</param>
        /// <returns>The markdown content</returns>
        public string BuildPage(MarkdownMethod item)
        {
            Dictionary<string, string> parameterPairs = new Dictionary<string, string>();
            Dictionary<string, string> returnPairs = new Dictionary<string, string>();
            XmlDocumentComment[] comments = new XmlDocumentComment[0];



            MarkdownBuilder mb = new MarkdownBuilder();
            // method name + param name
            var name = Cleaner.CreateFullMethodWithLinks(item, item.As<MarkdownMethod>(), false, true, true);
            var FullName = item.Name + name;


            if (File.Exists(MarkdownItemBuilder.xmlPath))
            {
                comments = VSDocParser.ParseXmlParameterComment(XDocument.Parse(File.ReadAllText(MarkdownItemBuilder.xmlPath)), "");

                foreach (var comment in comments)
                {
                    foreach (var param in item.Parameters)
                    {

                        var foundParameterComment = comment.Parameters.FirstOrDefault(x => x.Key == param.Name).Value;
                        if (foundParameterComment != null)
                        {
                            foundParameterComment = foundParameterComment.Substring(0, foundParameterComment.LastIndexOf('<'));
                            foundParameterComment = foundParameterComment.Substring(foundParameterComment.IndexOf('>') + 1);

                            var MemberName = Cleaner.CleanName(comment.MemberName, false, false);
                            // method name + param name + parameter summary
                            if (!parameterPairs.ContainsKey(MemberName + " " + param.Name))
                                parameterPairs.Add(MemberName + " " + param.Name, foundParameterComment);
                        }
                    }
                }
            }

            var typeZeroHeaders = new[] { "Return", "Name" };


            mb.HeaderWithLink(1, item.FullName, item.To(item));
            mb.AppendLine();


            mb.AppendLine(item.Summary);

            BuildTable(mb, item, typeZeroHeaders, item);

            mb.Append("#### Parameters");
            mb.AppendLine();
           
            Console.WriteLine(FullName);
            var numberOfParameters = item.Parameters.Length;

            if (numberOfParameters == 1)
            {
                var MethodName = FullName.Substring(0, FullName.IndexOf(" "));
                var ParamName = FullName.Substring(FullName.IndexOf(" "));
                ParamName = ParamName.Substring(0,ParamName.IndexOf("["));
                var methodAndParamName = MethodName + ParamName;
                

                var ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == methodAndParamName).Value;
                if (ParameterComment != null)
                {
                    //var ParameterName = Cleaner.BoldName(FullName.Substring(FullName.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                    ParamName = Cleaner.BoldName(ParamName.Trim());
                    var ParameterType = FullName.Substring(FullName.IndexOf('['));
                    ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                    mb.Append(ParamName + "  " + ParameterType + "<br>" + ParameterComment);
                    //Console.WriteLine(ParameterName + "  " + ParameterType + "<br>" + ParameterComment);
                }
            }


            if (numberOfParameters == 2)
            {
                int index2 = FullName.IndexOf('>');

                var methodAndParamName = FullName.Split('[').FirstOrDefault().Trim();

                var ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == methodAndParamName).Value;

                if (ParameterComment != null)
                {
                    var ParameterName = Cleaner.BoldName(FullName.Substring(FullName.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                    var ParameterTypeBegin = FullName.Substring(FullName.IndexOf('['));
                    var ParameterType = ParameterTypeBegin.Substring(0, ParameterTypeBegin.IndexOf("<br>"));
                    ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                    mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment + "<br><br>");
                }

                if (index2 > 0)
                {
                    var MethodName = FullName.Substring(0, FullName.IndexOf(" "));
                    var findSecond = MethodName + " " + FullName.Substring(FullName.IndexOf("<br>") + 4).Split('[').FirstOrDefault().Trim();
                    ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == findSecond).Value;
                    if (ParameterComment != null)
                    {
                        var ParameterName = Cleaner.BoldName(findSecond.Substring(findSecond.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                        var ParameterTypeBegin = FullName.Substring(FullName.IndexOf("<br>") + 4);
                        var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                        ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                        mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment);
                    }
                }
            }

            if (numberOfParameters == 3)
            {
                int index2 = FullName.IndexOf('>');

                var methodAndParamName = FullName.Split('[').FirstOrDefault().Trim();

                var ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == methodAndParamName).Value;

                if (ParameterComment != null)
                {
                    var ParameterName = Cleaner.BoldName(FullName.Substring(FullName.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                    var ParameterTypeBegin = FullName.Substring(FullName.IndexOf('['));
                    var ParameterType = ParameterTypeBegin.Substring(0, ParameterTypeBegin.IndexOf("<br>"));
                    ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                    mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment + "<br><br>");
                    //Console.WriteLine(ParameterName + "  " + ParameterType + "<br>" + ParameterComment + "<br><br>");
                }

                if (index2 > 0)
                {
                    var MethodName = FullName.Substring(0, FullName.IndexOf(" "));
                    // Console.WriteLine(FullName);
                    var findSecond = MethodName + " " + FullName.Substring(FullName.IndexOf("<br>") + 4).Split('[').FirstOrDefault().Trim();
                    ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == findSecond).Value;
                    if (ParameterComment != null)
                    {
                        var ParameterName = Cleaner.BoldName(findSecond.Substring(findSecond.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                        var ParameterTypeBegin = FullName.Substring(FullName.IndexOf("<br>") + 4);
                        var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                        ParameterType = ParameterType.Substring(0, ParameterType.IndexOf("<br>"));
                        ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                        mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment + "<br><br>");
                    }

                    // ----- idáig OK


                    var ParameterTypeBeginx = FullName.Substring(FullName.IndexOf("<br>") + 4);
                    int index3 = FullName.LastIndexOf(")<br>");
                    int index3alt = FullName.LastIndexOf(")><br>");
                    if (index3 > 0)
                    {
                        MethodName = FullName.Substring(0, FullName.IndexOf(" "));
                        findSecond = MethodName + " " + FullName.Substring(FullName.LastIndexOf(")<br>") + 5).Split('[').FirstOrDefault().Trim();
                        ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == findSecond).Value;
                        if (ParameterComment != null)
                        {
                            var ParameterName = Cleaner.BoldName(findSecond.Substring(findSecond.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                            var ParameterTypeBegin = FullName.Substring(FullName.IndexOf("<br>") + 4);
                            var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                            ParameterType = ParameterType.Substring(0, ParameterType.IndexOf("<br>"));
                            ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                            mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment);
                           
                        }

                    }
                    if (index3alt > 0)
                    {
                        MethodName = FullName.Substring(0, FullName.IndexOf(" "));
                        findSecond = MethodName + " " + FullName.Substring(FullName.LastIndexOf(")><br>") + 6).Split('[').FirstOrDefault().Trim();
                        ParameterComment = parameterPairs.FirstOrDefault(x => x.Key == findSecond).Value;
                        if(ParameterComment != null)
                        {
                            var ParameterName = Cleaner.BoldName(findSecond.Substring(findSecond.IndexOf(" ") + 1).Split('[').FirstOrDefault().Trim());
                            var ParameterTypeBegin = FullName.Substring(FullName.LastIndexOf("<br>") + 5);
                            var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                            
                            ParameterComment = Regex.Replace(ParameterComment, @"<see cref=""\w:([^\""]*)""\s*\/>", m => ResolveSeeElement(m, ""));
                            mb.Append(ParameterName + "  " + ParameterType + "<br>" + ParameterComment);
                            
                        }
                    }
                }
            }
            


            mb.AppendLine();
            mb.Append("#### Returns");
            mb.AppendLine();
            Type lookUpType = null;
            if (item.ItemType == MarkdownItemTypes.Method)
                lookUpType = item.As<MarkdownMethod>().ReturnType;
            var returned = Cleaner.CreateFullTypeWithLinks(item, lookUpType, false, false);

            if (File.Exists(MarkdownItemBuilder.xmlPath))
            {
                comments = VSDocParser.ParseXmlComment(XDocument.Parse(File.ReadAllText(MarkdownItemBuilder.xmlPath)), "");
                if (comments != null)
                {
                    foreach (var k in comments)
                    {
                        k.MemberName = Cleaner.CleanName(k.MemberName, false, false);
                        returnPairs[k.MemberName] = k.Returns;
                    }
                    foundReturn = returnPairs.FirstOrDefault(x => x.Key == item.Name).Value;
                }
            }



            mb.Append(returned);
            mb.AppendLine("<br>");
            mb.Append(foundReturn);

            return mb.ToString();

        }

        private void BuildTable(MarkdownBuilder mb, IMarkdownItem item, string[] headers, MarkdownMethod mdType)
        {
            mb.AppendLine();

            List<string[]> data = new List<string[]>();


            string[] dataValues = new string[headers.Length];

            Type lookUpType = null;
            if (item.ItemType == MarkdownItemTypes.Method)
                lookUpType = item.As<MarkdownMethod>().ReturnType;

            dataValues[0] = Cleaner.CreateFullTypeWithLinks(mdType, lookUpType, false, false);

            string name = item.FullName;
            if (item.ItemType == MarkdownItemTypes.Method)
            {
                name = Cleaner.CreateFullMethodWithLinks(mdType, item.As<MarkdownMethod>(), false, true, false);
            }
            else if (item.ItemType == MarkdownItemTypes.Property)
            {
                name = Cleaner.CreateFullParameterWithLinks(mdType, item.As<MarkdownProperty>(), false, _options.ShowParameterNames);
            }
            else if (item.ItemType == MarkdownItemTypes.Constructor)
            {
                name = Cleaner.CreateFullConstructorsWithLinks(mdType, item.As<MarkdownConstructor>(), false, _options.BuildConstructorPages);
            }


            dataValues[1] = name;

            data.Add(dataValues);
            mb.Table(headers, data, true);
            mb.AppendLine();
        }



    }
}
