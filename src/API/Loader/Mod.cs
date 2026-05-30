using System;
using System.Collections.Generic;
using log4net;
using NuGet.Versioning;

namespace Breach
{
    /// <summary>
    /// Marks the class as a Breach module to be loaded
    /// </summary>
    public class BreachModuleAttribute : Attribute
    {
        public bool HasOwnLogFile { get; set; } = false;
        public string LoggerName { get; set; } = null;
    }

    /// <summary>
    /// Represents a loadable Breach code module
    /// </summary>
    public class BreachModule
    {
        public ILog Logger { get; internal set; }

        /// <summary>
        /// Override to add initialization code to your mod (namely, add hooks)
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Override to undo initialization code and dispose of system resources (namely, dispose of hooks)
        /// </summary>
        public virtual void Deinitialize()
        {
        }

        /// <summary>
        /// Override to do additional content loading work
        /// </summary>
        public virtual void LoadContent()
        {
        }

        /// <summary>
        /// Override to do additional content unloading work
        /// </summary>
        public virtual void UnloadContent()
        {
        }
    }

    public class BreachMod
    {
        public BreachModMetadata Metadata;
        public List<BreachModule> Modules;
    }

    /// <summary>
    /// Serializable mod metadata
    /// </summary>
    public struct BreachModMetadata
    {
        /// <summary>
        /// Unique identifier for the mod
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name for the mod
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of the mod used
        /// </summary>
        public SemanticVersion Version;

        /// <summary>
        /// Filenames for the mod's assemblies
        /// </summary>
        public string[] Assemblies;

        /// <summary>
        /// IDs of dependency mods
        /// </summary>
        public string[] Dependencies;
    }
}