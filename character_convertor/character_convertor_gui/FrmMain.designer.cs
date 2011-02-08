namespace CharacterConverter.Gui
{
    partial class FrmMain
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
			  this.label1 = new System.Windows.Forms.Label();
			  this.label2 = new System.Windows.Forms.Label();
			  this.label3 = new System.Windows.Forms.Label();
			  this.label4 = new System.Windows.Forms.Label();
			  this.label5 = new System.Windows.Forms.Label();
			  this._btnConvert = new System.Windows.Forms.Button();
			  this._tbHost = new System.Windows.Forms.TextBox();
			  this._tbPort = new System.Windows.Forms.TextBox();
			  this._tbBase = new System.Windows.Forms.TextBox();
			  this._tbUser = new System.Windows.Forms.TextBox();
			  this._tbPass = new System.Windows.Forms.TextBox();
			  this._lbInfo = new System.Windows.Forms.Label();
			  this.SuspendLayout();
			  // 
			  // label1
			  // 
			  this.label1.AutoSize = true;
			  this.label1.Location = new System.Drawing.Point(1, 5);
			  this.label1.Name = "label1";
			  this.label1.Size = new System.Drawing.Size(32, 13);
			  this.label1.TabIndex = 0;
			  this.label1.Text = "Host:";
			  // 
			  // label2
			  // 
			  this.label2.AutoSize = true;
			  this.label2.Location = new System.Drawing.Point(1, 31);
			  this.label2.Name = "label2";
			  this.label2.Size = new System.Drawing.Size(29, 13);
			  this.label2.TabIndex = 1;
			  this.label2.Text = "Port:";
			  // 
			  // label3
			  // 
			  this.label3.AutoSize = true;
			  this.label3.Location = new System.Drawing.Point(1, 56);
			  this.label3.Name = "label3";
			  this.label3.Size = new System.Drawing.Size(56, 13);
			  this.label3.TabIndex = 2;
			  this.label3.Text = "Database:";
			  // 
			  // label4
			  // 
			  this.label4.AutoSize = true;
			  this.label4.Location = new System.Drawing.Point(1, 82);
			  this.label4.Name = "label4";
			  this.label4.Size = new System.Drawing.Size(36, 13);
			  this.label4.TabIndex = 3;
			  this.label4.Text = "Login:";
			  // 
			  // label5
			  // 
			  this.label5.AutoSize = true;
			  this.label5.Location = new System.Drawing.Point(1, 108);
			  this.label5.Name = "label5";
			  this.label5.Size = new System.Drawing.Size(56, 13);
			  this.label5.TabIndex = 4;
			  this.label5.Text = "Password:";
			  // 
			  // _btnConvert
			  // 
			  this._btnConvert.Location = new System.Drawing.Point(4, 131);
			  this._btnConvert.Name = "_btnConvert";
			  this._btnConvert.Size = new System.Drawing.Size(159, 23);
			  this._btnConvert.TabIndex = 5;
			  this._btnConvert.Text = "Go!";
			  this._btnConvert.UseVisualStyleBackColor = true;
			  this._btnConvert.Click += new System.EventHandler(this.BtnConvert_Click);
			  // 
			  // _tbHost
			  // 
			  this._tbHost.Location = new System.Drawing.Point(63, 2);
			  this._tbHost.Name = "_tbHost";
			  this._tbHost.Size = new System.Drawing.Size(100, 20);
			  this._tbHost.TabIndex = 6;
			  // 
			  // _tbPort
			  // 
			  this._tbPort.Location = new System.Drawing.Point(63, 28);
			  this._tbPort.Name = "_tbPort";
			  this._tbPort.Size = new System.Drawing.Size(100, 20);
			  this._tbPort.TabIndex = 7;
			  // 
			  // _tbBase
			  // 
			  this._tbBase.Location = new System.Drawing.Point(63, 54);
			  this._tbBase.Name = "_tbBase";
			  this._tbBase.Size = new System.Drawing.Size(100, 20);
			  this._tbBase.TabIndex = 8;
			  // 
			  // _tbUser
			  // 
			  this._tbUser.Location = new System.Drawing.Point(63, 79);
			  this._tbUser.Name = "_tbUser";
			  this._tbUser.Size = new System.Drawing.Size(100, 20);
			  this._tbUser.TabIndex = 9;
			  // 
			  // _tbPass
			  // 
			  this._tbPass.Location = new System.Drawing.Point(63, 105);
			  this._tbPass.Name = "_tbPass";
			  this._tbPass.PasswordChar = '*';
			  this._tbPass.Size = new System.Drawing.Size(100, 20);
			  this._tbPass.TabIndex = 10;
			  // 
			  // _lbInfo
			  // 
			  this._lbInfo.AutoSize = true;
			  this._lbInfo.Location = new System.Drawing.Point(169, 5);
			  this._lbInfo.Name = "_lbInfo";
			  this._lbInfo.Size = new System.Drawing.Size(0, 13);
			  this._lbInfo.TabIndex = 11;
			  // 
			  // FrmMain
			  // 
			  this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			  this.ClientSize = new System.Drawing.Size(525, 154);
			  this.Controls.Add(this._lbInfo);
			  this.Controls.Add(this._tbPass);
			  this.Controls.Add(this._tbUser);
			  this.Controls.Add(this._tbBase);
			  this.Controls.Add(this._tbPort);
			  this.Controls.Add(this._tbHost);
			  this.Controls.Add(this._btnConvert);
			  this.Controls.Add(this.label5);
			  this.Controls.Add(this.label4);
			  this.Controls.Add(this.label3);
			  this.Controls.Add(this.label2);
			  this.Controls.Add(this.label1);
			  this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			  this.MaximizeBox = false;
			  this.Name = "FrmMain";
			  this.Text = "Character Converter Gui";
			  this.ResumeLayout(false);
			  this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button _btnConvert;
        private System.Windows.Forms.TextBox _tbHost;
        private System.Windows.Forms.TextBox _tbPort;
        private System.Windows.Forms.TextBox _tbBase;
        private System.Windows.Forms.TextBox _tbUser;
		  private System.Windows.Forms.TextBox _tbPass;
		  private System.Windows.Forms.Label _lbInfo;
    }
}

