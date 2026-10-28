// src/pages/Home.jsx
import React from "react";
import Header from "../Component/header";
import Hero from "../Component/Home/Hero";
// بعداً بقیه کامپوننت‌ها رو import می‌کنیم
import { useState, useEffect } from "react";
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
import { startNewMeet } from "@/api/StartNewMeeting";
import { timeAgo } from "@/Tools/tools";
import Cookies from 'js-cookie';
import { getSlideshows } from "../api/GetRooms";
import { Navigate, useNavigate, useLocation, Outlet } from 'react-router-dom';
import RoomCard, { RoomCardLoadingGrid } from "../Component/Home/RoomCard";
import ModernMediaSlider from "../Component/Home/Sldieshow";
import { fetchRoomLivesPublic, fetchRoomsPublic, GetHekmatShobe } from "../api/GetRooms";
import { FaLeaf } from "react-icons/fa";        // آیکون پیشنهادی برای ایتا
import { IoChatbubbleEllipses } from "react-icons/io5"; // آیکون بله
import { HiDevicePhoneMobile } from "react-icons/hi2";   // آیکون روبیکا
import BranchItem from "../Component/Home/ShoabList";
import { GetPlanRoom } from "../api/AdminAPI";
import { GetPlanRoomPublic } from "../api/GetRooms";
import { MeetingMenu } from "../Component/Home/MeetingListMenu";
import LatestMeetings from "../Component/Home/LatestMeeting";
import LiveSectionSlider from "../Component/LiveSlider";
import StatsSection from "../Component/Home/StatsSection";
const HomeGuest = () => {
  const navigate = useNavigate();
  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isStartingMeet, setIsStartingMeet] = useState(null);
  const [roomLive, setRoomLive] = useState([]); // This will hold the list of live rooms
  const [roomLiveLoading, setRoomLiveLoading] = useState(true); // This will hold the list of live rooms
  const [isMeetingLive, setIsMeetingLive] = useState(false); // State to track if the current meeting is live
  const [slideshowList, setSlideshowList] = useState([])
  const [isSlideShowLoading, setIsSlideShowLoading] = useState(true)
  const [isLoadingShoab, setIsLoadingShoab] = useState(true)
  const [shoabList, setShoabList] = useState([])
  const [isPlanListLoading, setIsPlanListLoading] = useState(true)
  const [planList, setPlanList] = useState([])

  useEffect(() => {
    loadRooms();
    const intervalId = setInterval(CheckLives, 20000);
    CheckLives()
    document.title = "مجمع عالی حکمت اسلامی | صفحه نخست";
  }, []);

  const loadRooms = async () => {
    try {
      const sl = await getSlideshows();
      setSlideshowList(sl)
      setIsSlideShowLoading(false)
      const shoab = await GetHekmatShobe()
      setShoabList(shoab)
      setIsLoadingShoab(false)
      const plans = await GetPlanRoomPublic()
      console.log(plans.data)
      setPlanList(plans.data)
      setIsPlanListLoading(false)

      const data = await fetchRoomsPublic();
      setRooms(data);
    } catch (err) {
      console.error(err);
      setRooms([]);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isLoadingShoab && isSlideShowLoading && isPlanListLoading && isSlideShowLoading) {
    return (
      <div className="container py-5">
        <div className="row">در حال دریافت اطلاعات...</div>
      </div>
    );
  }

  if (!rooms || rooms.length === 0) {
    // return (
    //   <div className="container py-5">
    //     <div className="row g-4">
    //       گروهی برای نمایش وجود ندارد
    //     </div>
    //   </div>
    // );
  }
  const handleStart = async (roomID) => {
    setIsStartingMeet(roomID); // شروع لودینگ
    try {
      const data = await startNewMeet(roomID, "مجمع عالی حکمت اسلامی");
      console.warn(data)
      if (data?.redirectURL) {
        window.open(data.redirectURL, '_blank');
      } else {
        alert("لینک ورود یافت نشد.");
      }
    } catch (err) {
      console.error(err);
      alert("خطا در شروع جلسه");
    } finally {
      setIsStartingMeet(false); // پایان لودینگ، چه با موفقیت چه با خطا
    }
  };


  async function CheckLives() {
    try {
      var livedata = await fetchRoomLivesPublic(); // Fetch the list of live rooms
      setRoomLive(livedata); // Store the live rooms data
      setRoomLiveLoading(false)
    } catch (error) {
      console.error("Error fetching live rooms:", error);
      // در اینجا می‌توانید مدیریت خطا را انجام دهید، مثلاً نمایش پیام به کاربر
    }
  }

  const mediaItems = [
    { type: 'image', src: 'https://hek.either.ir/img/slider/DIN%20VA%20SAYBERTENIC%20148-87.jpg' },
    // { type: 'gif', src: 'https://img.soft98.ir/1/ircd6.gif' }, // Example GIF
    // { type: 'video', src: 'https://pistachihub.com/assets/video_2025-02-12_21-31-39-Dfep7rxj.mp4' }, // Example video URL
    // { type: 'image', src: 'https://panel.kaprila.com/storage/upload/4aa733177ccb4b6a52888dfd05819423112be696/2026/01/13/6966231344bce.webp' },
  ];
  // بقیه ساختار کارت‌ها...
  return (
    <div className="min-h-screen bg-gradient-to-b from-[#f0fdf4] via-[#f9fafb] to-[#ffffff] ">

      {/* Hero Slider + Sidebars - در یک ردیف */}
      <div className="bg-gradient-to-b from-[#02592A] via-[#4A8C6F] to-[#f0fdf4]" style={{
        '--tw-gradient-via-position': '72%'
      }}>

        <div className="mx-auto max-w-10/12 px-4 sm:px-6 lg:px-8 py-8 sm:py-12 lg:py-16 ">
          <div className="flex flex-col lg:flex-row gap-6 lg:gap-8">
            {/* قسمت خالی سمت چپ - که بعداً میتونید چیز دیگه ای بذارید */}

            <div className="flex-1 min-w-0 min-h-0 ">
              <div className="h-full " dir="ltr"> {/* این div را اضافه کنید */}
                <LiveSectionSlider
                  roomLive={roomLive}
                  rooms={rooms}
                  loading={roomLiveLoading || loading}
                  title="پخش زنده"
                  subtitle="پخش زنده مجمع عالی حکمت اسلامی در اینجا نمایش داده میشود"
                />            </div>
            </div>

            {/* RIGHT SIDEBAR - اسلایدر عمودی با سایز محدود */}
            <div className="lg:w-80 flex-shrink-0">
              <div className="sticky top-28">
                <div className="h-[400px] sm:h-[500px]">
                  <ModernMediaSlider items={slideshowList} loading={isSlideShowLoading} />
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>

      {/* Main content container */}
      <div className="mx-auto max-w-10/12 px-4 sm:px-6 lg:px-8 py-8 sm:py-12 lg:py-16
      
      ">
      
        <div className="flex flex-col lg:flex-row gap-8 lg:gap-12">

          {/* ========== LEFT SIDEBAR - شعب حکمت (آکاردئونی) ========== */}
          <aside className="lg:w-80 flex-shrink-0">
            <div className="sticky top-28">
              <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden">
                <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-5 py-4">
                  <h3 className="text-[#ffffff] font-bold  flex items-center gap-2">
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                    </svg>
                    پخش مجازی دفاتر مجمع عالی حکمت
                  </h3>
                </div>
                <div className="divide-y divide-[#f3f4f6]">
                  {shoabList.map((branch) => (
                    <BranchItem key={branch.id} branch={branch} lives={roomLive} />
                  ))}
                </div>

                <div className="p-4 bg-[#f9fafb] border-t border-[#f3f4f6]">
                  <a
                    href="#"
                    className="flex items-center justify-center gap-2 text-[#30d4cc] text-sm font-medium hover:gap-3 transition-all"
                  >
                    <span>مشاهده همه شعب</span>
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </a>
                </div>
              </div>
              <MeetingMenu planList={planList} />

            </div>

          </aside>

          {/* ========== MAIN CONTENT - Live Streams & Groups ========== */}
          <div className="flex-1 min-w-0">
      <StatsSection />

            {/* LIVE STREAMS SECTION */}
            <section className="mb-16 sm:mb-20 lg:mb-24">
              <div className="text-center mb-8 sm:mb-12">
                <div className="inline-flex items-center gap-2 bg-[#fef2f2] px-4 py-1.5 rounded-full mb-4">
                  <span className="relative flex h-2 w-2">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
                  </span>
                  <span className="text-[#dc2626] text-sm font-semibold tracking-wide">پخش زنده</span>
                </div>
                <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-[#111827] mb-3">
                  پخش زنده
                </h2>
                <p className="text-[#6b7280] text-sm sm:text-base max-w-2xl mx-auto">
                  پخش زنده مجمع عالی حکمت اسلامی در اینجا نمایش داده میشود
                </p>
              </div>

              {roomLiveLoading || loading ? (
                <RoomCardLoadingGrid count={3} />

              ) : roomLive && roomLive.length > 0 ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 lg:gap-8">
                  {roomLive.map((item) => {
                    const fullRoom = rooms.find(room => room.roomID === item.roomID);
                    if (fullRoom) {
                      return (
                        <div key={item.roomID} className="transform transition-all duration-300 hover:-translate-y-1">
                          <RoomCard i={fullRoom} isRoomLive={true} live={item} />
                        </div>
                      );
                    }
                    return null;
                  })}
                </div>
              ) : (
                <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] p-10 sm:p-12 text-center">
                  <div className="w-20 h-20 bg-[#f3f4f6] rounded-full flex items-center justify-center mx-auto mb-4">
                    <svg className="w-10 h-10 text-[#9ca3af]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                  </div>
                  <p className="text-[#6b7280] text-lg font-medium">متاسفانه در حال حاضر پخش زنده‌ای در دسترس نیست.</p>
                  <p className="text-[#9ca3af] text-sm mt-2">لطفاً بعداً مجدداً تلاش کنید</p>
                </div>
              )}
            </section>

            {/* Decorative divider */}
            <div className="relative my-12 sm:my-16">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-[#e5e7eb]"></div>
              </div>
              <div className="relative flex justify-center">
                <span className="bg-gradient-to-b from-[#f9fafb] to-[#ffffff] px-4 text-[#d1d5db]">
                  <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd" />
                  </svg>
                </span>
              </div>
            </div>

            {/* OTHER GROUPS SECTION */}
            <section>
              <div className="text-center mb-8 sm:mb-12" dir="rtl">
                <div className="inline-flex items-center gap-2 bg-[#30d4cc]/10 px-4 py-1.5 rounded-full mb-4">
                  <span className="text-[#30d4cc] text-sm font-semibold tracking-wide">گروه‌های علمی</span>
                </div>
                <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-[#111827] mb-3">
                  گروه‌های دیگر
                </h2>
                <p className="text-[#6b7280] text-sm sm:text-base max-w-2xl mx-auto">
                  گروه های عمومی مجمع عالی حکمت اسلامی در اینجا نمایش داده میشود
                </p>
              </div>

              {loading ? (
                <>
                  <div className="mb-6 flex justify-between items-center bg-[#ffffff] rounded-xl px-5 py-3 shadow-sm border border-[#f3f4f6]">
                    <div className="flex items-center gap-2">
                      <div className="w-2 h-2 bg-[#30d4cc] rounded-full"></div>
                      <span className="text-sm text-[#4b5563]">تعداد کل گروه‌ها:</span>
                    </div>
                    <span className="font-bold text-[#30d4cc] text-lg"></span>
                  </div>
                  <RoomCardLoadingGrid count={6} />
                </>

              ) : (!rooms || rooms.length === 0) ? (
                <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] p-10 sm:p-12 text-center">
                  <div className="w-20 h-20 bg-[#f3f4f6] rounded-full flex items-center justify-center mx-auto mb-4">
                    <svg className="w-10 h-10 text-[#9ca3af]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                    </svg>
                  </div>
                  <p className="text-[#6b7280] text-lg font-medium">در حال حاضر گروهی در دسترس نیست.</p>
                  <p className="text-[#9ca3af] text-sm mt-2">به زودی گروه‌های جدید اضافه خواهند شد</p>
                </div>
              ) : (
                <>
                  <div className="mb-6 flex justify-between items-center bg-[#ffffff] rounded-xl px-5 py-3 shadow-sm border border-[#f3f4f6]">
                    <div className="flex items-center gap-2">
                      <div className="w-2 h-2 bg-[#30d4cc] rounded-full"></div>
                      <span className="text-sm text-[#4b5563]">تعداد کل گروه‌ها:</span>
                    </div>
                    <span className="font-bold text-[#30d4cc] text-lg">{rooms.length}</span>
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 lg:gap-8">
                    {rooms.map((item) => {
                      const isLive = roomLive.some(live => live.roomID === item.roomID);
                      const live = roomLive.find(room => room.roomID === item.roomID);
                      return (
                        <div key={item.roomID} className="transform transition-all duration-300 hover:-translate-y-1">
                          <RoomCard i={item} isRoomLive={isLive} live={live} />
                        </div>
                      );
                    })}
                  </div>
                </>
              )}
            </section>
          </div>
        </div>

        {/* Call‑to‑Action banner */}
        <div className="mt-16 sm:mt-20 text-center">
          <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] rounded-2xl p-6 sm:p-8 text-white shadow-lg">
            <h3 className="text-xl sm:text-2xl font-bold mb-2">مجمع حکمت اسلامی را در صفحات مجازی دنبال کنید</h3>
            <p className="text-white/80 text-sm sm:text-base mb-5">
              برای اطلاع از جدیدترین جلسات و رویدادها، ما را در شبکه‌های اجتماعی دنبال کنید
            </p>
            <div className="flex justify-center gap-3 flex-wrap">
              <a href="https://eitaa.com/hekmateislami">
                <button className="bg-white/20 hover:bg-white/30 px-5 py-2 rounded-lg transition-all duration-300 text-sm font-medium backdrop-blur-sm flex items-center gap-2">
                  <FaLeaf />
                  ایتا
                </button>
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomeGuest;
