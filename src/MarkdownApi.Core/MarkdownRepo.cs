using igloo15.MarkdownApi.Core.Interfaces;
using igloo15.MarkdownApi.Core.MarkdownItems;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace igloo15.MarkdownApi.Core
{
    internal static class MarkdownRepo
    {

        private static Dictionary<string, IMarkdownItem> Items { get; } = new Dictionary<string, IMarkdownItem>();


        public static bool TryAdd(IMarkdownItem item)
        {
            return TryAdd(item.TypeInfo.GetId(), item);
        }

        private static bool TryAdd(string name, IMarkdownItem item)
        {
            if (!Items.ContainsKey(name))
            {
                Items.Add(name, item);
                return true;
            }

            Console.WriteLine($"Item {name} already exists");
            return false;
        }

        public static Dictionary<string, IMarkdownItem> GetLookup()
        {
            return Items;
        }

        public static MarkdownNamespace TryGetOrAdd(string name, IMarkdownItem item)
        {
            if(!Items.ContainsKey(name))
            {
                Constants.Logger?.LogTrace("Add Markdown Namespace {namespaceName}", name);
                TryAdd(name, item);
            }

            return Items[name] as MarkdownNamespace;
        }

    }
}
