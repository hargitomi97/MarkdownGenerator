using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems;
using igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string found = "";
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
        public string BuildPage(MarkdownMethod item, List<string> names)
        {

            //Console.WriteLine(name);
            //var myList = name.Split('>').ToList();

            /*if (item == null)
            {
                if (File.Exists(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml"))
                {
                    comments = ParseXmlComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");



                    //Console.WriteLine(p.Key + " " + p.Value); // Dictionary: paraméter név - paraméter leírás



                    foreach (var k in comments)
                    {
                        foreach (var p in k.Parameters)
                        {
                            for (int i = 0; i < myList.Count; i++)
                            {
                                //Console.WriteLine(myList[i]);
                                found = k.Parameters.FirstOrDefault(x => x.Key == myList[i]).Value;
                                Console.WriteLine(found);
                            }
                        }

                    }
                }
            }*/

            //var parameters = ParseXmlComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");
            //var found == "";
            /*foreach(var k in parameters)
            {
                Console.WriteLine(k);
            }*/
            /*foreach (var k in parameters)
            {
                Console.WriteLine(k.Name); // paraméter neve
            }*/

            XmlDocumentComment[] comments = new XmlDocumentComment[0];
            MarkdownBuilder mb = new MarkdownBuilder();
            name = Cleaner.CreateFullParametersInMethods(item, item.As<MarkdownMethod>(), false, true);
            
            if (name != "")
            {
                //Console.WriteLine(name);
                /*int index = name.IndexOf('[');
                int index2 = name.IndexOf('>');
                if (index > 0)
                {
                    var x = name.Substring(0, index);
                    names.Add(x);
                }
                if (index2 > 0)
                {
                    var y = name.Substring(name.IndexOf('>') + 1);
                    // Console.WriteLine(y);
                    int index3 = y.IndexOf('[');
                    if (index3 > 0)
                    {
                        //Console.WriteLine(y);
                        var z = y.Substring(0, index3);
                        //Console.WriteLine(z);
                        names.Add(z);

                    }
                }*/
                

                if (File.Exists(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml"))
                {
                    comments = ParseXmlComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tomi\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");



                    foreach (var k in comments)
                    {
                        foreach (var p in k.Parameters)
                        {
                            //Console.WriteLine(k.Parameters);
                            names.Add(p.Key);
                            //found = k.Parameters.First(x => x.Key == names.Find(y => y.ToString() == x.Key)).Value;
                            found = k.Parameters.Where(e => e.Key == names.Find(y => y.ToString() == e.Key))
                                                .Select(e => e.Value).FirstOrDefault();
                            if (found != null)
                            {
                                Console.WriteLine("BRAKE" + found);
                            }
                        }

                    }

                }

                // Console.WriteLine(p.Key + " " + p.Value); // p.Key = a paraméter neve neve, p.Value = paraméter szöveg

                //Console.WriteLine(found);


                //Console.WriteLine(names[0]);
                //found = k.Parameters.FirstOrDefault(x => x.Key == myList[i]).Value;

                var typeZeroHeaders = new[] { "Return", "Name" };


                mb.HeaderWithLink(1, item.FullName, item.To(item));
                mb.AppendLine();


                mb.AppendLine(item.Summary);

                BuildTable(mb, item, typeZeroHeaders, item);

                mb.Append("#### Parameters");
                mb.AppendLine();


                /*for (int i = 0; i < myList.Count; i++)
                {
                    int index = myList[i].IndexOf('[');
                    if (index > 0)
                        myList[i] = myList[i].Substring(0, index); // csak paraméternevek pl. p1
                }*/

                mb.Append(name);

                mb.AppendLine();
                mb.Append("#### Returns");
                mb.AppendLine();

                Type lookUpType = null;
                if (item.ItemType == MarkdownItemTypes.Method)
                    lookUpType = item.As<MarkdownMethod>().ReturnType;

                var returned = Cleaner.CreateFullTypeWithLinks(item, lookUpType, false, false);
                mb.Append(returned);

            }
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
            mb.TableMethod(headers, data);
            mb.AppendLine();
        }

        internal static XmlDocumentComment[] ParseXmlComment(XDocument xDocument, string namespaceMatch)
        {
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
                })
                .Where(x => x != null)
                .ToArray();
        }
    }
}
