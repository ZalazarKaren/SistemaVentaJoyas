using CapaEntidad;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario para consultar comprobantes de venta registrados por número de documento
    /// y visualizar el detalle de productos vendidos, montos abonados y vuelto entregado.
    /// </summary>
    public partial class frmDetalleVenta : Form
    {
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa los componentes visuales del formulario.
        /// </summary>
        public frmDetalleVenta()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configura los campos informativos en modo solo lectura, ajusta la alineación
        /// de las columnas de la grilla y prepara el buscador al abrir el formulario.
        /// </summary>
        private void frmDetalleVenta_Load(object sender, EventArgs e)
        {
            // Bloquear los campos informativos para evitar edición manual
            TextBox[] camposSoloLectura = {
                txtnumerodocumento, txtfecha, txttipodocumento, txtusuario,
                txtdoccliente, txtnombrecliente, txtmontototal, txtmontopago, txtmontocambio
            };

            foreach (TextBox txt in camposSoloLectura)
            {
                if (txt != null)
                {
                    txt.ReadOnly = true;
                    txt.TabStop = false;
                    txt.BackColor = Color.WhiteSmoke;
                }
            }

            // Configurar la grilla para evitar edición y ajustar el ancho de columnas numéricas
            dgvdata.ReadOnly = true;
            dgvdata.AllowUserToAddRows = false;

            if (dgvdata.Columns.Count >= 4)
            {
                dgvdata.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                dgvdata.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dgvdata.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Permitir ejecutar la búsqueda presionando Enter sin modificar el diseñador
            txtbusqueda.KeyDown += txtbusqueda_KeyDown;

            LimpiarCampos();
            txtbusqueda.Select();
        }

        /// <summary>
        /// Convierte un monto decimal a texto con separador de miles en formato argentino (ej. 15.000).
        /// </summary>
        private string FormatearMonedaAR(decimal valor)
        {
            return valor.ToString("N0", culturaAR);
        }

        /// <summary>
        /// Detecta la tecla Enter en el cuadro de búsqueda y ejecuta el botón Buscar.
        /// </summary>
        private void txtbusqueda_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnbuscar.PerformClick();
            }
        }

        /// <summary>
        /// Busca la venta por su número de comprobante y muestra su cabecera, productos y montos en pantalla.
        /// </summary>
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string busqueda = txtbusqueda.Text.Trim();

            if (string.IsNullOrEmpty(busqueda))
            {
                MessageBox.Show("Ingrese un número de documento para buscar.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtbusqueda.Select();
                return;
            }

            // Si el usuario ingresa un número corto (ej: "1"), completarlo con ceros a 5 dígitos ("00001")
            if (busqueda.All(char.IsDigit) && busqueda.Length < 5)
            {
                busqueda = busqueda.PadLeft(5, '0');
                txtbusqueda.Text = busqueda;
            }

            // Consultar cabecera y detalle de la venta en la capa de negocio
            Venta oVenta = new CN_Venta().ObtenerVenta(busqueda);

            if (oVenta != null && oVenta.IdVenta != 0)
            {
                // Cargar datos de cabecera y cliente
                txtnumerodocumento.Text = oVenta.NumeroDocumento;
                txtfecha.Text = oVenta.FechaRegistro;
                txttipodocumento.Text = oVenta.TipoDocumento;

                txtusuario.Text = oVenta.oUsuario != null
                    ? (oVenta.oUsuario.Nombre + " " + oVenta.oUsuario.Apellido).Trim()
                    : "";

                txtdoccliente.Text = oVenta.DocumentoCliente;
                txtnombrecliente.Text = oVenta.NombreCliente;

                // Cargar ítems en la grilla de detalle
                dgvdata.Rows.Clear();

                if (oVenta.oDetalleVenta != null)
                {
                    foreach (Detalle_Venta dv in oVenta.oDetalleVenta)
                    {
                        dgvdata.Rows.Add(new object[] {
                            dv.oProducto.Nombre,
                            FormatearMonedaAR(dv.PrecioVenta),
                            dv.Cantidad,
                            FormatearMonedaAR(dv.SubTotal)
                        });
                    }
                }

                // Mostrar totales de la operación
                txtmontototal.Text = FormatearMonedaAR(oVenta.MontoTotal);
                txtmontopago.Text = FormatearMonedaAR(oVenta.MontoPago);
                txtmontocambio.Text = FormatearMonedaAR(oVenta.MontoCambio);
            }
            else
            {
                LimpiarCampos();
                MessageBox.Show("No se encontró ninguna venta con ese número de documento.", "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtbusqueda.Select();
            }
        }

        /// <summary>
        /// Vacía todos los campos informativos, limpia la grilla y reinicia los montos a cero.
        /// </summary>
        private void LimpiarCampos()
        {
            txtnumerodocumento.Text = "";
            txtfecha.Text = "";
            txttipodocumento.Text = "";
            txtusuario.Text = "";
            txtdoccliente.Text = "";
            txtnombrecliente.Text = "";

            dgvdata.Rows.Clear();
            txtmontototal.Text = "0";
            txtmontopago.Text = "0";
            txtmontocambio.Text = "0";
        }

        /// <summary>
        /// Limpia el filtro de búsqueda, restablece el formulario y devuelve el foco al buscador.
        /// </summary>
        private void btnborrar_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            LimpiarCampos();
            txtbusqueda.Select();
        }
    }
}