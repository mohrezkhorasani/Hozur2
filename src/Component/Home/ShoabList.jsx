// BranchItem.jsx - کامپوننت تکی برای نمایش یک شعبه
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';

const BranchItem = ({ branch }) => {
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(false);

  // اگر شعبه وجود نداشت
  if (!branch) return null;

  // فقط 3 گروه اول نمایش داده شود
  const displayGroups = branch.room?.slice(0, 3) || [];
  const hasMoreGroups = branch.room?.length > 3;

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-all duration-200 overflow-hidden">
      <details 
        open={isOpen}
        onToggle={(e) => setIsOpen(e.target.open)}
        className="open:bg-gradient-to-r open:from-[#30d4cc]/5 open:to-transparent transition-all duration-200"
      >
        <summary className="flex items-center justify-between p-5 cursor-pointer list-none rounded-xl">
          <div className="flex items-center gap-3">
            <span className="text-3xl">{branch.icon || '📚'}</span>
            <div>
              <span className="text-gray-800 font-semibold text-base">
                {branch.fullname.replace("مجمع عالی حکمت اسلامی","شعبه") || branch.name.replace("مجمع عالی حکمت اسلامی","شعبه")}
              </span>
              <span className="text-xs text-gray-400 mr-2">
                ({branch.room?.length || 0} گروه)
              </span>
            </div>
          </div>
          <div className={`text-gray-400 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`}>
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </div>
        </summary>

        <div className="px-5 pb-5 pt-0 space-y-2">
          {displayGroups.map((group, gIdx) => (
            <div
              key={group.roomID || gIdx}
              onClick={() => window.location.href=group.href || `/rooms/${group.roomID}/join`}
              className="flex items-center justify-between p-3 pr-8 rounded-lg hover:bg-[#30d4cc]/10 transition-all duration-150 cursor-pointer group/item border border-transparent hover:border-[#30d4cc]/20"
            >
              <div className="flex items-center gap-3">
                <div className="w-2 h-2 rounded-full bg-[#30d4cc]"></div>
                <span className="text-sm text-gray-700 group-hover/item:text-[#30d4cc] transition-colors font-medium">
                  {group.title.replace("مجمع عالی حکمت اسلامی","شعبه") || group.name.replace("مجمع عالی حکمت اسلامی","شعبه")}
                </span>
              </div>
              <div className="flex items-center gap-2"> 
                {group.count > 0 && (
                  <span className="text-xs text-gray-500 bg-gray-100 px-2.5 py-1 rounded-full">
                    📊 {group.count} جلسه
                  </span>
                )}
                <span className="text-xs text-gray-500 bg-gray-100 px-2.5 py-1 rounded-full">
                  👥 {group.countPartis || group.members || 0} عضو
                </span>
              </div>
            </div>
          ))}

          {hasMoreGroups && (
            <div className="pr-8 pt-2">
              <button
                onClick={() => {
                  // می‌توانید به صفحه جزئیات شعبه هدایت کنید یا مودال باز کنید
                  console.log('نمایش همه گروه‌های شعبه:', branch.fullname);
                  // navigate(`/branch/${branch.id}/groups`);
                }}
                className="text-xs text-[#30d4cc] hover:text-[#26b8b1] transition-colors flex items-center gap-1 group"
              >
                <span>+ {branch.room.length - 3} گروه دیگر</span>
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