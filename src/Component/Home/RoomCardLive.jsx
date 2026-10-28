// RoomCardLive.jsx
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEye, faUsers, faBuilding, faCopy } from '@fortawesome/free-solid-svg-icons';
import EmojiIcon from "../EmojiIcon";

export default function RoomCardLive({ i, isRoomLive, live = null }) {
  const handleCopy = async (roomID) => {
    const link = `https://vc2.hekmateislami.com/rooms/${roomID}/join`;
    await navigator.clipboard.writeText(link);
    alert("کپی شد!");
  };

  return (
    <div
      dir='rtl'
      key={i.roomID}
      className={`
        relative rounded-2xl p-4 flex flex-col
        bg-white shadow-md hover:shadow-xl transition-all duration-300
        w-full h-full 
        ${isRoomLive ? 'ring-2 ring-[#ef4444] ring-offset-2' : 'border border-[#f3f4f6]'}
      `}
    >
      {/* راست بالا: نشانگر زنده */}
      {isRoomLive && (
        <div className="absolute top-2 right-2 flex items-center gap-1 bg-[#ef4444]/10 backdrop-blur-sm px-2 py-0.5 rounded-full z-10">
          <span className="relative flex h-1.5 w-1.5">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
            <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-[#ef4444]"></span>
          </span>
          <span className="text-[#dc2626] text-[10px] sm:text-xs font-bold">زنده</span>
        </div>
      )}

      {/* چپ بالا: تعداد حضار */}
      {isRoomLive && live?.countPartis > 0 && (
        <div className="absolute top-2 left-2 flex items-center gap-1 bg-[#f5f5f5] backdrop-blur-sm px-2 py-0.5 rounded-full">
          <span className="text-[#374151] text-[10px] sm:text-xs font-semibold tabular-nums">
            {live.countPartis} نفر
          </span>
          <FontAwesomeIcon icon={faEye} className="text-[#4b5563] text-[10px] sm:text-xs" />
        </div>
      )}

      {/* محتوای اصلی - عمودی */}
      <div className="flex-1 flex flex-col items-center justify-center text-center">
        {/* آیکون */}
        <div className="w-10 h-10 sm:w-12 sm:h-12 md:w-14 md:h-14 mb-2 text-[#30d4cc]">
          {i.avatar ? (
            <EmojiIcon name={i.avatar} className="text-lg sm:text-xl md:text-2xl" />
          ) : (
            <FontAwesomeIcon icon={faUsers} className="text-xl sm:text-2xl md:text-3xl" />
          )}
        </div>

        {/* عنوان */}
        <h2 className="text-xs sm:text-sm md:text-base font-bold mb-1 break-words text-[#1f2937] line-clamp-2 px-1">
          {i.title}
        </h2>

        {/* نام موسسه */}
        {i.owner && i.owner.fullname && (
          <p className="text-[#6b7280] text-[9px] sm:text-[10px] md:text-xs font-medium mb-1">
            <FontAwesomeIcon icon={faBuilding} className="ml-1 text-[9px] sm:text-[10px] md:text-xs" />
            {i.owner.fullname}
          </p>
        )}

        {/* موضوع جلسه */}
        <p className="text-[#6b7280] text-[9px] sm:text-[10px] md:text-xs mt-1 line-clamp-2 px-1">
          {isRoomLive ? (
            <span>موضوع: {i.latestTitle || '-'}</span>
          ) : (
            <span></span>
          )}
        </p>
      </div>

      {/* دکمه‌ها */}
      <div className="flex gap-2 mt-2">
        {isRoomLive && (
          <button
            onClick={() => window.open(`/rooms/${i.roomID}/join`, '_blank')}
            className="w-full py-1.5 sm:py-2 rounded-lg bg-[#ef4444] text-white text-[10px] sm:text-xs md:text-sm font-bold hover:bg-[#dc2626] transition shadow-sm animate-pulse"
          >
            ورود زنده
          </button>
        )}
      </div>
    </div>
  );
}