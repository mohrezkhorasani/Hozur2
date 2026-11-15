import { SVG_ROOMCARD_WAVE, timeAgo } from "../../Tools/tools";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEye, faUsers, faBuilding, faCopy, faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import EmojiIcon from "../EmojiIcon";
import { BaseURLSite } from "../../api/apiNeed";
import { useState } from "react";
import { Link } from "react-router-dom";

export default function RoomCard({ i, isRoomLive, live = null, joinButton = true, copyButton = true }) {
  const [isHovered, setIsHovered] = useState(false);

  // Determine if the room is currently live
  const handleCopy = async (roomID) => {
    const link = `${BaseURLSite}rooms/${roomID}/join`;

    await navigator.clipboard.writeText(link);
    alert("کپی شد!");
  };
  // Determine if there's an upcoming session and format its details
  let nextSessionInfo = null;
  if (i.nextMeet) {
    const nextMeetDate = new Date(i.nextMeet);
    const now = new Date();

    // Create UTC dates for comparison (to avoid timezone issues)
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
        i.nextMeet = null;
        nextSessionInfo = null;
      } else if (daysDifference > 30) {
        i.nextMeet = null;
        nextSessionInfo = null;
      } else {
        // Convert to Persian date
        const persianDate = new Intl.DateTimeFormat('fa-IR', {
          year: 'numeric',
          month: 'long',
          day: 'numeric'
        }).format(nextMeetDate);
        dateString = persianDate;
      }
    }

    if (i.nextMeet) {
      // Use UTC time (same as input)
      const timeString = nextMeetDate.toLocaleTimeString('fa-IR', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
        timeZone: 'UTC'
      });
      nextSessionInfo = `${dateString} ساعت ${timeString}`;
    }
  }

  // Extract teacher names if available
  const teacherNames = i.nextMeetTeacher && i.nextMeetTeacher.length > 0
    ? i.nextMeetTeacher.join(', ')
    : '';

  return (
    <div
      key={i.roomID}
      dir="rtl"
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className={`
        relative rounded-2xl p-3 sm:p-3.5 flex flex-col h-full
        bg-white shadow-md hover:shadow-xl transition-all duration-300
        ${isRoomLive ? 'ring-2 ring-[#ef4444] ring-offset-2' : 'border border-[#f3f4f6]'}
        overflow-hidden
      `}
    >

      <div className="absolute inset-0 top-0 w-full h-full pointer-events-none z-1">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1440 320">
          <path fill="#E6F7F6" fillOpacity="1" d={SVG_ROOMCARD_WAVE}></path>
        </svg>
      </div>

      {/* === راست بالا: نشانگر زنده === */}
      {isRoomLive && (
        <div className="absolute top-2 right-2 flex items-center gap-1.5 bg-[#ef4444]/10 backdrop-blur-sm px-2 py-0.5 rounded-full z-10">
          <span className="relative flex h-2 w-2">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
            <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
          </span>
          <span className="text-[#dc2626] text-xs font-bold">زنده</span>
        </div>
      )}

      {/* === چپ بالا: تعداد حضار با آیکون (فقط در حالت زنده) === */}
      {isRoomLive && live && live.countPartis > 0 && (
        <div className="absolute top-2 left-2 flex items-center gap-1.5 bg-[#f5f5f5] backdrop-blur-sm px-2 py-0.5 rounded-full z-10">
          <span className="text-[#374151] text-xs font-semibold tabular-nums">
            {live.countPartis} نفر
          </span>
          <FontAwesomeIcon icon={faEye} className="text-[#4b5563] text-xs" />
        </div>
      )}

      {/* محتوای اصلی کارت */}
      <div className="flex-1 flex flex-col justify-between z-4">
        {/* بالای کارت: آیکون، عنوان، موسسه، و عنوان جلسه زنده */}
        <div className="text-center">
          {/* ایموجی / آواتار با افکت hover - ارتفاع فشرده‌تر */}
          <div 
            className={`w-9 h-9 sm:w-10 sm:h-10 mx-auto mb-1 text-[#30d4cc] flex items-center justify-center transition-all duration-500 ease-out ${
              isHovered ? 'transform -translate-y-0.5 scale-110 rotate-6' : 'transform translate-y-0 scale-100 rotate-0'
            }`}
          >
            {i.avatar ? (
              <EmojiIcon 
                name={i.avatar} 
                className={`text-lg sm:text-xl transition-all duration-500 ease-out ${
                  isHovered ? 'scale-110' : 'scale-100'
                }`} 
              />
            ) : (
              <FontAwesomeIcon 
                icon={faUsers} 
                className={`text-xl sm:text-2xl transition-all duration-500 ease-out ${
                  isHovered ? 'scale-110' : 'scale-100'
                }`} 
              />
            )}
          </div>

          {/* عنوان با آیکون اطلاعات بیشتر در جلوی آن */}
          <div className="flex items-center justify-center gap-1.5 flex-wrap">
            <Link to={`/rooms/${i.roomID}`} className="hover:text-[#30d4cc] transition-colors" title={i.title}>
              <h2 className="text-xs sm:text-sm font-bold line-clamp-2 text-[#1f2937] hover:text-[#30d4cc] leading-snug">
                {i.title}
              </h2>
            </Link>
            
            {/* آیکون اطلاعات بیشتر - جلوی عنوان */}
            <Link 
              to={`/rooms/${i.roomID}`}
              title="مشاهده صفحه اتاق"
              className={`transition-all duration-500 ease-out ${
                isHovered ? 'opacity-100 scale-100 rotate-0' : 'opacity-0 scale-50 -rotate-45'
              }`}
            >
              <div className="bg-[#30d4cc]/10 rounded-full p-1 border border-[#30d4cc]/20 hover:bg-[#30d4cc]/20 transition-all duration-300 group cursor-pointer hover:scale-110">
                <FontAwesomeIcon 
                  icon={faInfoCircle} 
                  className="text-[#30d4cc] text-[10px] group-hover:rotate-12 transition-transform duration-300" 
                />
              </div>
            </Link>

            {isRoomLive && (
              <span className="inline-flex items-center gap-1 text-[#ef4444] text-[11px]">
                <span className="relative flex h-1.5 w-1.5">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-[#ef4444]"></span>
                </span>
                (در حال پخش)
              </span>
            )}
          </div>

          {/* نام موسسه */}
          {i.owner && i.owner.fullname && (
            <p className="text-[#4b5563] text-xs font-semibold mt-0.5 flex items-center justify-center gap-1">
              <a href={`/a/@${i.owner.username}`}
                className="relative text-[#6b7280] hover:text-[#59DDD6] text-[11px] transition-colors duration-300 group whitespace-nowrap truncate max-w-full"
              >
                <FontAwesomeIcon icon={faBuilding} className="text-[#6b7280] text-[10px] duration-300 group-hover:text-[#59DDD6] ml-1" />
                <span className="truncate max-w-full">
                  {i.owner.fullname}
                </span>
                <span className="absolute left-1/2 bottom-[-6px] h-[0.2px] w-0 bg-[#59DDD6] transition-all duration-300 group-hover:w-full group-hover:left-0" />
              </a>
            </p>
          )}

          {/* نمایش آخرین جلسه یا موضوع جلسه */}
          {isRoomLive && i.latestTitle && (
            <p className="text-[#6b7280] text-xs mt-0.5 truncate">
              موضوع جلسه: {i.latestTitle}
            </p>
          )}
        </div>

        {/* بخش برنامه آتی - فیکس شده در پایین، دقیقاً بالای ورود به جلسه با فونت بزرگتر و خواناتر */}
        {nextSessionInfo && i.nextMeet && (
          <div className="mt-auto pt-2 w-full">
            <div className="p-2 sm:p-2.5 bg-[#f0fdf4] rounded-xl border border-[#bbf7d0] text-right shadow-[0_1px_3px_rgba(0,0,0,0.03)]">
              <div className="flex items-center justify-between text-[#166534] font-bold mb-0.5">
                <span className="text-xs sm:text-[13px] font-black flex items-center gap-1.5">
                  <span className="inline-block w-2 h-2 rounded-full bg-emerald-500"></span>
                  برنامه آتی:
                </span>
              </div>
              <p className="text-[#15803d] font-bold text-xs sm:text-sm leading-relaxed">{nextSessionInfo}</p>
              {teacherNames && (
                <p className="text-[#374151] text-xs sm:text-[13px] font-semibold mt-0.5 truncate" title={teacherNames}>
                  استاد / ارائه: <span className="text-[#1f2937] font-bold">{teacherNames}</span>
                </p>
              )}
            </div>
          </div>
        )}
      </div>

      {/* دکمه‌ها */}
      <div className="mt-auto pt-2 w-full z-4">
        <div className="flex items-center justify-center gap-2 w-full">
          {copyButton && (
            <button
              onClick={() => handleCopy(i.roomID)}
              className="py-1.5 px-2 rounded-xl border border-[#d1d5db] text-[#374151] text-xs hover:bg-[#f9fafb] hover:border-[#30d4cc] transition flex-1 font-medium flex items-center justify-center gap-1.5 whitespace-nowrap"
            >
              <FontAwesomeIcon icon={faCopy} className="text-xs" />
              کپی لینک
            </button>
          )}

          {isRoomLive && (
            <button
              onClick={() => window.open(`/rooms/${i.roomID}/join`, '_blank')}
              className="py-1.5 px-3 rounded-xl bg-[#ef4444] text-white text-xs font-bold hover:bg-[#dc2626] transition flex-1 shadow-sm animate-pulse flex items-center justify-center gap-1 whitespace-nowrap"
            >
              ورود زنده
            </button>
          )}

          {!isRoomLive && i.nextMeet && joinButton && (
            <button
              onClick={() => window.open(`/rooms/${i.roomID}/join`, '_blank')}
              className="py-1.5 px-3 rounded-xl bg-[#30d4cc] text-[#0b3f32] text-xs font-bold hover:bg-[#30d4cc]/80 transition flex-1 shadow-sm flex items-center justify-center gap-1 whitespace-nowrap"
            >
              ورود به جلسه
            </button>
          )}
        </div>
      </div>
    </div>
  )
}



