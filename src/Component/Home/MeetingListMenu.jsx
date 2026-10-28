import { useState } from 'react';
import { formatPersianDate, getMeetingDateTime } from "../../Tools/tools";

export const MeetingMenu = ({ planList }) => {
  const [showAll, setShowAll] = useState(false);

  const displayData = showAll ? planList : planList?.slice(0, 3);
  console.log("planList",displayData)
  return (
    <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden mt-7">
      <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-5 py-4">
        <h3 className="text-[#ffffff] font-bold text-lg flex items-center gap-2">
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          برنامه جلسات آتی
        </h3>
      </div>

      <div className="divide-y divide-[#f3f4f6]">
        {displayData && displayData.length > 0 ? (
          displayData.map((item) => {
            const { time, date, prettDate } = formatPersianDate(item.startAt);
            const isPast = new Date(item.startAt) < new Date();

            return (
              <a
                key={item.id}
                href={`/rooms/${item.roomJSON?.roomID}/join`}
                className={`block px-5 py-4 hover:bg-[#f9fafb] transition-all group ${isPast ? 'opacity-60' : ''}`}
              >
                {/* محتوای هر جلسه - بدون تغییر */}
                <div className="flex flex-col gap-2">
                  <div className="inline-flex">
                    <span className="text-[#0b3f32] text-xs font-medium bg-[#e6fffa] px-2.5 py-0.5 rounded-full">
                      {item.roomJSON?.title || 'گروه حکمت'}
                    </span>
                  </div>

                  <div className="flex items-start justify-between gap-2">
                    <h4 className="text-[#1f2937] font-semibold text-sm leading-relaxed group-hover:text-[#0b3f32] transition-colors line-clamp-2">
                      ⬅️ {item.title}
                    </h4>
                    <svg className="w-4 h-4 text-[#9ca3af] group-hover:text-[#30d4cc] group-hover:translate-x-1 transition-all flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>

                  <div className="flex flex-wrap items-center gap-3 mt-1 text-xs text-[#6b7280]">
                    <span className="flex items-center gap-1.5">
                      <svg className="w-3.5 h-3.5 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                      <span>{item.teacher || 'نامشخص'}</span>
                    </span>

                    <span className="flex items-center gap-1.5">
                      <svg className="w-3.5 h-3.5 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                      <span>
                        {prettDate === 'امروز' ? 'امروز' :
                          prettDate === 'فردا' ? 'فردا' :
                            prettDate === 'پس فردا' ? 'پس فردا' :
                              prettDate} ساعت {time}  
                      </span>
                    </span>
                  </div>
                </div>
              </a>
            );
          })
        ) : (
          <div className="px-5 py-8 text-center text-[#6b7280] text-sm">
            فعلاً جلسه‌ای در آینده تنظیم نشده است
          </div>
        )}
      </div>

      {planList?.length > 3 && (
        <div className="p-4 bg-[#f9fafb] border-t border-[#f3f4f6]">
          <button
            onClick={() => setShowAll(!showAll)}
            className="flex items-center justify-center gap-2 text-[#30d4cc] text-sm font-medium hover:gap-3 transition-all w-full"
          >
            <span>{showAll ? 'نمایش کمتر' : 'مشاهده همه نشست‌ها'}</span>
            <svg className={`w-4 h-4 transition-transform ${showAll ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>
        </div>
      )}
    </div>
  );
};  