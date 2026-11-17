using CapaEntidad;
using CapaNegocio;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario principal (contenedor) del sistema que gestiona la navegación,
    /// el control de acceso según los permisos del rol y la apertura de módulos hijos.
    /// </summary>
    public partial class Inicio : Form
    {
        private static Usuario usuarioActual;
        private IconMenuItem menuActivo = null;
        private Form formularioActivo = null;

        /// <summary>
        /// Inicializa la ventana principal recibiendo el usuario autenticado
        /// o asignando un perfil administrador por defecto en modo de prueba.
        /// </summary>
        public Inicio(Usuario objusuario = null)
        {
            if (objusuario == null)
                usuarioActual = new Usuario() { Nombre = "ADMIN", Apellido = "PREDEFINIDO", IdUsuario = 1 };
            else
                usuarioActual = objusuario;

            InitializeComponent();
        }

        /// <summary>
        /// Filtra la visibilidad de los menús y submenús según los permisos del usuario
        /// en la base de datos, ajusta el diseño visual y muestra el nombre del operador activo.
        /// </summary>
        private void Inicio_Load(object sender, EventArgs e)
        {
            this.Text = "Sistema de Ventas de Joyas";

            // Consultar los permisos habilitados para el usuario autenticado
            List<Permiso> ListaPermisos = new CN_Permiso().Listar(usuarioActual.IdUsuario);

            // Mostrar u ocultar opciones principales y submenús según coincidencia de nombre
            foreach (IconMenuItem iconmenu in menu.Items)
            {
                bool menuHabilitado = ListaPermisos.Any(m => m.NombreMenu == iconmenu.Name);
                iconmenu.Visible = menuHabilitado;

                if (menuHabilitado && iconmenu.HasDropDownItems)
                {
                    foreach (ToolStripItem subitem in iconmenu.DropDownItems)
                    {
                        if (subitem is ToolStripMenuItem)
                        {
                            subitem.Visible = ListaPermisos.Any(m => m.NombreMenu == subitem.Name);
                        }
                    }
                }
            }

            AjustarEspaciadoMenus();

            lblusuario.Text = usuarioActual.Nombre + " " + usuarioActual.Apellido;
        }

        /// <summary>
        /// Configura el tamaño automático, márgenes, rellenos y alineación de íconos
        /// para los elementos de la barra de menú superior y sus desplegables.
        /// </summary>
        private void AjustarEspaciadoMenus()
        {
            menu.AutoSize = true;
            menu.Padding = new Padding(10, 4, 10, 6);

            foreach (ToolStripItem item in menu.Items)
            {
                if (item is IconMenuItem iconmenu)
                {
                    iconmenu.AutoSize = true;
                    iconmenu.Margin = new Padding(6, 2, 16, 2);
                    iconmenu.Padding = new Padding(10, 6, 10, 10);
                    iconmenu.TextImageRelation = TextImageRelation.ImageAboveText;
                    iconmenu.ImageAlign = ContentAlignment.MiddleCenter;
                    iconmenu.TextAlign = ContentAlignment.BottomCenter;

                    if (iconmenu.HasDropDownItems)
                    {
                        foreach (ToolStripItem subitem in iconmenu.DropDownItems)
                        {
                            if (subitem is ToolStripMenuItem)
                            {
                                subitem.AutoSize = true;
                                subitem.Margin = new Padding(0, 2, 0, 2);
                                subitem.Padding = new Padding(6, 5, 12, 6);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cierra el formulario hijo activo (si existe) y embebe el nuevo formulario
        /// dentro del panel contenedor principal.
        /// </summary>
        private void AbrirFormulario(IconMenuItem menu, Form formularioHijo)
        {
            menuActivo = menu;

            if (formularioActivo != null)
            {
                formularioActivo.Close();
            }

            formularioActivo = formularioHijo;

            // Incrustar el formulario hijo sin bordes dentro del panel contenedor
            formularioHijo.TopLevel = false;
            formularioHijo.FormBorderStyle = FormBorderStyle.None;
            formularioHijo.Dock = DockStyle.Fill;
            formularioHijo.BackColor = Color.Thistle;

            contenedor.Controls.Add(formularioHijo);
            contenedor.Tag = formularioHijo;
            formularioHijo.BringToFront();
            formularioHijo.Show();
        }

        /// <summary>
        /// Sobrecarga para abrir un formulario hijo sin asociar un ítem de menú activo.
        /// </summary>
        private void AbrirFormulario(Form formularioHijo)
        {
            AbrirFormulario(null, formularioHijo);
        }

        /// <summary>
        /// Abre el módulo de gestión de usuarios.
        /// </summary>
        private void menuUsuario_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmUsuarios());
        }

        /// <summary>
        /// Abre el módulo de administración de categorías de productos.
        /// </summary>
        private void submenucategoria_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuConfiguracion, new frmCategoria());
        }

        /// <summary>
        /// Abre el módulo de administración del catálogo de productos.
        /// </summary>
        private void submenuproducto_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuConfiguracion, new frmProducto());
        }

        /// <summary>
        /// Abre el módulo de configuración de datos y logotipo del negocio.
        /// </summary>
        private void submenunegocio_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuConfiguracion, new frmNegocio());
        }

        /// <summary>
        /// Abre el formulario de registro de nuevas ventas enviando el usuario en sesión.
        /// </summary>
        private void submenuregistrarventa_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuVentas, new frmVentas(usuarioActual));
        }

        /// <summary>
        /// Abre el formulario de consulta de detalle de ventas registradas.
        /// </summary>
        private void submenuverdetalleventa_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuVentas, new frmDetalleVenta());
        }

        /// <summary>
        /// Abre el formulario de registro de nuevas compras enviando el usuario en sesión.
        /// </summary>
        private void submenuregistrarcompra_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuCompras, new frmCompras(usuarioActual));
        }

        /// <summary>
        /// Abre el formulario de consulta y exportación PDF de compras registradas.
        /// </summary>
        private void submenuverdetallecompra_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuCompras, new frmDetalleCompra());
        }

        /// <summary>
        /// Abre el módulo de gestión de clientes.
        /// </summary>
        private void menuClientes_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmClientes());
        }

        /// <summary>
        /// Abre el módulo de gestión de proveedores.
        /// </summary>
        private void menuProveedores_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmProveedores());
        }

        /// <summary>
        /// Abre el módulo de reportes de compras por período y proveedor.
        /// </summary>
        private void submenureportecompras_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuReportes, new frmReporteCompras());
        }

        /// <summary>
        /// Abre el módulo de reportes de ventas por período y tipo de comprobante.
        /// </summary>
        private void submenureporteventas_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuReportes, new frmReporteVentas());
        }

        /// <summary>
        /// Abre el módulo de consulta de auditoría e históricos de ventas y compras.
        /// </summary>
        private void submenuhistoricos_Click(object sender, EventArgs e)
        {
            AbrirFormulario(menuReportes, new frmHistoricos());
        }

        /// <summary>
        /// Abre el módulo de respaldo y restauración de la base de datos enviando el usuario en sesión.
        /// </summary>
        private void menuRespaldo_Click(object sender, EventArgs e)
        {
            AbrirFormulario((IconMenuItem)sender, new frmBackup(usuarioActual));
        }

        /// <summary>
        /// Evento de clic en la etiqueta de título (sin acción asociada).
        /// </summary>
        private void label1_Click(object sender, EventArgs e) { }

        /// <summary>
        /// Evento de clic en elementos de la barra superior de título (sin acción asociada).
        /// </summary>
        private void menuTitulo_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }
    }
}