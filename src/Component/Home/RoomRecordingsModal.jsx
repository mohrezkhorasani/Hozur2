import React, { useState, useEffect, useMemo } from 'react';
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
                        playbackUrl: m.recordingPlaybackUrl || m.videoUrl,
                        videoUrl: m.videoUrl,
                        voiceUrl: m.voiceUrl,
                        isRecorded: hasRecord,
                        participantsCount: m.participantsCount || m.attendeesCount || 0
                    };
                });

                // Sort: items with recordings first, then by date descending
                formatted.sort((a, b) => (b.isRecorded ? 1 : 0) - (a.isRecorded ? 1 : 0));
                setMeetings(formatted);
            } catch (fallbackErr) {
                console.error("Error fetching room meetings for recordings modal:", fallbackErr);
                if (!isCancelled) {
                    setError("خطا در برقراری ارتباط با سرور و دریافت لیست جلسات اتاق");
                }
            } finally {
                if (!isCancelled) {
                    setLoading(false);
                }
            }
        };

        fetchRecordingsData();

        return () => {
            isCancelled = true;
        };
    }, [isOpen, room, targetFid, targetNumId]);

    // Handle fetching BBB live recordings for a specific meeting
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
            console.error(`Failed to fetch BBB recordings for meeting #${meetingId}:`, err);
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

    return (
        <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-3 sm:p-4">
            <div
                className="bg-[#ffffff] rounded-3xl max-w-3xl w-full max-h-[90vh] flex flex-col shadow-2xl border border-[#f3f4f6] relative animate-fadeIn text-right"
                dir="rtl"
            >
                {/* Header */}
                <div className="flex items-center justify-between p-5 border-b border-[#f3f4f6] bg-[#f9fafb] rounded-t-3xl">
                    <div className="flex items-center gap-3">
                        <span className="p-2.5 bg-[#fef2f2] text-[#ef4444] rounded-2xl border border-[#fca5a5]/40 shadow-xs">
                            <FaVideo size={20} />
                        </span>
                        <div>
                            <h3 className="text-base sm:text-lg font-black text-[#1f2937]">
                                جلسات ضبط‌شده در بیگ‌بلو‌باتن
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
                            <div className="w-16 h-16 mx-auto mb-3 rounded-2xl bg-[#fef2f2] text-[#ef4444] flex items-center justify-center text-2xl shadow-inner">
                                <FaVideo />
                            </div>
                            <h4 className="text-sm font-black text-[#1f2937] mb-1">جلسه ضبط‌شده‌ای یافت نشد</h4>
                            <p className="text-xs text-[#6b7280] max-w-sm mx-auto">
                                هنوز جلسه‌ای برای این اتاق در بیگ‌بلو‌باتن ضبط نشده است یا ضبط‌ها در حال پردازش در سرور می‌باشند.
                            </p>
                        </div>
                    ) : (
                        filteredMeetings.map((item) => {
                            const bbbRecs = expandedRecordings[item.id];
                            const isExpanded = Array.isArray(bbbRecs);
                            const isLoadingThis = !!loadingBbbRecs[item.id];

                            return (
                                <div
                                    key={item.id}
                                    className="p-4 rounded-2xl border border-[#e5e7eb] bg-[#ffffff] hover:border-[#fdba74] hover:shadow-md transition-all space-y-3"
                                >
                                    {/* Meeting Title & Status */}
                                    <div className="flex flex-wrap items-start justify-between gap-2">
                                        <div className="flex items-center gap-2">
                                            <span className="w-2.5 h-2.5 rounded-full bg-[#ef4444] animate-pulse shrink-0"></span>
                                            <h4 className="text-sm font-black text-[#111827]">
                                                {item.title}
                                            </h4>
                                        </div>
                                        {item.isRecorded && (
                                            <span className="px-2.5 py-0.5 bg-[#fef2f2] text-[#b91c1c] border border-[#fca5a5] rounded-full text-[11px] font-bold flex items-center gap-1 shadow-2xs">
                                                <FaCheckCircle size={10} className="text-[#ef4444]" />
                                                <span>دارای ضبط</span>
                                            </span>
                                        )}
                                    </div>

                                    {/* Metadata Grid */}
                                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-2.5 bg-[#f9fafb] p-3 rounded-xl border border-[#f3f4f6] text-xs">
                                        {/* تاریخ شمسی */}
                                        <div className="flex items-center gap-1.5 text-[#374151]">
                                            <FaCalendarAlt className="text-[#d97706] shrink-0" size={13} />
                                            <span className="text-[#6b7280]">تاریخ:</span>
                                            <strong className="text-[#111827]">{item.jalaliDate}</strong>
                                        </div>

                                        {/* ساعت شروع و پایان */}
                                        <div className="flex items-center gap-1.5 text-[#374151]">
                                            <FaClock className="text-[#0284c7] shrink-0" size={13} />
                                            <span className="text-[#6b7280]">زمان:</span>
                                            <span className="font-bold text-[#111827]">
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

                                        {/* BBB Live Query Button */}
                                        <button
                                            type="button"
                                            disabled={isLoadingThis}
                                            onClick={() => handleFetchBbbRecordings(item.id)}
                                            className="px-3 py-1.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#4b5563] font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
                                            title="استعلام آخرین فایل‌های ضبط‌شده از سرور بیگ‌بلو‌باتن"
                                        >
                                            {isLoadingThis ? <FaSpinner className="animate-spin text-[#ef4444]" /> : <FaSync size={11} />}
                                            <span>{isExpanded ? 'بروزرسانی رکوردهای BBB' : 'دریافت رکوردهای سرور BBB'}</span>
                                        </button>
                                    </div>

                                    {/* BBB Nested Recordings Details */}
                                    {isExpanded && (
                                        <div className="mt-3 p-3 bg-[#f8fafc] rounded-xl border border-[#e2e8f0] text-xs space-y-2">
                                            <p className="font-bold text-[#334155]">رکوردهای پردازش‌شده بیگ‌بلو‌باتن:</p>
                                            {bbbRecs.length === 0 ? (
                                                <p className="text-[#94a3b8] text-[11px]">رکوردی روی سرور BBB برای این نشست یافت نشد یا هنوز پردازش آن به پایان نرسیده است.</p>
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
}
