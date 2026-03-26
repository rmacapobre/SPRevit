using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;

using Application = Autodesk.Revit.ApplicationServices.Application;
using Category = Autodesk.Revit.DB.Category;


namespace Specpoint.Revit2026
{
    public class RevitDrawing
    {
        /// <summary>
        /// Key for the Specpoint Guid parameter.
        /// </summary>
        public static readonly string SPECPOINT_ID = "SpecpointProjectGuid";

        /// <summary>
        /// Key for the Project Information definition group.
        /// </summary>
        public static readonly string REVIT_PROJECT_GROUP = "Project Info";

        /// <summary>
        /// The application that has the Document open.
        /// </summary>
        private UIApplication uiApplication;

        /// <summary>
        /// The Revit UI document.
        /// </summary>
        private UIDocument uiDocument;

        /// <summary>
        /// Whether the Revit document has been modified.
        /// </summary>
        private bool isModified;

        /// <summary>
        /// Initializes a new instance of the <see cref="RevitDrawing"/> class.
        /// </summary>
        /// <param name="uiDoc">The Revit UI document.</param>
        /// <param name="uiApp">The Revit UI application.</param>
        public RevitDrawing(UIDocument uiDoc, UIApplication uiApp)
        {
            this.uiDocument = uiDoc;
            this.uiApplication = uiApp;
            this.isModified = false;

        }

        /// <summary>
        /// Gets the database-level Revit application.
        /// </summary>
        /// <value>The application.</value>
        public Application Application
        {
            get
            {
                return this.UIApplication.Application;
            }
        }

        /// <summary>
        /// Gets the database-level Revit document being worked with.
        /// </summary>
        /// <value>The Revit document.</value>
        public Document Document
        {
            get
            {
                return this.UIDocument.Document;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any Revit documents have been modified.
        /// </summary>
        public bool IsModified
        {
            get
            {
                return this.isModified;
            }
            set
            {
                this.IsModified = value;
            }
        }

        /// <summary>
        /// Gets the UI-level Revit application.
        /// </summary>
        /// <value>The Revit UI application.</value>
        public UIApplication UIApplication
        {
            get
            {
                return this.uiApplication;
            }
        }

        /// <summary>
        /// Gets the UI-level Revit document being worked with.
        /// </summary>
        /// <value>The Revit UI document.</value>
        public UIDocument UIDocument
        {
            get
            {
                return this.uiDocument;
            }
        }

        public Assembly GetAssemblyFromElementType(ElementType elementType, bool includeParameters)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException("elementType");
            }

            Assembly assembly = new Assembly();

            // Get category the Revit Type falls under
            Category cat = elementType.Category;
            if (cat != null)
            {
                assembly.BIMCategory = cat.Name;
                assembly.BIMCategoryId = cat.Id.Value;
            }

            // Get family the Revit Type belongs to
            Parameter familyParam = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            if (familyParam != null)
            {
                assembly.RevitFamily = familyParam.AsString();
            }

            // Get Revit Type's name
            assembly.RevitType = elementType.Name;

            // Get Revit Type's Uniformat Code and Description
            Parameter codeParam = elementType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE);
            if (codeParam != null &&
                codeParam.AsString() != null &&
                codeParam.AsString().Length > 0)
            {
                assembly.AssemblyCode = codeParam.AsString();
            }

            Parameter descParam = elementType.get_Parameter(BuiltInParameter.ASSEMBLY_DESCRIPTION);
            assembly.AssemblyDescription = (descParam == null) ? string.Empty : descParam.AsString();

            // Get Revit Type's Id so we can access the symbol directly if we need to update it later
            // (e.g., when the user changes the Assembly Code).
            assembly.RevitTypeId = elementType.UniqueId;

            return assembly;
        }

        public Assembly GetSelectedAssembly()
        {
            try
            {
                // Get the last selected element
                Element element = null;
                ICollection<ElementId> selectedIds = UIDocument.Selection.GetElementIds();
                foreach (var elementId in selectedIds)
                    element = UIDocument.Document.GetElement(elementId);

                // validate
                if (element == null) return null;

                ElementType elementType = element.GetElementType();
                if (elementType == null || elementType.Category == null)
                    return null;

                return GetAssemblyFromElementType(elementType, true);
            }
            catch (Exception ex)
            {
                Globals.Log.Write(ex);
                return null;
            }


        }

        public bool ProjectInfoProjectGroupExists()
        {
            DefinitionFile defFile = Application.OpenSharedParameterFile();
            if (defFile == null) return false;
            if (defFile.Groups == null) return false;

            DefinitionGroup dg =
                defFile.Groups.get_Item(REVIT_PROJECT_GROUP) ??
                defFile.Groups.Create(REVIT_PROJECT_GROUP);

            // Define the parameter options
            ExternalDefinitionCreationOptions options =
                new ExternalDefinitionCreationOptions(RevitDrawing.SPECPOINT_ID,
                    SpecTypeId.String.Text)
            {
                Visible = true,
                Description = "Unique project identifier for Specpoint",
                GUID = Guid.NewGuid(), // Optional: assign a custom GUID
            };

            // Create the parameter definition
            Definition definition = dg.Definitions.get_Item(RevitDrawing.SPECPOINT_ID);
            if (definition == null)
            {
                definition = dg.Definitions.Create(options);
            }

            return dg != null;
        }

