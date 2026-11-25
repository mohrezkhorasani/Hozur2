import React, { useState, useEffect, useMemo, useRef } from "react";
import { useNavigate } from "react-router-dom";
import {
    FaPlus,
    FaEdit,
    FaTrash,
    FaTimes,
    FaCheck,
    FaPoll,
    FaSearch,
    FaFilter,
    FaTimesCircle,
    FaUndo,
    FaSyncAlt,
    FaChartBar,
    FaCheckCircle,
    FaBan,
    FaGlobe,
    FaVideo,
    FaUsers,
    FaChalkboardTeacher,
    FaChevronDown
} from "react-icons/fa";
import {
    GetPollListAdmin,
    AddPollQuiz,
    UpdatePollQuiz,
    DeletePollQuiz,
    TogglePollStatus
} from "../api/AdminAPI";
import { GetTeacherList } from "../api/GetRooms";

// نرمال‌سازی متن فارسی جهت تطبیق دقیق نام استاد/دبیر درون متن سوال
const normalizePersianText = (str) => {
    if (!str) return "";
    return String(str)
        .replace(/[*_`~]/g, "")
        .replace(/[ي]/g, "ی")
        .replace(/[ك]/g, "ک")
        .replace(/[أإآ]/g, "ا")
        .replace(/[\u200c\u200f]/g, " ")
        .replace(/\s+/g, " ")
        .trim()
        .toLowerCase();
};

const PollManagementSkeleton = () => (
    <div className="min-h-screen bg-gradient-to-b from-[#f9fafb] to-[#ffffff]" dir="rtl">
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-10 space-y-6 animate-pulse">
            <div className="flex flex-col sm:flex-row justify-between gap-4">
                <div className="space-y-2">
                    <div className="h-7 w-44 bg-[#e5e7eb] rounded-lg" />
                    <div className="h-4 w-64 bg-[#f3f4f6] rounded" />
                </div>
                <div className="flex gap-2">
                    <div className="h-10 w-36 bg-[#e5e7eb] rounded-xl" />
                    <div className="h-10 w-32 bg-[#e5e7eb] rounded-xl" />
                </div>
            </div>
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3 sm:gap-4">
                {[1, 2, 3, 4, 5].map((i) => (
                    <div key={i} className="bg-white rounded-2xl border border-[#e5e7eb] p-4 space-y-2">
                        <div className="h-3.5 w-20 bg-[#f3f4f6] rounded" />
                        <div className="h-7 w-14 bg-[#e5e7eb] rounded-lg" />
                    </div>
                ))}
            </div>
            <div className="bg-white rounded-2xl border border-[#e5e7eb] p-5 space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
                    {[1, 2, 3, 4, 5].map((i) => (
                        <div key={i} className="h-10 bg-[#f3f4f6] rounded-xl" />
                    ))}
                </div>
            </div>
            <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 space-y-3">
                {[1, 2, 3, 4].map((row) => (
                    <div key={row} className="h-14 bg-[#f9fafb] rounded-xl" />
                ))}
            </div>
        </div>
    </div>
);

const SearchableTeacherDropdown = ({
    teachers,
    loadingTeachers,
    value,
    onChange
}) => {
    const [isOpen, setIsOpen] = useState(false);
    const [query, setQuery] = useState("");
    const containerRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (containerRef.current && !containerRef.current.contains(e.target)) {
                setIsOpen(false);
            }
        };
        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    const filteredTeachers = useMemo(() => {
        const normQ = normalizePersianText(query);
        if (!normQ) return teachers;
        return teachers.filter(
            (t) =>
                normalizePersianText(t.fullname).includes(normQ) ||
                normalizePersianText(t.role).includes(normQ)
        );
    }, [teachers, query]);

    const selectedItem = teachers.find((t) => t.fullname === value);

    return (
        <div ref={containerRef} className="relative">
            <div
                onClick={() => setIsOpen((prev) => !prev)}
                className={`w-full px-3 py-2 text-xs sm:text-sm border rounded-xl bg-white flex items-center justify-between gap-2 cursor-pointer transition ${
                    isOpen ? "border-[#0284c7] ring-2 ring-[#0284c7]/20" : "border-[#d1d5db] hover:border-[#9ca3af]"
                }`}
            >
                <div className="flex items-center gap-2 min-w-0 flex-1">
                    <FaChalkboardTeacher className="text-[#7c3aed] shrink-0 text-sm" />
                    {value ? (
                        <div className="flex items-center gap-1.5 truncate">
                            <span className="font-bold text-[#1f2937] truncate">{value}</span>
                            {selectedItem?.role && (
                                <span
                                    className={`text-[10px] px-1.5 py-0.5 rounded font-bold shrink-0 ${
                                        selectedItem.role === "دبیر"
                                            ? "bg-[#f3e8ff] text-[#7e22ce]"
                                            : "bg-[#dbeafe] text-[#1d4ed8]"
                                    }`}
                                >
                                    {selectedItem.role}
                                </span>
                            )}
                        </div>
                    ) : (
                        <span className="text-[#6b7280] truncate">
                            {loadingTeachers ? "در حال دریافت لیست اساتید و دبیرها..." : "همه اساتید و دبیرها (انتخاب / جستجو)"}
                        </span>
                    )}
                </div>

                <div className="flex items-center gap-1.5 shrink-0">
                    {value && (
                        <button
                            type="button"
                            onClick={(e) => {
                                e.stopPropagation();
                                onChange("");
                                setQuery("");
                            }}
                            className="text-[#9ca3af] hover:text-[#dc2626] p-0.5 cursor-pointer"
                            title="حذف فیلتر استاد/دبیر"
                        >
                            <FaTimesCircle size={13} />
                        </button>
                    )}
                    <FaChevronDown
                        size={11}
                        className={`text-[#6b7280] transition-transform ${isOpen ? "rotate-180" : ""}`}
                    />
                </div>
            </div>

            {isOpen && (
                <div className="absolute right-0 left-0 mt-1.5 bg-white border border-[#e5e7eb] rounded-2xl shadow-xl z-40 overflow-hidden">
                    <div className="p-2.5 border-b border-[#f3f4f6] bg-[#f9fafb]">
                        <div className="relative">
                            <FaSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-xs" />
                            <input
                                type="text"
                                autoFocus
                                value={query}
                                onChange={(e) => setQuery(e.target.value)}
                                placeholder="جستجوی نام استاد یا دبیر..."
                                className="w-full pr-8 pl-3 py-1.5 text-xs border border-[#d1d5db] rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                            />
                        </div>
                    </div>

                    <div className="max-h-60 overflow-y-auto divide-y divide-[#f9fafb]">
                        <button
                            type="button"
                            onClick={() => {
                                onChange("");
                                setQuery("");
                                setIsOpen(false);
                            }}
                            className={`w-full text-right px-3.5 py-2 text-xs font-bold transition flex items-center justify-between cursor-pointer ${
                                !value ? "bg-[#eff6ff] text-[#1d4ed8]" : "text-[#4b5563] hover:bg-[#f9fafb]"
                            }`}
                        >
                            <span>همه اساتید و دبیرها</span>
                            {!value && <FaCheck size={10} />}
                        </button>

                        {filteredTeachers.length === 0 ? (
                            <div className="py-5 px-3 text-center text-xs text-[#9ca3af]">
                                استاد یا دبیری با این نام یافت نشد
                            </div>
                        ) : (
                            filteredTeachers.map((t) => {
                                const isSelected = value === t.fullname;
                                return (
                                    <button
                                        key={`${t.id}-${t.fullname}`}
                                        type="button"
                                        onClick={() => {
                                            onChange(t.fullname);
                                            setIsOpen(false);
                                        }}
                                        className={`w-full text-right px-3.5 py-2 text-xs transition flex items-center justify-between gap-2 cursor-pointer ${
                                            isSelected
                                                ? "bg-[#eff6ff] text-[#1d4ed8] font-bold"
                                                : "text-[#1f2937] hover:bg-[#f9fafb]"
                                        }`}
                                    >
                                        <span className="truncate">{t.fullname}</span>
                                        <span
                                            className={`text-[10px] px-2 py-0.5 rounded-full font-bold shrink-0 ${
                                                t.role === "دبیر"
                                                    ? "bg-[#f3e8ff] text-[#7e22ce]"
                                                    : "bg-[#dbeafe] text-[#1d4ed8]"
                                            }`}
                                        >
                                            {t.role || "استاد"}
                                        </span>
                                    </button>
                                );
                            })
                        )}
                    </div>
                </div>
            )}
        </div>
    );
};

const PollManagement = () => {
    const navigate = useNavigate();

    const [polls, setPolls] = useState([]);
    const [loading, setLoading] = useState(true);
    const [togglingId, setTogglingId] = useState(null);
    const [teachers, setTeachers] = useState([]);
    const [loadingTeachers, setLoadingTeachers] = useState(false);

    const [showModal, setShowModal] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [editingPoll, setEditingPoll] = useState(null);
    const [deletingPollId, setDeletingPollId] = useState(null);

    // Professional Filters State
    const [searchTerm, setSearchTerm] = useState("");
    const [filterType, setFilterType] = useState("all");
    const [filterStatus, setFilterStatus] = useState("all"); // all | active | inactive
    const [filterIsForAll, setFilterIsForAll] = useState("all"); // all | true | false
    const [teacherName, setTeacherName] = useState("");
    const [fromDate, setFromDate] = useState("");
    const [toDate, setToDate] = useState("");
    const [sortBy, setSortBy] = useState("newest"); // newest | oldest | votes

    const [formData, setFormData] = useState({
        question: "",
        type: 625,
        required: false,
        answers: [
            { content: "", score: 0 },
            { content: "", score: 0 },
            { content: "", score: 0 },
            { content: "", score: 0 }
        ]
    });

    const loadPolls = async () => {
        try {
            setLoading(true);
            const data = await GetPollListAdmin();
            setPolls(Array.isArray(data) ? data : []);
        } catch (err) {
            console.error("Error loading polls:", err);
        } finally {
            setLoading(false);
        }
    };

    const loadTeachersAndDabirs = async () => {
        try {
            setLoadingTeachers(true);
            const list = await GetTeacherList("", true);
            if (Array.isArray(list)) {
                const seen = new Map();
                list.forEach((item) => {
                    const fullname = (item.fullname || item.fullName || item.name || "").trim();
                    if (!fullname) return;
                    const key = `${normalizePersianText(fullname)}_${item.role || ""}`;
                    if (!seen.has(key)) {
                        seen.set(key, {
                            id: item.id || item.ID,
                            fullname,
                            role: item.role || "استاد"
                        });
                    }
                });
                setTeachers(Array.from(seen.values()));
            }
        } catch (err) {
            console.error("Error loading teachers & dabirs:", err);
        } finally {
            setLoadingTeachers(false);
        }
    };

    useEffect(() => {
        loadPolls();
        loadTeachersAndDabirs();
        document.title = "مدیریت نظرسنجی‌ها | پنل ادمین";
    }, []);

    // Summary stats
    const summaryStats = useMemo(() => {
        const total = polls.length;
        const active = polls.filter((p) => p.status === 300).length;
        const inactive = total - active;
        const isForAllCount = polls.filter((p) => p.isForAll === true).length;
        const totalVotes = polls.reduce((sum, p) => sum + (p.votesCount || 0), 0);
        const type625 = polls.filter((p) => p.type === 625).length;
        const type627 = polls.filter((p) => p.type === 627).length;
        const type626 = polls.filter((p) => p.type === 626).length;
        return { total, active, inactive, isForAllCount, totalVotes, type625, type627, type626 };
    }, [polls]);

    // Filtered & sorted polls
    const filteredPolls = useMemo(() => {
        let list = [...polls];

        if (searchTerm.trim()) {
            const normTerm = normalizePersianText(searchTerm);
            list = list.filter(
                (poll) =>
                    normalizePersianText(poll.question).includes(normTerm) ||
                    String(poll.id).includes(searchTerm.trim()) ||
                    normalizePersianText(poll.meetingTitle).includes(normTerm) ||
                    (poll.answers || []).some((a) => normalizePersianText(a.content).includes(normTerm))
            );
        }

        if (filterType !== "all") {
            const t = Number(filterType);
            list = list.filter((poll) => poll.type === t);
        }

        if (filterStatus === "active") {
            list = list.filter((poll) => poll.status === 300);
        } else if (filterStatus === "inactive") {
            list = list.filter((poll) => poll.status !== 300);
        }

        if (filterIsForAll === "true") {
            list = list.filter((poll) => poll.isForAll === true);
        } else if (filterIsForAll === "false") {
            list = list.filter((poll) => poll.isForAll === false);
        }

        if (teacherName.trim()) {
            const normTeacher = normalizePersianText(teacherName);
            list = list.filter((poll) =>
                normalizePersianText(poll.question).includes(normTeacher)
            );
        }

        if (fromDate) {
            const from = new Date(fromDate);
            from.setHours(0, 0, 0, 0);
            list = list.filter((poll) => poll.dateInsert && new Date(poll.dateInsert) >= from);
        }

        if (toDate) {
            const to = new Date(toDate);
            to.setHours(23, 59, 59, 999);
            list = list.filter((poll) => poll.dateInsert && new Date(poll.dateInsert) <= to);
        }

        list.sort((a, b) => {
            if (sortBy === "oldest") return (a.id || 0) - (b.id || 0);
            if (sortBy === "votes") return (b.votesCount || 0) - (a.votesCount || 0);
            return (b.id || 0) - (a.id || 0);
        });

        return list;
    }, [polls, searchTerm, filterType, filterStatus, filterIsForAll, teacherName, fromDate, toDate, sortBy]);

    const hasActiveFilters =
        Boolean(searchTerm.trim()) ||
        filterType !== "all" ||
        filterStatus !== "all" ||
        filterIsForAll !== "all" ||
        Boolean(teacherName.trim()) ||
        Boolean(fromDate) ||
        Boolean(toDate) ||
        sortBy !== "newest";

    const resetFilters = () => {
        setSearchTerm("");
        setFilterType("all");
        setFilterStatus("all");
        setFilterIsForAll("all");
        setTeacherName("");
        setFromDate("");
        setToDate("");
        setSortBy("newest");
    };

    const handleToggleStatus = async (poll) => {
        if (togglingId === poll.id) return;
        const nextStatus = poll.status === 300 ? -100 : 300;
        try {
            setTogglingId(poll.id);
            const res = await TogglePollStatus(poll.id, nextStatus);
            const updatedStatus = res?.pollStatus ?? nextStatus;
            setPolls((prev) =>
                prev.map((item) => (item.id === poll.id ? { ...item, status: updatedStatus } : item))
            );
        } catch (err) {
            console.error("Error toggling poll status:", err);
            alert(err?.message || "خطا در تغییر وضعیت نظرسنجی");
        } finally {
            setTogglingId(null);
        }
    };

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData((prev) => ({
            ...prev,
            [name]: type === "checkbox" ? checked : name === "type" ? parseInt(value) : value
        }));
    };

    const handleAnswerChange = (index, field, value) => {
        const newAnswers = [...formData.answers];
        newAnswers[index][field] = field === "score" ? parseInt(value) || 0 : value;
        setFormData((prev) => ({
            ...prev,
            answers: newAnswers
        }));
    };

    const addAnswerField = () => {
        setFormData((prev) => ({
            ...prev,
            answers: [...prev.answers, { content: "", score: 0 }]
        }));
    };

    const removeAnswerField = (index) => {
        if (formData.answers.length > 2) {
            const newAnswers = formData.answers.filter((_, i) => i !== index);
            setFormData((prev) => ({
                ...prev,
                answers: newAnswers
            }));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        let pollToSubmit;

        if (formData.type === 626) {
            pollToSubmit = {
                question: formData.question,
                type: formData.type,
                required: formData.required,
                ansPollRequestJSONs: [{ content: "INPUT", score: -1 }]
            };
        } else {
            const filteredAnswers = formData.answers.filter((ans) => ans.content.trim() !== "");

            if (filteredAnswers.length < 2) {
                alert("حداقل دو گزینه پاسخ وارد کنید");
                return;
            }
            pollToSubmit = {
                question: formData.question,
                type: formData.type,
                ansPollRequestJSONs: filteredAnswers
            };
        }

        if (!formData.question.trim()) {
            alert("لطفاً سوال نظرسنجی را وارد کنید");
            return;
        }

        try {
            if (editingPoll) {
                await UpdatePollQuiz(editingPoll.id, pollToSubmit);
            } else {
                await AddPollQuiz(pollToSubmit);
            }
            closeModal();
            await loadPolls();
        } catch (err) {
            console.error("Error saving poll:", err);
            alert("خطا در ذخیره نظرسنجی");
        }
    };

    const handleDelete = async () => {
        if (deletingPollId) {
            try {
                await DeletePollQuiz(deletingPollId);
                closeDeleteModal();
                await loadPolls();
            } catch (err) {
                console.error("Error deleting poll:", err);
                alert("خطا در حذف نظرسنجی");
            }
        }
    };

    const openModal = (poll = null) => {
        if (poll) {
            setEditingPoll(poll);
            if (poll.type === 626) {
                setFormData({
                    question: poll.question,
                    type: poll.type,
                    required: poll.required || false,
                    answers: []
                });
            } else {
                setFormData({
                    question: poll.question,
                    type: poll.type,
                    required: false,
                    answers: (poll.answers || []).map((ans) => ({
                        content: ans.content,
                        score: ans.score ?? 0
                    }))
                });
            }
        } else {
            setEditingPoll(null);
            setFormData({
                question: "",
                type: 625,
                required: false,
                answers: [
                    { content: "", score: 0 },
                    { content: "", score: 0 },
                    { content: "", score: 0 },
                    { content: "", score: 0 }
                ]
            });
        }
        setShowModal(true);
    };

    const closeModal = () => {
        setShowModal(false);
        setEditingPoll(null);
    };

    const openDeleteModal = (id) => {
        setDeletingPollId(id);
        setShowDeleteModal(true);
    };

    const closeDeleteModal = () => {
        setShowDeleteModal(false);
        setDeletingPollId(null);
    };

    const getTypeName = (type) => {
        switch (type) {
            case 626:
                return "متن آزاد (INPUT)";
            case 625:
                return "چند گزینه‌ای نمره‌دار";
            case 627:
                return "چند گزینه‌ای بدون نمره";
            default:
                return "نامشخص";
        }
    };

    const getTypeColor = (type) => {
        switch (type) {
            case 626:
                return "bg-purple-100 text-purple-800 border-purple-200";
            case 625:
                return "bg-blue-100 text-blue-800 border-blue-200";
            case 627:
                return "bg-orange-100 text-orange-800 border-orange-200";
            default:
                return "bg-gray-100 text-gray-800 border-gray-200";
        }
    };

    const renderStatusToggle = (poll) => {
        const isActive = poll.status === 300;
        const isBusy = togglingId === poll.id;
        return (
            <button
                type="button"
                disabled={isBusy}
                onClick={() => handleToggleStatus(poll)}
                title={isActive ? "کلیک جهت غیرفعال کردن نظرسنجی" : "کلیک جهت فعال کردن نظرسنجی"}
                className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-bold border transition-all cursor-pointer disabled:opacity-60 ${
                    isActive
                        ? "bg-[#dcfce7] text-[#15803d] border-[#86efac] hover:bg-[#bbf7d0]"
                        : "bg-[#fee2e2] text-[#b91c1c] border-[#fca5a5] hover:bg-[#fecaca]"
                }`}
            >
                {isBusy ? (
                    <FaSyncAlt className="animate-spin text-xs" />
                ) : (
                    <span
                        className={`relative inline-flex h-4 w-7 shrink-0 items-center rounded-full transition-colors ${
                            isActive ? "bg-[#16a34a]" : "bg-[#dc2626]"
                        }`}
                    >
                        <span
                            className={`inline-block h-3 w-3 transform rounded-full bg-white transition-transform ${
                                isActive ? "-translate-x-0.5" : "-translate-x-3.5"
                            }`}
                        />
                    </span>
                )}
                <span>{isActive ? "فعال" : "غیرفعال"}</span>
            </button>
        );
    };

    if (loading) {
        return <PollManagementSkeleton />;
    }

    return (
        <div className="min-h-screen bg-gradient-to-b from-[#f9fafb] to-[#ffffff]" dir="rtl">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-6 sm:py-10 space-y-6">
                {/* Header */}
                <div className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm p-5 sm:p-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                    <div className="flex items-start gap-3">
                        <div className="p-3 bg-gradient-to-br from-[#0b3f32] to-[#0a352a] text-white rounded-2xl shadow-sm shrink-0">
                            <FaPoll size={24} />
                        </div>
                        <div>
                            <h1 className="text-xl sm:text-2xl font-black text-[#111827]">
                                مدیریت نظرسنجی‌ها
                            </h1>
                            <p className="text-xs sm:text-sm text-[#6b7280] mt-1">
                                مدیریت سوالات، گزینه‌ها، وضعیت فعال/غیرفعال و فیلتر بر اساس استاد، دبیر و IsForAll
                            </p>
                        </div>
                    </div>

                    <div className="flex flex-wrap items-center gap-2">
                        <button
                            type="button"
                            onClick={() => navigate("/adminlay/polls/reports")}
                            className="inline-flex items-center gap-2 px-4 py-2.5 bg-[#ecfdf5] hover:bg-[#d1fae5] text-[#047857] border border-[#a7f3d0] rounded-xl font-bold text-xs sm:text-sm transition cursor-pointer"
                        >
                            <FaChartBar />
                            <span>گزارش کلی نظرسنجی</span>
                        </button>

                        <button
                            type="button"
                            onClick={loadPolls}
                            className="inline-flex items-center gap-2 px-3.5 py-2.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl font-bold text-xs sm:text-sm transition cursor-pointer"
                            title="بروزرسانی لیست"
                        >
                            <FaSyncAlt />
                            <span className="hidden sm:inline">بروزرسانی</span>
                        </button>

                        <button
                            type="button"
                            onClick={() => openModal()}
                            className="inline-flex items-center gap-2 bg-gradient-to-r from-[#0b3f32] to-[#0a352a] hover:from-[#0c4a3a] hover:to-[#0b3f32] text-white px-4 sm:px-5 py-2.5 rounded-xl font-bold text-xs sm:text-sm transition shadow-md cursor-pointer"
                        >
                            <FaPlus />
                            <span>نظرسنجی جدید</span>
                        </button>
                    </div>
                </div>

                {/* KPI Summary Cards */}
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3 sm:gap-4">
                    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
                        <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
                            <span>کل سوالات</span>
                            <FaPoll className="text-[#0284c7]" />
                        </div>
                        <div className="text-2xl font-black text-[#1f2937] tabular-nums">
                            {summaryStats.total}
                        </div>
                    </div>

                    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
                        <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
                            <span>سوالات فعال</span>
                            <FaCheckCircle className="text-[#16a34a]" />
                        </div>
                        <div className="text-2xl font-black text-[#16a34a] tabular-nums">
                            {summaryStats.active}
                        </div>
                    </div>

                    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
                        <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
                            <span>غیرفعال</span>
                            <FaBan className="text-[#dc2626]" />
                        </div>
                        <div className="text-2xl font-black text-[#dc2626] tabular-nums">
                            {summaryStats.inactive}
                        </div>
                    </div>

                    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
                        <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
                            <span>عمومی (IsForAll)</span>
                            <FaGlobe className="text-[#0d9488]" />
                        </div>
                        <div className="text-2xl font-black text-[#0d9488] tabular-nums">
                            {summaryStats.isForAllCount}
                        </div>
                    </div>

                    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs col-span-2 sm:col-span-1">
                        <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
                            <span>مجموع آراء ثبت‌شده</span>
                            <FaUsers className="text-[#7c3aed]" />
                        </div>
                        <div className="text-2xl font-black text-[#7c3aed] tabular-nums">
                            {summaryStats.totalVotes}
                        </div>
                    </div>
                </div>

                {/* Professional Filters Section */}
                <div className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm p-4 sm:p-5 space-y-4">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                        <div className="flex items-center gap-2 text-sm font-bold text-[#1f2937]">
                            <FaFilter className="text-[#0284c7]" />
                            <span>فیلترهای پیشرفته</span>
                            <span className="text-xs font-bold px-2.5 py-0.5 rounded-full bg-[#eff6ff] text-[#1d4ed8] border border-[#bfdbfe]">
                                {filteredPolls.length} مورد یافت شد
                            </span>
                        </div>

                        {/* Type Quick Filter Pills */}
                        <div className="flex flex-wrap gap-1.5">
                            {[
                                { id: "all", label: `همه (${summaryStats.total})` },
                                { id: "625", label: `نمره‌دار (${summaryStats.type625})` },
                                { id: "627", label: `بدون نمره (${summaryStats.type627})` },
                                { id: "626", label: `متن آزاد (${summaryStats.type626})` }
                            ].map((t) => (
                                <button
                                    key={t.id}
                                    type="button"
                                    onClick={() => setFilterType(t.id)}
                                    className={`px-3 py-1.5 rounded-xl text-xs font-bold transition cursor-pointer ${
                                        filterType === t.id
                                            ? "bg-[#0284c7] text-white shadow-xs"
                                            : "bg-[#f3f4f6] text-[#4b5563] hover:bg-[#e5e7eb]"
                                    }`}
                                >
                                    {t.label}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Row 1: Search, Searchable Teacher/Dabir Dropdown, IsForAll, Status */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
                        {/* Search */}
                        <div className="relative">
                            <FaSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-xs" />
                            <input
                                type="text"
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                placeholder="جستجو در سوال، شناسه یا گزینه‌ها..."
                                className="w-full pr-9 pl-8 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                            />
                            {searchTerm && (
                                <button
                                    type="button"
                                    onClick={() => setSearchTerm("")}
                                    className="absolute left-2.5 top-1/2 -translate-y-1/2 text-[#9ca3af] hover:text-[#4b5563] cursor-pointer"
                                >
                                    <FaTimesCircle size={13} />
                                </button>
                            )}
                        </div>

                        {/* Searchable Teacher & Dabir Dropdown */}
                        <SearchableTeacherDropdown
                            teachers={teachers}
                            loadingTeachers={loadingTeachers}
                            value={teacherName}
                            onChange={setTeacherName}
                        />

                        {/* isForAll Filter */}
                        <select
                            value={filterIsForAll}
                            onChange={(e) => setFilterIsForAll(e.target.value)}
                            className="w-full px-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                        >
                            <option value="all">همه سوالات (IsForAll: همه)</option>
                            <option value="true">فقط عمومی برای همه (IsForAll = بله)</option>
                            <option value="false">فقط اختصاصی / غیراشتراکی (IsForAll = خیر)</option>
                        </select>

                        {/* Status Filter */}
                        <select
                            value={filterStatus}
                            onChange={(e) => setFilterStatus(e.target.value)}
                            className="w-full px-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                        >
                            <option value="all">همه وضعیت‌ها</option>
                            <option value="active">فقط فعال</option>
                            <option value="inactive">فقط غیرفعال</option>
                        </select>
                    </div>

                    {/* Row 2: Dates, Sort, Reset */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 items-center">
                        <div className="flex items-center gap-1.5">
                            <span className="text-[11px] text-[#6b7280] shrink-0">از تاریخ:</span>
                            <input
                                type="date"
                                value={fromDate}
                                onChange={(e) => setFromDate(e.target.value)}
                                className="w-full px-2.5 py-2 text-xs border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                            />
                        </div>

                        <div className="flex items-center gap-1.5">
                            <span className="text-[11px] text-[#6b7280] shrink-0">تا تاریخ:</span>
                            <input
                                type="date"
                                value={toDate}
                                onChange={(e) => setToDate(e.target.value)}
                                className="w-full px-2.5 py-2 text-xs border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                            />
                        </div>

                        <div className="flex items-center gap-2">
                            <span className="text-[11px] text-[#6b7280] shrink-0">مرتب‌سازی:</span>
                            <select
                                value={sortBy}
                                onChange={(e) => setSortBy(e.target.value)}
                                className="w-full px-3 py-2 border border-[#d1d5db] rounded-xl bg-white text-xs font-bold text-[#374151] focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                            >
                                <option value="newest">جدیدترین</option>
                                <option value="oldest">قدیمی‌ترین</option>
                                <option value="votes">بیشترین رأی</option>
                            </select>
                        </div>

                        <div className="flex justify-end">
                            {hasActiveFilters && (
                                <button
                                    type="button"
                                    onClick={resetFilters}
                                    className="inline-flex items-center gap-1.5 px-4 py-2 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl text-xs font-bold transition cursor-pointer"
                                >
                                    <FaUndo size={11} />
                                    <span>پاک کردن فیلترها</span>
                                </button>
                            )}
                        </div>
                    </div>
                </div>

                {/* Desktop Table + Mobile Cards */}
                <div className="bg-white rounded-2xl shadow-sm border border-[#e5e7eb] overflow-hidden">
                    {/* Desktop View */}
                    <div className="hidden md:block overflow-x-auto">
                        <table className="w-full text-right">
                            <thead className="bg-[#f9fafb] border-b border-[#e5e7eb]">
                                <tr>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280]">شناسه</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280]">سوال و محدوده (IsForAll)</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280]">نوع</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280]">وضعیت (تغییر سریع)</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280] text-center">آراء</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280]">گزینه‌ها</th>
                                    <th className="px-5 py-3.5 text-xs font-bold text-[#6b7280] text-center">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-[#f3f4f6]">
                                {filteredPolls.map((poll) => (
                                    <tr key={poll.id} className="hover:bg-[#f9fafb] transition-colors">
                                        <td className="px-5 py-4 whitespace-nowrap text-sm font-mono font-bold text-[#111827]">
                                            #{poll.id}
                                        </td>
                                        <td className="px-5 py-4 text-sm text-[#1f2937] max-w-md space-y-1.5">
                                            <div className="font-bold line-clamp-2 leading-relaxed">
                                                {poll.question}
                                            </div>
                                            <div className="flex flex-wrap gap-1.5">
                                                {poll.isForAll ? (
                                                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-[#ecfeff] text-[#0e7490] border border-[#a5f3fc] text-[11px] font-bold">
                                                        <FaGlobe size={10} />
                                                        عمومی (IsForAll)
                                                    </span>
                                                ) : (
                                                    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md bg-[#fef3c7] text-[#92400e] border border-[#fde68a] text-[11px] font-bold">
                                                        <FaVideo size={10} />
                                                        {poll.meetingTitle || poll.meetID || (poll.idMeetingTBL ? `جلسه #${poll.idMeetingTBL}` : "اختصاصی")}
                                                    </span>
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-5 py-4 whitespace-nowrap">
                                            <span
                                                className={`inline-flex px-2.5 py-1 text-xs font-bold rounded-full border ${getTypeColor(
                                                    poll.type
                                                )}`}
                                            >
                                                {getTypeName(poll.type)}
                                            </span>
                                        </td>
                                        <td className="px-5 py-4 whitespace-nowrap">
                                            {renderStatusToggle(poll)}
                                        </td>
                                        <td className="px-5 py-4 whitespace-nowrap text-center">
                                            <span className="inline-flex px-2.5 py-1 rounded-lg bg-[#f3f4f6] text-[#1f2937] text-xs font-black tabular-nums">
                                                {poll.votesCount ?? 0}
                                            </span>
                                        </td>
                                        <td className="px-5 py-4 text-xs text-[#4b5563]">
                                            <div className="flex flex-wrap gap-1.5">
                                                {poll.type === 626 ? (
                                                    <span className="inline-flex items-center gap-1 bg-[#f3f4f6] px-2 py-0.5 rounded">
                                                        <span className="w-1.5 h-1.5 rounded-full bg-purple-500"></span>
                                                        پاسخ متنی {poll.required && "(اجباری)"}
                                                    </span>
                                                ) : (
                                                    (poll.answers || []).map((answer) => (
                                                        <span
                                                            key={answer.id}
                                                            className="inline-flex items-center gap-1 bg-[#f3f4f6] px-2 py-0.5 rounded"
                                                        >
                                                            <span className="w-1.5 h-1.5 rounded-full bg-[#30d4cc]"></span>
                                                            <span>{answer.content}</span>
                                                            {poll.type === 625 && (
                                                                <span className="text-[10px] text-[#0284c7] font-bold">
                                                                    ({answer.score ?? 0})
                                                                </span>
                                                            )}
                                                        </span>
                                                    ))
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-5 py-4 whitespace-nowrap text-center">
                                            <div className="flex items-center justify-center gap-3">
                                                <button
                                                    type="button"
                                                    onClick={() => openModal(poll)}
                                                    className="p-1.5 rounded-lg text-[#0284c7] hover:bg-[#e0f2fe] transition-colors cursor-pointer"
                                                    title="ویرایش"
                                                >
                                                    <FaEdit className="w-4 h-4" />
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => openDeleteModal(poll.id)}
                                                    className="p-1.5 rounded-lg text-red-500 hover:bg-red-50 transition-colors cursor-pointer"
                                                    title="حذف"
                                                >
                                                    <FaTrash className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    {/* Mobile Cards View */}
                    <div className="block md:hidden divide-y divide-[#f3f4f6]">
                        {filteredPolls.map((poll) => (
                            <div key={poll.id} className="p-4 space-y-3">
                                <div className="flex items-start justify-between gap-2">
                                    <div className="space-y-1 flex-1">
                                        <div className="flex flex-wrap items-center gap-1.5">
                                            <span className="text-xs font-mono font-bold text-[#6b7280]">
                                                #{poll.id}
                                            </span>
                                            <span
                                                className={`px-2 py-0.5 text-[11px] font-bold rounded-full border ${getTypeColor(
                                                    poll.type
                                                )}`}
                                            >
                                                {getTypeName(poll.type)}
                                            </span>
                                        </div>
                                        <h3 className="font-bold text-sm text-[#111827] leading-snug">
                                            {poll.question}
                                        </h3>
                                    </div>
                                    <div className="shrink-0">{renderStatusToggle(poll)}</div>
                                </div>

                                <div className="flex flex-wrap items-center gap-2 text-xs">
                                    {poll.isForAll ? (
                                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-[#ecfeff] text-[#0e7490] text-[11px] font-bold">
                                            <FaGlobe size={10} /> عمومی (IsForAll)
                                        </span>
                                    ) : (
                                        <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded bg-[#fef3c7] text-[#92400e] text-[11px] font-bold">
                                            <FaVideo size={10} /> {poll.meetingTitle || poll.meetID || "اختصاصی"}
                                        </span>
                                    )}
                                    <span className="text-[#4b5563]">
                                        تعداد آراء: <strong className="text-[#0284c7]">{poll.votesCount ?? 0}</strong>
                                    </span>
                                </div>

                                {poll.type !== 626 && poll.answers?.length > 0 && (
                                    <div className="flex flex-wrap gap-1">
                                        {poll.answers.map((answer) => (
                                            <span
                                                key={answer.id}
                                                className="inline-flex items-center gap-1 bg-[#f3f4f6] px-2 py-0.5 rounded text-[11px] text-[#374151]"
                                            >
                                                <span>{answer.content}</span>
                                                {poll.type === 625 && (
                                                    <span className="text-[#0284c7] font-bold">({answer.score ?? 0})</span>
                                                )}
                                            </span>
                                        ))}
                                    </div>
                                )}

                                <div className="flex items-center justify-end gap-2 pt-2 border-t border-[#f3f4f6]">
                                    <button
                                        type="button"
                                        onClick={() => openModal(poll)}
                                        className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-[#eff6ff] text-[#0284c7] rounded-xl text-xs font-bold cursor-pointer"
                                    >
                                        <FaEdit size={12} />
                                        <span>ویرایش</span>
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => openDeleteModal(poll.id)}
                                        className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-[#fef2f2] text-[#dc2626] rounded-xl text-xs font-bold cursor-pointer"
                                    >
                                        <FaTrash size={12} />
                                        <span>حذف</span>
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>

                    {filteredPolls.length === 0 && (
                        <div className="text-center py-16">
                            <div className="w-20 h-20 bg-[#f3f4f6] rounded-full flex items-center justify-center mx-auto mb-4">
                                <FaPoll className="w-10 h-10 text-[#9ca3af]" />
                            </div>
                            <p className="text-[#6b7280] text-lg font-medium">
                                {hasActiveFilters ? "نتیجه‌ای مطابق فیلترها یافت نشد" : "هیچ نظرسنجی یافت نشد"}
                            </p>
                            <p className="text-[#9ca3af] text-sm mt-2">
                                {hasActiveFilters
                                    ? "لطفاً فیلترهای جستجو را تغییر دهید یا پاک کنید"
                                    : "با کلیک روی دکمه 'نظرسنجی جدید' اولین نظرسنجی را ایجاد کنید"}
                            </p>
                        </div>
                    )}
                </div>
            </div>

            {/* Add/Edit Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black/50 backdrop-blur-xs flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl max-w-3xl w-full my-8">
                        <div className="flex justify-between items-center p-6 border-b border-[#f3f4f6]">
                            <h2 className="text-xl font-bold text-[#111827]">
                                {editingPoll ? "ویرایش نظرسنجی" : "نظرسنجی جدید"}
                            </h2>
                            <button
                                type="button"
                                onClick={closeModal}
                                className="text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
                            >
                                <FaTimes className="w-6 h-6" />
                            </button>
                        </div>

                        <form onSubmit={handleSubmit} className="p-6 space-y-6">
                            <div>
                                <label className="block text-sm font-medium text-[#374151] mb-2">
                                    سوال نظرسنجی <span className="text-red-500">*</span>
                                </label>
                                <input
                                    type="text"
                                    name="question"
                                    value={formData.question}
                                    onChange={handleInputChange}
                                    required
                                    className="w-full px-4 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                    placeholder="مثال: موضوع نشست از نظر شما چگونه بود؟"
                                />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-[#374151] mb-2">
                                    نوع نظرسنجی
                                </label>
                                <select
                                    name="type"
                                    value={formData.type}
                                    onChange={handleInputChange}
                                    className="w-full px-4 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                >
                                    <option value={625}>چند گزینه‌ای نمره‌دار (OPTIONAL)</option>
                                    <option value={626}>پاسخ متنی (INPUT)</option>
                                    <option value={627}>چند گزینه‌ای بدون نمره (OPTIONAL NO SCORE)</option>
                                </select>
                            </div>

                            {formData.type === 626 && (
                                <div className="flex items-center gap-3">
                                    <input
                                        type="checkbox"
                                        name="required"
                                        checked={formData.required}
                                        onChange={handleInputChange}
                                        className="w-4 h-4 text-[#30d4cc] rounded focus:ring-[#30d4cc]"
                                    />
                                    <label className="text-sm font-medium text-[#374151]">
                                        پاسخ به این سوال الزامی است
                                    </label>
                                </div>
                            )}

                            {formData.type !== 626 && (
                                <div>
                                    <div className="flex justify-between items-center mb-3">
                                        <label className="block text-sm font-medium text-[#374151]">
                                            گزینه‌های پاسخ <span className="text-red-500">*</span>
                                        </label>
                                        <button
                                            type="button"
                                            onClick={addAnswerField}
                                            className="text-[#0284c7] hover:text-[#0369a1] text-sm font-bold flex items-center gap-1 transition-colors cursor-pointer"
                                        >
                                            <FaPlus className="w-3 h-3" />
                                            <span>افزودن گزینه</span>
                                        </button>
                                    </div>

                                    <div className="space-y-3">
                                        {formData.answers.map((answer, index) => (
                                            <div key={index} className="flex gap-3 items-center">
                                                <div className="flex-1">
                                                    <input
                                                        type="text"
                                                        value={answer.content}
                                                        onChange={(e) =>
                                                            handleAnswerChange(index, "content", e.target.value)
                                                        }
                                                        placeholder={`گزینه ${index + 1}`}
                                                        className="w-full px-4 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                                    />
                                                </div>
                                                <div className="w-28">
                                                    <input
                                                        type="number"
                                                        value={answer.score}
                                                        onChange={(e) =>
                                                            handleAnswerChange(index, "score", e.target.value)
                                                        }
                                                        placeholder="امتیاز"
                                                        className="w-full px-3 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                                    />
                                                </div>
                                                {formData.answers.length > 2 && (
                                                    <button
                                                        type="button"
                                                        onClick={() => removeAnswerField(index)}
                                                        className="text-red-500 hover:text-red-700 transition-colors cursor-pointer"
                                                    >
                                                        <FaTrash className="w-4 h-4" />
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                    <p className="text-xs text-gray-500 mt-2">حداقل ۲ گزینه پاسخ الزامی است</p>
                                </div>
                            )}

                            <div className="flex justify-end gap-3 pt-4 border-t border-[#f3f4f6]">
                                <button
                                    type="button"
                                    onClick={closeModal}
                                    className="px-6 py-2 border border-[#e5e7eb] rounded-xl text-[#6b7280] hover:bg-[#f9fafb] transition-all cursor-pointer"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="submit"
                                    className="px-6 py-2 bg-gradient-to-r from-[#0b3f32] to-[#0a352a] text-white rounded-xl hover:shadow-lg transition-all flex items-center gap-2 cursor-pointer"
                                >
                                    <FaCheck className="w-4 h-4" />
                                    {editingPoll ? "بروزرسانی" : "افزودن"}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {showDeleteModal && (
                <div className="fixed inset-0 bg-black/50 backdrop-blur-xs flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-xl max-w-md w-full">
                        <div className="p-6">
                            <div className="flex items-center justify-center mb-4">
                                <div className="bg-red-100 rounded-full p-3">
                                    <FaTrash className="w-8 h-8 text-red-600" />
                                </div>
                            </div>
                            <h3 className="text-lg font-bold text-[#111827] text-center mb-2">
                                تأیید حذف
                            </h3>
                            <p className="text-[#6b7280] text-center mb-6">
                                آیا از حذف این نظرسنجی اطمینان دارید؟ این عمل قابل بازگشت نیست.
                            </p>
                            <div className="flex justify-center gap-3">
                                <button
                                    type="button"
                                    onClick={closeDeleteModal}
                                    className="px-6 py-2 border border-[#e5e7eb] rounded-xl text-[#6b7280] hover:bg-[#f9fafb] transition-all cursor-pointer"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="button"
                                    onClick={handleDelete}
                                    className="px-6 py-2 bg-red-600 text-white rounded-xl hover:bg-red-700 transition-all cursor-pointer"
                                >
                                    حذف
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default PollManagement;
