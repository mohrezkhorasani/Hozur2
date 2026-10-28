// src/pages/admin/MeetingLogsList.jsx
import { useState, useEffect, useMemo } from 'react';
import { FaSearch, FaSort, FaUser, FaSignInAlt, FaSignOutAlt, FaClock, FaChevronDown, FaCheckCircle } from 'react-icons/fa';
import { GetUserMeetLogs } from '../../api/GetRooms';
import { useParams, useSearchParams } from 'react-router-dom';

// ---------- گزینه‌های مرتب‌سازی ----------
const sortOptions = [
    { value: 100, label: 'جدیدترین', field: 'date', order: 'desc' },
    { value: 101, label: 'قدیمی‌ترین', field: 'date', order: 'asc' },
    { value: 102, label: 'نام (ی - الف)', field: 'user.fullname', order: 'desc' },
    { value: 103, label: 'نام (الف - ی)', field: 'user.fullname', order: 'asc' },
];

// ---------- گزینه‌های فیلتر نوع رویداد ----------
const eventTypeOptions = [
    { value: 'ALL', label: 'همه' },
    { value: 'JOIN', label: 'ورود' },
    { value: 'LEFT', label: 'خروج' },
    { value: 'ISLAMIC_ATTENDANCE_VERIFIED', label: 'حضور غیاب' },
];

// ---------- کامپوننت نمایش هر پارامتر جزئیات ایجنت با قابلیت نمایش بیشتر/کمتر ----------
const AgentDetailItem = ({ text, index, total }) => {
    const [isExpanded, setIsExpanded] = useState(false);
    const cleanText = (text || '').trim();
    const isLong = cleanText.length > 15;
    const displayText = isLong && !isExpanded ? `${cleanText.slice(0, 15)}...` : cleanText;
    const label = total > 1 ? `جزئیات ایجنت (${index + 1})` : 'جزئیات ایجنت';

    return (
        <div className="bg-gray-50 hover:bg-gray-100/80 border border-gray-200 rounded p-1.5 text-xs transition-colors">
            <div className="flex items-center justify-between gap-2 text-[11px] text-gray-500 font-medium mb-1">
                <span>{label}:</span>
                {isLong && (
                    <button
                        type="button"
                        onClick={() => setIsExpanded(!isExpanded)}
                        className="text-[#0b3f32] hover:text-[#30d4cc] font-bold text-[10px] hover:underline cursor-pointer transition"
                    >
                        {isExpanded ? 'نمایش کمتر' : 'نمایش بیشتر'}
                    </button>
                )}
            </div>
            <div
                className="font-mono text-gray-800 break-all text-left text-[11px] leading-relaxed select-all"
                dir="ltr"
                title={cleanText}
            >
                {displayText}
            </div>
        </div>
    );
};

// ---------- کامپوننت ستون آی پی / جزئیات ایجنت برای حضور غیاب ----------
const AgentDetailsCell = ({ log }) => {
    const rawValue = (typeof log?.value === 'string' && log.value.trim())
        ? log.value
        : (typeof log?.ip === 'string' ? log.ip : '');

    if (!rawValue || !rawValue.trim()) {
        return <span className="text-gray-400">-</span>;
    }

    const items = rawValue
        .split('-')
        .map(part => part.trim())
        .filter(Boolean);

    if (items.length === 0) {
        return <span className="text-gray-400">-</span>;
    }

    return (
        <div className="flex flex-col gap-1.5 py-1 min-w-[180px] max-w-xs">
            {items.map((item, idx) => (
                <AgentDetailItem
                    key={idx}
                    text={item}
                    index={idx}
                    total={items.length}
                />
            ))}
        </div>
    );
};

