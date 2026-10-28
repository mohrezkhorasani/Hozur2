import React, { useState, useMemo, useEffect, useRef } from 'react';
import { persianAndArabicToEnglishNumbers } from '../../Tools/tools';
import DayMeetingsModal from './MiniCalanderMeetingsModal';
import { GetRoomCalendarActivity } from '../../api/GetRooms';

const RoomCardMiniCalander = ({ data = [], roomid, handleDayClick, load, onMonthChange }) => {
    // تنظیم روی ماه جاری شمسی
    const [currentMonth, setCurrentMonth] = useState(() => {
        const now = new Date();
        const jalaliDate = new Intl.DateTimeFormat('fa-IR', {
            calendar: 'persian',
            year: 'numeric',
            month: 'numeric'
        }).format(now);

        const [year, month] = persianAndArabicToEnglishNumbers(jalaliDate).split('/').map(Number);
        return { year, month: month - 1 };
    });

    const [hoveredDay, setHoveredDay] = useState(null);
    const [hoveredCount, setHoveredCount] = useState(0);
    const [calendarData, setCalendarData] = useState(data);
    const [calendarDataLoading, setCalendarDataLoading] = useState(false);
    const abortControllerRef = useRef(null);

    // نام ماه‌های شمسی
    const persianMonths = [
        'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
        'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
    ];

    // پیدا کردن داده‌های ماه جاری       
    const currentMonthData = useMemo(() => {
        return calendarData.find(
            item => item.month === currentMonth.month && item.year === currentMonth.year
        );
    }, [calendarData, currentMonth]);

    // دریافت تعداد جلسات یک روز خاص (حداکثر 3)
    const getDayCount = (day) => {
        const activity = currentMonthData?.activities?.find(a => a.day === day);
        const count = activity?.count || 0;
        return Math.min(count, 3);
    };

    // تابع دریافت لیست جلسات یک روز
    const getDayMeetings = (day) => {
        const activity = currentMonthData?.activities?.find(a => a.day === day);
        return activity?.meetJSONs || [];
    };

    // دریافت رنگ بر اساس تعداد جلسات
    const getDayColor = (count) => {
        switch (count) {
            case 0: return 'bg-[#ebedf0]';
            case 1: return 'bg-[#9be9a8]';
            case 2: return 'bg-[#40c463]';
            case 3: return 'bg-[#216e39]';
            default: return 'bg-[#ebedf0]';
        }
    };

    // تابع لود دیتا با ماه و سال
    const loadData = async (month, year) => {
        if (!roomid) return;
        
        setCalendarDataLoading(true);
        
        // کنسل کردن درخواست قبلی
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
        }
        
        abortControllerRef.current = new AbortController();

        try {
            const teas = await GetRoomCalendarActivity(
                roomid,
                month,
                year,
                abortControllerRef.current.signal
            );
            console.log("Data loaded for:", month, year, teas);
            setCalendarData(teas);
        } catch (error) {
            if (error.name !== 'AbortError') {
                console.error("Error loading calendar data:", error);
            }
        } finally {
            setCalendarDataLoading(false);
        }
    };

    // وقتی ماه عوض می‌شه، دیتا رو لود کن
    useEffect(() => {
        const monthName = persianMonths[currentMonth.month];
        const year = currentMonth.year;
        
        // لود کردن دیتا با ماه و سال جدید
        loadData(monthName, year);
        
        // اگر تابع onMonthChange وجود داشت، صدا بزن
        if (onMonthChange) {
            onMonthChange(monthName, year);
        }
    }, [currentMonth.month, currentMonth.year, roomid]);

    // تولید روزهای ماه شمسی
    const getDaysInMonth = (year, month) => {
        const monthDays = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];

        const isLeapYear = (y) => {
            const diff = y - 1375;
            const leapCycle = ((diff % 33) + 33) % 33;
            return leapCycle === 1 || leapCycle === 5 || leapCycle === 9 ||
                leapCycle === 13 || leapCycle === 17 || leapCycle === 22 ||
                leapCycle === 26 || leapCycle === 30;
        };

        if (month === 11 && isLeapYear(year)) {
            return 30;
        }
        return monthDays[month];
    };

    // تبدیل تاریخ شمسی به میلادی (ساده شده)
    const getFirstDayOfMonth = (year, month) => {
        const persianMonthDays = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
        let totalDays = 0;

        for (let y = 1400; y < year; y++) {
            totalDays += 365;
            const diff = y - 1375;
            const leapCycle = ((diff % 33) + 33) % 33;
            if (leapCycle === 1 || leapCycle === 5 || leapCycle === 9 ||
                leapCycle === 13 || leapCycle === 17 || leapCycle === 22 ||
                leapCycle === 26 || leapCycle === 30) {
                totalDays++;
            }
        }

        for (let m = 0; m < month; m++) {
            totalDays += persianMonthDays[m];
        }

        const epoch = new Date(2021, 2, 21);
        const targetDate = new Date(epoch.getTime() + totalDays * 86400000);
        let dayOfWeek = targetDate.getDay();
        let saturdayBased = (dayOfWeek + 1) % 7;
        return saturdayBased;
    };

    const daysInMonth = getDaysInMonth(currentMonth.year, currentMonth.month);
    const firstDayOfMonth = getFirstDayOfMonth(currentMonth.year, currentMonth.month);

    // ساخت آرایه روزها
    const days = [];
    for (let i = 0; i < firstDayOfMonth; i++) {
        days.push(null);
    }
    for (let i = 1; i <= daysInMonth; i++) {
        days.push(i);
    }

    const goToPreviousMonth = () => {
        if (currentMonth.month === 0) {
            setCurrentMonth({ year: currentMonth.year - 1, month: 11 });
        } else {
            setCurrentMonth({ ...currentMonth, month: currentMonth.month - 1 });
        }
    };

    const goToNextMonth = () => {
        if (currentMonth.month === 11) {
            setCurrentMonth({ year: currentMonth.year + 1, month: 0 });
        } else {
            setCurrentMonth({ ...currentMonth, month: currentMonth.month + 1 });
        }
    };

    const handleDayHover = (day) => {
        if (day) {
            const count = getDayCount(day);
            setHoveredDay(day);
            setHoveredCount(count);
        } else {
            setHoveredDay(null);
            setHoveredCount(0);
        }
    };

    // پاک کردن AbortController هنگام unmount
    useEffect(() => {
        return () => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }
        };
    }, []);

    return (
        <div className="w-full bg-white rounded-xl shadow-lg p-4">
            {/* نمایش لودینگ */}
            {calendarDataLoading && (
                <div className="absolute inset-0 bg-white/50 flex items-center justify-center rounded-xl z-10">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-[#30d4cc]"></div>
                </div>
            )}
            
            {/* Header با نام ماه و فلش‌ها */}
            <div className="flex items-center justify-between mb-4">
                <button
                    onClick={goToPreviousMonth}
                    className="p-1.5 hover:bg-gray-100 rounded-full transition-all duration-200"
                >
                    <svg className="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                    </svg>
                </button>

                <h2 className="text-base font-bold text-gray-800">
                    {persianMonths[currentMonth.month]} {currentMonth.year}
                </h2>

                <button
                    onClick={goToNextMonth}
                    className="p-1.5 hover:bg-gray-100 rounded-full transition-all duration-200"
                >
                    <svg className="w-4 h-4 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                </button>
            </div>

            {/* روزهای هفته */}
            <div className="grid grid-cols-7 gap-1 mb-2">
                {['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'].map((day, index) => (
                    <div key={index} className="text-center text-[10px] font-semibold text-gray-500 py-1">
                        {day}
                    </div>
                ))}
            </div>

            {/* مربع‌های روزهای ماه */}
            <div className="grid grid-cols-7 gap-1">
                {days.map((day, index) => {
                    const count = day ? getDayCount(day) : 0;
                    const bgColor = getDayColor(count);

                    return (
                        <div
                            key={index}
                            className="relative"
                            onMouseEnter={() => handleDayHover(day)}
                            onMouseLeave={() => handleDayHover(null)}
                        >
                            {day ? (
                                <>
                                    <div
                                        className={`
                                            aspect-square rounded-md flex items-center justify-center
                                            transition-all duration-200 cursor-pointer max-w-8 max-h-8
                                            ${bgColor}
                                            hover:scale-85 hover:shadow-md
                                        `}
                                        onClick={() => handleDayClick(getDayMeetings(day), day, currentMonth.month, currentMonth.year)}
                                    >
                                        <span className={`text-[10px] font-light
                                            ${count > 0 ? 'text-white' : 'text-gray-600'}`}>
                                            {day}
                                        </span>
                                    </div>

                                    {/* Tooltip */}
                                    {hoveredDay === day && (
                                        <div className="absolute bottom-full 
                                            left-1/2 transform -translate-x-1/2 mb-1 z-20">
                                            <div className="bg-gray-800 text-white text-[10px] rounded-lg px-2 py-1 whitespace-nowrap shadow-lg">
                                                {count === 0 ? 'بدون جلسه' : `${count} جلسه`}
                                            </div>
                                            <div className="absolute left-1/2 transform -translate-x-1/2 -bottom-0.5 w-1.5 h-1.5 bg-gray-800 rotate-45"></div>
                                        </div>
                                    )}
                                </>
                            ) : (
                                <div className=""></div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

export default RoomCardMiniCalander;