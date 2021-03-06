﻿using igloo15.MarkdownApi.Core.Builders;
using igloo15.MarkdownApi.Core.MarkdownItems;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;

namespace igloo15.MarkdownApi.Core.Themes.Default
{
    /// <summary>
    /// The markdown namespace page builder
    /// </summary>
    public class DefaultNamespaceBuilder
    {
        private DefaultOptions _options;

        /// <summary>
        /// Constructs a namespace page builder
        /// </summary>
        /// <param name="options">The default options for rendering</param>
        public DefaultNamespaceBuilder(DefaultOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Builds the markdown content for the namespace page
        /// </summary>
        /// <param name="item">The markdown namespace item</param>
        /// <returns>The markdown content</returns>
        public string BuildPage(MarkdownNamespace item)
        {
            DefaultTheme.ThemeLogger?.LogDebug("Building Namespace Page");
            var namespaceBuilder = new MarkdownBuilder();
            namespaceBuilder.HeaderWithLink(1, item.FullName, item.To(item));
            namespaceBuilder.AppendLine();

            var summary = "";
            if (!string.IsNullOrEmpty(item.Summary))
            {
                summary = item.Summary;
            }
            else if(_options.NamespaceSummaries.ContainsKey(item.FullName))
            {
                summary = _options.NamespaceSummaries[item.FullName];
            }

            if (!string.IsNullOrEmpty(summary))
            {
                namespaceBuilder.Header(2, "Summary");
                namespaceBuilder.AppendLine(summary).AppendLine();
            }

            namespaceBuilder.Header(2, "Types").AppendLine();

            foreach (var type in item.Types.OrderBy(x => x.Name))
            {
                var sb = new StringBuilder();
                if (!String.IsNullOrEmpty(type.FileName))
                {
                    namespaceBuilder.List(Cleaner.CreateFullTypeWithLinks(item, type.InternalType, false, true));
                }
                else
                {
                    namespaceBuilder.List(item.Name);
                }

                if (!string.IsNullOrEmpty(type.Summary))
                    namespaceBuilder.Tab().List(type.Summary);
            }

            namespaceBuilder.AppendLine();


            return namespaceBuilder.ToString();
        }
    }
}
