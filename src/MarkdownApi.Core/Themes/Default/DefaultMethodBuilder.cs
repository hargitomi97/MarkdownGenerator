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
    private string nameFirst;
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
      Dictionary<string, string> pairs = new Dictionary<string, string>();
      XmlDocumentComment[] comments = new XmlDocumentComment[0];
      MarkdownBuilder mb = new MarkdownBuilder();
      name = Cleaner.CreateFullParametersInMethods(item, item.As<MarkdownMethod>(), false, true);





      if (File.Exists(@"C:\Users\Tom\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml"))
      {
        comments = ParseXmlComment(XDocument.Parse(File.ReadAllText(@"C:\Users\Tom\Desktop\sigstat\src\SigStat.Common\bin\Debug\net461\SigStat.Common.xml")), "");

        foreach (var k in comments)
        {
          foreach (var i in item.Parameters)
          {
            found = k.Parameters.FirstOrDefault(x => x.Key == i.Name).Value;//.Find(y => y.ToString() == x.Key)).Value;
            if (found != null)
            {
              //Console.WriteLine(i.Name + " " + found);
              if (!pairs.ContainsKey(i.Name))
                pairs.Add(i.Name, found);
            }
          }
        }
      }

      /*foreach(var k in pairs)
      {
        Console.WriteLine(k.Key + " " + k.Value);
      }*/

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
        ParameterComments = pairs.FirstOrDefault(x => x.Key == name.Split('[').FirstOrDefault().Trim()).Value;
        if (ParameterComments != null)
        {
          var ParameterType = name.Substring(name.IndexOf('['));
          mb.Append("**`" + name.Split('[').FirstOrDefault().Trim() + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
        }
      }


      if (numberOfParameters == 2)
      {
        int index2 = name.IndexOf('>');
        ParameterComments = pairs.FirstOrDefault(y => y.Key == name.Split('[').FirstOrDefault().Trim()).Value;
        // Console.WriteLine(ParameterComments); OK

        if (ParameterComments != null)
        {
          var append = name.Split('[').FirstOrDefault().Trim();
          // Console.WriteLine(append); OK
          var ParameterTypeBegin = name.Substring(name.IndexOf('['));
          var ParameterType = ParameterTypeBegin.Substring(0, ParameterTypeBegin.IndexOf("<br>"));
          //Console.WriteLine(ParameterType);
          mb.Append("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments +"<br><br>");
          //Console.WriteLine("**`" + append + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
        }

        if (index2 > 0)
        {
          var findSecond = name.Substring(name.IndexOf("<br>") + 4).Split('[').FirstOrDefault().Trim();
          ParameterComments = pairs.FirstOrDefault(y => y.Key == findSecond).Value;
          if (ParameterComments != null)
          {
            var ParameterTypeBegin = name.Substring(name.IndexOf("<br>") + 4);
            var ParameterType = ParameterTypeBegin.Substring(ParameterTypeBegin.IndexOf("["));
            mb.Append("**`" + findSecond + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
            Console.WriteLine("**`" + findSecond + "`**" + "  " + ParameterType + "<br>" + ParameterComments);
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
        mb.Append(returned);

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