export const RoomCardLoading = () => {
  return (
    <div className="relative rounded-2xl p-3 sm:p-3.5 flex flex-col h-full bg-white shadow-md border border-[#f3f4f6] overflow-hidden">
      {/* راست بالا: placeholder نشانگر زنده */}
      <div className="absolute top-2 right-2">
        <div className="w-10 h-5 bg-gray-200 rounded-full shimmer"></div>
      </div>

      {/* چپ بالا: placeholder تعداد حضار */}
      <div className="absolute top-2 left-2">
        <div className="w-12 h-5 bg-gray-200 rounded-full shimmer"></div>
      </div>

      {/* بقیه محتوای کارت */}
      <div className="flex-1 flex flex-col justify-between">
        <div className="text-center">
          {/* آیکون */}
          <div className="w-9 h-9 sm:w-10 sm:h-10 mx-auto mb-1.5">
            <div className="w-full h-full rounded-full bg-gray-200 shimmer"></div>
          </div>

          {/* عنوان */}
          <div className="h-5 w-3/4 mx-auto mb-1">
            <div className="w-full h-full bg-gray-200 rounded-lg shimmer"></div>
          </div>

          {/* نام موسسه */}
          <div className="h-4 w-1/2 mx-auto mt-0.5">
            <div className="w-full h-full bg-gray-200 rounded-md shimmer"></div>
          </div>
        </div>

        {/* بخش جلسه بعدی */}
        <div className="mt-auto pt-2">
          <div className="p-2 bg-gray-100 rounded-xl">
            <div className="h-4 w-1/3 mx-auto mb-1.5">
              <div className="w-full h-full bg-gray-200 rounded shimmer"></div>
            </div>
            <div className="h-4 w-2/3 mx-auto mb-1">
              <div className="w-full h-full bg-gray-200 rounded shimmer"></div>
            </div>
            <div className="h-3 w-1/2 mx-auto">
              <div className="w-full h-full bg-gray-200 rounded shimmer"></div>
            </div>
          </div>
        </div>
      </div>

      {/* دکمه‌ها */}
      <div className="mt-auto pt-2 w-full">
        <div className="flex flex-row items-center justify-center gap-2 w-full">
          {/* دکمه کپی لینک */}
          <div className="flex-1 h-8">
            <div className="w-full h-full bg-gray-200 rounded-xl shimmer"></div>
          </div>

          {/* دکمه ورود */}
          <div className="flex-1 h-8">
            <div className="w-full h-full bg-gray-300 rounded-xl shimmer"></div>
          </div>
        </div>
      </div>

      <style>{`
        .shimmer {
          background: linear-gradient(
            90deg,
            #f0f0f0 0%,
            #e0e0e0 25%,
            #f0f0f0 50%,
            #e0e0e0 75%,
            #f0f0f0 100%
          );
          background-size: 200% 100%;
        }
      `}</style>
    </div>
  );
};

// کامپوننت برای نمایش چند کارت در حالت لودینگ
export const RoomCardLoadingGrid = ({ count = 6 }) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 lg:gap-8">
      {Array.from({ length: count }).map((_, index) => (
        <RoomCardLoading key={index} />
      ))}
    </div>
  );
};