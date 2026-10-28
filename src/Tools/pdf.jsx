import { useRef } from 'react';
import html2pdf from 'html2pdf.js';

// ==============================
//  تبدیل عدد به حروف فارسی
// ==============================
const convertNumberToWords = (num) => {
  if (num === 0) return 'صفر';
  if (num < 0) return 'منفی ' + convertNumberToWords(-num);

  const units = ['', 'یک', 'دو', 'سه', 'چهار', 'پنج', 'شش', 'هفت', 'هشت', 'نه'];
  const tens = ['', 'ده', 'بیست', 'سی', 'چهل', 'پنجاه', 'شصت', 'هفتاد', 'هشتاد', 'نود'];
  const hundreds = ['', 'صد', 'دویست', 'سیصد', 'چهارصد', 'پانصد', 'ششصد', 'هفتصد', 'هشتصد', 'نهصد'];
  const scales = ['', 'هزار', 'میلیون', 'میلیارد', 'تریلیون'];

  const convertGroup = (n) => {
    if (n === 0) return '';
    let result = '';
    const h = Math.floor(n / 100);
    const t = Math.floor((n % 100) / 10);
    const u = n % 10;

    if (h > 0) result += hundreds[h] + ' ';
    if (t > 0) {
      if (t === 1 && u > 0) {
        const teens = ['', 'یازده', 'دوازده', 'سیزده', 'چهارده', 'پانزده', 'شانزده', 'هفده', 'هجده', 'نوزده'];
        result += teens[u] + ' ';
      } else {
        result += tens[t] + ' ';
        if (u > 0) result += 'و ' + units[u] + ' ';
      }
    } else if (u > 0) {
      result += units[u] + ' ';
    }
    return result.trim();
  };

  let result = '';
  let scaleIndex = 0;
  let number = Math.abs(num);
  while (number > 0) {
    const group = number % 1000;
    if (group > 0) {
      const groupStr = convertGroup(group);
      if (scaleIndex > 0) {
        result = groupStr + ' ' + scales[scaleIndex] + ' ' + result;
      } else {
        result = groupStr + ' ' + result;
      }
    }
    number = Math.floor(number / 1000);
    scaleIndex++;
  }
  return result.trim() || 'صفر';
};

