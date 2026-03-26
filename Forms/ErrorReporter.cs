using System;
using System.Text;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public static class ErrorReporter
    {
        /// <summary>
        /// Text for caption of all error/question dialogs.
        /// </summary>
        private static string dialogCaption = "Specpoint";

        /// <summary>
        /// Gets or sets the caption to use for all error dialogs and prompts.
        /// </summary>
        public static string DialogCaption
        {
            get
            {
                return dialogCaption;
            }

            set
            {
                dialogCaption = value;
            }
        }

        /// <summary>
        /// Shows a Yes/No/Cancel dialog box and returns the result.
        /// </summary>
        /// <param name="question">Text of question.</param>
        /// <returns>User's chosen response.</returns>
        public static DialogResult AskYesNoCancelQuestion(string question)
        {
            return System.Windows.Forms.MessageBox.Show(
                question,
                dialogCaption,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }

        /// <summary>
        /// Shows a Yes/No dialog box with default response of Yes and returns the result.
        /// </summary>
        /// <param name="question">Text of question.</param>
        /// <returns>User's chosen response.</returns>
        public static DialogResult AskYesNoQuestion(string question)
        {
            return AskYesNoQuestion(question, DialogResult.Yes, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Shows a Yes/No dialog box with default response of Yes and returns the result.
        /// </summary>
        /// <param name="question">Text of question.</param>
        /// <param name="defaultAnswer">Default response.</param>
        /// <returns>User's chosen response.</returns>
        public static DialogResult AskYesNoQuestion(string question, DialogResult defaultAnswer)
        {
            return AskYesNoQuestion(question, defaultAnswer, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Shows a Yes/No dialog box with the given default response and returns the result.
        /// </summary>
        /// <param name="question">Text of question.</param>
        /// <param name="defaultAnswer">Default response.</param>
        /// <param name="icon">Icon to display in dialog.</param>
        /// <returns>User's chosen response.</returns>
        public static DialogResult AskYesNoQuestion(string question, DialogResult defaultAnswer, MessageBoxIcon icon)
        {
            return System.Windows.Forms.MessageBox.Show(
                question,
                dialogCaption,
                MessageBoxButtons.YesNo,
                icon,
                defaultAnswer == DialogResult.Yes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2);
        }

        /// <summary>
        /// Reports an exception to the user.
        /// </summary>
        /// <param name="ex">Exception that was thrown.</param>
        public static void ReportError(Exception ex)
        {
            ShowErrorDialog(ex.ToString());
        }

        /// <summary>
        /// Reports an error message to the user
        /// </summary>
        /// <param name="errorMessage">Message to display.</param>
        public static void ReportError(string errorMessage)
        {
            ShowErrorDialog(errorMessage);
        }

        /// <summary>
        /// Reports an exception to the user, prefacing it with a descriptive message.
        /// </summary>
        /// <param name="ex">The exception that was thrown.</param>
        /// <param name="message">The message to show before the exception details.</param>
        public static void ReportError(Exception ex, string message)
        {
            string details = ex.ToString();
            string text = string.Format("{0}{1}{1}Details:{1}{2}", message, Environment.NewLine, details);
            ShowErrorDialog(text);
        }

        /// <summary>
        /// Reports an informational (non-error) message to the user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public static void ShowMessage(string message)
        {
            ShowMessage(message, MessageBoxIcon.None);
        }

        /// <summary>
        /// Reports an informational (non-error) message to the user.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="icon">Icon to show in dialog.</param>
        public static void ShowMessage(string message, MessageBoxIcon icon)
        {
            System.Windows.Forms.MessageBox.Show(
                message,
                dialogCaption,
                System.Windows.Forms.MessageBoxButtons.OK,
                icon);
        }

        /// <summary>
        /// Shows an informational dialog with OK and Cancel options. Returns result of the dialog.
        /// </summary>
        /// <param name="warning">Warning to display</param>
        /// <returns>User's chosen response.</returns>
        public static DialogResult ShowOKCancelMessage(string warning)
        {
            return System.Windows.Forms.MessageBox.Show(
                warning,
                dialogCaption,
                System.Windows.Forms.MessageBoxButtons.OKCancel,
                System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows a warning dialog with Retry and Cancel buttons. Returns user's response.
        /// </summary>
        /// <param name="messasge">The warning.</param>
        /// <returns>User's response.</returns>
        public static DialogResult ShowRetryCancelMessage(string messasge)
        {
            return System.Windows.Forms.MessageBox.Show(
                messasge,
                dialogCaption,
                System.Windows.Forms.MessageBoxButtons.RetryCancel,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Shows a warning dialog with Retry and Cancel buttons. Returns user's response.
        /// </summary>
        /// <param name="message">The warning.</param>
        /// <param name="error">The error that occurred.</param>
        /// <param name="includeStackTrace">if set to <c>true</c> the stack trace is included;
        /// otherwise, just the message from the exception is shown.</param>
        /// <returns>
        /// User's response.
        /// </returns>
        public static DialogResult ShowRetryCancelMessage(string message, Exception error, bool includeStackTrace = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine();
            sb.Append("Error details: ");
            if (includeStackTrace)
            {
                sb.AppendLine(error.ToString());
            }
            else
            {
                sb.AppendLine(error.Message);
            }

            string prompt = sb.ToString();
            return System.Windows.Forms.MessageBox.Show(
                prompt,
                dialogCaption,
                System.Windows.Forms.MessageBoxButtons.RetryCancel,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Shows an error dialog to the user.
        /// </summary>
        /// <param name="errorMessage">Text of error.</param>
        private static void ShowErrorDialog(string errorMessage)
        {
            System.Windows.Forms.MessageBox.Show(
                errorMessage,
                dialogCaption,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
