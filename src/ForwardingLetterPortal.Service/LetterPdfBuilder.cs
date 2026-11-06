using ForwardingLetterPortal.Interface.Constants;
using ForwardingLetterPortal.Interface.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ForwardingLetterPortal.Service
{
    /// <summary>Builds the PDF purely from the stored letter snapshot, so every re-print is identical.</summary>
    public static class LetterPdfBuilder
    {
        public static byte[] Build(ForwardingLetter l)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var isInternal = l.LetterType == LetterTypes.InternalCopy;
            var date = l.GeneratedAt.AddHours(6); // Bangladesh Standard Time (UTC+6, no DST)
            var officer = l.Officer;

            return Document.Create(doc => doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(t => t.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text("Agrani Bank PLC.").Bold().FontSize(16);
                    row.ConstantItem(170).AlignRight().Text(isInternal ? "Internal Forwarding Copy" : "Customer Copy")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Ref: ").Bold();
                            t.Span(l.ReferenceNumber);
                        });
                        row.ConstantItem(150).AlignRight().Text($"Date: {date:dd MMMM yyyy}");
                    });

                    col.Item().Column(to =>
                    {
                        to.Item().Text("To,");
                        foreach (var line in Lines(l.AddressedTo))
                            to.Item().Text(line);
                    });

                    col.Item().Text(t =>
                    {
                        t.Span("Subject: ").Bold();
                        t.Span(l.SubjectSnapshot).Bold();
                    });

                    col.Item().Column(body =>
                    {
                        body.Spacing(6);
                        foreach (var line in l.BodySnapshot.Replace("\r\n", "\n").Split('\n'))
                        {
                            if (string.IsNullOrWhiteSpace(line)) body.Item().Height(4);
                            else body.Item().Text(line.TrimEnd());
                        }
                    });

                    if (isInternal && !string.IsNullOrWhiteSpace(l.Enclosures))
                    {
                        col.Item().Column(enc =>
                        {
                            enc.Item().Text("Enclosures:").Bold();
                            var n = 1;
                            foreach (var line in Lines(l.Enclosures))
                                enc.Item().Text($"{n++}. {line}");
                        });
                    }

                    col.Item().PaddingTop(30).Column(sig =>
                    {
                        sig.Item().Text("Sincerely,");
                        sig.Item().PaddingTop(28).Text(officer.FullName).Bold();
                        if (!string.IsNullOrWhiteSpace(officer.Designation))
                            sig.Item().Text(officer.Designation);
                        sig.Item().Text(officer.BranchName);
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span($"{l.ReferenceNumber}  |  Page ");
                    t.CurrentPageNumber();
                    t.Span(" of ");
                    t.TotalPages();
                });
            })).GeneratePdf();
        }

        private static IEnumerable<string> Lines(string? text)
            => (text ?? "").Replace("\r\n", "\n").Split('\n')
                .Select(s => s.Trim()).Where(s => s.Length > 0);
    }
}
