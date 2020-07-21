namespace node_client.Forms {
    partial class SettingUpdate {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.buttonSettingSend = new System.Windows.Forms.Button();
            this.labelSettingNotes = new System.Windows.Forms.Label();
            this.comboBoxSettingsValues = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // buttonSettingSend
            // 
            this.buttonSettingSend.Location = new System.Drawing.Point(12, 134);
            this.buttonSettingSend.Name = "buttonSettingSend";
            this.buttonSettingSend.Size = new System.Drawing.Size(416, 23);
            this.buttonSettingSend.TabIndex = 0;
            this.buttonSettingSend.Text = "Send";
            this.buttonSettingSend.UseVisualStyleBackColor = true;
            this.buttonSettingSend.Click += new System.EventHandler(this.ButtonSettingSend_Click);
            // 
            // labelSettingNotes
            // 
            this.labelSettingNotes.AutoSize = true;
            this.labelSettingNotes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSettingNotes.Location = new System.Drawing.Point(9, 9);
            this.labelSettingNotes.Name = "labelSettingNotes";
            this.labelSettingNotes.Size = new System.Drawing.Size(39, 15);
            this.labelSettingNotes.TabIndex = 3;
            this.labelSettingNotes.Text = "Notes";
            // 
            // comboBoxSettingsValues
            // 
            this.comboBoxSettingsValues.FormattingEnabled = true;
            this.comboBoxSettingsValues.Location = new System.Drawing.Point(12, 107);
            this.comboBoxSettingsValues.Name = "comboBoxSettingsValues";
            this.comboBoxSettingsValues.Size = new System.Drawing.Size(416, 21);
            this.comboBoxSettingsValues.TabIndex = 4;
            // 
            // SettingUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 169);
            this.Controls.Add(this.comboBoxSettingsValues);
            this.Controls.Add(this.labelSettingNotes);
            this.Controls.Add(this.buttonSettingSend);
            this.Name = "SettingUpdate";
            this.Text = "SettingUpdate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSettingSend;
        private System.Windows.Forms.Label labelSettingNotes;
        private System.Windows.Forms.ComboBox comboBoxSettingsValues;
    }
}