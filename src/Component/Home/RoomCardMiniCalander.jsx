import React, { useState, useMemo } from 'react';
import { persianAndArabicToEnglishNumbers } from '../../Tools/tools';
import DayMeetingsModal from './MiniCalanderMeetingsModal';

const RoomCardMiniCalander = ({ data = [],handleDayClick }) => {
    // تنظیم روی ماه جاری شمسی
    const [currentMonth, setCurrentMonth] = useState(() => {
        const now = new Date();
        const jalaliDate = new Intl.DateTimeFormat('fa-IR', {
            calendar: 'persian',
            year: 'numeric',
            month: 'numeric'
        }).format(now);
        console.log(jalaliDate)

        // فرمت خروجی: 1404/12/5
        const [year, month] = persianAndArabicToEnglishNumbers(jalaliDate).split('/').map(Number);
        return { year, month: month - 1 };
    });

    const [hoveredDay, setHoveredDay] = useState(null);
    const [hoveredCount, setHoveredCount] = useState(0);

    // نام ماه‌های شمسی
    const persianMonths = [
        'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
        'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
    ];
    // پیدا کردن داده‌های ماه جاری
    const currentMonthData = useMemo(() => {
        return data.find(
            item => item.month === currentMonth.month && item.year === currentMonth.year
        );
    }, [data, currentMonth]);


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
        // ساخت یک تاریخ تقریبی
        const persianMonthDays = [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29];
        let totalDays = 0;

        // روزهای سال‌های گذشته
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

        // روزهای ماه‌های گذشته در سال جاری
        for (let m = 0; m < month; m++) {
            totalDays += persianMonthDays[m];
        }

        // 1 فروردین 1400 = 21 مارس 2021
        const epoch = new Date(2021, 2, 21);
        const targetDate = new Date(epoch.getTime() + totalDays * 86400000);

        // محاسبه روز هفته (0 = یکشنبه، تبدیل به شنبه = 0)
        let dayOfWeek = targetDate.getDay();
        let saturdayBased = (dayOfWeek + 1) % 7;
        return saturdayBased;
    };

    const daysInMonth = getDaysInMonth(currentMonth.year, currentMonth.month);
    const firstDayOfMonth = getFirstDayOfMonth(currentMonth.year, currentMonth.month);

    // ساخت آرایه روزها
    const days = [];
    // روزهای خالی قبل از شروع ماه
    for (let i = 0; i < firstDayOfMonth; i++) {
        days.push(null);
    }
    // روزهای ماه
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

    return (
        <div className="w-full bg-white rounded-xl shadow-lg p-4">
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
                      onClick={() => handleDayClick(getDayMeetings(day),day,currentMonth.month,currentMonth.year)}

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