// Pages/LoadingPage.jsx
import { useState, useEffect } from 'react';
import { FaSpinner, FaTimes } from 'react-icons/fa';
import DatePicker from 'react-multi-date-picker';
import persian from 'react-date-object/calendars/persian';
import persian_fa from 'react-date-object/locales/persian_fa';
import { convertNumberToWords, persianAndArabicToEnglishNumbers } from '../../Tools/tools';
import { addNewPayment } from '../../api/GetRooms';
import generatePaymentPDF from '../../Tools/pdf';

const PaymentForm = ({ isOpen, onClose, onSuccess }) => {
  const [formData, setFormData] = useState({
    amount: '',
    subject: '',
    source: '',
    date: new Date(),
    taxPercent: '0',
    netAmount: '',
    documentNumber: '',
    to: ''
  });

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (isOpen) {
      setFormData({
        amount: '',
        subject: '',
        source: '',
        date: new Date(),
        taxPercent: '0',
        netAmount: '',
        documentNumber: '',
        to: ''
      });
      setErrors({});
    }
  }, [isOpen]);

  const formatNumberWithCommas = (number) => {
    if (!number) return '';
    const numStr = number.toString().replace(/,/g, '');
    if (isNaN(numStr) || numStr === '') return '';
    return numStr.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
  };

  const handleAmountChange = (e, field) => {
    const value = e.target.value.replace(/,/g, '');
    if (!isNaN(value) || value === '') {
      setFormData({
        ...formData,
        [field]: value
      });
    }
  };

  const getDisplayValue = (value) => {
    if (!value) return '';
    return formatNumberWithCommas(value);
  };

  useEffect(() => {
    const amount = parseFloat(formData.amount.replace(/,/g, ''));
    const tax = parseFloat(formData.taxPercent);
    if (!isNaN(amount) && !isNaN(tax)) {
      const net = amount * (1 - tax / 100);
      setFormData(prev => ({
        ...prev,
        netAmount: net.toString()
      }));
    } else if (!isNaN(amount) && isNaN(tax)) {
      setFormData(prev => ({
        ...prev,
        netAmount: amount.toString()
      }));
    }
  }, [formData.amount, formData.taxPercent]);

  const getPersianDate = (date) => {
    if (!date) return '';
    try {
      if (date.year && date.month && date.day) {
        const persianMonths = [
          'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
          'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
        ];
        return `${date.day} ${persianMonths[date.month - 1]} ${date.year}`;
      }
      if (date instanceof Date) {
        return date.toLocaleDateString('fa-IR');
      }
      return '';
    } catch (error) {
      return '';
    }
  };

  // فقط همین تابع رو اصلاح کردم
  const formatDateForAPI = (date) => {
    if (!date) return '';

    try {
      // اگه شیء DatePicker باشه (بعد از انتخاب کاربر)
      if (date.year !== undefined && date.month !== undefined && date.day !== undefined) {
        // formData.date
        const year = date.year;
        const month = String(date.month).padStart(2, '0');
        const day = String(date.day).padStart(2, '0');
        return `${year}/${month}/${day}`;
      }

      // اگه شیء Date معمولی باشه (همون پیشفرض)
      if (date instanceof Date && !isNaN(date.getTime())) {
        let d = persianAndArabicToEnglishNumbers(getPersianDate(date))
        return `${d}`;
      }

      return '';
    } catch (error) {
      console.error('Error formatting date:', error);
      return '';
    }
  };

  const validateForm = () => {
    const newErrors = {};
    if (!formData.amount) newErrors.amount = 'مبلغ الزامی است';
    if (!formData.subject) newErrors.subject = 'بابت الزامی است';
    if (!formData.source) newErrors.source = 'از محل الزامی است';
    if (!formData.date) newErrors.date = 'تاریخ الزامی است';
    if (!formData.documentNumber) newErrors.documentNumber = 'سند شماره الزامی است';
    if (!formData.to) newErrors.to = 'به الزامی است';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const sendPaymentToDatabase = async (data) => {
    const res = await addNewPayment(data);
    window.open(res.path, '_blank');

  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setLoading(true);

    try {
      const paymentData = {
        amount: formData.amount.replace(/,/g, ''),
        subject: formData.subject,
        source: formData.source,
        date: formatDateForAPI(formData.date),
        taxPercent: Number.parseFloat(formData.taxPercent),
        netAmount: (Math.round(formData.netAmount.replace(/,/g, ''))),
        documentNumber: formData.documentNumber,
        to: formData.to
      };

      console.log('Sending payment data:', paymentData);

      await sendPaymentToDatabase(paymentData);
      alert('پرداخت با موفقیت ثبت شد');
      if (onSuccess) onSuccess(paymentData);
      onClose();
    } catch (error) {
      alert('خطا در ثبت پرداخت');
      console.error('Submit error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl w-full max-w-6xl max-h-[90vh] overflow-y-auto">
        <div className="sticky top-0 bg-white border-b border-gray-200 rounded-t-xl px-6 py-4 flex items-center justify-between z-10">
          <h2 className="text-2xl font-bold text-gray-800">
            فرم پرداخت
          </h2>
          <button
            onClick={handleClose}
            disabled={loading}
            className="p-2 hover:bg-gray-100 rounded-lg transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <FaTimes className="w-5 h-5 text-gray-500 hover:text-gray-700" />
          </button>
        </div>

        <div className="p-6">
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  مبلغ به ریال
                </label>
                <div className="relative">
                  <input
                    type="text"
                    value={getDisplayValue(formData.amount)}
                    onChange={(e) => handleAmountChange(e, 'amount')}
                    className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 
                      px-[14%]
                      outline-none transition text-sm ${errors.amount ? 'border-red-500' : 'border-gray-300'
                      }`}
                    placeholder="مبلغ را وارد کنید"
                    dir="ltr"
                  />
                  <span className="absolute left-2.5 top-2.5 text-gray-500 text-sm">ریال</span>
                </div>
                {errors.amount && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.amount}</p>
                )}
                <div className="w-full text-right rtl">
                  <span className="font-bold text-right block w-full px-2 py-0.5 rounded mt-0.5 text-xs">
                    {(() => {
                      const numbSTR = formData.amount.replace(/,/g, "");
                      const numbINT = Number.parseInt(numbSTR);
                      return convertNumberToWords(numbINT);
                    })()}
                    <span className='px-2 font-normal text-gray-600'>
                      ریال
                    </span>
                  </span>
                </div>
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  بابت
                </label>
                <input
                  type="text"
                  value={formData.subject}
                  onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                  className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.subject ? 'border-red-500' : 'border-gray-300'
                    }`}
                  placeholder="توضیحات پرداخت"
                />
                {errors.subject && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.subject}</p>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  از محل
                </label>
                <input
                  type="text"
                  value={formData.source}
                  onChange={(e) => setFormData({ ...formData, source: e.target.value })}
                  className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.source ? 'border-red-500' : 'border-gray-300'
                    }`}
                  placeholder="منبع پرداخت"
                />
                {errors.source && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.source}</p>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  تاریخ
                </label>
                <DatePicker
                  calendar={persian}
                  locale={persian_fa}
                  value={formData.date}
                  onChange={(date) => setFormData({ ...formData, date })}
                  format="YYYY/MM/DD"
                  className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.date ? 'border-red-500' : 'border-gray-300'
                    }`}
                  placeholder="تاریخ را انتخاب کنید"
                  containerClassName="w-full"
                  inputClass="w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                />
                {errors.date && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.date}</p>
                )}
                {formData.date && (
                  <div className="w-full text-right mt-0.5">
                    <span className="font-bold text-gray-700 text-xs" dir='rtl'>
                      {getPersianDate(formData.date)}
                    </span>
                  </div>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  مالیات درصد
                </label>
                <div className="relative">
                  <input
                    type="number"
                    value={formData.taxPercent}
                    onChange={(e) => setFormData({ ...formData, taxPercent: e.target.value })}
                    className={`w-full p-2.5 border rounded-lg focus:ring-2 px-[12%] focus:ring-blue-500 outline-none transition text-sm ${errors.taxPercent ? 'border-red-500' : 'border-gray-300'
                      }`}
                    placeholder="درصد مالیات را وارد کنید"
                    min="0"
                    max="100"
                    dir="ltr"
                  />
                  <span className="absolute left-2.5 top-2.5 text-gray-500 text-sm">%</span>
                </div>
                {errors.taxPercent && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.taxPercent}</p>
                )}
                {formData.amount && formData.taxPercent && (
                  <div className="w-full text-right rtl mt-0.5">
                    <span className="font-bold text-gray-700 text-xs">
                      {(() => {
                        const amount = parseFloat(formData.amount.replace(/,/g, ''));
                        const tax = parseFloat(formData.taxPercent);
                        if (!isNaN(amount) && !isNaN(tax)) {
                          const taxAmount = amount * (tax / 100);
                          return `مبلغ مالیات: ${formatNumberWithCommas(taxAmount.toString())} ریال`;
                        }
                        return '';
                      })()}
                    </span>
                  </div>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  خالص پرداختی به ریال
                </label>
                <div className="relative">
                  <input
                    type="text"
                    value={getDisplayValue(formData.netAmount)}
                    onChange={(e) => {
                      const value = e.target.value.replace(/,/g, '');
                      if (!isNaN(value) || value === '') {
                        setFormData({ ...formData, netAmount: value });
                      }
                    }}
                    className={`w-full px-[14%] p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.netAmount ? 'border-red-500' : 'border-gray-300'
                      }`}
                    dir="ltr"
                  />
                  <span className="absolute left-2.5 top-2.5 text-gray-500 text-sm">ریال</span>
                </div>
                {formData.netAmount && (
                  <div className="w-full text-right rtl">
                    <span className="font-bold text-right block w-full px-2 py-0.5 rounded mt-0.5 text-xs">
                      {(() => {
                        const numbSTR = formData.netAmount.replace(/,/g, "");
                        const numbINT = Number.parseInt(numbSTR);
                        return convertNumberToWords(numbINT);
                      })()}
                      <span className='px-2 font-normal text-gray-600'>
                        ریال
                      </span>
                    </span>
                  </div>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  سند شماره
                </label>
                <input
                  type="text"
                  value={formData.documentNumber}
                  onChange={(e) => setFormData({ ...formData, documentNumber: e.target.value })}
                  className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.documentNumber ? 'border-red-500' : 'border-gray-300'
                    }`}
                  placeholder="شماره سند را وارد کنید"
                />
                {errors.documentNumber && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.documentNumber}</p>
                )}
              </div>

              <div>
                <label className="block text-gray-700 font-medium mb-1 text-sm">
                  به
                </label>
                <input
                  type="text"
                  value={formData.to}
                  onChange={(e) => setFormData({ ...formData, to: e.target.value })}
                  className={`w-full p-2.5 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm ${errors.to ? 'border-red-500' : 'border-gray-300'
                    }`}
                  placeholder="گیرنده را وارد کنید"
                />
                {errors.to && (
                  <p className="text-red-500 text-xs mt-0.5">{errors.to}</p>
                )}
              </div>
            </div>

            <div className="mt-4">
              <button
                type="submit"
                disabled={loading}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-2.5 px-4 rounded-lg transition duration-200 disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center text-sm"
              >
                {loading ? (
                  <>
                    <FaSpinner className="animate-spin ml-2" />
                    در حال ثبت...
                  </>
                ) : (
                  'ثبت پرداخت'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default PaymentForm;