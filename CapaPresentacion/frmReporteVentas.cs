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
    /// Formulario para generar, filtrar y exportar reportes resumidos de ventas
    /// por rango de fechas, tipo de comprobante y criterios de búsqueda en grilla.
    /// </summary>
    public partial class frmReporteVentas : Form
    {
        private readonly CultureInfo culturaAR = new CultureInfo("es-AR");

        /// <summary>
        /// Inicializa los componentes visuales del formulario.
        /// </summary>
        public frmReporteVentas()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Configura los tipos de comprobante, carga las columnas disponibles para búsqueda rápida,
        /// alinea los importes a la derecha y ejecuta la consulta inicial del último mes.
        /// </summary>
        private void frmReporteVentas_Load(object sender, EventArgs e)
        {
            // 1. Poblar el selector de tipos de documento incluyendo la opción "TODOS"
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "TODOS", Texto = "TODOS" });
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Boleta", Texto = "Boleta" });
            cbotipodocumento.Items.Add(new OpcionCombo() { Valor = "Factura", Texto = "Factura" });
            cbotipodocumento.DisplayMember = "Texto";
            cbotipodocumento.ValueMember = "Valor";
            cbotipodocumento.SelectedIndex = 0;

            // 2. Cargar los encabezados de la grilla en el combo de búsqueda secundaria
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                cbobusqueda.Items.Add(new OpcionCombo() { Valor = columna.Name, Texto = columna.HeaderText });
            }
            cbobusqueda.DisplayMember = "Texto";
            cbobusqueda.ValueMember = "Valor";
            cbobusqueda.SelectedIndex = 0;

            // Alinear columnas monetarias a la derecha
            dgvdata.Columns["MontoPago"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvdata.Columns["MontoCambio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvdata.Columns["MontoTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 3. Definir período inicial de un mes y cargar resultados
            txtfechainicio.Value = DateTime.Now.AddMonths(-1);
            txtfechafin.Value = DateTime.Now;

            BuscarVentas();
        }

        /// <summary>
        /// Convierte un texto numérico en formato argentino a su valor decimal equivalente.
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
        /// Reemplaza caracteres especiales (&amp;, &lt;, &gt;) para evitar errores al renderizar el HTML del PDF.
        /// </summary>
        private string EscaparHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        /// <summary>
        /// Recuenta las filas visibles en la grilla y suma sus montos totales para actualizar la etiqueta de resumen.
        /// </summary>
        private void ActualizarResumen()
        {
            int visibles = 0;
            decimal total = 0m;

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.Visible)
                {
                    visibles++;
                    total += ParsearMonedaAR(row.Cells["MontoTotal"].Value?.ToString());
                }
            }

            lblresumen.Text = $"Registros visibles: {visibles}   |   Total Acumulado: $ {total.ToString("N0", culturaAR)}";
        }

        /// <summary>
        /// Consulta las ventas registradas en la base de datos según el período y tipo de documento seleccionados,
        /// cargando los resultados en la grilla.
        /// </summary>
        private void BuscarVentas()
        {
            string tipoDoc = ((OpcionCombo)cbotipodocumento.SelectedItem).Valor.ToString();

            List<ReporteVenta> lista = new CN_Reporte().VentaSinDetalle(
                txtfechainicio.Value.ToString("dd/MM/yyyy"),
                txtfechafin.Value.ToString("dd/MM/yyyy"),
                tipoDoc
            );

            dgvdata.Rows.Clear();

            foreach (ReporteVenta rv in lista)
            {
                dgvdata.Rows.Add(new object[] {
                    rv.FechaRegistro,
                    rv.TipoDocumento,
                    rv.NumeroDocumento,
                    rv.DocumentoCliente,
                    rv.NombreCliente,
                    rv.UsuarioRegistro,
                    rv.MontoPago,
                    rv.MontoCambio,
                    rv.MontoTotal
                });
            }

            ActualizarResumen();
        }

        /// <summary>
        /// Limpia el filtro secundario y ejecuta una nueva consulta contra la base de datos.
        /// </summary>
        private void btnbuscarresultado_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            BuscarVentas();
        }

        /// <summary>
        /// Filtra las filas ya cargadas en la grilla según la columna y el texto ingresado.
        /// </summary>
        private void btnbuscar_Click(object sender, EventArgs e)
        {
            string columnaFiltro = ((OpcionCombo)cbobusqueda.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    string valorCelda = row.Cells[columnaFiltro].Value?.ToString() ?? "";
                    row.Visible = valorCelda.Trim().ToUpper().Contains(txtbusqueda.Text.Trim().ToUpper());
                }
            }

            ActualizarResumen();
        }

        /// <summary>
        /// Restablece la visibilidad de todas las filas cargadas y limpia el cuadro de búsqueda rápida.
        /// </summary>
        private void btnlimpiarbuscador_Click(object sender, EventArgs e)
        {
            txtbusqueda.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
            ActualizarResumen();
        }

        /// <summary>
        /// Exporta los registros visibles del reporte a un archivo Excel (.xlsx) mediante ClosedXML,
        /// manteniendo los importes monetarios como valores numéricos e incluyendo la fila de resumen.
        /// </summary>
        private void btnexportar_Click(object sender, EventArgs e)
        {
            // 1. Verificar que existan filas visibles antes de generar el archivo
            if (dgvdata.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == 0)
            {
                MessageBox.Show("No hay registros visibles para exportar a Excel.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 2. Construir el DataTable tipado a partir de las columnas y filas visibles
            DataTable dt = new DataTable();

            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                if (columna.Visible)
                {
                    if (columna.Name == "MontoPago" || columna.Name == "MontoCambio" || columna.Name == "MontoTotal")
                        dt.Columns.Add(columna.HeaderText, typeof(decimal));
                    else
                        dt.Columns.Add(columna.HeaderText, typeof(string));
                }
            }

            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                if (row.Visible)
                {
                    dt.Rows.Add(new object[] {
                        row.Cells[0].Value?.ToString() ?? "",
                        row.Cells[1].Value?.ToString() ?? "",
                        row.Cells[2].Value?.ToString() ?? "",
                        row.Cells[3].Value?.ToString() ?? "",
                        row.Cells[4].Value?.ToString() ?? "",
                        row.Cells[5].Value?.ToString() ?? "",
                        ParsearMonedaAR(row.Cells[6].Value?.ToString()),
                        ParsearMonedaAR(row.Cells[7].Value?.ToString()),
                        ParsearMonedaAR(row.Cells[8].Value?.ToString())
                    });
                }
            }

            // 3. Guardar el libro de Excel con formato numérico en las columnas de importes y totales al pie
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            savefile.Filter = "Archivo Excel (*.xlsx)|*.xlsx";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var hoja = wb.Worksheets.Add(dt, "Ventas");
                        int filaResumen = dt.Rows.Count + 3;

                        hoja.Column(7).Style.NumberFormat.Format = "#,##0";
                        hoja.Column(8).Style.NumberFormat.Format = "#,##0";
                        hoja.Column(9).Style.NumberFormat.Format = "#,##0";
                        hoja.Cell(filaResumen, 1).Value = lblresumen.Text;
                        hoja.Cell(filaResumen, 1).Style.Font.Bold = true;
                        hoja.ColumnsUsed().AdjustToContents();

                        wb.SaveAs(savefile.FileName);
                    }

                    MessageBox.Show("Reporte Excel generado correctamente.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar reporte Excel: " + ex.Message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Genera y exporta el reporte de ventas visible a un archivo PDF en formato horizontal,
        /// incorporando los datos y el logotipo del negocio mediante iTextSharp.
        /// </summary>
        private void btnpdf_Click(object sender, EventArgs e)
        {
            // 1. Validar que haya filas visibles para incluir en el documento
            if (dgvdata.Rows.Cast<DataGridViewRow>().Count(r => r.Visible) == 0)
            {
                MessageBox.Show("No hay registros visibles para exportar a PDF.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            savefile.Filter = "Archivo PDF (*.pdf)|*.pdf";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 2. Obtener datos institucionales y armar la estructura HTML del reporte
                    Negocio odatos = new CN_Negocio().ObtenerDatos();
                    string nombreNegocio = EscaparHtml(odatos?.Nombre?.ToUpper() ?? "SISTEMA DE VENTAS DE JOYAS");
                    string rucNegocio = EscaparHtml(odatos?.RUC ?? "-");
                    string dirNegocio = EscaparHtml(odatos?.Direccion ?? "-");
                    string tipoFiltro = EscaparHtml(((OpcionCombo)cbotipodocumento.SelectedItem).Texto);

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

                    sb.AppendLine($"<h2>{nombreNegocio}</h2>");
                    sb.AppendLine($"<div class='sub'>CUIT/RUC: {rucNegocio} | Dirección: {dirNegocio}<br/>");
                    sb.AppendLine($"<strong>REPORTE DE VENTAS</strong> | Comprobante: {tipoFiltro} | Período: {txtfechainicio.Value:dd/MM/yyyy} al {txtfechafin.Value:dd/MM/yyyy} | Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}</div>");

                    sb.AppendLine("<table><thead><tr>");
                    foreach (DataGridViewColumn col in dgvdata.Columns)
                    {
                        sb.AppendLine($"<th>{EscaparHtml(col.HeaderText)}</th>");
                    }
                    sb.AppendLine("</tr></thead><tbody>");

                    foreach (DataGridViewRow row in dgvdata.Rows)
                    {
                        if (!row.Visible) continue;
                        sb.AppendLine("<tr>");
                        foreach (DataGridViewColumn col in dgvdata.Columns)
                        {
                            string valor = EscaparHtml(row.Cells[col.Index].Value?.ToString() ?? "");
                            bool esNumero = col.Name == "MontoPago" || col.Name == "MontoCambio" || col.Name == "MontoTotal";
                            string clase = esNumero ? " class='num'" : "";
                            sb.AppendLine($"<td{clase}>{valor}</td>");
                        }
                        sb.AppendLine("</tr>");
                    }

                    sb.AppendLine("</tbody></table>");
                    sb.AppendLine($"<div class='resumen'>{EscaparHtml(lblresumen.Text)}</div>");
                    sb.AppendLine("</body></html>");

                    // 3. Renderizar el HTML y el logotipo en un PDF apaisado (A4 Rotate)
                    using (FileStream stream = new FileStream(savefile.FileName, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();

                        bool obtenido = true;
                        byte[] byteImage = new CN_Negocio().ObtenerLogo(out obtenido);
                        if (obtenido && byteImage != null && byteImage.Length > 0)
                        {
                            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(byteImage);
                            img.ScaleToFit(55, 55);
                            img.Alignment = iTextSharp.text.Image.ALIGN_RIGHT;
                            pdfDoc.Add(img);
                        }

                        using (StringReader sr = new StringReader(sb.ToString()))
                        {
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                        }

                        pdfDoc.Close();
                    }

                    MessageBox.Show("Reporte PDF generado correctamente.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar reporte PDF: " + ex.Message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
    }
}