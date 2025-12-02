using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Quality;

namespace PseudoRun.Desktop.Services
{
    public class ExportService : IExportService
    {
        public Task ExportToPdfAsync(string code, string outputPath)
        {
            return Task.Run(() =>
            {
                // Create a new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = "PseudoRun Code Export";
                document.Info.Author = "PseudoRun Desktop";
                document.Info.CreationDate = DateTime.Now;

                // Create a page
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Define fonts
                XFont titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
                XFont codeFont = new XFont("Consolas", 10, XFontStyleEx.Regular);
                XFont infoFont = new XFont("Arial", 9, XFontStyleEx.Regular);

                // Starting position
                double yPosition = 50;
                double leftMargin = 50;
                double rightMargin = page.Width - 50;
                double lineHeight = 14;

                // Title
                gfx.DrawString("PseudoRun Code Export", titleFont, XBrushes.Black,
                    new XRect(leftMargin, yPosition, page.Width - 100, 30),
                    XStringFormats.TopLeft);
                yPosition += 30;

                // Timestamp
                string timestamp = $"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                gfx.DrawString(timestamp, infoFont, XBrushes.Gray,
                    new XRect(leftMargin, yPosition, page.Width - 100, 20),
                    XStringFormats.TopLeft);
                yPosition += 30;

                // Code content
                var lines = code.Split('\n');
                foreach (var line in lines)
                {
                    // Check if we need a new page
                    if (yPosition > page.Height - 50)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPosition = 50;
                    }

                    // Draw the line of code
                    gfx.DrawString(line, codeFont, XBrushes.Black,
                        new XRect(leftMargin, yPosition, rightMargin - leftMargin, lineHeight),
                        XStringFormats.TopLeft);
                    yPosition += lineHeight;
                }

                // Save the document
                document.Save(outputPath);
            });
        }

        public Task ExportToDocxAsync(string code, string outputPath)
        {
            return Task.Run(() =>
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document))
                {
                    // Add a main document part
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Title
                    Paragraph titlePara = body.AppendChild(new Paragraph());
                    Run titleRun = titlePara.AppendChild(new Run());
                    titleRun.AppendChild(new Text("PseudoRun Code Export"));
                    RunProperties titleProps = titleRun.AppendChild(new RunProperties());
                    titleProps.AppendChild(new Bold());
                    titleProps.AppendChild(new FontSize() { Val = "28" });

                    // Timestamp
                    Paragraph timestampPara = body.AppendChild(new Paragraph());
                    Run timestampRun = timestampPara.AppendChild(new Run());
                    timestampRun.AppendChild(new Text($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));

                    // Blank line
                    body.AppendChild(new Paragraph());

                    // Code content
                    var lines = code.Split('\n');
                    foreach (var line in lines)
                    {
                        Paragraph para = body.AppendChild(new Paragraph());
                        Run run = para.AppendChild(new Run());
                        run.AppendChild(new Text(line));
                        RunProperties runProps = run.AppendChild(new RunProperties());
                        runProps.AppendChild(new RunFonts() { Ascii = "Consolas" });
                        runProps.AppendChild(new FontSize() { Val = "20" });
                    }

                    mainPart.Document.Save();
                }
            });
        }
    }
}
