using Application.General;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using System.Text.RegularExpressions;


public interface IPaymentPDFGenerator
{
    public string CreatePaymentPDF(
    string pdfPath,
    string fontPath,
    string Name,
    decimal amount,
    decimal tax,
    string to,
    string subject,
    string source,
    string documentNumber,
    string dateInsert,
    string friendlyID,
    string? bankName = null,
    string? paymentType = null);
}
public class PaymentPDFGenerator : IPaymentPDFGenerator
{
    public string wwwroot;

    public static int varFontSize = 20;
    public static int headerFontSize = 16;
    public static int normalFontSize = 13;
    public static int paddingbottom = 1;
    public PaymentPDFGenerator(string wwwroot)
    {
        this.wwwroot = wwwroot;
    }

    public string CreatePaymentPDF(
        string pdfPath,
        string fontPath,
        string Name,
        decimal amount,
        decimal tax,
        string to,
        string subject,
        string source,
        string documentNumber,
        string dateInsert,
        string friendlyID,
        string? bankName = null,
        string? paymentType = null)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // ==============================
            //  تبدیل عدد به حروف فارسی
            // ==============================
            string ConvertNumberToWords(decimal num)
            {
                if (num == 0) return "صفر";
                if (num < 0) return "منفی " + ConvertNumberToWords(-num);

                string[] units = { "", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
                string[] tens = { "", "ده", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
                string[] hundreds = { "", "صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };
                string[] scales = { "", "هزار", "میلیون", "میلیارد", "تریلیون" };

                string ConvertGroup(int n)
                {
                    if (n == 0) return "";
                    string result = "";
                    int h = n / 100;
                    int t = (n % 100) / 10;
                    int u = n % 10;

                    if (h > 0) result += hundreds[h] + " ";
                    if (t > 0)
                    {
                        if (t == 1 && u > 0)
                        {
                            string[] teens = { "", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
                            result += teens[u] + " ";
                        }
                        else
                        {
                            result += tens[t] + " ";
                            if (u > 0) result += "و " + units[u] + " ";
                        }
                    }
                    else if (u > 0)
                    {
                        result += units[u] + " ";
                    }
                    return result.Trim();
                }

                string result = "";
                int scaleIndex = 0;
                long number = (long)Math.Abs(num);
                while (number > 0)
                {
                    int group = (int)(number % 1000);
                    if (group > 0)
                    {
                        string groupStr = ConvertGroup(group);
                        if (scaleIndex > 0)
                        {
                            result = groupStr + " " + scales[scaleIndex] + " " + result;
                        }
                        else
                        {
                            result = groupStr + " " + result;
                        }
                    }
                    number /= 1000;
                    scaleIndex++;
                }
                return string.IsNullOrEmpty(result.Trim()) ? "صفر" : result.Trim();
            }

            // ==============================
            //  تبدیل اعداد به فارسی
            // ==============================
            string ToPersianNumbers(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;

                string[] persianDigits = { "۰", "۱", "۲", "۳", "۴", "۵", "۶", "۷", "۸", "۹" };
                string[] englishDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

                for (int i = 0; i < 10; i++)
                {
                    input = input.Replace(englishDigits[i], persianDigits[i]);
                }
                return input;
            }

            // ==============================
            //  تبدیل تاریخ به شمسی
            // ==============================
            string ToPersianDate(DateTime date)
            {
                var persianCalendar = new PersianCalendar();
                int year = persianCalendar.GetYear(date);
                int month = persianCalendar.GetMonth(date);
                int day = persianCalendar.GetDayOfMonth(date);
                return $"{year}/{month:D2}/{day:D2}";
            }

            // ==============================
            //  محاسبات
            // ==============================
            decimal taxAmount = (amount * tax) / 100;
            decimal netAmount = amount - taxAmount;

            DateTime dateObj = DateTime.Parse(dateInsert);
            string dateStr = dateInsert;


            string amountStr = ToPersianNumbers(amount.ToString("#,0"));
            string taxAmountStr = ToPersianNumbers(taxAmount.ToString("#,0"));
            string netAmountStr = ToPersianNumbers(netAmount.ToString("#,0"));
            string documentNumberStr = ToPersianNumbers(documentNumber);
            if (!string.IsNullOrWhiteSpace(bankName))
            {
                documentNumberStr = $"{documentNumberStr}";
            }
            string outputpath = pdfPath;
            string fontNastaliq = Path.Combine(wwwroot, "pdf", "IranNastaliq.ttf");
            pdfPath = Path.Combine(wwwroot, pdfPath);
            if (!Directory.Exists(pdfPath))
            {
                Directory.CreateDirectory(pdfPath);
            }
            string name = Guid.NewGuid().ToString()+".pdf";
            // ==============================
            //  ساخت PDF
            // ==============================
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontFamily("https://vcfile.hekmateislami.com/fonts/zar.ttf","zar", "https://vcfile.hekmateislami.com/fonts/zar","Zar", "Iranian Sans", "IranNastaliq"));
                    page.Size(PageSizes.A5.Landscape());

                    page.Content().Border(4, Colors.Black).Padding(7).Column(column =>
                    {
                        // ==========================================
                        // هدر اصلی
                        // ==========================================
                        column.Item().PaddingBottom(paddingbottom)
                            .Text("مجمع عالی حکمت اسلامی")
                            .FontSize(20)
                            .FontColor(Colors.Black)
                            .Bold()
                            .AlignCenter()
                            .DirectionFromRightToLeft();

                        // ==========================================
                        // ردیف مدیریت اجرایی + دستور پرداخت
                        // ==========================================
                        column.Item().PaddingBottom(paddingbottom).ContentFromRightToLeft()
                            .Row(row =>
                            {
                                // ستون اول (راست): مدیریت اجرایی
                                row.RelativeItem() // ← عرض مساوی با سایر ستون‌ها
                                    .AlignRight()
                                    .ContentFromRightToLeft()
                                    .PaddingHorizontal(6)
                                    .ScaleToFit() // ← متن را در یک خط نگه می‌دارد
                                    .Text(text =>
                                    {
                                        text.Span("مديريت اجرايی مجمع: ").FontSize(normalFontSize).Bold().FontColor(Colors.Black);
                                        //text.Span("\t\t\t محمد باقر\tخراسانی").FontSize(varFontSize).FontFamily("IranNastaliq").FontColor(Colors.Black);
                                        text.AlignRight();
                                    });

                                // ستون دوم (وسط): دستور پرداخت
                                row.RelativeItem() // ← عرض مساوی با سایر ستون‌ها
                                    .AlignCenter()
                                    .Text("دستور پرداخت")
                                    .FontColor(Colors.Black)
                                    .ExtraBold()
                                    .FontSize(headerFontSize)
                                    .DirectionFromRightToLeft();

                                // ستون سوم (چپ): خالی (برای حفظ تقارن)
                                row.RelativeItem(); // ← عرض مساوی با سایر ستون‌ها
                            });

                        // ==========================================
                        // بخش اول - با کادر
                        // ==========================================
                        column.Item().PaddingBottom(paddingbottom)
                            .Border(1)  // ← کادر بیرونی که هر دو بخش را شامل می‌شود
                            .BorderColor(Colors.Black)
                            .Padding(5) // حاشیه داخلی کادر
                            .Column(innerColumn =>
                            {
                                // ==========================================
                                // بخش اول: متن اصلی (بدون کادر جداگانه)
                                // ==========================================
                                innerColumn.Item()
                                    .ContentFromRightToLeft()
                                    .Text(text =>
                                    {
                                        text.DefaultTextStyle(x => x.FontSize(normalFontSize).SemiBold().LineHeight(1.3f));

                                        text.Span("مبلغ \t").FontColor(Colors.Black);
                                        text.Span(amountStr + "").Price();
                                        text.Span(" \t ريال به حروف (\t").FontColor(Colors.Black);
                                        text.Span(ConvertNumberToWords(amount).Replace("ی", "ي") + " ريال\t").PricePersian();
                                        text.Span("\t) به \t").FontColor(Colors.Black);
                                        text.Span(to.FixYe()).FontColor(Colors.Black).NormalVar();
                                        text.Span("\t بابت \t").FontColor(Colors.Black);
                                        text.Span(subject.FixYe()).NormalVar();
                                        text.Span("\t از محل \t").FontColor(Colors.Black);
                                        text.Span(source.FixYe()).FontColor(Colors.Black).NormalVar();
                                        text.Span("\t پرداخت گردد.\t").FontColor(Colors.Black).DirectionFromRightToLeft();
                                    });

                                // ==========================================
                                // بخش دوم: نام و امضاء مقام دستوردهنده (زیر متن اصلی، سمت چپ)
                                // ==========================================
                                innerColumn.Item().PaddingTop(paddingbottom)
                                    .ContentFromRightToLeft()
                                    .PaddingLeft(25)
                                    .AlignLeft()
                                    .Text("نام و امضاء مقام دستوردهنده:")
                                    .FontSize(normalFontSize)
                                    .AlignRight()   
                                    .FontColor(Colors.Black)
                                    .DirectionFromRightToLeft();
                            });

                        // ==========================================
                        // رسید پرداخت
                        // ==========================================
                        column.Item().PaddingTop(paddingbottom)
                            .Text("رسيد پرداخت")
                            .FontColor(Colors.Black)
                            .AlignCenter()
                            .ExtraBold()
                            .FontSize(headerFontSize)
                            .DirectionFromRightToLeft();

                        // ==========================================
                        // جزئیات پرداخت - اصلاح شده
                        // ==========================================
                        column.Item().PaddingTop(0).ContentFromRightToLeft()
                            .PaddingHorizontal(10)
                            .Text(text =>
                            {
                                text.DefaultTextStyle(x => x
                                    .Bold()
                                    .FontSize(normalFontSize)
                                    .LineHeight(1.1f)
                                );

                                // ==========================================
                                // تاریخ
                                // ==========================================
                                text.Span("در تاريخ: ").Bold().FontColor(Colors.Black);
                                text.Span(Statics.ToPersianNumbers(dateStr))
                                    .DirectionFromRightToLeft()
                                    .FontColor(Colors.Black)
                                    .FontFamily("IranNastaliq")
                                    .FontSize(varFontSize)
                                    .Underline()
                                    .Bold();
                                text.Span("\n"); // ← خط جدید با فاصله کمتر

                                // ==========================================
                                // مبلغ
                                // ==========================================
                                text.Span("مبلغ \t\t").FontColor(Colors.Black);
                                text.Span(amountStr).Price();

                                text.Span("\t\tريال ( به حروف: \t\t").FontColor(Colors.Black);
                                text.Span(ConvertNumberToWords(amount).Replace("ی", "ي") + " ريال")
                                   .PricePersian();
                                text.Span("\t\t)").FontColor(Colors.Black);
                                text.Span("\n");

                                // ==========================================
                                // مالیات
                                // ==========================================
                                text.Span("ماليات \t\t").FontColor(Colors.Black);
                                text.Span(ToPersianNumbers(tax.ToString()))
                                    .FontColor(Colors.Black)
                                    .FontFamily("IranNastaliq")
                                    .FontSize(varFontSize)
                                    .Bold();
                                text.Span("\t\t درصد  \t\t").FontColor(Colors.Black);
                                text.Span(taxAmountStr)
                                    .Price();
                                text.Span("\t\tريال\n");      

                                // ==========================================
                                // خالص پرداختی
                                // ==========================================
                                text.Span("خالص پرداختی : ").FontColor(Colors.Black);
                                text.Span(netAmountStr)
                                    .Price();
                                text.Span("\t\t ريال (به حروف \t\t").FontColor(Colors.Black);
                                text.Span(ConvertNumberToWords(netAmount).Replace("ی", "ي") + " ريال")
                                    .PricePersian();
                                text.Span("\t\t)").FontColor(Colors.Black);
                                text.Span("\n\n");

                                // ==========================================
                                // توضیحات نهایی
                                // ==========================================
                                string payTypeStr = !string.IsNullOrWhiteSpace(paymentType) ? paymentType.FixYe() : "نقد/چک/واريز";
                                text.Span($"مبلغ فوق بصورت {payTypeStr} با سند شماره \t").FontColor(Colors.Black);
                                text.Span(documentNumberStr)
                                    .NormalVar();
                                if(!string.IsNullOrEmpty(bankName)) text.Span($"\t-\t{bankName?.FixYe()} \t\t").NormalVar();

                                text.Span("\t به \t").FontColor(Colors.Black);
                                text.Span(to.FixYe()).NormalVar();


                                text.Span("\t\t\t پرداخت گرديد.").FontColor(Colors.Black);
                            });
                        // ==========================================
                        // سه بخش امضا
                        // ==========================================
                        column.Item().PaddingTop(10).ContentFromRightToLeft()

                            .Row(row =>
                            {
                                row.RelativeItem().AlignCenter()
                                    .Text("نام و امضاء دریافت کننده:")
                                    .FontSize(normalFontSize)
                                    .FontColor(Colors.Black)
                                    .DirectionFromRightToLeft();

                                row.RelativeItem().AlignCenter()
                                    .Text("نام و امضاء مدیر اجرایی:")
                                    .FontSize(normalFontSize)
                                    .FontColor(Colors.Black)
                                    .DirectionFromRightToLeft();

                                row.RelativeItem().AlignCenter()
                                    .Text("نام و امضاء تایید کننده / مقام مجاز:")
                                    .FontSize(normalFontSize)
                                    .FontColor(Colors.Black)
                                    .DirectionFromRightToLeft();
                            });
                    });
                });
            })
            .GeneratePdf(Path.Combine("C:\\hekmatFiles\\download\\pdf", name));

            return Statics.CombinePath(Statics.URLFiles, "download/pdf", name);
        }
        catch (UnauthorizedAccessException ex)
        {
            // If this still happens, it means the App Pool user lacks permissions on the folder.
            return $"Access denied to '{pdfPath}'. Grant Modify permission to the IIS user.";
        }
        catch (Exception ex)
        {
            return $"PDF generation failed: {ex.Message}";
        }
    }

}

