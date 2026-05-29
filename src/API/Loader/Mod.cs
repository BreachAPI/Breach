using System;

namespace Breach
{
    /// <summary>
    /// Marks the class as a Breach module to be loaded
    /// </summary>
    public class BreachModuleAttribute : Attribute
    {
    }

    /// <summary>
    /// Represents a loadable Breach code module
    /// </summary>
    public class BreachModule
    {
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
}