// ==============================
//  کامپوننت PDF Generator
// ==============================
const PDFGenerator = ({ data }) => {
  const contentRef = useRef(null);

  // اعتبارسنجی داده‌ها
  const safeData = {
    amount: data?.amount || 0,
    tax: data?.tax || 0,
    to: data?.to || 'نامشخص',
    subject: data?.subject || 'بدون موضوع',
    source: data?.source || 'نامشخص',
    documentNumber: data?.documentNumber || 'بدون سند',
    friendlyID: data?.friendlyID || 'unknown',
    dateInsert: data?.dateInsert || new Date().toISOString(),
  };

  const taxAmount = (safeData.amount * safeData.tax) / 100;
  const netAmount = safeData.amount - taxAmount;
  const dateStr = new Date(safeData.dateInsert).toLocaleDateString('fa-IR');

  const generatePDF = () => {
    const element = contentRef.current;
    
    html2pdf()
      .set({
        margin: 0.5,
        filename: `پرداخت_${safeData.id || safeData.friendlyID}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { 
          scale: 2,
          useCORS: true,
          letterRendering: true
        },
        jsPDF: { 
          unit: 'in', 
          format: 'a5', 
          orientation: 'portrait' 
        }
      })
      .from(element)
      .save()
      .catch((error) => {
        console.error('خطا در تولید PDF:', error);
        alert('مشکلی در تولید فایل PDF رخ داد. لطفاً مجدداً تلاش کنید.');
      });
  };

  return (
    <div>
      {/* دکمه دانلود */}
      <button 
        onClick={generatePDF}
        style={{
          padding: '10px 20px',
          backgroundColor: '#2196F3',
          color: 'white',
          border: 'none',
          borderRadius: '5px',
          cursor: 'pointer',
          fontSize: '16px',
          marginBottom: '20px'
        }}
      >
        📄 دریافت PDF
      </button>

      {/* محتوای PDF */}
      <div 
        ref={contentRef}
        style={{
          width: '148mm', // A5 width
          minHeight: '210mm', // A5 height
          padding: '15px',
          border: '3px solid black',
          direction: 'rtl',
          fontFamily: 'Vazir, Tahoma, sans-serif',
          backgroundColor: 'white',
          fontSize: '12px',
          margin: '0 auto',
        }}
      >
        {/* هدر اصلی */}
        <h1 style={{ 
          fontSize: '24px', 
          textAlign: 'center', 
          marginBottom: '5px',
          fontWeight: 'bold',
          marginTop: '0'
        }}>
          مجمع عالی حکمت اسلامی
        </h1>

        {/* ردیف مدیریت اجرایی + دستور پرداخت */}
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          marginBottom: '5px',
          direction: 'rtl'
        }}>
          <div style={{ display: 'flex', direction: 'rtl' }}>
            <span style={{ fontWeight: 'bold' }}>مدیریت اجرایی مجمع: </span>
            <span style={{ color: '#45556C' }}>محمدباقر خراسانی</span>
          </div>
          <div style={{ fontWeight: 'bold' }}>دستور پرداخت</div>
        </div>

        {/* بخش اول - با کادر */}
        <div style={{
          border: '1px solid black',
          padding: '8px',
          marginBottom: '10px'
        }}>
          <p style={{ margin: 0 }}>
            مبلغ 
            <span style={{ fontWeight: 'bold' }}> {safeData.amount.toLocaleString('fa-IR')} </span>
            ریال به حروف (
            <span style={{ fontWeight: 'bold' }}>{convertNumberToWords(safeData.amount)}</span>
            ریال) به 
            <span style={{ fontWeight: 'bold' }}> {safeData.to} </span>
            بابت 
            <span style={{ fontWeight: 'bold' }}> {safeData.subject} </span>
            از محل 
            <span style={{ fontWeight: 'bold' }}> {safeData.source} </span>
            پرداخت گردد
          </p>
        </div>

        {/* نام و امضاء مقام دستوردهنده */}
        <div style={{ textAlign: 'left', marginTop: '5px' }}>
          نام و امضاء مقام دستوردهنده:
        </div>

        {/* رسید پرداخت */}
        <h2 style={{ textAlign: 'center', marginTop: '15px', marginBottom: '10px' }}>
          رسید پرداخت
        </h2>

        {/* جزئیات پرداخت */}
        <div style={{ marginTop: '5px' }}>
          <p style={{ textAlign: 'right', margin: '5px 0' }}>
            در تاریخ: {dateStr}
          </p>
          
          <p style={{ textAlign: 'right', margin: '5px 0' }}>
            <span>مبلغ : </span>
            <span style={{ fontWeight: 'bold' }}>
              {safeData.amount.toLocaleString('fa-IR')} 
              (ریال به حروف: {convertNumberToWords(safeData.amount)} ریال)
            </span>
          </p>
          
          <p style={{ textAlign: 'right', margin: '5px 0' }}>
            مالیات {safeData.tax} درصد  {taxAmount.toLocaleString('fa-IR')} ریال
          </p>
          
          <p style={{ textAlign: 'right', margin: '5px 0' }}>
            خالص پرداختی : {netAmount.toLocaleString('fa-IR')} ریال 
            (به حروف {convertNumberToWords(netAmount)} ریال)
          </p>
          
          <p style={{ textAlign: 'right', margin: '5px 0' }}>
            مبلغ فوق بصورت نقد/چک/واریز با سند شماره {safeData.documentNumber} 
            به {safeData.to} پرداخت گردید.
          </p>
        </div>

        {/* سه بخش امضا */}
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          marginTop: '20px',
          direction: 'rtl'
        }}>
          <div style={{ flex: 1, textAlign: 'center' }}>نام و امضاء دریافت کننده:</div>
          <div style={{ flex: 1, textAlign: 'center' }}>نام و امضاء مدیر اجرایی:</div>
          <div style={{ flex: 1, textAlign: 'center' }}>نام و امضاء تایید کننده / مقام مجاز:</div>
        </div>
      </div>
    </div>
  );
};

export default PDFGenerator;