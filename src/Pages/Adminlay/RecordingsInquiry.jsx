import React, { useState, useEffect, useMemo, useCallback } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import {
    FaSearch,
    FaDatabase,
    FaPlay,
    FaCopy,
    FaCheckCircle,
    FaSpinner,
    FaTimes,
    FaDownload,
    FaCode,
    FaThLarge,
    FaColumns,
    FaExternalLinkAlt,
    FaCalendarAlt,
    FaClock,
    FaChalkboardTeacher,
    FaVideo,
    FaInfoCircle,
    FaShieldAlt,
    FaSync,
    FaChevronDown,
    FaChevronUp,
    FaFileAlt
} from "react-icons/fa";
import { useAuth } from "@/Tools/AuthProvider";
import { InquiryServerRecordings } from "@/api/AdminAPI";

// Helper for highlighting JSON syntax
function formatJsonWithHighlight(jsonObj) {
    if (!jsonObj) return "";
    const jsonStr = typeof jsonObj === "string" ? jsonObj : JSON.stringify(jsonObj, null, 2);
    // Escape HTML
    const escaped = jsonStr
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    // Replace JSON tokens with styled spans
    return escaped.replace(
        /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
        (match) => {
            let cls = "text-[#7c3aed]"; // number default (purple)
            if (/^"/.test(match)) {
                if (/:$/.test(match)) {
                    cls = "text-[#2563eb] font-semibold"; // key (blue)
                } else {
                    cls = "text-[#059669]"; // string (emerald)
                }
            } else if (/true|false/.test(match)) {
                cls = "text-[#d97706] font-bold"; // boolean (amber)
            } else if (/null/.test(match)) {
                cls = "text-[#6b7280] italic"; // null (gray)
            }
            return `<span class="${cls}">${match}</span>`;
        }
    );
}

