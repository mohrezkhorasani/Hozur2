// src/Pages/Admin/RoomsManagement.jsx
import React, { useState, useEffect } from "react";
import {
    FaSearch, FaEdit, FaTrash, FaPlus, FaEye, FaVideo,
    FaCalendarAlt, FaUser, FaClock, FaChevronLeft, FaChevronRight,
    FaTimesCircle, FaFilter, FaChalkboardTeacher, FaBookOpen,
    FaUsers, FaMapMarkerAlt, FaStar, FaRegClock, FaCheckCircle,
    FaSpinner, FaArrowLeft, FaArrowRight, FaSync, FaExclamationTriangle,
    FaHome, FaBuilding, FaUniversity, FaGlobe, FaEllipsisV,
    FaPlay, FaInfoCircle, FaCog, FaDoorOpen, FaCircle
} from "react-icons/fa";
import EmojiIcon from "../../Component/EmojiIcon";
import { fetchRoomLiveAdmin, GetAllRooms } from "../../api/AdminAPI";
import { joinMeeting, startNewMeet } from "../../api/StartNewMeeting";
import { fetchRoomLive } from "../../api/GetRooms";
import { BaseURL, BaseURLSite } from "../../api/apiNeed";
import EditRoomModal from "../../Modal/Admin/EditRoomModal";

