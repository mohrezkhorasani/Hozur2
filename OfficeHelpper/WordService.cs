using Application.General;
using Application.Interface.IInfrastructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Drawing;
using System.IO;
using A = DocumentFormat.OpenXml.Drawing;
using Color = System.Drawing.Color;
using WP = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace Infrastructure.Word
{
    public class WordService : IWordService
    {
        private readonly string _wwwroot;

        public WordService(string wwwroot)
        {
            _wwwroot = wwwroot;
        }

        public string CreateCertificatePdf(string pdfPath, string fontPath, string bgImagePath, string Name,
            bool isMale, string NationalCode, string category, string title, string date, string role, string number)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            // متن‌ها
            var date1 = Statics.ToPersianNumbers($"{number}");
            var date2 = Statics.ToPersianNumbers($"{date}");
            string sex = "جناب آقای";
            if (!isMale)
                sex = "سرکار خانم";
            var lineIntroductionUser = $@"بدین وسیله گواهی می‌شود {sex} ";
            var lineIntroductionUser2 = $"{Name} ";
            var lineIntroductionUser3 = $"با کد ملی {Statics.ToPersianNumbers(NationalCode)}  در {category}  {(!string.IsNullOrEmpty(title) ? " با موضوع " : "")}";

            var lineTitleOfMeet = $"" + "" + $"{title}";
            var lineDateTitle = ".که در تاریخ";
            var lineDateValue = Statics.ToPersianNumbers($"       {date}       ");
            var line3Part3 = "برگزار شد، به عنوان ";
            var linerole = $"       {role}       ";
            var lineFinish = $"   در جلسه حضور داشتند   ";
            var line4 = " توفیق روزافزون ایشان را در عرصه های مختلف علمی از خدای منان مسئلت داریم";



            var image = System.Drawing.Image.FromFile(bgImagePath);
            int imgWidthPx = image.Width;
            int imgHeightPx = image.Height;
            float dpiX = image.HorizontalResolution;
            float dpiY = image.VerticalResolution;

            // تبدیل پیکسل به میلی‌متر یا پوینت (QuestPDF از pt استفاده می‌کند)
            float widthPt = imgWidthPx * 72f / dpiX;
            float heightPt = imgHeightPx * 72f / dpiY;



            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(widthPt, heightPt);
                    page.Margin(0);
                    page.Background().Image(bgImagePath, ImageScaling.FitArea);

                    page.Content().PaddingHorizontal(100).PaddingVertical(30).Column(column =>
                    {
                        // --- دو تاریخ بالا سمت چپ ---
                        column.Item().PaddingLeft(45).PaddingTop(73)
                            .ScaleToFit()
                            .AlignLeft()
                            .Text(date1)
                            .FontFamily(fontPath)
                            .FontSize(24)
                            .FontColor("#000000");

                        column.Item().PaddingLeft(45).PaddingTop(-10)
                            .ScaleToFit()
                            .AlignLeft()
                            .Text(date2)
                            .FontFamily(fontPath)
                            .FontSize(24)
                            .FontColor("#000000");

                        // --- متن معرفی ---
                        column.Item().PaddingTop(55).PaddingHorizontal(60)
                            .ScaleToFit()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontFamily(fontPath).FontSize(34));
                                text.Span(lineIntroductionUser).FontColor("#000000");
                                text.Span(lineIntroductionUser2).FontColor("#065E47");
                                text.Span(lineIntroductionUser3).FontColor("#000000");
                                text.AlignCenter();
                            });

                        // --- عنوان اصلی ---
                        column.Item()
                            .PaddingTop(-5)
                            .PaddingHorizontal(60)
                            .ScaleToFit()
                            .Text(lineTitleOfMeet)
                            .FontFamily(fontPath)
                            .FontSize(40)
                            .FontColor("#065E47")
                            .AlignCenter();

                        // --- خط سوم ---
                        column.Item()
                            //.PaddingTop(10)
                            .ScaleToFit()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(x => x.FontFamily(fontPath).FontSize(34));
                                text.Span(lineDateTitle).FontColor("#000000");
                                text.Span(lineDateValue).FontColor("#FB2C36");
                                text.Span(" برگزار شد، به عنوان ").FontColor("#000000");
                                text.Span(linerole).FontColor("#FB2C36");
                                text.Span(lineFinish).FontColor("#000000");
                                text.AlignCenter();
                            });

                        // --- خط چهارم ---
                        column.Item()
                            //.PaddingTop(10)
                            .ScaleToFit()
                            .Text(line4)
                            .FontFamily(fontPath)
                            .FontSize(34)
                            .FontColor("#000000")
                            .AlignCenter()
                            .DirectionFromRightToLeft();
                    });
                });
            }).GeneratePdf(pdfPath);
            return pdfPath;
        }
    }
}
