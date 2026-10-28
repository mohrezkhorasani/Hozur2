import { useState } from 'react';
import PaymentList from './PaymentList';
import PaymentForm from './PaymentForm';

const PaymentManagement = () => {
  const [showModal, setShowModal] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0); // برای بروزرسانی لیست

  const handleAddPayment = () => {
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
  };

  const handleSuccess = (data) => {
    console.log('Payment added successfully:', data);
    setRefreshKey(prev => prev + 1); // باعث اجرای مجدد loadData در PaymentList می‌شود
  };

  return (
    <div>
      <PaymentList 
        onAddPayment={handleAddPayment} 
        refreshKey={refreshKey} // ارسال به لیست
      />
      <PaymentForm 
        isOpen={showModal}
        onClose={handleCloseModal}
        onSuccess={handleSuccess}
      />
    </div>
  );
};

export default PaymentManagement;