        public bool SharedParameterExists(string name)
        {
            if (this.UIDocument == null) return false;

            // Get the value of the Specpoint ID shared parameter.
            Element projectInfo = GetProjectInformation();
            if (projectInfo == null) return false;

            Parameter param = GetParameter(projectInfo, name);
            return param != null;
        }

        public string GetSharedParameter(string name)
        {
            if (this.UIDocument == null) return null;

            // Get the value of the Specpoint ID shared parameter.
            Element projectInfo = GetProjectInformation();
            if (projectInfo == null) return null;

            Parameter param = GetParameter(projectInfo, name);
            return param == null ? string.Empty : param.AsString();
        }

        public void SetSharedParameter(string name, string value)
        {
            // Validate.
            if (UIDocument == null) return;

            Element projectInfo = GetProjectInformation();
            if (projectInfo == null) return;

            Parameter projectName = GetParameter(projectInfo, name);
            if (projectName == null)
            {
                AddSharedParameter(name);
                projectName = GetParameter(projectInfo, name);
                if (projectName == null)
                {
                    throw new Exception(String.Format("Shared Parameter ({0}) does not exist.", name));
                }
            }

            // Assign new GUID.
            Transaction transaction = new Transaction(this.Document, string.Format("Set {0}", name));
            try
            {
                transaction.Start();
                if (!projectName.Set(value))
                {
                    throw new Exception(String.Format("Revit API call to set {0} parameter value to {1} failed.", name, value));
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction.HasStarted())
                {
                    transaction.RollBack();
                }

                Globals.Log.Write(ex);
                throw;
            }

            // Set modified flag
            this.isModified = true;
        }

        // Gets a parameter with the given name from the given element
        protected Parameter GetParameter(Element element, string paramName)
        {
            // Find the paramter with the given name
            //return element.get_Parameter(paramName);

            ParameterSetIterator pi = element.Parameters.ForwardIterator();
            while (pi.MoveNext())
            {
                Parameter p = pi.Current as Parameter;
                if (p == null || p.Definition == null)
                    continue;

                if (p.Definition.Name == paramName)
                    return p;
            }

            return null; // param not found
        }

        /// <summary>
        /// Adds the Specpoint shared parameter to the Project Info element.
        /// </summary>
        private void AddSharedParameter(string value)
        {
            // Save current shared param file so we can restore
            string previous = this.Application.SharedParametersFilename;

            // Add shared parameter.
            Transaction transaction = new Transaction(this.Document, string.Format("Add {0} Shared Parameter", value));
            try
            {
                if (string.IsNullOrEmpty(previous))
                {   
                    // Set shared param file
                    FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    this.Application.SharedParametersFilename = fi.DirectoryName + @"\SharedParams.txt";
                }

                if (!ProjectInfoProjectGroupExists())
                {
                    // Restore shared params file.
                    this.Application.SharedParametersFilename = previous;

                    return;
                }

                // Set definition
                DefinitionFile defFile = Application.OpenSharedParameterFile();
                DefinitionGroup dg = defFile.Groups.get_Item(REVIT_PROJECT_GROUP);

                // Get dummy value from the file
                ExternalDefinition ed = dg.Definitions.get_Item(value) as ExternalDefinition;
                if (ed == null)
                {
                    ForgeTypeId typeID = new ForgeTypeId();
                    ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(value, typeID);
                    ed = dg.Definitions.Create(options) as ExternalDefinition;
                }

                // Begin transaction
                transaction.Start();

                // Set category
                CategorySet catSet = this.Application.Create.NewCategorySet();
                Settings docSettings = this.Document.Settings;
                Categories cats = docSettings.Categories;
                Category projectInfoCat = cats.get_Item(BuiltInCategory.OST_ProjectInformation);
                if (!catSet.Insert(projectInfoCat))
                {
                    throw new Exception("Failed to insert Project Information category into new category set.");
                }

                // Bind
                InstanceBinding instanceBind = this.Application.Create.NewInstanceBinding(catSet);

                // Add definition
                if (!this.Document.ParameterBindings.Insert(ed, instanceBind))
                {
                    if (!this.Document.ParameterBindings.ReInsert(ed, instanceBind))
                    {
                        throw new Exception(string.Format("Failed to insert {0} parameter into Project Information", value));
                    }
                }

                // Commit transaction
                transaction.Commit();

                // Set modified flag so user will be prompted to save when they close their model.
                this.isModified = true;

                // Restore previous shared param file
                this.Application.SharedParametersFilename = previous;
            }
            catch (Exception ex)
            {
                // Restore shared params file.
                this.Application.SharedParametersFilename = previous;

                // Rollback transaction.
                if (transaction.HasStarted())
                {
                    transaction.RollBack();
                }

                Globals.Log.Write(ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the project information element.
        /// </summary>
        /// <returns>Project information element.</returns>
        private Element GetProjectInformation()
        {
            try
            {
                return this.Document.ProjectInformation;
            }
            catch (Exception ex)
            {
                string msg = "Could not access the Project Information element. Please consult your Revit administrator to make \"Project Information\" available under the \"Settings\" menu.";
                throw new Exception(msg, ex);
            }
        }

    }
}
