namespace CapaPresentacion
{
    partial class frmBackup
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.grpSeguridad = new System.Windows.Forms.GroupBox();
            this.lblUsuarioInfo = new System.Windows.Forms.Label();
            this.lblClave = new System.Windows.Forms.Label();
            this.txtclave = new System.Windows.Forms.TextBox();
            this.chkverclave = new System.Windows.Forms.CheckBox();
            this.grpGenerar = new System.Windows.Forms.GroupBox();
            this.lblCarpeta = new System.Windows.Forms.Label();
            this.txtrutacarpeta = new System.Windows.Forms.TextBox();
            this.btnexaminarcarpeta = new System.Windows.Forms.Button();
            this.lblNotaPermisos = new System.Windows.Forms.Label();
            this.btngenerarbackup = new System.Windows.Forms.Button();
            this.grpRestaurar = new System.Windows.Forms.GroupBox();
            this.lblArchivo = new System.Windows.Forms.Label();
            this.txtrutaarchivo = new System.Windows.Forms.TextBox();
            this.btnexaminararchivo = new System.Windows.Forms.Button();
            this.lblAdvertencia = new System.Windows.Forms.Label();
            this.btnrestaurarbackup = new System.Windows.Forms.Button();
            this.grpSeguridad.SuspendLayout();
            this.grpGenerar.SuspendLayout();
            this.grpRestaurar.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.Location = new System.Drawing.Point(24, 18);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(356, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Respaldo y Restauración del Sistema";
            // 
            // grpSeguridad
            // 
            this.grpSeguridad.Controls.Add(this.chkverclave);
            this.grpSeguridad.Controls.Add(this.txtclave);
            this.grpSeguridad.Controls.Add(this.lblClave);
            this.grpSeguridad.Controls.Add(this.lblUsuarioInfo);
            this.grpSeguridad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpSeguridad.Location = new System.Drawing.Point(28, 56);
            this.grpSeguridad.Name = "grpSeguridad";
            this.grpSeguridad.Size = new System.Drawing.Size(760, 96);
            this.grpSeguridad.TabIndex = 1;
            this.grpSeguridad.TabStop = false;
            this.grpSeguridad.Text = "1. Autorización de Seguridad";
            // 
            // lblUsuarioInfo
            // 
            this.lblUsuarioInfo.AutoSize = true;
            this.lblUsuarioInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUsuarioInfo.Location = new System.Drawing.Point(18, 26);
            this.lblUsuarioInfo.Name = "lblUsuarioInfo";
            this.lblUsuarioInfo.Size = new System.Drawing.Size(156, 16);
            this.lblUsuarioInfo.TabIndex = 0;
            this.lblUsuarioInfo.Text = "Usuario Administrador:";
            // 
            // lblClave
            // 
            this.lblClave.AutoSize = true;
            this.lblClave.Location = new System.Drawing.Point(18, 58);
            this.lblClave.Name = "lblClave";
            this.lblClave.Size = new System.Drawing.Size(153, 16);
            this.lblClave.TabIndex = 1;
            this.lblClave.Text = "Ingrese su contraseña:";
            // 
            // txtclave
            // 
            this.txtclave.Location = new System.Drawing.Point(180, 55);
            this.txtclave.Name = "txtclave";
            this.txtclave.Size = new System.Drawing.Size(240, 22);
            this.txtclave.TabIndex = 2;
            this.txtclave.UseSystemPasswordChar = true;
            // 
            // chkverclave
            // 
            this.chkverclave.AutoSize = true;
            this.chkverclave.Location = new System.Drawing.Point(432, 57);
            this.chkverclave.Name = "chkverclave";
            this.chkverclave.Size = new System.Drawing.Size(141, 20);
            this.chkverclave.TabIndex = 3;
            this.chkverclave.Text = "Mostrar contraseña";
            this.chkverclave.UseVisualStyleBackColor = true;
            this.chkverclave.CheckedChanged += new System.EventHandler(this.chkverclave_CheckedChanged);
            // 
            // grpGenerar
            // 
            this.grpGenerar.Controls.Add(this.btngenerarbackup);
            this.grpGenerar.Controls.Add(this.lblNotaPermisos);
            this.grpGenerar.Controls.Add(this.btnexaminarcarpeta);
            this.grpGenerar.Controls.Add(this.txtrutacarpeta);
            this.grpGenerar.Controls.Add(this.lblCarpeta);
            this.grpGenerar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpGenerar.Location = new System.Drawing.Point(28, 168);
            this.grpGenerar.Name = "grpGenerar";
            this.grpGenerar.Size = new System.Drawing.Size(760, 152);
            this.grpGenerar.TabIndex = 2;
            this.grpGenerar.TabStop = false;
            this.grpGenerar.Text = "2. Generar Copia de Seguridad (.bak)";
            // 
            // lblCarpeta
            // 
            this.lblCarpeta.AutoSize = true;
            this.lblCarpeta.Location = new System.Drawing.Point(18, 30);
            this.lblCarpeta.Name = "lblCarpeta";
            this.lblCarpeta.Size = new System.Drawing.Size(129, 16);
            this.lblCarpeta.TabIndex = 0;
            this.lblCarpeta.Text = "Carpeta de destino:";
            // 
            // txtrutacarpeta
            // 
            this.txtrutacarpeta.Location = new System.Drawing.Point(21, 52);
            this.txtrutacarpeta.Name = "txtrutacarpeta";
            this.txtrutacarpeta.ReadOnly = true;
            this.txtrutacarpeta.Size = new System.Drawing.Size(585, 22);
            this.txtrutacarpeta.TabIndex = 1;
            this.txtrutacarpeta.Text = "C:\\Respaldos_Joyeria";
            // 
            // btnexaminarcarpeta
            // 
            this.btnexaminarcarpeta.Location = new System.Drawing.Point(618, 50);
            this.btnexaminarcarpeta.Name = "btnexaminarcarpeta";
            this.btnexaminarcarpeta.Size = new System.Drawing.Size(120, 26);
            this.btnexaminarcarpeta.TabIndex = 2;
            this.btnexaminarcarpeta.Text = "Examinar...";
            this.btnexaminarcarpeta.UseVisualStyleBackColor = true;
            this.btnexaminarcarpeta.Click += new System.EventHandler(this.btnexaminarcarpeta_Click);
            // 
            // lblNotaPermisos
            // 
            this.lblNotaPermisos.AutoSize = true;
            this.lblNotaPermisos.ForeColor = System.Drawing.Color.DimGray;
            this.lblNotaPermisos.Location = new System.Drawing.Point(18, 82);
            this.lblNotaPermisos.Name = "lblNotaPermisos";
            this.lblNotaPermisos.Size = new System.Drawing.Size(561, 16);
            this.lblNotaPermisos.TabIndex = 3;
            this.lblNotaPermisos.Text = "Se creará un archivo .bak con fecha y hora actual dentro de la carpeta seleccionada.";
            // 
            // btngenerarbackup
            // 
            this.btngenerarbackup.BackColor = System.Drawing.Color.ForestGreen;
            this.btngenerarbackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btngenerarbackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btngenerarbackup.ForeColor = System.Drawing.Color.White;
            this.btngenerarbackup.Location = new System.Drawing.Point(21, 108);
            this.btngenerarbackup.Name = "btngenerarbackup";
            this.btngenerarbackup.Size = new System.Drawing.Size(230, 32);
            this.btngenerarbackup.TabIndex = 4;
            this.btngenerarbackup.Text = "Generar Copia de Seguridad";
            this.btngenerarbackup.UseVisualStyleBackColor = false;
            this.btngenerarbackup.Click += new System.EventHandler(this.btngenerarbackup_Click);
            // 
            // grpRestaurar
            // 
            this.grpRestaurar.Controls.Add(this.btnrestaurarbackup);
            this.grpRestaurar.Controls.Add(this.lblAdvertencia);
            this.grpRestaurar.Controls.Add(this.btnexaminararchivo);
            this.grpRestaurar.Controls.Add(this.txtrutaarchivo);
            this.grpRestaurar.Controls.Add(this.lblArchivo);
            this.grpRestaurar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpRestaurar.Location = new System.Drawing.Point(28, 336);
            this.grpRestaurar.Name = "grpRestaurar";
            this.grpRestaurar.Size = new System.Drawing.Size(760, 158);
            this.grpRestaurar.TabIndex = 3;
            this.grpRestaurar.TabStop = false;
            this.grpRestaurar.Text = "3. Restaurar Copia de Seguridad (.bak)";
            // 
            // lblArchivo
            // 
            this.lblArchivo.AutoSize = true;
            this.lblArchivo.Location = new System.Drawing.Point(18, 30);
            this.lblArchivo.Name = "lblArchivo";
            this.lblArchivo.Size = new System.Drawing.Size(175, 16);
            this.lblArchivo.TabIndex = 0;
            this.lblArchivo.Text = "Archivo de respaldo (.bak):";
            // 
            // txtrutaarchivo
            // 
            this.txtrutaarchivo.Location = new System.Drawing.Point(21, 52);
            this.txtrutaarchivo.Name = "txtrutaarchivo";
            this.txtrutaarchivo.ReadOnly = true;
            this.txtrutaarchivo.Size = new System.Drawing.Size(585, 22);
            this.txtrutaarchivo.TabIndex = 1;
            // 
            // btnexaminararchivo
            // 
            this.btnexaminararchivo.Location = new System.Drawing.Point(618, 50);
            this.btnexaminararchivo.Name = "btnexaminararchivo";
            this.btnexaminararchivo.Size = new System.Drawing.Size(120, 26);
            this.btnexaminararchivo.TabIndex = 2;
            this.btnexaminararchivo.Text = "Seleccionar...";
            this.btnexaminararchivo.UseVisualStyleBackColor = true;
            this.btnexaminararchivo.Click += new System.EventHandler(this.btnexaminararchivo_Click);
            // 
            // lblAdvertencia
            // 
            this.lblAdvertencia.AutoSize = true;
            this.lblAdvertencia.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAdvertencia.Location = new System.Drawing.Point(18, 82);
            this.lblAdvertencia.Name = "lblAdvertencia";
            this.lblAdvertencia.Size = new System.Drawing.Size(603, 16);
            this.lblAdvertencia.TabIndex = 3;
            this.lblAdvertencia.Text = "Atención: Restaurar un respaldo reemplazará los datos actuales por los del archivo seleccionado.";
            // 
            // btnrestaurarbackup
            // 
            this.btnrestaurarbackup.BackColor = System.Drawing.Color.Firebrick;
            this.btnrestaurarbackup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnrestaurarbackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnrestaurarbackup.ForeColor = System.Drawing.Color.White;
            this.btnrestaurarbackup.Location = new System.Drawing.Point(21, 110);
            this.btnrestaurarbackup.Name = "btnrestaurarbackup";
            this.btnrestaurarbackup.Size = new System.Drawing.Size(230, 32);
            this.btnrestaurarbackup.TabIndex = 4;
            this.btnrestaurarbackup.Text = "Restaurar Base de Datos";
            this.btnrestaurarbackup.UseVisualStyleBackColor = false;
            this.btnrestaurarbackup.Click += new System.EventHandler(this.btnrestaurarbackup_Click);
            // 
            // frmBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(830, 520);
            this.Controls.Add(this.grpRestaurar);
            this.Controls.Add(this.grpGenerar);
            this.Controls.Add(this.grpSeguridad);
            this.Controls.Add(this.lblTitulo);
            this.Name = "frmBackup";
            this.Text = "Respaldo del Sistema";
            this.Load += new System.EventHandler(this.frmBackup_Load);
            this.grpSeguridad.ResumeLayout(false);
            this.grpSeguridad.PerformLayout();
            this.grpGenerar.ResumeLayout(false);
            this.grpGenerar.PerformLayout();
            this.grpRestaurar.ResumeLayout(false);
            this.grpRestaurar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.GroupBox grpSeguridad;
        private System.Windows.Forms.Label lblUsuarioInfo;
        private System.Windows.Forms.Label lblClave;
        private System.Windows.Forms.TextBox txtclave;
        private System.Windows.Forms.CheckBox chkverclave;
        private System.Windows.Forms.GroupBox grpGenerar;
        private System.Windows.Forms.Label lblCarpeta;
        private System.Windows.Forms.TextBox txtrutacarpeta;
        private System.Windows.Forms.Button btnexaminarcarpeta;
        private System.Windows.Forms.Label lblNotaPermisos;
        private System.Windows.Forms.Button btngenerarbackup;
        private System.Windows.Forms.GroupBox grpRestaurar;
        private System.Windows.Forms.Label lblArchivo;
        private System.Windows.Forms.TextBox txtrutaarchivo;
        private System.Windows.Forms.Button btnexaminararchivo;
        private System.Windows.Forms.Label lblAdvertencia;
        private System.Windows.Forms.Button btnrestaurarbackup;
    }
}