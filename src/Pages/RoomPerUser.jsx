// ========== کد اول: کامپوننت اصلی ProfilePage.jsx ==========
// این فایل بر اساس منطق RoomPerPage.jsx ساخته شده و تمام state و useEffect در همان کامپوننت قرار دارد.
import { useParams, Navigate } from 'react-router-dom';
import React, { useState, useEffect } from 'react';
import RoomCard from "../Component/Home/RoomCard";
import { fetchRoomsPublic, fetchRoomLivesPublic, getSlideshows, getProfile } from "../api/GetRooms";
import { FaLeaf } from "react-icons/fa";
import NotFoundPage from './Error/404';
// src/pages/Home.jsx
import Header from "../Component/header";
import Hero from "../Component/Home/Hero";
// بعداً بقیه کامپوننت‌ها رو import می‌کنیم
import ImageAboutUsVideoThumb from "@assets/ax/ups/video_cap.jpg";
import VideoAboutUsVideoThumb from "@assets/ax/ups/video_2025-02-12_21-31-39.mp4";
import cert1 from "@assets/ax/ups/2025/02/Group-863.jpg";
import cert2 from "@assets/ax/ups/2025/02/cer1.png";
import { CategoryCard } from "@/Component/Home/Category";
import ImageMain from "@assets/ax/ups/2025/02/Vector_2.png";
import SpecialOfferBox from "@/Component/Home/SpecialOfferBox";
import GridMainProductCard from "@/Component/Home/Product/GridMainProductCard";
import ArticlesCarousel from "@/Component/Home/News/ArticlesCarousel";
import ImageBackgroundContactUS from "@assets/ax/ups/2025/04/back.png";
import ImageSignContactUS from "@assets/ax/ups/2025/04/r.php-63.png";
import ContactusField from "@/Component/Home/ContactUs/ContactUsField";
import ContactUsFieldIconOnly from "@/Component/Home/ContactUs/ContactUsFieldIconOnly";
import Footer from "@/Component/Footer";
import { useLazyInView } from "@/Tools/LazyLoading";
import { FadeIn } from "@/Tools/FadeIn";
import FooterUpper from "@/Component/FooterUpper";
import HeaderStickTop from "../Component/HeaderStickTop";
import { fetchRoomLive, fetchRooms } from "@/api/GetRooms.jsx"; // مسیر دلخواه تو پروژه
import { startNewMeet, VerifyToken } from "@/api/StartNewMeeting";
import { timeAgo } from "@/Tools/tools";
import Cookies from 'js-cookie';
import {  useNavigate, useLocation, Outlet } from 'react-router-dom';
import useFetch from "../Tools/Hooks/UseFetchHook";
import { useAuth } from "../Tools/AuthProvider";

