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
    FaSync,
    FaSortAmountDown,
    FaSortAmountUp,
    FaCopy
} from 'react-icons/fa';
import { GetAdminMeetings, GetMeetingRecordings, GetRoomRecordings } from '../../api/AdminAPI';

export function getMeetingTimestamp(item) {
    if (!item) return 0;

    // 1. If explicit rawDate number exists
    if (typeof item.rawDate === 'number' && !isNaN(item.rawDate) && item.rawDate > 0) {
        return item.rawDate;
    }

    // 2. Date fields: dateInsert, startDate, startTime, createdAt
    const dateFields = [item.dateInsert, item.DateInsert, item.startDate, item.startTime, item.createdAt, item.date];
    for (let val of dateFields) {
        if (val) {
            if (typeof val === 'number') return val;
            if (!isNaN(Number(val)) && Number(val) > 1000000000) return Number(val);
            const parsed = new Date(val);
            if (!isNaN(parsed.getTime())) return parsed.getTime();
        }
    }

    // 3. Jalali date parsing: e.g. "1403/08/24", "1404-09-01", etc.
    const jalaliStr = item.jalaliDate || item.JalaliDate;
    if (jalaliStr && typeof jalaliStr === 'string') {
        const enStr = jalaliStr.replace(/[۰-۹]/g, d => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d))
                               .replace(/[٠-٩]/g, d => '٠١٢٣٤٥٦٧٨٩'.indexOf(d));
        const parts = enStr.match(/(\d{4})[/-](\d{1,2})[/-](\d{1,2})/);
        if (parts) {
            const year = parseInt(parts[1], 10);
            const month = parseInt(parts[2], 10);
            const day = parseInt(parts[3], 10);

            let hour = 0;
            let minute = 0;
            const timeStr = item.startTime || item.StartTime;
            if (timeStr && typeof timeStr === 'string') {
                const enTime = timeStr.replace(/[۰-۹]/g, d => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d))
                                      .replace(/[٠-٩]/g, d => '٠١٢٣٤٥٦٧٨٩'.indexOf(d));
                const timeParts = enTime.match(/(\d{1,2}):(\d{1,2})/);
                if (timeParts) {
                    hour = parseInt(timeParts[1], 10);
                    minute = parseInt(timeParts[2], 10);
                }
            }

            const approxGregorianYear = year + 621;
            let jalaliDays = 0;
            if (month <= 6) {
                jalaliDays = (month - 1) * 31 + day;
            } else {
                jalaliDays = 6 * 31 + (month - 7) * 30 + day;
            }
            const baseTime = new Date(Date.UTC(approxGregorianYear, 2, 21, hour, minute, 0)).getTime();
            return baseTime + (jalaliDays - 1) * 86400000;
        }
    }

    // 4. Fallback to numeric ID if available
    const numId = parseInt(item.id, 10);
    if (!isNaN(numId) && numId > 0) return numId;

    return 0;
}

