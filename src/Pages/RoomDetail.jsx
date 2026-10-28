import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faUsers,
  faChalkboardTeacher,
  faPoll,
  faCalendarAlt,
  faPlay,
  faCheckCircle,
  faCopy,
  faShareAlt,
  faChartBar,
  faTimes,
  faGraduationCap,
  faClock,
  faVideo,
  faStar,
  faArrowRight,
  faDownload,
  faInfoCircle,
  faCheck,
  faSignal,
  faBuilding,
  faEye,
  faChevronDown,
  faChevronUp,
  faSpinner,
  faVolumeUp,
  faFileAlt,
  faExternalLinkAlt
} from "@fortawesome/free-solid-svg-icons";
import { mockRoomDetail } from "../mock/roomDetailMock";
import { BaseURL, BaseURLFiles, BaseURLSite } from "../api/apiNeed";
import { combinePath } from "../Tools/tools";
import EmojiIcon from "../Component/EmojiIcon";
import {
  GetRoomDetails,
  GetMeetingDetail,
  GetTopTeachers,
  GetRoomMeetingHistory,
  fetchRoomLivesPublicWithID,
  GetUpcomingPlans
} from "../api/GetRooms";

const DEFAULT_TEACHER_AVATAR = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=300&q=80";

// ==========================================
// 1. کامپوننت اسکلتون (Shimmer Loading Skeleton)
// ==========================================
const RoomDetailSkeleton = () => {
  return (
    <div className="min-h-screen bg-gray-50 dir-rtl p-4 sm:p-6 lg:p-8 animate-pulse">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* کاور و هدر اسکلتون */}
        <div className="bg-white rounded-3xl overflow-hidden shadow-sm border border-gray-100">
          <div className="h-48 sm:h-64 bg-gray-200"></div>
          <div className="p-6 space-y-4">
            <div className="h-8 bg-gray-200 rounded-lg w-2/3"></div>
            <div className="h-4 bg-gray-200 rounded-md w-1/2"></div>
            <div className="flex gap-3 pt-4">
              <div className="h-10 bg-gray-200 rounded-xl w-32"></div>
              <div className="h-10 bg-gray-200 rounded-xl w-32"></div>
            </div>
          </div>
        </div>

        {/* کارت‌های آمار اسکلتون */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="bg-white p-5 rounded-2xl border border-gray-100 space-y-3">
              <div className="w-10 h-10 bg-gray-200 rounded-xl"></div>
              <div className="h-6 bg-gray-200 rounded w-1/2"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4"></div>
            </div>
          ))}
        </div>

        {/* آخرین جلسه اسکلتون */}
        <div className="bg-white p-6 rounded-3xl border border-gray-100 space-y-4">
          <div className="h-6 bg-gray-200 rounded w-1/4"></div>
          <div className="h-12 bg-gray-200 rounded-xl w-full"></div>
          <div className="h-4 bg-gray-200 rounded w-3/4"></div>
        </div>

        {/* بخش اساتید اسکلتون */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <div key={i} className="bg-white p-6 rounded-2xl border border-gray-100 space-y-4 text-center">
              <div className="w-20 h-20 bg-gray-200 rounded-full mx-auto"></div>
              <div className="h-5 bg-gray-200 rounded w-1/2 mx-auto"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4 mx-auto"></div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

// ==========================================
// 2. کامپوننت اصلی صفحه اختصاصی اتاق (RoomDetail)
// ==========================================
export default function RoomDetail() {
  const { roomId } = useParams();
  const navigate = useNavigate();

  // ----------------------------------------
  // State های صفحه
  // ----------------------------------------
  const [loading, setLoading] = useState(true);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [roomData, setRoomData] = useState(null);
  const [pastMeetings, setPastMeetings] = useState([]);
  const [historyPage, setHistoryPage] = useState(1);
  const [historyTotalPages, setHistoryTotalPages] = useState(1);
  const [showAllTeachers, setShowAllTeachers] = useState(false);
  const [showAllUpcoming, setShowAllUpcoming] = useState(false);
  const [activePoll, setActivePoll] = useState(null);
  const [selectedOption, setSelectedOption] = useState(null);

  // State های مربوط به مدال‌ها
  const [isPollModalOpen, setIsPollModalOpen] = useState(false);
  const [isPollResultModalOpen, setIsPollResultModalOpen] = useState(false);
  const [streamingModal, setStreamingModal] = useState({
    open: false,
    type: "video", // "video" | "audio"
    url: "",
    title: ""
  });

  // Notification Toast ساده
  const [toastMessage, setToastMessage] = useState("");

  const showToast = (msg) => {
    setToastMessage(msg);
    setTimeout(() => setToastMessage(""), 3000);
  };

  // ----------------------------------------
  // دریافت اولیه اطلاعات اتاق از API
  // ----------------------------------------
  useEffect(() => {
    async function loadAllRoomData() {
      if (!roomId) return;
      try {
        setLoading(true);
        const [detailsRes, meetingDetailRes, topTeachersRes, historyRes, livesRes, upcomingPlansRes] = await Promise.allSettled([
          GetRoomDetails(roomId),
          GetMeetingDetail(roomId),
          GetTopTeachers(roomId),
          GetRoomMeetingHistory(roomId, 1, 10),
          fetchRoomLivesPublicWithID(roomId),
          GetUpcomingPlans(roomId)
        ]);

        const details = detailsRes.status === "fulfilled" ? detailsRes.value : null;
        const meetingDetail = meetingDetailRes.status === "fulfilled" ? meetingDetailRes.value : null;
        const topTeachers = topTeachersRes.status === "fulfilled" ? topTeachersRes.value : [];
        const historyData = historyRes.status === "fulfilled" ? historyRes.value : null;
        const lives = livesRes.status === "fulfilled" ? livesRes.value : null;
        const upcomingPlansVal = upcomingPlansRes.status === "fulfilled" && Array.isArray(upcomingPlansRes.value)
          ? upcomingPlansRes.value
          : (details?.upcomingPlans && Array.isArray(details.upcomingPlans) ? details.upcomingPlans : []);

        const logoUrl = details?.logo
          ? (details.logo.startsWith("http") ? details.logo : combinePath(BaseURL, details.logo))
          : null;

        const formattedUpcomingPlans = upcomingPlansVal.map(p => {
          const d = p.startDate ? new Date(p.startDate) : null;
          const formattedDate = d && !isNaN(d.getTime())
            ? d.toLocaleDateString("fa-IR", {
                weekday: "long",
                year: "numeric",
                month: "long",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
              })
            : "زمان‌بندی شده";

          return {
            id: p.id,
            title: p.title || "نشست پیش‌رو",
            teacher: p.teacher || p.teachers || "مدرس جلسه",
            dabir: p.dabir,
            startDate: p.startDate,
            formattedDate: formattedDate,
            explain: p.explain || "",
            requiredToEnterName: p.requiredToEnterName,
            presentationFileURL: p.presentationFileURL
          };
        });
        console.log(details)
        setRoomData({
          roomID: roomId,
          isSubscribed: details?.isSubscribed,
          isAdmin: details?.isAdmin,
          isSubscribedByUser: details?.isSubscribedByUser,
          subscriptionStatus: details?.subscriptionStatus,
          title: details?.title || "اتاق آموزشی",
          description: details?.bio || "",
          logo: logoUrl,
          avatar: logoUrl,
          coverImage: logoUrl || "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?auto=format&fit=crop&w=1200&q=80",
          owner: details?.host ? {
            id: details.host.id,
            fullname: details.host.name,
            username: details.host.name
          } : null,
          stats: {
            totalMeetings: meetingDetail?.countMeet || 0,
            totalParticipants: meetingDetail?.countPartis || 0,
            totalHours: meetingDetail?.sumMinutes ? Math.round(meetingDetail.sumMinutes / 60) : 0,
            satisfactionRate: meetingDetail?.percentage ? `${meetingDetail.percentage}%` : "100%"
          },
          upcomingPlans: formattedUpcomingPlans,
          latestMeeting: meetingDetail?.lastMeeting ? {
            id: meetingDetail.lastMeeting.id,
            title: "آخرین جلسه برگزار شده",
            teacherName: meetingDetail.lastMeeting.teachers?.[0]?.name || "استاد دوره",
            date: meetingDetail.lastMeeting.date ? new Date(meetingDetail.lastMeeting.date).toLocaleDateString("fa-IR") : "",
            summary: meetingDetail.lastMeeting.summary
          } : null,
          teachers: Array.isArray(topTeachers) ? topTeachers.map(t => ({
            id: t.id || t.uuid,
            name: t.name || `${t.firstname || ''} ${t.lastname || ''}`.trim(),
            avatar: t.avatar
              ? (t.avatar.startsWith("http") ? t.avatar : combinePath(BaseURL, t.avatar))
              : DEFAULT_TEACHER_AVATAR,
            coursesCount: t.count || 0,
            title: "مدرس ارشد",
            specialization: "استاد اتاق",
            bio: `تعداد ${t.count || 0} جلسه برگزار شده در این اتاق`
          })) : [],
          isLive: Array.isArray(lives) ? lives.length > 0 : !!lives
        });

        if (historyData && historyData.items) {
          setPastMeetings(historyData.items.map(m => {
            const vUrl = m.videoUrl || m.recordingUrl;
            const fullVideoUrl = vUrl ? (vUrl.startsWith("http") ? vUrl : combinePath(BaseURLFiles, vUrl)) : null;
            const fullVoiceUrl = m.voiceUrl ? (m.voiceUrl.startsWith("http") ? m.voiceUrl : combinePath(BaseURLFiles, m.voiceUrl)) : null;
            const fullReportUrl = m.reportUrl ? (m.reportUrl.startsWith("http") ? m.reportUrl : `https://${m.reportUrl}`) : null;

            return {
              id: m.id || m.meetID,
              title: m.title || "نشست آموزشی",
              teacherName: m.teacher?.name || m.teachers?.[0]?.name || "-",
              date: m.jalaliDate || (m.dateInsert ? new Date(m.dateInsert).toLocaleDateString("fa-IR") : "-"),
              duration: m.endDate && m.dateInsert ? `${Math.abs(Math.round((new Date(m.endDate) - new Date(m.dateInsert)) / 60000))} دقیقه` : "-",
              participantsCount: m.participantsCount || 0,
              hasRecording: m.isRecorded || !!vUrl,
              videoUrl: fullVideoUrl,
              videoType: m.videoType ?? 102,
              voiceUrl: fullVoiceUrl,
              voiceType: m.voiceType ?? 102,
              reportUrl: fullReportUrl,
              reportType: m.reportType ?? 102
            };
          }));
          setHistoryTotalPages(historyData.totalPages || 1);
        }
      } catch (err) {
        console.error("Error loading room detail:", err);
      } finally {
        setLoading(false);
      }
    }

    loadAllRoomData();
  }, [roomId]);

  // ----------------------------------------
  // بارگذاری مجدد تاریخچه در هنگام تغییر صفحه (فقط بخش جدول)
  // ----------------------------------------
  useEffect(() => {
    if (loading) return;
    async function loadHistoryPage() {
      try {
        setHistoryLoading(true);
        const historyData = await GetRoomMeetingHistory(roomId, historyPage, 10);
        if (historyData && historyData.items) {
          setPastMeetings(historyData.items.map(m => {
            const vUrl = m.videoUrl || m.recordingUrl;
            const fullVideoUrl = vUrl ? (vUrl.startsWith("http") ? vUrl : combinePath(BaseURLFiles, vUrl)) : null;
            const fullVoiceUrl = m.voiceUrl ? (m.voiceUrl.startsWith("http") ? m.voiceUrl : combinePath(BaseURLFiles, m.voiceUrl)) : null;
            const fullReportUrl = m.reportUrl ? (m.reportUrl.startsWith("http") ? m.reportUrl : `https://${m.reportUrl}`) : null;

            return {
              id: m.id || m.meetID,
              title: m.title || "نشست آموزشی",
              teacherName: m.teacher?.name || m.teachers?.[0]?.name || "-",
              date: m.jalaliDate || (m.dateInsert ? new Date(m.dateInsert).toLocaleDateString("fa-IR") : "-"),
              duration: m.endDate && m.dateInsert ? `${Math.abs(Math.round((new Date(m.endDate) - new Date(m.dateInsert)) / 60000))} دقیقه` : "-",
              participantsCount: m.participantsCount || 0,
              hasRecording: m.isRecorded || !!vUrl,
              videoUrl: fullVideoUrl,
              videoType: m.videoType ?? 102,
              voiceUrl: fullVoiceUrl,
              voiceType: m.voiceType ?? 102,
              reportUrl: fullReportUrl,
              reportType: m.reportType ?? 102
            };
          }));
          setHistoryTotalPages(historyData.totalPages || 1);
        }
      } catch (err) {
        console.error("Error loading history page:", err);
      } finally {
        setHistoryLoading(false);
      }
    }

    loadHistoryPage();
  }, [historyPage]);

  // ----------------------------------------
  // اکشن‌ها (قابل جایگزینی با API)
  // ----------------------------------------
  const handleCopyLink = async () => {
    const link = `${BaseURLSite || window.location.origin}/rooms/${roomData?.roomID || roomId}/join`;
    try {
      await navigator.clipboard.writeText(link);
      showToast("لینک ورود به جلسه کپی شد!");
    } catch {
      showToast("خطا در کپی لینک");
    }
  };

  const handleVoteSubmit = () => {
    if (!selectedOption) {
      showToast("لطفا یک گزینه را انتخاب کنید");
      return;
    }

    // 💡 راهنمای اتصال API:
    // می‌توانید فراخوانی API ثبت رای مانند SubmitPollVote(pollId, selectedOption) را اینجا قرار دهید.
    setRoomData((prev) => {
      if (!prev) return prev;
      const updatedPolls = prev.polls.map((p) => {
        if (p.id === activePoll.id) {
          const updatedOptions = p.options.map((opt) => {
            if (opt.id === selectedOption) {
              return { ...opt, count: opt.count + 1 };
            }
            return opt;
          });
          const newTotal = p.totalVotes + 1;
          const recalculatedOptions = updatedOptions.map((opt) => ({
            ...opt,
            percentage: Math.round((opt.count / newTotal) * 100),
          }));
          return {
            ...p,
            totalVotes: newTotal,
            userVoted: true,
            userSelectedOption: selectedOption,
            options: recalculatedOptions,
          };
        }
        return p;
      });

      // به‌روزرسانی activePoll
      const currentUpdated = updatedPolls.find((p) => p.id === activePoll.id);
      setActivePoll(currentUpdated);

      return { ...prev, polls: updatedPolls };
    });

    showToast("رای شما با موفقیت ثبت شد!");
    setIsPollModalOpen(false);
    setIsPollResultModalOpen(true);
  };

  // ----------------------------------------
  // نمایش حالت اسکلتون در زمان بارگذاری
  // ----------------------------------------
  if (loading || !roomData) {
    return <RoomDetailSkeleton />;
  }

  const { title, description, coverImage, isLive, owner, stats, upcomingPlans, latestMeeting, teachers, polls } = roomData;

  return (
    <div className="min-h-screen bg-[#f8fafc] text-gray-800 dir-rtl pb-16">

      {/* ========== TOAST NOTIFICATION ========== */}
      {toastMessage && (
        <div className="fixed bottom-5 left-5 z-50 bg-[#0b3f32] text-white px-5 py-3 rounded-2xl shadow-xl border border-[#30d4cc]/30 flex items-center gap-3 animate-bounce">
          <FontAwesomeIcon icon={faCheckCircle} className="text-[#30d4cc] text-lg" />
          <span className="text-sm font-semibold">{toastMessage}</span>
        </div>
      )}

      {/* ========== HERO & HEADER BANNER ========== */}
      <section className="relative bg-white border-b border-gray-100 shadow-sm">
        <div className="h-56 sm:h-72 md:h-80 w-full relative overflow-hidden bg-gradient-to-r from-[#0b3f32] to-[#1e5849]">
          <img
            src={coverImage}
            alt={title}
            className="w-full h-full object-cover opacity-35 mix-blend-overlay scale-105 transition-transform duration-1000 hover:scale-100"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-gray-900/90 via-gray-900/40 to-transparent"></div>

          {/* نشانگر زنده در صورت آنلاین بودن */}
          {isLive && (
            <div className="absolute top-6 right-6 flex items-center gap-2 bg-red-600/90 backdrop-blur-md text-white px-4 py-1.5 rounded-full shadow-lg text-xs sm:text-sm font-bold animate-pulse z-10">
              <span className="w-2.5 h-2.5 bg-white rounded-full"></span>
              <span>هم‌اکنون در حال پخش زنده</span>
            </div>
          )}
        </div>

        {/* محتوای پروفایل و عنوان اتاق */}
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative -mt-20 pb-8 z-10">
          <div className="bg-white/95 backdrop-blur-md rounded-3xl p-6 sm:p-8 shadow-xl border border-gray-100">
            <div className="flex flex-col md:flex-row items-center md:items-start justify-between gap-6">

              {/* آواتار و مشخصات */}
              <div className="flex flex-col sm:flex-row items-center sm:items-start gap-5 text-center sm:text-right w-full">
                <div className="w-24 h-24 sm:w-28 sm:h-28 rounded-2xl bg-gradient-to-tr from-[#30d4cc] to-[#0b3f32] p-1 shadow-lg shrink-0">
                  <div className="w-full h-full bg-white rounded-[14px] flex items-center justify-center overflow-hidden">
                    {roomData.logo ? (
                      <img src={roomData.logo} alt={title} className="w-full h-full object-cover rounded-[14px]" />
                    ) : roomData.avatar ? (
                      <EmojiIcon name={roomData.avatar} className="text-4xl" />
                    ) : (
                      <FontAwesomeIcon icon={faUsers} className="text-4xl text-[#0b3f32]" />
                    )}
                  </div>
                </div>

                <div className="flex-1 space-y-2">
                  <div className="flex flex-wrap items-center justify-center sm:justify-start gap-3">
                    <h1 className="text-2xl sm:text-3xl font-black text-gray-900 leading-tight">
                      {title}
                    </h1>
                  </div>

                  <p className="text-gray-600 text-sm sm:text-base leading-relaxed max-w-3xl">
                    {description}
                  </p>

                  {/* مالک / موسسه */}
                  {owner && (
                    <div className="flex items-center justify-center sm:justify-start gap-2 pt-1 text-xs sm:text-sm text-gray-500 font-medium">
                      <FontAwesomeIcon icon={faBuilding} className="text-[#30d4cc]" />
                      <span>میزبان:</span>
                      <a href={`/a/@${owner.username}`} className="text-[#0b3f32] font-bold hover:underline">
                        {owner.fullname}
                      </a>
                    </div>
                  )}
                </div>
              </div>

              {/* دکمه‌های اکشن اصلی */}
              <div className="flex flex-wrap sm:flex-nowrap md:flex-col lg:flex-row gap-3 w-full md:w-auto shrink-0 justify-center">
                {isLive ? (
                  <button
                    onClick={() => window.open(`/rooms/${roomData.roomID}/join`, "_blank")}
                    className="flex-1 sm:flex-none px-6 py-3.5 rounded-2xl bg-gradient-to-r from-red-600 to-red-500 text-white font-bold text-sm shadow-lg shadow-red-500/30 hover:shadow-red-500/50 hover:scale-[1.02] transition-all flex items-center justify-center gap-2"
                  >
                    <FontAwesomeIcon icon={faPlay} className="animate-ping text-xs" />
                    <span>ورود مستقیم به جلسه زنده</span>
                  </button>
                ) : (
                  <button
                    onClick={() => window.open(`/rooms/${roomData.roomID}/join`, "_blank")}
                    className="flex-1 sm:flex-none px-6 py-3.5 rounded-2xl bg-[#30d4cc] text-[#0b3f32] font-extrabold text-sm shadow-lg shadow-[#30d4cc]/30 hover:bg-[#28c2ba] hover:scale-[1.02] transition-all flex items-center justify-center gap-2"
                  >
                    <FontAwesomeIcon icon={faUsers} />
                    <span>ورود به اتاق آنلاین</span>
                  </button>
                )}

                <button
                  // onClick={handleCopyLink}
                  onClick={()=>{
                    alert(JSON.stringify(roomData))
                    // handleCopyLink()
                  }}
                  className="px-4 py-3.5 rounded-2xl bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold text-sm transition-all flex items-center justify-center gap-2"
                  title="کپی لینک ورود"
                >
                  <FontAwesomeIcon icon={faCopy} className="text-gray-500" />
                  <span className="hidden sm:inline">کپی لینک</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ========== MAIN CONTENT CONTAINER ========== */}
      {roomData.isSubscribed && !roomData.isAdmin && roomData.subscriptionStatus !== 100 ? (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-8">
          <div className="bg-white rounded-3xl p-10 text-center shadow-sm border-2 border-orange-200">
            <div className="text-6xl mb-4">🔒</div>
            <h2 className="text-2xl font-black text-gray-800 mb-2">محتوای این کلاس خصوصی است</h2>
            <p className="text-gray-600 mb-6 font-medium">برای مشاهده محتوا و دسترسی به اطلاعات اتاق، باید در این کلاس ثبت‌نام کرده و توسط مدیر تایید شده باشید.</p>
            <button
              onClick={() => window.location.href = `/rooms/${roomData.roomID}/join`}
              className="px-8 py-3 bg-orange-50 hover:bg-orange-100 text-orange-700 font-bold rounded-xl transition-colors border border-orange-200"
            >
              ثبت درخواست / بررسی وضعیت عضویت
            </button>
          </div>
        </div>
      ) : (
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-8 space-y-10">
        {/* ========== STATS CARDS SECTION ========== */}
        <section className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-xl sm:text-2xl font-extrabold text-gray-900 flex items-center gap-2">
              <FontAwesomeIcon icon={faChartBar} className="text-[#30d4cc]" />
              <span>آمار گروه در سال جاری</span>
            </h2>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 sm:gap-6">
            {/* کارت ۱: تعداد نشست‌ها */}
            <div className="bg-white p-5 sm:p-6 rounded-3xl border border-gray-100 shadow-sm hover:shadow-md transition-all flex items-center gap-4">
              <div className="w-12 h-12 rounded-2xl bg-[#30d4cc]/15 text-[#0b3f32] flex items-center justify-center text-xl shrink-0">
                <FontAwesomeIcon icon={faCalendarAlt} />
              </div>
              <div>
                <p className="text-xs sm:text-sm text-gray-500 font-medium">تعداد کل جلسات</p>
                <h3 className="text-xl sm:text-2xl font-black text-gray-900 mt-1">
                  {stats?.totalMeetings || 0} <span className="text-xs font-normal text-gray-400">جلسه</span>
                </h3>
              </div>
            </div>

            {/* کارت ۲: شرکت‌کنندگان */}
            <div className="bg-white p-5 sm:p-6 rounded-3xl border border-gray-100 shadow-sm hover:shadow-md transition-all flex items-center gap-4">
              <div className="w-12 h-12 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center text-xl shrink-0">
                <FontAwesomeIcon icon={faUsers} />
              </div>
              <div>
                <p className="text-xs sm:text-sm text-gray-500 font-medium">مجموع شرکت‌کنندگان</p>
                <h3 className="text-xl sm:text-2xl font-black text-gray-900 mt-1">
                  {stats?.totalParticipants || 0} <span className="text-xs font-normal text-gray-400">نفر</span>
                </h3>
              </div>
            </div>

            {/* کارت ۳: ساعات آموزش */}
            <div className="bg-white p-5 sm:p-6 rounded-3xl border border-gray-100 shadow-sm hover:shadow-md transition-all flex items-center gap-4">
              <div className="w-12 h-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center text-xl shrink-0">
                <FontAwesomeIcon icon={faClock} />
              </div>
              <div>
                <p className="text-xs sm:text-sm text-gray-500 font-medium">مدت زمان آموزش</p>
                <h3 className="text-xl sm:text-2xl font-black text-gray-900 mt-1">
                  {stats?.totalHours || 0} <span className="text-xs font-normal text-gray-400">ساعت</span>
                </h3>
              </div>
            </div>

            {/* کارت ۴: میزان رضایت */}
            <div className="bg-white p-5 sm:p-6 rounded-3xl border border-gray-100 shadow-sm hover:shadow-md transition-all flex items-center gap-4">
              <div className="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center text-xl shrink-0">
                <FontAwesomeIcon icon={faStar} />
              </div>
              <div>
                <p className="text-xs sm:text-sm text-gray-500 font-medium">میزان رضایت شرکت‌کنندگان</p>
                <h3 className="text-xl sm:text-2xl font-black text-gray-900 mt-1">
                  {stats?.satisfactionRate || "100%"}
                </h3>
              </div>
            </div>
          </div>
        </section>

        {/* ========== UPCOMING MEETINGS / NEXT SESSION SECTION ========== */}
        {upcomingPlans && upcomingPlans.length > 0 && (
          <section className="space-y-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
              <div className="space-y-1">
                <h2 className="text-xl sm:text-2xl font-black text-gray-900 flex items-center gap-2.5">
                  <span className="relative flex h-3.5 w-3.5">
                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#30d4cc] opacity-75"></span>
                    <span className="relative inline-flex rounded-full h-3.5 w-3.5 bg-[#0b3f32]"></span>
                  </span>
                  <FontAwesomeIcon icon={faCalendarAlt} className="text-[#30d4cc]" />
                  <span>جلسات و برنامه‌های پیش‌رو</span>
                </h2>
                <p className="text-xs sm:text-sm text-gray-500">
                  برنامه‌زمانی جلسات آینده این اتاق آنلاین (به ترتیب نزدیک‌ترین زمان برگزاری)
                </p>
              </div>
              <span className="inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-full bg-[#0b3f32]/10 text-[#0b3f32] border border-[#30d4cc]/30 text-xs font-extrabold">
                <FontAwesomeIcon icon={faClock} className="text-[#30d4cc]" />
                <span>{upcomingPlans.length} جلسه زمان‌بندی‌شده</span>
              </span>
            </div>

            <div className="space-y-4">
              {(showAllUpcoming ? upcomingPlans : upcomingPlans.slice(0, 2)).map((plan, index) => {
                const isSoonest = index === 0;

                if (isSoonest) {
                  return (
                    /* ===== SOONEST MEETING: BOLD, SHAPED & SHINY DESIGN ===== */
                    <div
                      key={plan.id}
                      className="relative overflow-hidden rounded-[2.5rem] bg-gradient-to-br from-[#062c24] via-[#0b3f32] to-[#125444] p-6 sm:p-8 text-white shadow-2xl shadow-[#0b3f32]/30 border-2 border-[#30d4cc]/60 transition-all duration-500 hover:shadow-[#30d4cc]/20 hover:scale-[1.005]"
                    >
                      {/* Decorative Shimmer & Glow Effects */}
                      <div className="absolute -top-24 -left-24 w-60 h-60 bg-[#30d4cc]/20 rounded-full blur-3xl pointer-events-none animate-pulse"></div>
                      <div className="absolute -bottom-24 -right-24 w-60 h-60 bg-emerald-400/10 rounded-full blur-3xl pointer-events-none"></div>

                      {/* Top Shiny Badge */}
                      <div className="flex flex-wrap items-center justify-between gap-3 mb-6 relative z-10">
                        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-gradient-to-r from-amber-400 via-amber-300 to-yellow-500 text-gray-950 font-black text-xs sm:text-sm shadow-md animate-bounce">
                          <FontAwesomeIcon icon={faStar} className="text-amber-950" />
                          <span>🔥 نزدیک‌ترین جلسه پیش‌رو</span>
                        </div>

                        <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-2xl bg-white/10 backdrop-blur-md border border-white/20 text-[#30d4cc] font-extrabold text-xs sm:text-sm">
                          <FontAwesomeIcon icon={faClock} />
                          <span>{plan.formattedDate}</span>
                        </div>
                      </div>

                      {/* Main Content Grid */}
                      <div className="relative z-10 grid grid-cols-1 lg:grid-cols-3 gap-6 items-center">
                        <div className="lg:col-span-2 space-y-3">
                          <h3 className="text-2xl sm:text-3xl font-black text-white leading-tight tracking-wide">
                            {plan.title}
                          </h3>

                          {plan.teacher && (
                            <div className="flex items-center gap-2.5 text-sm sm:text-base text-[#30d4cc] font-bold">
                              <FontAwesomeIcon icon={faChalkboardTeacher} />
                              <span>مدرس: {plan.teacher}</span>
                              {plan.dabir && (
                                <span className="text-xs text-emerald-200 font-medium bg-white/10 px-2.5 py-0.5 rounded-lg mr-2">
                                  دبیر: {plan.dabir}
                                </span>
                              )}
                            </div>
                          )}

                          {plan.explain && (
                            <p className="text-xs sm:text-sm text-gray-200/90 leading-relaxed bg-white/10 backdrop-blur-sm p-4 rounded-2xl border border-white/10 mt-2">
                              {plan.explain}
                            </p>
                          )}
                        </div>

                        <div className="flex flex-col sm:flex-row lg:flex-col gap-3 justify-end items-stretch lg:items-end relative z-10">
                          <button
                            onClick={() => window.open(`/rooms/${roomData.roomID}/join`, "_blank")}
                            className="px-6 py-4 rounded-2xl bg-gradient-to-r from-[#30d4cc] to-[#28c2ba] text-[#0b3f32] font-black text-sm shadow-xl shadow-[#30d4cc]/30 hover:shadow-[#30d4cc]/50 hover:scale-105 active:scale-95 transition-all flex items-center justify-center gap-2"
                          >
                            <FontAwesomeIcon icon={faPlay} className="text-xs" />
                            <span>ورود / آماده‌سازی برای جلسه</span>
                          </button>

                          <button
                            onClick={handleCopyLink}
                            className="px-4 py-3.5 rounded-2xl bg-white/15 hover:bg-white/25 backdrop-blur-md text-white font-bold text-xs sm:text-sm border border-white/20 transition-all flex items-center justify-center gap-2"
                          >
                            <FontAwesomeIcon icon={faCopy} />
                            <span>اشتراک‌گذاری لینک</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                }

                /* ===== SUBSEQUENT UPCOMING MEETINGS: CLEAN CARD DESIGN ===== */
                return (
                  <div
                    key={plan.id}
                    className="bg-white rounded-3xl p-5 sm:p-6 border border-gray-200 shadow-sm hover:border-[#30d4cc]/60 hover:shadow-md transition-all flex flex-col md:flex-row md:items-center justify-between gap-4"
                  >
                    <div className="space-y-2">
                      <div className="flex flex-wrap items-center gap-3">
                        <span className="px-3 py-1 rounded-xl bg-gray-100 text-gray-700 text-xs font-bold flex items-center gap-1.5">
                          <FontAwesomeIcon icon={faClock} className="text-[#30d4cc]" />
                          <span>{plan.formattedDate}</span>
                        </span>
                        <span className="text-xs font-semibold text-[#0b3f32] bg-[#30d4cc]/15 px-2.5 py-0.5 rounded-lg">
                          برنامه‌ریزی شده
                        </span>
                      </div>

                      <h4 className="text-lg font-extrabold text-gray-900">
                        {plan.title}
                      </h4>

                      {plan.teacher && (
                        <p className="text-xs sm:text-sm text-gray-600 font-medium">
                          مدرس: <span className="font-bold text-gray-800">{plan.teacher}</span>
                          {plan.dabir && <span className="text-gray-400 mr-2">(دبیر: {plan.dabir})</span>}
                        </p>
                      )}
                    </div>

                    <div className="flex items-center gap-3 shrink-0">
                      <button
                        onClick={() => window.open(`/rooms/${roomData.roomID}/join`, "_blank")}
                        className="px-5 py-2.5 rounded-xl bg-[#0b3f32] text-white hover:bg-[#125444] font-bold text-xs transition-all flex items-center gap-2"
                      >
                        <FontAwesomeIcon icon={faPlay} className="text-xs text-[#30d4cc]" />
                        <span>مشاهده جزئیات</span>
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Toggle Button if upcomingPlans.length > 2 */}
            {upcomingPlans.length > 2 && (
              <div className="text-center pt-2">
                <button
                  onClick={() => setShowAllUpcoming(!showAllUpcoming)}
                  className="px-6 py-3 rounded-2xl bg-white hover:bg-gray-50 border border-gray-200 text-[#0b3f32] text-xs sm:text-sm font-extrabold shadow-sm hover:border-[#30d4cc] transition-all inline-flex items-center gap-2"
                >
                  <span>
                    {showAllUpcoming
                      ? "بستن و نمایش کمتر"
                      : `مشاهده همه جلسات پیش‌رو (${upcomingPlans.length} جلسه)`}
                  </span>
                  <FontAwesomeIcon
                    icon={showAllUpcoming ? faChevronUp : faChevronDown}
                    className="text-[#30d4cc]"
                  />
                </button>
              </div>
            )}
          </section>
        )}

        {/* ========== LATEST MEETING SECTION ========== */}
        {latestMeeting && (
          <section className="bg-gradient-to-br from-white to-[#f0fdfa] p-6 sm:p-8 rounded-3xl border border-[#30d4cc]/30 shadow-sm relative overflow-hidden">
            <div className="absolute -left-10 -bottom-10 w-40 h-40 bg-[#30d4cc]/10 rounded-full blur-2xl pointer-events-none"></div>

            <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-6 mb-6 pb-6 border-b border-gray-100">
              <div className="space-y-1">
                <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-[#0b3f32] text-[#30d4cc] text-xs font-bold mb-2">
                  <FontAwesomeIcon icon={faVideo} />
                  <span>آخرین جلسه برگزار شده</span>
                </div>
                <h2 className="text-xl sm:text-2xl font-extrabold text-gray-900">
                  {latestMeeting.title}
                </h2>
                <p className="text-xs sm:text-sm text-gray-500">
                  مدرس: <span className="font-semibold text-gray-700">{latestMeeting.teacherName}</span> • زمان: {latestMeeting.date}
                </p>
              </div>

              {latestMeeting.recordingUrl && (
                <a
                  href={latestMeeting.recordingUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="px-5 py-3 rounded-2xl bg-white hover:bg-gray-50 border border-gray-200 text-gray-800 text-xs sm:text-sm font-bold shadow-sm transition-all flex items-center gap-2 shrink-0"
                >
                  <FontAwesomeIcon icon={faDownload} className="text-[#30d4cc]" />
                  <span>مشاهده و دانلود ضبط جلسه</span>
                </a>
              )}
            </div>

            <p className="text-sm sm:text-base text-gray-600 leading-relaxed bg-white/70 backdrop-blur-sm p-4 rounded-2xl border border-gray-100">
              {latestMeeting.summary}
            </p>
          </section>
        )}

        {/* ========== TEACHERS SECTION ========== */}
        <section className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h2 className="text-xl sm:text-2xl font-extrabold text-gray-900 flex items-center gap-2">
                <FontAwesomeIcon icon={faChalkboardTeacher} className="text-[#30d4cc]" />
                <span>استادان و ارائه‌دهندگان این اتاق</span>
              </h2>
              <p className="text-xs sm:text-sm text-gray-500">
                استادان رسمی برگزارکننده کارگاه‌ها و جلسات آموزشی این دوره
              </p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {teachers && (showAllTeachers ? teachers : teachers.slice(0, 3)).map((teacher) => (
              <div
                key={teacher.id}
                className="bg-white rounded-3xl p-6 border border-gray-100 shadow-sm hover:shadow-lg transition-all duration-300 flex flex-col justify-between"
              >
                <div className="space-y-4 text-center">
                  <img
                    src={teacher.avatar }
                    alt={teacher.name}
                    className="w-24 h-24 rounded-full mx-auto object-cover border-4 border-[#30d4cc]/20 shadow-md"
                  />
                  <div>
                    <h3 className="text-lg font-extrabold text-gray-900">{teacher.name}</h3>
                    <p className="text-xs text-[#0b3f32] font-semibold mt-1">{teacher.title}</p>
                  </div>
                  <span className="inline-block bg-gray-100 text-gray-600 text-xs px-3 py-1 rounded-full font-medium">
                    {teacher.specialization}
                  </span>
                  <p className="text-xs sm:text-sm text-gray-500 leading-relaxed line-clamp-3">
                    {teacher.bio}
                  </p>
                </div>

                <div className="mt-6 pt-4 border-t border-gray-50 flex items-center justify-between text-xs text-gray-400">
                  <span>{teacher.coursesCount} جلسه برگزار شده</span>
                  {teacher.social?.eitaa && (
                    <a
                      href={teacher.social.eitaa}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-[#30d4cc] hover:underline font-semibold"
                    >
                      صفحه استاد ↵
                    </a>
                  )}
                </div>
              </div>
            ))}
          </div>

          {/* Toggle Button for collapsing/expanding teachers */}
          {teachers && teachers.length > 3 && (
            <div className="text-center pt-2">
              <button
                onClick={() => setShowAllTeachers(!showAllTeachers)}
                className="px-6 py-2.5 rounded-2xl bg-white hover:bg-gray-50 border border-gray-200 text-gray-700 text-xs sm:text-sm font-bold shadow-sm transition-all inline-flex items-center gap-2"
              >
                <span>{showAllTeachers ? "بستن و نمایش کمتر" : `مشاهده همه استادان (${teachers.length})`}</span>
                <FontAwesomeIcon icon={showAllTeachers ? faChevronUp : faChevronDown} className="text-[#30d4cc]" />
              </button>
            </div>
          )}
        </section>

        {/* ========== POLLS SECTION & BUTTONS ========== */}
        <section className="bg-white rounded-3xl p-6 sm:p-8 border border-gray-100 shadow-sm space-y-6">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div className="space-y-1">
              <h2 className="text-xl sm:text-2xl font-extrabold text-gray-900 flex items-center gap-2">
                <FontAwesomeIcon icon={faPoll} className="text-[#30d4cc]" />
                <span>نظرسنجی و ارزیابی کاربران</span>
              </h2>
              <p className="text-xs sm:text-sm text-gray-500">
                در نظرسنجی‌های فعال شرکت کنید یا نتایج ارزیابی قبلی را مشاهده فرمایید
              </p>
            </div>

            <div className="flex gap-3 w-full sm:w-auto">
              {activePoll && !activePoll.userVoted && (
                <button
                  onClick={() => setIsPollModalOpen(true)}
                  className="flex-1 sm:flex-none px-5 py-3 rounded-2xl bg-[#0b3f32] text-white text-xs sm:text-sm font-bold hover:bg-[#125847] transition-all flex items-center justify-center gap-2"
                >
                  <FontAwesomeIcon icon={faPoll} />
                  <span>شرکت در نظرسنجی فعال</span>
                </button>
              )}

              <button
                onClick={() => setIsPollResultModalOpen(true)}
                className="flex-1 sm:flex-none px-5 py-3 rounded-2xl bg-gray-100 hover:bg-gray-200 text-gray-700 text-xs sm:text-sm font-bold transition-all flex items-center justify-center gap-2"
              >
                <FontAwesomeIcon icon={faChartBar} />
                <span>مشاهده آمار و نتایج</span>
              </button>
            </div>
          </div>

          {/* پیش‌نمایش کارت نظرسنجی فعال */}
          {activePoll && (
            <div className="bg-gradient-to-r from-gray-50 to-white p-5 rounded-2xl border border-gray-200/80 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
              <div className="space-y-1">
                <span className="text-xs font-bold text-[#0b3f32] bg-[#30d4cc]/20 px-2.5 py-0.5 rounded-md">
                  {activePoll.userVoted ? "رای داده شده" : "نظرسنجی جدید"}
                </span>
                <p className="text-sm sm:text-base font-bold text-gray-800">{activePoll.question}</p>
                <p className="text-xs text-gray-400">تعداد آراء تا کنون: {activePoll.totalVotes} رای</p>
              </div>

              {!activePoll.userVoted ? (
                <button
                  onClick={() => setIsPollModalOpen(true)}
                  className="px-5 py-2.5 rounded-xl bg-[#30d4cc] text-[#0b3f32] text-xs font-extrabold hover:bg-[#25c4bd] transition-all shrink-0"
                >
                  ثبت رای ↵
                </button>
              ) : (
                <button
                  onClick={() => setIsPollResultModalOpen(true)}
                  className="px-5 py-2.5 rounded-xl bg-gray-200 text-gray-700 text-xs font-bold hover:bg-gray-300 transition-all shrink-0"
                >
                  نمایش نتایج ↵
                </button>
              )}
            </div>
          )}
        </section>

        {/* ========== PAST MEETINGS HISTORY TABLE ========== */}
        <section className="bg-white rounded-3xl p-6 sm:p-8 border border-gray-100 shadow-sm space-y-6">
          <div className="flex items-center justify-between">
            <h2 className="text-xl sm:text-2xl font-extrabold text-gray-900 flex items-center gap-2">
              <FontAwesomeIcon icon={faCalendarAlt} className="text-[#30d4cc]" />
              <span>تاریخچه و فهرست نشست‌های قبلی</span>
            </h2>
          </div>

          <div className="overflow-x-auto relative">
            {historyLoading && (
              <div className="absolute inset-0 bg-white/80 backdrop-blur-xs z-10 flex items-center justify-center rounded-2xl py-8">
                <div className="flex items-center gap-2 bg-[#0b3f32] text-white px-4 py-2 rounded-xl text-xs font-bold shadow-md">
                  <FontAwesomeIcon icon={faSpinner} className="animate-spin text-[#30d4cc]" />
                  <span>در حال بارگذاری نشست‌ها...</span>
                </div>
              </div>
            )}
            <table className="w-full text-right text-xs sm:text-sm">
              <thead>
                <tr className="border-b border-gray-100 text-gray-400 font-semibold bg-gray-50/50">
                  <th className="p-4 rounded-r-2xl">عنوان جلسه</th>
                  <th className="p-4">استاد / ارائه‌دهنده</th>
                  <th className="p-4">تاریخ برگزاری</th>
                  <th className="p-4">مدت</th>
                  <th className="p-4">شرکت‌کنندگان</th>
                  <th className="p-4 rounded-l-2xl text-center">عملیات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {pastMeetings && pastMeetings.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50/80 transition-colors">
                    <td className="p-4 font-bold text-gray-800">{item.title}</td>
                    <td className="p-4 text-gray-600">{item.teacherName}</td>
                    <td className="p-4 text-gray-500">{item.date}</td>
                    <td className="p-4 text-gray-500">{item.duration}</td>
                    <td className="p-4 text-gray-500">{item.participantsCount} نفر</td>
                    <td className="p-4 text-center">
                      <div className="flex items-center justify-center gap-2 flex-wrap">
                        {/* 1. فیلم جلسه (دانلود یا مشاهده آنلاین بر اساس نوع لاگ) */}
                        {item.videoUrl ? (
                          item.videoType === 103 ? (
                            <button
                              type="button"
                              onClick={() => setStreamingModal({
                                open: true,
                                type: "video",
                                url: item.videoUrl,
                                title: item.title
                              })}
                              title="مشاهده آنلاین فیلم (فقط پخش آنلاین و بدون قابلیت دانلود)"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-purple-50 text-purple-700 hover:bg-purple-100 font-bold text-xs transition-all shadow-sm border border-purple-200/60"
                            >
                              <FontAwesomeIcon icon={faPlay} className="text-[11px]" />
                              <span>مشاهده آنلاین فیلم</span>
                            </button>
                          ) : (
                            <a
                              href={item.videoUrl}
                              target="_blank"
                              rel="noopener noreferrer"
                              download
                              title="دانلود فایل فیلم جلسه"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-emerald-50 text-emerald-700 hover:bg-emerald-100 font-bold text-xs transition-all shadow-sm border border-emerald-200/60"
                            >
                              <FontAwesomeIcon icon={faDownload} className="text-[11px]" />
                              <span>دانلود فیلم</span>
                            </a>
                          )
                        ) : (
                          <button
                            disabled
                            title="فیلم جلسه موجود نیست"
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-gray-100 text-gray-400 font-medium text-xs cursor-not-allowed opacity-60"
                          >
                            <FontAwesomeIcon icon={faVideo} className="text-[11px]" />
                            <span>فیلم جلسه</span>
                          </button>
                        )}

                        {/* 2. صوت جلسه (دانلود یا پخش آنلاین بر اساس نوع لاگ) */}
                        {item.voiceUrl ? (
                          item.voiceType === 103 ? (
                            <button
                              type="button"
                              onClick={() => setStreamingModal({
                                open: true,
                                type: "audio",
                                url: item.voiceUrl,
                                title: item.title
                              })}
                              title="پخش آنلاین صوت (فقط پخش آنلاین و بدون قابلیت دانلود)"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-purple-50 text-purple-700 hover:bg-purple-100 font-bold text-xs transition-all shadow-sm border border-purple-200/60"
                            >
                              <FontAwesomeIcon icon={faPlay} className="text-[11px]" />
                              <span>پخش آنلاین صوت</span>
                            </button>
                          ) : (
                            <a
                              href={item.voiceUrl}
                              target="_blank"
                              rel="noopener noreferrer"
                              download
                              title="دانلود فایل صوتی جلسه"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-blue-50 text-blue-700 hover:bg-blue-100 font-bold text-xs transition-all shadow-sm border border-blue-200/60"
                            >
                              <FontAwesomeIcon icon={faDownload} className="text-[11px]" />
                              <span>دانلود صوت</span>
                            </a>
                          )
                        ) : (
                          <button
                            disabled
                            title="صوت جلسه موجود نیست"
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-gray-100 text-gray-400 font-medium text-xs cursor-not-allowed opacity-60"
                          >
                            <FontAwesomeIcon icon={faVolumeUp} className="text-[11px]" />
                            <span>صوت جلسه</span>
                          </button>
                        )}

                        {/* 3. گزارش و شرح جلسه (دانلود فایل یا هدایت به وب‌سایت) */}
                        {item.reportUrl ? (
                          item.reportType === 102 ? (
                            <a
                              href={item.reportUrl}
                              target="_blank"
                              rel="noopener noreferrer"
                              download
                              title="دانلود فایل گزارش جلسه"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-amber-50 text-amber-700 hover:bg-amber-100 font-bold text-xs transition-all shadow-sm border border-amber-200/60"
                            >
                              <FontAwesomeIcon icon={faDownload} className="text-[11px]" />
                              <span>دانلود فایل گزارش</span>
                            </a>
                          ) : (
                            <a
                              href={item.reportUrl}
                              target="_blank"
                              rel="noopener noreferrer"
                              title="مشاهده گزارش و شرح جلسه"
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-amber-50 text-amber-700 hover:bg-amber-100 font-bold text-xs transition-all shadow-sm border border-amber-200/60"
                            >
                              <FontAwesomeIcon icon={faFileAlt} className="text-[11px]" />
                              <span>مشاهده گزارش</span>
                              <FontAwesomeIcon icon={faExternalLinkAlt} className="text-[9px] opacity-70" />
                            </a>
                          )
                        ) : (
                          <button
                            disabled
                            title="گزارش جلسه ثبت نشده است"
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-gray-100 text-gray-400 font-medium text-xs cursor-not-allowed opacity-60"
                          >
                            <FontAwesomeIcon icon={faFileAlt} className="text-[11px]" />
                            <span>گزارش جلسه</span>
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination controls for meeting history */}
          {historyTotalPages > 1 && (
            <div className="flex items-center justify-center gap-3 pt-4 border-t border-gray-100">
              <button
                disabled={historyPage <= 1}
                onClick={() => setHistoryPage((prev) => Math.max(prev - 1, 1))}
                className="px-4 py-2 rounded-xl bg-gray-100 disabled:opacity-40 font-bold text-xs hover:bg-gray-200 transition-all"
              >
                صفحه قبلی
              </button>
              <span className="text-xs font-bold text-gray-600">
                صفحه {historyPage} از {historyTotalPages}
              </span>
              <button
                disabled={historyPage >= historyTotalPages}
                onClick={() => setHistoryPage((prev) => Math.min(prev + 1, historyTotalPages))}
                className="px-4 py-2 rounded-xl bg-gray-100 disabled:opacity-40 font-bold text-xs hover:bg-gray-200 transition-all"
              >
                صفحه بعدی
              </button>
            </div>
          )}
        </section>

      </div>
      )}

      {/* ========================================== */}
      {/* 3. MODAL: ثبت رای در نظرسنجی (POLL MODAL)  */}
      {/* ========================================== */}
      {isPollModalOpen && activePoll && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 animate-in fade-in zoom-in duration-200">
            <div className="flex items-center justify-between pb-4 border-b border-gray-100 mb-6">
              <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                <FontAwesomeIcon icon={faPoll} className="text-[#30d4cc]" />
                <span>شرکت در نظرسنجی</span>
              </h3>
              <button
                onClick={() => setIsPollModalOpen(false)}
                className="w-8 h-8 rounded-full bg-gray-100 hover:bg-gray-200 flex items-center justify-center text-gray-500 transition-colors"
              >
                <FontAwesomeIcon icon={faTimes} />
              </button>
            </div>

            <p className="text-sm sm:text-base font-bold text-gray-800 mb-6 leading-relaxed">
              {activePoll.question}
            </p>

            <div className="space-y-3 mb-8">
              {activePoll.options.map((option) => (
                <label
                  key={option.id}
                  onClick={() => setSelectedOption(option.id)}
                  className={`flex items-center justify-between p-4 rounded-2xl border-2 cursor-pointer transition-all ${
                    selectedOption === option.id
                      ? "border-[#30d4cc] bg-[#30d4cc]/10 shadow-sm"
                      : "border-gray-100 hover:border-gray-200 bg-white"
                  }`}
                >
                  <span className="text-sm font-semibold text-gray-700">{option.text}</span>
                  <input
                    type="radio"
                    name="poll-option"
                    checked={selectedOption === option.id}
                    onChange={() => setSelectedOption(option.id)}
                    className="w-4 h-4 text-[#30d4cc] focus:ring-[#30d4cc]"
                  />
                </label>
              ))}
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={handleVoteSubmit}
                className="flex-1 py-3.5 rounded-2xl bg-[#30d4cc] text-[#0b3f32] font-black text-sm hover:bg-[#26c5bd] shadow-lg shadow-[#30d4cc]/20 transition-all"
              >
                ثبت و تایید رای
              </button>
              <button
                onClick={() => setIsPollModalOpen(false)}
                className="px-5 py-3.5 rounded-2xl bg-gray-100 hover:bg-gray-200 text-gray-600 font-bold text-sm transition-all"
              >
                انصراف
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ========================================== */}
      {/* 4. MODAL: مشاهده نتایج نظرسنجی (POLL RESULTS) */}
      {/* ========================================== */}
      {isPollResultModalOpen && activePoll && (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 animate-in fade-in zoom-in duration-200">
            <div className="flex items-center justify-between pb-4 border-b border-gray-100 mb-6">
              <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                <FontAwesomeIcon icon={faChartBar} className="text-[#30d4cc]" />
                <span>نتایج نظرسنجی</span>
              </h3>
              <button
                onClick={() => setIsPollResultModalOpen(false)}
                className="w-8 h-8 rounded-full bg-gray-100 hover:bg-gray-200 flex items-center justify-center text-gray-500 transition-colors"
              >
                <FontAwesomeIcon icon={faTimes} />
              </button>
            </div>

            <p className="text-sm sm:text-base font-bold text-gray-800 mb-2">
              {activePoll.question}
            </p>
            <p className="text-xs text-gray-400 mb-6">مجموع آراء ثبت شده: {activePoll.totalVotes} نفر</p>

            <div className="space-y-4 mb-8">
              {activePoll.options.map((option) => (
                <div key={option.id} className="space-y-1.5">
                  <div className="flex justify-between text-xs sm:text-sm font-bold text-gray-700">
                    <span>{option.text}</span>
                    <span className="text-[#0b3f32]">{option.percentage}% ({option.count} رای)</span>
                  </div>
                  <div className="w-full bg-gray-100 h-3 rounded-full overflow-hidden">
                    <div
                      className="bg-gradient-to-r from-[#0b3f32] to-[#30d4cc] h-full rounded-full transition-all duration-700"
                      style={{ width: `${option.percentage}%` }}
                    ></div>
                  </div>
                </div>
              ))}
            </div>

            <button
              onClick={() => setIsPollResultModalOpen(false)}
              className="w-full py-3.5 rounded-2xl bg-gray-100 hover:bg-gray-200 text-gray-800 font-bold text-sm transition-all"
            >
              بستن پنجره
            </button>
          </div>
        </div>
      )}

      {/* ========================================== */}
      {/* 5. MODAL: پخش آنلاین مدیا (صوت / ویدیو بدون دانلود مستقیم) */}
      {/* ========================================== */}
      {streamingModal.open && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-3xl max-w-2xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 animate-in fade-in zoom-in duration-200" dir="rtl">
            <div className="flex items-center justify-between pb-4 border-b border-gray-100 mb-5">
              <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                <FontAwesomeIcon icon={streamingModal.type === 'video' ? faVideo : faVolumeUp} className="text-[#30d4cc]" />
                <span>
                  {streamingModal.type === 'video' ? 'پخش آنلاین فیلم جلسه' : 'پخش آنلاین صوت جلسه'}
                  {streamingModal.title ? ` - ${streamingModal.title}` : ''}
                </span>
              </h3>
              <button
                onClick={() => setStreamingModal({ open: false, type: 'video', url: '', title: '' })}
                className="w-8 h-8 rounded-full bg-gray-100 hover:bg-gray-200 flex items-center justify-center text-gray-500 transition-colors"
              >
                <FontAwesomeIcon icon={faTimes} />
              </button>
            </div>

            <div className="mb-6">
              {streamingModal.type === 'video' ? (
                <div className="rounded-2xl overflow-hidden bg-black shadow-inner">
                  <video
                    src={streamingModal.url}
                    controls
                    controlsList="nodownload"
                    onContextMenu={(e) => e.preventDefault()}
                    className="w-full max-h-[60vh] object-contain"
                    autoPlay
                  />
                </div>
              ) : (
                <div className="p-6 bg-gray-50 rounded-2xl border border-gray-200 flex flex-col items-center gap-4">
                  <div className="w-16 h-16 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-2xl shadow-sm">
                    <FontAwesomeIcon icon={faVolumeUp} />
                  </div>
                  <p className="text-xs font-bold text-gray-600">پخش آنلاین فایل صوتی (قابلیت دانلود مستقیم غیرفعال است)</p>
                  <audio
                    src={streamingModal.url}
                    controls
                    controlsList="nodownload"
                    onContextMenu={(e) => e.preventDefault()}
                    className="w-full"
                    autoPlay
                  />
                </div>
              )}
            </div>

            <div className="flex justify-end">
              <button
                onClick={() => setStreamingModal({ open: false, type: 'video', url: '', title: '' })}
                className="px-6 py-2.5 rounded-xl bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold text-xs transition-all"
              >
                بستن پنجره
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
