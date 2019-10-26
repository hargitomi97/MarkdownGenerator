﻿using System;

namespace igloo15.MarkdownApi.Core.Interfaces
{
    /// <summary>
    /// A Markdown Type Part basically this is a part of a MarkdownType
    /// </summary>
    public interface IMarkdownTypePartValue
    {
        /// <summary>
        /// The type name
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// The Type the part
        /// </summary>
        Type Type { get; }
    }
}
