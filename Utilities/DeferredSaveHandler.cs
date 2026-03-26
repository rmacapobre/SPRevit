using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// External event handler for deferred document saving.
    /// This executes the save operation outside of the command execution context,
    /// which is safer and avoids timing issues with native Revit operations.
    /// </summary>
    public class DeferredSaveHandler : IExternalEventHandler
    {
        private Document _documentToSave;
        private string _saveContext;
        private bool _saveCompleted;
        private bool _saveSucceeded;
        private string _saveError;

        /// <summary>
        /// Sets the document to be saved on the next Execute call.
        /// </summary>
        /// <param name="document">The document to save</param>
        /// <param name="context">Context description for logging</param>
        public void SetDocument(Document document, string context)
        {
            _documentToSave = document;
            _saveContext = context;
            _saveCompleted = false;
            _saveSucceeded = false;
            _saveError = null;
        }

        /// <summary>
        /// Returns true if the save operation has completed (success or failure).
        /// </summary>
        public bool IsSaveCompleted => _saveCompleted;

        /// <summary>
        /// Returns true if the save succeeded.
        /// </summary>
        public bool SaveSucceeded => _saveSucceeded;

        /// <summary>
        /// Returns the error message if save failed, or null if successful.
        /// </summary>
        public string SaveError => _saveError;

        /// <summary>
        /// Executes the deferred save operation.
        /// Called by Revit on the main thread when idle.
        /// </summary>
        public void Execute(UIApplication app)
        {
            try
            {
                Globals.Log.Write($"[DeferredSaveHandler] Execute called for context: {_saveContext}");

                if (_documentToSave == null)
                {
                    Globals.Log.Write("[DeferredSaveHandler] No document to save");
                    _saveCompleted = true;
                    _saveSucceeded = false;
                    _saveError = "No document was set for saving";
                    return;
                }

                // Use SafeSave which has comprehensive exception handling
                Globals.Log.Write($"[DeferredSaveHandler] Calling SafeSave for context: {_saveContext}");
                bool result = _documentToSave.SafeSave(_saveContext);

                _saveCompleted = true;
                _saveSucceeded = result;

                if (result)
                {
                    Globals.Log.Write($"[DeferredSaveHandler] Save succeeded for context: {_saveContext}");
                    _saveError = null;
                }
                else
                {
                    Globals.Log.Write($"[DeferredSaveHandler] Save failed for context: {_saveContext}");
                    _saveError = "Save operation failed (see log for details)";
                }
            }
            catch (Exception ex)
            {
                // This should rarely happen since SafeSave catches most exceptions,
                // but we catch here as a final safety net
                _saveCompleted = true;
                _saveSucceeded = false;
                _saveError = ex.Message;

                Globals.Log.Write($"[DeferredSaveHandler] EXCEPTION in Execute: {ex.GetType().Name}");
                Globals.Log.Write($"[DeferredSaveHandler] Message: {ex.Message}");
                Globals.Log.Write($"[DeferredSaveHandler] Stack: {ex.StackTrace}");
            }
            finally
            {
                // Clear the document reference to avoid holding it
                _documentToSave = null;
            }
        }

        /// <summary>
        /// Returns the name of this external event handler.
        /// </summary>
        public string GetName()
        {
            return "DeferredSaveHandler";
        }

        /// <summary>
        /// Resets the handler state for a new save operation.
        /// </summary>
        public void Reset()
        {
            _documentToSave = null;
            _saveContext = null;
            _saveCompleted = false;
            _saveSucceeded = false;
            _saveError = null;
        }
    }
}
