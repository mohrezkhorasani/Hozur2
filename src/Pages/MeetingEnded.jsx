// src/pages/MeetingEnded.jsx
import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { FaHome, FaCalendarAlt, FaUser, FaInfoCircle, FaClock, FaMicrophoneAlt, FaHourglassHalf } from 'react-icons/fa';
import moment from 'moment-jalaali';
import { fetchRoom } from '@/api/GetRooms'; // مسیر API شما

const MeetingEnded = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [roomData, setRoomData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [countdown, setCountdown] = useState(10);

  // دریافت reason و roomid از URL
  const reason = decodeURIComponent(searchParams.get('reason') || '');
  const roomId = searchParams.get('roomid');

  useEffect(() => {
    const getRoomDetails = async () => {
      if (!roomId) {
        setError('شناسه جلسه یافت نشد');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const data = await fetchRoom(roomId);
        setRoomData(data);
        setError(null);
      } catch (err) {
        console.error('خطا در دریافت اطلاعات جلسه:', err);
        setError('مشکلی در دریافت اطلاعات جلسه پیش آمد');
      } finally {
        setLoading(false);
      }
    };

    getRoomDetails();
  }, [roomId]);

  // تایمر 10 ثانیه برای ریدایرکت
  useEffect(() => {
    if (!loading && !error) {
      const timer = setInterval(() => {
        setCountdown((prev) => {
          if (prev <= 1) {
            clearInterval(timer);
            navigate('/rooms');
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [loading, error, navigate]);

  // نمایش وضعیت بارگذاری
  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto mb-4"></div>
          <p className="text-gray-600">در حال بارگذاری اطلاعات جلسه...</p>
        </div>
      </div>
    );
  }

  // نمایش خطا
  if (error || !roomData) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-xl max-w-md w-full p-6 text-center">
          <div className="text-red-500 text-6xl mb-4">⚠️</div>
          <h2 className="text-xl font-bold text-gray-800 mb-2">خطا در دریافت اطلاعات</h2>
          <p className="text-gray-600 mb-6">{error || 'جلسه مورد نظر یافت نشد'}</p>
          <button
            onClick={() => navigate('/')}
            className="px-6 py-2 bg-emerald-600 text-white rounded-xl hover:bg-emerald-700 transition-colors"
          >
            بازگشت به صفحه اصلی
          </button>
        </div>
      </div>
    );
  }

  // تاریخ جلسه بعدی (در صورت وجود)
  const nextMeetingDate = roomData.nextMeet ? moment(roomData.nextMeet).format('jYYYY/jMM/jDD HH:mm') : null;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 py-8 px-4">
      <div className="max-w-3xl mx-auto">
        {/* کارت اصلی */}
        <div className="bg-white rounded-2xl shadow-xl overflow-hidden">
          
          {/* هدر با آیکون پایان جلسه */}
          <div className="bg-gradient-to-r from-red-500 to-red-600 px-6 py-8 text-center">
            <div className="inline-flex items-center justify-center w-20 h-20 bg-white/20 rounded-full mb-4">
              <FaMicrophoneAlt className="text-white text-3xl" />
            </div>
            <h1 className="text-2xl md:text-3xl font-bold text-white mb-2">جلسه به پایان رسید</h1>
            <div className="w-20 h-1 bg-white/40 mx-auto rounded-full"></div>
          </div>

          {/* نوار تایمر زیر هدر قرمز */}
          <div className="bg-gradient-to-r from-amber-500 to-orange-500 px-6 py-3">
            <div className="flex items-center justify-between gap-4">
              <div className="flex items-center gap-2 text-white">
                <FaHourglassHalf className="text-lg animate-pulse" />
                <span className="text-sm font-medium">انتقال خودکار به صفحه جلسات</span>
              </div>
              <div className="bg-white/20 backdrop-blur-sm rounded-lg px-4 py-1.5">
                <span className="text-white font-bold text-xl">{countdown}</span>
                <span className="text-white text-sm mr-1">ثانیه</span>
              </div>
            </div>
            {/* نوار پیشرفت */}
            <div className="w-full bg-white/30 rounded-full h-1.5 mt-2 overflow-hidden">
              <div 
                className="bg-white h-1.5 rounded-full transition-all duration-1000 ease-linear"
                style={{ width: `${(countdown / 10) * 100}%` }}
              ></div>
            </div>
          </div>

          {/* محتوای اصلی */}
          <div className="p-6 md:p-8">
            
            {/* دلیل پایان جلسه */}
            {reason && (
              <div className="mb-8 p-4 bg-red-50 border-r-4 border-red-500 rounded-lg">
                <div className="flex items-start gap-3">
                  <FaInfoCircle className="text-red-500 text-lg mt-0.5" />
                  <div>
                    <h3 className="font-bold text-red-700 mb-1">علت پایان جلسه:</h3>
                    <p className="text-red-800">{reason}</p>
                  </div>
                </div>
              </div>
            )}

            {/* اطلاعات جلسه */}
            <div className="space-y-4 mb-8">
              <div className="flex items-start gap-3 pb-3 border-b border-gray-100">
                <FaCalendarAlt className="text-emerald-600 text-lg mt-0.5" />
                <div>
                  <p className="text-xs text-gray-500">نام جلسه</p>
                  <p className="font-semibold text-gray-800">{roomData.title || '—'}</p>
                </div>
              </div>

              {roomData.owner?.fullname && (
                <div className="flex items-start gap-3 pb-3 border-b border-gray-100">
                  <FaUser className="text-emerald-600 text-lg mt-0.5" />
                  <div>
                    <p className="text-xs text-gray-500">برگزار کننده</p>
                    <p className="font-semibold text-gray-800">{roomData.owner.fullname}</p>
                  </div>
                </div>
              )}

              {roomData.latestTitle && (
                <div className="flex items-start gap-3 pb-3 border-b border-gray-100">
                  <FaInfoCircle className="text-emerald-600 text-lg mt-0.5" />
                  <div>
                    <p className="text-xs text-gray-500">آخرین موضوع جلسه</p>
                    <p className="font-semibold text-gray-800">{roomData.latestTitle}</p>
                  </div>
                </div>
              )}

              {nextMeetingDate && (
                <div className="flex items-start gap-3">
                  <FaClock className="text-emerald-600 text-lg mt-0.5" />
                  <div>
                    <p className="text-xs text-gray-500">زمان جلسه بعدی</p>
                    <p className="font-semibold text-emerald-700">{nextMeetingDate}</p>
                  </div>
                </div>
              )}
            </div>

            {/* دکمه‌های اقدام - فقط دکمه صفحه اصلی با تایمر */}
            <div className="flex flex-col sm:flex-row gap-3 pt-4 border-t border-gray-100">
              <button
                onClick={() => navigate('/rooms')}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-emerald-600 text-white rounded-xl font-semibold hover:bg-emerald-700 transition-all duration-200"
              >
                <FaHome />
                <span>رفتن به لیست جلسات</span>
                <span className="mr-2 text-sm bg-white/20 px-2 py-0.5 rounded-full">
                  {countdown}
                </span>
              </button>
            </div>
            
            {/* متن راهنما */}
            <p className="text-center text-xs text-gray-400 mt-4">
              پس از {countdown} ثانیه به طور خودکار به لیست جلسات منتقل می‌شوید
            </p>
          </div>
        </div>

        {/* فوتر */}
        <div className="text-center mt-6 text-sm text-gray-400">
          <p>سامانه مجمع عالی حکمت اسلامی</p>
        </div>
      </div>
    </div>
  );
};

export default MeetingEnded;