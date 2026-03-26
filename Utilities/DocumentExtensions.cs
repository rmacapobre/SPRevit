using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public static class DocumentExtensions
    {
        /// <summary>
        /// Gets an enumerator of all the materials in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>Enumerator over materials in document.</returns>
        public static IEnumerable<Material> GetMaterials(this Document document)
        {
            FilteredElementCollector fec = new FilteredElementCollector(document);
            fec.OfClass(typeof(Material));
            IEnumerable<Material> materials = fec.ToElements().Cast<Material>();
            return materials;
        }

        /// <summary>
        /// Gets the material having the specified name.
        /// </summary>
        /// <param name="document">Document to retrieve material from.</param>
        /// <param name="materialName">Name of material to retrieve.</param>
        /// <returns>Material having specified name or <c>null</c> if not found.</returns>
        public static Material GetMaterial(this Document document, string materialName)
        {
            FilteredElementCollector fec = new FilteredElementCollector(document);
            fec.OfClass(typeof(Material));
            IEnumerable<Material> materials = fec.ToElements().Cast<Material>();
            return materials.FirstOrDefault(x => x.Name == materialName);
        }

        /// <summary>
        /// Safely saves a Revit document with comprehensive exception handling.
        /// Catches all exception types including WPF threading exceptions, SEH exceptions, and access violations.
        /// This method prevents crashes by catching all possible exception types that can occur during document save.
        ///
        /// NOTE: In .NET 8+, some native/unmanaged exceptions from Document.Save() may still cause crashes
        /// at the native interop boundary. This is a limitation of the .NET runtime's ability to catch
        /// exceptions thrown from unmanaged Revit code.
        /// </summary>
        /// <param name="document">The Revit document to save</param>
        /// <param name="context">Context description for logging (e.g., "project link update", "assembly code update")</param>
        /// <returns>True if save succeeded, false if an error occurred</returns>
        public static bool SafeSave(this Document document, string context = "document save")
        {
            try
            {
                Globals.Log.Write($"[SafeSave] Attempting to save document (context: {context})");

                // Pre-save validation
                if (document == null)
                {
                    Globals.Log.Write("[SafeSave] Document is null, cannot save");
                    MessageBox.Show("The document reference is null and cannot be saved.",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (document.IsReadOnly)
                {
                    Globals.Log.Write("[SafeSave] Document is read-only, cannot save");
                    MessageBox.Show("The document is read-only and cannot be saved.",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // NOTE: We do NOT check IsModifiable here because document.Save() should be called
                // OUTSIDE of transactions (when IsModifiable is false). IsModifiable only indicates
                // whether elements can be modified (requires active Transaction), not whether the
                // document can be saved to disk.

                // Perform the save operation with comprehensive exception handling
                try
                {
                    Globals.Log.Write($"[SafeSave] Calling document.Save() (context: {context})");
                    document.Save();
                    Globals.Log.Write($"[SafeSave] Document saved successfully (context: {context})");
                    return true;
                }
                catch (Exception saveEx)
                {
                    // Log the immediate exception from Save()
                    Globals.Log.Write($"[EXCEPTION during document.Save()] Context: {context}");
                    Globals.Log.Write($"[EXCEPTION] Type: {saveEx.GetType().FullName}");
                    Globals.Log.Write($"[EXCEPTION] Message: {saveEx.Message}");
                    if (saveEx.StackTrace != null)
                    {
                        Globals.Log.Write($"[EXCEPTION] Stack: {saveEx.StackTrace}");
                    }
                    if (saveEx.InnerException != null)
                    {
                        Globals.Log.Write($"[EXCEPTION] Inner: {saveEx.InnerException.GetType().FullName} - {saveEx.InnerException.Message}");
                    }

                    // Re-throw to be caught by outer catch blocks
                    throw;
                }
            }
            catch (System.Runtime.InteropServices.SEHException sehEx)
            {
                // Structured Exception Handling exception - low-level Windows error
                int errorCode = sehEx.ErrorCode;
                Globals.Log.Write($"[SEH EXCEPTION in SafeSave] Context: {context}");
                Globals.Log.Write($"[SEH EXCEPTION] Code: 0x{errorCode:X8}, Message: {sehEx.Message}");
                Globals.Log.Write($"Stack: {sehEx.StackTrace}");

                if (errorCode == unchecked((int)0xC0000374))
                {
                    Globals.Log.Write("[HEAP CORRUPTION during Save] Error code 0xC0000374");
                    MessageBox.Show("A critical heap corruption error occurred while saving.\n\nPlease save your work manually and restart Revit.",
                        "Critical Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Unable to save the document due to a system error (Code: 0x{errorCode:X8}).\n\nPlease try saving manually.",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return false;
            }
            catch (System.AccessViolationException avEx)
            {
                // Memory access violation
                Globals.Log.Write($"[ACCESS VIOLATION in SafeSave] Context: {context}");
                Globals.Log.Write($"[ACCESS VIOLATION] Message: {avEx.Message}");
                Globals.Log.Write($"Stack: {avEx.StackTrace}");

                MessageBox.Show("A critical memory access violation occurred while saving.\n\nPlease save your work manually and restart Revit.",
                    "Critical Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException revitEx)
            {
                // Revit-specific exception
                Globals.Log.Write($"[REVIT EXCEPTION in SafeSave] Context: {context}");
                Globals.Log.Write($"[REVIT EXCEPTION] {revitEx.Message}");
                Globals.Log.Write($"Stack: {revitEx.StackTrace}");

                MessageBox.Show($"Revit error while saving:\n\n{revitEx.Message}\n\nPlease try saving manually.",
                    "Revit Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (System.InvalidOperationException invOpEx)
            {
                // General invalid operation (could be WPF-related)
                Globals.Log.Write($"[INVALID OPERATION in SafeSave] Context: {context}");
                Globals.Log.Write($"[INVALID OPERATION] {invOpEx.Message}");
                Globals.Log.Write($"Stack: {invOpEx.StackTrace}");

                if (invOpEx.StackTrace?.Contains("System.Windows") == true)
                {
                    Globals.Log.Write("[WPF THREADING ISSUE DETECTED] Exception originated from System.Windows");
                }

                MessageBox.Show($"Error saving document:\n\n{invOpEx.Message}\n\nPlease try saving manually.",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                // Catch-all for any other exception type
                Globals.Log.Write($"[GENERAL EXCEPTION in SafeSave] Context: {context}");
                Globals.Log.Write($"[EXCEPTION] Type: {ex.GetType().FullName}");
                Globals.Log.Write($"[EXCEPTION] Message: {ex.Message}");
                Globals.Log.Write($"Stack: {ex.StackTrace}");

                // Check if this is a WPF/threading exception
                if (ex.GetType().FullName.Contains("System.Windows") ||
                    ex.StackTrace?.Contains("System.Windows") == true)
                {
                    Globals.Log.Write("[WPF/THREADING EXCEPTION DETECTED] This may be a cross-thread access issue");
                }

                // Check for inner exceptions
                if (ex.InnerException != null)
                {
                    Globals.Log.Write($"[INNER EXCEPTION] Type: {ex.InnerException.GetType().FullName}");
                    Globals.Log.Write($"[INNER EXCEPTION] Message: {ex.InnerException.Message}");
                }

                MessageBox.Show($"Unexpected error while saving document:\n\n{ex.Message}\n\nType: {ex.GetType().Name}\n\nPlease try saving manually.",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
