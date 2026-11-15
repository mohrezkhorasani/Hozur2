import React, { useState, useEffect, useMemo } from 'react';
import { createPortal } from 'react-dom';
import {
    FaTimes,
    FaVideo,
    FaCalendarAlt,
    FaClock,
    FaChalkboardTeacher,
    FaExternalLinkAlt,
    FaPlay,
    FaSpinner,
    FaSearch,
    FaCheckCircle,
    FaVolumeUp,
    FaSync
} from 'react-icons/fa';
import { GetAdminMeetings, GetMeetingRecordings, GetRoomRecordings } from '../../api/AdminAPI';

export default function RoomRecordingsModal({ isOpen, onClose, room }) {
    const [meetings, setMeetings] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [expandedRecordings, setExpandedRecordings] = useState({});
    const [loadingBbbRecs, setLoadingBbbRecs] = useState({});

    const targetFid = room?.friendlyID || room?.friendlyId || room?.roomID || room?.RoomID || '';
    const targetNumId = room?.id || room?.ID || '';

    // Lock body scroll and handle Escape key
    useEffect(() => {
        if (!isOpen) return;
        const originalOverflow = document.body.style.overflow;
        document.body.style.overflow = 'hidden';

        const handleKeyDown = (e) => {
            if (e.key === 'Escape') onClose();
        };
        window.addEventListener('keydown', handleKeyDown);

        return () => {
            document.body.style.overflow = originalOverflow;
            window.removeEventListener('keydown', handleKeyDown);
        };
    }, [isOpen, onClose]);

    useEffect(() => {
        if (!isOpen || !room) return;

        let isCancelled = false;

        const fetchRecordingsData = async () => {
            setLoading(true);
            setError(null);
            setMeetings([]);
            setExpandedRecordings({});

            // 1. Try dedicated room recordings endpoint
            try {
                if (targetFid) {
                    const data = await GetRoomRecordings(targetFid);
                    if (!isCancelled && Array.isArray(data) && data.length > 0) {
                        setMeetings(data);
                        setLoading(false);
                        return;
                    }
                }
            } catch (err) {
                console.warn("GetRoomRecordings endpoint returned error/404, using meetings fallback:", err);
            }

            // 2. Fallback: Query GetAdminMeetings and filter for this room
            try {
                const res = await GetAdminMeetings(1, 100, "", null, null, null, { roomFriendlyId: targetFid });
                if (isCancelled) return;

                const items = res?.items || (Array.isArray(res?.data) ? res.data : (Array.isArray(res) ? res : []));
                const targetStr = String(targetFid).toLowerCase().trim();
                const targetNumStr = String(targetNumId).trim();

                const filtered = items.filter(m => {
                    if (m.type === -99) return false;
                    const mFid = String(m.roomFriendlyID || m.roomFriendlyId || m.friendlyID || m.friendlyId || '').toLowerCase().trim();
                    const mNumId = String(m.roomId || m.RoomId || m.roomID || '').trim();
                    if (targetStr && mFid && mFid === targetStr) return true;
                    if (targetNumStr && mNumId && mNumId === targetNumStr) return true;
                    return false;
                });

                // Format meeting items
                const formatted = filtered.map(m => {
                    const start = m.dateInsert ? new Date(m.dateInsert) : null;
                    const end = m.endDate ? new Date(m.endDate) : null;
                    const startTimeStr = start && !isNaN(start.getTime())
                        ? start.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false })
                        : '-';
                    const endTimeStr = end && !isNaN(end.getTime())
                        ? end.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false })
                        : (m.durationMinutes && start ? new Date(start.getTime() + m.durationMinutes * 60000).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false }) : '-');

                    const teacherName = m.teacher?.name || m.teacher?.fullname || m.teacherFullname ||
                        (Array.isArray(m.teachers) ? m.teachers.map(t => (typeof t === 'string' ? t : t.name)).filter(Boolean).join('، ') : '') ||
                        'تعیین نشده';

                    const hasRecord = m.isRecorded || !!m.recordingPlaybackUrl || !!m.videoUrl || !!m.voiceUrl;

                    return {
                        id: m.id || m.ID,
                        meetId: m.meetID || m.MeetID,
                        title: m.title || m.Title || 'نشست بدون عنوان',
                        jalaliDate: m.jalaliDate || m.JalaliDate || (start ? start.toLocaleDateString('fa-IR') : '-'),
                        startTime: startTimeStr,
                        endTime: endTimeStr,
                        teacherName: teacherName,
                        durationMinutes: m.durationMinutes || (start && end ? Math.max(0, Math.round((end.getTime() - start.getTime()) / 60000)) : 0),
                        playbackUrl: m.recordingPlaybackUrl || m.videoUrl || (m.playbackUrl || null),
                        voiceUrl: m.voiceUrl || null,
                        isRecorded: hasRecord,
                        participantsCount: m.participantsCount || m.participantCount || 0
                    };
                });

                setMeetings(formatted);
            } catch (err) {
                console.error("Failed to load recordings/meetings for room:", err);
                if (!isCancelled) {
                    setError("خطا در دریافت لیست جلسات اتاق. لطفاً مجدداً تلاش کنید.");
                }
            } finally {
                if (!isCancelled) setLoading(false);
            }
        };

        fetchRecordingsData();

        return () => {
            isCancelled = true;
        };
    }, [isOpen, room, targetFid, targetNumId]);

    // Handle fetching live recordings for a specific meeting
    const handleFetchBbbRecordings = async (meetingId) => {
        if (!meetingId) return;
        setLoadingBbbRecs(prev => ({ ...prev, [meetingId]: true }));
        try {
            const res = await GetMeetingRecordings(meetingId);
            const recList = res?.data?.recordings || res?.recordings || [];
            setExpandedRecordings(prev => ({
                ...prev,
                [meetingId]: recList
            }));
        } catch (err) {
            console.error(`Failed to fetch recordings for meeting #${meetingId}:`, err);
            setExpandedRecordings(prev => ({
                ...prev,
                [meetingId]: []
            }));
        } finally {
            setLoadingBbbRecs(prev => ({ ...prev, [meetingId]: false }));
        }
    };

    // Filter by search term
    const filteredMeetings = useMemo(() => {
        if (!searchTerm.trim()) return meetings;
        const term = searchTerm.trim().toLowerCase();
        return meetings.filter(m =>
            (m.title && m.title.toLowerCase().includes(term)) ||
            (m.teacherName && m.teacherName.toLowerCase().includes(term)) ||
            (m.jalaliDate && m.jalaliDate.includes(term))
        );
    }, [meetings, searchTerm]);

    if (!isOpen) return null;

    const modalContent = (
        <div
            className="fixed inset-0 z-[9999] bg-black/65 backdrop-blur-sm flex items-center justify-center p-3 sm:p-6 overflow-y-auto"
            onClick={(e) => {
                if (e.target === e.currentTarget) onClose();
            }}
        >
            <div
                className="bg-[#ffffff] rounded-3xl max-w-4xl lg:max-w-5xl w-full max-h-[88vh] flex flex-col shadow-2xl border border-[#e5e7eb] relative text-right my-auto overflow-hidden animate-in fade-in zoom-in-95 duration-200"
                dir="rtl"
                onClick={(e) => e.stopPropagation()}
            >
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b border-[#f3f4f6] bg-[#f9fafb] rounded-t-3xl">
                    <div className="flex items-center gap-3">
                        <span className="p-2.5 bg-[#fef2f2] text-[#ef4444] rounded-2xl border border-[#fca5a5]/40 shadow-xs">
                            <FaVideo size={20} />
                        </span>
                        <div>
                            <h3 className="text-base sm:text-lg font-black text-[#1f2937]">
                                جلسات ضبط شده در سامانه پخش مجازی
                            </h3>
                            <p className="text-xs text-[#6b7280] mt-0.5 font-medium">
                                اتاق: <strong className="text-[#111827]">{room?.title || targetFid}</strong>
                            </p>
                        </div>
                    </div>
                    <button
                        type="button"
                        onClick={onClose}
                        className="p-2 text-[#9ca3af] hover:text-[#ef4444] hover:bg-[#fef2f2] rounded-xl transition cursor-pointer"
                        title="بستن پنجره"
                    >
                        <FaTimes size={18} />
                    </button>
                </div>

                {/* Search Bar */}
                <div className="p-4 border-b border-[#f3f4f6] bg-[#ffffff]">
                    <div className="relative">
                        <input
                            type="text"
                            placeholder="جستجو در عنوان جلسه، نام استاد یا تاریخ..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pr-10 pl-4 py-2.5 bg-[#f9fafb] border border-[#e5e7eb] rounded-2xl text-xs font-medium focus:outline-none focus:ring-2 focus:ring-[#ef4444] focus:bg-[#ffffff] transition"
                        />
                        <FaSearch className="absolute right-3.5 top-3.5 text-[#9ca3af] text-xs" />
                    </div>
                </div>

                {/* Content Body */}
                <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-4">
                    {loading ? (
                        <div className="py-16 text-center">
                            <FaSpinner className="animate-spin text-[#ef4444] mx-auto text-3xl mb-3" />
                            <p className="text-xs font-bold text-[#4b5563]">در حال بارگذاری لیست ضبط‌های اتاق...</p>
                        </div>
                    ) : error ? (
                        <div className="p-4 bg-[#fef2f2] border border-[#fca5a5] rounded-2xl text-[#b91c1c] text-xs font-bold text-center">
                            {error}
                        </div>
                    ) : filteredMeetings.length === 0 ? (
                        <div className="py-16 text-center">
                            <div className="w-16 h-16 mx-auto mb-3 bg-[#fef2f2] text-[#ef4444] rounded-full flex items-center justify-center">
                                <FaVideo size={28} />
                            </div>
                            <h4 className="text-sm font-bold text-[#1f2937]">هیچ جلسه ضبط‌شده‌ای یافت نشد</h4>
                            <p className="text-xs text-[#6b7280] mt-1 max-w-sm mx-auto">
                                در صورتی که جلسه‌ای ضبط شده باشد، پس از پردازش نهایی در این بخش قابل مشاهده و بازپخش خواهد بود.
                            </p>
                        </div>
                    ) : (
                        filteredMeetings.map((item) => {
                            const bbbRecs = expandedRecordings[item.id] || [];
                            const isExpanded = expandedRecordings[item.id] !== undefined;
                            const isLoadingThis = loadingBbbRecs[item.id] || false;

                            return (
                                <div
                                    key={item.id}
                                    className="p-4 rounded-2xl border border-[#e5e7eb] bg-[#ffffff] hover:border-[#fca5a5] hover:shadow-sm transition space-y-3"
                                >
                                    {/* Top Row: Title & Badge */}
                                    <div className="flex flex-wrap items-center justify-between gap-2">
                                        <div className="flex items-center gap-2">
                                            <span className="w-2.5 h-2.5 rounded-full bg-[#ef4444]" />
                                            <h4 className="font-bold text-sm text-[#111827]">
                                                {item.title}
                                            </h4>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            {item.isRecorded && (
                                                <span className="inline-flex items-center gap-1 text-[11px] font-bold px-2 py-0.5 rounded-full bg-[#fef2f2] text-[#dc2626] border border-[#fca5a5]">
                                                    <FaCheckCircle size={10} />
                                                    <span>دارای ضبط</span>
                                                </span>
                                            )}
                                            <span className="text-[11px] text-[#9ca3af] font-mono">
                                                #{item.id}
                                            </span>
                                        </div>
                                    </div>

                                    {/* Middle Row: Date, Time, Teacher */}
                                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-2 text-xs text-[#4b5563] bg-[#f9fafb] p-3 rounded-xl border border-[#f3f4f6]">
                                        {/* تاریخ شمسی */}
                                        <div className="flex items-center gap-1.5 text-[#374151]">
                                            <FaCalendarAlt className="text-[#0284c7] shrink-0" size={13} />
                                            <span className="text-[#6b7280]">تاریخ:</span>
                                            <strong className="text-[#111827]">{item.jalaliDate}</strong>
                                        </div>

                                        {/* ساعت شروع و پایان */}
                                        <div className="flex items-center gap-1.5 text-[#374151]">
                                            <FaClock className="text-[#059669] shrink-0" size={13} />
                                            <span className="text-[#6b7280]">زمان:</span>
                                            <span className="font-mono text-[#111827]">
                                                {item.startTime !== '-' && item.endTime !== '-'
                                                    ? `از ${item.startTime} تا ${item.endTime}`
                                                    : item.startTime !== '-' ? `ساعت ${item.startTime}` : '-'}
                                            </span>
                                            {item.durationMinutes > 0 && (
                                                <span className="text-[10px] text-[#0284c7] bg-[#e0f2fe] px-1.5 py-0.5 rounded font-mono mr-1">
                                                    {item.durationMinutes} د
                                                </span>
                                            )}
                                        </div>

                                        {/* نام اساتید */}
                                        <div className="flex items-center gap-1.5 text-[#374151] truncate">
                                            <FaChalkboardTeacher className="text-[#9333ea] shrink-0" size={14} />
                                            <span className="text-[#6b7280]">استاد:</span>
                                            <strong className="text-[#111827] truncate">{item.teacherName}</strong>
                                        </div>
                                    </div>

                                    {/* Actions & Playback Links */}
                                    <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-[#f3f4f6]">
                                        <div className="flex flex-wrap items-center gap-2">
                                            {item.playbackUrl && (
                                                <a
                                                    href={item.playbackUrl}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="px-3.5 py-1.5 bg-[#ef4444] hover:bg-[#dc2626] text-[#ffffff] font-bold text-xs rounded-xl shadow-xs transition flex items-center gap-1.5"
                                                >
                                                    <FaPlay size={10} />
                                                    <span>مشاهده ضبط جلسه</span>
                                                    <FaExternalLinkAlt size={9} />
                                                </a>
                                            )}
                                            {item.voiceUrl && (
                                                <a
                                                    href={item.voiceUrl}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="px-3 py-1.5 bg-[#f0f9ff] hover:bg-[#e0f2fe] text-[#0369a1] border border-[#bae6fd] font-bold text-xs rounded-xl transition flex items-center gap-1.5"
                                                >
                                                    <FaVolumeUp size={11} />
                                                    <span>صوت جلسه</span>
                                                </a>
                                            )}
                                        </div>

                                        {/* Live Query Button */}
                                        <button
                                            type="button"
                                            disabled={isLoadingThis}
                                            onClick={() => handleFetchBbbRecordings(item.id)}
                                            className="px-3 py-1.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#4b5563] font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
                                            title="استعلام آخرین فایل‌های ضبط‌شده از سرور"
                                        >
                                            {isLoadingThis ? <FaSpinner className="animate-spin text-[#ef4444]" /> : <FaSync size={11} />}
                                            <span>{isExpanded ? 'بروزرسانی فایل‌های ضبط' : 'استعلام فایل‌های ضبط سرور'}</span>
                                        </button>
                                    </div>

                                    {/* Nested Recordings Details */}
                                    {isExpanded && (
                                        <div className="mt-3 p-3 bg-[#f8fafc] rounded-xl border border-[#e2e8f0] text-xs space-y-2">
                                            <p className="font-bold text-[#334155]">فایل‌های ضبط‌شده در سرور سامانه:</p>
                                            {bbbRecs.length === 0 ? (
                                                <p className="text-[#94a3b8] text-[11px]">فایل ضبط‌شده‌ای روی سرور برای این نشست یافت نشد یا هنوز پردازش آن به پایان نرسیده است.</p>
                                            ) : (
                                                bbbRecs.map((r, rIdx) => (
                                                    <div key={r.recordId || rIdx} className="flex flex-wrap items-center justify-between gap-2 p-2 bg-[#ffffff] rounded-lg border border-[#e2e8f0]">
                                                        <div>
                                                            <strong className="text-[#0f172a] block">{r.name || `رکورد #${rIdx + 1}`}</strong>
                                                            <span className="text-[10px] text-[#64748b]">
                                                                مدت: {r.lengthMinutes || '-'} دقیقه | حجم: {r.size ? `${Math.round(r.size / (1024 * 1024))} مگابایت` : '-'}
                                                            </span>
                                                        </div>
                                                        {r.playbackUrl && (
                                                            <a
                                                                href={r.playbackUrl}
                                                                target="_blank"
                                                                rel="noopener noreferrer"
                                                                className="px-3 py-1 bg-[#10b981] hover:bg-[#059669] text-[#ffffff] font-bold rounded-lg transition flex items-center gap-1"
                                                            >
                                                                <FaPlay size={9} />
                                                                <span>پخش</span>
                                                            </a>
                                                        )}
                                                    </div>
                                                ))
                                            )}
                                        </div>
                                    )}
                                </div>
                            );
                        })
                    )}
                </div>

                {/* Footer */}
                <div className="p-4 border-t border-[#f3f4f6] bg-[#f9fafb] rounded-b-3xl flex items-center justify-between">
                    <span className="text-xs text-[#6b7280]">
                        تعداد نشست‌های شناسایی‌شده: <strong className="text-[#111827]">{filteredMeetings.length}</strong>
                    </span>
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-5 py-2 bg-[#1f2937] hover:bg-[#111827] text-[#ffffff] font-bold text-xs rounded-xl transition cursor-pointer"
                    >
                        بستن
                    </button>
                </div>
            </div>
        </div>
    );

    return typeof document !== 'undefined' ? createPortal(modalContent, document.body) : modalContent;
}
