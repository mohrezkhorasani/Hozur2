// src/Pages/Admin/WebhooksManagement.jsx
import React, { useState, useEffect } from "react";
import {
    FaSearch, FaTrash, FaPlus, FaEye, FaClock, FaChevronLeft, FaChevronRight,
    FaTimesCircle, FaFilter, FaSpinner, FaLink, FaVideo, FaUsers,
    FaCalendarAlt, FaInfoCircle, FaCheckCircle, FaExclamationTriangle,
    FaEllipsisV, FaGlobe, FaRegClock, FaCircle, FaEdit, FaCopy,
    FaMicrophone, FaVideoSlash, FaShareAlt, FaFileAlt, FaDoorOpen, FaUserPlus, FaUserMinus,
    FaChalkboardTeacher, FaRecordVinyl, FaStop, FaPhone, FaPhoneSlash
} from "react-icons/fa";

// All available BBB webhook events
const ALL_EVENTS = [
    { id: "meeting-created", name: "جلسه ایجاد شد", icon: <FaVideo />, color: "bg-green-100 text-green-700", description: "زمانی که یک جلسه جدید ایجاد می‌شود" },
    { id: "meeting-destroyed", name: "جلسه حذف شد", icon: <FaTrash />, color: "bg-red-100 text-red-700", description: "زمانی که یک جلسه به طور کامل حذف می‌شود" },
    { id: "meeting-ended", name: "جلسه پایان یافت", icon: <FaClock />, color: "bg-orange-100 text-orange-700", description: "زمانی که یک جلسه به پایان می‌رسد" },
    { id: "user-joined", name: "کاربر وارد شد", icon: <FaUserPlus />, color: "bg-blue-100 text-blue-700", description: "زمانی که کاربر به جلسه می‌پیوندد" },
    { id: "user-left", name: "کاربر خارج شد", icon: <FaUserMinus />, color: "bg-yellow-100 text-yellow-700", description: "زمانی که کاربر از جلسه خارج می‌شود" },
    { id: "user-joined-voice", name: "ورود به صدا", icon: <FaMicrophone />, color: "bg-purple-100 text-purple-700", description: "زمانی که کاربر به میکروفون متصل می‌شود" },
    { id: "user-left-voice", name: "خروج از صدا", icon: <FaPhoneSlash />, color: "bg-pink-100 text-pink-700", description: "زمانی که کاربر از میکروفون جدا می‌شود" },
    { id: "user-shared-camera", name: "دوربین روشن شد", icon: <FaVideo />, color: "bg-indigo-100 text-indigo-700", description: "زمانی که کاربر دوربین خود را روشن می‌کند" },
    { id: "user-unshared-camera", name: "دوربین خاموش شد", icon: <FaVideoSlash />, color: "bg-gray-100 text-gray-700", description: "زمانی که کاربر دوربین خود را خاموش می‌کند" },
    { id: "user-started-sharing-screen", name: "اشتراک صفحه شروع شد", icon: <FaShareAlt />, color: "bg-cyan-100 text-cyan-700", description: "زمانی که کاربر صفحه خود را به اشتراک می‌گذارد" },
    { id: "user-stopped-sharing-screen", name: "اشتراک صفحه متوقف شد", icon: <FaShareAlt />, color: "bg-slate-100 text-slate-700", description: "زمانی که کاربر اشتراک صفحه را متوقف می‌کند" },
    { id: "recording-started", name: "ضبط شروع شد", icon: <FaRecordVinyl />, color: "bg-red-100 text-red-700", description: "زمانی که ضبط جلسه شروع می‌شود" },
    { id: "recording-stopped", name: "ضبط متوقف شد", icon: <FaStop />, color: "bg-amber-100 text-amber-700", description: "زمانی که ضبط جلسه متوقف می‌شود" },
    { id: "participant-joined", name: "شرکت‌کننده وارد شد", icon: <FaUsers />, color: "bg-emerald-100 text-emerald-700", description: "زمانی که یک شرکت‌کننده وارد جلسه می‌شود" },
    { id: "participant-left", name: "شرکت‌کننده خارج شد", icon: <FaUsers />, color: "bg-rose-100 text-rose-700", description: "زمانی که یک شرکت‌کننده از جلسه خارج می‌شود" }
];

