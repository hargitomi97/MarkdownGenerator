using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace igloo15.MarkdownApi.Core
{
    public class AllClasses
    {
        HashSet<string> hs = new HashSet<string>();
        Dictionary<string, string> myDictionary = new Dictionary<string, string>();

        public AllClasses(HashSet<string> hs, Dictionary<string, string> myDictionary)
        {
            this.hs = hs;
            this.myDictionary = myDictionary;

            
        }


    }
}
