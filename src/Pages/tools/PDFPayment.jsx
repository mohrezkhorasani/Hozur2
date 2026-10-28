// Pages/PaymentList.jsx
import { useState, useEffect } from 'react';
import PDFGenerator from '../../Tools/pdf';

const PDFPayment = ({data}) => {
  return (
    <div>
      <PDFGenerator data={data} />
    </div>

  )
};

export default PDFPayment; 