// Predefined event groups
const EVENT_GROUPS = {
    all: { name: "همه رویدادها", events: ALL_EVENTS.map(e => e.id) },
    recording: { name: "رویدادهای ضبط", events: ["recording-started", "recording-stopped", "meeting-ended"] },
    participant: { name: "رویدادهای شرکت‌کنندگان", events: ["user-joined", "user-left", "participant-joined", "participant-left", "user-joined-voice", "user-left-voice"] },
    moderation: { name: "رویدادهای مدیریتی", events: ["meeting-created", "meeting-destroyed", "meeting-ended"] },
    media: { name: "رویدادهای رسانه‌ای", events: ["user-shared-camera", "user-unshared-camera", "user-started-sharing-screen", "user-stopped-sharing-screen"] }
};

import { GetAllRoomsMini } from "../../api/AdminAPI";
import {
    GetWebhooksList,
    DestroyWebhook,
    GetWebhookInfo,
    CreateWebhookWithEvents
} from "../../api/AdminAPI";

const WebhooksManagement = () => {
    const [webhooks, setWebhooks] = useState([]);
    const [filteredWebhooks, setFilteredWebhooks] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [selectedHook, setSelectedHook] = useState(null);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [deletingHookId, setDeletingHookId] = useState(null);
    const [rooms, setRooms] = useState([]);
    const [roomsLoading, setRoomsLoading] = useState(false);
    const [showActionMenu, setShowActionMenu] = useState(null);
    const [filterMeeting, setFilterMeeting] = useState("all");
    const [showFilters, setShowFilters] = useState(false);

    const itemsPerPage = 10;

    // Form state for creating webhook
    const [formData, setFormData] = useState({
        baseURL: "https://vc2.hekmateislami.com",
        actionPath: "",
        meetingID: "",
        hookType: "all",
        selectedEvents: []
    });

    // Load webhooks list
    const loadWebhooks = async () => {
        try {
            setLoading(true);
            const data = await GetWebhooksList();
            if (data && data.hooks) {
                setWebhooks(data.hooks);
                setFilteredWebhooks(data.hooks);
            }
        } catch (err) {
            console.error("Error loading webhooks:", err);
        } finally {
            setLoading(false);
        }
    };

    // Load rooms for dropdown
    const loadRooms = async () => {
        try {
            setRoomsLoading(true);
            const data = await GetAllRoomsMini();
            if (data && data.data) {
                setRooms(data.data);
            }
        } catch (err) {
            console.error("Error loading rooms:", err);
        } finally {
            setRoomsLoading(false);
        }
    };

    useEffect(() => {
        loadWebhooks();
        loadRooms();
        document.title = "مدیریت هوک‌ها | پنل ادمین";
    }, []);

    // Apply filters
    useEffect(() => {
        let filtered = [...webhooks];

        if (searchTerm) {
            filtered = filtered.filter(hook =>
                hook.callbackURL?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                hook.hookID?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                hook.meetingID?.toLowerCase().includes(searchTerm.toLowerCase())
            );
        }

        if (filterMeeting !== "all") {
            if (filterMeeting === "global") {
                filtered = filtered.filter(hook => !hook.meetingID);
            } else {
                filtered = filtered.filter(hook => hook.meetingID === filterMeeting);
            }
        }

        setFilteredWebhooks(filtered);
        setCurrentPage(1);
    }, [searchTerm, filterMeeting, webhooks]);

    const uniqueMeetings = [...new Map(webhooks
        .filter(hook => hook.meetingID)
        .map(hook => [hook.meetingID, hook.meetingID]))
        .values()];

    const totalPages = Math.ceil(filteredWebhooks.length / itemsPerPage);
    const paginatedHooks = filteredWebhooks.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    const resetFilters = () => {
        setSearchTerm("");
        setFilterMeeting("all");
        setShowFilters(false);
    };

    const getFullCallbackURL = () => {
        const base = formData.baseURL.trim();
        const action = formData.actionPath.trim();
        if (!base) return "";
        if (!action) return base;
        // Remove trailing slash from base if exists, and ensure action starts with /
        const cleanBase = base.replace(/\/$/, '');
        const cleanAction = action.startsWith('/') ? action : `/${action}`;
        return `${cleanBase}${cleanAction}`;
    };
    const handleCreateWebhook = async () => {
        const fullURL = getFullCallbackURL();
        if (!fullURL) {
            alert("لطفاً آدرس Base URL را وارد کنید");
            return;
        }

        try {
            setLoading(true);
            let result;
            const meetingID = formData.meetingID || null;
            let eventIDs = null; // پیش‌فرض null به معنی همه رویدادها

            // فقط برای حالت custom و سفارشی، eventIDs رو مقداردهی کن
            if (formData.hookType === "custom" && formData.selectedEvents.length > 0) {
                eventIDs = formData.selectedEvents;
            }
            // برای گروه‌های از پیش تعریف شده (recording, participant, moderation)
            else if (formData.hookType !== "all" && formData.hookType !== "custom") {
                const groupEvents = EVENT_GROUPS[formData.hookType]?.events;
                if (groupEvents && groupEvents.length > 0) {
                    eventIDs = groupEvents;
                }
            }
            // اگر hookType === "all" باشه، eventIDs = null می‌مونه (همه رویدادها)

            result = await CreateWebhookWithEvents(fullURL, eventIDs, meetingID);

            if (result) {
                alert("هوک با موفقیت ایجاد شد");
                setShowCreateModal(false);
                setFormData({
                    baseURL: "https://vc2.hekmateislami.com",
                    actionPath: "",
                    meetingID: "",
                    hookType: "all",
                    selectedEvents: []
                });
                loadWebhooks();
            }
        } catch (err) {
            console.error("Error creating webhook:", err);
            alert(err.message || "خطا در ایجاد هوک");
        } finally {
            setLoading(false);
        }
    };

    const handleDeleteWebhook = async () => {
        try {
            setLoading(true);
            await DestroyWebhook(deletingHookId);
            alert("هوک با موفقیت حذف شد");
            setShowDeleteModal(false);
            setDeletingHookId(null);
            loadWebhooks();
        } catch (err) {
            console.error("Error deleting webhook:", err);
            alert(err.message || "خطا در حذف هوک");
        } finally {
            setLoading(false);
        }
    };

    const handleViewDetails = async (hook) => {
        try {
            setLoading(true);
            // const details = await GetWebhookInfo(hook.hookID);
            const details = hook
            setSelectedHook(details);
            setShowDetailModal(true);
            setShowActionMenu(null);
        } catch (err) {
            console.error("Error fetching hook details:", err);
            alert(err.message || "خطا در دریافت اطلاعات هوک");
        } finally {
            setLoading(false);
        }
    };

    const copyToClipboard = async (text) => {
        try {
            await navigator.clipboard.writeText(text);
            alert("کپی شد!");
        } catch (err) {
            console.error("Failed to copy:", err);
        }
    };

    const getRoomTitle = (meetingID) => {
        const room = rooms.find(r => r.bbbMeetID === meetingID);
        return room ? room.title : meetingID || "همه جلسات";
    };

    const toggleEventSelection = (eventId) => {
        setFormData(prev => ({
            ...prev,
            selectedEvents: prev.selectedEvents.includes(eventId)
                ? prev.selectedEvents.filter(id => id !== eventId)
                : [...prev.selectedEvents, eventId]
        }));
    };

    const selectAllEvents = () => {
        setFormData(prev => ({
            ...prev,
            selectedEvents: ALL_EVENTS.map(e => e.id)
        }));
    };

    const deselectAllEvents = () => {
        setFormData(prev => ({
            ...prev,
            selectedEvents: []
        }));
    };

    if (loading && webhooks.length === 0) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
                <div className="container mx-auto px-4 py-8">
                    <div className="flex justify-center items-center h-96">
                        <div className="text-center">
                            <FaSpinner className="animate-spin text-5xl text-blue-600 mx-auto mb-4" />
                            <p className="text-gray-600">در حال دریافت اطلاعات هوک‌ها...</p>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100" dir="rtl">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">

                {/* Header */}
                <div className="mb-8">
                    <div className="flex justify-between items-center">
                        <div>
                            <h1 className="text-3xl font-bold text-gray-800 mb-2">مدیریت هوک‌های وب</h1>
                            <p className="text-gray-600">مدیریت Webhook‌های دریافت رویدادهای بیگ بلو باتن</p>
                        </div>
                        <button
                            onClick={() => {
                                setFormData({
                                    baseURL: "https://core.hekmateislami.com",
                                    actionPath: "",
                                    meetingID: "",
                                    hookType: "all",
                                    selectedEvents: []
                                });
                                setShowCreateModal(true);
                            }}
                            className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white px-5 py-2.5 rounded-xl transition-all flex items-center gap-2 shadow-lg"
                        >
                            <FaPlus className="text-sm" />
                            <span>هوک جدید</span>
                        </button>
                    </div>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">کل هوک‌ها</p>
                                <p className="text-2xl font-bold text-gray-800">{webhooks.length}</p>
                            </div>
                            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                                <FaLink className="text-blue-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">هوک‌های سراسری</p>
                                <p className="text-2xl font-bold text-green-600">{webhooks.filter(h => !h.meetingID).length}</p>
                            </div>
                            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                                <FaGlobe className="text-green-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">هوک‌های اختصاصی</p>
                                <p className="text-2xl font-bold text-orange-600">{webhooks.filter(h => h.meetingID).length}</p>
                            </div>
                            <div className="w-10 h-10 bg-orange-100 rounded-lg flex items-center justify-center">
                                <FaVideo className="text-orange-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">هوک‌های دائمی</p>
                                <p className="text-2xl font-bold text-purple-600">{webhooks.filter(h => h.permanent).length}</p>
                            </div>
                            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                                <FaCheckCircle className="text-purple-600" />
                            </div>
                        </div>
                    </div>
                </div>

                {/* Search and Filters */}
                <div className="mb-6 space-y-4">
                    <div className="flex gap-3">
                        <div className="flex-1 relative">
                            <FaSearch className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                            <input  
                                type="text"
                                placeholder="جستجو بر اساس آدرس، شناسه هوک یا شناسه جلسه..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="w-full pr-10 pl-4 py-2.5 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
                            />
                            {searchTerm && (
                                <button
                                    onClick={() => setSearchTerm("")}
                                    className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
                                >
                                    <FaTimesCircle className="w-4 h-4" />
                                </button>
                            )}
                        </div>
                        <button
                            onClick={() => setShowFilters(!showFilters)}
                            className={`px-4 py-2.5 rounded-xl border transition-all duration-200 flex items-center gap-2 ${showFilters || filterMeeting !== "all"
                                ? "bg-blue-600 text-white border-blue-600"
                                : "bg-white text-gray-700 border-gray-200 hover:border-blue-400"
                                }`}
                        >
                            <FaFilter className="text-sm" />
                            <span>فیلتر</span>
                        </button>
                        {filterMeeting !== "all" && (
                            <button
                                onClick={resetFilters}
                                className="px-4 py-2.5 rounded-xl border border-gray-200 bg-white text-gray-700 hover:bg-gray-50 transition-all flex items-center gap-2"
                            >
                                <FaTimesCircle className="text-sm" />
                                <span>حذف فیلترها</span>
                            </button>
                        )}
                    </div>

                    {/* Filter Panel */}
                    {showFilters && (
                        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
                            <div className="grid grid-cols-1 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">فیلتر بر اساس جلسه</label>
                                    <select
                                        value={filterMeeting}
                                        onChange={(e) => setFilterMeeting(e.target.value)}
                                        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                                    >
                                        <option value="all">همه جلسات</option>
                                        <option value="global">هوک‌های سراسری (بدون جلسه مشخص)</option>
                                        {uniqueMeetings.map((meeting) => (
                                            <option key={meeting} value={meeting}>
                                                {getRoomTitle(meeting)}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                        </div>
                    )}
                </div>

                {/* Webhooks Table */}
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead className="bg-gray-50 border-b border-gray-200">
                                <tr>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">شناسه هوک</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">آدرس Callback</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">جلسه</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">نوع</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">وضعیت</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {paginatedHooks.map((hook) => (
                                    <tr key={hook.hookID} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2">
                                                <span className="text-sm font-mono text-gray-600">{hook.hookID}</span>
                                                <button
                                                    onClick={() => copyToClipboard(hook.hookID)}
                                                    className="text-gray-400 hover:text-blue-600"
                                                >
                                                    <FaCopy className="w-3 h-3" />
                                                </button>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2">
                                                <span className="text-sm text-gray-600 max-w-md truncate">{hook.callbackURL}</span>
                                                <button
                                                    onClick={() => copyToClipboard(hook.callbackURL)}
                                                    className="text-gray-400 hover:text-blue-600"
                                                >
                                                    <FaCopy className="w-3 h-3" />
                                                </button>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className="text-sm text-gray-600">
                                                {getRoomTitle(hook.meetingID)}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4">
                                            {hook.permanent ? (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-purple-100 text-purple-700 text-xs rounded-full">
                                                    <FaCheckCircle className="w-2 h-2" />
                                                    دائمی
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-500 text-xs rounded-full">
                                                    معمولی
                                                </span>
                                            )}
                                            {hook.rawData && (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 bg-yellow-100 text-yellow-700 text-xs rounded-full mr-1">
                                                    Raw Data
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className="inline-flex items-center gap-1 px-2 py-1 bg-green-100 text-green-700 text-xs rounded-full">
                                                <FaCheckCircle className="w-2 h-2" />
                                                فعال
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm relative">
                                            <button
                                                onClick={() => {handleViewDetails(hook)}}
                                                className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                                            >
                                                <FaEllipsisV className="w-5 h-5 text-gray-500" />
                                            </button>

                                            {/* Action Menu Dropdown */}
                                            {showActionMenu === hook.hookID && (
                                                <>
                                                    <div
                                                        className="fixed inset-0 z-40"
                                                        onClick={() => setShowActionMenu(null)}
                                                    />
                                                    <div className="absolute left-0 mt-2 w-56 bg-white rounded-xl shadow-lg border border-gray-100 z-50 overflow-hidden">
                                                        <button
                                                            onClick={() => handleViewDetails(hook)}
                                                            className="w-full px-4 py-3 text-right hover:bg-blue-50 transition-colors flex items-center gap-3 border-b border-gray-50"
                                                        >
                                                            <FaEye className="w-4 h-4 text-blue-600" />
                                                            <span className="text-sm text-gray-700">مشاهده جزئیات</span>
                                                        </button>
                                                        {!hook.permanent && (
                                                            <button
                                                                onClick={() => {
                                                                    setDeletingHookId(hook.hookID);
                                                                    setShowDeleteModal(true);
                                                                    setShowActionMenu(null);
                                                                }}
                                                                className="w-full px-4 py-3 text-right hover:bg-red-50 transition-colors flex items-center gap-3"
                                                            >
                                                                <FaTrash className="w-4 h-4 text-red-600" />
                                                                <span className="text-sm text-gray-700">حذف هوک</span>
                                                            </button>
                                                        )}
                                                    </div>
                                                </>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    {filteredWebhooks.length === 0 && (
                        <div className="text-center py-16">
                            <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                                <FaSearch className="w-10 h-10 text-gray-400" />
                            </div>
                            <p className="text-gray-600 text-lg">نتیجه‌ای یافت نشد</p>
                            <p className="text-gray-400 text-sm mt-2">لطفاً عبارت جستجو را تغییر دهید</p>
                        </div>
                    )}

                    {/* Pagination */}
                    {totalPages > 1 && (
                        <div className="flex justify-center items-center gap-2 py-6 border-t border-gray-100">
                            <button
                                onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                                disabled={currentPage === 1}
                                className="p-2 rounded-lg border border-gray-200 bg-white text-gray-600 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                            >
                                <FaChevronRight className="w-5 h-5" />
                            </button>
                            <div className="flex gap-1">
                                {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                                    let pageNum;
                                    if (totalPages <= 5) {
                                        pageNum = i + 1;
                                    } else if (currentPage <= 3) {
                                        pageNum = i + 1;
                                    } else if (currentPage >= totalPages - 2) {
                                        pageNum = totalPages - 4 + i;
                                    } else {
                                        pageNum = currentPage - 2 + i;
                                    }
                                    return (
                                        <button
                                            key={pageNum}
                                            onClick={() => setCurrentPage(pageNum)}
                                            className={`min-w-[40px] h-10 rounded-lg transition-colors ${currentPage === pageNum
                                                ? "bg-blue-600 text-white"
                                                : "border border-gray-200 bg-white text-gray-600 hover:bg-gray-50"
                                                }`}
                                        >
                                            {pageNum}
                                        </button>
                                    );
                                })}
                            </div>
                            <button
                                onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                                disabled={currentPage === totalPages}
                                className="p-2 rounded-lg border border-gray-200 bg-white text-gray-600 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                            >
                                <FaChevronLeft className="w-5 h-5" />
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Create Webhook Modal - with semi-transparent background */}
            {showCreateModal && (
                <div className="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl max-w-4xl w-full my-8 max-h-[90vh] overflow-y-auto">
                        <div className="sticky top-0 bg-white z-10 flex justify-between items-center p-6 border-b border-gray-100">
                            <h2 className="text-xl font-bold text-gray-800">ایجاد هوک جدید</h2>
                            <button onClick={() => setShowCreateModal(false)} className="text-gray-400 hover:text-gray-600">
                                <FaTimesCircle className="w-6 h-6" />
                            </button>
                        </div>

                        <div className="p-6 space-y-6">
                            {/* URL Builder Section */}
                            <div className="bg-gray-50 rounded-xl p-4 border border-gray-200">
                                <label className="block text-sm font-medium text-gray-700 mb-3">
                                    آدرس Callback URL <span className="text-red-500">*</span>
                                </label>
                                <div className="flex flex-col md:flex-row gap-3 items-center" dir="ltr">
                                    <div className="flex-1 w-full">
                                        <input
                                            type="text"
                                            value={formData.baseURL}
                                            onChange={(e) => setFormData({ ...formData, baseURL: e.target.value })}
                                            placeholder="https://vc2.hekmateislami.com"
                                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 bg-white"
                                            dir="ltr"
                                        />
                                        <p className="text-xs text-gray-500 mt-1">Base URL (آدرس پایه سرور)</p>
                                    </div>
                                    {/* <span className="text-gray-400 text-xl hidden md:block">/</span> */}
                                    <div className="flex-1 w-full">
                                        <div className="flex gap-2">
                                            <span className="inline-flex items-center px-3 py-2 border border-white rounded-lg bg-white text-2xl text-gray-600 hidden md:flex">
                                                /
                                            </span>
                                            <input
                                                type="text"
                                                value={formData.actionPath}
                                                onChange={(e) => setFormData({ ...formData, actionPath: e.target.value })}
                                                placeholder="hook/endpoint"
                                                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 bg-white"
                                                dir="ltr"
                                            />
                                        </div>
                                        <p className="text-xs text-gray-500 mt-1">Action Path (مسیر endpoint - اختیاری)</p>
                                    </div>
                                </div>
                                {getFullCallbackURL() && (
                                    <div className="mt-3 p-2 bg-blue-50 rounded-lg border border-blue-200">
                                        <p className="text-xs text-blue-700 mb-1">آدرس کامل Callback:</p>
                                        <code className="text-sm text-blue-800 break-all" dir="ltr">
                                            {getFullCallbackURL()}
                                        </code>
                                    </div>
                                )}
                            </div>

                            {/* Meeting Selection */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    جلسه (اختیاری)
                                </label>
                                <select
                                    value={formData.meetingID}
                                    onChange={(e) => setFormData({ ...formData, meetingID: e.target.value })}
                                    className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 bg-white"
                                >
                                    <option value="">همه جلسات (هوک سراسری)</option>
                                    {rooms.map((room) => (
                                        <option key={room.bbbMeetID} value={room.bbbMeetID}>
                                            {room.title} - {room.friendlyID}
                                        </option>
                                    ))}
                                </select>
                                <p className="text-xs text-gray-500 mt-1">
                                    در صورت انتخاب نکردن، هوک برای تمام جلسات فعال خواهد بود
                                </p>
                            </div>

                            {/* Hook Type Selection */}
                            {/* Hook Type Selection */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    نوع هوک
                                </label>
                                <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
                                    <button
                                        type="button"
                                        onClick={() => setFormData({ ...formData, hookType: "all", selectedEvents: [] })}
                                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${formData.hookType === "all"
                                            ? "bg-blue-600 text-white shadow-md"
                                            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                                            }`}
                                    >
                                        همه رویدادها
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => setFormData({ ...formData, hookType: "recording", selectedEvents: [] })}
                                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${formData.hookType === "recording"
                                            ? "bg-red-600 text-white shadow-md"
                                            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                                            }`}
                                    >
                                        ضبط
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => setFormData({ ...formData, hookType: "participant", selectedEvents: [] })}
                                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${formData.hookType === "participant"
                                            ? "bg-green-600 text-white shadow-md"
                                            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                                            }`}
                                    >
                                        شرکت‌کنندگان
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => setFormData({ ...formData, hookType: "moderation", selectedEvents: [] })}
                                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${formData.hookType === "moderation"
                                            ? "bg-orange-600 text-white shadow-md"
                                            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                                            }`}
                                    >
                                        مدیریتی
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => setFormData({ ...formData, hookType: "custom", selectedEvents: [] })}
                                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${formData.hookType === "custom"
                                            ? "bg-purple-600 text-white shadow-md"
                                            : "bg-gray-100 text-gray-700 hover:bg-gray-200"
                                            }`}
                                    >
                                        سفارشی
                                    </button>
                                </div>
                                <p className="text-xs text-gray-500 mt-2">
                                    {formData.hookType === "all" && "✅ هوک برای تمام رویدادها فعال خواهد شد"}
                                    {formData.hookType === "recording" && "📹 فقط رویدادهای مربوط به ضبط (شروع، توقف، پایان جلسه)"}
                                    {formData.hookType === "participant" && "👥 فقط رویدادهای مربوط به شرکت‌کنندگان (ورود، خروج، صدا، دوربین)"}
                                    {formData.hookType === "moderation" && "⚙️ فقط رویدادهای مدیریتی (ایجاد، حذف، پایان جلسه)"}
                                    {formData.hookType === "custom" && "🎯 رویدادهای مورد نظر خود را انتخاب کنید"}
                                </p>
                            </div>
                            {/* Custom Events Selection */}
                            {formData.hookType === "custom" && (
                                <div className="border border-gray-200 rounded-xl p-4 bg-gray-50">
                                    <div className="flex justify-between items-center mb-4">
                                        <label className="block text-sm font-medium text-gray-700">
                                            انتخاب رویدادها
                                        </label>
                                        <div className="flex gap-2">
                                            <button
                                                onClick={selectAllEvents}
                                                className="text-xs text-blue-600 hover:text-blue-800 px-2 py-1"
                                            >
                                                انتخاب همه
                                            </button>
                                            <button
                                                onClick={deselectAllEvents}
                                                className="text-xs text-red-600 hover:text-red-800 px-2 py-1"
                                            >
                                                حذف همه
                                            </button>
                                        </div>
                                    </div>
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3 max-h-80 overflow-y-auto">
                                        {ALL_EVENTS.map((event) => (
                                            <label
                                                key={event.id}
                                                className={`flex items-center gap-3 p-3 rounded-lg cursor-pointer transition-all ${formData.selectedEvents.includes(event.id)
                                                    ? "bg-blue-50 border border-blue-200"
                                                    : "bg-white border border-gray-200 hover:bg-gray-50"
                                                    }`}
                                            >
                                                <input
                                                    type="checkbox"
                                                    checked={formData.selectedEvents.includes(event.id)}
                                                    onChange={() => toggleEventSelection(event.id)}
                                                    className="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
                                                />
                                                <div className={`p-1.5 rounded-lg ${event.color}`}>
                                                    {event.icon}
                                                </div>
                                                <div className="flex-1">
                                                    <p className="text-sm font-medium text-gray-800">{event.name}</p>
                                                    <p className="text-xs text-gray-500">{event.description}</p>
                                                </div>
                                            </label>
                                        ))}
                                    </div>
                                    <div className="mt-3 text-center">
                                        <p className="text-sm text-gray-600">
                                            تعداد رویدادهای انتخاب شده: <span className="font-bold text-blue-600">{formData.selectedEvents.length}</span>
                                        </p>
                                    </div>
                                </div>
                            )}

                            {/* Info Box */}
                            <div className="bg-blue-50 rounded-lg p-3 border border-blue-200">
                                <p className="text-sm text-blue-800 flex items-center gap-2">
                                    <FaInfoCircle />
                                    هوک‌های دائمی (Permanent) قابل حذف نیستند و فقط از طریق فایل تنظیمات سرور قابل مدیریت هستند.
                                </p>
                            </div>
                        </div>

                        <div className="sticky bottom-0 bg-white flex justify-end gap-3 p-6 border-t border-gray-100">
                            <button
                                onClick={() => setShowCreateModal(false)}
                                className="px-6 py-2 border border-gray-200 rounded-xl text-gray-600 hover:bg-gray-50"
                            >
                                انصراف
                            </button>
                            <button
                                onClick={handleCreateWebhook}
                                disabled={!formData.baseURL || loading}
                                className="px-6 py-2 bg-blue-600 text-white rounded-xl hover:bg-blue-700 flex items-center gap-2 disabled:opacity-50"
                            >
                                {loading ? <FaSpinner className="animate-spin" /> : <FaPlus />}
                                ایجاد هوک
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal - with semi-transparent background */}
            {showDeleteModal && (
                <div className="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl max-w-md w-full">
                        <div className="p-6">
                            <div className="flex items-center justify-center mb-4">
                                <div className="bg-red-100 rounded-full p-3">
                                    <FaTrash className="w-8 h-8 text-red-600" />
                                </div>
                            </div>
                            <h3 className="text-lg font-bold text-gray-800 text-center mb-2">تأیید حذف</h3>
                            <p className="text-gray-600 text-center mb-6">
                                آیا از حذف این هوک اطمینان دارید؟ این عمل قابل بازگشت نیست.
                            </p>
                            <div className="flex justify-center gap-3">
                                <button
                                    onClick={() => setShowDeleteModal(false)}
                                    className="px-6 py-2 border border-gray-200 rounded-xl text-gray-600 hover:bg-gray-50"
                                >
                                    انصراف
                                </button>
                                <button
                                    onClick={handleDeleteWebhook}
                                    className="px-6 py-2 bg-red-600 text-white rounded-xl hover:bg-red-700"
                                >
                                    حذف
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Hook Detail Modal - with semi-transparent background */}
            {showDetailModal && selectedHook && (
                <div className="fixed inset-0 bg-black/30 backdrop-blur-sm flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl max-w-3xl w-full my-8">
                        <div className="sticky top-0 bg-white z-10 flex justify-between items-center p-6 border-b border-gray-100">
                            <h2 className="text-xl font-bold text-gray-800">جزئیات هوک</h2>
                            <button onClick={() => setShowDetailModal(false)} className="text-gray-400 hover:text-gray-600">
                                <FaTimesCircle className="w-6 h-6" />
                            </button>
                        </div>

                        <div className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">شناسه هوک</p>
                                    <p className="text-sm font-mono text-gray-800 break-all">{selectedHook.hookID}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">آدرس Callback</p>
                                    <p className="text-sm text-gray-800 break-all">{selectedHook.callbackURL}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">جلسه</p>
                                    <p className="text-sm text-gray-800">{getRoomTitle(selectedHook.meetingID)}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">نوع هوک</p>
                                    <div className="flex gap-2">
                                        {selectedHook.permanent && (
                                            <span className="inline-flex items-center gap-1 px-2 py-1 bg-purple-100 text-purple-700 text-xs rounded-full">
                                                دائمی
                                            </span>
                                        )}
                                        {selectedHook.rawData && (
                                            <span className="inline-flex items-center gap-1 px-2 py-1 bg-yellow-100 text-yellow-700 text-xs rounded-full">
                                                Raw Data
                                            </span>
                                        )}
                                        {!selectedHook.permanent && !selectedHook.rawData && (
                                            <span className="text-sm text-gray-600">معمولی</span>
                                        )}
                                    </div>
                                </div>
                            </div>

                            {selectedHook.eventIDs && selectedHook.eventIDs.length > 0 && (
                                <div className="bg-blue-50 rounded-xl p-4 border border-blue-100">
                                    <h3 className="font-bold text-blue-800 mb-3 flex items-center gap-2">
                                        <FaCalendarAlt /> رویدادهای ثبت شده ({selectedHook.eventIDs.length})
                                    </h3>
                                    <div className="flex flex-wrap gap-2 max-h-60 overflow-y-auto">
                                        {selectedHook.eventIDs.map((eventId, idx) => {
                                            const eventInfo = ALL_EVENTS.find(e => e.id === eventId);
                                            return (
                                                <span key={idx} className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs ${eventInfo?.color || "bg-gray-100 text-gray-700"}`}>
                                                    {eventInfo?.icon}
                                                    {eventInfo?.name || eventId}
                                                </span>
                                            );
                                        })}
                                    </div>
                                </div>
                            )}
                        </div>

                        <div className="sticky bottom-0 bg-white flex justify-end gap-3 p-6 border-t border-gray-100">
                            <button
                                onClick={() => setShowDetailModal(false)}
                                className="px-6 py-2 border border-gray-200 rounded-xl text-gray-600 hover:bg-gray-50"
                            >
                                بستن
                            </button>
                            {!selectedHook.permanent && (
                                <button
                                    onClick={() => {
                                        setShowDetailModal(false);
                                        setDeletingHookId(selectedHook.hookID);
                                        setShowDeleteModal(true);
                                    }}
                                    className="px-6 py-2 bg-red-600 text-white rounded-xl hover:bg-red-700 flex items-center gap-2"
                                >
                                    <FaTrash />
                                    حذف هوک
                                </button>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default WebhooksManagement;