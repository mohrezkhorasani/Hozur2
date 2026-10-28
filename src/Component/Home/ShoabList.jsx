// BranchItem.jsx - کامپوننت تکی برای نمایش یک شعبه
import { useNavigate } from 'react-router-dom';
import { useState, useMemo } from 'react';

const BranchItem = ({ branch, lives }) => {
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(false);

  // پیدا کردن roomIDهای زنده
  const liveRoomIDs = useMemo(() => {
    return new Set(lives?.map(live => live.roomID) || []);
  }, [lives]);
  const getLiveRoom = useMemo(() => {
    const liveMap = new Map();
    lives?.forEach(live => {
      liveMap.set(live.roomID, live);
    });
    return (roomID) => liveMap.get(roomID);
  }, [lives]);
  // بررسی اینکه آیا این شعبه جلسه زنده دارد
  const hasLiveRoom = useMemo(() => {
    if (!branch.room) return false;
    return branch.room.some(room => liveRoomIDs.has(room.roomID));
  }, [branch.room, liveRoomIDs]);

  // مرتب‌سازی گروه‌ها: گروه‌های زنده در بالا
  const sortedGroups = useMemo(() => {
    if (!branch.room) return [];

    return [...branch.room].sort((a, b) => {
      const aIsLive = liveRoomIDs.has(a.roomID);
      const bIsLive = liveRoomIDs.has(b.roomID);

      if (aIsLive && !bIsLive) return -1;
      if (!aIsLive && bIsLive) return 1;
      return 0;
    });
  }, [branch.room, liveRoomIDs]);

  // فقط 3 گروه اول نمایش داده شود (با اولویت لایوها)
  const displayGroups = sortedGroups.slice(0, 3);
  const hasMoreGroups = sortedGroups.length > 3;

  // اگر شعبه وجود نداشت
  if (!branch) return null;
  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-all duration-200 overflow-hidden">
      <details
        open={isOpen}
        onToggle={(e) => setIsOpen(e.target.open)}
        className="open:bg-gradient-to-r open:from-[#30d4cc]/5 open:to-transparent transition-all duration-200"
      >
        <summary className="flex items-center justify-between p-5 cursor-pointer list-none rounded-xl">
          <div className="flex items-center gap-3">
            <div className="relative">
              <span className="text-3xl">{branch.icon || '📚'}</span>
              {hasLiveRoom && (
                <span className="absolute -top-1 -right-1 flex h-3 w-3">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-3 w-3 bg-red-500"></span>
                </span>
              )}
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="text-gray-800 font-semibold text-base">
                  {branch.fullname?.replace("مجمع عالی حکمت اسلامی", "شعبه") || branch.name?.replace("مجمع عالی حکمت اسلامی", "شعبه")}
                </span>
                <span className="text-xs text-gray-400">
                  ({branch.room?.length || 0} اتاق)
                </span>
              </div>
            </div>
          </div>
          <div className={`text-gray-400 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`}>
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </div>
        </summary>

        <div className="px-5 pb-5 pt-0 space-y-2">
          {displayGroups.map((group, gIdx) => {
            const isLive = liveRoomIDs.has(group.roomID);
            const liveData = getLiveRoom(group.roomID);

            return (
              <div
                key={group.roomID || gIdx}
                onClick={() => window.location.href = group.href || `/rooms/${group.roomID}/join`}
                className={`flex items-center justify-between p-3 pr-8 rounded-lg hover:bg-[#30d4cc]/10 transition-all duration-150 cursor-pointer group/item border ${isLive
                  ? 'border-red-300 bg-red-50/30 hover:bg-red-50/50'
                  : 'border-transparent hover:border-[#30d4cc]/20'
                  }`}
              >
                <div className="flex items-center gap-3">
                  <div className={`w-2 h-2 rounded-full ${isLive ? 'bg-red-500 animate-pulse' : 'bg-[#30d4cc]'}`}></div>
                  <div className="flex items-center gap-2">
                    <span className={`text-sm group-hover/item:text-[#30d4cc] transition-colors font-medium ${isLive ? 'text-red-600' : 'text-gray-700'
                      }`}>
                      {group.title?.replace("مجمع عالی حکمت اسلامی", "شعبه") || group.name?.replace("مجمع عالی حکمت اسلامی", "شعبه")}
                    </span>
                    {isLive && (
                      <div className="absolute top-3 right-3 flex items-center gap-1.5 bg-[#ef4444]/10 backdrop-blur-sm px-2 py-1 rounded-full z-10">
                        <span className="relative flex h-2 w-2">
                          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                          <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
                        </span>
                        <span className="text-[#dc2626] text-xs font-bold">زنده</span>
                      </div>
                    )}
                  </div>
                </div>
                {isLive ? (<div className='flex items-center gap-2'>
                  <span className="text-xs text-gray-500 bg-gray-100 px-2.5 py-1 rounded-full">
                    {liveData.countPartis} نفر
                  </span>
                </div>) : (<div className="flex items-center gap-2">
                  {group.count > 0 && (
                    <span className="text-xs text-gray-500 bg-gray-100 px-2.5 py-1 rounded-full">
                      📊 {group.count} جلسه
                    </span>
                  )}
                  <span className="text-xs text-gray-500 bg-gray-100 px-2.5 py-1 rounded-full">
                    👥 {group.countPartis || group.members || 0} عضو
                  </span>
                </div>)}

              </div>
            );
          })}

          {hasMoreGroups && (
            <div className="pr-8 pt-2">
              <button
                onClick={() => {
                  console.log('نمایش همه گروه‌های شعبه:', branch.fullname);
                }}
                className="text-xs text-[#30d4cc] hover:text-[#26b8b1] transition-colors flex items-center gap-1 group"
              >
                <span>+ {sortedGroups.length - 3} گروه دیگر</span>
                <svg className="w-3 h-3 group-hover:translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                </svg>
              </button>
            </div>
          )}
        </div>
      </details>
    </div>
  );
};

export default BranchItem;