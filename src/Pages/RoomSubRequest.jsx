import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { SendOTPRoomSubscribedRegistration, ConfirmOTPRoomSubscribedRegistration, DirectRoomSubscribedRegistration } from '@/api/StartNewMeeting';
import { fetchRoom, GetRoomDetails } from '../api/GetRooms';
import { UploadFiles } from '../api/FTP';
import { useToast } from '../Tools/ToastProvider';
import { setCookieWithConfig, syncUserTokens } from '../api/apiNeed';
import { useAuth } from '../Tools/AuthProvider';

// Helper to convert Persian/Arabic digits to English digits
function toEnglishDigits(str) {
  if (!str) return '';
  return String(str)
    .replace(/[۰-۹]/g, (d) => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d))
    .replace(/[٠-٩]/g, (d) => '٠١٢٣٤٥٦٧٨٩'.indexOf(d));
}

function RoomSubRequest() {
  const { Mid } = useParams();
  const toast = useToast();
  const navigate = useNavigate();

  // --- Meeting and Room States ---
  const [meet, setMeet] = useState(null);
  const [roomDetails, setRoomDetails] = useState(null);
  const [loadingInitial, setLoadingInitial] = useState(true);

  const { user } = useAuth();

  // --- Registration States ---
  const [firstname, setFirstname] = useState(user?.fullname?.split(' ')[0] || '');
  const [lastname, setLastname] = useState(user?.fullname?.split(' ').slice(1).join(' ') || '');
  const [nationalCode, setNationalCode] = useState(user?.nationalCode || '');
  const [phone, setPhone] = useState(user?.phone || '');

  // --- Registration OTP states ---
  const [isOtpSent, setIsOtpSent] = useState(false);
  const [hasPaid, setHasPaid] = useState(false);
  const [receiptUrl, setReceiptUrl] = useState('');
  const [uploadingReceipt, setUploadingReceipt] = useState(false);
  const [otpCode, setOtpCode] = useState('');
  const [otpGuid, setOtpGuid] = useState('');
  const [otpTimer, setOtpTimer] = useState(0);
  const [otpLoading, setOtpLoading] = useState(false);
  const [verifyError, setVerifyError] = useState('');
  const [verificationLoading, setVerificationLoading] = useState(false);
  const [maskedPhone, setMaskedPhone] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);

  // --- OTP Timer countdown ---
  useEffect(() => {
    let interval = null;
    if (otpTimer > 0) {
      interval = setInterval(() => {
        setOtpTimer((prev) => prev - 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [otpTimer]);

  // --- Fetch Meeting Info ---
  useEffect(() => {
    const fetchMeetingInfo = async () => {
      try {
        const data = await fetchRoom(Mid);
        const details = await GetRoomDetails(Mid);
        setMeet(data);
        setRoomDetails(details);
        if (data?.title) {
          document.title = `ثبت نام در | ${data.title}`;
        }
      } catch (error) {
        console.error('Error fetching meeting info:', error);
      } finally {
        setLoadingInitial(false);
      }
    };
    fetchMeetingInfo();
  }, [Mid]);

  // Determine dynamic background
  const bgImage = roomDetails?.metadatas?.image_cover || roomDetails?.logo;
  const bgStyle = bgImage
    ? { backgroundImage: `url(${bgImage})`, backgroundSize: 'cover', backgroundPosition: 'center', backgroundRepeat: 'no-repeat' }
    : {};

  const price = roomDetails?.metadatas?.price;
  const oldprice = roomDetails?.metadatas?.oldprice;
  const support = roomDetails?.metadatas?.support;
  const isRegistrationDisabled = roomDetails?.metadatas?.isRegistrationEnabled === "false" || roomDetails?.metadatas?.isRegistrationEnabled === false;
  const isSubscribedRoom = meet?.isSubscribed || roomDetails?.isSubscribed;

  const handleReceiptUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploadingReceipt(true);
    try {
      const res = await UploadFiles([file]);
      let fileUrl = '';
      if (res && res.urls && res.urls.length > 0) fileUrl = res.urls[0];
      else if (Array.isArray(res) && res.length > 0) fileUrl = typeof res[0] === 'string' ? res[0] : res[0].url || res[0].path;
      else if (res && res.url) fileUrl = res.url;
      else if (res && res.data) fileUrl = Array.isArray(res.data) ? res.data[0] : res.data;
      
      if (fileUrl) {
        setReceiptUrl(fileUrl);
      } else {
        toast.showError('خطا در دریافت آدرس فایل آپلود شده');
      }
    } catch (err) {
      toast.showError('خطا در آپلود فایل رسید');
    } finally {
      setUploadingReceipt(false);
    }
  };

  // Handlers
  const handleDirectSubmit = async () => {
    if (!firstname.trim() || !lastname.trim() || !nationalCode.trim() || !phone.trim()) {
      toast.showError('لطفاً تمامی فیلدها را پر کنید.');
      return;
    }
    setOtpLoading(true);
    try {
      const response = await DirectRoomSubscribedRegistration({
        RoomId: Mid,
        ReceiptUrl: receiptUrl
      });
      if (response && response.status === 200) {
        setIsSuccess(true);
        toast.showSuccess('درخواست شما با موفقیت ثبت شد!');
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        toast.showError(response?.message || 'خطا در ثبت درخواست.');
      }
    } catch (error) {
      toast.showError('خطا در برقراری ارتباط با سرور.');
    } finally {
      setOtpLoading(false);
    }
  };
  const handleSendOTP = async () => {
    if (!firstname.trim() || !lastname.trim() || !nationalCode.trim() || !phone.trim()) {
      toast.showError('لطفاً تمامی فیلدها را پر کنید.');
      return;
    }
    setOtpLoading(true);
    setVerifyError('');
    try {
      const response = await SendOTPRoomSubscribedRegistration({
        nationalCode: toEnglishDigits(nationalCode),
        phone: toEnglishDigits(phone),
        ReceiptUrl: receiptUrl
      });
      
      if (response && response.status === 200) {
        setOtpGuid(response.guid);
        setMaskedPhone(response.maskedPhone || toEnglishDigits(phone));
        setIsOtpSent(true);
        setOtpTimer(60);
        toast.showSuccess('کد تایید برای شما ارسال شد.');
      } else {
        toast.showError(response?.message || 'خطا در ارسال کد تایید.');
      }
    } catch (error) {
      toast.showError('خطا در برقراری ارتباط با سرور.');
    } finally {
      setOtpLoading(false);
    }
  };

  const handleConfirmOTP = async () => {
    if (!otpCode.trim()) return;
    setVerificationLoading(true);
    setVerifyError('');
    try {
      const response = await ConfirmOTPRoomSubscribedRegistration({
        code: toEnglishDigits(otpCode),
        guid: otpGuid,
        phone: toEnglishDigits(phone),
        nationalCode: toEnglishDigits(nationalCode),
        firstname: firstname,
        lastname: lastname,
        roomId: Mid,
        ReceiptUrl: receiptUrl
      });

      if (response && response.status === 200) {
        // Save token and sync
        setCookieWithConfig('vc2AuthNorm', response.token, 365 * 10);
        syncUserTokens();
        
        setIsSuccess(true);
        toast.showSuccess('درخواست شما با موفقیت ثبت شد!');
        setTimeout(() => {
          window.location.reload();
        }, 1000);
      } else {
        setVerifyError(response?.message || 'کد وارد شده صحیح نمی‌باشد.');
      }
    } catch (error) {
      setVerifyError('خطا در ارتباط با سرور. لطفاً مجدداً تلاش کنید.');
    } finally {
      setVerificationLoading(false);
    }
  };

  if (loadingInitial) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-100" dir="rtl">
        <div className="flex flex-col items-center gap-4">
          <svg className="animate-spin h-10 w-10 text-emerald-600" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          <p className="text-gray-600 font-bold text-sm animate-pulse">در حال دریافت اطلاعات اتاق...</p>
        </div>
      </div>
    );
  }

  return (
    <div className={`min-h-screen flex items-center justify-center p-4 relative ${!bgImage && 'bg-gradient-to-br from-emerald-800 via-teal-900 to-emerald-950'}`} dir="rtl" style={bgStyle}>
      {bgImage && <div className="absolute inset-0 bg-black/60 z-0"></div>}
      
      <div className="bg-white/95 backdrop-blur-md rounded-[2.5rem] shadow-[0_20px_60px_-15px_rgba(0,0,0,0.5)] w-full max-w-lg overflow-hidden border border-white/20 relative z-10 transition-all duration-500 ease-in-out p-6 md:p-10">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-gradient-to-tr from-emerald-100 to-teal-50 text-4xl mb-4 shadow-inner">
             {isSuccess ? '✅' : '📝'}
          </div>
          <h1 className="text-2xl font-black text-gray-800 tracking-tight">درخواست عضویت در کلاس</h1>
          {meet?.title && (
            <p className="mt-2 text-sm text-gray-500 font-medium">کلاس: {meet.title}</p>
          )}
        </div>

        {isSuccess || roomDetails?.isSubscribedByUser ? (
          <div className="flex flex-col items-center justify-center gap-4 py-8">
            <div className="w-24 h-24 bg-emerald-100 rounded-full flex items-center justify-center animate-bounce">
              {roomDetails?.subscriptionStatus === 98 ? (
                <span className="text-5xl">⏳</span>
              ) : roomDetails?.subscriptionStatus === 100 ? (
                <span className="text-5xl">✅</span>
              ) : roomDetails?.subscriptionStatus === 0 ? (
                <span className="text-5xl">❌</span>
              ) : (
                <svg className="w-12 h-12 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                </svg>
              )}
            </div>
            
            {roomDetails?.subscriptionStatus === 98 ? (
              <>
                <h2 className="text-xl font-bold text-blue-800">درخواست شما در لیست انتظار است</h2>
                <p className="text-sm text-gray-600 text-center px-4">
                  درخواست عضویت شما در این کلاس قبلاً ثبت شده و هم‌اکنون در انتظار تایید مدیر می‌باشد.
                </p>
              </>
            ) : roomDetails?.subscriptionStatus === 100 ? (
              <>
                <h2 className="text-xl font-bold text-emerald-800">شما عضو این کلاس هستید</h2>
                <p className="text-sm text-gray-600 text-center px-4">
                  درخواست شما تایید شده است و شما هم‌اکنون عضو این کلاس هستید.
                </p>
              </>
            ) : roomDetails?.subscriptionStatus === 0 ? (
              <>
                <h2 className="text-xl font-bold text-red-800">درخواست شما رد شده است</h2>
                <p className="text-sm text-gray-600 text-center px-4">
                  متاسفانه مدیر کلاس درخواست شما را تایید نکرده است.
                </p>
              </>
            ) : (
              <>
                <h2 className="text-xl font-bold text-emerald-800">درخواست شما با موفقیت ثبت شد</h2>
                <p className="text-sm text-gray-600 text-center px-4">
                  درخواست عضویت شما در این کلاس ثبت شد و هم‌اکنون در انتظار تایید مدیر می‌باشد. 
                  پس از تایید مدیر، شما می‌توانید وارد این کلاس شوید.
                </p>
              </>
            )}
            <button
              onClick={() => navigate(`/rooms/${Mid}/join`)}
              className="mt-6 w-full py-3 bg-gray-100 hover:bg-gray-200 text-gray-800 rounded-xl font-bold transition-colors"
            >
              بازگشت به صفحه ورود
            </button>
          </div>
        ) : (
          <div className="space-y-6">
            {!isOtpSent ? (
              <div className="space-y-4">
                {isRegistrationDisabled && (
                  <div className="bg-red-50 border border-red-100 p-4 rounded-2xl flex gap-3 text-sm text-red-800 mb-4">
                    <span className="text-xl">🚫</span>
                    <p className="font-medium leading-relaxed pt-1">
                      ثبت نام در این کلاس در حال حاضر غیرفعال می‌باشد.
                    </p>
                  </div>
                )}
                
                {!isRegistrationDisabled && (
                  <div className="bg-blue-50 border border-blue-100 p-4 rounded-2xl flex gap-3 text-sm text-blue-800">
                    <span className="text-xl">ℹ️</span>
                    <p className="font-medium leading-relaxed pt-1">
                      برای ارسال درخواست عضویت در این کلاس، لطفاً اطلاعات خود را به صورت کامل وارد نمایید.
                    </p>
                  </div>
                )}
                
                {isSubscribedRoom && (price || support) && (
                  <div className="bg-emerald-50 border border-emerald-100 p-4 rounded-2xl text-sm text-emerald-800 space-y-2 mt-4">
                    {price && (
                      <div className="flex items-center gap-2 font-bold">
                        <span>هزینه اشتراک:</span>
                        {oldprice && (
                          <span className="text-red-500 line-through text-xs font-normal opacity-70">{oldprice}</span>
                        )}
                        <span className="text-emerald-700 bg-emerald-100 px-2 py-0.5 rounded-lg">{price} تومان</span>
                      </div>
                    )}
                    {support && (
                      <div className="flex items-start gap-2">
                        <span className="text-lg">📞</span>
                        <p className="font-medium leading-relaxed">پشتیبانی: {support}</p>
                      </div>
                    )}
                  </div>
                )}

                <div className="grid grid-cols-2 gap-4 mt-4">
                  <div className="space-y-1.5">
                    <label className="text-xs font-bold text-gray-600 mr-2 block">نام</label>
                    <input
                      type="text"
                      disabled={isRegistrationDisabled || !!(user && user.fullname?.split(' ')[0])}
                      value={firstname}
                      onChange={(e) => setFirstname(e.target.value)}
                      className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 transition-all outline-none bg-gray-50/50 hover:bg-white text-sm text-gray-800 disabled:opacity-60 disabled:bg-gray-100"
                      placeholder="مثال: علی"
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-xs font-bold text-gray-600 mr-2 block">نام خانوادگی</label>
                    <input
                      type="text"
                      disabled={isRegistrationDisabled || !!(user && user.fullname?.split(' ').slice(1).join(' '))}
                      value={lastname}
                      onChange={(e) => setLastname(e.target.value)}
                      className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 transition-all outline-none bg-gray-50/50 hover:bg-white text-sm text-gray-800 disabled:opacity-60 disabled:bg-gray-100"
                      placeholder="مثال: محمدی"
                    />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold text-gray-600 mr-2 block">کد ملی</label>
                  <input
                    type="text"
                    disabled={isRegistrationDisabled || !!(user && user.nationalCode)}
                    value={nationalCode}
                    onChange={(e) => setNationalCode(e.target.value)}
                    dir="ltr"
                    className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 transition-all outline-none bg-gray-50/50 hover:bg-white text-sm font-medium tracking-wider text-center text-gray-800 placeholder-gray-400 disabled:opacity-60 disabled:bg-gray-100"
                    placeholder="کد ملی 10 رقمی"
                  />
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold text-gray-600 mr-2 block">شماره موبایل</label>
                  <input
                    type="text"
                    disabled={isRegistrationDisabled || !!(user && user.phone)}
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    dir="ltr"
                    className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 transition-all outline-none bg-gray-50/50 hover:bg-white text-sm font-medium tracking-wider text-center text-gray-800 placeholder-gray-400 disabled:opacity-60 disabled:bg-gray-100"
                    placeholder="09120000000"
                  />
                </div>

                {isSubscribedRoom && (
                  <div className="mt-4 p-4 border border-gray-200 rounded-2xl bg-gray-50 space-y-4">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={hasPaid}
                        disabled={isRegistrationDisabled}
                        onChange={(e) => setHasPaid(e.target.checked)}
                        className="w-5 h-5 accent-emerald-600 rounded cursor-pointer disabled:opacity-60"
                      />
                      <span className="text-sm font-bold text-gray-700">پرداخت کرده‌ام (ارسال رسید)</span>
                    </label>

                    {hasPaid && (
                      <div className="pt-2">
                        <label className="text-xs font-bold text-gray-600 mr-2 block mb-2">تصویر فیش واریزی</label>
                        <div className="flex items-center gap-3">
                          <label className={`flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-2xl border-2 border-dashed ${receiptUrl ? 'border-emerald-300 bg-emerald-50' : 'border-gray-300 bg-white hover:bg-gray-50'} transition-all cursor-pointer ${isRegistrationDisabled ? 'opacity-60 pointer-events-none' : ''}`}>
                            {uploadingReceipt ? (
                              <svg className="animate-spin h-5 w-5 text-emerald-600" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                              </svg>
                            ) : receiptUrl ? (
                              <span className="text-emerald-700 text-sm font-bold flex items-center gap-1">
                                <span>✅</span> رسید آپلود شد
                              </span>
                            ) : (
                              <span className="text-gray-500 text-sm font-medium">انتخاب و آپلود تصویر...</span>
                            )}
                            <input
                              type="file"
                              accept="image/*"
                              className="hidden"
                              disabled={uploadingReceipt || isRegistrationDisabled}
                              onChange={handleReceiptUpload}
                            />
                          </label>
                          {receiptUrl && (
                            <div className="w-12 h-12 rounded-xl overflow-hidden border border-emerald-200 shrink-0 bg-white">
                              <img src={receiptUrl} alt="Receipt" className="w-full h-full object-cover" />
                            </div>
                          )}
                        </div>
                      </div>
                    )}
                  </div>
                )}

                {user?.phone ? (
                  <button
                    type="button"
                    onClick={handleDirectSubmit}
                    disabled={otpLoading || !nationalCode || !phone || !firstname || !lastname || isRegistrationDisabled || (hasPaid && !receiptUrl) || uploadingReceipt}
                    className="w-full py-3.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-2xl font-bold shadow-lg shadow-emerald-200 transition-all active:scale-95 flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed mt-4"
                  >
                    {otpLoading ? (
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                    ) : (
                      'ثبت درخواست'
                    )}
                  </button>
                ) : (
                  <button
                    type="button"
                    onClick={handleSendOTP}
                    disabled={otpLoading || !nationalCode || !phone || !firstname || !lastname || isRegistrationDisabled || (hasPaid && !receiptUrl) || uploadingReceipt}
                    className="w-full py-3.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-2xl font-bold shadow-lg shadow-emerald-200 transition-all active:scale-95 flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed mt-4"
                  >
                    {otpLoading ? (
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                    ) : (
                      'دریافت کد تایید'
                    )}
                  </button>
                )}
                
                <button
                  type="button"
                  onClick={() => navigate(`/rooms/${Mid}/join`)}
                  className="w-full py-3 bg-transparent text-gray-500 hover:text-gray-700 text-sm font-bold transition-colors"
                >
                  انصراف و بازگشت
                </button>
              </div>
            ) : (
              <div className="space-y-6 animate-fadeIn">
                <div className="text-center space-y-2">
                  <div className="inline-block px-4 py-1.5 bg-emerald-50 text-emerald-700 rounded-full text-xs font-bold border border-emerald-100 mb-2">
                    کد تایید پیامک شد
                  </div>
                  <p className="text-sm text-gray-600 font-medium leading-relaxed">
                    کد ۴ رقمی ارسال شده به شماره
                    <span className="inline-block mx-2 px-2 py-0.5 bg-gray-100 rounded text-gray-800 font-bold" dir="ltr">{maskedPhone}</span>
                    را وارد کنید.
                  </p>
                  <button 
                    onClick={() => setIsOtpSent(false)} 
                    className="text-xs text-blue-500 hover:text-blue-700 font-bold underline underline-offset-4"
                  >
                    ویرایش اطلاعات
                  </button>
                </div>

                <div className="space-y-2">
                  <input
                    type="text"
                    value={otpCode}
                    onChange={(e) => {
                      setOtpCode(e.target.value);
                      setVerifyError('');
                    }}
                    dir="ltr"
                    maxLength={4}
                    className={`w-full px-4 py-4 rounded-2xl border-2 text-center tracking-[0.5em] font-black text-2xl outline-none transition-all ${
                      verifyError 
                        ? 'border-red-300 focus:border-red-500 focus:ring-4 focus:ring-red-500/10 text-red-600 bg-red-50/50' 
                        : 'border-emerald-100 focus:border-emerald-500 focus:ring-4 focus:ring-emerald-500/10 text-gray-800 bg-emerald-50/30'
                    }`}
                    placeholder="----"
                  />
                  {verifyError && (
                    <p className="text-red-500 text-xs font-bold text-center mt-2 flex items-center justify-center gap-1">
                      <span>⚠️</span> {verifyError}
                    </p>
                  )}
                </div>

                <div className="flex flex-col gap-3">
                  <button
                    type="button"
                    onClick={handleConfirmOTP}
                    disabled={verificationLoading || otpCode.length !== 4}
                    className="w-full py-4 bg-emerald-600 hover:bg-emerald-700 text-white rounded-2xl font-bold shadow-lg shadow-emerald-200 transition-all active:scale-95 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {verificationLoading ? (
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                    ) : (
                      'تایید و ثبت درخواست'
                    )}
                  </button>
                  
                  <div className="text-center mt-2">
                    {otpTimer > 0 ? (
                      <p className="text-xs text-gray-500 font-medium">
                        ارسال مجدد کد تا <span className="font-bold text-emerald-600 inline-block w-8 text-left" dir="ltr">00:{otpTimer.toString().padStart(2, '0')}</span>
                      </p>
                    ) : (
                      <button 
                        onClick={handleSendOTP} 
                        disabled={otpLoading}
                        className="text-xs font-bold text-emerald-600 hover:text-emerald-800 flex items-center justify-center gap-1 mx-auto p-2 rounded-lg hover:bg-emerald-50 transition-colors"
                      >
                        <span>🔄</span> ارسال مجدد کد تایید
                      </button>
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

export default RoomSubRequest;
