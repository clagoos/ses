﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ses.MsSql {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Scripts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Scripts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Ses.MsSql.Scripts", typeof(Scripts).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [Streams] (
        ///    [StreamId] uniqueidentifier NOT NULL,
        ///    [Version] int NOT NULL,
        ///    [CommitId] uniqueidentifier NOT NULL,
        ///    [ContractName] nvarchar(225) NOT NULL,
        ///    [CreatedAtUtc] datetime NOT NULL,
        ///    [Payload] varbinary(max) NOT NULL,
        ///    [EventId] BIGINT NULL
        ///    CONSTRAINT [PK_Streams] PRIMARY KEY CLUSTERED ([StreamId],[Version],[CommitId])
        ///);
        ///GO
        ///
        ///CREATE NONCLUSTERED INDEX IX_Streams_EventId ON [dbo].[Streams] ([EventId])
        ///INCLUDE ([StreamId],[Version],[CommitId],[Contrac [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Destroy {
            get {
                return ResourceManager.GetString("Destroy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [Streams] (
        ///    [StreamId] uniqueidentifier NOT NULL,
        ///    [Version] int NOT NULL,
        ///    [CommitId] uniqueidentifier NOT NULL,
        ///    [ContractName] nvarchar(225) NOT NULL,
        ///    [CreatedAtUtc] datetime NOT NULL,
        ///    [Payload] varbinary(max) NOT NULL,
        ///    [EventId] BIGINT NULL
        ///    CONSTRAINT [PK_Streams] PRIMARY KEY CLUSTERED ([StreamId],[Version],[CommitId])
        ///);
        ///GO
        ///
        ///CREATE NONCLUSTERED INDEX IX_Streams_EventId ON [dbo].[Streams] ([EventId])
        ///INCLUDE ([StreamId],[Version],[CommitId],[Contrac [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Initialize {
            get {
                return ResourceManager.GetString("Initialize", resourceCulture);
            }
        }
    }
}