export default function RecordingsInquiry() {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();

    // Query Inputs
    const initialMeetId = searchParams.get("meetid") || searchParams.get("meetId") || "";
    const [meetId, setMeetId] = useState(initialMeetId);
    const [stateFilter, setStateFilter] = useState("");
    const [viewMode, setViewMode] = useState("split"); // "cards" | "json" | "split"

    // Results State
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [data, setData] = useState(null);
    const [activeTab, setActiveTab] = useState("all");
    const [localFilter, setLocalFilter] = useState("");
    const [expandedMeta, setExpandedMeta] = useState({});
    const [copiedKey, setCopiedKey] = useState(null);
    const [lastQueryInfo, setLastQueryInfo] = useState(null);

    // Verify Owner Access
    const isOwner = useMemo(() => {
        if (!user) return false;
        if (user.username && user.username.toLowerCase() === "mohrez") return true;
        if (user.type === 503 || user.Type === 503) return true;
        if (user.role && (user.role.toLowerCase() === "owner" || user.role.toLowerCase() === "administrator")) return true;
        if (Array.isArray(user.permissions) && user.permissions.includes("manage_admins")) return true;
        return false;
    }, [user]);

    // Handle Copy to clipboard
    const handleCopy = (text, key) => {
        if (!text) return;
        navigator.clipboard?.writeText(text);
        setCopiedKey(key);
        setTimeout(() => setCopiedKey(null), 2200);
    };

    // Execute Inquiry
    const runInquiry = useCallback(async (isGetAll = false) => {
        setLoading(true);
        setError(null);
        try {
            const targetMeetId = isGetAll ? "" : meetId.trim();
            const res = await InquiryServerRecordings({
                meetId: targetMeetId,
                state: stateFilter,
                getAll: isGetAll
            });

            if (res && (res.status === 200 || res.message === "ok")) {
                const responseData = res.data || res;
                setData(responseData);
                setLastQueryInfo({
                    isGetAll,
                    meetId: targetMeetId,
                    state: stateFilter,
                    count: responseData?.recordings?.length || 0,
                    time: new Date().toLocaleTimeString("fa-IR")
                });

                // Update URL parameter
                if (!isGetAll && targetMeetId) {
                    setSearchParams({ meetid: targetMeetId });
                } else if (isGetAll) {
                    setSearchParams({ all: "true" });
                }
            } else {
                setError(res?.message || "خطا در دریافت اطلاعات از سرور");
            }
        } catch (err) {
            console.error("Error inquiring recordings:", err);
            setError(err.message || "خطای ناشناخته در اتصال به سرور");
        } finally {
            setLoading(false);
        }
    }, [meetId, stateFilter, setSearchParams]);

    // Auto-run if query param provided on load
    useEffect(() => {
        if (initialMeetId) {
            runInquiry(false);
        }
    }, []); // eslint-disable-line react-hooks/exhaustive-deps

    // Filter recordings in local list
    const filteredRecordings = useMemo(() => {
        if (!data?.recordings) return [];
        let list = [...data.recordings];

        if (activeTab === "published") {
            list = list.filter(r => r.published || r.state === "published");
        } else if (activeTab === "unpublished") {
            list = list.filter(r => !r.published || r.state !== "published");
        }

        if (localFilter.trim()) {
            const term = localFilter.trim().toLowerCase();
            list = list.filter(r =>
                (r.name && r.name.toLowerCase().includes(term)) ||
                (r.recordId && r.recordId.toLowerCase().includes(term)) ||
                (r.meetingId && r.meetingId.toLowerCase().includes(term)) ||
                (r.matchedMeeting?.title && r.matchedMeeting.title.toLowerCase().includes(term)) ||
                (r.matchedMeeting?.teacherName && r.matchedMeeting.teacherName.toLowerCase().includes(term)) ||
                (r.jalaliDate && r.jalaliDate.includes(term))
            );
        }

        return list;
    }, [data, activeTab, localFilter]);

    // Download JSON
    const handleDownloadJson = () => {
        if (!data) return;
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `recordings-inquiry-${meetId || "all"}-${Date.now()}.json`;
        a.click();
        URL.revokeObjectURL(url);
    };

    // Toggle metadata drawer
    const toggleMeta = (id) => {
        setExpandedMeta(prev => ({
            ...prev,
            [id]: !prev[id]
        }));
    };

    if (!isOwner) {
        return (
            <div className="min-h-[70vh] flex items-center justify-center p-6" dir="rtl">
                <div className="max-w-md w-full bg-white border border-red-200 rounded-3xl p-8 shadow-xl text-center space-y-4">
                    <div className="w-16 h-16 bg-red-100 text-red-600 rounded-2xl flex items-center justify-center mx-auto shadow-xs">
                        <FaShieldAlt size={30} />
                    </div>
                    <h2 className="text-xl font-black text-gray-900">دسترسی اختصاصی مالک (Owner)</h2>
                    <p className="text-sm text-gray-600 leading-relaxed">
                        این صفحه و ابزار استعلام مستقیم وب‌سرویس BigBlueButton تنها در اختیار مالک کل سامانه (SuperAdmin) قرار دارد.
                    </p>
                    <button
                        onClick={() => navigate("/adminlay")}
                        className="px-6 py-2.5 bg-gray-900 hover:bg-black text-white rounded-xl text-xs font-bold transition"
                    >
                        بازگشت به داشبورد مدیریت
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="space-y-6 text-right pb-12" dir="rtl">
            {/* Top Page Header */}
            <div className="bg-white rounded-3xl p-6 border border-gray-200/80 shadow-xs flex flex-wrap items-center justify-between gap-4">
                <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-2xl bg-gradient-to-tr from-rose-500 to-red-600 text-white flex items-center justify-center shadow-md shadow-red-200">
                        <FaDatabase size={22} />
                    </div>
                    <div>
                        <div className="flex items-center gap-2">
                            <h1 className="text-xl font-black text-gray-900">
                                استعلام و تحلیل فایل‌های ضبط سرور (BBB)
                            </h1>
                            <span className="px-2.5 py-0.5 rounded-full bg-rose-50 text-rose-700 border border-rose-200 text-[11px] font-bold">
                                دسترسی ویژه Owner
                            </span>
                        </div>
                        <p className="text-xs text-gray-500 mt-1">
                            استعلام بلادرنگ ضبط‌های بیگ‌بلو‌باتن با شناسه MeetID یا دریافت کلیه رکوردهای موجود در سرور
                        </p>
                    </div>
                </div>

                <div className="flex items-center gap-2">
                    <span className="text-xs text-gray-400 font-mono">
                        مسیر دسترسی: <strong className="text-gray-700">/admin/recordings-inquiry</strong>
                    </span>
                </div>
            </div>

            {/* Answer to user's question: Help & Technical Info Card */}
            <div className="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200/80 rounded-2xl p-4 sm:p-5 flex items-start gap-3.5 shadow-2xs">
                <div className="p-2 bg-blue-100 text-blue-600 rounded-xl shrink-0 mt-0.5">
                    <FaInfoCircle size={18} />
                </div>
                <div className="space-y-1.5 text-xs text-blue-900 leading-relaxed">
                    <h3 className="font-bold text-sm text-blue-950 flex items-center gap-2">
                        <span>راهنمای فنی: آیا راهی هست که بشه همه رکوردها رو یکجا گرفت؟</span>
                        <span className="px-2 py-0.5 bg-blue-600 text-white rounded-md text-[10px] font-bold">پاسخ: بله قطعی</span>
                    </h3>
                    <p>
                        در معماری و پروتکل رسمی <strong>BigBlueButton API</strong>، متد <code className="bg-blue-100/70 px-1.5 py-0.5 rounded text-blue-800 font-mono">getRecordings</code> به گونه‌ای طراحی شده که اگر هیچ پارامتری نظیر <code className="bg-blue-100/70 px-1 py-0.5 rounded text-blue-800 font-mono">meetingID</code> یا <code className="bg-blue-100/70 px-1 py-0.5 rounded text-blue-800 font-mono">recordID</code> به آن پاس داده نشود، سرور بیگ‌بلو‌باتن <strong>تمامی رکوردهای ضبط‌شده تاریخچه سرور</strong> را برمی‌گرداند.
                    </p>
                    <p className="text-blue-800 font-medium">
                        کافی است روی دکمه صورتی‌رنگ <strong>«دریافت کلیه رکوردهای سرور»</strong> کلیک نمایید تا بدون اعمال فیلتر شناسه، کل رکوردهای سرور استعلام و به تفکیک فرم و JSON خام نمایش داده شوند.
                    </p>
                </div>
            </div>

            {/* Inquiry Form */}
            <div className="bg-white rounded-3xl p-5 sm:p-6 border border-gray-200 shadow-sm space-y-4">
                <h3 className="text-sm font-bold text-gray-800 flex items-center gap-2">
                    <FaSearch className="text-red-500" />
                    <span>پارامترهای استعلام ضبط:</span>
                </h3>

                <div className="grid grid-cols-1 md:grid-cols-12 gap-3 items-center">
                    {/* MeetID Input */}
                    <div className="md:col-span-6 relative">
                        <input
                            type="text"
                            placeholder="شناسه جلسه (MeetID) مثلا: 62411a3b17be4b40b18e58275b6a70ac46a2dd69-1794916874982"
                            value={meetId}
                            onChange={(e) => setMeetId(e.target.value)}
                            onKeyDown={(e) => {
                                if (e.key === "Enter") runInquiry(false);
                            }}
                            className="w-full pr-10 pl-10 py-3 bg-gray-50 border border-gray-200 rounded-2xl text-xs font-mono text-gray-800 focus:outline-none focus:ring-2 focus:ring-red-500 focus:bg-white transition"
                        />
                        <FaVideo className="absolute right-3.5 top-3.5 text-gray-400 text-sm" />
                        {meetId && (
                            <button
                                type="button"
                                onClick={() => setMeetId("")}
                                className="absolute left-3.5 top-3.5 text-gray-400 hover:text-red-500 transition cursor-pointer"
                                title="پاک کردن"
                            >
                                <FaTimes size={13} />
                            </button>
                        )}
                    </div>

                    {/* State Selector */}
                    <div className="md:col-span-2">
                        <select
                            value={stateFilter}
                            onChange={(e) => setStateFilter(e.target.value)}
                            className="w-full py-3 px-3 bg-gray-50 border border-gray-200 rounded-2xl text-xs font-bold text-gray-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:bg-white transition"
                        >
                            <option value="">همه وضعیت‌ها (State)</option>
                            <option value="published">منتشر شده (published)</option>
                            <option value="unpublished">منتشر نشده (unpublished)</option>
                            <option value="deleted">حذف شده (deleted)</option>
                            <option value="processing">در حال پردازش (processing)</option>
                        </select>
                    </div>

                    {/* Action Button 1: Inquiry with MeetID */}
                    <div className="md:col-span-2">
                        <button
                            type="button"
                            disabled={loading || !meetId.trim()}
                            onClick={() => runInquiry(false)}
                            className="w-full py-3 px-4 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 active:scale-[0.99] text-white font-bold text-xs rounded-2xl shadow-sm hover:shadow transition flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {loading && !lastQueryInfo?.isGetAll ? (
                                <FaSpinner className="animate-spin text-white" />
                            ) : (
                                <FaSearch size={12} />
                            )}
                            <span>استعلام با شناسه</span>
                        </button>
                    </div>

                    {/* Action Button 2: Get ALL recordings */}
                    <div className="md:col-span-2">
                        <button
                            type="button"
                            disabled={loading}
                            onClick={() => runInquiry(true)}
                            className="w-full py-3 px-3 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-700 hover:to-indigo-700 active:scale-[0.99] text-white font-bold text-xs rounded-2xl shadow-sm hover:shadow transition flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
                            title="دریافت کلیه رکوردهای موجود روی سرور بیگ‌بلو‌باتن بدون محدودیت شناسه"
                        >
                            {loading && lastQueryInfo?.isGetAll ? (
                                <FaSpinner className="animate-spin text-white" />
                            ) : (
                                <FaDatabase size={12} />
                            )}
                            <span>دریافت کلیه رکوردها</span>
                        </button>
                    </div>
                </div>

                {lastQueryInfo && (
                    <div className="pt-2 flex flex-wrap items-center justify-between gap-2 text-[11px] text-gray-500 border-t border-gray-100">
                        <div className="flex items-center gap-2">
                            <span>آخرین استعلام:</span>
                            <span className="font-bold text-gray-800">
                                {lastQueryInfo.isGetAll ? "دریافت کل رکوردهای سرور" : `شناسه: ${lastQueryInfo.meetId}`}
                            </span>
                            <span>• تعداد نتایج: <strong className="text-red-600">{lastQueryInfo.count} رکورد</strong></span>
                        </div>
                        <div className="flex items-center gap-2">
                            <span>زمان استعلام: {lastQueryInfo.time}</span>
                            <button
                                type="button"
                                onClick={() => runInquiry(lastQueryInfo.isGetAll)}
                                className="text-gray-600 hover:text-red-600 flex items-center gap-1 font-bold cursor-pointer"
                            >
                                <FaSync size={10} />
                                <span>بروزرسانی مجدد</span>
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {/* Error Message */}
            {error && (
                <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-2xl text-xs font-bold text-center">
                    {error}
                </div>
            )}

            {/* Results Section */}
            {data && (
                <div className="space-y-4">
                    {/* View Controls & Stats Bar */}
                    <div className="bg-white rounded-2xl p-4 border border-gray-200 flex flex-wrap items-center justify-between gap-3 shadow-2xs">
                        {/* Tabs for filtered view */}
                        <div className="flex items-center gap-1.5 bg-gray-100 p-1 rounded-xl">
                            <button
                                type="button"
                                onClick={() => setActiveTab("all")}
                                className={`px-3 py-1.5 rounded-lg text-xs font-bold transition cursor-pointer ${activeTab === "all" ? "bg-white text-gray-900 shadow-2xs" : "text-gray-600 hover:text-gray-900"}`}
                            >
                                همه ({data.recordings?.length || 0})
                            </button>
                            <button
                                type="button"
                                onClick={() => setActiveTab("published")}
                                className={`px-3 py-1.5 rounded-lg text-xs font-bold transition cursor-pointer ${activeTab === "published" ? "bg-white text-emerald-700 shadow-2xs" : "text-gray-600 hover:text-emerald-700"}`}
                            >
                                منتشر شده
                            </button>
                            <button
                                type="button"
                                onClick={() => setActiveTab("unpublished")}
                                className={`px-3 py-1.5 rounded-lg text-xs font-bold transition cursor-pointer ${activeTab === "unpublished" ? "bg-white text-amber-700 shadow-2xs" : "text-gray-600 hover:text-amber-700"}`}
                            >
                                غیرمنتشر / سایر
                            </button>
                        </div>

                        {/* Search Filter inside loaded results */}
                        <div className="relative min-w-[220px] flex-1 max-w-sm">
                            <input
                                type="text"
                                placeholder="فیلتر در نتایج (عنوان، شناسه، استاد)..."
                                value={localFilter}
                                onChange={(e) => setLocalFilter(e.target.value)}
                                className="w-full pr-8 pl-3 py-1.5 bg-gray-50 border border-gray-200 rounded-xl text-xs font-medium focus:outline-none focus:ring-1 focus:ring-red-500 focus:bg-white"
                            />
                            <FaSearch className="absolute right-2.5 top-2.5 text-gray-400 text-xs" />
                        </div>

                        {/* Layout View Mode Switcher */}
                        <div className="flex items-center gap-1">
                            <div className="flex items-center gap-1 bg-gray-100 p-1 rounded-xl">
                                <button
                                    type="button"
                                    onClick={() => setViewMode("cards")}
                                    className={`px-3 py-1.5 rounded-lg text-xs font-bold flex items-center gap-1.5 transition cursor-pointer ${viewMode === "cards" ? "bg-white text-red-600 shadow-2xs" : "text-gray-600 hover:text-gray-900"}`}
                                    title="نمای کارتی فرم"
                                >
                                    <FaThLarge size={12} />
                                    <span>کارت‌ها</span>
                                </button>
                                <button
                                    type="button"
                                    onClick={() => setViewMode("split")}
                                    className={`px-3 py-1.5 rounded-lg text-xs font-bold flex items-center gap-1.5 transition cursor-pointer ${viewMode === "split" ? "bg-white text-purple-600 shadow-2xs" : "text-gray-600 hover:text-gray-900"}`}
                                    title="نمای دوتایی همزمان (کارت + جیسون)"
                                >
                                    <FaColumns size={12} />
                                    <span>ترکیبی (Split)</span>
                                </button>
                                <button
                                    type="button"
                                    onClick={() => setViewMode("json")}
                                    className={`px-3 py-1.5 rounded-lg text-xs font-bold flex items-center gap-1.5 transition cursor-pointer ${viewMode === "json" ? "bg-white text-blue-600 shadow-2xs" : "text-gray-600 hover:text-gray-900"}`}
                                    title="نمای متن خام JSON"
                                >
                                    <FaCode size={12} />
                                    <span>جیسون خام</span>
                                </button>
                            </div>

                            {/* Download & Copy Full JSON */}
                            <button
                                type="button"
                                onClick={() => handleCopy(JSON.stringify(data, null, 2), "full-json")}
                                className="p-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition cursor-pointer"
                                title="کپی کل آبجکت JSON"
                            >
                                {copiedKey === "full-json" ? (
                                    <FaCheckCircle className="text-emerald-600" size={14} />
                                ) : (
                                    <FaCopy size={14} />
                                )}
                            </button>
                            <button
                                type="button"
                                onClick={handleDownloadJson}
                                className="p-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition cursor-pointer"
                                title="دانلود فایل JSON"
                            >
                                <FaDownload size={14} />
                            </button>
                        </div>
                    </div>

                    {/* Main Layout Grid */}
                    <div className={`grid gap-4 ${viewMode === "split" ? "grid-cols-1 lg:grid-cols-12" : "grid-cols-1"}`}>
                        {/* 1. Form & Cards Panel */}
                        {(viewMode === "cards" || viewMode === "split") && (
                            <div className={`${viewMode === "split" ? "lg:col-span-7" : "w-full"} space-y-3`}>
                                <div className="flex items-center justify-between px-1">
                                    <h4 className="text-xs font-bold text-gray-700">
                                        لیست رکوردهای استخراج‌شده ({filteredRecordings.length} مورد):
                                    </h4>
                                    <span className="text-[11px] text-gray-400">
                                        مرتب‌شده بر اساس شناسه و زمان ضبط
                                    </span>
                                </div>

                                {filteredRecordings.length === 0 ? (
                                    <div className="bg-white rounded-3xl p-12 text-center border border-gray-200">
                                        <FaVideo className="text-gray-300 text-4xl mx-auto mb-2" />
                                        <h5 className="text-sm font-bold text-gray-700">رکوردی یافت نشد</h5>
                                        <p className="text-xs text-gray-400 mt-1">
                                            هیچ فایلی با این فیلترها روی سرور ثبت نشده است.
                                        </p>
                                    </div>
                                ) : (
                                    filteredRecordings.map((r, idx) => {
                                        const isMetaOpen = !!expandedMeta[r.recordId || idx];
                                        return (
                                            <div
                                                key={r.recordId || idx}
                                                className="bg-white rounded-2xl border border-gray-200 p-4 hover:border-red-300 hover:shadow-sm transition space-y-3"
                                            >
                                                {/* Card Header: Title & Badges */}
                                                <div className="flex flex-wrap items-center justify-between gap-2">
                                                    <div className="flex items-center gap-2">
                                                        <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
                                                        <h4 className="font-extrabold text-sm text-gray-900">
                                                            {r.name || `رکورد #${idx + 1}`}
                                                        </h4>
                                                    </div>
                                                    <div className="flex items-center gap-1.5">
                                                        <span
                                                            className={`text-[11px] font-bold px-2.5 py-0.5 rounded-full border ${r.published ? "bg-emerald-50 text-emerald-700 border-emerald-200" : "bg-amber-50 text-amber-700 border-amber-200"}`}
                                                        >
                                                            {r.published ? "منتشر شده (Published)" : "منتشر نشده (Unpublished)"}
                                                        </span>
                                                        <span className="text-[10px] bg-gray-100 text-gray-600 px-2 py-0.5 rounded-md font-mono">
                                                            {r.playbackFormat || "presentation"}
                                                        </span>
                                                    </div>
                                                </div>

                                                {/* Identifier Details with Copy Buttons */}
                                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs bg-gray-50 p-2.5 rounded-xl border border-gray-100 font-mono">
                                                    <div className="flex items-center justify-between gap-1 overflow-hidden">
                                                        <span className="text-gray-500 text-[11px] shrink-0 font-sans">RecordID:</span>
                                                        <span className="truncate text-gray-800 text-[11px]" title={r.recordId}>
                                                            {r.recordId}
                                                        </span>
                                                        <button
                                                            type="button"
                                                            onClick={() => handleCopy(r.recordId, `rec-${idx}`)}
                                                            className="text-gray-400 hover:text-gray-800 p-1 shrink-0"
                                                            title="کپی RecordID"
                                                        >
                                                            {copiedKey === `rec-${idx}` ? (
                                                                <FaCheckCircle className="text-emerald-600" size={12} />
                                                            ) : (
                                                                <FaCopy size={11} />
                                                            )}
                                                        </button>
                                                    </div>

                                                    <div className="flex items-center justify-between gap-1 overflow-hidden">
                                                        <span className="text-gray-500 text-[11px] shrink-0 font-sans">MeetingID:</span>
                                                        <span className="truncate text-gray-800 text-[11px]" title={r.meetingId}>
                                                            {r.meetingId}
                                                        </span>
                                                        <button
                                                            type="button"
                                                            onClick={() => handleCopy(r.meetingId, `meet-${idx}`)}
                                                            className="text-gray-400 hover:text-gray-800 p-1 shrink-0"
                                                            title="کپی MeetingID"
                                                        >
                                                            {copiedKey === `meet-${idx}` ? (
                                                                <FaCheckCircle className="text-emerald-600" size={12} />
                                                            ) : (
                                                                <FaCopy size={11} />
                                                            )}
                                                        </button>
                                                    </div>
                                                </div>

                                                {/* Matched Local Meeting in Database (If exists) */}
                                                {r.matchedMeeting && (
                                                    <div className="bg-indigo-50/70 border border-indigo-100 rounded-xl p-2.5 text-xs text-indigo-900 flex flex-wrap items-center justify-between gap-2">
                                                        <div className="flex items-center gap-2">
                                                            <span className="px-1.5 py-0.5 rounded bg-indigo-200 text-indigo-800 font-bold text-[10px]">
                                                                دیتابیس #{r.matchedMeeting.id}
                                                            </span>
                                                            <strong className="text-indigo-950 font-bold">
                                                                اتاق: {r.matchedMeeting.roomTitle || r.matchedMeeting.roomFriendlyId}
                                                            </strong>
                                                            <span>• جلسه: {r.matchedMeeting.title}</span>
                                                        </div>
                                                        <div className="flex items-center gap-1.5 text-[11px] text-indigo-700">
                                                            <FaChalkboardTeacher size={12} />
                                                            <span>استاد: {r.matchedMeeting.teacherName}</span>
                                                        </div>
                                                    </div>
                                                )}

                                                {/* Stats Row: Date, Duration, Size */}
                                                <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 text-xs text-gray-600 bg-white p-2 border border-gray-100 rounded-xl">
                                                    <div className="flex items-center gap-1.5">
                                                        <FaCalendarAlt className="text-blue-500 shrink-0" size={12} />
                                                        <span>تاریخ:</span>
                                                        <strong className="text-gray-900 font-mono">{r.jalaliDate || "-"}</strong>
                                                    </div>
                                                    <div className="flex items-center gap-1.5">
                                                        <FaClock className="text-emerald-500 shrink-0" size={12} />
                                                        <span>زمان:</span>
                                                        <strong className="text-gray-900 font-mono">
                                                            {r.startTimeStr && r.startTimeStr !== "-" ? r.startTimeStr : "-"}
                                                        </strong>
                                                    </div>
                                                    <div className="flex items-center gap-1.5">
                                                        <span className="text-purple-500 font-bold shrink-0">⏳</span>
                                                        <span>مدت:</span>
                                                        <strong className="text-gray-900 font-mono">{r.formattedDuration || "-"}</strong>
                                                    </div>
                                                    <div className="flex items-center gap-1.5">
                                                        <span className="text-amber-500 font-bold shrink-0">💾</span>
                                                        <span>حجم:</span>
                                                        <strong className="text-gray-900 font-mono">{r.formattedSize || "-"}</strong>
                                                    </div>
                                                </div>

                                                {/* Actions Row */}
                                                <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-gray-100">
                                                    <div className="flex items-center gap-2">
                                                        {r.playbackUrl && (
                                                            <a
                                                                href={r.playbackUrl}
                                                                target="_blank"
                                                                rel="noopener noreferrer"
                                                                className="px-4 py-2 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-bold text-xs rounded-xl shadow-xs transition flex items-center gap-1.5 cursor-pointer"
                                                                title="مشاهده مستقیم ضبط جلسه"
                                                            >
                                                                <FaPlay size={10} />
                                                                <span>پخش جلسه ضبط‌شده</span>
                                                                <FaExternalLinkAlt size={9} />
                                                            </a>
                                                        )}
                                                        {r.playbackUrl && (
                                                            <button
                                                                type="button"
                                                                onClick={() => handleCopy(r.playbackUrl, `url-${idx}`)}
                                                                className="px-3 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition flex items-center gap-1.5 cursor-pointer"
                                                                title="کپی لینک بازپخش"
                                                            >
                                                                {copiedKey === `url-${idx}` ? (
                                                                    <>
                                                                        <FaCheckCircle className="text-emerald-600" size={12} />
                                                                        <span className="text-emerald-700">کپی شد</span>
                                                                    </>
                                                                ) : (
                                                                    <>
                                                                        <FaCopy size={12} />
                                                                        <span>کپی لینک</span>
                                                                    </>
                                                                )}
                                                            </button>
                                                        )}
                                                    </div>

                                                    {/* Toggle Metadata Drawer */}
                                                    {r.metadata && Object.keys(r.metadata).length > 0 && (
                                                        <button
                                                            type="button"
                                                            onClick={() => toggleMeta(r.recordId || idx)}
                                                            className="px-3 py-1.5 text-xs text-gray-500 hover:text-gray-900 font-medium flex items-center gap-1 cursor-pointer"
                                                        >
                                                            <span>متادیتا ({Object.keys(r.metadata).length})</span>
                                                            {isMetaOpen ? <FaChevronUp size={10} /> : <FaChevronDown size={10} />}
                                                        </button>
                                                    )}
                                                </div>

                                                {/* Expanded Metadata Table */}
                                                {isMetaOpen && r.metadata && (
                                                    <div className="mt-2 p-3 bg-gray-50 rounded-xl border border-gray-200 text-xs space-y-1.5">
                                                        <p className="font-bold text-gray-700 text-[11px]">فیلدهای متادیتای BigBlueButton:</p>
                                                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-1.5 font-mono text-[11px]">
                                                            {Object.entries(r.metadata).map(([mK, mV]) => (
                                                                <div key={mK} className="p-1.5 bg-white rounded border border-gray-100 flex items-center justify-between gap-2 overflow-hidden">
                                                                    <span className="text-blue-700 font-bold shrink-0">{mK}:</span>
                                                                    <span className="truncate text-gray-700" title={String(mV)}>
                                                                        {String(mV)}
                                                                    </span>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    </div>
                                                )}
                                            </div>
                                        );
                                    })
                                )}
                            </div>
                        )}

                        {/* 2. Clean Formatted JSON Panel */}
                        {(viewMode === "json" || viewMode === "split") && (
                            <div className={`${viewMode === "split" ? "lg:col-span-5" : "w-full"} space-y-2`}>
                                <div className="flex items-center justify-between px-1">
                                    <h4 className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
                                        <FaCode className="text-blue-600" />
                                        <span>محتوای تمیز و ساختاریافته JSON:</span>
                                    </h4>
                                    <div className="flex items-center gap-1.5">
                                        <button
                                            type="button"
                                            onClick={() => handleCopy(JSON.stringify(data, null, 2), "pane-json")}
                                            className="px-2.5 py-1 bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 rounded-lg text-xs font-bold transition flex items-center gap-1 cursor-pointer"
                                            title="کپی محتوای کامل"
                                        >
                                            {copiedKey === "pane-json" ? (
                                                <>
                                                    <FaCheckCircle className="text-emerald-600" size={11} />
                                                    <span className="text-emerald-700">کپی شد</span>
                                                </>
                                            ) : (
                                                <>
                                                    <FaCopy size={11} />
                                                    <span>کپی</span>
                                                </>
                                            )}
                                        </button>
                                        <button
                                            type="button"
                                            onClick={handleDownloadJson}
                                            className="px-2.5 py-1 bg-white border border-gray-200 hover:bg-gray-50 text-gray-700 rounded-lg text-xs font-bold transition flex items-center gap-1 cursor-pointer"
                                            title="دانلود فایل JSON"
                                        >
                                            <FaDownload size={11} />
                                            <span>دانلود</span>
                                        </button>
                                    </div>
                                </div>

                                <div className="bg-[#0f172a] text-[#f8fafc] rounded-2xl p-4 font-mono text-xs overflow-auto max-h-[750px] shadow-inner border border-slate-800" dir="ltr">
                                    <pre
                                        className="leading-relaxed select-text"
                                        dangerouslySetInnerHTML={{
                                            __html: formatJsonWithHighlight(data)
                                        }}
                                    />
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
