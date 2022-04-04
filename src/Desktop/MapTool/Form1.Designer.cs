using System.Windows.Forms;

namespace Xunkong.Desktop.MapTool
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.titleBar = new System.Windows.Forms.Panel();
            this.button_seting = new System.Windows.Forms.Button();
            this.button_resize = new System.Windows.Forms.Button();
            this.button_close = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.titleBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // webView21
            // 
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            this.webView21.Location = new System.Drawing.Point(0, 0);
            this.webView21.Margin = new System.Windows.Forms.Padding(0);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(0, 0);
            this.webView21.TabIndex = 0;
            this.webView21.ZoomFactor = 1D;
            // 
            // titleBar
            // 
            this.titleBar.AutoSize = true;
            this.titleBar.BackColor = System.Drawing.SystemColors.Control;
            this.titleBar.Controls.Add(this.button_seting);
            this.titleBar.Controls.Add(this.button_resize);
            this.titleBar.Controls.Add(this.button_close);
            this.titleBar.Location = new System.Drawing.Point(0, 0);
            this.titleBar.Margin = new System.Windows.Forms.Padding(0);
            this.titleBar.Name = "titleBar";
            this.titleBar.Size = new System.Drawing.Size(797, 38);
            this.titleBar.TabIndex = 1;
            this.titleBar.DoubleClick += new System.EventHandler(this.titleBar_DoubleClick);
            this.titleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.titleBar_MouseDown);
            this.titleBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.titleBar_MouseMove);
            // 
            // button_seting
            // 
            this.button_seting.Font = new System.Drawing.Font("Segoe Fluent Icons", 12F);
            this.button_seting.Location = new System.Drawing.Point(0, 0);
            this.button_seting.Margin = new System.Windows.Forms.Padding(2);
            this.button_seting.Name = "button_seting";
            this.button_seting.Size = new System.Drawing.Size(36, 36);
            this.button_seting.TabIndex = 2;
            this.button_seting.Text = "";
            this.button_seting.UseVisualStyleBackColor = true;
            this.button_seting.Click += new System.EventHandler(this.button_seting_Click);
            // 
            // button_resize
            // 
            this.button_resize.Font = new System.Drawing.Font("Segoe Fluent Icons", 12F);
            this.button_resize.Location = new System.Drawing.Point(33, 0);
            this.button_resize.Margin = new System.Windows.Forms.Padding(2);
            this.button_resize.Name = "button_resize";
            this.button_resize.Size = new System.Drawing.Size(36, 36);
            this.button_resize.TabIndex = 1;
            this.button_resize.Text = "";
            this.button_resize.UseVisualStyleBackColor = true;
            this.button_resize.Click += new System.EventHandler(this.button_resize_Click);
            // 
            // button_close
            // 
            this.button_close.Font = new System.Drawing.Font("Segoe Fluent Icons", 12F);
            this.button_close.Location = new System.Drawing.Point(65, 0);
            this.button_close.Margin = new System.Windows.Forms.Padding(0);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(36, 36);
            this.button_close.TabIndex = 0;
            this.button_close.Text = "";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.titleBar);
            this.Controls.Add(this.webView21);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "寻空小地图";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.titleBar.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Panel titleBar;
        private Button button_close;
        private Button button_resize;
        private Button button_seting;
    }
}