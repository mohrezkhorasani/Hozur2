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
            bool isMale,string NationalCode, string category, string title, string date, string role, string number)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            // متن‌ها
            var date1 = Statics.ToPersianNumbers($"{number}");
            var date2 = Statics.ToPersianNumbers($"{date}");
            string sex = "جناب آقای";
            if (!isMale)
                sex = "سرکار خانم";
            var lineIntroductionUser = $@"بدین وسیله گواهی می‌شود {sex} {Name} با کد ملی {Statics.ToPersianNumbers(NationalCode)}  در ";
            var lineTitleOfMeet = $"{category}  " + (!string.IsNullOrEmpty(title) ? " با موضوع " : "" )+ $"{title}" ;
            var lineDateTitle = "که در تاریخ ";
            var lineDateValue = Statics.ToPersianNumbers($"       {date}       ");
            var line3Part3 = "برگزار شد، به عنوان ";
            var linerole = $"       {role}       ";
            var lineFinish = $" در جلسه حضور داشتند ";
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

                    // تصویر پس‌زمینه کل صفحه
                    page.Background().Image(bgImagePath, ImageScaling.FitArea);

                    page.Content().Padding(50).Column(column =>
                    {
                        // --- دو تاریخ بالا سمت چپ با Margin ---
                        column.Item().PaddingLeft(55).PaddingTop(32)
                            .Text(date1).FontFamily(fontPath).FontSize(17).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000")).AlignLeft();

                        column.Item().PaddingLeft(55).PaddingTop(-10)
                            .Text(date2).FontFamily(fontPath).FontSize(17).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000")).AlignLeft();

                        // فاصله قبل متن اصلی
                        column.Item().PaddingTop(50).Text(lineIntroductionUser)
                            .FontFamily(fontPath).FontSize(28).WordSpacing(0.1f).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000")).AlignCenter();

                        column.Item().PaddingTop(-10).Text(lineTitleOfMeet)
                            .FontFamily(fontPath).FontSize(38).FontColor(QuestPDF.Infrastructure.Color.FromHex("#065E47")).AlignCenter();

                        // خط سوم با دو کلمه قرمز
                        column.Item().PaddingTop(10)
          .Text(text =>
          {
              text.DefaultTextStyle(x => x.FontFamily(fontPath).FontSize(28));

              text.Span(lineDateTitle).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000"));
              text.Span(lineDateValue).FontColor(QuestPDF.Infrastructure.Color.FromHex("#FB2C36"));
              text.Span(" برگزار شد، به عنوان ").FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000"));
              text.Span(linerole).FontColor(QuestPDF.Infrastructure.Color.FromHex("#FB2C36"));
              text.Span(lineFinish).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000")); // نقطه داخل Span آخر

              text.AlignCenter();  // راست چین کردن کل خط
          });
                        // خط چهارم
                        column.Item().PaddingTop(10)
                            .Text(line4)
                            .FontFamily(fontPath).FontSize(28).FontColor(QuestPDF.Infrastructure.Color.FromHex("#000000"))
                            .AlignCenter()
                            .DirectionFromRightToLeft(); // راست به چپ
                    });
                });
            })
            .GeneratePdf(pdfPath);
            return pdfPath;
        }
    }
}
