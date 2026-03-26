using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace Specpoint.Revit2026
{
    public partial class ProductSelectionProgress : System.Windows.Forms.Form
    {
        private const string title = "Product Selection";
        private ExternalCommandData commandData;

        public ProductSelectionProgress()
        {
            InitializeComponent();
        }

        public ProductSelectionProgress(ExternalCommandData commandData) : this()
        {
            this.commandData = commandData;
        }

        private async void ProductSelectionProgress_Load(object sender, EventArgs e)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var app = uiapp.Application;

            // Get Revit categories from document
            if (Globals.revitCategories == null)
            {
                Globals.revitCategories = new RevitCategories(doc);
            }

            // Get Specpoint projectId 
            RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);
            Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);

            // Get selected Revit element
            ElementId selElementId = uidoc.Selection.GetElementIds().FirstOrDefault();
            if (selElementId == null)
            {
                string msg = "Product Selection requires a selected BIM element in the drawing.";
                ErrorReporter.ReportError(msg);
                Close();
                return;
            }

            // Get selected Revit assembly code
            Element selElement = doc.GetElement(selElementId);
            ElementType elemType = doc.GetElement(selElement.GetTypeId()) as ElementType;
            string famName = elemType.FamilyName;
            string elementId = selElementId.ToString();
            string assemblyCode = elemType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE).AsString();
            this.Text = String.Format("Loading Product Listings for {0}", assemblyCode);
            if (string.IsNullOrEmpty(assemblyCode))
            {
                string msg = String.Format("The selected element has no Assembly Code.");
                ErrorReporter.ReportError(msg);
                Close();
                return;
            }

            // Get selected project category in Revit
            Assembly selectedProjectCategory = dwg.GetSelectedAssembly();
            List<string> includedCategories = new List<string>();
            if (selectedProjectCategory != null)
            {
                includedCategories.Add(selectedProjectCategory.BIMCategory);
            }

            // Get Revit elements
            Globals.revitElements = new RevitElements();
            Globals.revitElements.Init(doc, includedCategories);

            LoadProductListings listings = new LoadProductListings(
                this, Globals.SpecpointProjectID, assemblyCode,
                includedCategories);
            bool ret = await listings.Init();

            Close();

        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public bool ShowQuestion(string value)
        {
            this.Visible = false;

            // Get Revit Apps main window handle
            // Known Revit Issue
            // Set focus on Revit App because it loses it when modeless dialog closes
            Process revitApp = Process.GetCurrentProcess();
            SetForegroundWindow(revitApp.MainWindowHandle);

            // Ask user if you want to add to Specpoint project
            MessageBoxResult result = MessageBox.Show(value, title, MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowInformation(string value)
        {
            // Known Revit Issue
            // Set focus on Revit App because it loses it when modeless dialog closes
            Process revitApp = Process.GetCurrentProcess();
            SetForegroundWindow(revitApp.MainWindowHandle);

            MessageBox.Show(value, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
