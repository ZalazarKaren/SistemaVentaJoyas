using CapaEntidad;
using CapaNegocio;
using CapaPresentacion.Utilidades;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CapaPresentacion
{
    /// <summary>
    /// Formulario para consultar los registros históricos y de auditoría de ventas y compras,
    /// filtrar resultados en tiempo real y exportar la vista actual a PDF o Excel.
    /// </summary>
    public partial class frmHistoricos : Form
    {
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa los componentes visuales del formulario.
        /// </summary>
        public frmHistoricos()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configura las opciones del selector de tipo de histórico, establece el rango
        /// de fechas por defecto (último mes) y realiza la carga inicial de datos.
        /// </summary>
        private void frmHistoricos_Load(object sender, EventArgs e)
        {
            cbotipo.Items.Add(new OpcionCombo() { Valor = "VENTAS", Texto = "Histórico de Ventas" });
            cbotipo.Items.Add(new OpcionCombo() { Valor = "COMPRAS", Texto = "Histórico de Compras" });
            cbotipo.DisplayMember = "Texto";
            cbotipo.ValueMember = "Valor";
            cbotipo.SelectedIndex = 0;

            dtpfechainicio.Value = DateTime.Now.AddMonths(-1);
            dtpfechafin.Value = DateTime.Now;

            CargarHistorico();
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
        /// Reemplaza caracteres especiales (&amp;, &lt;, &gt;) para evitar errores al generar el documento HTML.
        /// </summary>
        private string EscaparHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>
        /// Limpia el filtro rápido y recarga la grilla al cambiar entre histórico de ventas y compras.
        /// </summary>
        private void cbotipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            CargarHistorico();
        }

        /// <summary>
        /// Consulta los registros históricos dentro del rango de fechas seleccionado.
        /// </summary>
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            CargarHistorico();
        }

        /// <summary>
        /// Restablece el filtro de texto y el rango de fechas al último mes, recargando la tabla.
        /// </summary>
        private void btnlimpiar_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            dtpfechainicio.Value = DateTime.Now.AddMonths(-1);
            dtpfechafin.Value = DateTime.Now;
            CargarHistorico();
        }

        /// <summary>
        /// Reconstruye dinámicamente las columnas de la grilla según el tipo de histórico elegido
        /// y carga los registros obtenidos desde la capa de negocio.
        /// </summary>
        private void CargarHistorico()
        {
            if (cbotipo.SelectedItem == null) return;

            string tipo = ((OpcionCombo)cbotipo.SelectedItem).Valor.ToString();
            string fechaInicio = dtpfechainicio.Value.ToString("dd/MM/yyyy");
            string fechaFin = dtpfechafin.Value.ToString("dd/MM/yyyy");

            // Limpiar filas y columnas previas antes de reconfigurar la estructura
            dgvdata.Rows.Clear();
            dgvdata.Columns.Clear();

            if (tipo == "VENTAS")
            {
                // 1. Configurar columnas específicas para el historial de ventas
                dgvdata.Columns.Add("FechaHistorico", "Fecha y Hora");
                dgvdata.Columns.Add("Accion", "Evento");
                dgvdata.Columns.Add("TipoDocumento", "Tipo Doc.");
                dgvdata.Columns.Add("NumeroDocumento", "N° Doc.");
                dgvdata.Columns.Add("DocumentoCliente", "Doc. Cliente");
                dgvdata.Columns.Add("NombreCliente", "Cliente");
                dgvdata.Columns.Add("UsuarioRegistro", "Usuario");
                dgvdata.Columns.Add("MontoTotal", "Monto Total");

                dgvdata.Columns["MontoTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // 2. Consultar ventas históricas y poblar la grilla
                List<HistoricoVenta> lista = new CN_Historico().ListarVentas(fechaInicio, fechaFin);
                foreach (HistoricoVenta item in lista)
                {
                    dgvdata.Rows.Add(new object[] {
                        item.FechaHistorico,
                        item.Accion,
                        item.TipoDocumento,
                        item.NumeroDocumento,
                        item.DocumentoCliente,
                        item.NombreCliente,
                        item.UsuarioRegistro,
                        FormatearMonedaAR(item.MontoTotal)
                    });
                }
            }
            else
            {
                // 1. Configurar columnas específicas para el historial de compras
                dgvdata.Columns.Add("FechaHistorico", "Fecha y Hora");
                dgvdata.Columns.Add("Accion", "Evento");
                dgvdata.Columns.Add("TipoDocumento", "Tipo Doc.");
                dgvdata.Columns.Add("NumeroDocumento", "N° Doc.");
                dgvdata.Columns.Add("DocumentoProveedor", "Doc. Proveedor");
                dgvdata.Columns.Add("RazonSocialProveedor", "Proveedor");
                dgvdata.Columns.Add("UsuarioRegistro", "Usuario");
                dgvdata.Columns.Add("MontoTotal", "Monto Total");

                dgvdata.Columns["MontoTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                // 2. Consultar compras históricas y poblar la grilla
                List<HistoricoCompra> lista = new CN_Historico().ListarCompras(fechaInicio, fechaFin);
                foreach (HistoricoCompra item in lista)
                {
                    dgvdata.Rows.Add(new object[] {
                        item.FechaHistorico,
                        item.Accion,
                        item.TipoDocumento,
                        item.NumeroDocumento,
                        item.DocumentoProveedor,
                        item.RazonSocialProveedor,
                        item.UsuarioRegistro,
                        FormatearMonedaAR(item.MontoTotal)
                    });
                }
            }

            // Aplicar el filtro rápido y actualizar el resumen de totales
            AplicarFiltroRapido();
        }

        /// <summary>
        /// Filtra las filas visibles de la grilla a medida que el usuario escribe en el cuadro de búsqueda.
        /// </summary>
        private void txtbusqueda_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroRapido();
        }

        /// <summary>
        /// Oculta o muestra filas buscando coincidencias en cualquier columna y recalcula
        /// la cantidad de registros visibles y el monto total acumulado.
        /// </summary>
        private void AplicarFiltroRapido()
        {
            string filtro = txtbusqueda.Text.Trim().ToUpper();
            int visibles = 0;
            decimal totalAcumulado = 0m;

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                bool coincide = string.IsNullOrEmpty(filtro);

                // Buscar el texto ingresado en todas las celdas de la fila actual
                if (!coincide)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Value != null && cell.Value.ToString().ToUpper().Contains(filtro))
                        {
                            coincide = true;
                            break;
                        }
                    }
                }

                row.Visible = coincide;

                // Sumar al contador y al total acumulado solo las filas que permanecen visibles
                if (coincide)
                {
                    visibles++;
                    totalAcumulado += ParsearMonedaAR(row.Cells["MontoTotal"].Value?.ToString());
                }
            }

            lblresumen.Text = $"Registros visibles: {visibles}   |   Total Acumulado: $ {FormatearMonedaAR(totalAcumulado)}";
        }

        /// <summary>
        /// Genera el código HTML completo del reporte incluyendo cabecera del negocio,
        /// período filtrado, filas visibles de la grilla y resumen de totales.
        /// </summary>
        private string ConstruirHtmlReporte()
        {
            string tituloReporte = ((OpcionCombo)cbotipo.SelectedItem).Texto.ToUpper();
            Negocio odatos = new CN_Negocio().ObtenerDatos();

            string nombreNegocio = EscaparHtml(odatos?.Nombre?.ToUpper() ?? "SISTEMA DE VENTAS DE JOYAS");
            string rucNegocio = EscaparHtml(odatos?.RUC ?? "-");
            string dirNegocio = EscaparHtml(odatos?.Direccion ?? "-");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<html><head><meta charset='utf-8'/>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, Helvetica, sans-serif; font-size: 10pt; color: #222222; }");
            sb.AppendLine("h2 { margin: 0 0 4px 0; color: #4B2E83; }");
            sb.AppendLine(".sub { font-size: 9pt; color: #555555; margin-bottom: 12px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("th { background-color: #4B2E83; color: #FFFFFF; font-weight: bold; padding: 6px; border: 1px solid #4B2E83; text-align: left; font-size: 9pt; }");
            sb.AppendLine("td { padding: 5px 6px; border: 1px solid #CCCCCC; font-size: 9pt; }");
            sb.AppendLine(".num { text-align: right; }");
            sb.AppendLine(".resumen { margin-top: 12px; font-weight: bold; font-size: 10pt; text-align: right; color: #4B2E83; }");
            sb.AppendLine("</style></head><body>");

            // Encabezado institucional y metadatos del filtro aplicado
            sb.AppendLine($"<h2>{nombreNegocio}</h2>");
            sb.AppendLine($"<div class='sub'>CUIT/RUC: {rucNegocio} | Dirección: {dirNegocio}<br/>");
            sb.AppendLine($"<strong>{tituloReporte}</strong> | Período: {dtpfechainicio.Value:dd/MM/yyyy} al {dtpfechafin.Value:dd/MM/yyyy} | Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

            // Cabeceras de la tabla
            sb.AppendLine("<table><thead><tr>");
            foreach (DataGridViewColumn col in dgvdata.Columns)
            {
                sb.AppendLine($"<th>{EscaparHtml(col.HeaderText)}</th>");
            }
            sb.AppendLine("</tr></thead><tbody>");

            // Filas visibles actualmente en pantalla
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (!row.Visible) continue;

                sb.AppendLine("<tr>");
                foreach (DataGridViewColumn col in dgvdata.Columns)
                {
                    string valor = EscaparHtml(row.Cells[col.Index].Value?.ToString() ?? "");
                    string clase = col.Name == "MontoTotal" ? " class='num'" : "";
                    sb.AppendLine($"<td{clase}>{valor}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine($"<div class='resumen'>{EscaparHtml(lblresumen.Text)}</div>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        /// <summary>
        /// Exporta los registros visibles del histórico a un archivo PDF en orientación horizontal
        /// incorporando el logotipo del negocio e iTextSharp.
        /// </summary>
        private void btnpdf_Click(object sender, EventArgs e)
        {
            // 1. Verificar que existan filas visibles para exportar
            if (dgvdata.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == 0)
            {
                MessageBox.Show("No hay registros visibles para exportar a PDF.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Configurar diálogo de guardado con nombre sugerido según tipo y fecha
            string tipo = ((OpcionCombo)cbotipo.SelectedItem).Valor.ToString();
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = $"Historico_{tipo}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            savefile.Filter = "Archivo PDF (*.pdf)|*.pdf";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string html = ConstruirHtmlReporte();

                    // 3. Crear documento PDF apaisado (A4 Rotate) para acomodar todas las columnas
                    using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        // Insertar logotipo del negocio en la parte superior derecha si existe
                        bool obtenido = true;
                        byte[] byteImage = new CN_Negocio().ObtenerLogo(out obtenido);

                        if (obtenido && byteImage != null && byteImage.Length > 0)
                        {
                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(byteImage);
                            img.ScaleToFit(55, 55);
                            img.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;
                            pdfDoc.Add(img);
                        }

                        using (StringReader sr = new StringReader(html))
                        {
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                        }

                        pdfDoc.Close();
                    }

                    MessageBox.Show("Reporte PDF generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar el PDF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Exporta los registros visibles del histórico a un libro de Excel (.xlsx) mediante ClosedXML,
        /// conservando los montos como valores numéricos e incluyendo la fila de resumen.
        /// </summary>
        private void btnexcel_Click(object sender, EventArgs e)
        {
            // 1. Verificar que existan filas visibles para exportar
            if (dgvdata.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == 0)
            {
                MessageBox.Show("No hay registros visibles para exportar a Excel.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Construir un DataTable tipado para que Excel reconozca MontoTotal como número
            DataTable dt = new DataTable();

            foreach (DataGridViewColumn col in dgvdata.Columns)
            {
                if (col.Name == "MontoTotal")
                    dt.Columns.Add(col.HeaderText, typeof(decimal));
                else
                    dt.Columns.Add(col.HeaderText, typeof(string));
            }

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (!row.Visible) continue;

                object[] valores = new object[dgvdata.Columns.Count];
                for (int i = 0; i < dgvdata.Columns.Count; i++)
                {
                    if (dgvdata.Columns[i].Name == "MontoTotal")
                        valores[i] = ParsearMonedaAR(row.Cells[i].Value?.ToString());
                    else
                        valores[i] = row.Cells[i].Value?.ToString() ?? "";
                }
                dt.Rows.Add(valores);
            }

            // 3. Solicitar ruta de guardado y generar la planilla Excel con formato
            string tipo = ((OpcionCombo)cbotipo.SelectedItem).Valor.ToString();
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = $"Historico_{tipo}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            savefile.Filter = "Archivo Excel (*.xlsx)|*.xlsx";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var hoja = wb.Worksheets.Add(dt, tipo);
                        int colMonto = dt.Columns.Count;
                        int filaResumen = dt.Rows.Count + 3;

                        // Aplicar formato numérico y agregar el resumen debajo de la tabla
                        hoja.Column(colMonto).Style.NumberFormat.Format = "#,##0";
                        hoja.Cell(filaResumen, 1).Value = lblresumen.Text;
                        hoja.Cell(filaResumen, 1).Style.Font.Bold = true;
                        hoja.ColumnsUsed().AdjustToContents();

                        wb.SaveAs(savefile.FileName);
                    }

                    MessageBox.Show("Reporte Excel generado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar el archivo Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}