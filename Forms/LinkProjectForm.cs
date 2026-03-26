using System;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Specpoint.Revit2026
{
    public partial class LinkProjectForm : Form
    {
        private string notSelected = "Specpoint Project Not Selected";
        private string doc = "";
       
        private string projectId = "";
        private string previousValue = "";

        // Flag to determine to exit the form right away
        // When the user has access to the model's linked project, we can exit right away
        public bool ExitRightAway { get; set; }

        // Flag to identify a project was rebased scenario 
        public bool RebasedProject { get; set; }

        public ProjectItem selectedProject = null;

        public LinkProjectForm()
        {
            RebasedProject = false;
            ExitRightAway = false;
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Normal;

            InitializeComponent();
        }

        public LinkProjectForm(string activeDoc, string specpointProjectId) : this()
        {
            doc = activeDoc;
            projectId = specpointProjectId;
            previousValue = notSelected;
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            selectedProject = null;
            textBoxSpecpointProject.Text = notSelected;

            // Enable OK and Remove
            EnableButtons();
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            try
            {
                using (SelectProjectForm form = new SelectProjectForm(selectedProject))
                {
                    DialogResult ret = form.ShowDialog(this);

                    if (ret == DialogResult.OK && form.selectedProject != null)
                    {
                        selectedProject = form.selectedProject;
                        textBoxSpecpointProject.Text = selectedProject.name;
                    }
                }

                // Enable OK and Remove
                EnableButtons();
            }
            catch (TokenExpiredException)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                throw;
            }
        }

        private async void LinkProjectForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Check if form is already disposed or disposing
                if (IsDisposed || Disposing)
                {
                    return;
                }

                labelUserValue.Text = "-";
                if (!string.IsNullOrEmpty(Globals.LoginUser))
                {
                    // If after login
                    labelUserValue.Text = Globals.LoginUser;
                }
                else
                {
                    // Otherwise get it from the config file
                    Globals.LoginUser = SpecpointRegistry.GetValue("name");
                    labelUserValue.Text = Globals.LoginUser;
                }

                textBoxSpecpointProject.Enabled = false;
                textBoxSpecpointProject.TextAlign = HorizontalAlignment.Center;
                textBoxSpecpointProject.Text = notSelected;

                // Enable OK and Remove
                EnableButtons();

                if (!string.IsNullOrEmpty(projectId))
                {
                    // Get the Specpoint project name
                    Query query = new Query(nameof(LinkProjectForm_Load));
                    var result = await query.getProject(projectId);

                    if (IsDisposed || Disposing)
                    {
                        return;
                    }

                    if (result != null &&
                        result.getProject != null)
                    {
                        // Verify if project exists
                        bool projectExists = false;
                        var gp = await query.getProjects();

                        if (IsDisposed || Disposing)
                        {
                            return;
                        }

                        foreach (var project in gp.getProjects.projects)
                            {
                                if (project.name == result.getProject.name &&
                                    project.groupName == result.getProject.groupName)
                                {
                                    projectExists = true;
                                    break;
                                }
                            }

                            if (projectExists == true)
                            {
                                selectedProject = new ProjectItem()
                                {
                                    id = result.getProject.id,
                                    name = result.getProject.name,
                                    groupName = result.getProject.groupName
                                };

                                // Set Specpoint project name
                                textBoxSpecpointProject.Text = selectedProject.name;
                                previousValue = selectedProject.name;

                                SpecpointRegistry.SetValue("ProjectId", selectedProject.id);
                                SpecpointRegistry.SetValue("ProjectName", selectedProject.name);

                                // Use async-safe button update method
                                // Don't call SetButtonText directly from async form load
                                Globals.SetButtonTextAsync("Current Project", selectedProject.name);
                            }
                            else
                            {
                                string msg = string.Format("The linked project ({0}) has been removed or cannot be found. It may have been deleted or moved to a different location.\n\n", result.getProject.name) +
                                    "Please verify the project link or contact your administrator if you believe this is an error.";
                                MessageBox.Show(msg, "Project Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                SpecpointRegistry.SetValue("ProjectId", "");
                                SpecpointRegistry.SetValue("ProjectName", "");

                                // Use async-safe button update method
                                // Don't call SetButtonText directly from async form load
                                Globals.SetButtonTextAsync("Current Project", "No current project");
                                RebasedProject = true;
                            }

                        if (ExitRightAway && !IsDisposed && !Disposing)
                        {
                            DialogResult = DialogResult.OK;
                            Close();
                        }
                    }
                }

                // Enable OK and Remove
                if (!IsDisposed && !Disposing)
                {
                    EnableButtons();
                }
            }
            catch (TokenExpiredException)
            {
                DialogResult = DialogResult.Cancel;
                Close();

                Globals.OnTokenExpired();
            }
            catch (ObjectDisposedException)
            {
                // Form was disposed during async operation, safe to ignore
            }
        }

        private void EnableButtons()
        {
            buttonRemove.Enabled = textBoxSpecpointProject.Text != notSelected;
            buttonOK.Enabled = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxSpecpointProject.Text != previousValue)
            {
                if (textBoxSpecpointProject.Text != notSelected)
                {
                    SpecpointRegistry.SetValue("ProjectId", selectedProject.id);
                    SpecpointRegistry.SetValue("ProjectName", selectedProject.name);

                    // Safe to call SetButtonText directly from button click event
                    // Button click runs on UI thread in proper Revit context
                    Globals.SetButtonText("Current Project", selectedProject.name);
                }
                else
                {
                    SpecpointRegistry.SetValue("ProjectId", "");
                    SpecpointRegistry.SetValue("ProjectName", "");

                    // Safe to call SetButtonText directly from button click event
                    Globals.SetButtonText("Current Project", "No current project");
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}