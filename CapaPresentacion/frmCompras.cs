using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Modales;
using CapaPresentacion.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario principal para el registro de compras a proveedores,
    /// selección de productos, cálculo de totales y actualización de inventario.
    /// </summary>
    public partial class frmCompras : Form
    {
        private Usuario _Usuario;
        private Image imagenDelete;
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa el formulario recibiendo el usuario autenticado y cargando el ícono de borrado de la grilla.
        /// </summary>
        public frmCompras(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();

            // Cargar la imagen del botón eliminar desde la carpeta Images del proyecto
            string carpetaImagenes = Path.Combine(
                Directory.GetParent(Application.StartupPath).Parent.Parent.FullName,
                "CapaPresentacion",
                "Images"
            );
            imagenDelete = Image.FromFile(Path.Combine(carpetaImagenes, "delete20.png"));
        }

        /// <summary>
        /// Configura los valores iniciales, campos de solo lectura, alineación de columnas
        /// y eventos de formateo de moneda al abrir el formulario.
        /// </summary>
        private void frmCompras_Load(object sender, EventArgs e)
        {
            // Cargar tipos de comprobante disponibles
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Boleta", Texto = "Boleta" });
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Factura", Texto = "Factura" });
            cbotipodocumento.DisplayMember = "Texto";
            cbotipodocumento.ValueMember = "Valor";
            cbotipodocumento.SelectedIndex = 0;

            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtidproveedor.Text = "0";
            txtidproducto.Text = "0";
            txttotalpagar.Text = "0";

            // Bloqueamos los campos que no deben editarse manualmente
            txtfecha.ReadOnly = true;
            txtfecha.TabStop = false;

            txtdocproveedor.ReadOnly = true;
            txtdocproveedor.TabStop = false;
            txtdocproveedor.BackColor = Color.WhiteSmoke;

            txtnombreproveedor.ReadOnly = true;
            txtnombreproveedor.TabStop = false;
            txtnombreproveedor.BackColor = Color.WhiteSmoke;

            txtproducto.ReadOnly = true;
            txtproducto.TabStop = false;
            txtproducto.BackColor = Color.WhiteSmoke;

            txttotalpagar.ReadOnly = true;
            txttotalpagar.TabStop = false;
            txttotalpagar.BackColor = Color.WhiteSmoke;

            // Ajuste automático y alineación derecha de columnas numéricas en la grilla
            string[] columnasMoneda = { "PrecioCompra", "PrecioVenta", "SubTotal" };
            foreach (string col in columnasMoneda)
            {
                if (dgvdata.Columns[col] != null)
                {
                    dgvdata.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dgvdata.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            // Formateo visual al salir de los cuadros de precio
            txtpreciocompra.Leave += (s, ev) =>
            {
                decimal val = ParsearMonedaAR(txtpreciocompra.Text);
                if (val > 0) txtpreciocompra.Text = FormatearMonedaAR(val);
            };
            txtprecioventa.Leave += (s, ev) =>
            {
                decimal val = ParsearMonedaAR(txtprecioventa.Text);
                if (val > 0) txtprecioventa.Text = FormatearMonedaAR(val);
            };
        }

        /// <summary>
        /// Convierte un valor decimal a texto con separador de miles en formato argentino (ej. 15.000).
        /// </summary>
        private string FormatearMonedaAR(decimal valor)
        {
            return valor.ToString("N0", culturaAR);
        }

        /// <summary>
        /// Convierte un texto numérico formateado a su valor decimal equivalente.
        /// </summary>
        private decimal ParsearMonedaAR(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return 0m;
            decimal valor;
            if (decimal.TryParse(texto.Trim(), NumberStyles.Number, culturaAR, out valor))
                return valor;
            return 0m;
        }

        /// <summary>
        /// Abre el modal de proveedores y completa los datos del proveedor seleccionado.
        /// </summary>
        private void btnbuscarproveedor_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProveedor())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtidproveedor.Text = modal._Proveedor.IdProveedor.ToString();
                    txtdocproveedor.Text = modal._Proveedor.Documento;
                    txtnombreproveedor.Text = modal._Proveedor.RazonSocial;
                    txtcodproducto.Select();
                }
                else
                {
                    txtdocproveedor.Select();
                }
            }
        }

        /// <summary>
        /// Abre el modal de productos y carga los datos y precios actuales del artículo elegido.
        /// </summary>
        private void btnbuscarproducto_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProducto())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtcodproducto.BackColor = Color.Honeydew;
                    txtidproducto.Text = modal._Producto.IdProducto.ToString();
                    txtcodproducto.Text = modal._Producto.Codigo;
                    txtproducto.Text = modal._Producto.Nombre;
                    txtpreciocompra.Text = modal._Producto.PrecioCompra > 0 ? FormatearMonedaAR(modal._Producto.PrecioCompra) : "";
                    txtprecioventa.Text = modal._Producto.PrecioVenta > 0 ? FormatearMonedaAR(modal._Producto.PrecioVenta) : "";
                    txtcantidad.Value = 1;
                    txtpreciocompra.Select();
                }
                else
                {
                    txtcodproducto.Select();
                }
            }
        }

        /// <summary>
        /// Busca automáticamente un producto activo por su código al presionar la tecla Enter.
        /// </summary>
        private void txtcodproducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string codigoBuscado = txtcodproducto.Text.Trim();
                Producto oProducto = new CN_Producto().Listar()
                    .Where(p => p.Codigo.Trim().Equals(codigoBuscado, StringComparison.OrdinalIgnoreCase) && p.Estado == true)
                    .FirstOrDefault();

                if (oProducto != null)
                {
                    // Producto encontrado: resaltar en verde y cargar sus datos
                    txtcodproducto.BackColor = Color.Honeydew;
                    txtidproducto.Text = oProducto.IdProducto.ToString();
                    txtproducto.Text = oProducto.Nombre;
                    txtpreciocompra.Text = oProducto.PrecioCompra > 0 ? FormatearMonedaAR(oProducto.PrecioCompra) : "";
                    txtprecioventa.Text = oProducto.PrecioVenta > 0 ? FormatearMonedaAR(oProducto.PrecioVenta) : "";
                    txtcantidad.Value = 1;
                    txtpreciocompra.Select();
                }
                else
                {
                    // Producto no encontrado: resaltar en rojo y limpiar campos
                    txtcodproducto.BackColor = Color.MistyRose;
                    txtidproducto.Text = "0";
                    txtproducto.Text = "";
                    txtpreciocompra.Text = "";
                    txtprecioventa.Text = "";
                    txtcantidad.Value = 1;
                }
            }
        }

        /// <summary>
        /// Valida los datos del producto, verifica márgenes y duplicados, y lo agrega a la grilla de compra.
        /// </summary>
        private void btnagregarproducto_Click(object sender, EventArgs e)
        {
            decimal preciocompra = 0;
            decimal precioventa = 0;
            bool producto_existe = false;

            // 1. Validar selección de producto y formato numérico de precios
            if (int.Parse(txtidproducto.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txtpreciocompra.Text.Trim(), NumberStyles.Number, culturaAR, out preciocompra) || preciocompra <= 0)
            {
                MessageBox.Show("Precio Compra - Formato moneda incorrecto o mayor a cero requerido", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtpreciocompra.Select();
                return;
            }

            if (!decimal.TryParse(txtprecioventa.Text.Trim(), NumberStyles.Number, culturaAR, out precioventa) || precioventa <= 0)
            {
                MessageBox.Show("Precio Venta - Formato moneda incorrecto o mayor a cero requerido", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtprecioventa.Select();
                return;
            }

            // 2. Advertir si el precio de venta es menor al costo de compra
            if (precioventa < preciocompra)
            {
                var confirmar = MessageBox.Show(
                    "El precio de venta es menor al precio de compra. ¿Desea agregarlo de todas formas?",
                    "Advertencia de margen",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );
                if (confirmar == DialogResult.No)
                {
                    txtprecioventa.Select();
                    return;
                }
            }

            int cantidad = Convert.ToInt32(txtcantidad.Value);
            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 3. Evitar que el mismo producto se cargue dos veces en el detalle
            foreach (DataGridViewRow fila in dgvdata.Rows)
            {
                if (fila.Cells["IdProducto"].Value.ToString() == txtidproducto.Text)
                {
                    producto_existe = true;
                    break;
                }
            }

            if (producto_existe)
            {
                MessageBox.Show("Este producto ya fue agregado a la compra. Si desea modificar la cantidad o el precio, elimínelo de la lista y agréguelo nuevamente.", "Producto duplicado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 4. Calcular subtotal, insertar fila en la grilla y actualizar total general
            decimal subTotal = cantidad * preciocompra;

            dgvdata.Rows.Add(new object[] {
                txtidproducto.Text,
                txtproducto.Text,
                FormatearMonedaAR(preciocompra),
                FormatearMonedaAR(precioventa),
                cantidad.ToString(),
                FormatearMonedaAR(subTotal)
            });

            calcularTotal();
            limpiarProducto();
            txtcodproducto.Select();
        }

        /// <summary>
        /// Restablece los controles de selección de producto a sus valores por defecto.
        /// </summary>
        private void limpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.BackColor = SystemColors.Window;
            txtproducto.Text = "";
            txtpreciocompra.Text = "";
            txtprecioventa.Text = "";
            txtcantidad.Value = 1;
        }

        /// <summary>
        /// Suma los subtotales de todos los ítems cargados en la grilla y actualiza el total a pagar.
        /// </summary>
        private void calcularTotal()
        {
            decimal total = 0;
            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    total += ParsearMonedaAR(row.Cells["SubTotal"].Value.ToString());
                }
            }
            txttotalpagar.Text = FormatearMonedaAR(total);
        }

        /// <summary>
        /// Dibuja el ícono de eliminación centrado dentro de la celda correspondiente en cada fila.
        /// </summary>
        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvdata.Columns["btneliminar"].Index)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                int ancho = 20;
                int alto = 20;
                int x = e.CellBounds.Left + (e.CellBounds.Width - ancho) / 2;
                int y = e.CellBounds.Top + (e.CellBounds.Height - alto) / 2;

                e.Graphics.DrawImage(imagenDelete, new Rectangle(x, y, ancho, alto));
                e.Handled = true;
            }
        }

        /// <summary>
        /// Elimina el producto seleccionado de la grilla al hacer clic en el botón de borrado y recalcula el total.
        /// </summary>
        private void dgvdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgvdata.Columns["btneliminar"].Index)
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    dgvdata.Rows.RemoveAt(index);
                    calcularTotal();
                }
            }
        }

        /// <summary>
        /// Restringe la entrada de teclado en el precio de compra para permitir solo dígitos y separadores numéricos.
        /// </summary>
        private void txtpreciocompra_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if ((e.KeyChar == '.' || e.KeyChar == ',') && txtpreciocompra.Text.Trim().Length > 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Restringe la entrada de teclado en el precio de venta para permitir solo dígitos y separadores numéricos.
        /// </summary>
        private void txtprecioventa_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if ((e.KeyChar == '.' || e.KeyChar == ',') && txtprecioventa.Text.Trim().Length > 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Valida la compra completa, construye la tabla de detalle y registra la operación en la base de datos.
        /// </summary>
        private void btnregistrar_Click(object sender, EventArgs e)
        {
            // 1. Verificar que haya un proveedor seleccionado y al menos un producto en la lista
            if (Convert.ToInt32(txtidproveedor.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un proveedor", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la compra", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Armar el DataTable con la estructura esperada por el parámetro tipo tabla en SQL Server
            DataTable detalle_compra = new DataTable();
            detalle_compra.Columns.Add("IdProducto", typeof(int));
            detalle_compra.Columns.Add("PrecioCompra", typeof(decimal));
            detalle_compra.Columns.Add("PrecioVenta", typeof(decimal));
            detalle_compra.Columns.Add("Cantidad", typeof(int));
            detalle_compra.Columns.Add("MontoTotal", typeof(decimal));

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                detalle_compra.Rows.Add(
                    new object[] {
                        Convert.ToInt32(row.Cells["IdProducto"].Value),
                        ParsearMonedaAR(row.Cells["PrecioCompra"].Value.ToString()),
                        ParsearMonedaAR(row.Cells["PrecioVenta"].Value.ToString()),
                        Convert.ToInt32(row.Cells["Cantidad"].Value),
                        ParsearMonedaAR(row.Cells["SubTotal"].Value.ToString())
                    });
            }

            // 3. Generar el número correlativo y construir el objeto cabecera Compra
            int idcorrelativo = new CN_Compra().ObtenerCorrelativo();
            string numerodocumento = string.Format("{0:00000}", idcorrelativo);

            Compra oCompra = new Compra()
            {
                oUsuario = new Usuario() { IdUsuario = _Usuario != null ? _Usuario.IdUsuario : 1 },
                oProveedor = new Proveedor() { IdProveedor = Convert.ToInt32(txtidproveedor.Text) },
                TipoDocumento = ((OpcionCombo)cbotipodocumento.SelectedItem).Texto,
                NumeroDocumento = numerodocumento,
                MontoTotal = ParsearMonedaAR(txttotalpagar.Text)
            };

            // 4. Enviar la compra a la capa de negocio y limpiar el formulario si tuvo éxito
            string mensaje = string.Empty;
            bool respuesta = new CN_Compra().Registrar(oCompra, detalle_compra, out mensaje);

            if (respuesta)
            {
                var result = MessageBox.Show("Numero de compra generada:\n" + numerodocumento + "\n\n¿Desea copiar al portapapeles?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    Clipboard.SetText(numerodocumento);

                txtidproveedor.Text = "0";
                txtdocproveedor.Text = "";
                txtnombreproveedor.Text = "";
                dgvdata.Rows.Clear();
                calcularTotal();
                limpiarProducto();
            }
            else
            {
                MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}