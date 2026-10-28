import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    FaSearch, FaUsers, FaAward, FaCalendarCheck, FaSync,
    FaIdCard, FaPhone, FaEnvelope, FaUser, FaTimes, FaCheckCircle,
    FaExclamationTriangle, FaExternalLinkAlt, FaChevronLeft, FaChevronRight,
    FaSpinner, FaFileDownload, FaClock, FaDoorOpen
} from 'react-icons/fa';
import { GetParticipants, GetParticipantMeetings } from '../../api/AdminAPI';
import { GenerateCertificateWithUserMeetID } from '../../api/GetRooms';
import { BaseURLFiles } from '../../api/apiNeed';
import { combinePath } from '../../Tools/tools';
import { useToast } from '../../Tools/ToastProvider';
import { useAuth } from '../../Tools/AuthProvider';

const ParticipantManagement = () => {
    const navigate = useNavigate();
    const toast = useToast();
    const { user: currentUser } = useAuth();

    // Participants list state
    const [participants, setParticipants] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [totalCount, setTotalCount] = useState(0);

    // Filters & Pagination (Strict 10 items per page as requested)
    const [searchTerm, setSearchTerm] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 10;
    const [totalPages, setTotalPages] = useState(1);

    // Attended Meetings Modal State
    const [attendedModalOpen, setAttendedModalOpen] = useState(false);
    const [selectedParticipantForMeetings, setSelectedParticipantForMeetings] = useState(null);
    const [participantMeetings, setParticipantMeetings] = useState([]);
    const [loadingMeetings, setLoadingMeetings] = useState(false);
    const [meetingsSearch, setMeetingsSearch] = useState('');

    // Certificate Issuance Modal State
    const [certificateModalOpen, setCertificateModalOpen] = useState(false);
    const [selectedParticipantForCert, setSelectedParticipantForCert] = useState(null);
    const [certMeetings, setCertMeetings] = useState([]);
    const [loadingCertMeetings, setLoadingCertMeetings] = useState(false);
    const [certMeetingsSearch, setCertMeetingsSearch] = useState('');
    const [issuingCertMeetingId, setIssuingCertMeetingId] = useState(null);

    // Helper for formatting duration
    const formatDuration = (seconds) => {
        if (!seconds && seconds !== 0) return '-';
        const s = Math.round(Number(seconds));
        if (isNaN(s)) return '-';
        const m = Math.floor(s / 60);
        const remS = s % 60;
        if (m === 0) return `${remS} ثانیه`;
        return `${m} دقیقه و ${remS} ثانیه`;
    };

    // Helper for avatar URL
    const getAvatarUrl = (path) => {
        if (!path) return null;
        if (path.startsWith('http://') || path.startsWith('https://')) return path;
        return combinePath(BaseURLFiles, path);
    };

    // Fetch participants from server (paginated by 10)
    const fetchParticipants = async (page = currentPage, q = searchTerm) => {
        setLoading(true);
        setError(null);
        try {
            const res = await GetParticipants(page, itemsPerPage, q);
            if (res) {
                const items = res.items || (Array.isArray(res.data) ? res.data : []);
                setParticipants(items);
                setTotalCount(res.totalCount || items.length);
                setTotalPages(res.totalPages || Math.ceil((res.totalCount || items.length) / itemsPerPage) || 1);
            } else {
                setParticipants([]);
                setTotalCount(0);
                setTotalPages(1);
            }
        } catch (err) {
            console.error("Error fetching participants:", err);
            setError("خطا در برقراری ارتباط با سرور و دریافت لیست شرکت‌کنندگان");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchParticipants(currentPage, searchTerm);
    }, [currentPage]);

    const handleSearchSubmit = (e) => {
        if (e) e.preventDefault();
        setCurrentPage(1);
        fetchParticipants(1, searchTerm);
    };

    // Open Attended Meetings Modal
    const openAttendedModal = async (participant) => {
        setSelectedParticipantForMeetings(participant);
        setAttendedModalOpen(true);
        setLoadingMeetings(true);
        setMeetingsSearch('');
        try {
            const data = await GetParticipantMeetings(participant.id, '');
            setParticipantMeetings(Array.isArray(data) ? data : []);
        } catch (err) {
            console.error("Error loading participant meetings:", err);
            if (toast?.error) toast.error("خطا در دریافت لیست جلسات شرکت‌کننده");
            setParticipantMeetings([]);
        } finally {
            setLoadingMeetings(false);
        }
    };

    // Filtered meetings for Attended Meetings Modal
    const filteredParticipantMeetings = useMemo(() => {
        if (!participantMeetings) return [];
        const term = (meetingsSearch || '').trim().toLowerCase();
        if (!term) return participantMeetings;
        return participantMeetings.filter(m =>
            (m.title && m.title.toLowerCase().includes(term)) ||
            (m.meetId && m.meetId.toLowerCase().includes(term)) ||
            (m.roomTitle && m.roomTitle.toLowerCase().includes(term)) ||
            (m.roomFriendlyId && m.roomFriendlyId.toLowerCase().includes(term))
        );
    }, [participantMeetings, meetingsSearch]);

    // Open Certificate Modal
    const openCertificateModal = async (participant) => {
        setSelectedParticipantForCert(participant);
        setCertificateModalOpen(true);
        setLoadingCertMeetings(true);
        setCertMeetingsSearch('');
        try {
            const data = await GetParticipantMeetings(participant.id, '');
            setCertMeetings(Array.isArray(data) ? data : []);
        } catch (err) {
            console.error("Error loading meetings for certificate:", err);
            if (toast?.error) toast.error("خطا در دریافت جلسات شرکت‌کننده");
            setCertMeetings([]);
        } finally {
            setLoadingCertMeetings(false);
        }
    };

    // Filtered meetings for Certificate Modal (ordered newest first)
    const filteredCertMeetings = useMemo(() => {
        if (!certMeetings) return [];
        const term = (certMeetingsSearch || '').trim().toLowerCase();
        if (!term) return certMeetings;
        return certMeetings.filter(m =>
            (m.title && m.title.toLowerCase().includes(term)) ||
            (m.meetId && m.meetId.toLowerCase().includes(term)) ||
            (m.roomTitle && m.roomTitle.toLowerCase().includes(term)) ||
            (m.roomFriendlyId && m.roomFriendlyId.toLowerCase().includes(term))
        );
    }, [certMeetings, certMeetingsSearch]);

    // Issue certificate action
    const handleIssueCertificate = async (meeting) => {
        if (!selectedParticipantForCert) return;

        // Strict verification: NationalCode and Phone must be present
        if (!selectedParticipantForCert.nationalCode || !selectedParticipantForCert.phoneNumber) {
            if (toast?.error) {
                toast.error("امکان صدور گواهی وجود ندارد: کد ملی و شماره موبایل شرکت‌کننده باید ثبت شده باشد.");
            }
            return;
        }

        setIssuingCertMeetingId(meeting.meetingId || meeting.meetId);
        try {
            const resultLink = await GenerateCertificateWithUserMeetID(meeting.userMeetId || meeting.meetingId, {
                nationalCode: selectedParticipantForCert.nationalCode,
                phone: selectedParticipantForCert.phoneNumber,
                meetID: meeting.meetId,
                name: selectedParticipantForCert.fullName || selectedParticipantForCert.username
            });

            if (toast?.success) {
                toast.success(`گواهینامه حضور در جلسه «${meeting.title}» با موفقیت صادر شد`);
            }

            // Update in local list
            setCertMeetings(prev => prev.map(m => {
                if (m.meetingId === meeting.meetingId || m.meetId === meeting.meetId) {
                    return { ...m, isCertificated: true, certificateFileUrl: resultLink || m.certificateFileUrl };
                }
                return m;
            }));

            // If link returned, open it
            if (resultLink) {
                window.open(resultLink, '_blank');
            }
        } catch (err) {
            console.error("Error generating certificate:", err);
            if (toast?.error) {
                toast.error(err.message || "خطا در صدور گواهینامه");
            }
        } finally {
            setIssuingCertMeetingId(null);
        }
    };

    return (
        <div className="p-4 sm:p-6 lg:p-8 bg-gray-50 min-h-screen rounded-2xl" dir="rtl">
            {/* Header section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
                <div>
                    <h2 className="text-2xl font-black text-gray-900 flex items-center gap-3">
                        <span className="p-2.5 bg-gradient-to-r from-emerald-600 to-teal-600 text-white rounded-xl shadow-md">
                            <FaUsers size={22} />
                        </span>
                        <span>مدیریت شرکت‌کنندگان</span>
                    </h2>
                    <p className="text-sm text-gray-500 mt-1 font-medium">
                        مشاهده لیست یکتا، جلسات حضور یافته و صدور گواهینامه برای شرکت‌کنندگان اتاق‌های تحت مدیریت
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        onClick={() => fetchParticipants(currentPage, searchTerm)}
                        disabled={loading}
                        className="flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-100 transition shadow-sm font-bold text-sm disabled:opacity-50"
                    >
                        <FaSync className={loading ? "animate-spin text-emerald-600" : "text-gray-500"} />
                        <span>بروزرسانی لیست</span>
                    </button>
                </div>
            </div>

            {/* Quick Stats Bar */}
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">مجموع کل شرکت‌کنندگان یکتا</p>
                        <p className="text-2xl font-black text-gray-900">{totalCount}</p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold">
                        <FaUsers size={22} />
                    </div>
                </div>

                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">تعداد نمایش در صفحه</p>
                        <p className="text-2xl font-black text-blue-600">{participants.length} <span className="text-xs text-gray-400 font-medium">(از ۱۰)</span></p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center font-bold">
                        <FaIdCard size={22} />
                    </div>
                </div>

                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">واجد شرایط صدور گواهی</p>
                        <p className="text-2xl font-black text-purple-600">
                            {participants.filter(p => p.canIssueCertificate).length} <span className="text-xs text-gray-400 font-medium">نفر در این صفحه</span>
                        </p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-purple-50 text-purple-600 flex items-center justify-center font-bold">
                        <FaAward size={22} />
                    </div>
                </div>
            </div>

            {/* Filter & Search Bar */}
            <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm mb-6">
                <form onSubmit={handleSearchSubmit} className="flex flex-col sm:flex-row gap-3">
                    <div className="relative flex-1">
                        <input
                            type="text"
                            placeholder="جستجو بر اساس نام، نام کاربری، کد ملی، شماره موبایل، ایمیل یا شناسه..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pr-10 pl-24 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:outline-none transition"
                        />
                        <FaSearch className="absolute right-3.5 top-3.5 text-gray-400" />
                        <button
                            type="submit"
                            className="absolute left-1.5 top-1.5 bottom-1.5 px-4 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-bold transition shadow-xs"
                        >
                            جستجو
                        </button>
                    </div>
                    {searchTerm && (
                        <button
                            type="button"
                            onClick={() => {
                                setSearchTerm('');
                                setCurrentPage(1);
                                fetchParticipants(1, '');
                            }}
                            className="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                        >
                            حذف فیلتر
                        </button>
                    )}
                </form>
            </div>

            {/* Error Banner */}
            {error && (
                <div className="mb-6 p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm flex items-center gap-2">
                    <FaTimes size={16} />
                    <span>{error}</span>
                </div>
            )}

            {/* Table View (8 Columns) */}
            {loading ? (
                <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center shadow-sm">
                    <FaSync className="animate-spin text-emerald-600 mx-auto text-3xl mb-3" />
                    <p className="text-gray-600 font-bold">در حال بارگذاری لیست شرکت‌کنندگان...</p>
                </div>
            ) : participants.length === 0 ? (
                <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center shadow-sm">
                    <FaUsers className="text-gray-300 mx-auto text-5xl mb-4" />
                    <h3 className="text-lg font-black text-gray-800 mb-1">هیچ شرکت‌کننده‌ای یافت نشد</h3>
                    <p className="text-sm text-gray-500">شرکت‌کننده‌ای با مشخصات انتخابی در سیستم ثبت نشده است.</p>
                </div>
            ) : (
                <div className="bg-white rounded-2xl border border-gray-200 shadow-sm overflow-hidden mb-6">
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50/80">
                                <tr>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600 w-16">آواتار</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">نام کامل</th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600">شناسه</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">نام کاربری</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">شماره موبایل</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">ایمیل</th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600">نقش</th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600 min-w-[140px]">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-100 text-sm">
                                {participants.map((item) => {
                                    const avatarUrl = getAvatarUrl(item.avatarPath);
                                    const initialChar = (item.fullName || item.username || '؟').charAt(0);

                                    return (
                                        <tr key={item.id} className="hover:bg-emerald-50/20 transition-colors">
                                            {/* 1. آواتار */}
                                            <td className="px-4 py-3 text-center">
                                                <div className="flex justify-center">
                                                    {avatarUrl ? (
                                                        <img
                                                            src={avatarUrl}
                                                            alt={item.fullName}
                                                            className="w-10 h-10 rounded-full object-cover border border-gray-200 shadow-xs"
                                                            onError={(e) => { e.target.style.display = 'none'; }}
                                                        />
                                                    ) : (
                                                        <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-emerald-500 to-teal-400 text-white font-black text-sm flex items-center justify-center shadow-xs">
                                                            {initialChar}
                                                        </div>
                                                    )}
                                                </div>
                                            </td>

                                            {/* 2. نام کامل */}
                                            <td className="px-4 py-3">
                                                <div className="flex flex-col">
                                                    <span className="font-extrabold text-gray-900 leading-snug">
                                                        {item.fullName || 'بدون نام'}
                                                    </span>
                                                    {item.nationalCode && (
                                                        <span className="text-[11px] text-gray-500 font-mono flex items-center gap-1 mt-0.5" dir="ltr">
                                                            <FaIdCard size={11} className="text-gray-400" />
                                                            <span>{item.nationalCode}</span>
                                                        </span>
                                                    )}
                                                </div>
                                            </td>

                                            {/* 3. شناسه */}
                                            <td className="px-4 py-3 text-center">
                                                <span className="font-mono text-xs font-black text-gray-700 bg-gray-100 px-2 py-0.5 rounded-md">
                                                    #{item.id}
                                                </span>
                                            </td>

                                            {/* 4. نام کاربری */}
                                            <td className="px-4 py-3 font-mono text-xs text-gray-700" dir="ltr">
                                                {item.username ? `@${item.username}` : '-'}
                                            </td>

                                            {/* 5. شماره موبایل */}
                                            <td className="px-4 py-3 text-xs" dir="ltr">
                                                {item.phoneNumber ? (
                                                    <span className="font-mono font-bold text-gray-800 flex items-center gap-1.5 justify-end">
                                                        <span>{item.phoneNumber}</span>
                                                        <FaPhone size={10} className="text-emerald-600 shrink-0" />
                                                    </span>
                                                ) : (
                                                    <span className="text-[11px] text-gray-400 bg-gray-50 px-2 py-0.5 rounded border border-gray-200">ثبت نشده</span>
                                                )}
                                            </td>

                                            {/* 6. ایمیل */}
                                            <td className="px-4 py-3 text-xs" dir="ltr">
                                                {item.email ? (
                                                    <span className="font-mono text-gray-600 truncate max-w-[140px] block" title={item.email}>
                                                        {item.email}
                                                    </span>
                                                ) : (
                                                    <span className="text-[11px] text-gray-400">ثبت نشده</span>
                                                )}
                                            </td>

                                            {/* 7. نقش */}
                                            <td className="px-4 py-3 text-center">
                                                <span className="px-2.5 py-1 rounded-full text-xs font-bold bg-blue-50 text-blue-700 border border-blue-200 whitespace-nowrap">
                                                    {item.roleTitle || 'کاربر'}
                                                </span>
                                            </td>

                                            {/* 8. عملیات (۲ دکمه: صدور گواهی + جلساتی که شرکت کرده) */}
                                            <td className="px-4 py-3 text-center">
                                                <div className="flex items-center justify-center gap-2">
                                                    {/* دکمه صدور گواهینامه (فقط آیکون) */}
                                                    <button
                                                        type="button"
                                                        onClick={() => openCertificateModal(item)}
                                                        className={`p-2 rounded-xl transition shadow-xs flex items-center justify-center ${
                                                            item.canIssueCertificate
                                                                ? 'bg-amber-50 hover:bg-amber-100 text-amber-700 border border-amber-200'
                                                                : 'bg-gray-100 hover:bg-gray-200 text-gray-400 border border-gray-200'
                                                        }`}
                                                        title={
                                                            item.canIssueCertificate
                                                                ? "صدور گواهینامه برای این شرکت‌کننده"
                                                                : "عدم امکان صدور گواهی (کد ملی یا شماره موبایل ثبت نشده است)"
                                                        }
                                                    >
                                                        <FaAward size={16} />
                                                    </button>

                                                    {/* دکمه جلساتی که شرکت کرده (با badge تعداد) */}
                                                    <button
                                                        type="button"
                                                        onClick={() => openAttendedModal(item)}
                                                        className="relative p-2 bg-blue-50 hover:bg-blue-100 text-blue-700 border border-blue-200 rounded-xl transition shadow-xs flex items-center justify-center group"
                                                        title={`مشاهده ${item.attendedMeetingsCount || 0} جلسه حضور یافته`}
                                                    >
                                                        <FaCalendarCheck size={15} />
                                                        <span
                                                            className="absolute -top-1.5 -right-1.5 bg-blue-600 text-white text-[9px] font-black px-1.5 py-0.2 rounded-full min-w-[18px] text-center shadow-xs"
                                                            title={`${item.attendedMeetingsCount || 0} جلسه حضور یافته`}
                                                        >
                                                            {item.attendedMeetingsCount || 0}
                                                        </span>
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>
            )}

            {/* Pagination Controls (Strict 10 items per page) */}
            {!loading && totalPages > 1 && (
                <div className="bg-white p-4 rounded-2xl border border-gray-200 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-sm">
                    <p className="text-xs text-gray-500 font-bold">
                        نمایش صفحه <strong className="text-gray-900">{currentPage}</strong> از <strong className="text-gray-900">{totalPages}</strong> (مجموعاً {totalCount} شرکت‌کننده)
                    </p>

                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                            disabled={currentPage === 1}
                            className="p-2 border border-gray-200 rounded-xl hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition"
                            title="صفحه قبلی"
                        >
                            <FaChevronRight size={12} />
                        </button>

                        <div className="flex items-center gap-1">
                            {Array.from({ length: totalPages }, (_, i) => i + 1)
                                .filter(p => p === 1 || p === totalPages || Math.abs(p - currentPage) <= 2)
                                .map((page, idx, arr) => {
                                    const showEllipsis = idx > 0 && page - arr[idx - 1] > 1;
                                    return (
                                        <React.Fragment key={page}>
                                            {showEllipsis && <span className="px-2 text-gray-400">...</span>}
                                            <button
                                                onClick={() => setCurrentPage(page)}
                                                className={`w-8 h-8 rounded-xl text-xs font-bold transition ${
                                                    currentPage === page
                                                        ? 'bg-emerald-600 text-white shadow-md'
                                                        : 'text-gray-700 hover:bg-gray-100'
                                                }`}
                                            >
                                                {page}
                                            </button>
                                        </React.Fragment>
                                    );
                                })}
                        </div>

                        <button
                            onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                            disabled={currentPage === totalPages}
                            className="p-2 border border-gray-200 rounded-xl hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition"
                            title="صفحه بعدی"
                        >
                            <FaChevronLeft size={12} />
                        </button>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* MODAL 1: لیست جلساتی که شرکت کرده */}
            {/* ==================================================== */}
            {attendedModalOpen && selectedParticipantForMeetings && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-3xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn flex flex-col max-h-[85vh]" dir="rtl">
                        {/* Header */}
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4 shrink-0">
                            <div className="flex items-center gap-3">
                                <span className="p-2.5 bg-blue-100 text-blue-700 rounded-xl">
                                    <FaCalendarCheck size={20} />
                                </span>
                                <div>
                                    <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                        <span>جلسات حضور یافته: {selectedParticipantForMeetings.fullName || selectedParticipantForMeetings.username}</span>
                                        <span className="text-xs font-bold px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 border border-blue-200">
                                            {participantMeetings.length} جلسه
                                        </span>
                                    </h3>
                                    <p className="text-xs text-gray-500 mt-0.5 font-mono">
                                        شناسه کاربر: #{selectedParticipantForMeetings.id} — نام کاربری: @{selectedParticipantForMeetings.username}
                                    </p>
                                </div>
                            </div>
                            <button
                                onClick={() => setAttendedModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        {/* Search inside modal */}
                        <div className="mb-4 shrink-0">
                            <div className="relative">
                                <input
                                    type="text"
                                    placeholder="جستجو در جلسات حضور یافته..."
                                    value={meetingsSearch}
                                    onChange={(e) => setMeetingsSearch(e.target.value)}
                                    className="w-full pr-9 pl-4 py-2 bg-gray-50 border border-gray-200 rounded-xl text-xs focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
                                />
                                <FaSearch className="absolute right-3 top-2.5 text-gray-400 text-xs" />
                            </div>
                        </div>

                        {/* Meetings Content List */}
                        <div className="flex-1 overflow-y-auto min-h-0 border border-gray-100 rounded-2xl">
                            {loadingMeetings ? (
                                <div className="p-12 text-center">
                                    <FaSync className="animate-spin text-blue-600 mx-auto text-3xl mb-3" />
                                    <p className="text-gray-600 text-xs font-bold">در حال دریافت لیست جلسات...</p>
                                </div>
                            ) : filteredParticipantMeetings.length === 0 ? (
                                <div className="p-12 text-center">
                                    <FaCalendarCheck className="text-gray-300 mx-auto text-4xl mb-3" />
                                    <p className="text-gray-600 text-sm font-bold">جلسه‌ای یافت نشد</p>
                                </div>
                            ) : (
                                <table className="min-w-full divide-y divide-gray-100 text-xs">
                                    <thead className="bg-gray-50/80 sticky top-0">
                                        <tr>
                                            <th className="px-3 py-3 text-right font-black text-gray-600">عنوان جلسه</th>
                                            <th className="px-3 py-3 text-right font-black text-gray-600">اتاق</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">تاریخ برگزاری</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">مدت حضور</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">گواهینامه</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100 bg-white">
                                        {filteredParticipantMeetings.map((m, idx) => (
                                            <tr key={m.meetingId || idx} className="hover:bg-blue-50/30 transition">
                                                <td className="px-3 py-3">
                                                    <div className="flex flex-col">
                                                        <span className="font-extrabold text-gray-900">{m.title || 'بدون عنوان'}</span>
                                                        <span className="font-mono text-[10px] text-gray-400" dir="ltr">{m.meetId}</span>
                                                    </div>
                                                </td>
                                                <td className="px-3 py-3">
                                                    <span className="font-semibold text-gray-700">{m.roomTitle || '-'}</span>
                                                    {m.roomFriendlyId && (
                                                        <span className="block font-mono text-[10px] text-orange-600">{m.roomFriendlyId}</span>
                                                    )}
                                                </td>
                                                <td className="px-3 py-3 text-center text-gray-600 whitespace-nowrap">
                                                    {m.jalaliDate || (m.dateInsert ? new Date(m.dateInsert).toLocaleDateString('fa-IR') : '-')}
                                                </td>
                                                <td className="px-3 py-3 text-center font-bold text-gray-700">
                                                    {formatDuration(m.timeSpendSeconds)}
                                                </td>
                                                <td className="px-3 py-3 text-center">
                                                    {m.isCertificated ? (
                                                        <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 border border-amber-200 flex items-center justify-center gap-1 w-max mx-auto">
                                                            <FaAward size={10} />
                                                            <span>صادر شده</span>
                                                        </span>
                                                    ) : (
                                                        <span className="text-[11px] text-gray-400">صادر نشده</span>
                                                    )}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            )}
                        </div>

                        {/* Footer */}
                        <div className="mt-4 pt-3 border-t border-gray-100 flex items-center justify-between shrink-0">
                            <span className="text-xs text-gray-500 font-bold">
                                نمایش {filteredParticipantMeetings.length} جلسه از مجموع {participantMeetings.length} جلسه
                            </span>
                            <button
                                type="button"
                                onClick={() => setAttendedModalOpen(false)}
                                className="px-5 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                            >
                                بستن
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* MODAL 2: صدور گواهینامه */}
            {/* ==================================================== */}
            {certificateModalOpen && selectedParticipantForCert && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-3xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn flex flex-col max-h-[85vh]" dir="rtl">
                        {/* Header */}
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4 shrink-0">
                            <div className="flex items-center gap-3">
                                <span className="p-2.5 bg-amber-100 text-amber-800 rounded-xl">
                                    <FaAward size={20} />
                                </span>
                                <div>
                                    <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                        <span>صدور گواهینامه حضور برای {selectedParticipantForCert.fullName || selectedParticipantForCert.username}</span>
                                    </h3>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        یک جلسه را از لیست جدیدترین جلسات حضور یافته جهت صدور گواهینامه انتخاب نمایید
                                    </p>
                                </div>
                            </div>
                            <button
                                onClick={() => setCertificateModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        {/* Identity Warning Alert if NationalCode or Phone missing */}
                        {(!selectedParticipantForCert.nationalCode || !selectedParticipantForCert.phoneNumber) ? (
                            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-2xl flex items-start gap-3 shrink-0">
                                <FaExclamationTriangle className="text-red-500 mt-0.5 shrink-0" size={18} />
                                <div className="text-xs text-red-800 leading-relaxed">
                                    <strong className="block font-black mb-1">عدم امکان صدور گواهینامه</strong>
                                    کد ملی و شماره موبایل این شرکت‌کننده در سیستم تکمیل نشده است. طبق ضوابط سامانه، جهت صدور گواهینامه معتبر، ثبت هر دوی کد ملی و شماره موبایل الزامی می‌باشد.
                                    <div className="mt-2 flex gap-4 text-[11px] font-mono">
                                        <span>کد ملی: {selectedParticipantForCert.nationalCode || '❌ ثبت نشده'}</span>
                                        <span>موبایل: {selectedParticipantForCert.phoneNumber || '❌ ثبت نشده'}</span>
                                    </div>
                                </div>
                            </div>
                        ) : (
                            <div className="mb-4 p-3 bg-emerald-50 border border-emerald-200 rounded-2xl flex items-center justify-between text-xs text-emerald-800 shrink-0">
                                <div className="flex items-center gap-2">
                                    <FaCheckCircle className="text-emerald-600" size={15} />
                                    <span>مشخصات هویتی کاربر جهت صدور گواهی تأیید است.</span>
                                </div>
                                <div className="flex items-center gap-4 font-mono font-bold text-[11px]">
                                    <span>کد ملی: {selectedParticipantForCert.nationalCode}</span>
                                    <span>موبایل: {selectedParticipantForCert.phoneNumber}</span>
                                </div>
                            </div>
                        )}

                        {/* Search inside modal */}
                        <div className="mb-4 shrink-0">
                            <div className="relative">
                                <input
                                    type="text"
                                    placeholder="جستجو در جلسات (به ترتیب جدیدترین)..."
                                    value={certMeetingsSearch}
                                    onChange={(e) => setCertMeetingsSearch(e.target.value)}
                                    className="w-full pr-9 pl-4 py-2 bg-gray-50 border border-gray-200 rounded-xl text-xs focus:bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none"
                                />
                                <FaSearch className="absolute right-3 top-2.5 text-gray-400 text-xs" />
                            </div>
                        </div>

                        {/* Meetings list */}
                        <div className="flex-1 overflow-y-auto min-h-0 border border-gray-100 rounded-2xl">
                            {loadingCertMeetings ? (
                                <div className="p-12 text-center">
                                    <FaSync className="animate-spin text-amber-600 mx-auto text-3xl mb-3" />
                                    <p className="text-gray-600 text-xs font-bold">در حال بارگذاری جلسات شرکت‌کننده...</p>
                                </div>
                            ) : filteredCertMeetings.length === 0 ? (
                                <div className="p-12 text-center">
                                    <FaAward className="text-gray-300 mx-auto text-4xl mb-3" />
                                    <p className="text-gray-600 text-sm font-bold">جلسه‌ای برای صدور گواهی یافت نشد</p>
                                </div>
                            ) : (
                                <div className="divide-y divide-gray-100">
                                    {filteredCertMeetings.map((m) => {
                                        const isIssuingThis = issuingCertMeetingId === (m.meetingId || m.meetId);
                                        const canIssue = selectedParticipantForCert.canIssueCertificate;

                                        return (
                                            <div key={m.meetingId || m.meetId} className="p-4 hover:bg-amber-50/20 transition flex items-center justify-between gap-4">
                                                <div className="flex-1 min-w-0">
                                                    <div className="flex items-center gap-2 mb-1">
                                                        <h4 className="font-extrabold text-sm text-gray-900 truncate">{m.title || 'بدون عنوان'}</h4>
                                                        {m.isCertificated && (
                                                            <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
                                                                گواهی قبلاً صادر شده
                                                            </span>
                                                        )}
                                                    </div>
                                                    <div className="flex flex-wrap items-center gap-4 text-xs text-gray-500">
                                                        <span>اتاق: <strong className="text-gray-700">{m.roomTitle || '-'}</strong> ({m.roomFriendlyId || '-'})</span>
                                                        <span>تاریخ: <strong className="text-gray-700">{m.jalaliDate || (m.dateInsert ? new Date(m.dateInsert).toLocaleDateString('fa-IR') : '-')}</strong></span>
                                                        <span>مدت حضور: <strong className="text-gray-700">{formatDuration(m.timeSpendSeconds)}</strong></span>
                                                    </div>
                                                </div>

                                                <div className="shrink-0 flex items-center gap-2">
                                                    {m.certificateFileUrl && (
                                                        <a
                                                            href={m.certificateFileUrl}
                                                            target="_blank"
                                                            rel="noopener noreferrer"
                                                            className="px-3 py-2 bg-emerald-50 hover:bg-emerald-100 text-emerald-800 border border-emerald-200 rounded-xl text-xs font-bold transition flex items-center gap-1.5"
                                                            title="مشاهده گواهینامه صادرشده"
                                                        >
                                                            <FaFileDownload size={12} />
                                                            <span>دانلود گواهی</span>
                                                        </a>
                                                    )}

                                                    <button
                                                        type="button"
                                                        disabled={!canIssue || isIssuingThis}
                                                        onClick={() => handleIssueCertificate(m)}
                                                        className={`px-4 py-2 rounded-xl text-xs font-bold transition flex items-center gap-1.5 shadow-sm ${
                                                            !canIssue
                                                                ? 'bg-gray-100 text-gray-400 cursor-not-allowed border border-gray-200'
                                                                : 'bg-amber-600 hover:bg-amber-700 text-white'
                                                        }`}
                                                        title={!canIssue ? "نیاز به تکمیل کد ملی و موبایل" : "صدور یا بازتولید گواهی"}
                                                    >
                                                        {isIssuingThis ? (
                                                            <>
                                                                <FaSpinner className="animate-spin text-xs" />
                                                                <span>در حال صدور...</span>
                                                            </>
                                                        ) : (
                                                            <>
                                                                <FaAward size={13} />
                                                                <span>{m.isCertificated ? 'صدور مجدد گواهی' : 'صدور گواهینامه'}</span>
                                                            </>
                                                        )}
                                                    </button>
                                                </div>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>

                        {/* Footer */}
                        <div className="mt-4 pt-3 border-t border-gray-100 flex items-center justify-between shrink-0">
                            <span className="text-xs text-gray-500 font-bold">
                                {filteredCertMeetings.length} جلسه قابل مشاهده
                            </span>
                            <button
                                type="button"
                                onClick={() => setCertificateModalOpen(false)}
                                className="px-5 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                            >
                                بستن
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ParticipantManagement;
