using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Specpoint.Revit2026
{
    public class RevitElements : Dictionary<string, Assembly>
    {
        private Document _doc;

        // List of assembly codes that have been assigned to the model
        public HashSet<string> AssignedAssemblyCodes { get; set; }

        public RevitElements()
        {
            AssignedAssemblyCodes = new HashSet<string>();
        }

        public RevitElements(Document doc) : this()
        {
            _doc = doc;
        }

        public void Init(Document doc, List<string> includedCategories)
        {
            Globals.Log.Write("RevitElements.Init");

            Dictionary<string, Assembly> rows = new Dictionary<string, Assembly>();
            using (new WaitCursor())
            {
                // For all Revit categories
                foreach (var c in Globals.revitCategories)
                {
                    if (c.Value == null)
                    {
                        Globals.Log.Write("Category Value is null: " + c.Key);
                        continue;
                    }

                    // Filter out categories not included in the filter assemblies
                    if (!includedCategories.Contains(c.Value.Name)) continue;

                    // Create Revit category filters from the assembly filter's included categories.
                    List<ElementFilter> filters = new List<ElementFilter>();

                    // Create Category filter or add to it if it already exists.
                    BuiltInCategory revitCategory = (BuiltInCategory)c.Value.BuiltInCategory;
                    ElementCategoryFilter categoryFilter = new ElementCategoryFilter(revitCategory);
                    filters.Add(categoryFilter);

                    // OR together the category filters (element must have one of the
                    // specified categories).
                    ElementFilter elementFilter = new LogicalOrFilter(filters);

                    // Retrieve elements.
                    List<Element> elems = new List<Element>();
                    FilteredElementCollector collector = new FilteredElementCollector(doc).WherePasses(elementFilter);

                    IEnumerator ei = collector.GetEnumerator();
                    while (ei.MoveNext())
                    {
                        try
                        {
                            Element element = ei.Current as Element;
                            if (element == null) continue;

                            // Get element's Revit Type
                            ElementType elementType = element.GetElementType();
                            if (elementType == null) continue;

                            // Get category the Revit Type falls under
                            Category bimCategory = elementType.Category;
                            if (bimCategory == null) continue;

                            var materials = element.GetMaterials();

                            string revitFamily = "";
                            // Get family the Revit Type belongs to
                            Parameter familyParam = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                            if (familyParam != null)
                            {
                                revitFamily = familyParam.AsString();
                            }

                            string keynote = "";
                            Parameter keynoteParam = elementType.get_Parameter(BuiltInParameter.KEYNOTE_PARAM);
                            if (keynoteParam != null)
                            {
                                keynote = keynoteParam.AsString();
                            }

                            // Get Revit Type's name
                            string revitType = elementType.Name;

                            // Get Revit Type's Uniformat Code and Description
                            string assemblyCode = "";
                            Parameter codeParam = elementType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE);
                            if (codeParam != null &&
                                codeParam.AsString() != null &&
                                codeParam.AsString().Length > 0)
                            {
                                assemblyCode = codeParam.AsString();

                                // Keep a list of assembly codes assigned to the model
                                AssignedAssemblyCodes.Add(assemblyCode);
                            }

                            Parameter descParam = elementType.get_Parameter(BuiltInParameter.ASSEMBLY_DESCRIPTION);
                            string assemblyDescription = (descParam == null) ? string.Empty : descParam.AsString();

                            // Get Revit Type's Id so we can access the symbol directly if we need to update it later
                            // (e.g., when the user changes the Assembly Code).
                            string revitTypeId = elementType.UniqueId;

                            Assembly row = new Assembly()
                            {
                                BIMCategory = bimCategory.Name,
                                BIMCategoryId = bimCategory.Id.Value,
                                RevitFamily = revitFamily,
                                RevitType = revitType,
                                RevitTypeId = revitTypeId,
                                AssemblyCode = assemblyCode,
                                AssemblyDescription = assemblyDescription,
                                Keynote = keynote
                            };

                            this[elementType.UniqueId] = row;

                        }
                        catch (Exception)
                        {
                            // Proceed to next drawing element
                            continue;
                        }
                    }
                }
            }
        }

        public void Init(Document doc)
        {
            Init(doc, Globals.includedCategories);
        }

        public void GetMaterial(Document document, FamilyInstance familyInstance)
        {
            foreach (Parameter parameter in familyInstance.Parameters)
            {
                Definition definition = parameter.Definition;
                // material is stored as element id
                if (parameter.StorageType == StorageType.ElementId)
                {
                    if (definition.GetGroupTypeId() == GroupTypeId.Materials)
                    {
                        Autodesk.Revit.DB.Material material = null;
                        Autodesk.Revit.DB.ElementId materialId = parameter.AsElementId();
                        if (-1 == materialId.Value)
                        {
                            //Invalid ElementId, assume the material is "By Category"
                            if (null != familyInstance.Category)
                            {
                                material = familyInstance.Category.Material;
                            }
                        }
                        else
                        {
                            material = document.GetElement(materialId) as Material;
                        }

                        TaskDialog.Show("Revit", "Element material: " + material.Name);
                        break;
                    }
                }
            }
        }
    }
}
