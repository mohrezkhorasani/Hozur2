// src/Pages/Admin/PollManagement.jsx
import React, { useState, useEffect } from "react";
import { FaPlus, FaEdit, FaTrash, FaTimes, FaCheck, FaPoll, FaSearch, FaFilter, FaTimesCircle } from "react-icons/fa";
import { GetPollListAdmin, AddPollQuiz, UpdatePollQuiz, DeletePollQuiz } from "../api/AdminAPI";

const PollManagement = () => {
    const [polls, setPolls] = useState([]);
    const [filteredPolls, setFilteredPolls] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [editingPoll, setEditingPoll] = useState(null);
    const [deletingPollId, setDeletingPollId] = useState(null);
    
    // Filters state
    const [searchTerm, setSearchTerm] = useState("");
    const [filterType, setFilterType] = useState("all");
    const [filterStatus, setFilterStatus] = useState("all");
    const [showFilters, setShowFilters] = useState(false);
    
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

    // Load polls data
    const loadPolls = async () => {
        try {
            setLoading(true);
            const data = await GetPollListAdmin();
            setPolls(data);
            setFilteredPolls(data);
        } catch (err) {
            console.error("Error loading polls:", err);
        } finally {
            setLoading(false);
        }
    };

    // Apply filters
    const applyFilters = () => {
        let filtered = [...polls];
        
        // Search filter
        if (searchTerm.trim()) {
            filtered = filtered.filter(poll => 
                poll.question.toLowerCase().includes(searchTerm.toLowerCase()) ||
                poll.id.toString().includes(searchTerm)
            );
        }
        
        // Type filter
        if (filterType !== "all") {
            filtered = filtered.filter(poll => poll.type === parseInt(filterType));
        }
        
        // Status filter
        if (filterStatus !== "all") {
            filtered = filtered.filter(poll => poll.status === parseInt(filterStatus));
        }
        
        setFilteredPolls(filtered);
    };

    // Reset filters
    const resetFilters = () => {
        setSearchTerm("");
        setFilterType("all");
        setFilterStatus("all");
        setFilteredPolls(polls);
    };

    useEffect(() => {
        loadPolls();
        document.title = "مدیریت نظرسنجی‌ها | پنل ادمین";
    }, []);

    useEffect(() => {
        applyFilters();
    }, [searchTerm, filterType, filterStatus, polls]);

    // Form handlers
    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === "checkbox" ? checked : (name === "type" ? parseInt(value) : value)
        }));
    };

    const handleAnswerChange = (index, field, value) => {
        const newAnswers = [...formData.answers];
        newAnswers[index][field] = field === "score" ? parseInt(value) || 0 : value;
        setFormData(prev => ({
            ...prev,
            answers: newAnswers
        }));
    };

    const addAnswerField = () => {
        setFormData(prev => ({
            ...prev,
            answers: [...prev.answers, { content: "", score: 0 }]
        }));
    };

    const removeAnswerField = (index) => {
        if (formData.answers.length > 2) {
            const newAnswers = formData.answers.filter((_, i) => i !== index);
            setFormData(prev => ({
                ...prev,
                answers: newAnswers
            }));
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        let pollToSubmit;
        
        if (formData.type === 626) { // INPUT type
            pollToSubmit = {
                question: formData.question,
                type: formData.type,
                required: formData.required,
                ansPollRequestJSONs: [{ content: "INPUT", score: -1 }]
            };
        } else {
            const filteredAnswers = formData.answers.filter(ans => ans.content.trim() !== "");
            
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
                    answers: poll.answers.map(ans => ({
                        content: ans.content,
                        score: 0
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
            case 626: return "INPUT";
            case 625: return "OPTIONAL";
            case 627: return "OPTIONAL NO SCORE";
            default: return "نامشخص";
        }
    };

    const getTypeColor = (type) => {
        switch (type) {
            case 626: return "bg-purple-100 text-purple-800";
            case 625: return "bg-blue-100 text-blue-800";
            case 627: return "bg-orange-100 text-orange-800";
            default: return "bg-gray-100 text-gray-800";
        }
    };

    const getStatusName = (status) => {
        switch (status) {
            case 300: return "فعال";
            default: return "غیرفعال";
        }
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 300: return "bg-green-100 text-green-800";
            default: return "bg-red-100 text-red-800";
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-b from-[#f9fafb] to-[#ffffff]">
                <div className="container mx-auto px-4 py-8">
                    <div className="text-center py-20">
                        <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-[#30d4cc]"></div>
                        <p className="mt-4 text-gray-600">در حال دریافت اطلاعات...</p>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-b from-[#f9fafb] to-[#ffffff]">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8 sm:py-12">

                {/* Header Section */}
                <div className="mb-8 sm:mb-12">
                    <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                        <div>
                            <div className="inline-flex items-center gap-2 bg-[#30d4cc]/10 px-4 py-1.5 rounded-full mb-4">
                                <FaPoll className="text-[#30d4cc] text-sm" />
                                <span className="text-[#30d4cc] text-sm font-semibold tracking-wide">مدیریت</span>
                            </div>
                            <h1 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-[#111827] mb-3">
                                نظرسنجی‌ها
                            </h1>
                            <p className="text-[#6b7280] text-sm sm:text-base max-w-2xl">
                                مدیریت نظرسنجی‌ها، سوالات و گزینه‌های پاسخ
                            </p>
                        </div>

                        <button
                            onClick={() => openModal()}
                            className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] hover:from-[#0c4a3a] hover:to-[#0b3f32] text-white px-5 py-2.5 rounded-xl transition-all duration-300 flex items-center gap-2 shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
                        >
                            <FaPlus className="text-sm" />
                            <span>نظرسنجی جدید</span>
                        </button>
                    </div>
                </div>

                {/* Search and Filters Section */}
                <div className="mb-6 space-y-4">
                    {/* Search Bar */}
                    <div className="flex gap-3">
                        <div className="flex-1 relative">
                            <FaSearch className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                            <input
                                type="text"
                                placeholder="جستجو بر اساس سوال یا شناسه..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="w-full pr-10 pl-4 py-2.5 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all bg-white"
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
                            className={`px-4 py-2.5 rounded-xl border transition-all duration-200 flex items-center gap-2 ${
                                showFilters || filterType !== "all" || filterStatus !== "all"
                                    ? "bg-[#30d4cc] text-white border-[#30d4cc]"
                                    : "bg-white text-gray-700 border-[#e5e7eb] hover:border-[#30d4cc]"
                            }`}
                        >
                            <FaFilter className="text-sm" />
                            <span>فیلتر</span>
                        </button>
                        {(filterType !== "all" || filterStatus !== "all") && (
                            <button
                                onClick={resetFilters}
                                className="px-4 py-2.5 rounded-xl border border-[#e5e7eb] bg-white text-gray-700 hover:bg-gray-50 transition-all flex items-center gap-2"
                            >
                                <FaTimes className="text-sm" />
                                <span>حذف فیلترها</span>
                            </button>
                        )}
                    </div>

                    {/* Filter Options */}
                    {showFilters && (
                        <div className="bg-white rounded-xl border border-[#e5e7eb] p-4 shadow-sm">
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-[#374151] mb-2">
                                        نوع نظرسنجی
                                    </label>
                                    <select
                                        value={filterType}
                                        onChange={(e) => setFilterType(e.target.value)}
                                        className="w-full px-3 py-2 border border-[#e5e7eb] rounded-lg focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent"
                                    >
                                        <option value="all">همه</option>
                                        <option value="625">OPTIONAL</option>
                                        <option value="626">INPUT</option>
                                        <option value="627">OPTIONAL NO SCORE</option>
                                    </select>
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-[#374151] mb-2">
                                        وضعیت
                                    </label>
                                    <select
                                        value={filterStatus}
                                        onChange={(e) => setFilterStatus(e.target.value)}
                                        className="w-full px-3 py-2 border border-[#e5e7eb] rounded-lg focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent"
                                    >
                                        <option value="all">همه</option>
                                        <option value="300">فعال</option>
                                        <option value="400">غیرفعال</option>
                                    </select>
                                </div>
                            </div>
                        </div>
                    )}
                </div>

                {/* Stats Bar */}
                <div className="mb-6 flex justify-between items-center bg-[#ffffff] rounded-xl px-5 py-3 shadow-sm border border-[#f3f4f6]">
                    <div className="flex items-center gap-2">
                        <div className="w-2 h-2 bg-[#30d4cc] rounded-full"></div>
                        <span className="text-sm text-[#4b5563]">تعداد کل نظرسنجی‌ها:</span>
                    </div>
                    <span className="font-bold text-[#30d4cc] text-lg">{filteredPolls.length}</span>
                </div>

                {/* Polls Table */}
                <div className="bg-[#ffffff] rounded-2xl shadow-sm border border-[#f3f4f6] overflow-hidden">
                    <div className="overflow-x-auto">
                        <table className="w-full">
                            <thead className="bg-[#f9fafb] border-b border-[#f3f4f6]">
                                <tr>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">شناسه</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">سوال</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">نوع</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">وضعیت</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">گزینه‌ها</th>
                                    <th className="px-6 py-4 text-right text-xs font-medium text-[#6b7280] uppercase tracking-wider">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-[#f3f4f6]">
                                {filteredPolls.map((poll) => (
                                    <tr key={poll.id} className="hover:bg-[#f9fafb] transition-colors duration-200">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-[#111827] font-medium">{poll.id}</td>
                                        <td className="px-6 py-4 text-sm text-[#374151] max-w-md">
                                            <div className="line-clamp-2">{poll.question}</div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getTypeColor(poll.type)}`}>
                                                {getTypeName(poll.type)}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(poll.status)}`}>
                                                {getStatusName(poll.status)}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 text-sm text-[#6b7280]">
                                            <div className="flex flex-wrap gap-1">
                                                {poll.type === 626 ? (
                                                    <span className="inline-flex items-center gap-1 bg-[#f3f4f6] px-2 py-0.5 rounded text-xs">
                                                        <span className="w-1.5 h-1.5 rounded-full bg-purple-500"></span>
                                                        پاسخ متنی {poll.required && "(اجباری)"}
                                                    </span>
                                                ) : (
                                                    poll.answers.map((answer, idx) => (
                                                        <span key={answer.id} className="inline-flex items-center gap-1 bg-[#f3f4f6] px-2 py-0.5 rounded text-xs">
                                                            <span className="w-1.5 h-1.5 rounded-full bg-[#30d4cc]"></span>
                                                            {answer.content}
                                                        </span>
                                                    ))
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm">
                                            <div className="flex items-center gap-3">
                                                <button
                                                    onClick={() => openModal(poll)}
                                                    className="text-[#30d4cc] hover:text-[#0b3f32] transition-colors"
                                                    title="ویرایش"
                                                >
                                                    <FaEdit className="w-5 h-5" />
                                                </button>
                                                <button
                                                    onClick={() => openDeleteModal(poll.id)}
                                                    className="text-red-500 hover:text-red-700 transition-colors"
                                                    title="حذف"
                                                >
                                                    <FaTrash className="w-5 h-5" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    {filteredPolls.length === 0 && (
                        <div className="text-center py-16">
                            <div className="w-20 h-20 bg-[#f3f4f6] rounded-full flex items-center justify-center mx-auto mb-4">
                                <FaPoll className="w-10 h-10 text-[#9ca3af]" />
                            </div>
                            <p className="text-[#6b7280] text-lg font-medium">
                                {searchTerm || filterType !== "all" || filterStatus !== "all" 
                                    ? "نتیجه‌ای یافت نشد" 
                                    : "هیچ نظرسنجی یافت نشد"}
                            </p>
                            <p className="text-[#9ca3af] text-sm mt-2">
                                {searchTerm || filterType !== "all" || filterStatus !== "all"
                                    ? "لطفاً فیلترهای جستجو را تغییر دهید"
                                    : "با کلیک روی دکمه 'نظرسنجی جدید' اولین نظرسنجی را ایجاد کنید"}
                            </p>
                        </div>
                    )}
                </div>
            </div>

            {/* Add/Edit Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-white rounded-2xl shadow-xl max-w-3xl w-full my-8">
                        <div className="flex justify-between items-center p-6 border-b border-[#f3f4f6]">
                            <h2 className="text-xl font-bold text-[#111827]">
                                {editingPoll ? "ویرایش نظرسنجی" : "نظرسنجی جدید"}
                            </h2>
                            <button
                                onClick={closeModal}
                                className="text-gray-400 hover:text-gray-600 transition-colors"
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
                                    <option value={625}>OPTIONAL (چند گزینه‌ای)</option>
                                    <option value={626}>INPUT (پاسخ متنی)</option>
                                    <option value={627}>OPTIONAL NO SCORE (بدون امتیاز)</option>
                                </select>
                            </div>

                            {/* Required checkbox for INPUT type */}
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

                            {/* Answers section for non-INPUT types */}
                            {formData.type !== 626 && (
                                <div>
                                    <div className="flex justify-between items-center mb-3">
                                        <label className="block text-sm font-medium text-[#374151]">
                                            گزینه‌های پاسخ <span className="text-red-500">*</span>
                                        </label>
                                        <button
                                            type="button"
                                            onClick={addAnswerField}
                                            className="text-[#30d4cc] hover:text-[#0b3f32] text-sm flex items-center gap-1 transition-colors"
                                        >
                                            <FaPlus className="w-3 h-3" />
                                            <span>افزودن گزینه</span>
                                        </button>
                                    </div>

                                    <div className="space-y-3">
                                        {formData.answers.map((answer, index) => {
                                            // Auto-calculate score based on number of answers
                                            const count = formData.answers.filter(a => a.content.trim() !== "" || a.content === "").length;
                                            const each = 100 / count;
                                            let defScore = Math.round((index + 1) * each);
                                            if (index + 1 === formData.answers.length) {
                                                defScore = 100;
                                            }
                                            if (answer.score === 0 && answer.content) {
                                                answer.score = defScore;
                                            }
                                            
                                            return (
                                                <div key={index} className="flex gap-3 items-center">
                                                    <div className="flex-1">
                                                        <input
                                                            type="text"
                                                            value={answer.content}
                                                            onChange={(e) => handleAnswerChange(index, "content", e.target.value)}
                                                            placeholder={`گزینه ${index + 1}`}
                                                            className="w-full px-4 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                                        />
                                                    </div>
                                                    <div className="w-28">
                                                        <input
                                                            type="number"
                                                            value={answer.score}
                                                            onChange={(e) => handleAnswerChange(index, "score", e.target.value)}
                                                            placeholder="امتیاز"
                                                            className="w-full px-3 py-2 border border-[#e5e7eb] rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
                                                        />
                                                    </div>
                                                    {formData.answers.length > 2 && (
                                                        <button
                                                            type="button"
                                                            onClick={() => removeAnswerField(index)}
                                                            className="text-red-500 hover:text-red-700 transition-colors"
                                                        >
                                                            <FaTrash className="w-4 h-4" />
                                                        </button>
                                                    )}
                                                </div>
                                            );
                                        })}
                                    </div>
                                    <p className="text-xs text-gray-500 mt-2">حداقل ۲ گزینه پاسخ الزامی است</p>
                                </div>
                            )}

                            <div className="flex justify-end gap-3 pt-4 border-t border-[#f3f4f6]">
                                <button
                                    type="button"
                                    onClick={closeModal}
                                    className="px-6 py-2 border border-[#e5e7eb] rounded-xl text-[#6b7280] hover:bg-[#f9fafb] transition-all"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="submit"
                                    className="px-6 py-2 bg-gradient-to-r from-[#0b3f32] to-[#0a352a] text-white rounded-xl hover:shadow-lg transition-all flex items-center gap-2"
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
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
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
                                    onClick={closeDeleteModal}
                                    className="px-6 py-2 border border-[#e5e7eb] rounded-xl text-[#6b7280] hover:bg-[#f9fafb] transition-all"
                                >
                                    انصراف
                                </button>
                                <button
                                    onClick={handleDelete}
                                    className="px-6 py-2 bg-red-600 text-white rounded-xl hover:bg-red-700 transition-all"
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