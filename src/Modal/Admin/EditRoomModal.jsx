// src/Modal/Admin/EditRoomModal.jsx
import React, { useState, useEffect, useRef, useMemo } from 'react';
import {
    FaTimes, FaUpload, FaImage, FaEdit, FaCheckCircle,
    FaExclamationTriangle, FaUserTie, FaSearch, FaChevronDown, FaTimesCircle, FaSpinner
} from 'react-icons/fa';
import { EditRoom, GetNajmBosses, SetRoomNajmBoss } from '../../api/AdminAPI';
import { useAuth } from '../../Tools/AuthProvider';
import { useToast } from '../../Tools/ToastProvider';

// ---------- Helpers ----------

// خواندن مقاوم فیلد از آبجکت: هر دو حالت camelCase و PascalCase را چک می‌کند
const pickStr = (obj, ...keys) => {
    if (!obj) return '';
    for (const k of keys) {
        const v = obj[k];
        if (v !== undefined && v !== null) return String(v);
    }
    return '';
};

// نرمال‌سازی متن برای جستجوی فارسی/انگلیسی
const normalizeText = (value) => {
    if (value === null || value === undefined) return '';
    return String(value)
        .trim()
        .toLowerCase()
        .replace(/\s+/g, ' ')
        .replace(/[\u064A\u0649]/g, '\u06CC') // ي / ى  ->  ی
        .replace(/\u0643/g, '\u06A9')          // ك      ->  ک
        .replace(/[\u06F0-\u06F9]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 0x06F0 + 0x30)) // ۰-۹
        .replace(/[\u0660-\u0669]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 0x0660 + 0x30)); // ٠-٩
};

// ساخت نام نمایشی از روی رکورد
const getBossDisplayName = (boss) => {
    if (!boss) return '';
    const full = pickStr(boss, 'fullName', 'FullName');
    if (full) return full;
    const first = pickStr(boss, 'firstName', 'FirstName');
    const last = pickStr(boss, 'lastname', 'lastName', 'Lastname', 'LastName');
    const combined = `${first} ${last}`.trim();
    if (combined) return combined;
    const id = pickStr(boss, 'najmId', 'NajmId');
    return id ? `شناسه نجم: ${id}` : '';
};

const getBossId = (boss) => pickStr(boss, 'najmId', 'NajmId');

// ---------- Component ----------

