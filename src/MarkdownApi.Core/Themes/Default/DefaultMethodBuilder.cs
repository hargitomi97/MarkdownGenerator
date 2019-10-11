using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems;
using igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace igloo15.MarkdownApi.Core.Themes.Default
{
    /// <summary>
    /// Default markdown method page builder - Warning this is not yet implemented
    /// </summary>
    public class DefaultMethodBuilder
    {
        private DefaultOptions _options;

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
            var typeZeroHeaders = new[] {"Return", "Name"};

            MarkdownBuilder mb = new MarkdownBuilder();
            mb.HeaderWithLink(1, item.FullName, item.To(item));
            mb.AppendLine();

            string name = Cleaner.CreateFullMethodWithLinks(item, item.As<MarkdownMethod>(), false, true);
            // mb.AppendLine("| Name | Summary  |");
            // mb.AppendLine("| ------| -----------:|");
            //mb.AppendLine("| " + name + " | "+ item.Summary);
            mb.AppendLine(item.Summary);

            BuildTable(mb, item, typeZeroHeaders, item);

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
    }
}
