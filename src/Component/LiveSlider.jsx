// LiveSectionSlider.jsx
import { useState, useEffect } from "react";
import RoomCard, { RoomCardLoadingGrid } from './Home/RoomCard';
import RoomCardLive from "./Home/RoomCardLive";

export default function LiveSectionSlider({
    roomLive = [],
    rooms = [],
    loading = false,
    title = "پخش زنده",
    subtitle = "پخش زنده مجمع عالی حکمت اسلامی در اینجا نمایش داده میشود",
    className = ""
}) {
    const [currentPage, setCurrentPage] = useState(0);
    const [isLive, setIsLive] = useState(false);
    const [isMobile, setIsMobile] = useState(false);
    let itemsPerPage = 3;

    // تشخیص سایز موبایل
    useEffect(() => {
        const checkMobile = () => {
            setIsMobile(window.innerWidth < 768); // 768px breakpoint for mobile
        };
        checkMobile();
        window.addEventListener('resize', checkMobile);
        return () => window.removeEventListener('resize', checkMobile);
    }, []);
    // آماده‌سازی داده‌های کامل
    let validRooms = roomLive
        .map(item => {
            const fullRoom = rooms.find(room => room.roomID === item.roomID);
            return fullRoom ? { ...fullRoom, live: item } : null;
        })
        .filter(item => item !== null);
    if (!isLive) {
        itemsPerPage = 4
        validRooms = rooms
            .map(item => {
                const fullRoom = rooms.find(room => room.roomID === item.roomID);
                return fullRoom ? { ...fullRoom, live: item } : null;
            })
    }
    useEffect(() => {
        if (roomLive.length != 0)
            setIsLive(true)
        else
            setIsLive(false)
    }, [roomLive])

    let totalPages = Math.ceil(validRooms.length / itemsPerPage);

    const startIndex = currentPage * itemsPerPage;
    const currentItems = validRooms.slice(startIndex, startIndex + itemsPerPage);

    const nextPage = () => {
        if (currentPage < totalPages - 1) {
            setCurrentPage(currentPage + 1);
        }
    };

    const prevPage = () => {
        if (currentPage > 0) {
            setCurrentPage(currentPage - 1);
        }
    };

    if (loading) {
        return (
            <div className={`bg-white rounded-2xl shadow-sm border border-[#f3f4f6] p-4 ${className}`}>
                <div className="text-center mb-6">
                    <div className="inline-flex items-center gap-2 bg-[#fef2f2] px-4 py-1.5 rounded-full mb-3">
                        <span className="relative flex h-2 w-2">
                            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                            <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
                        </span>
                        <span className="text-[#dc2626] text-sm font-semibold">پخش زنده</span>
                    </div>
                    <h3 className="text-xl font-bold text-[#111827]">{title}</h3>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {[1, 2, 3].map((_, idx) => (
                        <div key={idx} className="aspect-[4/5] w-full bg-gray-100 rounded-xl animate-pulse"></div>
                    ))}
                </div>
            </div>
        );
    }

    // if (validRooms.length === 0) {
    //     return (
    //         <div className={`bg-white h-full rounded-2xl shadow-sm border border-[#f3f4f6] p-6 ${className}`}>
    //             <div className="flex flex-col items-center justify-center h-full min-h-[400px]">
    //                 <div className="inline-flex items-center gap-2 bg-[#fef2f2] px-4 py-1.5 rounded-full mb-4">
    //                     <span className="relative flex h-2 w-2">
    //                         <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
    //                         <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
    //                     </span>
    //                     <span className="text-[#dc2626] text-sm font-semibold">پخش زنده</span>
    //                 </div>
    //                 <div className="w-16 h-16 bg-[#f3f4f6] rounded-full flex items-center justify-center mx-auto mb-3">
    //                     <svg className="w-8 h-8 text-[#9ca3af]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    //                         <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
    //                     </svg>
    //                 </div>
    //                 <p className="text-[#6b7280] font-medium text-center">پخش زنده‌ای در دسترس نیست</p>
    //                 <p className="text-[#9ca3af] text-sm mt-1 text-center">لطفاً بعداً مجدداً تلاش کنید</p>
    //             </div>
    //         </div>
    //     );
    // }

    // حالت موبایل: نمایش عمودی همه کارت‌ها با w-full
    if (isMobile) {
        return (
            <div className={`bg-white rounded-2xl h-full shadow-sm border border-[#f3f4f6] pt-4 ${className}`}>
                {/* هدر */}
                <div className="text-center mb-5">
                    <div className="inline-flex items-center gap-2 bg-[#fef2f2] px-3 py-1 rounded-full mb-2">
                        <span className="relative flex h-2 w-2">
                            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                            <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
                        </span>
                        <span className="text-[#dc2626] text-xs font-semibold">{!isLive ? "برنامه های آتی" : "پخش زنده"}</span>
                    </div>
                    <h3 className="text-lg font-bold text-[#111827]">{!isLive ? "برنامه های آتی" : "پخش زنده"}</h3>
                    <p className="text-[#6b7280] text-xs mt-1">{!isLive ? "" : subtitle} </p>
                </div>

                {/* نمایش عمودی همه کارت‌ها */}
                <div className="space-y-4 px-4">
                    {isLive ? (
                        validRooms.map((item) => (
                            <RoomCardLive
                                key={item.roomID}
                                i={item}
                                isRoomLive={true}
                                live={item.live}
                            />
                        ))
                    ) : (
                        (() => {
                            const roomsWithNextPlan = validRooms.filter(item => item.nextPlan != null);
                            const roomsToDisplay = roomsWithNextPlan.length > 0
                                ? roomsWithNextPlan
                                : [validRooms[0]];
                            return roomsToDisplay.map((item) => (
                                <RoomCard
                                    key={item.roomID}
                                    i={item}
                                    isRoomLive={false}
                                    live={item.live}
                                />
                            ));
                        })()
                    )}
                </div>
            </div>
        );
    }

    // حالت دسکتاپ: اسلایدر افقی
    return (
        <div className={`bg-white rounded-2xl h-full shadow-sm border border-[#f3f4f6] pt-4 ${className}`}>
            {/* هدر */}
            <div className="text-center mb-5">
                <div className="inline-flex items-center gap-2 bg-[#fef2f2] px-3 py-1 rounded-full mb-2">
                    <span className="relative flex h-2 w-2">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-[#f87171] opacity-75"></span>
                        <span className="relative inline-flex rounded-full h-2 w-2 bg-[#ef4444]"></span>
                    </span>
                    <span className="text-[#dc2626] text-xs font-semibold">{!isLive ? "برنامه های آتی" : "پخش زنده"}</span>
                </div>
                <h3 className="text-lg font-bold text-[#111827]">{!isLive ? "برنامه های آتی" : "پخش زنده"}</h3>
                <p className="text-[#6b7280] text-xs mt-1">{!isLive ? "" : subtitle}</p>
            </div>

            {/* اسلایدر افقی */}
            <div className="relative overflow-hidden px-4 py-2 h-8/12">
                <div
                    className="flex transition-transform duration-300 ease-out gap-4 h-full"
                    style={{ transform: `translateX(-${currentPage * 100}%)` }}
                >
                    {Array.from({ length: totalPages }).map((_, pageIdx) => (
                        <div key={pageIdx} className="flex gap-4 flex-shrink-0 w-full h-full">
                            {!isLive ? (
                                validRooms.slice(pageIdx * itemsPerPage, (pageIdx + 1) * itemsPerPage)
                                    .map((i) => (
                                        <div className="flex-1 min-w-0 max-w-100 min-h-0" key={i.roomID}>
                                            {(() => {
                                                let nextSessionInfo = null;
                                                let n = false;
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
                                                            n = true;
                                                            nextSessionInfo = null;
                                                        } else if (daysDifference > 30) {
                                                            n = true; nextSessionInfo = null;
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
                                                return (
                                                    <RoomCard
                                                        i={i}
                                                        isRoomLive={false}
                                                        live={null}
                                                        joinButton={true}
                                                        copyButton={n}
                                                    />
                                                );
                                            })()}
                                        </div>
                                    ))) : (
                                validRooms.slice(pageIdx * itemsPerPage, (pageIdx + 1) * itemsPerPage)
                                    .map((item) => (
                                        <div className="flex-1 min-w-0 max-w-70 min-h-0 " key={item.roomID}>

                                            <RoomCardLive
                                                i={item}
                                                isRoomLive={true}
                                                live={item.live}
                                            />
                                        </div>
                                    ))
                            )}

                        </div>
                    ))}
                </div>

                {/* دکمه قبلی */}
                {totalPages > 1 && currentPage > 0 && (
                    <button
                        onClick={prevPage}
                        className="absolute -left-2 top-1/2 
            -translate-y-1/2 z-10 bg-[#0B3F32] text-white w-8 h-8 rounded-full
             shadow-lg hover:bg-[#0a352a] transition-all flex items-center justify-center"
                    >
                        <svg className="w-5 h-5 rotate-180" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                    </button>
                )}

                {/* دکمه بعدی */}
                {totalPages > 1 && currentPage < totalPages - 1 && (
                    <button
                        onClick={nextPage}
                        className="absolute -right-2 top-1/2 -translate-y-1/2 z-10 bg-[#0B3F32] text-white w-8 h-8 rounded-full shadow-lg hover:bg-[#0a352a] transition-all flex items-center justify-center"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                    </button>
                )}
            </div>
            {/* صفحه‌نما */}
            {totalPages > 1 && (
                <div className="flex justify-center gap-1.5 mt-7">
                    {Array.from({ length: totalPages }).map((_, idx) => (
                        <button
                            key={idx}
                            onClick={() => setCurrentPage(idx)}
                            className={`w-2 h-2 rounded-full transition-all ${currentPage === idx
                                ? "bg-[#ef4444] w-4"
                                : "bg-[#e5e7eb] hover:bg-[#d1d5db]"
                                }`}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}