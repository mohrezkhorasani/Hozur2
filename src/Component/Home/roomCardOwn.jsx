import React, { useState, useRef, useEffect } from 'react';
import { FaSignInAlt, FaCalendarPlus, FaPaintBrush, FaSpinner } from 'react-icons/fa';
import { GetRoomCalendarActivity } from '../../api/GetRooms';
import RoomCardMiniCalander from './RoomCardMiniCalander';

const RoomCardOwn = ({
    room,
    isLive,
    isStartingMeet,
    showPlanMenu,
    activeMenu,
    flippedCards,
    onFlip,
    onMainButtonClick,
    onCopy,
    onSetShowPlanMenu,
    onSetActiveMenu,
    onSetSelectedRoomForMeeting,
    onSetIsStartMeetingModalOpen,
    onSetIsPlanModalOpen,
    onHandleOpenNextMeetingModal,
    onSetModalOpenMobileEmojiModal,
    getFuturePlans,
    handleStart,
    timeAgo,
    menuRef,
    EmojiIcon,
    handleDayClick
}) => {
    const [isHoveringIcon, setIsHoveringIcon] = useState(false);
    const [calendarData, setCalendarData] = useState([])
    const [calendarDataLoading, setCalendarDataLoading] = useState(true)

    useEffect(() => {
    }, [])
    const loadData = async () => {

        const teas = await GetRoomCalendarActivity(room.roomID);
        console.log(teas)
        setCalendarData(teas)
        setCalendarDataLoading(false)
    };

    return (
        <div
            className="relative perspective-1000"
            style={{ perspective: '1000px' }}
        >
            <div
                className={`relative w-full transition-transform duration-700 transform-gpu preserve-3d
          ${flippedCards[room.roomID] ? 'rotate-y-180 h-100' : ''}`}

                onMouseEnter={() => setIsHoveringIcon(true)}
                onMouseLeave={() => setIsHoveringIcon(false)}
                style={{ transformStyle: 'preserve-3d' }}
            >
                {/* روی کارت (Front) */}
                <div
                    className="backface-hidden bg-[#ffffff] rounded-2xl shadow-lg border border-[#f3f4f6] card-hover-effect overflow-visible"
                    style={{ backfaceVisibility: 'hidden' }}
                >
                    {/* Live Indicator */}
                    {isLive && (
                        <div className="absolute top-3 left-3 z-10">
                            <div className="flex items-center gap-1.5 bg-linear-to-r from-[#ef4444] to-[#dc2626] text-[#ffffff] px-3 py-1 rounded-full text-xs font-semibold shadow-lg">
                                <div className="relative flex h-2 w-2">
                                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#ffffff] opacity-75"></span>
                                    <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ffffff]"></span>
                                </div>
                                LIVE
                            </div>
                        </div>
                    )}

                    {/* Content Front */}
                    <div className="relative p-6">
                        {/* Icon - با تغییر آیکون روی hover */}
                        <div className="text-center mb-4">
                            <div
                                onClick={async () => {
                                    loadData()
                                    onFlip(room.roomID)
                                }}
                                className="w-20 h-20 mx-auto bg-linear-to-br
                 from-[#30d4cc]/10 to-[#30d4cc]/20 rounded-2xl
                  flex items-center justify-center mb-4 shadow-inner cursor-pointer hover:shadow-lg hover:bg-[#30d4cc]
                   transition-all duration-300"
                            >

                                <svg className={`w-10 h-10 text-[#30d4cc] hover:text-[#ffffff] ${isHoveringIcon ? "" : "hidden"}
                                     transition-all duration-500`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3" />
                                </svg>

                                <svg className={`w-10 h-10 text-[#30d4cc] hover:text-[#ffffff] ${isHoveringIcon ? "hidden" : ""} 
                                    transition-all duration-500 `} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                </svg>
                            </div>

                            <h2 className="text-lg font-bold text-[#1f2937] mb-2 line-clamp-1">
                                <EmojiIcon name={room.avatar} />
                                <span className="mx-3">
                                    {room.title}
                                </span>
                            </h2>

                            <div className="flex items-center justify-center gap-4 text-xs text-[#6b7280]">
                                <div className="flex items-center gap-1">
                                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <span>{timeAgo(room.lastMeet)}</span>
                                </div>
                                {room.count > 0 && (
                                    <div className="flex items-center gap-1">
                                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                        </svg>
                                        <span>{room.count} جلسه</span>
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Action Buttons */}
                        <div className="grid grid-cols-4 gap-2 mt-4">
                            <button
                                ref={el => menuRef.current[room.roomID] = el}
                                onClick={() => onMainButtonClick(room)}
                                disabled={isStartingMeet === room.roomID}
                                className={`col-span-2 py-2.5 rounded-xl text-sm font-semibold transition-all duration-200 flex items-center justify-center gap-2 relative ${isStartingMeet === room.roomID
                                    ? "bg-[#f3f4f6] text-[#9ca3af] cursor-not-allowed"
                                    : isLive
                                        ? "bg-[#22c55e] text-[#ffffff] hover:bg-[#16a34a] active:scale-95 shadow-md hover:shadow-lg"
                                        : "bg-[#30d4cc] text-[#ffffff] hover:bg-[#26b8b1] active:scale-95 shadow-md hover:shadow-lg"
                                    }`}
                            >
                                {isStartingMeet === room.roomID ? (
                                    <svg className="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 8.001l-2.707 2.707A9.959 9.959 0 004 20.291z"></path>
                                    </svg>
                                ) : (
                                    isLive ? "ورود" : "شروع"
                                )}

                                {/* منوی dropdown برای انتخاب plan */}
                                {showPlanMenu === room.roomID && !isLive && (
                                    <div className="absolute bottom-full left-0 right-0 mb-2 bg-white rounded-xl shadow-xl border border-gray-200 overflow-hidden z-50">
                                        <div className="py-2">
                                            <div className="text-xs text-gray-400 px-4 py-1 border-b">برنامه‌های پیش‌رو</div>
                                            {getFuturePlans(room.planJSONs).map((plan, idx) => (
                                                <button
                                                    key={idx}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        onSetShowPlanMenu(null);
                                                        let teachers = []
                                                        let teacherSplit = plan.teacher.split("،")
                                                        teacherSplit.map((t => {
                                                            teachers.push({ name: t })
                                                        }))
                                                        handleStart(
                                                            room.roomID,
                                                            plan.title,
                                                            teachers || [],
                                                            plan.dabir || "",
                                                            true
                                                        );
                                                    }}
                                                    className="w-full text-right px-4 py-2.5 hover:bg-gray-50 transition-colors flex flex-col gap-1"
                                                >
                                                    <span className="text-sm font-medium text-[#636363]">{plan.title}</span>
                                                    <span className="text-xs text-gray-400">
                                                        📅 {plan.startAt ? new Date(plan.startAt).toLocaleDateString('fa-IR') : ''}
                                                    </span>
                                                </button>
                                            ))}
                                            <div className="border-t my-1"></div>
                                            <button
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    onSetShowPlanMenu(null);
                                                    onSetSelectedRoomForMeeting(room);
                                                    onSetIsStartMeetingModalOpen(true);
                                                }}
                                                className="w-full text-right px-4 py-2.5 text-sm text-[#30d4cc] hover:bg-gray-50 transition-colors flex items-center gap-2"
                                            >
                                                <span>⚡</span>
                                                <span>شروع جلسه بدون برنامه</span>
                                            </button>
                                        </div>
                                    </div>
                                )}
                            </button>

                            <button
                                onClick={() => onCopy(room.roomID)}
                                className="py-2.5 rounded-xl border border-[#e5e7eb] text-[#6b7280] hover:border-[#30d4cc] hover:text-[#30d4cc] hover:bg-[#30d4cc]/5 transition-all duration-200 flex items-center justify-center group"
                                title="کپی لینک"
                            >
                                <svg className="w-4 h-4 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                                </svg>
                            </button>

                            <div className="relative">
                                <button
                                    onClick={() => {
                                        if (activeMenu === room.roomID) {
                                            onSetActiveMenu(null);
                                        } else {
                                            onSetActiveMenu(room.roomID);
                                        }
                                    }}
                                    className="py-2.5 rounded-xl border border-[#e5e7eb] text-[#6b7280] hover:border-[#30d4cc] hover:text-[#30d4cc] hover:bg-[#30d4cc]/5 transition-all duration-200 flex items-center justify-center w-full"
                                    title="عملیات"
                                >
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z" />
                                    </svg>
                                </button>

                                {/* Dropdown Menu */}
                                {activeMenu === room.roomID && (
                                    <>
                                        <div
                                            className="fixed inset-0 z-99 overflow-visible"
                                            onClick={() => onSetActiveMenu(null)}
                                        />
                                        <div className="absolute left-0 -top-50
                                         mt-1 w-48 bg-[#ffffff] rounded-xl shadow-lg 
                                         border border-[#e5e7eb] overflow-visible z-100 animate-fadeIn">
                                            <button
                                                onClick={() => {
                                                    window.open(`/rooms/${room.roomID}/join`, '_blank');
                                                    onSetActiveMenu(null);
                                                }}
                                                className="w-full px-4 py-3 text-right text-sm
                                                 text-[#374151] hover:bg-[#f3f4f6] transition-colors 
                                                 flex items-center gap-3 border-b border-[#f3f4f6]"
                                            >
                                                <FaSignInAlt className="text-[#30d4cc] text-sm" />
                                                <span>فرم ورود به جلسه</span>
                                            </button>
                                            <button
                                                onClick={() => {
                                                    onSetSelectedRoomForMeeting(room);
                                                    onSetIsPlanModalOpen(true);
                                                    onSetActiveMenu(null);
                                                }}
                                                className="w-full px-4 py-3 text-right text-sm z-90 text-[#374151] hover:bg-[#f3f4f6] transition-colors flex items-center gap-3"
                                            >
                                                <FaCalendarPlus className="text-[#30d4cc] text-sm" />
                                                <span>لیست جلسات آتی</span>
                                            </button>
                                            <button
                                                onClick={() => {
                                                    onSetSelectedRoomForMeeting(room);
                                                    onHandleOpenNextMeetingModal(room);
                                                    onSetActiveMenu(null);
                                                }}
                                                className="w-full px-4 py-3 text-right text-sm z-90 text-[#374151] hover:bg-[#f3f4f6] transition-colors flex items-center gap-3"
                                            >
                                                <FaCalendarPlus className="text-[#30d4cc] text-sm" />
                                                <span>تنظیم جلسه آتی</span>
                                            </button>
                                            <button
                                                onClick={() => {
                                                    onSetModalOpenMobileEmojiModal(true);
                                                    onSetSelectedRoomForMeeting(room);
                                                }}
                                                className="w-full px-4 py-3 text-right text-sm z-90 text-[#374151] hover:bg-[#f3f4f6] transition-colors flex items-center gap-3"
                                            >
                                                <FaPaintBrush className="text-[#30d4cc] text-sm" />
                                                <span>تنظیم آیکون</span>
                                            </button>
                                        </div>
                                    </>
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                {/* پشت کارت (Back) */}
                <div
                    className="absolute top-0 left-0 w-full h-full backface-hidden bg-linear-to-br from-[#30d4cc] to-[#26b8b1] rounded-2xl shadow-lg p-6 rotate-y-180"
                    style={{ backfaceVisibility: 'hidden' }}
                >
                    <div className="flex flex-col h-full relative">
                        {/* دکمه برگشت */}
                        <button
                            onClick={() => onFlip(room.roomID)}
                            className="absolute top-0 left-0 w-10 h-10 rounded-full bg-white/20 hover:bg-white/30 backdrop-blur-sm transition-all duration-200 flex items-center justify-center group"
                        >
                            <svg className="w-5 h-5 text-white transform group-hover:-translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                            </svg>
                        </button>

                        {/* محتوای پشت کارت */}
                        <div className="flex-1 flex flex-col justify-center items-center text-center space-y-6">
                            {calendarDataLoading ? (
                                <>  <FaSpinner className="animate-spin" />
                                    <span>در حال دریافت داده ها...</span></>
                            ) : (
                                <div className='w-full'>
                                    <div className="flex items-center justify-between mb-4">
                                        <h3 className="text-lg font-bold text-[#005868]">
                                            <EmojiIcon name={room.avatar} />
                                            <span className="mx-3">
                                                {room.title}
                                            </span>
                                        </h3>

                                    </div>
                                    <RoomCardMiniCalander data={calendarData} handleDayClick={handleDayClick} />
                                </div>
                            )}

                        </div>
                    </div>
                </div>
            </div>
        </div >
    );
};

export default RoomCardOwn;