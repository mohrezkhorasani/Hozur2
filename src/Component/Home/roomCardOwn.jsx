import React, { useState, useRef, useEffect } from 'react';
import { FaSignInAlt, FaCalendarPlus, FaPaintBrush, FaSpinner, FaEdit, FaUsers } from 'react-icons/fa';
import { GetRoomCalendarActivity } from '../../api/GetRooms';
import RoomCardMiniCalander from './RoomCardMiniCalander';
import { EndMeet } from '../../api/StartNewMeeting';
import { findNextPlan } from '../../Tools/tools';
import NextMeetingBadge from '../NextMeetBadge';

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
    handleEndMeet,
    timeAgo,
    menuRef,
    EmojiIcon,
    handleDayClick,
    onOpenEditRoomModal
}) => {
    const [showConfirmDialog, setShowConfirmDialog] = useState(false);
    const [roomToEnd, setRoomToEnd] = useState(null);
    const [endLoading, setEndLoading] = useState(false);
    const [isTestSession, setIsTestSession] = useState(false);
    const [isHoveringIcon, setIsHoveringIcon] = useState(false);
    const [calendarData, setCalendarData] = useState([]);
    const [calendarDataLoading, setCalendarDataLoading] = useState(false);
    const abortControllerRef = useRef(null);
    const [currentCalendarMonth, setCurrentCalendarMonth] = useState(null);
    const [currentCalendarYear, setCurrentCalendarYear] = useState(null);
    let nextPlan = findNextPlan(room.planJSONs);
    let hasNextPlan = !!nextPlan;

    // پاک کردن AbortController هنگام unmount
    useEffect(() => {
        return () => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }
        };
    }, []);

    // تابع لود دیتا با ماه و سال
    const loadData = async (month, year) => {
        if (!room?.roomID) return;

        setCalendarDataLoading(true);

        // کنسل کردن درخواست قبلی
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }

        abortControllerRef.current = new AbortController();

        try {
            const teas = await GetRoomCalendarActivity(
                room.roomID,
                month,
                year,
                abortControllerRef.current.signal
            );
            console.log("Calendar data loaded:", teas);
            setCalendarData(teas);
        } catch (error) {
            if (error.name !== 'AbortError') {
                console.error("Error loading calendar data:", error);
            }
        } finally {
            setCalendarDataLoading(false);
        }
    };

    // تابع هندل تغییر ماه در تقویم
    const handleMonthChange = (month, year) => {
        console.log("Month changed to:", month, year);
        setCurrentCalendarMonth(month);
        setCurrentCalendarYear(year);
        // دیتا مستقیماً در کامپوننت MiniCalander لود می‌شه
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
                    className={`backface-hidden rounded-2xl shadow-lg border border-[#f3f4f6] card-hover-effect overflow-visible
        ${hasNextPlan ? 'bg-linear-to-br from-[#ffffff] to-[#f0fdfa] border-[#30d4cc]/20' : 'bg-[#ffffff]'}`}
                    style={{ backfaceVisibility: 'hidden' }}
                >
                    {/* Live Indicator */}
                    {isLive && (
                        <div className="absolute top-3 right-3 z-10">
                            <div className="flex items-center gap-1.5 bg-linear-to-r from-[#ef4444] to-[#dc2626] text-[#ffffff] px-3 py-1 rounded-full text-xs font-semibold shadow-lg">
                                <div className="relative flex h-2 w-2">
                                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#ffffff] opacity-75"></span>
                                    <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ffffff]"></span>
                                </div>
                                LIVE
                            </div>
                        </div>
                    )}

                    {/* Edit Room Button for Owners & Admins */}
                    {(room.isAdmin || room.isOwner) && (
                        <div className="absolute top-3 left-3 z-20 flex items-center gap-2">
                            <button
                                type="button"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    if (onOpenEditRoomModal) {
                                        onOpenEditRoomModal(room.roomID);
                                    }
                                }}
                                className="p-2 bg-white/90 hover:bg-[#30d4cc] text-gray-600 hover:text-white rounded-full shadow-md hover:shadow-lg transition-all border border-gray-200"
                                title="ویرایش کامل اتاق"
                            >
                                <FaEdit className="w-4 h-4" />
                            </button>
                            <button
                                type="button"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    if (onOpenEditRoomModal) {
                                        onOpenEditRoomModal(room.roomID);
                                    }
                                }}
                                className="relative p-2 bg-white/90 hover:bg-[#30d4cc] text-gray-600 hover:text-white rounded-full shadow-md hover:shadow-lg transition-all border border-gray-200"
                                title="اعضا"
                            >
                                <FaUsers className="w-4 h-4" />
                                {room.waitingMembersCount > 0 && (
                                    <span className="absolute -top-1.5 -right-1.5 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white ring-2 ring-white">
                                        {room.waitingMembersCount}
                                    </span>
                                )}
                            </button>
                        </div>
                    )}

                    {/* Content Front */}
                    <div className="relative p-6">
                        {/* Icon */}
                        <div className="text-center mb-4">
                            <div
                                onClick={async () => {
                                    onFlip(room.roomID);
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
                                    transition-all duration-500`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
                        {hasNextPlan && (
                            <NextMeetingBadge plan={nextPlan} />
                        )}
                        {/* Action Buttons */}
                        <div className="grid grid-cols-4 gap-2 mt-4">

                            <button className={`col-span-2 py-2.5 rounded-xl text-sm font-semibold transition-all
                                     duration-200 flex items-center
                                     ${isLive || room.isAdmin ? "hidden" : ""}
                                     justify-center gap-2 relative ${isStartingMeet === room.roomID
                                    ? "bg-[#f3f4f6] text-[#9ca3af] cursor-not-allowed"
                                    : isLive
                                        ? "bg-[#22c55e] text-[#ffffff] hover:bg-[#16a34a] active:scale-95 shadow-md hover:shadow-lg"
                                        : "bg-[#9230d4] text-[#ffffff] hover:bg-[#ff00bf] active:scale-95 shadow-md hover:shadow-lg"
                                }`}
                                onClick={() => {
                                    window.open(`/rooms/${room.roomID}/join`, '_blank');
                                }}

                            >
                                فرم ورود
                            </button>
                            <button
                                ref={el => menuRef.current[room.roomID] = el}
                                onClick={() => onMainButtonClick(room)}
                                disabled={isStartingMeet === room.roomID || !room.isAdmin}
                                className={`col-span-2 py-2.5 rounded-xl text-sm font-semibold transition-all
                                     duration-200 flex items-center
                                     ${room.isAdmin ? "" : "hidden"}
                                     justify-center gap-2 relative ${isStartingMeet === room.roomID
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
                                    isLive ? "ورود" : room.isAdmin ? "شروع" : "مدیر نیستید"
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
                                                        let teachers = [];
                                                        let teacherSplit = plan.teacher.split("،");
                                                        teacherSplit.map((t => {
                                                            teachers.push({ name: t });
                                                        }));
                                                        console.log("planroomcard", plan);
                                                        handleStart(
                                                            room.roomID,
                                                            plan.id,
                                                            plan.title,
                                                            teachers || [],
                                                            plan.dabir || "",
                                                            true,
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
                            {isLive ?
                                (
                                    <button
                                        onClick={async () => {
                                            setRoomToEnd(room.roomID);
                                            setShowConfirmDialog(true);
                                        }}
                                        className="py-2.5 rounded-xl border-2 bg-[#FFFFFF] hover:bg-[#ff0000] border-[#ff0000] text-[#ff0000] hover:border-[#ff0000] hover:text-[#ff0000] hover:bg-[#30d4cc]/5 transition-all duration-200 flex items-center justify-center group"
                                        title="پایان جلسه"
                                    >
                                        <span className='group-hover:inline hidden transition-all duration-75 text-[#FFFFFF] text-xs mr-2'>پایان جلسه</span>
                                        <svg className="w-4 h-4 group-hover:hidden transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                )
                                : (
                                    <button
                                        onClick={() => onCopy(room.roomID)}
                                        className="py-2.5 rounded-xl border border-[#e5e7eb] text-[#6b7280] hover:border-[#30d4cc] hover:text-[#30d4cc] hover:bg-[#30d4cc]/5 transition-all duration-200 flex items-center justify-center group"
                                        title="کپی لینک"
                                    >
                                        <svg className="w-4 h-4 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                                        </svg>
                                    </button>
                                )
                            }
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
                            <div className='w-full'>
                                <div className="flex items-center justify-between mb-4">
                                    <h3 className="text-lg font-bold text-[#005868]">
                                        <EmojiIcon name={room.avatar} />
                                        <span className="mx-3">
                                            {room.title}
                                        </span>
                                    </h3>
                                </div>
                                <RoomCardMiniCalander
                                    data={calendarData}
                                    roomid={room.roomID}
                                    handleDayClick={handleDayClick}
                                    onMonthChange={handleMonthChange}
                                />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* دیالوگ تایید پایان جلسه */}
            {showConfirmDialog && (
                <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-6 max-w-sm mx-4">
                        <h3 className="text-lg font-semibold mb-2">پایان جلسه</h3>
                        <p className="text-gray-600 mb-4">آیا از پایان این جلسه مطمئن هستید؟</p>
                        
                        <label className="flex items-center gap-2 mb-6 cursor-pointer border border-amber-200 bg-amber-50 p-2 rounded-lg">
                            <input
                                type="checkbox"
                                checked={isTestSession}
                                onChange={(e) => setIsTestSession(e.target.checked)}
                                className="w-4 h-4 text-amber-600 rounded border-amber-300 focus:ring-amber-500"
                            />
                            <div className="flex flex-col">
                                <span className="text-sm font-bold text-amber-800">این یک جلسه تستی بود!</span>
                                <span className="text-[10px] text-amber-600 font-medium">(توجه: جلسات تستی در گزارشات و سوابق ذخیره نمی‌شوند)</span>
                            </div>
                        </label>

                        <div className="flex gap-3 justify-end">
                            <button
                                onClick={() => {
                                    setShowConfirmDialog(false);
                                    setIsTestSession(false);
                                }}
                                className="px-4 py-2 rounded-lg border border-gray-300 hover:bg-gray-50"
                            >
                                انصراف
                            </button>
                            <button
                                onClick={async () => {
                                    setEndLoading(true);
                                    const data = await EndMeet(room.roomID, isTestSession);
                                    console.log(data);
                                    if (data.status === 200) {
                                        setShowConfirmDialog(false);
                                        setRoomToEnd(null);
                                        handleEndMeet(roomToEnd);
                                    } else {
                                        alert(data.message);
                                    }
                                    setEndLoading(false);
                                }}
                                disabled={endLoading}
                                className={`px-4 py-2 rounded-lg bg-red-500 text-white hover:bg-red-600 ${endLoading ? "disabled:cursor-not-allowed" : ""}`}
                            >
                                {endLoading ? "در حال انجام..." : "بله، پایان جلسه"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default RoomCardOwn;