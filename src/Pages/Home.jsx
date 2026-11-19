// src/pages/Home.jsx
import React, { useRef, useState, useEffect, useMemo } from "react";
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
import { timeAgo, formatPersianDateTime, parseUtcDate, isPlanDeleted } from "@/Tools/tools";
import Cookies from 'js-cookie';
import { Navigate, useNavigate, useLocation, Outlet } from 'react-router-dom';
import useFetch from "../Tools/Hooks/UseFetchHook";
import { useAuth } from "../Tools/AuthProvider";
import PersianCalendarModal from "./Dashboard/modal/PersianCalanderModal";
import { FaCalendarPlus, FaPaintBrush, FaSignInAlt, FaClock, FaChalkboardTeacher, FaPlay, FaChevronDown, FaChevronUp, FaUsers } from "react-icons/fa";
import PlanRoomModal from "./Dashboard/modal/PlanRoomModal";
import { joinMeeting } from "../api/StartNewMeeting";
import MobileEmojiModal from "../Component/MobileEmojiModal";
import { ChangeAvatarOfRoom, GetPlanRoom, GetMyUpcomingPlans } from "../api/AdminAPI";
import EmojiIcon from "../Component/EmojiIcon";
import { GetTeacherList } from "../api/GetRooms";
import StartMeetingModal from "./Admin/startMeetModal";
import RoomCardOwn from "../Component/Home/roomCardOwn";
import DayMeetingsModal from "../Component/Home/MiniCalanderMeetingsModal";
import QuickAccessToolbar from "../Component/QuickAccess";
import EditRoomLargeModal from "../Modal/Admin/EditRoomLargeModal";



