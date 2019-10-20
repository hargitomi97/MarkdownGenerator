using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems;
using igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private string name = "";
        private string foundParameter = "";
        private string foundReturn = "";
        private string ParameterComments;


        /// <summary>
        /// Constructs a markdown method page builder  - Warning this is not yet implemented
        /// </summary>
        /// <param name="options">The default options to be constructed with</param>
        public DefaultMethodBuilder(DefaultOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Builds the markdown method page content - Warning this is not yet implemented
        /// </summary>
        /// <param name="item">The markdown method item</param>
        /// <returns>The markdown content</returns>
        public string BuildPage(MarkdownMethod item)
        {
            Dictionary<string, string> Parameterpairs = new Dictionary<string, string>();
            Dictionary<string, string> Returnpairs = new Dictionary<string, string>();
            XmlDocumentComment[] comments = new XmlDocumentComment[0];
            MarkdownBuilder mb = new MarkdownBuilder();
            name = Cleaner.CreateFullParametersInMethods(item, item.As<MarkdownMethod>(), false, true);





            if (File.Exists(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml"))
            {
                comments = ParseXmlParameterComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");

                foreach (var k in comments)
                {
                    foreach (var i in item.Parameters)
                    {

                        foundParameter = k.Parameters.FirstOrDefault(x => x.Key == i.Name).Value;//.Find(y => y.ToString() == x.Key)).Value;
                        if (foundParameter != null)
                        {

                            //Console.WriteLine(i + " " + found);
                            if (!Parameterpairs.ContainsKey(i.Name))
                                Parameterpairs.Add(i.Name, foundParameter);
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

            var numberOfParameters = item.Parameters.Length;
            if (numberOfParameters == 1)
            {
                //Console.WriteLine(name.Split('[').FirstOrDefault());
                ParameterComments = Parameterpairs.FirstOrDefault(x => x.Key == name.Split('[').FirstOrDefault().Trim()).Value;
                if (ParameterComments != null)
                {
                    var ParameterType = name.Substring(name.IndexOf('['));
                    mb.Append("**`" + name.Split('[').FirstOrDefault().Trim() + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
                }
            }


            if (numberOfParameters == 2)
            {
                int index2 = name.IndexOf('>');
                ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == name.Split('[').FirstOrDefault().Trim()).Value;
                // Console.WriteLine(ParameterComments); OK

                if (ParameterComments != null)
                {
                    var append = name.Split('[').FirstOrDefault().Trim();
                    // Console.WriteLine(append); OK
                    var ParameterTypeBegin = name.Substring(name.IndexOf('['));
                    var ParameterType = ParameterTypeBegin.Substring(0, ParameterTypeBegin.IndexOf("<br>"));
                    //Console.WriteLine(ParameterType);
                    mb.Append("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments + "<br><br>");
                    //Console.WriteLine("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
                }

                if (index2 > 0)
                {
                    var findSecond = name.Substring(name.IndexOf("<br>") + 4).Split('[').FirstOrDefault().Trim();
                    ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == findSecond).Value;
                    if (ParameterComments != null)
                    {
                        var ParameterTypeBegin = name.Substring(name.IndexOf("<br>") + 4);
                        var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                        mb.Append("**`" + findSecond + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
                        //Console.WriteLine("**`" + findSecond + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
                    }
                }
            }

            if (numberOfParameters == 3)
            {
                int index2 = name.IndexOf('>');
                //Console.WriteLine(name);
                ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == name.Split('[').FirstOrDefault().Trim()).Value;

                if (ParameterComments != null)
                {
                    var append = name.Split('[').FirstOrDefault().Trim();
                    var ParameterTypeBegin = name.Substring(name.IndexOf('['));
                    var ParameterType = ParameterTypeBegin.Substring(0, ParameterTypeBegin.IndexOf("<br>"));

                    mb.Append("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments + "<br><br>");
                    //Console.WriteLine("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments+ "<br><br>");
                }

                if (index2 > 0)
                {
                    var findSecond = name.Substring(name.IndexOf("<br>") + 4).Split('[').FirstOrDefault().Trim();
                    //Console.WriteLine(findSecond);
                    ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == findSecond).Value;
                    if (ParameterComments != null)
                    {
                        var ParameterTypeBegin = name.Substring(name.IndexOf("<br>") + 4);
                        var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
                        var final = ParameterType.Substring(0, ParameterType.IndexOf(")") + 1);
                        mb.Append("**`" + findSecond + "`**" + "  " + final + "<br>" + ParameterComments + "<br><br>");
                        //Console.WriteLine(ParameterTypeBegin);
                        //Console.WriteLine("**`" + findSecond + "`**" + "  " + final + "<br>" + ParameterComments + "<br><br>");
                        int index3 = ParameterTypeBegin.IndexOf(")<br>");
                        int index3alt = ParameterTypeBegin.IndexOf(")><br>");
                        if (index3 > 0)
                        {
                            var test = ParameterTypeBegin.Substring(index3 + 5);
                            var append = test.Split('[').FirstOrDefault().Trim();
                            int index4 = test.Trim().IndexOf('[');
                            if (index4 > 0)
                            {
                                ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == append).Value;
                                var ParameterTypex = test.Trim().Substring(index4);
                                mb.Append("**`" + append + "`**" + "  " + ParameterTypex + "<br>" + ParameterComments);

                            }

                        }
                        if (index3alt > 0)
                        {
                            var test = ParameterTypeBegin.Substring(index3alt + 6).Trim();
                            var append = test.Split('[').FirstOrDefault().Trim();
                            int index4 = test.Trim().IndexOf('[');
                            if (index4 > 0)
                            {
                                ParameterComments = Parameterpairs.FirstOrDefault(y => y.Key == append).Value;

                                var ParameterTypex = test.Trim().Substring(index4);
                                mb.Append("**`" + append + "`**" + "  " + ParameterTypex + "<br>" + ParameterComments);
                            }
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

            if (File.Exists(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml"))
            {
                comments = VSDocParser.ParseXmlReturnComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");
                if (comments != null)
                {
                    foreach (var k in comments)
                    {
                        k.MemberName = Cleaner.CleanName(k.MemberName, false, false);
                        //Console.WriteLine(k.MemberName + " " + k.Returns);
                        //Console.WriteLine(k.MemberName);

                        //Console.WriteLine(item.Name);
                        //Console.WriteLine(k.Returns);
                        //Console.WriteLine(k.MemberName + " " + k.Returns);
                        Returnpairs[k.MemberName] = k.Returns;
                    }
                    foundReturn = Returnpairs.FirstOrDefault(x => x.Key == item.Name).Value;
                    //Console.WriteLine(item.Name + " " + foundReturn);
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

            dataValues[0] = "<sub>" + Cleaner.CreateFullTypeWithLinks(mdType, lookUpType, false, false) + "</sub>";

            string name = item.FullName;
            if (item.ItemType == MarkdownItemTypes.Method)
            {
                name = Cleaner.CreateFullMethodWithLinks(mdType, item.As<MarkdownMethod>(), false, true);
            }
            else if (item.ItemType == MarkdownItemTypes.Property)
            {
                name = Cleaner.CreateFullParameterWithLinks(mdType, item.As<MarkdownProperty>(), false, _options.ShowParameterNames);
            }
            else if (item.ItemType == MarkdownItemTypes.Constructor)
            {
                name = Cleaner.CreateFullConstructorsWithLinks(mdType, item.As<MarkdownConstructor>(), false, _options.BuildConstructorPages);
            }


            dataValues[1] = "<sub>" + name + "</sub>";

            data.Add(dataValues);
            mb.Table(headers, data, false);
            mb.AppendLine();
        }

        internal static XmlDocumentComment[] ParseXmlParameterComment(XDocument xDocument, string namespaceMatch)
        {
            //Console.WriteLine(xDocument.Descendants("member").Select(x=> x.Attribute("name").Value));//.Foreach(x => Console.WriteLine(x)));


            /*var parameterTypes = xDocument.Descendants("member").Select(e => Tuple.Create(e.Attribute("name").Value.Split(')').FirstOrDefault().Substring(e.Attribute("name").Value.IndexOf('(')), e))
                  .Distinct(new Item1EqualityCompaerer<string, XElement>())
                  .ToDictionary(e => e.Item1, e => e.Item2);


            foreach (var k in parameterTypes)
            {
                Console.WriteLine(k.Key);

            }*/

            return xDocument.Descendants("member")
                .Select(x =>
                {
                    var parameters = x.Elements("param")
                          .Select(e => Tuple.Create(e.Attribute("name").Value, e))
                          .Distinct(new Item1EqualityCompaerer<string, XElement>())
                          .ToDictionary(e => e.Item1, e => e.Item2.Value);

                    return new XmlDocumentComment
                    {
                        Parameters = parameters,
                    };
                }).Where(x => x != null)
            .ToArray();
        }

    }
}