export default function EditRoomModal({
    isOpen,
    onClose,
    initialRoom = null, // { friendlyID, roomTitle, logo, najmIDBoss }
    onSuccess = null
}) {
    const { user } = useAuth();
    const toast = useToast();

    const [friendlyID, setFriendlyID] = useState('');
    const [roomTitle, setRoomTitle] = useState('');
    const [logoFile, setLogoFile] = useState(null);
    const [previewUrl, setPreviewUrl] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    // Najm Boss State
    const [najmBosses, setNajmBosses] = useState([]);
    const [isLoadingBosses, setIsLoadingBosses] = useState(false);
    const [selectedBossId, setSelectedBossId] = useState('');
    const [originalBossId, setOriginalBossId] = useState('');
    const [bossSearchQuery, setBossSearchQuery] = useState('');
    const [isBossDropdownOpen, setIsBossDropdownOpen] = useState(false);
    const dropdownRef = useRef(null);
    const searchInputRef = useRef(null);

    // Close dropdown on outside click
    useEffect(() => {
        function handleClickOutside(e) {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
                setIsBossDropdownOpen(false);
            }
        }
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    useEffect(() => {
        if (isOpen) {
            const fid = initialRoom?.friendlyID || initialRoom?.roomID || initialRoom?.id || '';
            const title = initialRoom?.roomTitle || initialRoom?.title || initialRoom?.name || '';
            const logo = initialRoom?.logo || initialRoom?.avatar || '';
            const bossId = initialRoom?.najmIDBoss || initialRoom?.najmID || '';

            setFriendlyID(fid);
            setRoomTitle(title);
            setPreviewUrl(logo);
            setLogoFile(null);
            setSelectedBossId(bossId ? String(bossId) : '');
            setOriginalBossId(bossId ? String(bossId) : '');
            setBossSearchQuery('');
            setIsBossDropdownOpen(false);
            setError('');
            setSuccessMessage('');

            loadBosses();
        }
    }, [isOpen, initialRoom]);

    const loadBosses = async () => {
        try {
            setIsLoadingBosses(true);
            const res = await GetNajmBosses();
            // حالت‌های ممکن پاسخ API را پوشش می‌دهیم
            let items = [];
            if (Array.isArray(res?.data)) {
                items = res.data;
            } else if (Array.isArray(res?.data?.data)) {
                items = res.data.data;
            } else if (Array.isArray(res?.data?.Data)) {
                items = res.data.Data;
            } else if (Array.isArray(res?.Data)) {
                items = res.Data;
            } else if (Array.isArray(res)) {
                items = res;
            }
            setNajmBosses(items);
        } catch (err) {
            console.error("Error loading Najm bosses:", err);
            setNajmBosses([]);
        } finally {
            setIsLoadingBosses(false);
        }
    };

    // فیلتر پیشرفته با useMemo - با تغییر bossSearchQuery دوباره محاسبه می‌شود
    const filteredBosses = useMemo(() => {
        const q = normalizeText(bossSearchQuery);
        if (!q) return najmBosses;

        return najmBosses.filter((b) => {
            const first = pickStr(b, 'firstName', 'FirstName');
            const last = pickStr(b, 'lastname', 'lastName', 'Lastname', 'LastName');
            const full = pickStr(b, 'fullName', 'FullName');
            const nId = pickStr(b, 'najmId', 'NajmId');
            const nc = pickStr(b, 'nationalCode', 'NationalCode');
            const bbb = pickStr(b, 'bbbID', 'bbbId', 'BbbID', 'BbbId');

            const haystacks = [
                full,
                first,
                last,
                `${first} ${last}`.trim(),
                `${last} ${first}`.trim(),
                nId,
                nc,
                bbb,
            ];

            return haystacks.some((h) => normalizeText(h).includes(q));
        });
    }, [najmBosses, bossSearchQuery]);


    if (!isOpen) return null;

    const handleFileChange = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            setLogoFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    const selectedBoss = najmBosses.find((b) => getBossId(b) === String(selectedBossId));

    const handleSelectBoss = (boss) => {
        setSelectedBossId(getBossId(boss));
        setIsBossDropdownOpen(false);
        setBossSearchQuery('');
    };

    const handleClearBoss = (e) => {
        e.stopPropagation();
        setSelectedBossId('');
        setBossSearchQuery('');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!friendlyID.trim()) {
            setError('شناسه اتاق (FriendlyID) الزامی است');
            return;
        }
        if (!roomTitle.trim()) {
            setError('عنوان اتاق (RoomTitle) الزامی است');
            return;
        }

        setIsSubmitting(true);
        setError('');
        setSuccessMessage('');

        try {
            const res = await EditRoom(friendlyID.trim(), roomTitle.trim(), logoFile, selectedBossId || null);
            if (!res || res.status !== 200) {
                const errText = res?.message || 'خطا در ویرایش اطلاعات اتاق';
                setError(errText);
                if (toast?.error) toast.error(errText);
                setIsSubmitting(false);
                return;
            }

            if (String(selectedBossId) !== String(originalBossId)) {
                try {
                    const bossRes = await SetRoomNajmBoss(friendlyID.trim(), selectedBossId || null);
                    if (bossRes && bossRes.status !== 200) {
                        const bossErr = bossRes?.message || 'خطا در استعلام یا ثبت مسئول در سامانه نجم';
                        setError(bossErr);
                        if (toast?.error) toast.error(bossErr);
                        setIsSubmitting(false);
                        return;
                    }
                } catch (bossErr) {
                    console.error("Error setting Najm boss:", bossErr);
                    const errMsg = bossErr.message || 'خطا در اعتبارسنجی مسئول با سامانه نجم';
                    setError(errMsg);
                    if (toast?.error) toast.error(errMsg);
                    setIsSubmitting(false);
                    return;
                }
            }

            const msg = 'ویرایش اتاق و تنظیم مسئول با موفقیت انجام شد';
            setSuccessMessage(msg);
            if (toast?.success) toast.success(msg);

            if (onSuccess) {
                onSuccess({
                    friendlyID,
                    roomTitle,
                    logo: previewUrl,
                    najmIDBoss: selectedBossId,
                    najmID: selectedBossId
                });
            }

            setTimeout(() => {
                onClose();
            }, 1000);
        } catch (err) {
            console.error("Error editing room:", err);
            const errText = err.message || 'خطا در ارتباط با سرور';
            setError(errText);
            if (toast?.error) toast.error(errText);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 dir-rtl font-sans">
            <div className="bg-white rounded-3xl shadow-2xl w-full max-w-lg overflow-hidden border border-gray-100 animate-in fade-in zoom-in duration-200">
                {/* هدر مدال */}
                <div className="bg-gradient-to-r from-[#0b3f32] to-[#1379ff] text-white px-6 py-4 flex justify-between items-center">
                    <div className="flex items-center gap-2">
                        <FaEdit className="text-xl text-[#30d4cc]" />
                        <h2 className="text-lg font-bold">ویرایش اطلاعات اتاق</h2>
                    </div>
                    <button
                        onClick={onClose}
                        disabled={isSubmitting}
                        className="text-white/80 hover:text-white transition-all hover:rotate-90 duration-200"
                    >
                        <FaTimes size={20} />
                    </button>
                </div>

                {/* فرم ویرایش */}
                <form onSubmit={handleSubmit} className="p-6 space-y-4 max-h-[85vh] overflow-y-auto">
                    {/* شناسه اتاق */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5">
                            شناسه اتاق (FriendlyID) <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="text"
                            value={friendlyID}
                            onChange={(e) => setFriendlyID(e.target.value)}
                            placeholder="مثلا: room-101"
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent text-left dir-ltr transition-all"
                            required
                            disabled={isSubmitting}
                        />
                    </div>

                    {/* عنوان اتاق */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5">
                            عنوان جدید اتاق (RoomTitle) <span className="text-red-500">*</span>
                        </label>
                        <input
                            type="text"
                            value={roomTitle}
                            onChange={(e) => setRoomTitle(e.target.value)}
                            placeholder="مثلا: کلاس فلسفه اسلامی"
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                            required
                            disabled={isSubmitting}
                        />
                    </div>

                    {/* انتخاب مسئول نجم */}
                    <div className="relative" ref={dropdownRef}>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5 flex items-center justify-between">
                            <span className="flex items-center gap-1.5">
                                <FaUserTie className="text-[#0b3f32]" />
                                مسئول گروه در سامانه نجم (NajmIDBoss)
                            </span>
                            {isLoadingBosses && (
                                <span className="text-xs text-blue-600 flex items-center gap-1">
                                    <FaSpinner className="animate-spin" /> در حال دریافت مسئولین...
                                </span>
                            )}
                        </label>

                        {/* فیلد نمایش انتخاب شده یا دکمه بازکردن دراپ‌دان */}
                        <div
                            onClick={() => {
                                if (isSubmitting) return;
                                setIsBossDropdownOpen((prev) => !prev);
                            }}
                            className="w-full min-h-[44px] px-3.5 py-2 border border-gray-300 rounded-xl bg-gray-50/70 hover:bg-white cursor-pointer transition-all flex items-center justify-between gap-2 focus-within:ring-2 focus-within:ring-[#30d4cc]"
                        >
                            {selectedBossId ? (
                                <div className="flex items-center gap-2 flex-wrap flex-1">
                                    <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-emerald-100 text-emerald-800 text-xs font-bold border border-emerald-300">
                                        <FaUserTie className="text-emerald-600" />
                                        <span>
                                            {selectedBoss
                                                ? getBossDisplayName(selectedBoss)
                                                : `شناسه نجم: ${selectedBossId}`}
                                        </span>
                                        <span className="text-emerald-600 font-mono text-[11px]">({selectedBossId})</span>
                                        <button
                                            type="button"
                                            onClick={handleClearBoss}
                                            className="mr-1 text-emerald-700 hover:text-red-600 transition"
                                            title="حذف مسئول"
                                        >
                                            <FaTimes size={12} />
                                        </button>
                                    </span>
                                </div>
                            ) : (
                                <span className="text-gray-400 text-sm">انتخاب مسئول از سامانه نجم (اختیاری)...</span>
                            )}
                            <FaChevronDown className={`text-gray-400 text-xs transition-transform duration-200 ${isBossDropdownOpen ? 'rotate-180' : ''}`} />
                        </div>

                        {/* دراپ‌دان */}
                        {isBossDropdownOpen && (
                            <div className="absolute top-full left-0 right-0 mt-1.5 bg-white rounded-2xl shadow-xl border border-gray-200 z-50 overflow-hidden">
                                {/* کادر جستجو */}
                                <div className="p-2.5 border-b border-gray-100 bg-gray-50 flex items-center gap-2">
                                    <FaSearch className="text-gray-400 text-sm shrink-0" />
                                    <input
                                        ref={searchInputRef}
                                        type="text"
                                        autoComplete="off"
                                        spellCheck={false}
                                        value={bossSearchQuery}
                                        onChange={(e) => setBossSearchQuery(e.target.value)}
                                        onMouseDown={(e) => e.stopPropagation()}
                                        onKeyDown={(e) => {
                                            // جلوگیری از submit شدن فرم هنگام Enter زدن در کادر جستجو
                                            if (e.key === 'Enter') e.preventDefault();
                                        }}
                                        placeholder="جستجو بر اساس نام، نام خانوادگی یا شناسه نجم..."
                                        className="w-full bg-transparent text-sm focus:outline-none placeholder-gray-400"
                                        autoFocus
                                    />
                                    {bossSearchQuery && (
                                        <button
                                            type="button"
                                            onClick={() => setBossSearchQuery('')}
                                            className="text-gray-400 hover:text-gray-600"
                                        >
                                            <FaTimesCircle size={14} />
                                        </button>
                                    )}
                                </div>

                                {/* لیست افراد */}
                                <div className="max-h-56 overflow-y-auto divide-y divide-gray-50 text-sm">
                                    <div
                                        onClick={() => { setSelectedBossId(''); setIsBossDropdownOpen(false); }}
                                        className="px-4 py-2.5 hover:bg-gray-100 cursor-pointer text-gray-500 flex items-center justify-between text-xs"
                                    >
                                        <span>-- بدون مسئول نجم (خالی) --</span>
                                        {!selectedBossId && <FaCheckCircle className="text-emerald-500" />}
                                    </div>

                                    {filteredBosses.length > 0 ? (
                                        filteredBosses.map((boss, idx) => {
                                            const bossId = getBossId(boss);
                                            const isSelected = String(bossId) === String(selectedBossId);
                                            const displayName = getBossDisplayName(boss);
                                            return (
                                                <div
                                                    key={`${bossId}-${idx}`} 
                                                    onClick={() => handleSelectBoss(boss)}
                                                    className={`px-4 py-2.5 hover:bg-emerald-50 cursor-pointer flex items-center justify-between transition-colors ${isSelected ? 'bg-emerald-50/80 font-bold text-emerald-900' : 'text-gray-700'}`}
                                                >
                                                    <div className="flex items-center gap-2">
                                                        <span className="w-6 h-6 rounded-full bg-gray-100 flex items-center justify-center text-xs text-gray-500">
                                                            <FaUserTie size={11} />
                                                        </span>
                                                        <span>{displayName}</span>
                                                    </div>
                                                    <div className="flex items-center gap-2">
                                                        <span className="text-[11px] font-mono px-2 py-0.5 rounded bg-gray-100 text-gray-600">
                                                            {bossId}
                                                        </span>
                                                        {isSelected && <FaCheckCircle className="text-emerald-600" size={14} />}
                                                    </div>
                                                </div>
                                            );
                                        })
                                    ) : (
                                        <div className="p-4 text-center text-xs text-gray-400">
                                            {isLoadingBosses ? 'در حال بارگیری مسئولین...' : 'موردی یافت نشد'}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>

                    {/* لوگو */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-1.5">
                            لوگو / تصویر جدید اتاق (LogoFile)
                        </label>
                        <div className="flex items-center gap-4">
                            <div className="w-16 h-16 rounded-2xl bg-gray-100 border border-gray-200 flex items-center justify-center overflow-hidden shrink-0">
                                {previewUrl ? (
                                    <img src={previewUrl} alt="پیش‌نمایش لوگو" className="w-full h-full object-cover" />
                                ) : (
                                    <FaImage className="text-2xl text-gray-400" />
                                )}
                            </div>
                            <label className="flex-1 cursor-pointer">
                                <div className="border-2 border-dashed border-gray-300 hover:border-[#30d4cc] rounded-xl p-3 text-center transition-all bg-gray-50 hover:bg-gray-100 flex items-center justify-center gap-2 text-sm text-gray-600">
                                    <FaUpload className="text-[#30d4cc]" />
                                    <span>{logoFile ? logoFile.name : 'انتخاب تصویر فایل...'}</span>
                                </div>
                                <input
                                    type="file"
                                    accept="image/*"
                                    onChange={handleFileChange}
                                    className="hidden"
                                    disabled={isSubmitting}
                                />
                            </label>
                        </div>
                    </div>

                    {error && (
                        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-2.5 rounded-xl text-xs sm:text-sm flex items-center gap-2">
                            <FaExclamationTriangle className="shrink-0" />
                            <span>{error}</span>
                        </div>
                    )}
                    {successMessage && (
                        <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-2.5 rounded-xl text-xs sm:text-sm flex items-center gap-2">
                            <FaCheckCircle className="shrink-0" />
                            <span>{successMessage}</span>
                        </div>
                    )}

                    <div className="flex items-center gap-3 pt-2">
                        <button
                            type="submit"
                            disabled={isSubmitting}
                            className="flex-1 py-3 rounded-xl bg-gradient-to-r from-[#0b3f32] to-[#30d4cc] text-white font-bold text-sm shadow-md hover:shadow-lg transition-all disabled:opacity-50"
                        >
                            {isSubmitting ? (
                                <span className="flex items-center justify-center gap-2">
                                    <span className="inline-block animate-spin rounded-full h-4 w-4 border-b-2 border-white"></span>
                                    در حال استعلام و به‌روزرسانی...
                                </span>
                            ) : (
                                'ذخیره تغییرات اتاق'
                            )}
                        </button>
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={isSubmitting}
                            className="px-5 py-3 rounded-xl bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold text-sm transition-all"
                        >
                            انصراف
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}