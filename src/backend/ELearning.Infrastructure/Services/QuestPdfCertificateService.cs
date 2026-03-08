using ELearning.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ELearning.Infrastructure.Services;

public sealed class QuestPdfCertificateService : ICertificatePdfService
{
    static QuestPdfCertificateService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(CertificatePdfData data)
    {
        return Document
            .Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(32);
                    page.PageColor(Colors.White);

                    page.Content().Border(2).BorderColor(Colors.Blue.Medium).Padding(24).Column(column =>
                    {
                        column.Spacing(14);

                        column.Item().AlignCenter().Text("CERTIFICADO").FontSize(36).SemiBold().FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("de finalizacion").FontSize(16).FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingTop(16).AlignCenter().Text("Se certifica que").FontSize(14);
                        column.Item().AlignCenter().Text(data.StudentName).FontSize(30).Bold().FontColor(Colors.Black);

                        column.Item().AlignCenter().Text("ha completado satisfactoriamente el curso").FontSize(14);
                        column.Item().AlignCenter().Text(data.CourseName).FontSize(24).SemiBold().FontColor(Colors.Blue.Darken3);

                        column.Item().PaddingTop(10).AlignCenter().Text($"Fecha: {data.CompletedAt:dd/MM/yyyy}").FontSize(12).FontColor(Colors.Grey.Darken2);
                        column.Item().AlignCenter().Text($"Codigo: {data.CertificateCode}").FontSize(11).FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingTop(22).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Column(sig =>
                            {
                                sig.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                sig.Item().PaddingTop(4).Text("Plataforma ELearning").FontSize(11).AlignCenter();
                            });

                            row.RelativeItem().AlignCenter().Column(sig =>
                            {
                                sig.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                sig.Item().PaddingTop(4).Text("Direccion Academica").FontSize(11).AlignCenter();
                            });
                        });
                    });
                });
            })
            .GeneratePdf();
    }
}
