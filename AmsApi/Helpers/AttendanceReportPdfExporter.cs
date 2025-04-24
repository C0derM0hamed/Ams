using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using System.IO;
using System;
using System.Collections.Generic;
namespace AmsApi.Helpers
{
    public class AttendanceReportPdfExporter
    {
        public byte[] GeneratePdf(string subjectName, DateTime date, List<string> present, List<string> absent)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(14));

                    page.Header()
                        .Text($"تقرير حضور - {subjectName}")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"التاريخ: {date:yyyy-MM-dd}").FontSize(14).AlignRight();
                        col.Item().PaddingVertical(10).Text(" "); // Spacer

                        col.Item().Text("الحاضرين").FontSize(16).Bold();
                        col.Item().Text(string.Join("\n", present)).FontSize(14);

                        col.Item().PaddingVertical(10).Text(" "); // Spacer

                        col.Item().Text("الغائبين").FontSize(16).Bold();
                        col.Item().Text(string.Join("\n", absent)).FontSize(14);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("تم توليد التقرير تلقائياً - ");
                            x.Span(DateTime.Now.ToString("f")).Italic();
                        });
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