const HomePage = () => {
  const navigate = useNavigate();
  const [rooms, setRooms] = useState(null);
  const [loadingRooms, setLoadingRooms] = useState(true);
  const [isStartingMeet, setIsStartingMeet] = useState(null);
  const [roomLive, setRoomLive] = useState([]); // This will hold the list of live rooms
  const [isMeetingLive, setIsMeetingLive] = useState(false); // State to track if the current meeting is live
  const [token, setToken] = useState(Cookies.get("vc2AuthNorm"))
  const { user, loading, refetchUser } = useAuth();
  const [teachers, setTeachers] = useState([])
  const [teachersLoading, setTeachersLoading] = useState(true)

  const [isPersianCalandarOpen, setIsPersianCalandarOpen] = useState(false);
  const [plans, setPlans] = useState([]);
  const [plansLoading, setPlansLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [selectedDayMeetings, setSelectedDayMeetings] = useState([]);
  const [selectedDayNumber, setSelectedDayNumber] = useState(null);
  const [selectedYearNumber, setSelectedYearNumber] = useState(null);
  const [selectedMonthNumber, setSelectedMonthNumber] = useState(null);


  const [activeMenu, setActiveMenu] = useState(null); // برای کنترل باز/بسته شدن منو
  const [selectedRoomForMeeting, setSelectedRoomForMeeting] = useState(null); // برای تنظیم جلسه آتی
  const [isPlanModalOpen, setIsPlanModalOpen] = useState(false);
  const [modalOpenMobileEmojiModal, setModalOpenMobileEmojiModal] = useState(false);
  const [isStartMeetingModalOpen, setIsStartMeetingModalOpen] = useState(false);

  const [showPlanMenu, setShowPlanMenu] = useState(null);
  const [isEditRoomModalOpen, setIsEditRoomModalOpen] = useState(false);
  const [selectedRoomIdForEdit, setSelectedRoomIdForEdit] = useState(null);
  const menuRef = useRef({});
  const [flippedCards, setFlippedCards] = useState({});
  const [upcomingPlans, setUpcomingPlans] = useState([]);
  const [loadingUpcomingPlans, setLoadingUpcomingPlans] = useState(false);
  const [isUpcomingExpanded, setIsUpcomingExpanded] = useState(false);

  // Helper to determine plan remaining time & Jalali date
  // «در حال برگزاری» صرفاً از طریق متد getlives تعیین می‌شود و هرگز بر اساس تاریخ محاسبه نخواهد شد
  const getPlanTimingInfo = (startDateStr, isLive = false) => {
    const dateFormatted = startDateStr ? formatPersianDateTime(startDateStr) : '-';
    if (isLive) {
      return {
        remainingText: 'در حال برگزاری',
        dateStr: dateFormatted,
        isOngoing: true,
      };
    }
    if (!startDateStr) return { remainingText: '-', dateStr: '-', isOngoing: false };
    const now = new Date();
    const start = parseUtcDate(startDateStr);
    if (!start) return { remainingText: '-', dateStr: '-', isOngoing: false };

    const diffMs = start.getTime() - now.getTime();
    const diffMins = Math.round(diffMs / 60000);

    // اگر زنده نیست، هرگز بر اساس تاریخ نباید «در حال برگزاری» باشد
    if (diffMs <= 0) {
      return {
        remainingText: 'در انتظار شروع',
        dateStr: dateFormatted,
        isOngoing: false,
        isWaiting: true,
      };
    }

    const totalMinutes = diffMins;
    const days = Math.floor(totalMinutes / (24 * 60));
    const hours = Math.floor((totalMinutes % (24 * 60)) / 60);
    const minutes = totalMinutes % 60;

    let remainingText = '';
    if (days > 0) {
      remainingText = `${days} روز${hours > 0 ? ` و ${hours} ساعت` : ''} مانده`;
    } else if (hours > 0) {
      remainingText = `${hours} ساعت${minutes > 0 ? ` و ${minutes} دقیقه` : ''} مانده`;
    } else {
      remainingText = `${Math.max(1, minutes)} دقیقه مانده`;
    }

    return { remainingText, dateStr: dateFormatted, isOngoing: false, isWaiting: false };
  };

  // Helper to check if a plan's room has started (is live) from getlives (like roomcards do)
  const isPlanLive = (plan) => {
    if (!plan || !Array.isArray(roomLive) || roomLive.length === 0) return false;
    const fid = String(plan.roomFriendlyId || plan.roomObj?.friendlyID || plan.roomObj?.roomID || '').toLowerCase().trim();
    const bbbId = String(plan.roomObj?.BBB_MeetID || plan.roomObj?.bbbMeetID || '').toLowerCase().trim();
    const numId = String(plan.roomObj?.id || plan.roomObj?.ID || '').trim();

    return roomLive.some(r => {
      if (!r) return false;
      const liveId = String(r.roomID || r.RoomID || '').toLowerCase().trim();
      const liveBbb = String(r.bbb_MeetID || r.BBB_MeetID || '').toLowerCase().trim();
      return (fid && (liveId === fid || liveBbb === fid)) ||
             (bbbId && (liveId === bbbId || liveBbb === bbbId)) ||
             (numId && liveId === numId);
    });
  };

  // Aggregate upcoming plans from both GetMyUpcomingPlans and rooms' planJSONs
  // Strictly filters out test rooms (only keeps rooms with Type in [1, 2] and Status === 100)
  const activeUpcomingPlans = useMemo(() => {
    const plansMap = new Map();

    // Fast lookup map of loaded rooms
    const roomsLookup = new Map();
    if (Array.isArray(rooms)) {
      rooms.forEach(r => {
        if (!r) return;
        const fid = String(r.friendlyID || r.friendlyId || r.roomID || r.RoomID || '').toLowerCase().trim();
        const numId = String(r.id || r.ID || '').trim();
        if (fid) roomsLookup.set(fid, r);
        if (numId) roomsLookup.set(numId, r);
      });
    }

    // Helper: room Type MUST be 1 (ROOM_NORMAL) or 2 (ROOM_CREATED_BY_ADMIN)
    const isRoomTypeEligible = (typeVal) => {
      if (typeVal === undefined || typeVal === null || isNaN(typeVal)) return true;
      const n = Number(typeVal);
      return n === 1 || n === 2;
    };

    // 1. Add plans from upcomingPlans (if loaded from API)
    if (Array.isArray(upcomingPlans)) {
      upcomingPlans.forEach(p => {
        if (!p || (!p.id && !p.planId)) return;
        const planId = p.id || p.planId;

        // Exclude deleted plans (-400) and test/ignored (-99, 99)
        if (isPlanDeleted(p)) return;
        const pType = Number(p.type !== undefined ? p.type : p.Type);
        if (pType === -99 || pType === 99) return;

        const fid = String(p.roomFriendlyId || p.roomID || '').toLowerCase().trim();
        const numId = String(p.roomId || '').trim();
        const matchingRoom = (fid && roomsLookup.get(fid)) || (numId && roomsLookup.get(numId)) || null;

        if (matchingRoom) {
          const rType = matchingRoom.type !== undefined ? matchingRoom.type : matchingRoom.Type;
          if (!isRoomTypeEligible(rType)) return;
          const rStatus = Number(matchingRoom.status !== undefined ? matchingRoom.status : matchingRoom.Status);
          if (rStatus !== 100 && rStatus !== 1) return;
        } else {
          const pRoomType = p.roomType !== undefined ? p.roomType : p.RoomType;
          if (pRoomType !== undefined && pRoomType !== null && !isRoomTypeEligible(pRoomType)) return;
        }

        plansMap.set(String(planId), {
          id: planId,
          title: p.title || 'جلسه بدون عنوان',
          roomFriendlyId: p.roomFriendlyId || p.roomID || (matchingRoom ? (matchingRoom.friendlyID || matchingRoom.roomID) : ''),
          roomTitle: (matchingRoom ? (matchingRoom.title || matchingRoom.Title) : '') || p.roomTitle || 'اتاق جلسه',
          teacherName: p.teacherName || (typeof p.teacher === 'string' ? p.teacher : p.teacher?.name) || p.teachers || '',
          dabir: p.dabir || null,
          startDate: p.startDate || p.startAt,
          roomObj: matchingRoom
        });
      });
    }

    // 2. Also aggregate from all loaded rooms (fall-back & enrichment)
    if (Array.isArray(rooms)) {
      rooms.forEach(r => {
        if (!r) return;
        const rType = r.type !== undefined ? r.type : r.Type;
        if (!isRoomTypeEligible(rType)) return;
        const rStatus = Number(r.status !== undefined ? r.status : r.Status);
        if (rStatus !== 100 && rStatus !== 1) return;

        const plans = r.planJSONs || r.PlanJSONs || r.plan || [];
        if (Array.isArray(plans)) {
          plans.forEach(p => {
            const pId = p.id || p.ID;
            if (!pId) return;
            if (isPlanDeleted(p)) return;
            const pType = Number(p.type !== undefined ? p.type : p.Type);
            if (pType === -99 || pType === 99) return;

            const start = p.startAt || p.startDate || p.StartAt || p.StartDate;
            if (start && !plansMap.has(String(pId))) {
              const teacherStr = typeof p.teacher === 'string'
                ? p.teacher
                : (p.teacher?.name || (Array.isArray(p.teachers) ? p.teachers.map(t => typeof t === 'string' ? t : t.name).filter(Boolean).join('، ') : ''));

              plansMap.set(String(pId), {
                id: pId,
                title: p.title || p.Title || 'جلسه بدون عنوان',
                roomFriendlyId: r.friendlyID || r.friendlyId || r.roomID || r.RoomID,
                roomTitle: r.title || r.Title || r.friendlyID,
                teacherName: teacherStr || '',
                dabir: p.dabir || p.Dabir || null,
                startDate: start,
                roomObj: r
              });
            }
          });
        }
      });
    }

    // 3. Filter out plans that are already expired!
    const now = new Date();
    const list = Array.from(plansMap.values()).filter(plan => {
      if (isPlanLive(plan)) return true;
      if (!plan.startDate) return false;
      const start = parseUtcDate(plan.startDate);
      if (!start) return false;
      // Not expired: start is in future or started less than 3 hours ago (active session window)
      return (start.getTime() - now.getTime()) > (-3 * 3600 * 1000);
    });

    // 4. Sort: Live sessions first, then ascending by startDate
    list.sort((a, b) => {
      const aLive = isPlanLive(a);
      const bLive = isPlanLive(b);
      if (aLive && !bLive) return -1;
      if (!aLive && bLive) return 1;
      const da = parseUtcDate(a.startDate);
      const db = parseUtcDate(b.startDate);
      return (da?.getTime() || 0) - (db?.getTime() || 0);
    });
    return list;
  }, [upcomingPlans, rooms, roomLive]);

  const handleFlip = (roomId) => {
    setFlippedCards(prev => ({
      ...prev,
      [roomId]: !prev[roomId]
    }));
  };

  const getFuturePlans = (plans) => {
    if (!plans || !Array.isArray(plans)) return [];
    const now = new Date();
    return plans.filter(plan => {
      if (isPlanDeleted(plan)) return false;
      const planDate = parseUtcDate(plan.startAt || plan.startDate);
      return planDate && planDate.getTime() > now.getTime();
    });
  };
  const handleDayClick = (meetings, day, month, year) => {
    if (!meetings) return;
    if (meetings.length === 0) return;
    setSelectedDayMeetings(meetings);
    setSelectedDayNumber(day);
    setSelectedMonthNumber(month)
    setSelectedYearNumber(year)
    setModalOpen(true);
  };

  const loadRooms = async () => {
    try {
      const data = await fetchRooms();
      setRooms(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error(err);
      setRooms([]);
    } finally {
      setLoadingRooms(false);
    }
  };
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (showPlanMenu && menuRef.current[showPlanMenu] &&
        !menuRef.current[showPlanMenu].contains(event.target)) {
        setShowPlanMenu(null);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [showPlanMenu]);
  useEffect(() => {
    const initialize = async () => {
      loadRooms();
      loadteachers();
      loadUpcomingPlans();
      const intervalId = setInterval(CheckLives, 20000);
      CheckLives();
      document.title = "مجمع عالی حکمت اسلامی | صفحه شما";
      // Return cleanup function for the interval
      return () => clearInterval(intervalId);
    };

    const loadteachers = async () => {
      const teas = await GetTeacherList("");
      setTeachersLoading(false)
      setTeachers(teas)
    };

    const loadUpcomingPlans = async () => {
      setLoadingUpcomingPlans(true);
      try {
        const data = await GetMyUpcomingPlans();
        if (Array.isArray(data)) {
          setUpcomingPlans(data);
        }
      } catch (err) {
        console.warn("Failed to load upcoming plans:", err);
      } finally {
        setLoadingUpcomingPlans(false);
      }
    };
    initialize();
  }, [token, navigate]);
  useEffect(() => {
    const checkUserPermission = async () => {
      if (user == null) return
      if (user.permissions.includes("create_meeting")) {
        return;
      }
      navigate("/rooms")
    };
    checkUserPermission()
  }, [user])

  if (token === undefined) {
    navigate("/rooms");
    return
  }


  if (loadingRooms) {
    return (
      <div className="container py-5">
        <div className="row">در حال دریافت اطلاعات...</div>
      </div>
    );
  }

  if (!rooms || rooms.length === 0) {
    return (
      <div className="container py-5">
        <div className="row g-4">
          گروهی برای نمایش وجود ندارد
        </div>
      </div>
    );
  }

  const handleMainButtonClick = async (room) => {
    const isLive = roomLive.some(r => r.roomID === room.roomID);

    if (isLive) {
      // اگر جلسه زنده است -> مستقیم ورود
      handleStart(room.roomID);
    } else {
      // اگر جلسه زنده نیست -> شروع: دریافت مستقیم و بدون کش برنامه‌ها
      let freshPlans = room.planJSONs;
      try {
        const directPlans = await GetPlanRoom(room.roomID, 0, 50, 100, false);
        if (directPlans && Array.isArray(directPlans)) {
          freshPlans = directPlans;
          setRooms(prev => prev ? prev.map(r => r.roomID === room.roomID ? { ...r, planJSONs: directPlans } : r) : prev);
        }
      } catch (e) {
        console.warn("Direct fetch plans fallback to room.planJSONs:", e);
      }

      const futurePlans = getFuturePlans(freshPlans);

      if (futurePlans.length > 0) {
        // اگر plan آینده داره -> منو باز کن
        setShowPlanMenu(showPlanMenu === room.roomID ? null : room.roomID);
      } else {
        // اگر plan نداره -> مستقیم شروع کن
        setSelectedRoomForMeeting({ ...room, planJSONs: freshPlans });
        setIsStartMeetingModalOpen(true);
      }
    }
  };
  const handleEndMeet = async (roomID) => {
    CheckLives()

  }
  const handleStart = async (roomID, planID = -1, title = "", teachers = [], dabir = null, hasCert = false, ispublic = true, type = null) => {
    console.log("handleStart", planID, "type:", type)
    let teacherStrings = []
    let dabire = null
    teachers.map((t) => {
      teacherStrings.push(t.name)
    })

    if (dabir)
      dabire = dabir

    console.log(teacherStrings, teachers)

    setIsStartingMeet(roomID); // شروع لودینگ
    try {
      const isAlreadyLive = Array.isArray(roomLive) && roomLive.some(room => {
        if (!room) return false;
        const rId = String(room.roomID || room.RoomID || '').toLowerCase().trim();
        const rBbb = String(room.bbb_MeetID || room.BBB_MeetID || '').toLowerCase().trim();
        const target = String(roomID || '').toLowerCase().trim();
        return (rId && rId === target) || (rBbb && rBbb === target);
      });

      if (isAlreadyLive) {
        const data = await joinMeeting(roomID, user.fullname, true);

        if (data?.link) {
          window.open(data.link, "_blank");
        } else {
          if (data?.message) {
            alert(data.message)
          } else  
            alert("لینک ورود یافت نشد.");
        }
      } else {
        const data = await startNewMeet(roomID, user.fullname, title, teacherStrings, dabir, planID, hasCert, type);
        console.warn(data)
        if (data.status != 200) {
          alert(data.message);
          return  
        }
        if (data?.redirectURL) {
          window.open(data.redirectURL, '_blank');
        } else {
          alert("لینک ورود یافت نشد.");
        }
      }
    } catch (err) {
      console.error(err);
      alert("خطا در شروع جلسه");
    } finally {
      setIsStartingMeet(false); // پایان لودینگ، چه با موفقیت چه با خطا
    }

  };
  const handleCopy = async (roomID) => {
    const link = `https://vc2.hekmateislami.com/rooms/${roomID}/join`;

    await navigator.clipboard.writeText(link);
    alert("کپی شد!");
  };

  async function CheckLives() {
    // console.log("Checking live rooms..."); // برای اطمینان از اجرای تابع
    try {
      var livedata = await fetchRoomLive(); // Fetch the list of live rooms
      setRoomLive(livedata); // Store the live rooms data
    } catch (error) {
      // console.error("Error fetching live rooms:", error);
      // در اینجا می‌توانید مدیریت خطا را انجام دهید، مثلاً نمایش پیام به کاربر
    }
  }

  const handleOpenNextMeetingModal = (room) => {
    setSelectedRoomForMeeting(room);
    setIsPersianCalandarOpen(true);
    // setShowNextMeetingModal(true); // اگر مودال دارید
    console.log('تنظیم جلسه آتی برای:', room.title);
  };

  const handleEmojiSelect = async (emojiNameOrPath, brand, roomid) => {
    try {
      let finalAvatarPath = emojiNameOrPath;
      if (brand !== 'custom') {
        finalAvatarPath = `/${brand}/${emojiNameOrPath}`;
        const res = await ChangeAvatarOfRoom({
          roomID: roomid,
          avatarPath: finalAvatarPath
        });
        if (res?.status !== 200) {
          alert(res?.message || "خطا در تنظیم آیکون");
          return;
        }
      }

      // Update room avatar in local state immediately
      setRooms(prevRooms =>
        prevRooms.map(r => (r.roomID === roomid ? { ...r, avatar: finalAvatarPath } : r))
      );
      setModalOpenMobileEmojiModal(false);
    } catch (err) {
      console.error("Error setting room avatar:", err);
      alert("خطا در تنظیم آیکون اتاق");
    }
  };


  // بقیه ساختار کارت‌ها...
  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <QuickAccessToolbar />

      {/* سکشن جلسات پیش رو */}
      {activeUpcomingPlans && activeUpcomingPlans.length > 0 && (
        <div className="mb-10 bg-[#ffffff] rounded-3xl border border-[#e5e7eb] shadow-sm p-5 sm:p-6 text-right relative overflow-visible">
          <div className="flex flex-wrap items-center justify-between gap-3 mb-5 pb-4 border-b border-[#f3f4f6]">
            <div className="flex items-center gap-3">
              <span className="p-2.5 bg-[#eff6ff] text-[#2563eb] rounded-2xl border border-[#bfdbfe]">
                <FaCalendarPlus size={20} />
              </span>
              <div>
                <h3 className="text-lg sm:text-xl font-black text-[#1f2937]">جلسات پیش‌رو</h3>
                <p className="text-xs text-[#6b7280] mt-0.5 font-medium">
                  برنامه‌ها و نشست‌های آینده اتاق‌های فعال
                </p>
              </div>
            </div>
            <span className="text-xs font-bold px-3 py-1 bg-[#eff6ff] text-[#1d4ed8] border border-[#bfdbfe] rounded-full">
              {activeUpcomingPlans.length} جلسه پیش‌رو
            </span>
          </div>

          <div className="overflow-visible min-h-[220px] pb-6">
            <table className="w-full text-right text-sm">
              <thead>
                <tr className="bg-[#f9fafb] text-[#4b5563] text-xs font-bold border-b border-[#e5e7eb]">
                  <th className="py-3 px-4 min-w-[200px]">عنوان جلسه</th>
                  <th className="py-3 px-4">گروه / اتاق</th>
                  <th className="py-3 px-4">زمان باقی‌مانده تا شروع</th>
                  <th className="py-3 px-4 text-center">عملیات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#f3f4f6]">
                {(isUpcomingExpanded ? activeUpcomingPlans : activeUpcomingPlans.slice(0, 3)).map((plan, index) => {
                  const isStarted = isPlanLive(plan);
                  const timing = getPlanTimingInfo(plan.startDate, isStarted);
                  const isStarting = isStartingMeet === plan.roomFriendlyId;
                  return (
                    <tr key={plan.id} className="relative hover:z-[70] hover:bg-[#f9fafb] transition">
                      {/* عنوان جلسه با محدودیت دو خط، سه نقطه و پاپ‌اور شناور روی هاور */}
                      <td className="py-3.5 px-4 font-bold text-[#1f2937] max-w-xs sm:max-w-md relative hover:z-[80]">
                        <div className="relative group/title inline-block w-full hover:z-[90]">
                          <p
                            className="line-clamp-2 text-ellipsis overflow-hidden text-xs sm:text-sm font-bold text-[#1f2937] leading-relaxed cursor-help hover:text-[#0284c7] transition"
                            title={plan.title}
                          >
                            {plan.title}
                          </p>

                          {/* پنجره عنوان کامل بالای موس/المان */}
                          <div
                            className={`pointer-events-none absolute right-0 w-72 sm:w-80 p-3 bg-[#0f172a] text-[#f8fafc] text-xs font-medium rounded-2xl shadow-2xl border border-[#334155] opacity-0 group-hover/title:opacity-100 transition-all duration-200 z-[100] leading-relaxed text-right ${
                              index === 0 ? 'top-full mt-2' : 'bottom-full mb-2'
                            }`}
                          >
                            <div className="text-[10px] text-[#38bdf8] font-bold mb-1">عنوان کامل جلسه:</div>
                            <div className="break-words font-semibold">{plan.title}</div>
                            {index === 0 ? (
                              <div className="absolute bottom-full right-6 -mb-px border-4 border-transparent border-b-[#0f172a]" />
                            ) : (
                              <div className="absolute top-full right-6 -mt-px border-4 border-transparent border-t-[#0f172a]" />
                            )}
                          </div>
                        </div>
                      </td>

                      {/* گروه / اتاق با هاور زیبا شامل مشخصات اتاق، شناسه و نام استاد */}
                      <td className="py-3.5 px-4 whitespace-nowrap relative hover:z-[80]">
                        <div className="relative inline-block group/group hover:z-[90]">
                          <div className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-[#f1f5f9] hover:bg-[#e2e8f0] text-[#334155] rounded-xl font-bold text-xs cursor-pointer transition border border-[#cbd5e1]/60 shadow-2xs">
                            <FaUsers className="text-[#64748b] shrink-0" size={13} />
                            <span className="max-w-[130px] sm:max-w-[180px] truncate">
                              {plan.roomTitle || plan.roomFriendlyId}
                            </span>
                          </div>

                          {/* پاپ‌اور شناور اطلاعات گروه و استاد در بالای دکمه */}
                          <div
                            className={`pointer-events-none absolute right-0 sm:right-1/2 sm:translate-x-1/2 w-64 p-3.5 bg-[#1e293b] text-[#ffffff] rounded-2xl shadow-2xl border border-[#334155] opacity-0 group-hover/group:opacity-100 group-hover/group:pointer-events-auto transition-all duration-200 z-[100] transform ${
                              index === 0
                                ? 'top-full mt-2 group-hover/group:translate-y-1'
                                : 'bottom-full mb-2 group-hover/group:-translate-y-1'
                            }`}
                          >
                            <div className="text-xs space-y-2 text-right" dir="rtl">
                              <div className="flex items-center gap-1.5 border-b border-[#334155] pb-2 text-[#38bdf8] font-black">
                                <FaUsers size={13} />
                                <span className="truncate">{plan.roomTitle || 'اتاق جلسه'}</span>
                              </div>
                              <div className="text-[11px] text-[#94a3b8] font-mono">
                                <span>شناسه: </span><span className="text-[#f1f5f9]">{plan.roomFriendlyId}</span>
                              </div>
                              <div className="flex items-center gap-1.5 text-[11px] text-[#cbd5e1]">
                                <FaChalkboardTeacher className="text-[#a855f7] shrink-0" size={13} />
                                <span>استاد:</span>
                                <strong className="text-[#ffffff] truncate">{plan.teacherName || 'تعیین نشده'}</strong>
                              </div>
                              {plan.dabir && (
                                <div className="text-[11px] text-[#cbd5e1]">
                                  <span className="text-[#94a3b8]">دبیر علمی: </span>
                                  <span className="text-[#ffffff]">{plan.dabir}</span>
                                </div>
                              )}
                            </div>
                            {index === 0 ? (
                              <div className="absolute bottom-full right-6 sm:right-1/2 sm:translate-x-1/2 -mb-px border-4 border-transparent border-b-[#1e293b]" />
                            ) : (
                              <div className="absolute top-full right-6 sm:right-1/2 sm:translate-x-1/2 -mt-px border-4 border-transparent border-t-[#1e293b]" />
                            )}
                          </div>
                        </div>
                      </td>

                      {/* زمان باقی مانده تا شروع + تاریخ شمسی */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        <div className="flex flex-col">
                          <span className={`text-xs font-bold inline-flex items-center gap-1.5 ${
                            timing.isOngoing
                              ? 'text-[#059669]'
                              : timing.isWaiting
                                ? 'text-[#d97706]'
                                : 'text-[#0284c7]'
                          }`}>
                            {timing.isOngoing && (
                              <span className="w-2 h-2 rounded-full bg-[#10b981] animate-ping shrink-0" />
                            )}
                            {timing.isWaiting && (
                              <span className="w-2 h-2 rounded-full bg-[#f59e0b] shrink-0" />
                            )}
                            <span>{timing.remainingText}</span>
                          </span>
                          <span className="text-[11px] text-[#6b7280] font-mono mt-0.5">
                            {timing.dateStr}
                          </span>
                        </div>
                      </td>

                      {/* عملیات: اگر جلسه شروع شده است، دکمه ورود (نه شروع) - برگرفته از getlives مثل roomcard */}
                      <td className="py-3.5 px-4 text-center whitespace-nowrap">
                        {isStarted ? (
                          <button
                            type="button"
                            disabled={isStarting}
                            onClick={() => {
                              const teachersArr = plan.teacherName ? [{ name: plan.teacherName }] : [];
                              handleStart(
                                plan.roomFriendlyId,
                                plan.id,
                                plan.title,
                                teachersArr,
                                plan.dabir || null,
                                true,
                                true
                              );
                            }}
                            className={`px-4 py-2 font-bold text-xs rounded-xl transition shadow-xs hover:shadow active:scale-95 flex items-center justify-center gap-1.5 mx-auto cursor-pointer ${
                              isStarting
                                ? "bg-[#f3f4f6] text-[#9ca3af] cursor-not-allowed"
                                : "bg-[#16a34a] hover:bg-[#15803d] text-[#ffffff] ring-2 ring-[#16a34a]/30"
                            }`}
                            title="ورود به جلسه آنلاین (در حال برگزاری)"
                          >
                            {isStarting ? (
                              <svg className="animate-spin h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 8.001l-2.707 2.707A9.959 9.959 0 004 20.291z"></path>
                              </svg>
                            ) : (
                              <>
                                <span className="w-2 h-2 rounded-full bg-[#ffffff] animate-ping shrink-0" />
                                <FaSignInAlt size={12} />
                                <span>ورود به جلسه</span>
                              </>
                            )}
                          </button>
                        ) : (
                          <button
                            type="button"
                            disabled={isStarting}
                            onClick={() => {
                              const teachersArr = plan.teacherName ? [{ name: plan.teacherName }] : [];
                              handleStart(
                                plan.roomFriendlyId,
                                plan.id,
                                plan.title,
                                teachersArr,
                                plan.dabir || null,
                                true,
                                true
                              );
                            }}
                            className={`px-4 py-2 font-bold text-xs rounded-xl transition shadow-xs hover:shadow active:scale-95 flex items-center justify-center gap-1.5 mx-auto cursor-pointer ${
                              isStarting
                                ? "bg-[#f3f4f6] text-[#9ca3af] cursor-not-allowed"
                                : "bg-[#0284c7] hover:bg-[#0369a1] text-[#ffffff]"
                            }`}
                            title="شروع جلسه آنلاین"
                          >
                            {isStarting ? (
                              <svg className="animate-spin h-3.5 w-3.5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 8.001l-2.707 2.707A9.959 9.959 0 004 20.291z"></path>
                              </svg>
                            ) : (
                              <>
                                <FaSignInAlt size={12} />
                                <span>شروع جلسه</span>
                              </>
                            )}
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* دکمه مشاهده بیشتر (expand) */}
          {activeUpcomingPlans.length > 3 && (
            <div className="mt-4 pt-3 border-t border-[#f3f4f6] text-center">
              <button
                type="button"
                onClick={() => setIsUpcomingExpanded(prev => !prev)}
                className="inline-flex items-center gap-2 px-5 py-2 bg-[#f8fafc] hover:bg-[#eff6ff] text-[#2563eb] border border-[#dbeafe] rounded-2xl font-bold text-xs transition cursor-pointer shadow-xs hover:shadow active:scale-95"
              >
                {isUpcomingExpanded ? (
                  <>
                    <FaChevronUp size={12} />
                    <span>نمایش کمتر (بستن)</span>
                  </>
                ) : (
                  <>
                    <FaChevronDown size={12} />
                    <span>مشاهده بیشتر ({activeUpcomingPlans.length - 3} جلسه دیگر)</span>
                  </>
                )}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Header with stats */}
      <div className="mb-8 text-right">
        <h2 className="text-2xl font-bold text-[#1f2937] mb-2">اتاق‌های جلسات</h2>
        <p className="text-[#6b7280] text-sm">
          {rooms.length} اتاق فعال |
          {rooms.filter(r => roomLive.some(room => room.roomID === r.roomID)).length} جلسه زنده
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {rooms.map((room) => (
          <RoomCardOwn
            key={room.roomID}
            room={room}
            isLive={roomLive.some(r => r.roomID === room.roomID)}
            isStartingMeet={isStartingMeet}
            showPlanMenu={showPlanMenu}
            activeMenu={activeMenu}
            flippedCards={flippedCards}
            onFlip={handleFlip}
            onMainButtonClick={handleMainButtonClick}
            onCopy={handleCopy}
            onSetShowPlanMenu={setShowPlanMenu}
            onSetActiveMenu={setActiveMenu}
            onSetSelectedRoomForMeeting={setSelectedRoomForMeeting}
            onSetIsStartMeetingModalOpen={setIsStartMeetingModalOpen}
            onSetIsPlanModalOpen={setIsPlanModalOpen}
            onHandleOpenNextMeetingModal={handleOpenNextMeetingModal}
            onSetModalOpenMobileEmojiModal={setModalOpenMobileEmojiModal}
            getFuturePlans={getFuturePlans}
            handleStart={handleStart}
            handleEndMeet={handleEndMeet}
            timeAgo={timeAgo}
            menuRef={menuRef}
            EmojiIcon={EmojiIcon}
            handleDayClick={handleDayClick}
            onOpenEditRoomModal={(roomId) => {
              setSelectedRoomIdForEdit(roomId);
              setIsEditRoomModalOpen(true);
            }}
          />
        ))}

      </div>
      <PersianCalendarModal
        isOpen={isPersianCalandarOpen}
        onClose={() => setIsPersianCalandarOpen(false)}
        onSubmit={(room) => {

        }}
        room={selectedRoomForMeeting}
        initialData={{
          startDate: null,
          title: '',
          friendlyID: "",
          teacher: '',
        }}
        teachersList={teachers}
      />
      <PlanRoomModal
        isOpen={isPlanModalOpen}
        onClose={() => setIsPlanModalOpen(false)}
        roomId={selectedRoomForMeeting?.roomID}
        roomTitle={selectedRoomForMeeting?.title}
      />
      <MobileEmojiModal
        isOpen={modalOpenMobileEmojiModal}
        onClose={() => setModalOpenMobileEmojiModal(false)}
        onSelectEmoji={handleEmojiSelect}
        roomid={selectedRoomForMeeting?.roomID}
      />
      <StartMeetingModal
        isOpen={isStartMeetingModalOpen}
        onClose={() => { setIsStartMeetingModalOpen(false) }}
        room={selectedRoomForMeeting}
        onStartWithoutInfo={(data) => { handleStart(data.roomId, -1, "", [], null, false, true, data.type ?? (data.isTest ? -99 : null)) }}
        teachersList={teachers}
        onStartMeeting={(data) => { handleStart(data.roomId, 0, data.title, data.teachers, data.scientificSecretary?.name || null, true, data.isPublic, data.type ?? (data.isTest ? -99 : null)) }}
      />
      {modalOpen && (
        <DayMeetingsModal
          isOpen={modalOpen}
          onClose={() => setModalOpen(false)}
          meetings={selectedDayMeetings}
          persianMonth={selectedMonthNumber}
          day={selectedDayNumber}
          year={selectedYearNumber}
        />
      )}
      <EditRoomLargeModal
        isOpen={isEditRoomModalOpen}
        onClose={() => setIsEditRoomModalOpen(false)}
        roomId={selectedRoomIdForEdit}
        onUpdated={loadRooms}
      />
    </div>
  );
};

export default HomePage;
