﻿using Igloo15.MarkdownApi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Igloo15.MarkdownApi.Core
{
    internal static class MarkdownRepo
    {

        private static Dictionary<string, IMarkdownItem> Items { get; } = new Dictionary<string, IMarkdownItem>();


        public static bool TryAdd(string key, IMarkdownItem item)
        {
            if(!Items.ContainsKey(key))
            {
                Items.Add(key, item);
                return true;
            }

            Console.WriteLine($"Item {key} already exists");
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
                TryAdd(name, item);
            }

            return Items[name] as MarkdownNamespace;
        }

    }
}
