import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { GetLatestMeetings } from '../../api/GetRooms'; // adjust path as needed
import { formatPersianDate } from './../../Tools/tools';
import EmojiIcon from '../EmojiIcon';

// Helper: get avatar – if exists use it, otherwise show book icon element
const getAvatarContent = (avatar, title) => {
  if (avatar && avatar.trim() !== '') {
    return <EmojiIcon name={avatar} alt={title} className="w-8 h-8 rounded object-cover" />;
  }
  // Default book icon (SVG)
  return (
    <svg className="w-6 h-6 text-[#30d4cc]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
    </svg>
  );
};

export default function LatestMeetings() {
  const [meetings, setMeetings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchMeetings = async () => {
      try {
        setLoading(true);
        const data = await GetLatestMeetings();
        // API returns array of meeting objects
        setMeetings(Array.isArray(data) ? data : []);
        setError(null);
      } catch (err) {
        console.error('Failed to fetch latest meetings:', err);
        setError('خطا در دریافت جلسات');
      } finally {
        setLoading(false);
      }
    };

    fetchMeetings();
  }, []);

  const handleMeetingClick = (meeting) => {
    // Navigate to join page using roomID from roomJSON
    const roomId = meeting.roomJSON?.roomID;
    if (roomId) {
    //   navigate(`/join/${roomId}`);
    }
  };

  if (loading) {
    return (
      <aside className="lg:w-72 flex-shrink-0">
        <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden">
          <div className="bg-gradient-to-r from-[#30d4cc] to-[#2bc4bc] px-4 py-3">
            <h3 className="text-[#ffffff] font-bold text-base flex items-center gap-2">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              آخرین جلسات
            </h3>
          </div>
          <div className="p-6 text-center text-gray-500">در حال بارگذاری...</div>
        </div>
      </aside>
    );
  }

  if (error) {
    return (
      <aside className="lg:w-72 flex-shrink-0">
        <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden">
          <div className="bg-gradient-to-r from-[#30d4cc] to-[#2bc4bc] px-4 py-3">
            <h3 className="text-[#ffffff] font-bold text-base flex items-center gap-2">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              آخرین جلسات
            </h3>
          </div>
          <div className="p-6 text-center text-red-500">{error}</div>
        </div>
      </aside>
    );
  }

  return (
    <aside className="lg:w-72 shrink-0">
      <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden h-full">
        <div className="bg-gradient-to-r from-[#30d4cc] to-[#2bc4bc] px-4 py-3">
          <h3 className="text-[#ffffff] font-bold text-base flex items-center gap-2">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            آخرین جلسات
          </h3>
        </div>
        <div className="divide-y divide-[#f3f4f6] max-h-90 overflow-y-auto">
          {meetings.length === 0 ? (
            <div className="p-4 text-center text-gray-400 text-sm">جلسه‌ای یافت نشد</div>
          ) : (
            meetings.map((meeting) => {
              const { title, teacher, startAt, roomJSON } = meeting;
              const avatar = roomJSON?.avatar || null;
              const dateObj = formatPersianDate(startAt);
              const timeStr = dateObj.time;
              const displayDate = dateObj.prettDate; // today, tomorrow, weekday, or full Persian date

              return (
                <div
                  key={meeting.id}
                  onClick={() => handleMeetingClick(meeting)}
                  className="p-3 hover:bg-[#f9fafb] transition-all duration-200 cursor-pointer group"
                >
                  <div className="flex items-start gap-2">
                    <div className="flex-shrink-0 w-8 h-8 flex items-center justify-center">
                      {getAvatarContent(avatar, title)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <h4 className="font-semibold text-[#1f2937] group-hover:text-[#30d4cc] transition-colors text-xs mb-0.5 line-clamp-2">
                        {title}
                      </h4>
                      <p className="text-xs text-[#6b7280] truncate">{teacher || 'استاد'}</p>
                      <div className="flex items-center gap-2 text-xs text-[#9ca3af] mt-1">
                        <span className="flex items-center gap-0.5">
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                          </svg>
                          {displayDate}
                        </span>
                        <span className="flex items-center gap-0.5">
                          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                          {timeStr}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })
          )}
        </div>
        <div className="p-3 bg-[#f9fafb] border-t border-[#f3f4f6]">
          <button
            onClick={() => navigate('/meetings')} // adjust to your all meetings route
            className="w-full flex items-center justify-center gap-2 text-[#30d4cc] text-xs font-medium hover:gap-3 transition-all"
          >
            <span>مشاهده تمام جلسات</span>
            <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
      </div>
    </aside>
  );
}