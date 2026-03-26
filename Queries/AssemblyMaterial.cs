using System;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Represents a building material.
    /// </summary>
    public class AssemblyMaterial : Assembly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyMaterial"/> class.
        /// </summary>
        public AssemblyMaterial()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyMaterial"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssemblyMaterial(String name)
        {
            this._name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        private String _name;

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        public String Class
        {
            get
            {
                return this._class;
            }
            set
            {
                this._class = value;
            }
        }

        private String _class;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this._name;
        }
    }
}
