namespace ThreadsAndDelegates
{
    partial class DirectorySearcherForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SearchLabel = new System.Windows.Forms.Label();
            this.searchText = new System.Windows.Forms.TextBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.directorySearcher = new ThreadsAndDelegates.DirectorySearcher();
            this.SuspendLayout();
            // 
            // searchLabel
            // 
            this.SearchLabel.ForeColor = System.Drawing.Color.Red;
            this.SearchLabel.Location = new System.Drawing.Point(102, 48);
            this.SearchLabel.Name = "searchLabel";
            this.SearchLabel.Size = new System.Drawing.Size(176, 16);
            this.SearchLabel.TabIndex = 7;
            // 
            // searchText
            // 
            this.searchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchText.Location = new System.Drawing.Point(102, 24);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(175, 20);
            this.searchText.TabIndex = 5;
            this.searchText.Text = "c:\\*.cs";
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(6, 16);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(88, 40);
            this.SearchButton.TabIndex = 4;
            this.SearchButton.Text = "&Search";
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click_1);
            // 
            // directorySearcher
            // 
            this.directorySearcher.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directorySearcher.Location = new System.Drawing.Point(6, 72);
            this.directorySearcher.Name = "directorySearcher";
            this.directorySearcher.SearchCriteria = null;
            this.directorySearcher.Size = new System.Drawing.Size(271, 173);
            this.directorySearcher.TabIndex = 6;
            // 
            // DirectorySearcherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.SearchLabel);
            this.Controls.Add(this.directorySearcher);
            this.Controls.Add(this.searchText);
            this.Controls.Add(this.SearchButton);
            this.Name = "DirectorySearcherForm";
            this.Text = "DirectorySearcherForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SearchLabel;
        private DirectorySearcher directorySearcher;
        private System.Windows.Forms.TextBox searchText;
        private System.Windows.Forms.Button SearchButton;
    }
}