function ProfilePage() {
  const { Uid } = useParams();  // از روت "/@:username"
  const { user, loading: authLoading } = useAuth();
    console.log(Uid)
 

  // ---------- State های مشابه صفحه اصلی ----------
  const [rooms, setRooms] = useState(null);               // لیست گروه‌های این پروفایل
  const [loading, setLoading] = useState(true);          // لودینگ گروه‌ها
  const [roomLive, setRoomLive] = useState([]);          // همه پخش‌های زنده (جهانی)
  const [roomLiveLoading, setRoomLiveLoading] = useState(true);
  const [slideshowList, setSlideshowList] = useState([]); // در صورت نیاز (می‌توانید حذف کنید)
  const [isSlideShowLoading, setIsSlideShowLoading] = useState(true);

  // ---------- State های مخصوص پروفایل ----------
  const [profileData, setProfileData] = useState(null);   // { id, name, bio, avatar, followersCount }
  const [profileLoading, setProfileLoading] = useState(true);
  const [isFollowing, setIsFollowing] = useState(false);  // آیا کاربر فعلی دنبال می‌کند؟
  const [followLoading, setFollowLoading] = useState(false);
 // اعتبارسنجی نام کاربری (باید با @ شروع شود)
  if (!Uid.startsWith('@')) {
    return <NotFoundPage />;
  }
  const Username = Uid.replace("@","")
  // ---------- توابع مشابه صفحه اصلی ----------
  const loadRooms = async () => {
    try {
      // اسلایدشو (در صورت نیاز)
      const sl = await getSlideshows();
      setSlideshowList(sl);
      setIsSlideShowLoading(false);
      // دریافت گروه‌های مربوط به این Username
      const data = await fetchRooms(Username);
      setRooms(data);
    } catch (err) {
      console.error(err);
      setRooms([]);
    } finally {
      setLoading(false);
    }
  };

  const checkLives = async () => {
    try {
      const livedata = await fetchRoomLivesPublic();
      setRoomLive(livedata);
      setRoomLiveLoading(false);
    } catch (error) {
      console.error("Error fetching live rooms:", error);
    }
  };

  // // ---------- توابع مخصوص پروفایل (شما باید API واقعی را جایگزین کنید) ----------
  const fetchProfileInfo = async () => {
    try {
      const response = await getProfile(Username);
      setProfileData(response);
      setIsFollowing(response.isFollowing);
      setProfileLoading(false);
    } catch (err) {
      console.error(err);
      setProfileData(null);
      setProfileLoading(false);
    }
  };

  const handleFollowToggle = async () => {
    // بررسی مجوز
    if (!user?.permissions?.includes('follow_user')) {
      alert('شما مجوز دنبال کردن ندارید');
      return;
    }
    if (profileData?.id === user?.id) {
      alert('نمی‌توانید خودتان را دنبال کنید');
      return;
    }
    setFollowLoading(true);
    try {
      if (isFollowing) {
        // await unfollowUser(profileData.id);
        setIsFollowing(false);
        setProfileData(prev => ({ ...prev, followersCount: prev.followersCount - 1 }));
      } else {
        // await followUser(profileData.id);
        setIsFollowing(true);
        setProfileData(prev => ({ ...prev, followersCount: prev.followersCount + 1 }));
      }
    } catch (err) {
      console.error(err);
      alert('خطا در انجام عملیات');
    } finally {
      setFollowLoading(false);
    }
  };

  // ---------- useEffect ها ----------
  useEffect(() => {
    document.title = `${Username} | پروفایل`;
    loadRooms();
    checkLives();
    fetchProfileInfo();

    const intervalId = setInterval(checkLives, 20000);
    return () => clearInterval(intervalId);
  }, [Username]);

  // نمایش لودینگ اولیه
  if (profileLoading || loading) {
    return (
      <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white flex items-center justify-center">
        <div className="flex flex-col items-center space-y-4">
          <div className="relative w-16 h-16">
            <div className="absolute inset-0 border-4 border-gray-200 rounded-full"></div>
            <div className="absolute inset-0 border-4 border-[#30d4cc] rounded-full border-t-transparent animate-spin"></div>
          </div>
          <p className="text-gray-500 font-medium">در حال بارگذاری پروفایل...</p>
        </div>
      </div>
    );
  }

  // if (!profileData) {
  //   return <NotFoundPage />;
  // }

  // نمایش دکمه دنبال کردن (فقط در صورت داشتن مجوز و غیر از خود کاربر)
  const showFollowButton = user?.permissions?.includes('follow_user') && profileData?.id !== user?.id;

  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white">
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 py-6 sm:py-8 lg:py-12">
        {/* ========== هدر پروفایل ========== */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden mb-8 sm:mb-12">
          {/* کاور تزئینی */}
          <div className="h-32 sm:h-40 lg:h-48 bg-gradient-to-r from-[#0b3f32] to-[#30d4cc]/80"></div>
          <div className="px-4 sm:px-6 pb-6 relative">
            <div className="relative -mt-12 sm:-mt-16 mb-4">
              <div className="w-24 h-24 sm:w-32 sm:h-32 rounded-full bg-white p-1 shadow-lg mx-auto sm:mx-0">
                <img
                  src={profileData.avatar || '/default-avatar.png'}
                  alt={profileData.name}
                  className="w-full h-full rounded-full object-cover"
                />
              </div>
            </div>
            <div className="flex flex-col sm:flex-row sm:justify-between sm:items-start gap-4">
              <div className="text-center sm:text-right">
                <h1 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-gray-900 mb-2">
                  {profileData.name}
                </h1>
                {profileData.bio && (
                  <p className="text-gray-600 text-sm sm:text-base max-w-2xl">{profileData.bio}</p>
                )}
                <div className="flex flex-wrap gap-3 justify-center sm:justify-start mt-3 text-sm text-gray-500">
                  <span className="flex items-center gap-1">
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                    </svg>
                    {rooms?.length || 0} گروه
                  </span>
                  <span className="flex items-center gap-1">
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                    </svg>
                    {profileData.followersCount || 0} دنبال‌کننده
                  </span>
                </div>
              </div>
              {showFollowButton && (
                <div className="flex justify-center sm:justify-end">
                  <button
                    onClick={handleFollowToggle}
                    disabled={followLoading}
                    className={`flex items-center gap-2 px-6 py-2.5 rounded-lg font-semibold transition-all duration-300 ${
                      isFollowing
                        ? 'bg-gray-100 text-gray-700 hover:bg-gray-200 border border-gray-300'
                        : 'bg-[#30d4cc] text-white hover:bg-[#26b8b0] shadow-sm'
                    } disabled:opacity-50 disabled:cursor-not-allowed`}
                  >
                    {followLoading ? (
                      <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    ) : isFollowing ? (
                      <>✅ دنبال شده</>
                    ) : (
                      <>➕ دنبال کردن</>
                    )}
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* ========== بخش پخش زنده (فقط گروه‌های این پروفایل که زنده هستند) ========== */}
        <section className="mb-16 sm:mb-20 lg:mb-24">
          <div className="text-center mb-8 sm:mb-12">
            <div className="inline-flex items-center gap-2 bg-red-50 px-4 py-1.5 rounded-full mb-4">
              <span className="relative flex h-2 w-2">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-2 w-2 bg-red-500"></span>
              </span>
              <span className="text-red-600 text-sm font-semibold tracking-wide">پخش زنده</span>
            </div>
            <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-gray-900 mb-3">پخش زنده</h2>
            <p className="text-gray-500 text-sm sm:text-base max-w-2xl mx-auto">
              پخش‌های زنده {profileData.name}
            </p>
          </div>

          {roomLiveLoading ? (
            <div className="flex justify-center items-center py-20">
              <div className="flex flex-col items-center space-y-4">
                <div className="relative w-16 h-16">
                  <div className="absolute inset-0 border-4 border-gray-200 rounded-full"></div>
                  <div className="absolute inset-0 border-4 border-[#30d4cc] rounded-full border-t-transparent animate-spin"></div>
                </div>
                <p className="text-gray-500 font-medium">در حال بارگذاری پخش‌های زنده...</p>
              </div>
            </div>
          ) : (() => {
            // فقط گروه‌های این پروفایل که در حال پخش زنده هستند
            const liveRoomsOfProfile = roomLive.filter(live => rooms?.some(r => r.roomID === live.roomID));
            if (liveRoomsOfProfile.length === 0) {
              return (
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-10 sm:p-12 text-center">
                  <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                    <svg className="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <p className="text-gray-500 text-lg font-medium">در حال حاضر پخش زنده‌ای از این مجموعه در دسترس نیست.</p>
                  <p className="text-gray-400 text-sm mt-2">لطفاً بعداً مجدداً تلاش کنید</p>
                </div>
              );
            }
            return (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5 sm:gap-6 lg:gap-8">
                {liveRoomsOfProfile.map((item) => (
                  <div key={item.roomID} className="transform transition-all duration-300 hover:-translate-y-1">
                    <RoomCard i={item} isRoomLive={true} />
                  </div>
                ))}
              </div>
            );
          })()}
        </section>

        {/* ========== دکوراتیو جداکننده ========== */}
        <div className="relative my-12 sm:my-16">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-gray-200"></div>
          </div>
          <div className="relative flex justify-center">
            <span className="bg-gradient-to-b from-gray-50 to-white px-4 text-gray-300">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" />
              </svg>
            </span>
          </div>
        </div>

        {/* ========== همه گروه‌های این پروفایل ========== */}
        <section>
          <div className="text-center mb-8 sm:mb-12">
            <div className="inline-flex items-center gap-2 bg-[#30d4cc]/10 px-4 py-1.5 rounded-full mb-4">
              <span className="text-[#30d4cc] text-sm font-semibold tracking-wide">گروه‌های {profileData.name}</span>
            </div>
            <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-gray-900 mb-3">همه گروه‌ها</h2>
            <p className="text-gray-500 text-sm sm:text-base max-w-2xl mx-auto">
              گروه‌های فعال {profileData.name}
            </p>
          </div>

          {!rooms || rooms.length === 0 ? (
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-10 sm:p-12 text-center">
              <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </div>
              <p className="text-gray-500 text-lg font-medium">در حال حاضر گروهی در دسترس نیست.</p>
              <p className="text-gray-400 text-sm mt-2">به زودی گروه‌های جدید اضافه خواهند شد</p>
            </div>
          ) : (
            <>
              <div className="mb-6 flex justify-between items-center bg-white rounded-xl px-5 py-3 shadow-sm border border-gray-100">
                <div className="flex items-center gap-2">
                  <div className="w-2 h-2 bg-[#30d4cc] rounded-full"></div>
                  <span className="text-sm text-gray-600">تعداد کل گروه‌ها:</span>
                </div>
                <span className="font-bold text-[#30d4cc] text-lg">{rooms.length}</span>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5 sm:gap-6 lg:gap-8">
                {rooms.map((item) => {
                  const isLive = roomLive.some(live => live.roomID === item.roomID);
                  return (
                    <div key={item.roomID} className="transform transition-all duration-300 hover:-translate-y-1">
                      <RoomCard i={item} isRoomLive={isLive} />
                    </div>
                  );
                })}
              </div>
            </>
          )}
        </section>

        {/* ========== بنر دنبال کردن در شبکه‌های اجتماعی (اختیاری) ========== */}
        <div className="mt-16 sm:mt-20 text-center">
          <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] rounded-2xl p-6 sm:p-8 text-white shadow-lg">
            <h3 className="text-xl sm:text-2xl font-bold mb-2">{profileData.name} را در صفحات مجازی دنبال کنید</h3>
            <p className="text-white/80 text-sm sm:text-base mb-5">
              برای اطلاع از جدیدترین جلسات و رویدادها، ما را در شبکه‌های اجتماعی دنبال کنید
            </p>
            <div className="flex justify-center gap-3 flex-wrap">
              <a href="https://eitaa.com/hekmateislami" target="_blank" rel="noopener noreferrer">
                <button className="bg-white/20 hover:bg-white/30 px-5 py-2 rounded-lg transition-all duration-300 text-sm font-medium backdrop-blur-sm flex items-center gap-2">
                  <FaLeaf /> ایتا
                </button>
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ProfilePage;