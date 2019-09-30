using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using igloo15.MarkdownApi.Core;
using igloo15.MarkdownApi.Core.Themes;
using igloo15.MarkdownApi.Core.Themes.Default;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace igloo15.MarkdownApi.Tester
{
    internal static class Program
    {
 
        private static void Main(string[] args)
        {
            var test = "igloo15.MarkdownApi.Tests.MarkdownTestGenericClass{igloo15.MarkdownApi.Tests.MarkdownTestGenericClass{System.String[0:,0:],System.Collections.Generic.List{System.String[0:,0:]},System.String},System.String,System.String[0:,0:]},igloo15.MarkdownApi.Tests.MarkdownTestGenericClass{``0,System.String,System.String[0:,0:]}";
            var shiz = new System.Collections.Generic.List<string>();
            var nestCount = 0;
            var startIdx = 0;
            /*for (int i = 0; i < test.Length; i++)
            {
                if (test[i] == ',' && nestCount == 0)
                {
                    shiz.Add(test.Substring(startIdx, i - startIdx));
                    startIdx = i + 1;
                }
                else if (i == test.Length - 1)
                {
                    shiz.Add(test.Substring(startIdx, test.Length - startIdx));
                }
                else if (test[i] == '[' || test[i] == '{')
                {
                    nestCount++;
                }
                else if (test[i] == ']' || test[i] == '}')
                {
                    nestCount--;
                }
            }*/

            var factory = new LoggerFactory();
            var myDictionary = new Dictionary<string, string>();
            var myDictionary2 = new Dictionary<string, string>();
            var hs = new HashSet<string>();
            var hs2 = new HashSet<string>();

            factory.AddConsole();

            //var project = MarkdownApiGenerator.GenerateProject(@"D:\Development\Projects\Nuget.Searcher\dist\NuGetSearcher\Release\netstandard2.0\publish\*.dll", "", "Api");
            var project = MarkdownApiGenerator.GenerateProject("../../../**/SigStat.Common.dll", myDictionary, hs, myDictionary2, hs2, "", factory);
            //var project = MarkdownApiGenerator.GenerateProject("../../../**/MarkDownTestProject.dll", "", factory);
            //var project = MarkdownApiGenerator.GenerateProject("../../../../../Nuget.Searcher/dist/NuGetSearcher/Release/netstandard2.0/publish/igloo15.NuGetSearcher.dll", factory);

            project.Build(new DefaultTheme(new DefaultOptions
            {
                BuildNamespacePages = true,
                BuildTypePages = true,
                RootFileName = "README.md",
                RootTitle = "API",
                RootSummary = "The Root Page Summary",
                ShowParameterNames = true,
            }
                ),
                @"..\..\..\..\..\sigstat\docs\md"
            );

            AppDomain.CurrentDomain.ProcessExit += (e, s) =>
            {
                factory.Dispose();
            };

            Console.ReadLine();
        }

    }
}