public static class PDFComponents
{
    public static TextSpanDescriptor Price(this TextSpanDescriptor t)
    {
        return t.Underline()
                .FontColor(Colors.Black)
                .FontFamily("IranNastaliq")
                .FontSize(PaymentPDFGenerator.varFontSize + 5)
                .LetterSpacing(0.1f)
                .ExtraLight()
                ;
    }
    public static TextSpanDescriptor PricePersian(this TextSpanDescriptor t)
    {
        return t.FontColor(Colors.Black)
                                    .FontSize(PaymentPDFGenerator.normalFontSize + 1)
                                    .Light()
                                    .Underline()
                                    ;
    }
    public static TextSpanDescriptor NormalVar(this TextSpanDescriptor t)
    {
        return t.FontColor(Colors.Black)
                                    .FontSize(PaymentPDFGenerator.normalFontSize + 1)
                                    .Underline()
                                    .Light()
                                    ;
        ;
    }

    public static string FixYe(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // کلمات را با فاصله جدا می‌کنیم
        var words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            // اگر کلمه شامل حروف انگلیسی باشد
            bool hasEnglish = Regex.IsMatch(word, "[a-zA-Z]");

            // اگر کلمه شامل "ی" باشد و به "ی" ختم نشود (یعنی "ی" در وسط است)
            bool hasYe = word.Contains("ی");
            bool endsWithYe = word.EndsWith("ی");

            if ( hasYe && !endsWithYe)
            {
                // همه "ی"های کلمه را به "ي" تبدیل می‌کنیم
                words[i] = word.Replace("ی", "ي");
            }
        }

        return string.Join(" ", words);
    }

}