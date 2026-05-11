import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { joinMeeting } from '@/api/StartNewMeeting';
import { fetchRoom } from '@/api/GetRooms';
import { fetchRoomLive } from '@/api/GetRooms';
import { fetchRoomLivesPublic, fetchRoomLivesPublicWithID } from '../api/GetRooms';

// Helper to read a cookie by name
function getCookie(name) {
  const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
  return match ? decodeURIComponent(match[2]) : null;
}

// Helper to set a cookie with expiration in days
function setCookie(name, value, days) {
  const expires = new Date();
  expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
  document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=/`;
}

function JoinForm() {
  const { Mid } = useParams();
  const navigate = useNavigate();

  // --- Existing state ---
  const [displayName, setDisplayName] = useState('');
  const [loading, setLoading] = useState(false);
  const [meet, setMeet] = useState('');
  const [roomLive, setRoomLive] = useState([]);
  const [isMeetingLive, setIsMeetingLive] = useState(false);
  const [isGuest, setIsGuest] = useState(false);
  const [isFromAllLive, setIsFromAllLive] = useState(false);
  const [nextSessionInfo, setNextSessionInfo] = useState("");

  // --- NEW: admin mode checkbox state ---
  const [isAdmin, setIsAdmin] = useState(false);

  // --- Assume a `user` object is available (e.g., from context/props) ---
  // Replace this with your actual user source (e.g., useSelector, useContext, etc.)
  const user = null; // <-- Example: set to actual user object or null

  // --- NEW: Set default name from user.fullname or cookie "joinName" ---
  useEffect(() => {
    const defaultName = (user && user.fullname) || getCookie('joinName') || '';
    if (defaultName) {
      setDisplayName(defaultName);
    }
  }, [user]); // re-run if user object changes

  // --- Existing useEffect for next session info ---
  useEffect(() => {
    console.log(meet)

    if (meet.nextMeet) {
      const nextMeetDate = new Date(meet.nextMeet);
      const now = new Date();

      const nextUTCDate = new Date(Date.UTC(
        nextMeetDate.getUTCFullYear(),
        nextMeetDate.getUTCMonth(),
        nextMeetDate.getUTCDate()
      ));

      const nowUTCDate = new Date(Date.UTC(
        now.getUTCFullYear(),
        now.getUTCMonth(),
        now.getUTCDate()
      ));

      let dateString = '';

      if (nextUTCDate.getTime() === nowUTCDate.getTime()) {
        dateString = 'امروز';
      } else {
        const daysDifference = Math.ceil((nextUTCDate - nowUTCDate) / (1000 * 60 * 60 * 24));

        if (daysDifference === 1) {
          dateString = 'فردا';
        } else if (daysDifference < 0) {
          meet.nextMeet = null;
          setNextSessionInfo(null)
        } else if (daysDifference > 30) {
          meet.nextMeet = null;
          setNextSessionInfo(null)
        } else {
          const persianDate = new Intl.DateTimeFormat('fa-IR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
          }).format(nextMeetDate);
          dateString = persianDate;
        }
      }

      if (meet.nextMeet) {
        const timeString = nextMeetDate.toLocaleTimeString('fa-IR', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: false,
          timeZone: 'UTC'
        });
        setNextSessionInfo(`${dateString} ساعت ${timeString}`)
      }
    }
  }, [meet])

  const fetchMeetingInfo = async () => {
    try {
      var data = await fetchRoom(Mid);
      CheckLives();
      setMeet(data);
      document.title = `مجمع عالی حکمت اسلامی | ${data.title}`;
    } catch (error) {
      console.error("Error fetching meeting info:", error);
      setMeet(null);
    }
  };

  useEffect(() => {
    const liveStatus = roomLive.some(room => room.roomID === Mid);
    setIsMeetingLive(liveStatus);
  }, [roomLive, Mid]);

  // --- MODIFIED joinMeetings: uses isAdmin flag, no password field, sets cookie ---
  const joinMeetings = async () => {
    if (!displayName && !isGuest) {
      alert("لطفا نام نمایشی خود را وارد کنید.");
      return;
    }

    setLoading(true);
    var livedata = await fetchRoomLivesPublic();
    setRoomLive(livedata);
    const liveStatus = roomLive.some(room => room.roomID === Mid);
    if (liveStatus != true) {
      setLoading(false)
      alert("جلسه هنوز شروع نشده است لطفا بعد از شروع شدن جلسه مجدد تلاش کنید")
      return
    }
    try {
      // Set 30-day cookie for joinName (only if not guest)
      if (!isGuest && displayName) {
        setCookie('joinName', displayName, 30);
      }

      // third parameter now uses isAdmin flag (checkbox) instead of false
      var r = await joinMeeting(Mid, isGuest ? "مهمان" : displayName, isAdmin);
      window.open(r.link, "_blank");
    } catch (error) {
      console.error("Error joining meeting:", error);
      alert("خطا در پیوستن به جلسه. لطفا دوباره تلاش کنید.");
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMeetingInfo();
  }, [Mid]);

  async function CheckLives() {
    console.log("Checking live rooms...");
    try {
      var livedata = await fetchRoomLivesPublicWithID(Mid);
      if (!Array.isArray(livedata) || livedata.length === 0) {
        livedata = await fetchRoomLivesPublic();
        setIsFromAllLive(true)
      }
      setRoomLive(livedata);
      console.log("Live rooms data updated.");
    } catch (error) {
      console.error("Error fetching live rooms:", error);
    }
  }

  useEffect(() => {
    const intervalId = setInterval(CheckLives, 20000);
    // Optional cleanup (uncomment if needed):
    // return () => clearInterval(intervalId);
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-br from-[#f0f7f4] to-[#e6f0ec] flex items-center justify-center px-4 py-8 sm:py-12">
      <div className="w-full max-w-md mx-auto bg-white/95 backdrop-blur-sm rounded-2xl shadow-2xl shadow-[#0b3f32]/10 p-6 sm:p-8 transition-all duration-300 hover:shadow-xl">

        {/* Header */}
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-[#0b3f32]/10 rounded-full mb-3">
            <svg className="w-8 h-8 text-[#0b3f32]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold text-[#1f2937]">ورود به جلسه</h1>
          <p className="text-[#6b7280] text-sm mt-1">اطلاعات جلسه را وارد کنید</p>
        </div>

        {/* Meeting Info */}
        <div className="bg-gradient-to-r from-[#0b3f32]/5 to-[#30d4cc]/5 rounded-xl p-4 mb-6 border border-[#30d4cc]/20">
          <p className="text-xs text-[#6b7280] uppercase tracking-wider mb-1">نام جلسه</p>
          <p className="text-xl font-bold text-[#1f2937] break-words">{meet.title}</p>
        </div>

        {/* Additional Meeting Info */}
        <div className="space-y-3 mb-6">
          {meet?.owner?.fullname && (
            <div className="flex items-center gap-2 text-sm text-[#6b7280] bg-[#f9fafb] p-2 rounded-lg border border-[#f3f4f6]">
              <svg className="w-4 h-4 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
              <span className="font-medium text-[#374151]">موسسه:</span>
              <span>{meet.owner.fullname}</span>
            </div>
          )}

          {nextSessionInfo && !isMeetingLive && (
            <>
              <div className="flex items-center gap-2 text-sm text-[#6b7280] bg-[#f9fafb] p-2 rounded-lg border border-[#f3f4f6]">
                <svg className="w-4 h-4 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span className="font-medium text-[#374151]">تاریخ جلسه بعدی:</span>
                <span>{nextSessionInfo}</span>
              </div>

              {meet.latestTitle && !isMeetingLive && (
                <div className="flex items-center gap-2 text-sm text-[#6b7280] bg-[#f9fafb] p-2 rounded-lg border border-[#f3f4f6]">
                  <svg className="w-4 h-4 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                  </svg>
                  <span className="font-medium text-[#374151]">موضوع جلسه بعدی:</span>
                  <span>{meet.latestTitle}</span>
                </div>
              )}
            </>
          )}


        </div>
        {/* Current Meeting Topic (prominent when live) */}
        {isMeetingLive && meet?.latestTitle && (
          <div className="bg-gradient-to-r from-[#f0f9ff] to-[#e6f4ff] border-r-4 border-[#0b3f32] rounded-xl p-4 mb-5 shadow-md">
            <p className="text-xs text-[#0b3f32] font-semibold tracking-wide mb-1">موضوع جلسه فعلی</p>
            <p className="text-lg font-bold text-[#1f2937] leading-relaxed">{meet.latestTitle}</p>
          </div>
        )}
        {/* Live Indicator */}
        {isMeetingLive && (
          <div className="bg-[#f0fdf4] border-r-4 border-[#22c55e] text-[#166534] p-3 rounded-lg mb-5 flex items-start gap-3 shadow-sm">
            <div className="flex-shrink-0 mt-0.5">
              <span className="relative flex h-3 w-3">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#4ade80] opacity-75"></span>
                <span className="relative inline-flex rounded-full h-3 w-3 bg-[#22c55e]"></span>
              </span>
            </div>
            <div>
              <p className="font-bold text-sm">جلسه در حال برگزاری است</p>
              {isFromAllLive ? (
                <p className="text-xs text-[#15803d]">همین الآن به گفتگو بپیوندید</p>
              ) : (
                <p className="text-xs text-[#15803d]">همین حالا به گفتگو بپیوندید</p>
              )}
            </div>
          </div>
        )}

        {/* Form Fields */}
        <div className="space-y-5">
          {/* Display Name Field */}
          <div>
            <div className="flex items-center justify-between flex-wrap gap-2 mb-1">
              <label htmlFor="displayName" className="text-sm font-medium text-[#374151]">
                نام نمایشی شما <span className="text-[#ef4444]">*</span>
              </label>
              <label className="flex items-center gap-1.5 cursor-pointer select-none text-sm text-[#4b5563]">
                <input
                  type="checkbox"
                  disabled
                  hidden
                  checked={isGuest}
                  onChange={(e) => setIsGuest(e.target.checked)}
                  className="w-3.5 h-3.5 text-[#30d4cc] border-[#d1d5db] rounded focus:ring-[#30d4cc]"
                />
                <span hidden>ورود به عنوان مهمان</span>
              </label>
            </div>
            <input
              type="text"
              id="displayName"
              value={isGuest ? "مهمان" : displayName}
              onChange={(e) => !isGuest && setDisplayName(e.target.value)}
              placeholder="مثال: محمد رضایی"
              disabled={isGuest}
              className={`w-full px-4 py-2.5 border border-[#e5e7eb] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all duration-200 ${isGuest ? 'bg-[#f3f4f6] text-[#6b7280] cursor-not-allowed' : 'bg-[#f9fafb]/50'
                }`}
              required
            />
            <p className="text-xs text-[#9ca3af] mt-1">
              {isGuest ? "به عنوان مهمان وارد می‌شوید (بدون نیاز به رمز)" : "نام شما در جلسه نمایش داده می‌شود"}
            </p>
          </div>

          {/* NEW: Admin Checkbox - shown only if meet.isAdmin is true */}
          {meet?.isAdmin === true && (
            <div className="flex items-center gap-3 mt-2">
              <input
                type="checkbox"
                id="adminMode"
                checked={isAdmin}
                onChange={(e) => setIsAdmin(e.target.checked)}
                className="w-5 h-5 text-[#0b3f32] border-[#d1d5db] rounded focus:ring-[#30d4cc]"
              />
              <label htmlFor="adminMode" className="text-sm font-medium text-[#374151] cursor-pointer">
                ورود به عنوان مدیر
              </label>
            </div>
          )}
        </div>

        {/* Join Button */}
        <button
          onClick={isMeetingLive ? joinMeetings : () => { }}
          disabled={loading || !isMeetingLive}
          className={`
        w-full mt-8 py-3 rounded-xl font-bold text-base flex items-center justify-center gap-2 transition-all duration-300
        ${loading
              ? 'bg-[#9ca3af] cursor-not-allowed text-[#4b5563]'
              : isMeetingLive
                ? 'bg-linear-to-r from-[#0b3f32] to-[#1a6b58] hover:from-[#0a3327] hover:to-[#135a4a] text-white shadow-lg hover:shadow-xl transform hover:-translate-y-0.5'
                : 'bg-[#f3f4f6] cursor-not-allowed text-[#9ca3af] border border-[#e5e7eb]'
            }
      `}
        >
          {loading ? (
            <>
              <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              در حال اتصال...
            </>
          ) : isMeetingLive ? (
            <>
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              پیوستن به جلسه
            </>
          ) : (
            "در انتظار شروع جلسه..."
          )}
        </button>

        <p className="text-center text-xs text-[#9ca3af] mt-6">
          {isGuest ? "ورود مهمان - بدون نیاز به نام و رمز" : "برای شرکت در جلسه، نام نمایشی خود را وارد کنید"}
        </p>
      </div>
    </div>
  );
}

export default JoinForm;