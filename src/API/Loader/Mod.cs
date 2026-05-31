using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using log4net;
using NuGet.Versioning;

namespace Breach
{
    /// <summary>
    /// Marks the class as a Breach module to be loaded at startup
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoLoadAttribute : Attribute
    {
        public bool HasOwnLogFile { get; set; } = false;
        public string LoggerName { get; set; } = null;
    }

    /// <summary>
    /// Represents a loadable Breach code module
    /// </summary>
    public abstract class BreachModule
    {
        public ILog Logger { get; internal set; }

        /// <summary>
        /// Override to load optional modules (integration with other mods).
        /// <br/>
        /// This method is called <i>before</i> <see cref="Initialize"/>,
        /// it allows mods to dynamically register additional modules.
        /// <br/>
        /// This can be used to implement integration with other mods,
        /// which are not a direct dependency of this mod.
        /// Inspect the <paramref name="loadedMods"/> list to determine
        /// which other mods have been loaded.
        /// </summary>
        /// <param name="loadedMods">list of all mods which have been loaded, in load order</param>
        /// <returns>A list of additional modules that this mod wishes to register.
        /// <br/>
        /// or <c>null</c>, if no additional modules should be registered.</returns>
        public virtual IEnumerable<BreachModule> RegisterModules(IReadOnlyList<BreachModMetadata> loadedMods) {
            return null;
        }

        /// <summary>
        /// Override to add initialization code to your mod (namely, add hooks).
        /// <br/>
        /// Module initialization happens immediately after
        /// <see cref="RegisterModules"/> has been called on all mods,
        /// but <b>before</b> the game itself is initialized.
        /// <br/>
        /// Mods are initialized in load order, a mod is always
        /// loaded <b>after</b> all of its dependencies.
        /// Mods that do not depend on each other may be loaded
        /// in any order.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Override to run logic after the game is fully initialized.
        /// <br/>
        /// The post-initialization step happens <b>after</b>
        /// the game has been initialized,
        /// but <b>before</b> the core game loop is started.
        /// <br/>
        /// Mods are initialized in load order, a mod is always
        /// loaded <b>after</b> all of its dependencies.
        /// Mods that do not depend on each other may be loaded
        /// in any order.
        /// </summary>
        public virtual void PostInitialize()
        {
            // TODO: make sure this is actually called
        }

        /// <summary>
        /// Override to undo initialization code and dispose of system resources (namely, dispose of hooks).
        /// </summary>
        public virtual void Deinitialize()
        {
            // TODO: make sure this is actually called
        }

        /// <summary>
        /// Override to do additional content loading work.
        /// </summary>
        public virtual void LoadContent()
        {
            // TODO: make sure this is actually called
        }

        /// <summary>
        /// Override to do additional content unloading work.
        /// </summary>
        public virtual void UnloadContent()
        {
            // TODO: make sure this is actually called
        }
    }

    /// <summary>
    /// Serializable mod metadata
    /// </summary>
    public class BreachModMetadata
    {
        /// <summary>
        /// Unique identifier of this mod.
        /// <br/>
        /// A mod's ID can be referenced in the
        /// <see cref="Dependencies"/> of another mod.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Display name for this mod.
        /// <br/>
        /// Used in menus and settings.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Version of this mod used.
        /// <br/>
        /// The version is determined by the mod author.
        /// </summary>
        public SemanticVersion Version { get; init; }

        /// <summary>
        /// Filenames of this mod's assemblies.
        /// </summary>
        public ImmutableArray<string> Assemblies { get; init; } = ImmutableArray<string>.Empty;

        /// <summary>
        /// IDs of other mods which this mod depends on.
        /// <br/>
        /// Breach ensures that all of a mod's dependencies are present,
        /// and will produce an error if any are missing.
        /// <br/>
        /// On startup, all mods are sorted such that each mod
        /// is only initialized after <b>all</b> its dependencies
        /// have been initialized.
        /// </summary>
        public ImmutableArray<string> Dependencies { get; init; } = ImmutableArray<string>.Empty;
    }

    public class BreachMod {
        /// <summary>
        /// Contents of this mod's <c>mod.json</c>.
        /// </summary>
        public BreachModMetadata Metadata { get; init; }

        /// <summary>
        /// Path of the folder which this mod was loaded from.
        /// </summary>
        public string Path { get; init; }

        /// <summary>
        /// List of all modules that have been registered by this mod.
        /// </summary>
        public ImmutableArray<BreachModule> Modules { get; init; }
    }

    internal class BreachModHandle {
        /// <summary>
        /// Contents of this mod's <c>mod.json</c>.
        /// </summary>
        public BreachModMetadata Metadata { get; init; }

        /// <summary>
        /// Path of the folder which this mod was loaded from.
        /// </summary>
        public string Path { get; init; }
    }
}