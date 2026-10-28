// src/pages/Home.jsx
import React, { useRef } from "react";
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
import { startNewMeet, VerifyToken } from "@/api/StartNewMeeting";
import { timeAgo } from "@/Tools/tools";
import Cookies from 'js-cookie';
import { Navigate, useNavigate, useLocation, Outlet } from 'react-router-dom';
import useFetch from "../Tools/Hooks/UseFetchHook";
import { useAuth } from "../Tools/AuthProvider";
import PersianCalendarModal from "./Dashboard/modal/PersianCalanderModal";
import { FaCalendarPlus, FaPaintBrush, FaSignInAlt } from "react-icons/fa";
import PlanRoomModal from "./Dashboard/modal/PlanRoomModal";
import { joinMeeting } from "../api/StartNewMeeting";
import MobileEmojiModal from "../Component/MobileEmojiModal";
import { ChangeAvatarOfRoom, GetPlanRoom } from "../api/AdminAPI";
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
      const planDate = new Date(plan.startAt);
      return planDate > now;
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

      setRooms(data);
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

  const handleMainButtonClick = (room) => {
    const isLive = roomLive.some(r => r.roomID === room.roomID);

    if (isLive) {
      // اگر جلسه زنده است -> مستقیم ورود
      handleStart(room.roomID);
    } else {
      // اگر جلسه زنده نیست -> شروع
      const futurePlans = getFuturePlans(room.planJSONs);

      if (futurePlans.length > 0) {
        // اگر plan آینده داره -> منو باز کن
        setShowPlanMenu(showPlanMenu === room.roomID ? null : room.roomID);
      } else {
        console.log("aa")
        // اگر plan نداره -> مستقیم شروع کن
        setSelectedRoomForMeeting(room)
        setIsStartMeetingModalOpen(true)
      }
    }
  };
  const handleEndMeet = async (roomID) => {
    CheckLives()

  }
  const handleStart = async (roomID, planID = -1, title = "", teachers = [], dabir = null, hasCert = false, ispublic = true) => {
    console.log("handleStart", planID)
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

      if (roomLive.some(room => room.roomID === roomID)) {
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
        const data = await startNewMeet(roomID, user.fullname, title, teacherStrings, dabir, planID, hasCert);
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

  const handleEmojiSelect = (emojiName, brand, roomid) => {
    console.log(`Selected: ${emojiName} from ${brand}`);
    // e.g., save to state, insert into text, etc.
    var res = ChangeAvatarOfRoom({
      "roomID": roomid,
      "avatarPath": `/${brand}/${emojiName}`
    })
    if (res?.status == 200) {
      alert("ایموجی انتخاب شد")
      setModalOpenMobileEmojiModal(false)
    } else {
      alert("error")
      console.error(res)
    }
  };


  // بقیه ساختار کارت‌ها...
  return (
    <div className="container mx-auto px-4 py-8 max-w-7xl">
      <QuickAccessToolbar />

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
        onStartWithoutInfo={(data) => { handleStart(data.roomId) }}
        teachersList={teachers}
        onStartMeeting={(data) => { handleStart(data.roomId, 0, data.title, data.teachers, data.scientificSecretary?.name || null, true, data.isPublic) }}
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