export default function RoomRecordingsModal({ isOpen, onClose, room }) {
    const [meetings, setMeetings] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortOrder, setSortOrder] = useState('desc'); // 'desc' = جدیدترین به قدیمی‌ترین, 'asc' = قدیمی‌ترین به جدیدترین
    const [expandedRecordings, setExpandedRecordings] = useState({});
    const [loadingBbbRecs, setLoadingBbbRecs] = useState({});
    const [copiedId, setCopiedId] = useState(null);

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
                    if (!isCancelled && Array.isArray(data)) {
                        const recordedOnly = data.filter(m => m.isRecorded || m.hasRecording || !!m.recordingPlaybackUrl || !!m.videoUrl || !!m.playbackUrl || !!(m.meetID && String(m.meetID).trim().length > 10));
                        recordedOnly.forEach(item => {
                            const meetId = String(item.meetID || item.meetId || item.MeetID || '').trim();
                            item.meetId = meetId;
                            if (!item.playbackUrl) {
                                item.playbackUrl = item.recordingPlaybackUrl || item.videoUrl || (meetId && meetId.length > 10 ? `https://vc.hekmateislami.com/playback/presentation/2.3/${meetId}` : null);
                            }
                            if (!item.teacherName) {
                                item.teacherName = item.teacher?.name || item.teacher?.fullname || item.teacherFullname ||
                                    (Array.isArray(item.teachers) ? item.teachers.map(t => (typeof t === 'string' ? t : t.name)).filter(Boolean).join('، ') : '') ||
                                    'تعیین نشده';
                            }
                            if (!item.startTime || item.startTime === '-') {
                                const sDate = item.dateInsert ? new Date(item.dateInsert) : (item.startDate ? new Date(item.startDate) : null);
                                if (sDate && !isNaN(sDate.getTime())) {
                                    item.startTime = sDate.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false });
                                }
                            }
                            if (!item.endTime || item.endTime === '-') {
                                const eDate = item.endDate ? new Date(item.endDate) : null;
                                if (eDate && !isNaN(eDate.getTime())) {
                                    item.endTime = eDate.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false });
                                } else if (item.durationMinutes && item.dateInsert) {
                                    const sDate = new Date(item.dateInsert);
                                    if (!isNaN(sDate.getTime())) {
                                        const calculatedEnd = new Date(sDate.getTime() + item.durationMinutes * 60000);
                                        item.endTime = calculatedEnd.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false });
                                    }
                                }
                            }
                            if (!item.rawDate) {
                                item.rawDate = getMeetingTimestamp(item);
                            }
                        });
                        setMeetings(recordedOnly);
                        setLoading(false);
                        return;
                    }
                }
            } catch (err) {
                console.warn("GetRoomRecordings endpoint returned error/404, using meetings fallback:", err);
            }

            // 2. Fallback: Query GetAdminMeetings and match recordings via recordID == meetingID
            try {
                const res = await GetAdminMeetings(1, 100, "", null, null, null, { roomFriendlyId: targetFid });
                if (isCancelled) return;

                const items = res?.items || (Array.isArray(res?.data) ? res.data : (Array.isArray(res) ? res : []));
                const targetStr = String(targetFid).toLowerCase().trim();
                const targetNumStr = String(targetNumId).trim();

                const roomMeetings = items.filter(m => {
                    if (m.type === -99) return false;
                    const mFid = String(m.roomFriendlyID || m.roomFriendlyId || m.friendlyID || m.friendlyId || '').toLowerCase().trim();
                    const mNumId = String(m.roomId || m.RoomId || m.roomID || '').trim();
                    return (targetStr && mFid && mFid === targetStr) || (targetNumStr && mNumId && mNumId === targetNumStr);
                });

                // Try to fetch BBB recordings using any meeting of this room
                let bbbRecs = [];
                if (roomMeetings.length > 0) {
                    try {
                        const recRes = await GetMeetingRecordings(roomMeetings[0].id || roomMeetings[0].ID);
                        bbbRecs = recRes?.data?.recordings || recRes?.recordings || [];
                    } catch (e) {
                        console.warn("Could not fetch BBB recordings via GetMeetingRecordings:", e);
                    }
                }

                const recsByRecordId = new Map();
                if (Array.isArray(bbbRecs)) {
                    bbbRecs.forEach(r => {
                        if (r && r.recordId) {
                            recsByRecordId.set(String(r.recordId).trim(), r);
                        }
                    });
                }

                // Match: recordID in BBB is exactly MeetID in MeetingTBL!
                const matchedList = [];
                const matchedMeetingIds = new Set();

                // First, for every BBB recording, find the meeting in MeetingTBL
                if (recsByRecordId.size > 0) {
                    recsByRecordId.forEach((r, recordId) => {
                        const m = roomMeetings.find(item => {
                            const itemMeetId = String(item.meetID || item.MeetID || '').trim();
                            return itemMeetId && itemMeetId === recordId;
                        });

                        let startTimeVal = r.startTime;
                        if (startTimeVal && !isNaN(Number(startTimeVal))) {
                            startTimeVal = Number(startTimeVal);
                        }
                        const start = m?.dateInsert ? new Date(m.dateInsert) : (startTimeVal ? new Date(startTimeVal) : null);
                        const end = m?.endDate ? new Date(m.endDate) : (r.endTime ? new Date(!isNaN(Number(r.endTime)) ? Number(r.endTime) : r.endTime) : null);
                        const startTimeStr = start && !isNaN(start.getTime())
                            ? start.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false })
                            : '-';
                        const endTimeStr = end && !isNaN(end.getTime())
                            ? end.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false })
                            : (r.lengthMinutes && start ? new Date(start.getTime() + r.lengthMinutes * 60000).toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit', hour12: false }) : '-');

                        let teacherName = m?.teacher?.name || m?.teacher?.fullname || m?.teacherFullname ||
                            (Array.isArray(m?.teachers) ? m.teachers.map(t => (typeof t === 'string' ? t : t.name)).filter(Boolean).join('، ') : '');

                        if (!teacherName && r.metadata?.teachers) {
                            try {
                                const parsedTeachers = JSON.parse(r.metadata.teachers);
                                if (Array.isArray(parsedTeachers)) {
                                    teacherName = parsedTeachers.map(t => t.name).filter(Boolean).join('، ');
                                }
                            } catch {}
                        }

                        if (m) matchedMeetingIds.add(m.id || m.ID);

                        const rawTimestamp = start && !isNaN(start.getTime())
                            ? start.getTime()
                            : (m?.dateInsert ? new Date(m.dateInsert).getTime() : 0);

                        matchedList.push({
                            id: m ? (m.id || m.ID) : recordId,
                            meetId: recordId,
                            title: (m && m.title && m.title !== 'None') ? (m.title || m.Title) : (r.name || r.metadata?.meetingName || room?.title || 'نشست ضبط‌شده'),
                            rawDate: rawTimestamp,
                            dateInsert: m?.dateInsert || (start ? start.toISOString() : null),
                            jalaliDate: m?.jalaliDate || m?.JalaliDate || (start ? start.toLocaleDateString('fa-IR') : '-'),
                            startTime: startTimeStr,
                            endTime: endTimeStr,
                            teacherName: teacherName || 'تعیین نشده',
                            durationMinutes: r.lengthMinutes || (start && end ? Math.max(0, Math.round((end.getTime() - start.getTime()) / 60000)) : 0),
                            playbackUrl: r.playbackUrl || m?.recordingPlaybackUrl || m?.videoUrl || (recordId && recordId.length > 10 ? `https://vc.hekmateislami.com/playback/presentation/2.3/${recordId}` : null),
                            voiceUrl: m?.voiceUrl || null,
                            isRecorded: true,
                            participantsCount: r.participants || m?.participantsCount || m?.participantCount || 0
                        });
                    });
                }

                // Also include meetings that have isRecorded / videoUrl / playbackUrl directly
                roomMeetings.forEach(m => {
                    const mId = m.id || m.ID;
                    if (matchedMeetingIds.has(mId)) return;
                    const meetId = String(m.meetID || m.MeetID || '').trim();
                    const hasRecord = m.isRecorded || m.hasRecording || !!m.recordingPlaybackUrl || !!m.videoUrl || !!m.playbackUrl || (meetId && meetId.length > 10);
                    if (!hasRecord) return; // ONLY RECORDED SESSIONS!

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

                    const rawTimestamp = start && !isNaN(start.getTime()) ? start.getTime() : 0;
                    const derivedPlayback = m.recordingPlaybackUrl || m.videoUrl || m.playbackUrl || (meetId && meetId.length > 10 ? `https://vc.hekmateislami.com/playback/presentation/2.3/${meetId}` : null);

                    matchedList.push({
                        id: mId,
                        meetId: meetId,
                        title: m.title || m.Title || 'نشست بدون عنوان',
                        rawDate: rawTimestamp,
                        dateInsert: m.dateInsert || null,
                        jalaliDate: m.jalaliDate || m.JalaliDate || (start ? start.toLocaleDateString('fa-IR') : '-'),
                        startTime: startTimeStr,
                        endTime: endTimeStr,
                        teacherName: teacherName,
                        durationMinutes: m.durationMinutes || (start && end ? Math.max(0, Math.round((end.getTime() - start.getTime()) / 60000)) : 0),
                        playbackUrl: derivedPlayback,
                        voiceUrl: m.voiceUrl || null,
                        isRecorded: true,
                        participantsCount: m.participantsCount || m.participantCount || 0
                    });
                });

                setMeetings(matchedList);
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
    const handleFetchBbbRecordings = async (item) => {
        const meetingId = item?.id;
        if (!meetingId) return;

        // Toggle collapse if already open
        if (expandedRecordings[meetingId] !== undefined) {
            setExpandedRecordings(prev => {
                const next = { ...prev };
                delete next[meetingId];
                return next;
            });
            return;
        }

        const targetMeetId = String(item.meetId || item.meetID || item.MeetID || '').trim();

        setLoadingBbbRecs(prev => ({ ...prev, [meetingId]: true }));
        try {
            const res = await GetMeetingRecordings(meetingId);
            let recList = res?.data?.recordings || res?.recordings || (Array.isArray(res) ? res : []);

            // STRICT FILTER: فقط و فقط فایل هایی بیاره که ای دیش برابر meetid جلسه انتخابی باشه
            if (targetMeetId) {
                recList = recList.filter(r => {
                    const rId = String(r.recordId || r.recordID || r.RecordID || '').trim();
                    const mId = String(r.meetingId || r.meetingID || r.MeetingID || '').trim();
                    return rId === targetMeetId || mId === targetMeetId;
                });
            }

            // تناسب آی‌دی با لینک پخش در صورت خالی بودن
            if (recList.length === 0 && targetMeetId && targetMeetId.length > 10) {
                recList.push({
                    recordId: targetMeetId,
                    meetingId: targetMeetId,
                    name: item.title || 'ضبط جلسه در سامانه پخش مجازی',
                    playbackUrl: `https://vc.hekmateislami.com/playback/presentation/2.3/${targetMeetId}`,
                    playbackFormat: 'presentation',
                    published: true,
                    state: 'published'
                });
            }

            // اگر دکمه پخش هنوز ست نشده بود، آپدیت شود
            if (recList.length > 0 && recList[0].playbackUrl && !item.playbackUrl) {
                setMeetings(prev => prev.map(m => m.id === meetingId ? { ...m, playbackUrl: recList[0].playbackUrl } : m));
            }

            setExpandedRecordings(prev => ({
                ...prev,
                [meetingId]: recList
            }));
        } catch (err) {
            console.error(`Failed to fetch recordings for meeting #${meetingId}:`, err);
            if (targetMeetId && targetMeetId.length > 10) {
                setExpandedRecordings(prev => ({
                    ...prev,
                    [meetingId]: [{
                        recordId: targetMeetId,
                        meetingId: targetMeetId,
                        name: item.title || 'ضبط جلسه در سامانه پخش مجازی',
                        playbackUrl: `https://vc.hekmateislami.com/playback/presentation/2.3/${targetMeetId}`,
                        published: true
                    }]
                }));
            } else {
                setExpandedRecordings(prev => ({
                    ...prev,
                    [meetingId]: []
                }));
            }
        } finally {
            setLoadingBbbRecs(prev => ({ ...prev, [meetingId]: false }));
        }
    };

    // Filter by search term & Sort by date
    const sortedAndFilteredMeetings = useMemo(() => {
        let list = [...meetings];

        // 1. Filter by search term
        if (searchTerm.trim()) {
            const term = searchTerm.trim().toLowerCase();
            list = list.filter(m =>
                (m.title && m.title.toLowerCase().includes(term)) ||
                (m.teacherName && m.teacherName.toLowerCase().includes(term)) ||
                (m.jalaliDate && m.jalaliDate.includes(term))
            );
        }

        // 2. Sort by date
        list.sort((a, b) => {
            const timeA = getMeetingTimestamp(a);
            const timeB = getMeetingTimestamp(b);
            if (sortOrder === 'asc') {
                return timeA - timeB;
            }
            return timeB - timeA; // default: 'desc' (جدیدترین به قدیمی‌ترین)
        });

        return list;
    }, [meetings, searchTerm, sortOrder]);

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

                {/* Search Bar & Date Sort Toggle */}
                <div className="p-4 border-b border-[#f3f4f6] bg-[#ffffff] flex flex-wrap items-center gap-3">
                    <div className="relative flex-1 min-w-[220px]">
                        <input
                            type="text"
                            placeholder="جستجو در عنوان جلسه، نام استاد یا تاریخ..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pr-10 pl-4 py-2.5 bg-[#f9fafb] border border-[#e5e7eb] rounded-2xl text-xs font-medium focus:outline-none focus:ring-2 focus:ring-[#ef4444] focus:bg-[#ffffff] transition"
                        />
                        <FaSearch className="absolute right-3.5 top-3.5 text-[#9ca3af] text-xs" />
                    </div>

                    <button
                        type="button"
                        onClick={() => setSortOrder(prev => (prev === 'desc' ? 'asc' : 'desc'))}
                        className="px-3.5 py-2.5 bg-[#f9fafb] hover:bg-[#f3f4f6] active:bg-[#e5e7eb] border border-[#e5e7eb] rounded-2xl text-xs font-bold text-[#374151] flex items-center gap-2 transition shrink-0 cursor-pointer shadow-xs select-none"
                        title={sortOrder === 'desc' ? 'مرتب‌سازی: جدیدترین به قدیمی‌ترین' : 'مرتب‌سازی: قدیمی‌ترین به جدیدترین'}
                    >
                        {sortOrder === 'desc' ? (
                            <FaSortAmountDown className="text-[#ef4444]" size={14} />
                        ) : (
                            <FaSortAmountUp className="text-[#ef4444]" size={14} />
                        )}
                        <span className="text-[#6b7280]">سورت تاریخ:</span>
                        <span className="text-[#111827]">
                            {sortOrder === 'desc' ? 'جدیدترین اول' : 'قدیمی‌ترین اول'}
                        </span>
                        <span className="text-[10px] px-1.5 py-0.5 rounded-md bg-[#e5e7eb] text-[#4b5563] font-mono">
                            {sortOrder === 'desc' ? '▼ نزولی' : '▲ صعودی'}
                        </span>
                    </button>
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
                    ) : sortedAndFilteredMeetings.length === 0 ? (
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
                        sortedAndFilteredMeetings.map((item) => {
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
                                                    className="px-3.5 py-1.5 bg-[#ef4444] hover:bg-[#dc2626] text-[#ffffff] font-bold text-xs rounded-xl shadow-xs transition flex items-center gap-1.5 cursor-pointer"
                                                    title="مشاهده مستقیم ضبط جلسه"
                                                >
                                                    <FaPlay size={10} />
                                                    <span>پخش جلسه ضبط‌شده</span>
                                                    <FaExternalLinkAlt size={9} />
                                                </a>
                                            )}
                                            {item.playbackUrl && (
                                                <button
                                                    type="button"
                                                    onClick={() => {
                                                        navigator.clipboard?.writeText(item.playbackUrl);
                                                        setCopiedId(item.id);
                                                        setTimeout(() => setCopiedId(null), 2000);
                                                    }}
                                                    className="px-2.5 py-1.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer shadow-2xs"
                                                    title="کپی لینک بازپخش جهت اشتراک‌گذاری"
                                                >
                                                    {copiedId === item.id ? (
                                                        <>
                                                            <FaCheckCircle className="text-[#10b981]" size={11} />
                                                            <span className="text-[#10b981]">کپی شد</span>
                                                        </>
                                                    ) : (
                                                        <>
                                                            <FaCopy size={11} />
                                                            <span>کپی لینک</span>
                                                        </>
                                                    )}
                                                </button>
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
                                            onClick={() => handleFetchBbbRecordings(item)}
                                            className="px-3 py-1.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#4b5563] font-bold text-xs rounded-xl transition flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
                                            title="استعلام آخرین فایل‌های ضبط‌شده این نشست از سرور"
                                        >
                                            {isLoadingThis ? <FaSpinner className="animate-spin text-[#ef4444]" /> : <FaSync size={11} />}
                                            <span>{isExpanded ? 'بستن جزئیات استعلام' : 'استعلام فایل‌های ضبط سرور'}</span>
                                        </button>
                                    </div>

                                    {/* Nested Recordings Details */}
                                    {isExpanded && (
                                        <div className="mt-3 p-3 bg-[#f8fafc] rounded-xl border border-[#e2e8f0] text-xs space-y-2">
                                            <div className="flex items-center justify-between">
                                                <p className="font-bold text-[#334155]">فایل‌های ضبط‌شده استعلام‌شده از سرور:</p>
                                                {item.meetId && (
                                                    <span className="text-[10px] text-[#64748b] font-mono">
                                                        شناسه: {item.meetId}
                                                    </span>
                                                )}
                                            </div>
                                            {bbbRecs.length === 0 ? (
                                                <p className="text-[#94a3b8] text-[11px]">فایل ضبط‌شده‌ای روی سرور برای شناسه این نشست یافت نشد یا هنوز پردازش آن به پایان نرسیده است.</p>
                                            ) : (
                                                bbbRecs.map((r, rIdx) => (
                                                    <div key={r.recordId || rIdx} className="flex flex-wrap items-center justify-between gap-2 p-2 bg-[#ffffff] rounded-lg border border-[#e2e8f0]">
                                                        <div>
                                                            <strong className="text-[#0f172a] block">{r.name || `رکورد #${rIdx + 1}`}</strong>
                                                            <span className="text-[10px] text-[#64748b]">
                                                                مدت: {r.formattedDuration || (r.lengthMinutes ? `${r.lengthMinutes} دقیقه` : '-')} | حجم: {r.formattedSize || (r.size ? `${Math.round(r.size / (1024 * 1024))} مگابایت` : '-')}
                                                            </span>
                                                            {r.recordId && (
                                                                <span className="block text-[10px] text-[#94a3b8] font-mono">
                                                                    شناسه رکورد: {r.recordId}
                                                                </span>
                                                            )}
                                                        </div>
                                                        {r.playbackUrl && (
                                                            <div className="flex items-center gap-1.5">
                                                                <a
                                                                    href={r.playbackUrl}
                                                                    target="_blank"
                                                                    rel="noopener noreferrer"
                                                                    className="px-3 py-1 bg-[#10b981] hover:bg-[#059669] text-[#ffffff] font-bold rounded-lg transition flex items-center gap-1 text-xs"
                                                                >
                                                                    <FaPlay size={9} />
                                                                    <span>پخش</span>
                                                                </a>
                                                                <button
                                                                    type="button"
                                                                    onClick={() => {
                                                                        navigator.clipboard?.writeText(r.playbackUrl);
                                                                        setCopiedId(r.recordId || `rec-${rIdx}`);
                                                                        setTimeout(() => setCopiedId(null), 2000);
                                                                    }}
                                                                    className="px-2 py-1 bg-[#f1f5f9] hover:bg-[#e2e8f0] text-[#475569] font-bold rounded-lg transition flex items-center gap-1 text-[11px]"
                                                                    title="کپی لینک مستقیم"
                                                                >
                                                                    {copiedId === (r.recordId || `rec-${rIdx}`) ? (
                                                                        <FaCheckCircle className="text-[#10b981]" size={10} />
                                                                    ) : (
                                                                        <FaCopy size={10} />
                                                                    )}
                                                                </button>
                                                            </div>
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
                        <span>تعداد نشست‌های شناسایی‌شده: </span><strong className="text-[#111827]">{sortedAndFilteredMeetings.length}</strong>
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
