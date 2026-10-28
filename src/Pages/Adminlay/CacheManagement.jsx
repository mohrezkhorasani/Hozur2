import React, { useState, useEffect, useMemo } from 'react';
import {
    FaCloud, FaTrash, FaSync, FaSearch, FaDatabase, FaMicrochip,
    FaMemory, FaCheckSquare, FaSquare, FaEye, FaCopy, FaCheck,
    FaExclamationTriangle, FaShieldAlt, FaInfoCircle, FaBolt,
    FaArrowRight, FaLayerGroup
} from 'react-icons/fa';
import { GetCacheDetails, ClearCache, ReCache } from '../../api/AdminAPI';
import { useAuth } from '../../Tools/AuthProvider';
import { useToast } from '../../Tools/ToastProvider';

const CacheManagement = () => {
    const { user } = useAuth();
    const toast = useToast();

    // Data State
    const [stats, setStats] = useState(null);
    const [entries, setEntries] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Filter & Selection
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedKeys, setSelectedKeys] = useState(new Set());

    // Action Loaders
    const [actionLoadingKey, setActionLoadingKey] = useState(null);
    const [clearingAll, setClearingAll] = useState(false);
    const [recachingAll, setRecachingAll] = useState(false);
    const [batchActionLoading, setBatchActionLoading] = useState(false);

    // Modal State
    const [activeModalEntry, setActiveModalEntry] = useState(null);
    const [copiedKey, setCopiedKey] = useState(null);
    const [copiedJson, setCopiedJson] = useState(false);

    // Check superadmin permissions
    const isSuperAdmin = useMemo(() => {
        if (!user) return false;
        if (user.Type === 100 || user.Type === 500) return true; // USER_ADMINSTRATOR
        if (Array.isArray(user.permissions) && user.permissions.includes('manage_admins')) return true;
        return false;
    }, [user]);

    // Fetch cache details from server
    const fetchCacheData = async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await GetCacheDetails();
            if (res && res.status === 200 && res.data) {
                setStats(res.data.stats || {});
                setEntries(res.data.entries || []);
                setSelectedKeys(new Set());
            } else {
                setError(res?.message || 'خطا در دریافت اطلاعات کش');
            }
        } catch (err) {
            console.error('Error fetching cache data:', err);
            setError(err.message || 'خطا در برقراری ارتباط با سرور');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (isSuperAdmin) {
            fetchCacheData();
        }
    }, [isSuperAdmin]);

    // Filtered entries
    const filteredEntries = useMemo(() => {
        if (!searchTerm.trim()) return entries;
        const q = searchTerm.toLowerCase().trim();
        return entries.filter(e =>
            (e.key && e.key.toLowerCase().includes(q)) ||
            (e.typeName && e.typeName.toLowerCase().includes(q)) ||
            (e.memoryAddress && e.memoryAddress.toLowerCase().includes(q))
        );
    }, [entries, searchTerm]);

    // Selection handlers
    const handleToggleSelectAll = () => {
        if (selectedKeys.size === filteredEntries.length) {
            setSelectedKeys(new Set());
        } else {
            setSelectedKeys(new Set(filteredEntries.map(e => e.key)));
        }
    };

    const handleToggleKey = (key) => {
        const next = new Set(selectedKeys);
        if (next.has(key)) {
            next.delete(key);
        } else {
            next.add(key);
        }
        setSelectedKeys(next);
    };

    // Copy to clipboard helper
    const handleCopy = (text, type = 'key') => {
        if (!text) return;
        navigator.clipboard.writeText(text);
        if (type === 'key') {
            setCopiedKey(text);
            setTimeout(() => setCopiedKey(null), 2000);
        } else if (type === 'json') {
            setCopiedJson(true);
            setTimeout(() => setCopiedJson(false), 2000);
        }
    };

    // Actions
    const handleClearSingle = async (key) => {
        if (!window.confirm(`آیا از پاکسازی کلید کش "${key}" اطمینان دارید؟`)) return;
        setActionLoadingKey(key);
        try {
            const res = await ClearCache([key]);
            if (res && res.status === 200) {
                toast?.success ? toast.success(`کلید ${key} با موفقیت پاکسازی شد`) : alert('کلید با موفقیت پاکسازی شد');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در پاکسازی کش');
            }
        } catch (err) {
            alert(err.message || 'خطا در ارتباط با سرور');
        } finally {
            setActionLoadingKey(null);
        }
    };

    const handleReCacheSingle = async (key) => {
        setActionLoadingKey(key);
        try {
            const res = await ReCache([key]);
            if (res && res.status === 200) {
                toast?.success ? toast.success(`عملیات ری‌کش برای ${key} با موفقیت انجام شد`) : alert('عملیات ری‌کش با موفقیت انجام شد');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در بازسازی کش');
            }
        } catch (err) {
            alert(err.message || 'خطا در بازسازی کش');
        } finally {
            setActionLoadingKey(null);
        }
    };

    const handleClearSelected = async () => {
        if (selectedKeys.size === 0) return;
        const keysArr = Array.from(selectedKeys);
        if (!window.confirm(`آیا از پاکسازی ${keysArr.length} کلید کش انتخاب‌شده اطمینان دارید؟`)) return;

        setBatchActionLoading(true);
        try {
            const res = await ClearCache(keysArr);
            if (res && res.status === 200) {
                toast?.success ? toast.success(`${res.clearedCount || keysArr.length} کلید با موفقیت پاکسازی شد`) : alert('کلیدهای انتخابی پاکسازی شدند');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در پاکسازی');
            }
        } catch (err) {
            alert(err.message || 'خطا در پاکسازی کلیدهای انتخابی');
        } finally {
            setBatchActionLoading(false);
        }
    };

    const handleReCacheSelected = async () => {
        if (selectedKeys.size === 0) return;
        const keysArr = Array.from(selectedKeys);

        setBatchActionLoading(true);
        try {
            const res = await ReCache(keysArr);
            if (res && res.status === 200) {
                toast?.success ? toast.success(`${res.recachedCount || keysArr.length} کلید با موفقیت ری‌کش شد`) : alert('عملیات ری‌کش انجام شد');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در ری‌کش کلیدهای انتخابی');
            }
        } catch (err) {
            alert(err.message || 'خطا در ری‌کش کلیدها');
        } finally {
            setBatchActionLoading(false);
        }
    };

    const handleClearAll = async () => {
        if (!window.confirm('هشدار: آیا مطمئنید که می‌خواهید تمام کلیدهای کش سامانه را پاکسازی کنید؟')) return;
        setClearingAll(true);
        try {
            const res = await ClearCache(null);
            if (res && res.status === 200) {
                toast?.success ? toast.success('تمامی کلیدهای حافظه کش با موفقیت پاکسازی شدند') : alert('تمامی کش‌ها پاکسازی شدند');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در پاکسازی همه کش‌ها');
            }
        } catch (err) {
            alert(err.message || 'خطا در ارتباط با سرور');
        } finally {
            setClearingAll(false);
        }
    };

    const handleReCacheAll = async () => {
        if (!window.confirm('آیا مایلید تمام کش‌های ثبت‌شده سامانه مجدداً بارگذاری و Warm-Up شوند؟')) return;
        setRecachingAll(true);
        try {
            const res = await ReCache(null);
            if (res && res.status === 200) {
                toast?.success ? toast.success(`${res.recachedCount} کلید با موفقیت ری‌کش و تازه‌سازی شدند`) : alert('کلیه کش‌ها با موفقیت ری‌کش شدند');
                fetchCacheData();
            } else {
                alert(res?.message || 'خطا در ری‌کش عمومی');
            }
        } catch (err) {
            alert(err.message || 'خطا در ری‌کش');
        } finally {
            setRecachingAll(false);
        }
    };

    if (!isSuperAdmin) {
        return (
            <div className="min-h-[70vh] flex items-center justify-center p-6" dir="rtl">
                <div className="bg-red-50 border border-red-200 text-red-700 px-8 py-6 rounded-2xl max-w-lg text-center shadow-sm">
                    <FaShieldAlt className="w-16 h-16 text-red-500 mx-auto mb-4" />
                    <h2 className="text-xl font-bold mb-2">عدم دسترسی</h2>
                    <p className="text-sm text-red-600">
                        صفحه مدیریت کش و پایش حافظه موقت تنها مختص مدیر کل سامانه می‌باشد.
                    </p>
                </div>
            </div>
        );
    }

    return (
        <div className="container mx-auto px-4 py-8 max-w-7xl" dir="rtl">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
                <div>
                    <div className="flex items-center gap-3">
                        <div className="w-12 h-12 bg-teal-500 rounded-2xl flex items-center justify-center shadow-lg shadow-teal-500/20 text-white">
                            <FaCloud className="w-6 h-6" />
                        </div>
                        <div>
                            <h1 className="text-2xl font-bold text-gray-800">مدیریت کش و حافظه سیستم</h1>
                            <p className="text-sm text-gray-500 mt-1">
                                پایش لحظه‌ای IMemoryCache، مقادیر، آدرس حافظه و ابزارهای پاکسازی و ری‌کش
                            </p>
                        </div>
                    </div>
                </div>

                <div className="flex flex-wrap items-center gap-3">
                    <button
                        onClick={fetchCacheData}
                        disabled={loading}
                        className="flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 rounded-xl text-sm font-medium transition shadow-sm"
                        title="بروزرسانی وضعیت"
                    >
                        <FaSync className={`w-4 h-4 ${loading ? 'animate-spin text-teal-600' : 'text-gray-500'}`} />
                        <span>بروزرسانی</span>
                    </button>

                    <button
                        onClick={handleReCacheAll}
                        disabled={recachingAll || loading}
                        className="flex items-center gap-2 px-4 py-2.5 bg-teal-600 hover:bg-teal-700 text-white rounded-xl text-sm font-medium transition shadow-sm shadow-teal-600/20"
                    >
                        <FaBolt className={`w-4 h-4 ${recachingAll ? 'animate-spin' : ''}`} />
                        <span>ری‌کش همه (Warm-Up)</span>
                    </button>

                    <button
                        onClick={handleClearAll}
                        disabled={clearingAll || loading}
                        className="flex items-center gap-2 px-4 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-xl text-sm font-medium transition shadow-sm shadow-red-600/20"
                    >
                        <FaTrash className={`w-4 h-4 ${clearingAll ? 'animate-spin' : ''}`} />
                        <span>پاکسازی همه کش‌ها</span>
                    </button>
                </div>
            </div>

            {/* Stats Cards */}
            {stats && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
                    {/* Process Allocated Memory */}
                    <div className="bg-white rounded-2xl p-5 border border-gray-100 shadow-sm flex items-center justify-between">
                        <div>
                            <p className="text-xs font-medium text-gray-500 mb-1">حافظه کل اشغال‌شده (RAM)</p>
                            <h3 className="text-2xl font-bold text-gray-800 tracking-tight">
                                {stats.formattedTotalMemory || '۰ مگابایت'}
                            </h3>
                            <p className="text-[11px] text-teal-600 font-medium mt-1">تخصیص‌یافته توسط پروسه دات‌نت</p>
                        </div>
                        <div className="w-12 h-12 bg-teal-50 text-teal-600 rounded-2xl flex items-center justify-center">
                            <FaMicrochip className="w-6 h-6" />
                        </div>
                    </div>

                    {/* Total Cache Keys */}
                    <div className="bg-white rounded-2xl p-5 border border-gray-100 shadow-sm flex items-center justify-between">
                        <div>
                            <p className="text-xs font-medium text-gray-500 mb-1">تعداد کلیدهای فعال کش</p>
                            <h3 className="text-2xl font-bold text-gray-800 tracking-tight">
                                {stats.totalCacheEntriesCount ?? entries.length}
                            </h3>
                            <p className="text-[11px] text-blue-600 font-medium mt-1">آیتم‌های مقیم در IMemoryCache</p>
                        </div>
                        <div className="w-12 h-12 bg-blue-50 text-blue-600 rounded-2xl flex items-center justify-center">
                            <FaDatabase className="w-6 h-6" />
                        </div>
                    </div>

                    {/* Estimated Cache Size */}
                    <div className="bg-white rounded-2xl p-5 border border-gray-100 shadow-sm flex items-center justify-between">
                        <div>
                            <p className="text-xs font-medium text-gray-500 mb-1">حجم تخمینی داده‌های کش</p>
                            <h3 className="text-2xl font-bold text-gray-800 tracking-tight">
                                {stats.formattedTotalCacheSize || '۰ بایت'}
                            </h3>
                            <p className="text-[11px] text-amber-600 font-medium mt-1">مجموع حجم سریالایزشده آیتم‌ها</p>
                        </div>
                        <div className="w-12 h-12 bg-amber-50 text-amber-600 rounded-2xl flex items-center justify-center">
                            <FaMemory className="w-6 h-6" />
                        </div>
                    </div>

                    {/* GC Statistics */}
                    <div className="bg-white rounded-2xl p-5 border border-gray-100 shadow-sm flex items-center justify-between">
                        <div>
                            <p className="text-xs font-medium text-gray-500 mb-1">چرخه‌های Garbage Collection</p>
                            <div className="flex items-center gap-3 mt-1">
                                <span className="text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-mono">G0: {stats.gcGen0Collections}</span>
                                <span className="text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-mono">G1: {stats.gcGen1Collections}</span>
                                <span className="text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded font-mono">G2: {stats.gcGen2Collections}</span>
                            </div>
                            <p className="text-[11px] text-purple-600 font-medium mt-2">وضعیت بازیافت حافظه</p>
                        </div>
                        <div className="w-12 h-12 bg-purple-50 text-purple-600 rounded-2xl flex items-center justify-center">
                            <FaLayerGroup className="w-6 h-6" />
                        </div>
                    </div>
                </div>
            )}

            {/* Search & Batch Actions Toolbar */}
            <div className="bg-white rounded-2xl p-4 border border-gray-100 shadow-sm mb-6 flex flex-col md:flex-row items-center justify-between gap-4">
                <div className="relative w-full md:w-96">
                    <input
                        type="text"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                        placeholder="جستجو در کلیدها، نوع داده یا آدرس حافظه..."
                        className="w-full pl-4 pr-10 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-teal-500 focus:bg-white transition"
                    />
                    <FaSearch className="absolute right-3.5 top-3.5 text-gray-400 w-4 h-4" />
                </div>

                <div className="flex flex-wrap items-center gap-2 w-full md:w-auto justify-end">
                    {selectedKeys.size > 0 && (
                        <>
                            <span className="text-xs font-bold text-gray-600 ml-2">
                                {selectedKeys.size} مورد انتخاب شده
                            </span>
                            <button
                                onClick={handleReCacheSelected}
                                disabled={batchActionLoading}
                                className="flex items-center gap-1.5 px-3.5 py-2 bg-teal-50 hover:bg-teal-100 text-teal-700 border border-teal-200 rounded-xl text-xs font-bold transition"
                            >
                                <FaSync className={`w-3.5 h-3.5 ${batchActionLoading ? 'animate-spin' : ''}`} />
                                <span>ری‌کش موارد انتخابی</span>
                            </button>
                            <button
                                onClick={handleClearSelected}
                                disabled={batchActionLoading}
                                className="flex items-center gap-1.5 px-3.5 py-2 bg-red-50 hover:bg-red-100 text-red-700 border border-red-200 rounded-xl text-xs font-bold transition"
                            >
                                <FaTrash className={`w-3.5 h-3.5 ${batchActionLoading ? 'animate-spin' : ''}`} />
                                <span>پاکسازی انتخابی</span>
                            </button>
                        </>
                    )}
                </div>
            </div>

            {/* Error Message */}
            {error && (
                <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-xl flex items-center gap-3 text-red-700 text-sm">
                    <FaExclamationTriangle className="w-5 h-5 text-red-500 shrink-0" />
                    <span>{error}</span>
                </div>
            )}

            {/* Cache Entries Table */}
            <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-right border-collapse text-sm">
                        <thead>
                            <tr className="bg-gray-50/80 border-b border-gray-100 text-gray-600 font-bold text-xs uppercase tracking-wider">
                                <th className="p-4 w-12 text-center">
                                    <button
                                        type="button"
                                        onClick={handleToggleSelectAll}
                                        className="text-gray-400 hover:text-teal-600 transition"
                                    >
                                        {filteredEntries.length > 0 && selectedKeys.size === filteredEntries.length ? (
                                            <FaCheckSquare className="w-4 h-4 text-teal-600" />
                                        ) : (
                                            <FaSquare className="w-4 h-4" />
                                        )}
                                    </button>
                                </th>
                                <th className="p-4">کلید کش (Key)</th>
                                <th className="p-4">نوع داده</th>
                                <th className="p-4">آدرس خانه حافظه</th>
                                <th className="p-4">حجم تخمینی</th>
                                <th className="p-4">سیاست انقضا / اولویت</th>
                                <th className="p-4 text-center">عملیات</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {loading ? (
                                <tr>
                                    <td colSpan="7" className="text-center py-16">
                                        <div className="inline-flex items-center gap-3 text-gray-500">
                                            <FaSync className="w-5 h-5 animate-spin text-teal-600" />
                                            <span>در حال واکشی اطلاعات کش از حافظه...</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : filteredEntries.length === 0 ? (
                                <tr>
                                    <td colSpan="7" className="text-center py-16 text-gray-400">
                                        <FaInfoCircle className="w-10 h-10 mx-auto mb-2 text-gray-300" />
                                        {searchTerm ? 'هیچ کلیدی مطابق با جستجوی شما یافت نشد.' : 'در حال حاضر هیچ کلیدی در حافظه کش مقیم نیست.'}
                                    </td>
                                </tr>
                            ) : (
                                filteredEntries.map((entry) => {
                                    const isSelected = selectedKeys.has(entry.key);
                                    const isItemLoading = actionLoadingKey === entry.key;

                                    return (
                                        <tr
                                            key={entry.key}
                                            className={`hover:bg-gray-50/70 transition ${isSelected ? 'bg-teal-50/30' : ''}`}
                                        >
                                            {/* Checkbox */}
                                            <td className="p-4 text-center">
                                                <button
                                                    type="button"
                                                    onClick={() => handleToggleKey(entry.key)}
                                                    className="text-gray-400 hover:text-teal-600 transition"
                                                >
                                                    {isSelected ? (
                                                        <FaCheckSquare className="w-4 h-4 text-teal-600" />
                                                    ) : (
                                                        <FaSquare className="w-4 h-4" />
                                                    )}
                                                </button>
                                            </td>

                                            {/* Key */}
                                            <td className="p-4">
                                                <div className="flex items-center gap-2 max-w-xs sm:max-w-sm md:max-w-md">
                                                    <span className="font-mono font-bold text-gray-900 truncate" title={entry.key}>
                                                        {entry.key}
                                                    </span>
                                                    <button
                                                        onClick={() => handleCopy(entry.key, 'key')}
                                                        className="text-gray-400 hover:text-teal-600 transition shrink-0"
                                                        title="کپی نام کلید"
                                                    >
                                                        {copiedKey === entry.key ? (
                                                            <FaCheck className="w-3.5 h-3.5 text-teal-600" />
                                                        ) : (
                                                            <FaCopy className="w-3.5 h-3.5" />
                                                        )}
                                                    </button>
                                                </div>
                                            </td>

                                            {/* Type Name */}
                                            <td className="p-4">
                                                <span className="inline-block px-2.5 py-1 bg-gray-100 text-gray-700 rounded-lg text-xs font-mono font-medium truncate max-w-[180px]" title={entry.typeName}>
                                                    {entry.typeName}
                                                </span>
                                            </td>

                                            {/* Memory Address */}
                                            <td className="p-4">
                                                <span className="font-mono text-xs text-purple-700 bg-purple-50 px-2 py-0.5 rounded border border-purple-200/60 font-semibold" title="آدرس فیزیکی شیء در حافظه هیپ">
                                                    {entry.memoryAddress || '0x00000000'}
                                                </span>
                                            </td>

                                            {/* Estimated Size */}
                                            <td className="p-4">
                                                <span className="text-xs font-bold text-gray-700 font-mono">
                                                    {entry.formattedSize || '۰ بایت'}
                                                </span>
                                            </td>

                                            {/* Expiration & Priority */}
                                            <td className="p-4">
                                                <div className="flex flex-col gap-0.5">
                                                    <div className="flex items-center gap-1.5 text-xs text-gray-600">
                                                        <span className="font-medium">اولویت:</span>
                                                        <span className="font-mono text-[11px] text-gray-800 bg-gray-100 px-1.5 py-0.2 rounded">
                                                            {entry.priority || 'Normal'}
                                                        </span>
                                                    </div>
                                                    <span className="text-[11px] text-gray-400">
                                                        {entry.absoluteExpiration ? `انقضا: ${new Date(entry.absoluteExpiration).toLocaleTimeString('fa-IR')}` : 'انقضای نامحدود / پویا'}
                                                    </span>
                                                </div>
                                            </td>

                                            {/* Actions */}
                                            <td className="p-4 text-center">
                                                <div className="flex items-center justify-center gap-1.5">
                                                    {/* View JSON Value Button */}
                                                    <button
                                                        onClick={() => setActiveModalEntry(entry)}
                                                        className="p-2 text-blue-600 hover:bg-blue-50 rounded-xl transition"
                                                        title="مشاهده مقدار فعلی کش"
                                                    >
                                                        <FaEye className="w-4 h-4" />
                                                    </button>

                                                    {/* Re-Cache Button */}
                                                    {entry.canReCache && (
                                                        <button
                                                            onClick={() => handleReCacheSingle(entry.key)}
                                                            disabled={isItemLoading}
                                                            className="p-2 text-teal-600 hover:bg-teal-50 rounded-xl transition"
                                                            title="ری‌کش مجدد و تازه‌سازی"
                                                        >
                                                            <FaSync className={`w-4 h-4 ${isItemLoading ? 'animate-spin' : ''}`} />
                                                        </button>
                                                    )}

                                                    {/* Clear Button */}
                                                    <button
                                                        onClick={() => handleClearSingle(entry.key)}
                                                        disabled={isItemLoading}
                                                        className="p-2 text-red-500 hover:bg-red-50 rounded-xl transition"
                                                        title="پاکسازی این کلید"
                                                    >
                                                        <FaTrash className={`w-4 h-4 ${isItemLoading ? 'animate-spin' : ''}`} />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })
                            )}
                        </tbody>
                    </table>
                </div>

                {/* Table Footer */}
                <div className="p-4 bg-gray-50/50 border-t border-gray-100 flex items-center justify-between text-xs text-gray-500">
                    <span>
                        نمایش {filteredEntries.length} از {entries.length} کلید کش
                    </span>
                    <span className="font-mono">
                        مجموع حجم: {stats?.formattedTotalCacheSize || '۰ بایت'}
                    </span>
                </div>
            </div>

            {/* Modal: View Current Cache Value (JSON Viewer) */}
            {activeModalEntry && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-fade-in" dir="rtl">
                    <div className="bg-white w-full max-w-4xl max-h-[85vh] rounded-3xl shadow-2xl flex flex-col overflow-hidden border border-gray-100">
                        {/* Modal Header */}
                        <div className="p-6 border-b border-gray-100 flex items-center justify-between bg-gradient-to-r from-gray-50 to-white">
                            <div>
                                <div className="flex items-center gap-2">
                                    <span className="text-xs px-2.5 py-1 bg-teal-100 text-teal-800 rounded-lg font-bold">
                                        مقدار فعلی کش
                                    </span>
                                    <h3 className="text-lg font-bold text-gray-800 font-mono">
                                        {activeModalEntry.key}
                                    </h3>
                                </div>
                                <div className="flex items-center gap-4 mt-2 text-xs text-gray-500 font-mono">
                                    <span>نوع داده: {activeModalEntry.typeName}</span>
                                    <span>حجم: {activeModalEntry.formattedSize}</span>
                                    <span className="text-purple-600 font-bold">آدرس حافظه: {activeModalEntry.memoryAddress}</span>
                                </div>
                            </div>

                            <button
                                onClick={() => setActiveModalEntry(null)}
                                className="w-9 h-9 flex items-center justify-center rounded-xl bg-gray-100 hover:bg-gray-200 text-gray-500 transition text-sm font-bold"
                            >
                                ✕
                            </button>
                        </div>

                        {/* Modal Body: JSON View */}
                        <div className="p-6 flex-1 overflow-auto bg-gray-900 text-gray-100 text-xs font-mono rounded-b-none" dir="ltr">
                            <pre className="whitespace-pre-wrap break-all leading-relaxed">
                                {activeModalEntry.valueJson || 'null'}
                            </pre>
                        </div>

                        {/* Modal Footer */}
                        <div className="p-4 border-t border-gray-100 bg-white flex items-center justify-between">
                            <button
                                onClick={() => handleCopy(activeModalEntry.valueJson, 'json')}
                                className="flex items-center gap-2 px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                            >
                                {copiedJson ? (
                                    <>
                                        <FaCheck className="w-3.5 h-3.5 text-teal-600" />
                                        <span>کپی شد!</span>
                                    </>
                                ) : (
                                    <>
                                        <FaCopy className="w-3.5 h-3.5" />
                                        <span>کپی JSON</span>
                                    </>
                                )}
                            </button>

                            <div className="flex items-center gap-2">
                                {activeModalEntry.canReCache && (
                                    <button
                                        onClick={() => {
                                            handleReCacheSingle(activeModalEntry.key);
                                            setActiveModalEntry(null);
                                        }}
                                        className="flex items-center gap-1.5 px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-xl text-xs font-bold transition"
                                    >
                                        <FaSync className="w-3 h-3" />
                                        <span>ری‌کش مجدد</span>
                                    </button>
                                )}
                                <button
                                    onClick={() => setActiveModalEntry(null)}
                                    className="px-5 py-2 bg-gray-200 hover:bg-gray-300 text-gray-700 rounded-xl text-xs font-bold transition"
                                >
                                    بستن
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default CacheManagement;
