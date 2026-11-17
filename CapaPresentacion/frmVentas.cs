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
    /// Formulario principal para el registro de ventas a clientes, reserva preventiva
    /// de stock en tiempo real, cálculo de totales y vuelto.
    /// </summary>
    public partial class frmVentas : Form
    {
        private Usuario _Usuario;
        private Image imagenDelete;
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa el formulario recibiendo el usuario autenticado y cargando el ícono de borrado de la grilla.
        /// </summary>
        public frmVentas(Usuario oUsuario = null)
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
        /// Configura los tipos de comprobante, inicializa valores por defecto, bloquea campos
        /// calculados o de consulta, alinea columnas monetarias y enlaza el evento de cierre.
        /// </summary>
        private void frmVentas_Load(object sender, EventArgs e)
        {
            // Cargar tipos de comprobante disponibles
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Boleta", Texto = "Boleta" });
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Factura", Texto = "Factura" });
            cbotipodocumento.DisplayMember = "Texto";
            cbotipodocumento.ValueMember = "Valor";
            cbotipodocumento.SelectedIndex = 0;

            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtidproducto.Text = "0";

            txtpagocon.Text = "";
            txtcambio.Text = "";
            txttotalpagar.Text = "0";

            // Bloqueamos los campos calculados y el Stock que viene de la BD
            txtstock.ReadOnly = true;
            txtstock.TabStop = false;
            txtstock.BackColor = Color.WhiteSmoke;

            txtproducto.ReadOnly = true;
            txtproducto.TabStop = false;
            txtproducto.BackColor = Color.WhiteSmoke;

            txtcambio.ReadOnly = true;
            txtcambio.TabStop = false;
            txtcambio.BackColor = Color.WhiteSmoke;

            txtfecha.ReadOnly = true;
            txtfecha.TabStop = false;

            // Evita que la columna Precio o SubTotal se corte con "..."
            if (dgvdata.Columns["Precio"] != null)
            {
                dgvdata.Columns["Precio"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (dgvdata.Columns["SubTotal"] != null)
            {
                dgvdata.Columns["SubTotal"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvdata.Columns["SubTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // Si el usuario cierra la ventana sin concretar la venta, se devuelve el stock descontado
            this.FormClosing += frmVentas_FormClosing;
        }

        /// <summary>
        /// Convierte un monto decimal a texto con separador de miles en formato argentino (ej. 15.000).
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
        /// Restaura en la base de datos el stock previamente descontado si el usuario
        /// cierra el formulario teniendo productos pendientes sin registrar.
        /// </summary>
        private void frmVentas_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow fila in dgvdata.Rows)
                {
                    new CN_Venta().SumarStock(
                        Convert.ToInt32(fila.Cells["IdProducto"].Value),
                        Convert.ToInt32(fila.Cells["Cantidad"].Value)
                    );
                }
            }
        }

        /// <summary>
        /// Abre el modal de clientes y completa los datos del cliente seleccionado.
        /// </summary>
        private void btnbuscarcliente_Click(object sender, EventArgs e)
        {
            using (var modal = new mdCliente())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtdocumentocliente.Text = modal._Cliente.Documento;
                    txtnombrecliente.Text = modal._Cliente.Nombre + " " + modal._Cliente.Apellido;
                    txtcodproducto.Select();
                }
                else
                {
                    txtdocumentocliente.Select();
                }
            }
        }

        /// <summary>
        /// Abre el modal de productos y carga los datos, precio de venta y stock disponible del artículo elegido.
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
                    txtprecio.Text = FormatearMonedaAR(modal._Producto.PrecioVenta);
                    txtstock.Text = modal._Producto.Stock.ToString();
                    txtcantidad.Value = 1;
                    txtcantidad.Select();
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
                    // Producto encontrado: resaltar en verde y cargar sus datos y stock actual
                    txtcodproducto.BackColor = Color.Honeydew;
                    txtidproducto.Text = oProducto.IdProducto.ToString();
                    txtproducto.Text = oProducto.Nombre;
                    txtprecio.Text = FormatearMonedaAR(oProducto.PrecioVenta);
                    txtstock.Text = oProducto.Stock.ToString();
                    txtcantidad.Value = 1;
                    txtcantidad.Select();
                }
                else
                {
                    // Producto no encontrado: resaltar en rojo y limpiar campos
                    txtcodproducto.BackColor = Color.MistyRose;
                    txtidproducto.Text = "0";
                    txtproducto.Text = "";
                    txtprecio.Text = "";
                    txtstock.Text = "";
                    txtcantidad.Value = 1;
                }
            }
        }

        /// <summary>
        /// Valida el producto, verifica disponibilidad de stock y duplicados, descuenta
        /// preventivamente el stock en la base de datos y agrega la fila a la grilla.
        /// </summary>
        private void btnagregarproducto_Click(object sender, EventArgs e)
        {
            decimal precio = 0;
            int stockDisponible = 0;
            bool producto_existe = false;

            // 1. Validar selección de producto, formato de precio y disponibilidad de stock
            if (int.Parse(txtidproducto.Text) == 0)
            {
                MessageBox.Show("Debe seleccionar un producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!decimal.TryParse(txtprecio.Text.Trim(), NumberStyles.Number, culturaAR, out precio) || precio <= 0)
            {
                MessageBox.Show("Precio - Formato moneda incorrecto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtprecio.Select();
                return;
            }

            if (!int.TryParse(txtstock.Text, out stockDisponible) || stockDisponible <= 0)
            {
                MessageBox.Show("El producto seleccionado no cuenta con stock disponible.", "Sin Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cantidadSolicitada = Convert.ToInt32(txtcantidad.Value);
            if (cantidadSolicitada <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (stockDisponible < cantidadSolicitada)
            {
                MessageBox.Show($"La cantidad no puede ser mayor al stock disponible ({stockDisponible}).", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Evitar agregar dos veces el mismo artículo en el detalle
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
                MessageBox.Show("Este producto ya fue agregado a la venta. Si desea modificar la cantidad, elimínelo de la lista y agréguelo nuevamente.", "Producto duplicado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. Descontar preventivamente el stock en BD e insertar la fila en la grilla
            bool respuesta = new CN_Venta().RestarStock(
                Convert.ToInt32(txtidproducto.Text),
                cantidadSolicitada
            );

            if (respuesta)
            {
                decimal subTotal = cantidadSolicitada * precio;

                dgvdata.Rows.Add(new object[] {
                    txtidproducto.Text,
                    txtproducto.Text,
                    FormatearMonedaAR(precio),
                    cantidadSolicitada.ToString(),
                    FormatearMonedaAR(subTotal)
                });

                calcularTotal();
                limpiarProducto();
                txtcodproducto.Select();
            }
            else
            {
                MessageBox.Show("No se pudo descontar el stock en la base de datos. Verifique la disponibilidad.", "Error de Stock", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        /// Restablece los controles de selección de producto a sus valores por defecto.
        /// </summary>
        private void limpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.BackColor = SystemColors.Window;
            txtproducto.Text = "";
            txtprecio.Text = "";
            txtstock.Text = "";
            txtcantidad.Value = 1;
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
        /// Devuelve el stock reservado a la base de datos, elimina la fila de la grilla y recalcula el total.
        /// </summary>
        private void dgvdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvdata.Columns[e.ColumnIndex].Name == "btneliminar")
            {
                int index = e.RowIndex;

                if (index >= 0)
                {
                    bool respuesta = new CN_Venta().SumarStock(
                        Convert.ToInt32(dgvdata.Rows[index].Cells["IdProducto"].Value.ToString()),
                        Convert.ToInt32(dgvdata.Rows[index].Cells["Cantidad"].Value.ToString())
                    );

                    if (respuesta)
                    {
                        dgvdata.Rows.RemoveAt(index);
                        calcularTotal();
                    }
                }
            }
        }

        /// <summary>
        /// Restringe la entrada de teclado en el precio para permitir solo dígitos y separadores numéricos.
        /// </summary>
        private void txtprecio_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if ((e.KeyChar == '.' || e.KeyChar == ',') && txtprecio.Text.Trim().Length > 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Restringe la entrada de teclado en el monto abonado para permitir solo dígitos y separadores numéricos.
        /// </summary>
        private void txtpagocon_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if ((e.KeyChar == '.' || e.KeyChar == ',') && txtpagocon.Text.Trim().Length > 0)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Formatea el monto abonado por el cliente y calcula el vuelto respecto al total de la venta.
        /// </summary>
        private void calcularcambio()
        {
            if (txttotalpagar.Text.Trim() == "")
            {
                MessageBox.Show("No existen productos en la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            decimal pagacon = 0;
            decimal total = ParsearMonedaAR(txttotalpagar.Text);

            if (txtpagocon.Text.Trim() == "")
            {
                txtpagocon.Text = "0";
            }

            if (decimal.TryParse(txtpagocon.Text.Trim(), NumberStyles.Number, culturaAR, out pagacon))
            {
                txtpagocon.Text = FormatearMonedaAR(pagacon);

                if (pagacon < total)
                {
                    txtcambio.Text = "0";
                }
                else
                {
                    decimal cambio = pagacon - total;
                    txtcambio.Text = FormatearMonedaAR(cambio);
                }
            }
        }

        /// <summary>
        /// Ejecuta el cálculo del vuelto al presionar la tecla Enter en el cuadro de pago.
        /// </summary>
        private void txtpagocon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                calcularcambio();
            }
        }

        /// <summary>
        /// Valida los datos del cliente según el tipo de comprobante, verifica que el pago cubra
        /// el total, arma el detalle y registra la venta en la base de datos.
        /// </summary>
        private void btncrearventa_Click(object sender, EventArgs e)
        {
            string tipoDoc = ((OpcionCombo)cbotipodocumento.SelectedItem).Texto;
            string docCliente = txtdocumentocliente.Text.Trim();
            string nomCliente = txtnombrecliente.Text.Trim();

            // 1. Validar datos del cliente: obligatorios para Factura, opcionales para Boleta
            if (tipoDoc == "Factura")
            {
                if (docCliente == "" || nomCliente == "")
                {
                    MessageBox.Show("Para emitir Factura debe ingresar documento y nombre del cliente.", "Datos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                if (docCliente == "") docCliente = "0";
                if (nomCliente == "") nomCliente = "Consumidor Final";
            }

            // 2. Verificar que existan productos cargados y que el pago cubra el monto total
            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("Debe ingresar productos en la venta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            decimal pagacon = 0;
            decimal total = ParsearMonedaAR(txttotalpagar.Text);
            if (!decimal.TryParse(txtpagocon.Text.Trim(), NumberStyles.Number, culturaAR, out pagacon) || pagacon < total)
            {
                MessageBox.Show("El monto de pago no puede ser menor al total de la venta.", "Pago insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtpagocon.Select();
                return;
            }

            // 3. Construir el DataTable con la estructura esperada por el parámetro tipo tabla en SQL Server
            DataTable detalle_venta = new DataTable();

            detalle_venta.Columns.Add("IdProducto", typeof(int));
            detalle_venta.Columns.Add("PrecioVenta", typeof(decimal));
            detalle_venta.Columns.Add("Cantidad", typeof(int));
            detalle_venta.Columns.Add("SubTotal", typeof(decimal));

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                detalle_venta.Rows.Add(new object[] {
                    Convert.ToInt32(row.Cells["IdProducto"].Value),
                    ParsearMonedaAR(row.Cells["Precio"].Value.ToString()),
                    Convert.ToInt32(row.Cells["Cantidad"].Value),
                    ParsearMonedaAR(row.Cells["SubTotal"].Value.ToString())
                });
            }

            // 4. Generar el número correlativo, actualizar el vuelto y construir la cabecera Venta
            int idcorrelativo = new CN_Venta().ObtenerCorrelativo();
            string numerodocumento = string.Format("{0:00000}", idcorrelativo);
            calcularcambio();

            Venta oVenta = new Venta()
            {
                oUsuario = new Usuario() { IdUsuario = _Usuario != null ? _Usuario.IdUsuario : 1 },
                TipoDocumento = tipoDoc,
                NumeroDocumento = numerodocumento,
                DocumentoCliente = docCliente,
                NombreCliente = nomCliente,
                MontoPago = ParsearMonedaAR(txtpagocon.Text),
                MontoCambio = ParsearMonedaAR(txtcambio.Text),
                MontoTotal = ParsearMonedaAR(txttotalpagar.Text)
            };

            // 5. Registrar la venta en la capa de negocio y limpiar el formulario si tuvo éxito
            string mensaje = string.Empty;
            bool respuesta = new CN_Venta().Registrar(oVenta, detalle_venta, out mensaje);

            if (respuesta)
            {
                var result = MessageBox.Show("Numero de venta generada:\n" + numerodocumento + "\n\n¿Desea copiar al portapapeles?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                    Clipboard.SetText(numerodocumento);

                txtdocumentocliente.Text = "";
                txtnombrecliente.Text = "";
                dgvdata.Rows.Clear();
                calcularTotal();
                txtpagocon.Text = "";
                txtcambio.Text = "";
            }
            else
            {
                MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}