const RoomsManagement = () => {
    const [rooms, setRooms] = useState([]);
    const [filteredRooms, setFilteredRooms] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [currentPage, setCurrentPage] = useState(1);
    const [selectedRoom, setSelectedRoom] = useState(null);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [showActionMenu, setShowActionMenu] = useState(null);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [deletingRoomId, setDeletingRoomId] = useState(null);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [roomToEdit, setRoomToEdit] = useState(null);
    const itemsPerPage = 10;
    const [roomLive, setRoomLive] = useState([]);
    const [isStartingMeet, setIsStartingMeet] = useState(null);

    // Filters
    const [filterOwner, setFilterOwner] = useState("all");
    const [filterHasNextPlan, setFilterHasNextPlan] = useState("all");
    const [showFilters, setShowFilters] = useState(false);

    // Get user info from localStorage or context
    const [user, setUser] = useState(() => {
        try {
            const savedUser = localStorage.getItem('user');
            return savedUser ? JSON.parse(savedUser) : { fullname: "مدیر سیستم" };
        } catch {
            return { fullname: "مدیر سیستم" };
        }
    });

    async function CheckLives() {
        try {
            const livedata = await fetchRoomLiveAdmin();
            setRoomLive(livedata);
        } catch (error) {
            console.error("Error fetching live rooms:", error);
        }
    }

    // Load rooms
    const loadRooms = async () => {
        try {
            setLoading(true);
            const data = await GetAllRooms();
            if (data && data.data) {
                // Sort rooms: live rooms first, then by lastMeet date
                const sortedRooms = sortRoomsByLiveAndDate(data.data, roomLive);
                setRooms(sortedRooms);
                setFilteredRooms(sortedRooms);
            }
        } catch (err) {
            console.error("Error loading rooms:", err);
        } finally {
            setLoading(false);
        }
    };

    // Sort rooms: live first, then by lastMeet date (newest first)
    const sortRoomsByLiveAndDate = (roomsList, liveList) => {
        const liveRoomIds = new Set(liveList.map(live => live.roomID));

        const liveRooms = roomsList.filter(room => liveRoomIds.has(room.roomID));
        const nonLiveRooms = roomsList.filter(room => !liveRoomIds.has(room.roomID));

        // Sort live rooms by lastMeet (newest first)
        liveRooms.sort((a, b) => new Date(b.lastMeet) - new Date(a.lastMeet));
        // Sort non-live rooms by lastMeet (newest first)
        nonLiveRooms.sort((a, b) => new Date(b.lastMeet) - new Date(a.lastMeet));

        return [...liveRooms, ...nonLiveRooms];
    };

    // Update rooms when roomLive changes
    useEffect(() => {
        if (rooms.length > 0) {
            const sortedRooms = sortRoomsByLiveAndDate(rooms, roomLive);
            setRooms(sortedRooms);
        }
    }, [roomLive]);

    useEffect(() => {
        loadRooms();
        const intervalId = setInterval(CheckLives, 20000);
        CheckLives();
        document.title = "مدیریت اتاق‌ها | پنل ادمین";

        return () => clearInterval(intervalId);
    }, []);

    // Apply filters
    useEffect(() => {
        let filtered = [...rooms];

        if (searchTerm) {
            filtered = filtered.filter(room =>
                room.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                room.roomID?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                room.owner?.fullname?.toLowerCase().includes(searchTerm.toLowerCase()) ||
                room.owner?.username?.toLowerCase().includes(searchTerm.toLowerCase())
            );
        }

        if (filterOwner !== "all") {
            filtered = filtered.filter(room => room.owner?.username === filterOwner);
        }

        if (filterHasNextPlan !== "all") {
            const hasPlan = filterHasNextPlan === "yes";
            filtered = filtered.filter(room => hasPlan ? room.nextPlan !== null : room.nextPlan === null);
        }

        setFilteredRooms(filtered);
        setCurrentPage(1);
    }, [searchTerm, filterOwner, filterHasNextPlan, rooms]);

    const uniqueOwners = [...new Map(rooms.map(room => [room.owner?.username, room.owner])).values()].filter(owner => owner?.username);

    const totalPages = Math.ceil(filteredRooms.length / itemsPerPage);
    const paginatedRooms = filteredRooms.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    const formatDate = (dateString) => {
        if (!dateString || dateString === "9999-12-31T23:59:59.9999999") return "نامشخص";
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('fa-IR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        }).format(date);
    };

    const getAvatarName = (avatar) => {
        if (!avatar) return null;
        return avatar.split('/').pop();
    };

    const resetFilters = () => {
        setSearchTerm("");
        setFilterOwner("all");
        setFilterHasNextPlan("all");
        setShowFilters(false);
    };

    const openDetailModal = (room) => {
        setSelectedRoom(room);
        setShowDetailModal(true);
        setShowActionMenu(null);
    };

    const isRoomLive = (roomID) => {
        return roomLive.some(room => room.roomID === roomID);
    };

    const getLiveRoomData = (roomID) => {
        return roomLive.find(room => room.roomID === roomID);
    };

    const handleStartMeeting = async (room) => {
        const roomID = room.roomID;
        const planID = room.nextPlan?.id || -1;
        const title = room.nextPlan?.title || "";

        // Extract teachers from nextPlan if exists
        let teachers = [];
        if (room.nextPlan?.teacher) {
            teachers = room.nextPlan.teacher.split('،').map(t => ({ name: t.trim() }));
        }

        const dabir = room.nextPlan?.dabir || null;
        const hasCert = false;

        console.log("handleStart", planID);

        setIsStartingMeet(roomID);

        try {
            if (isRoomLive(roomID)) {
                const data = await joinMeeting(roomID, user.fullname, true);
                if (data?.link) {
                    window.open(data.link, "_blank");
                } else {
                    if (data?.message) {
                        alert(data.message);
                    } else {
                        alert("لینک ورود یافت نشد.");
                    }
                }
            } else {
                const teacherStrings = teachers.map(t => t.name);
                const data = await startNewMeet(roomID, user.fullname, title, teacherStrings, dabir, planID, hasCert);
                console.warn(data);

                if (data?.redirectURL) {
                    window.open(data.redirectURL, '_blank');
                } else {
                    alert("لینک ورود یافت نشد.");
                }
            }
        } catch (err) {
            console.error(err);
            alert("خطا در شروع جلسه");
        } finally {
            setIsStartingMeet(null);
        }
    };

    const handleEditRoom = (room) => {
        setRoomToEdit({
            friendlyID: room.friendlyID || room.roomID || room.id,
            roomTitle: room.title || room.name || '',
            logo: room.avatar || room.logo || '',
            najmIDBoss: room.najmIDBoss || room.najmID || ''
        });
        setIsEditModalOpen(true);
        setShowActionMenu(null);
    };

    const handleDeleteRoom = (roomId) => {
        setDeletingRoomId(roomId);
        setShowDeleteModal(true);
        setShowActionMenu(null);
    };

    const confirmDelete = () => {
        console.log("حذف اتاق:", deletingRoomId);
        setShowDeleteModal(false);
        setDeletingRoomId(null);
        loadRooms();
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
                <div className="container mx-auto px-4 py-8">
                    <div className="flex justify-center items-center h-96">
                        <div className="text-center">
                            <FaSpinner className="animate-spin text-5xl text-blue-600 mx-auto mb-4" />
                            <p className="text-gray-600">در حال دریافت اطلاعات اتاق‌ها...</p>
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
                            <h1 className="text-3xl font-bold text-gray-800 mb-2">مدیریت اتاق‌ها</h1>
                            <p className="text-gray-600">مدیریت اتاق‌های جلسات مجازی، گروه‌های علمی و برنامه‌ها</p>
                        </div>
                        <button
                            onClick={() => {
                                setRoomToEdit(null);
                                setIsEditModalOpen(true);
                            }}
                            className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white px-5 py-2.5 rounded-xl transition-all flex items-center gap-2 shadow-lg"
                        >
                            <FaPlus className="text-sm" />
                            <span>اتاق جدید / ویرایش</span>
                        </button>
                    </div>
                </div>

                {/* Stats Cards */}
                <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">کل اتاق‌ها</p>
                                <p className="text-2xl font-bold text-gray-800">{rooms.length}</p>
                            </div>
                            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                                <FaVideo className="text-blue-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">پخش زنده</p>
                                <p className="text-2xl font-bold text-red-600">{roomLive.length}</p>
                            </div>
                            <div className="w-10 h-10 bg-red-100 rounded-lg flex items-center justify-center">
                                <FaCircle className="text-red-600 animate-pulse" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">اتاق‌های فعال</p>
                                <p className="text-2xl font-bold text-green-600">{rooms.filter(r => r.nextPlan && r.nextPlan.startAt !== "9999-12-31T23:59:59.9999999").length}</p>
                            </div>
                            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                                <FaCheckCircle className="text-green-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">برنامه آینده</p>
                                <p className="text-2xl font-bold text-orange-600">{rooms.filter(r => r.nextPlan).length}</p>
                            </div>
                            <div className="w-10 h-10 bg-orange-100 rounded-lg flex items-center justify-center">
                                <FaCalendarAlt className="text-orange-600" />
                            </div>
                        </div>
                    </div>
                    <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-gray-500 text-sm">مدیران</p>
                                <p className="text-2xl font-bold text-purple-600">{uniqueOwners.length}</p>
                            </div>
                            <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                                <FaUsers className="text-purple-600" />
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
                                placeholder="جستجو بر اساس عنوان، شناسه، مدیر یا نام کاربری..."
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
                            className={`px-4 py-2.5 rounded-xl border transition-all duration-200 flex items-center gap-2 ${showFilters || filterOwner !== "all" || filterHasNextPlan !== "all"
                                ? "bg-blue-600 text-white border-blue-600"
                                : "bg-white text-gray-700 border-gray-200 hover:border-blue-400"
                                }`}
                        >
                            <FaFilter className="text-sm" />
                            <span>فیلتر</span>
                        </button>
                        {(filterOwner !== "all" || filterHasNextPlan !== "all") && (
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
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">فیلتر بر اساس مدیر</label>
                                    <select
                                        value={filterOwner}
                                        onChange={(e) => setFilterOwner(e.target.value)}
                                        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                                    >
                                        <option value="all">همه مدیران</option>
                                        {uniqueOwners.map((owner) => (
                                            <option key={owner.username} value={owner.username}>
                                                {owner.fullname || owner.username}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">وضعیت برنامه</label>
                                    <select
                                        value={filterHasNextPlan}
                                        onChange={(e) => setFilterHasNextPlan(e.target.value)}
                                        className="w-full px-3 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
                                    >
                                        <option value="all">همه</option>
                                        <option value="yes">دارای برنامه آینده</option>
                                        <option value="no">بدون برنامه آینده</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                    )}
                </div>

                {/* Rooms Table */}
                <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead className="bg-gray-50 border-b border-gray-200">
                                <tr>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">آیکون</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">عنوان</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">شناسه</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">مدیر</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">وضعیت</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">برنامه بعدی</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">آخرین جلسه</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-gray-100">
                                {paginatedRooms.map((room) => {
                                    const isLive = isRoomLive(room.roomID);
                                    const liveData = getLiveRoomData(room.roomID);
                                    const isStarting = isStartingMeet === room.roomID;

                                    return (
                                        <tr key={room.roomID} className={`hover:bg-gray-50 transition-colors ${isLive ? 'bg-red-50/30' : ''}`}>
                                            <td className="px-6 py-4 whitespace-nowrap relative">
                                                <img className="w-15 hover:w-150 transition-all duration-300" src={room.avatar}/>
                                                {isLive && (
                                                    <div className="absolute -top-1 -right-1">
                                                        <div className="relative">
                                                            <span className="flex h-3 w-3">
                                                                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75"></span>
                                                                <span className="relative inline-flex rounded-full h-3 w-3 bg-red-500"></span>
                                                            </span>
                                                        </div>
                                                    </div>
                                                )}
                                            </td>
                                            <td className="px-6 py-4 text-sm font-medium text-gray-900">
                                                <div className="flex items-center gap-2">
                                                    {room.title || "بدون عنوان"}
                                                    {isLive && (
                                                        <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full">
                                                            <FaCircle className="w-2 h-2 animate-pulse" />
                                                            زنده
                                                        </span>
                                                    )}
                                                </div>
                                                {isLive && liveData && (
                                                    <p className="text-xs text-red-600 mt-1">
                                                        {liveData.participantsCount || 0} نفر در حال تماشا
                                                    </p>
                                                )}
                                            </td>
                                            <td className="px-6 py-4 text-sm text-gray-500 font-mono">
                                                <span className="cursor-pointer" onClick={async () => {
                                                    const link = `${BaseURLSite}rooms/${room.roomID}/join`;

                                                    await navigator.clipboard.writeText(link);
                                                    alert("کپی شد!");
                                                }}>
                                                    {room.roomID}
                                                </span>
                                            </td>
                                            <td className="px-6 py-4 text-sm text-gray-600">
                                                <div className="flex flex-col">
                                                    <span className="font-medium">{room.owner?.fullname || "نامشخص"}</span>
                                                    <span className="text-xs text-gray-400">@{room.owner?.username}</span>
                                                </div>
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap">
                                                {isLive ? (
                                                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-red-100 text-red-700 text-xs rounded-full">
                                                        <FaCircle className="w-2 h-2 animate-pulse" />
                                                        در حال پخش
                                                    </span>
                                                ) : (
                                                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-gray-100 text-gray-500 text-xs rounded-full">
                                                        <FaRegClock className="w-2 h-2" />
                                                        آفلاین
                                                    </span>
                                                )}
                                            </td>
                                            <td className="px-6 py-4 text-sm">
                                                {room.nextPlan ? (
                                                    <div className="flex flex-col">
                                                        <span className="text-gray-900 font-medium line-clamp-1">{room.nextPlan.title}</span>
                                                        <span className="text-xs text-gray-500">{formatDate(room.nextPlan.startAt)}</span>
                                                    </div>
                                                ) : (
                                                    <span className="text-gray-400">—</span>
                                                )}
                                            </td>
                                            <td className="px-6 py-4 text-sm text-gray-500">
                                                {formatDate(room.lastMeet)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm relative">
                                                <button
                                                    onClick={() => setShowActionMenu(showActionMenu === room.roomID ? null : room.roomID)}
                                                    className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                                                    disabled={isStarting}
                                                >
                                                    {isStarting ? (
                                                        <FaSpinner className="w-5 h-5 text-blue-500 animate-spin" />
                                                    ) : (
                                                        <FaEllipsisV className="w-5 h-5 text-gray-500" />
                                                    )}
                                                </button>

                                                {/* Action Menu Dropdown */}
                                                {showActionMenu === room.roomID && !isStarting && (
                                                    <>
                                                        <div
                                                            className="fixed inset-0 z-40"
                                                            onClick={() => setShowActionMenu(null)}
                                                        />
                                                        <div className="absolute left-0 mt-2 w-56 bg-white rounded-xl shadow-lg border border-gray-100 z-50 overflow-hidden">
                                                            <button
                                                                onClick={() => handleStartMeeting(room)}
                                                                className="w-full px-4 py-3 text-right hover:bg-green-50 transition-colors flex items-center gap-3 border-b border-gray-50"
                                                            >
                                                                <FaPlay className="w-4 h-4 text-green-600" />
                                                                <span className="text-sm text-gray-700">شروع جلسه</span>
                                                            </button>
                                                            <button
                                                                onClick={() => openDetailModal(room)}
                                                                className="w-full px-4 py-3 text-right hover:bg-blue-50 transition-colors flex items-center gap-3 border-b border-gray-50"
                                                            >
                                                                <FaInfoCircle className="w-4 h-4 text-blue-600" />
                                                                <span className="text-sm text-gray-700">مشاهده جزئیات</span>
                                                            </button>
                                                            <button
                                                                onClick={() => handleEditRoom(room)}
                                                                className="w-full px-4 py-3 text-right hover:bg-yellow-50 transition-colors flex items-center gap-3 border-b border-gray-50"
                                                            >
                                                                <FaEdit className="w-4 h-4 text-yellow-600" />
                                                                <span className="text-sm text-gray-700">ویرایش اتاق</span>
                                                            </button>
                                                            <button
                                                                onClick={() => handleDeleteRoom(room.roomID)}
                                                                className="w-full px-4 py-3 text-right hover:bg-red-50 transition-colors flex items-center gap-3"
                                                            >
                                                                <FaTrash className="w-4 h-4 text-red-600" />
                                                                <span className="text-sm text-gray-700">حذف اتاق</span>
                                                            </button>
                                                        </div>
                                                    </>
                                                )}
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>

                    {filteredRooms.length === 0 && (
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

            {/* Room Detail Modal */}
            {showDetailModal && selectedRoom && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl max-w-3xl w-full my-8">
                        <div className="flex justify-between items-center p-6 border-b border-gray-100">
                            <div className="flex items-center gap-3">
                                <EmojiIcon name={getAvatarName(selectedRoom.avatar)} className="w-10 h-10" />
                                <h2 className="text-xl font-bold text-gray-800">{selectedRoom.title || "بدون عنوان"}</h2>
                                {isRoomLive(selectedRoom.roomID) && (
                                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-red-100 text-red-700 text-xs rounded-full">
                                        <FaCircle className="w-2 h-2 animate-pulse" />
                                        در حال پخش زنده
                                    </span>
                                )}
                            </div>
                            <button onClick={() => setShowDetailModal(false)} className="text-gray-400 hover:text-gray-600">
                                <FaTimesCircle className="w-6 h-6" />
                            </button>
                        </div>

                        <div className="p-6 space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div className="bg-gray-50 rounded-xl p-4 cursor-pointer" onClick={async () => {
                                    const link = `${BaseURLSite}/rooms/${selectedRoom.roomID}/join`;

                                    await navigator.clipboard.writeText(link);
                                    alert("کپی شد!");
                                }}>
                                    <p className="text-xs text-gray-500 mb-1">شناسه اتاق</p>
                                    <p className="text-sm font-mono text-gray-800">{selectedRoom.roomID}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">مدیر اتاق</p>
                                    <p className="text-sm font-medium text-gray-800">{selectedRoom.owner?.fullname || "نامشخص"}</p>
                                    <p className="text-xs text-gray-500">@{selectedRoom.owner?.username}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">تاریخ ایجاد</p>
                                    <p className="text-sm text-gray-800">{formatDate(selectedRoom.created)}</p>
                                </div>
                                <div className="bg-gray-50 rounded-xl p-4">
                                    <p className="text-xs text-gray-500 mb-1">آخرین جلسه</p>
                                    <p className="text-sm text-gray-800">{formatDate(selectedRoom.lastMeet)}</p>
                                </div>
                                {(selectedRoom.najmIDBoss || selectedRoom.najmID) && (
                                    <div className="bg-emerald-50 rounded-xl p-4 border border-emerald-100 col-span-2 flex items-center justify-between">
                                        <div>
                                            <p className="text-xs text-emerald-700 font-bold mb-0.5">شناسه مسئول در سامانه نجم (NajmIDBoss)</p>
                                            <p className="text-sm font-mono text-emerald-900 font-bold">{selectedRoom.najmIDBoss || selectedRoom.najmID}</p>
                                        </div>
                                    </div>
                                )}
                            </div>

                            {selectedRoom.nextPlan && (
                                <div className="bg-blue-50 rounded-xl p-4 border border-blue-100">
                                    <h3 className="font-bold text-blue-800 mb-3 flex items-center gap-2">
                                        <FaCalendarAlt /> برنامه بعدی
                                    </h3>
                                    <div className="space-y-2">
                                        <p><span className="text-gray-600">عنوان:</span> {selectedRoom.nextPlan.title}</p>
                                        {selectedRoom.nextPlan.teacher && (
                                            <p><span className="text-gray-600">استاد:</span> {selectedRoom.nextPlan.teacher}</p>
                                        )}
                                        {selectedRoom.nextPlan.dabir && (
                                            <p><span className="text-gray-600">دبیر:</span> {selectedRoom.nextPlan.dabir}</p>
                                        )}
                                        <p><span className="text-gray-600">زمان:</span> {formatDate(selectedRoom.nextPlan.startAt)}</p>
                                    </div>
                                </div>
                            )}

                            {selectedRoom.latestTitle && (
                                <div className="bg-green-50 rounded-xl p-4 border border-green-100">
                                    <h3 className="font-bold text-green-800 mb-2">آخرین جلسه برگزار شده</h3>
                                    <p>{selectedRoom.latestTitle}</p>
                                </div>
                            )}
                        </div>

                        <div className="flex justify-end gap-3 p-6 border-t border-gray-100">
                            <button onClick={() => setShowDetailModal(false)} className="px-6 py-2 border border-gray-200 rounded-xl text-gray-600 hover:bg-gray-50">
                                بستن
                            </button>
                            <button
                                onClick={() => {
                                    setShowDetailModal(false);
                                    handleStartMeeting(selectedRoom);
                                }}
                                className="px-6 py-2 bg-green-600 text-white rounded-xl hover:bg-green-700 flex items-center gap-2"
                            >
                                <FaPlay className="w-4 h-4" />
                                شروع جلسه
                            </button>
                            <button 
                                onClick={() => {
                                    setShowDetailModal(false);
                                    handleEditRoom(selectedRoom);
                                }}
                                className="px-6 py-2 bg-blue-600 text-white rounded-xl hover:bg-blue-700"
                            >
                                ویرایش اتاق
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {showDeleteModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl max-w-md w-full">
                        <div className="p-6">
                            <div className="flex items-center justify-center mb-4">
                                <div className="bg-red-100 rounded-full p-3">
                                    <FaTrash className="w-8 h-8 text-red-600" />
                                </div>
                            </div>
                            <h3 className="text-lg font-bold text-gray-800 text-center mb-2">تأیید حذف</h3>
                            <p className="text-gray-600 text-center mb-6">
                                آیا از حذف این اتاق اطمینان دارید؟ این عمل قابل بازگشت نیست.
                            </p>
                            <div className="flex justify-center gap-3">
                                <button onClick={() => setShowDeleteModal(false)} className="px-6 py-2 border border-gray-200 rounded-xl text-gray-600 hover:bg-gray-50">
                                    انصراف
                                </button>
                                <button onClick={confirmDelete} className="px-6 py-2 bg-red-600 text-white rounded-xl hover:bg-red-700">
                                    حذف
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Edit Room Modal */}
            <EditRoomModal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                initialRoom={roomToEdit}
                onSuccess={() => {
                    setIsEditModalOpen(false);
                    loadRooms();
                }}
            />
        </div>
    );
};

export default RoomsManagement;