// ---------- کامپوننت اصلی UserMeetingLogsList ----------
const UserMeetingLogsList = () => { // umid به عنوان prop
    const [searchParams] = useSearchParams();
    const [allLogs, setAllLogs] = useState([]);
    const { umid } = useParams();
    const umidQuery = searchParams.get('umid') || 0; // دریافت umid از URL
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState(100);
    const [eventTypeFilter, setEventTypeFilter] = useState('ALL');
    const [currentPage, setCurrentPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const itemsPerPage = 10;

    // دریافت لاگ‌ها با صفحه‌بندی
    const fetchLogs = async (page = currentPage) => {
        setLoading(true);
        setError(null);
        try {
            const response = await GetUserMeetLogs(umid, page, itemsPerPage);
            console.log(response)
            // بررسی ساختار پاسخ
            if (response && response.status === 200) {
                if (Array.isArray(response.data)) {
                    setAllLogs(response.data);
                    setTotalCount(response.totalCount || response.data.length);
                } else if (Array.isArray(response)) {
                    setAllLogs(response);
                    setTotalCount(response.length);
                } else {
                    setError('ساختار داده نامعتبر است');
                }
            } else {
                setError(response?.message || 'خطا در دریافت اطلاعات لاگ‌ها');
            }
        } catch (err) {
            console.error(err);
            setError('مشکل در ارتباط با سرور');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchLogs(1);
    }, [umid]); // با تغییر umid دوباره فراخوانی کن

    // وقتی صفحه تغییر کرد، داده‌ها رو دوباره دریافت کن
    useEffect(() => {
        if (currentPage > 1) {
            fetchLogs(currentPage);
        }
    }, [currentPage]);

    // فیلتر بر اساس جستجو (سمت کلاینت)
    const filteredLogs = useMemo(() => {
        let result = allLogs;

        // فیلتر جستجو
        if (searchTerm.trim()) {
            const term = searchTerm.toLowerCase();
            result = result.filter(
                log => log.user?.fullname?.toLowerCase().includes(term) ||
                    log.content?.toLowerCase().includes(term) ||
                    log.ip?.toLowerCase().includes(term) ||
                    log.value?.toLowerCase().includes(term) ||
                    (log.key === 'ISLAMIC_ATTENDANCE_VERIFIED' && ('حضور غیاب'.includes(term) || 'حضور و غیاب'.includes(term))) ||
                    log.id?.toString().includes(term)
            );
        }

        // فیلتر نوع رویداد
        if (eventTypeFilter !== 'ALL') {
            result = result.filter(log => log.key === eventTypeFilter);
        }

        return result;
    }, [allLogs, searchTerm, eventTypeFilter]);

    // مرتب‌سازی (سمت کلاینت)
    const sortedLogs = useMemo(() => {
        const option = sortOptions.find(opt => opt.value === sortBy);
        if (!option) return filteredLogs;
        const { field, order } = option;
        return [...filteredLogs].sort((a, b) => {
            let aVal = field.includes('.')
                ? field.split('.').reduce((obj, key) => obj?.[key], a)
                : a[field];
            let bVal = field.includes('.')
                ? field.split('.').reduce((obj, key) => obj?.[key], b)
                : b[field];

            if (typeof aVal === 'string' && typeof bVal === 'string') {
                return order === 'asc'
                    ? aVal.localeCompare(bVal)
                    : bVal.localeCompare(aVal);
            }

            if (order === 'asc') return aVal > bVal ? 1 : -1;
            return aVal < bVal ? 1 : -1;
        });
    }, [filteredLogs, sortBy]);

    const totalPages = Math.ceil(totalCount / itemsPerPage);
    const paginatedLogs = sortedLogs; // چون داده‌ها از سرور صفحه‌بندی شدن

    // وقتی جستجو یا فیلتر تغییر کرد، صفحه اول رو نشون بده
    useEffect(() => {
        setCurrentPage(1);
    }, [searchTerm, sortBy, eventTypeFilter]);

    // فرمت تاریخ
    const formatTime = (dateString) => {
        if (!dateString) return '-';
        try {
            const date = new Date(dateString);

            // اضافه کردن 3.5 ساعت به وقت UTC (تهران UTC+3:30)
            const tehranOffset = 3.5 * 60 * 60 * 1000; // 3.5 ساعت به میلی‌ثانیه
            const tehranTime = new Date(date.getTime() + tehranOffset);
            console.log("tehranTime", dateString, tehranTime)
            const hours = String(tehranTime.getHours()).padStart(2, '0');
            const minutes = String(tehranTime.getMinutes()).padStart(2, '0');
            const seconds = String(tehranTime.getSeconds()).padStart(2, '0');

            return `${hours}:${minutes}:${seconds}`;
        } catch {
            return "dateString";
        }
    };    // نمایش نوع رویداد
    const getEventBadge = (key) => {
        if (key === 'JOIN') {
            return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"><FaSignInAlt className="text-xs" /> ورود</span>;
        }
        if (key === 'ISLAMIC_ATTENDANCE_VERIFIED') {
            return (
                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-800 border border-emerald-200">
                    <FaCheckCircle className="text-xs" /> حضور غیاب
                </span>
            );
        }
        if (key === 'LEFT') {
            return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800"><FaSignOutAlt className="text-xs" /> خروج</span>;
        }
        return (
            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                {key}
            </span>
        );
    };

    // صفحه‌بندی
    const renderPaginationButtons = () => {
        if (totalPages <= 1) return null;
        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, currentPage + 2);
        if (endPage - startPage < 4) {
            if (startPage === 1) endPage = Math.min(totalPages, startPage + 4);
            else startPage = Math.max(1, endPage - 4);
        }
        const pages = [];
        for (let i = startPage; i <= endPage; i++) pages.push(i);
        return (
            <div className="flex gap-1">
                <button
                    onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                    disabled={currentPage === 1}
                    className="px-3 py-1 border rounded-md disabled:opacity-50 hover:bg-gray-50 transition"
                >
                    قبلی
                </button>
                {startPage > 1 && (
                    <>
                        <button onClick={() => setCurrentPage(1)} className="px-3 py-1 border rounded-md hover:bg-gray-50 transition">1</button>
                        {startPage > 2 && <span className="px-2">...</span>}
                    </>
                )}
                {pages.map(p => (
                    <button
                        key={p}
                        onClick={() => setCurrentPage(p)}
                        className={`px-3 py-1 border rounded-md transition ${currentPage === p ? 'bg-[#0b3f32] text-white' : 'hover:bg-gray-50'}`}
                    >
                        {p}
                    </button>
                ))}
                {endPage < totalPages && (
                    <>
                        {endPage < totalPages - 1 && <span className="px-2">...</span>}
                        <button onClick={() => setCurrentPage(totalPages)} className="px-3 py-1 border rounded-md hover:bg-gray-50 transition">{totalPages}</button>
                    </>
                )}
                <button
                    onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                    disabled={currentPage === totalPages}
                    className="px-3 py-1 border rounded-md disabled:opacity-50 hover:bg-gray-50 transition"
                >
                    بعدی
                </button>
            </div>
        );
    };

    if (error) return (
        <div className="bg-red-100 text-red-700 p-4 rounded-lg">
            {error}
            <button onClick={() => fetchLogs(currentPage)} className="mr-4 px-3 py-1 bg-red-600 text-white rounded-lg hover:bg-red-700 transition">
                تلاش مجدد
            </button>
        </div>
    );

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden" dir="rtl">
            {/* نوار جستجو و فیلتر و مرتب‌سازی */}
            <div className="p-4 border-b border-gray-200 flex flex-col sm:flex-row gap-4 justify-between">
                <div className="relative flex-1">
                    <input
                        type="text"
                        placeholder="جستجو بر اساس نام کاربر، محتوا یا ID..."
                        value={searchTerm}
                        onChange={e => setSearchTerm(e.target.value)}
                        className="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-[#0b3f32] focus:border-transparent outline-none"
                    />
                    <FaSearch className="absolute right-3 top-3 text-gray-400" />
                </div>
                <div className="flex items-center gap-2 flex-wrap">
                    <div className="flex items-center gap-1">
                        <FaUser className="text-gray-500" />
                        <select
                            value={eventTypeFilter}
                            onChange={e => setEventTypeFilter(e.target.value)}
                            className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-[#0b3f32] focus:border-transparent outline-none"
                        >
                            {eventTypeOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                        </select>
                    </div>
                    <div className="flex items-center gap-1">
                        <FaSort className="text-gray-500" />
                        <select
                            value={sortBy}
                            onChange={e => setSortBy(Number(e.target.value))}
                            className="border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-[#0b3f32] focus:border-transparent outline-none"
                        >
                            {sortOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                        </select>
                    </div>
                </div>
            </div>

            {/* جدول لاگ‌ها */}
            {loading ? (
                <div className="text-center py-12">
                    <div className="w-10 h-10 border-4 border-gray-200 border-t-[#0b3f32] rounded-full animate-spin mx-auto"></div>
                    <p className="text-gray-500 mt-3">در حال بارگذاری لاگ‌ها...</p>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">شناسه</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">کاربر</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">آی پی</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">رویداد</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">محتوا</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">تاریخ و زمان</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {paginatedLogs.length === 0 ? (
                                <tr>
                                    <td colSpan="6" className="px-6 py-12 text-center text-gray-500">
                                        هیچ لاگی یافت نشد
                                    </td>
                                </tr>
                            ) : (
                                paginatedLogs.map(log => (
                                    <tr key={log.id} className="hover:bg-gray-50 transition">
                                        <td className="px-6 py-4 text-sm text-gray-900 whitespace-nowrap">{log.id}</td>
                                        <td className="px-6 py-4 text-sm font-medium text-gray-900">
                                            <div className="flex items-center gap-2">
                                                <div className="w-8 h-8 rounded-full bg-gradient-to-br from-[#0b3f32] to-[#30d4cc] flex items-center justify-center text-white text-xs font-bold">
                                                    {log.user?.fullname?.charAt(0) || '?'}
                                                </div>
                                                {log.user?.fullname || 'ناشناس'}
                                            </div>
                                        </td>
                                        <td className={`px-6 py-4 text-sm ${log.key === 'ISLAMIC_ATTENDANCE_VERIFIED' ? '' : 'whitespace-nowrap'}`}>
                                            {log.key === 'ISLAMIC_ATTENDANCE_VERIFIED' ? (
                                                <AgentDetailsCell log={log} />
                                            ) : (
                                                log.ip || "-"
                                            )}
                                        </td>
                                        <td className="px-6 py-4 text-sm whitespace-nowrap">
                                            {getEventBadge(log.key)}
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-700">
                                            {log.key === 'ISLAMIC_ATTENDANCE_VERIFIED' && (!log.content || log.content === 'لاگ ناشناخته')
                                                ? 'حضور غیاب تایید شد'
                                                : (log.content || '-')}
                                        </td>
                                        <td className="px-6 py-4 text-sm text-gray-500 whitespace-nowrap">
                                            <div className="flex items-center gap-1">
                                                <FaClock className="text-xs text-gray-400" />
                                                {formatTime(log.date)}
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            {/* صفحه‌بندی */}
            {!loading && totalPages > 1 && (
                <div className="px-6 py-4 border-t border-gray-200 flex flex-col sm:flex-row justify-between items-center gap-3">
                    <div className="text-sm text-gray-700">
                        نمایش {paginatedLogs.length} از {totalCount} لاگ
                    </div>
                    {renderPaginationButtons()}
                </div>
            )}
        </div>
    );
};

export default UserMeetingLogsList;