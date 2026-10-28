import React from 'react';

const PrintPDFIframe = ({ pdfUrl }) => {
  const handlePrint = () => {
    // ایجاد iframe پنهان
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = pdfUrl;
    document.body.appendChild(iframe);

    // پس از بارگذاری PDF، پرینت بگیر
    iframe.onload = () => {
      iframe.contentWindow.print();

      // بعد از اجرای پرینت، iframe را حذف می‌کنیم (اختیاری)
      setTimeout(() => {
        document.body.removeChild(iframe);
      }, 1000);
    };
  };

  return (
    <button onClick={handlePrint}>
      پرینت PDF (با iframe)
    </button>
  );
};

export default PrintPDFIframe;