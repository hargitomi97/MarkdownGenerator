﻿using Igloo15.MarkdownApi.Core.Builders;
using Igloo15.MarkdownApi.Core.Interfaces;
using Igloo15.MarkdownApi.Core.MarkdownItems.TypeParts;
using System;
using System.Reflection;
using System.Text;

namespace Igloo15.MarkdownApi.Core.Themes.Default
{
    /// <summary>
    /// Used as a utility class for creating clean links and names for MarkdownItems
    /// </summary>
    public static class Cleaner
    {
        /// <summary>
        /// Create a Link to the target from the MarkadownItem with full type information
        /// </summary>
        /// <param name="currentItem">The current location to create a link from</param>
        /// <param name="target">The target to link to </param>
        /// <param name="useFullName">Use the full name of the type with namespace</param>
        /// <param name="useSpecialText">Use special code quote on link title to make it look nicer</param>
        /// <returns>The markdown string</returns>
        public static string CreateFullTypeWithLinks(IMarkdownItem currentItem, Type target, bool useFullName, bool useSpecialText)
        {
            StringBuilder sb = new StringBuilder();
            var genericArray = target.GetGenericArguments();
            for(var i = 0; i < genericArray.Length; i++)
            {
                var link = CreateFullTypeWithLinks(currentItem, genericArray[i], useFullName, useSpecialText);
                sb.Append(link);

                if (i + 1 != genericArray.Length)
                    sb.Append(", ");
            }

            var actualName = currentItem.GetNameOrNameLink(target, useFullName, useSpecialText);

            if(genericArray.Length > 0)
                return $"{actualName}\\<{sb.ToString()}>";
            return actualName;
        }

        /// <summary>
        /// Create a constructor with links and parameters
        /// </summary>
        /// <param name="currentItem">The current location to create this Constructor name with links</param>
        /// <param name="constructor">The constructor to clean up</param>
        /// <param name="useFullName">Use full name of the constructor</param>
        /// <param name="useParameterNames">Use parameter names if set to false only type will be shown</param>
        /// <returns>The markdown string</returns>
        public static string CreateFullConstructorsWithLinks(IMarkdownItem currentItem, MarkdownConstructor constructor, bool useFullName, bool useParameterNames)
        {
            var parameters = constructor.InternalItem.GetParameters();
            MarkdownBuilder mb = new MarkdownBuilder();

            string name = useFullName ? CleanFullName(constructor.ParentType.InternalType, false, false) : CleanName(constructor.ParentType.Name, false, false);

            if (constructor.FileName != null)
                mb.Link(name, currentItem.To(constructor));
            else
                mb.Append(name);
            mb.Append(" ( ");
            if (parameters.Length > 0)
            {

                StringBuilder sb = new StringBuilder();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var link = CreateFullTypeWithLinks(currentItem, type, useFullName, true);
                    sb.Append(link);

                    if (useParameterNames)
                        sb.Append($" {parameters[i].Name}");

                    if (i + 1 != parameters.Length)
                        sb.Append(", ");
                }
                mb.Append(sb.ToString());
            }
            mb.Append(" )");
            return mb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="method"></param>
        /// <param name="useFullName"></param>
        /// <param name="useParameterNames"></param>
        /// <returns></returns>
        public static string CreateFullMethodWithLinks(IMarkdownItem currentItem, MarkdownMethod method, bool useFullName, bool useParameterNames)
        {
            var parameters = method.InternalItem.GetParameters();
            MarkdownBuilder mb = new MarkdownBuilder();
            if (method.FileName != null)
                mb.Link(method.Name, currentItem.To(method));
            else
                mb.Append(method.Name);
            mb.Append(" ( ");
            if (parameters.Length > 0)
            {
                
                StringBuilder sb = new StringBuilder();
                for(var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var link = CreateFullTypeWithLinks(currentItem, type, useFullName, true);

                    if (link.IndexOf('&') > 0)
                    {
                        link = link.Replace("&", "");
                        sb.Append("out ");
                    }

                    sb.Append(link);

                    if(useParameterNames)
                        sb.Append($" {parameters[i].Name}");

                    
                        

                    if (i + 1 != parameters.Length)
                        sb.Append(", ");
                }
                mb.Append(sb.ToString());
            }
            mb.Append(" )");
            return mb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentItem"></param>
        /// <param name="property"></param>
        /// <param name="useFullName"></param>
        /// <param name="useParameterNames"></param>
        /// <returns></returns>
        public static string CreateFullParameterWithLinks(IMarkdownItem currentItem, MarkdownProperty property, bool useFullName, bool useParameterNames)
        {
            var fullParameterName = property.InternalItem.ToString();
            MarkdownBuilder mb = new MarkdownBuilder();
            if (property.FileName != null)
                mb.Link(property.Name, currentItem.To(property));
            else
                mb.Append(property.Name);

            var parts = fullParameterName.Split('[', ']');

            var index = property.InternalItem.GetPropertyKeyType();

            if (index.Key != null)
            {
                mb.Append(" [ ");

                var link = CreateFullTypeWithLinks(currentItem, index.Key, useFullName, true);

                mb.Append(link);

                if (useParameterNames)
                    mb.Append($" {index.Name}");

                mb.Append(" ]");
            }
            
            return mb.ToString();
        }

        private static (Type Key, String Name) GetPropertyKeyType(this PropertyInfo info)
        {
            var methodInfo = info.GetSetMethod();
            if (methodInfo != null)
            {
                foreach (var param in methodInfo.GetParameters())
                {
                    if (param.ParameterType != info.PropertyType)
                    {
                        return (param.ParameterType, param.Name);
                    }
                        
                }

            }

            return (null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="keepGenericNumber"></param>
        /// <param name="specialText"></param>
        /// <returns></returns>
        public static string CleanFullName(Type t, bool keepGenericNumber, bool specialText)
        {
            if (t == null) return "";
            if (t == typeof(void))
                return "void";


            var name = t.FullName;

            return CleanName(t.FullName, keepGenericNumber, specialText);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="keepGenericNumber"></param>
        /// <param name="specialText"></param>
        /// <returns></returns>
        public static string CleanName(string name, bool keepGenericNumber, bool specialText)
        {
            if (String.IsNullOrEmpty(name))
                return name;

            var indexBracket = name.IndexOf("[");

            if (indexBracket > 0)
                name = name.Substring(0, indexBracket);

            var indexDash = name.IndexOf('`');

            if (indexDash > 0)
            {
                if (keepGenericNumber)
                {
                    name = name.Replace('`', '-');
                }
                else
                {
                    name = name.Substring(0, indexDash);
                }
            }

            if (specialText)
                return $"`{name}`";
            return name;
        }

        

    }
}
