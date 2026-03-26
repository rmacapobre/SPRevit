using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.Revit.DB;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Provides extensions for the <see cref="Autodesk.Revit.DB.Element"/> class.
    /// </summary>
    public static class ElementExtensions
    {
        /// <summary>
        /// Gets the type of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element's <see cref="ElementType"/>.</returns>
        public static ElementType GetElementType(this Element element)
        {
            ElementId typeId = element.GetTypeId();
            return element.Document.GetElement(typeId) as ElementType;
        }

        /// <summary>
        /// Gets the element's materials.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Element's materials</returns>
        public static ICollection<Material> GetMaterials(this Element element)
        {
            var materials = new Collection<Material>();
            foreach (ElementId materialId in element.GetMaterialIds(true))
            {
                var material = element.Document.GetElement(materialId) as Material;
                if (material != null)
                {
                    materials.Add(material);
                }
            }

            return materials;
        }
    }
}
