// src/Pages/Admin/PollReportPage.jsx
import React, { useState, useMemo, useEffect } from "react";
import { useRef } from "react";
import { useParams, useSearchParams, useNavigate } from "react-router-dom";

import { createPortal } from 'react-dom';
import {
    FaArrowRight,
    FaChartLine,
    FaChartPie,
    FaChartBar,
    FaChartArea,
    FaCheckCircle,
    FaChevronLeft,
    FaChevronRight,
    FaClipboardList,
    FaEye,
    FaFilter,
    FaPoll,
    FaQuestionCircle,
    FaSearch,
    FaSpinner,
    FaStar,
    FaTimesCircle,
    FaUsers,
    FaDownload,
    FaKeyboard,
    FaTasks,
    FaAward,
    FaCalculator,
    FaCopy,
    FaCheck,
    FaTrophy,
    FaMedal,
    FaThumbsUp,
    FaPercentage,
    FaFilePdf,
} from "react-icons/fa";
import html2pdf from "html2pdf.js";
import {
    LineChart,
    Line,
    BarChart,
    Bar,
    PieChart,
    Pie,
    Cell,
    RadarChart,
    Radar,
    PolarGrid,
    PolarAngleAxis,
    PolarRadiusAxis,
    AreaChart,
    Area,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer
} from "recharts";
import { GetPollReport } from "../../api/AdminAPI";
import { BaseURLSite } from "../../api/apiNeed";

const COLORS = ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#06B6D4', '#84CC16', '#F97316', '#6366F1'];

const PollReportPage = () => {
    const { meetid } = useParams();
    const [searchParams, setSearchParams] = useSearchParams();
    const navigate = useNavigate();

    const [reportData, setReportData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [filterType, setFilterType] = useState("all");
    const [currentPage, setCurrentPage] = useState(1);
    const [selectedPoll, setSelectedPoll] = useState(null);
    const [showDetailModal, setShowDetailModal] = useState(false);
    const [copied, setCopied] = useState(false);
    const [hoveredData, setHoveredData] = useState(null);
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);
    const pdfPrintRef = useRef(null);

    const qidFromUrl = searchParams.get('qid');
    const useridFromUrl = searchParams.get('userid');

    const [filters, setFilters] = useState({
        qid: qidFromUrl ? parseInt(qidFromUrl) : null,
        userid: useridFromUrl || null
    });

    const [showFiltersPanel, setShowFiltersPanel] = useState(false);
    const [userIdInput, setUserIdInput] = useState(filters.userid || "");
    const [pollIdInput, setPollIdInput] = useState(filters.qid?.toString() || "");

    const itemsPerPage = 10;

    const loadReport = async () => {
        try {
            setLoading(true);
            const response = await GetPollReport(meetid, filters.qid, filters.userid);
            if (response && response.status === 200) {
                setReportData(response);
            }
        } catch (error) {
            console.error("Error loading poll report:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const newParams = new URLSearchParams();
        if (filters.qid) newParams.set('qid', filters.qid.toString());
        if (filters.userid) newParams.set('userid', filters.userid);
        setSearchParams(newParams, { replace: true });
    }, [filters.qid, filters.userid, setSearchParams]);

    useEffect(() => {
        if (meetid) loadReport();
    }, [meetid, filters.qid, filters.userid]);

    const filteredPolls = reportData?.data?.filter(poll => {
        const matchesSearch = poll.question?.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesType = filterType === "all" ||
            (filterType === "optional" && (poll.type === 625 || poll.type === 627)) ||
            (filterType === "input" && poll.type === 626);
        return matchesSearch && matchesType;
    }) || [];

    const totalPages = Math.ceil(filteredPolls.length / itemsPerPage);
    const paginatedPolls = filteredPolls.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    const getTypeName = (type) => {
        switch (type) {
            case 625: return "چند گزینه‌ای (نمره‌دار)";
            case 626: return "متن آزاد";
            case 627: return "چند گزینه‌ای (بدون نمره)";
            default: return "نامشخص";
        }
    };

    const getTypeColor = (type) => {
        switch (type) {
            case 625: return "from-blue-500 to-blue-600";
            case 626: return "from-green-500 to-green-600";
            case 627: return "from-purple-500 to-purple-600";
            default: return "from-gray-500 to-gray-600";
        }
    };

    const applyFilters = () => {
        setFilters({
            qid: pollIdInput ? parseInt(pollIdInput) : null,
            userid: userIdInput || null
        });
        setCurrentPage(1);
        setShowFiltersPanel(false);
    };

    const resetFilters = () => {
        setPollIdInput("");
        setUserIdInput("");
        setFilters({ qid: null, userid: null });
        setSearchTerm("");
        setFilterType("all");
        setCurrentPage(1);
    };

    const copyMeetingLink = async () => {
        const link = `${BaseURLSite}/rooms/${meetid}/join`;
        await navigator.clipboard.writeText(link);
        setCopied(true);
        setTimeout(() => setCopied(false), 2000);
    };
    // ============ محاسبات برای Tooltip کارت‌ها ============
    const tooltipData = useMemo(() => {
        // داده برای کارت "کل سوالات"
        const allQuestionsData = reportData?.data?.map(p => ({
            id: p.id,
            question: p.question,
            type: getTypeName(p.type)
        })) || [];

        // داده برای کارت "چند گزینه‌ای"
        const optionalQuestionsData = reportData?.data
            ?.filter(p => p.type === 625 || p.type === 627)
            .map(p => ({
                id: p.id,
                question: p.question,
                type: p.type === 625 ? "نمره‌دار" : "بدون نمره",
                overallScore: p.overallAverageScore,
                totalAnswers: p.answersReport?.reduce((sum, a) => sum + a.count, 0) || 0
            })) || [];

        // داده برای کارت "متن آزاد" - با نمایش پاسخ‌ها
        const inputQuestionsData = reportData?.data
            ?.filter(p => p.type === 626)
            .map(p => ({
                id: p.id,
                question: p.question,
                answers: p.answersReport?.flatMap(a =>
                    a.userAnswers?.map(u => ({
                        userMeetID: u.userMeetID,
                        content: u.content || "پاسخ خالی",
                        dateInsert: u.dateInsert
                    })) || []
                ) || []
            })) || [];

        // داده برای کارت "میانگین کل نمرات"
        const scoresData = reportData?.data
            ?.filter(p => p.overallAverageScore !== null)
            .map(p => ({
                id: p.id,
                question: p.question,
                score: p.overallAverageScore
            })) || [];

        // داده برای کارت "نمره جلسه" - محاسبه نمره هر سوال به ازای 20
        const sessionScoresData = reportData?.data
            ?.filter(p => p.overallAverageScore !== null)
            .map(p => ({
                id: p.id,
                question: p.question,
                scoreFrom20: (p.overallAverageScore / 5).toFixed(1),
                originalScore: p.overallAverageScore
            })) || [];

        // داده برای کارت "شرکت‌کنندگان" - اطلاعات هر کاربر و میانگین امتیاز
        const userScoresMap = new Map();
        reportData?.data?.forEach(poll => {
            if (poll.type === 625 || poll.type === 627) {
                poll.answersReport?.forEach(answer => {
                    answer.userAnswers?.forEach(user => {
                        if (!userScoresMap.has(user.userMeetID)) {
                            userScoresMap.set(user.userMeetID, {
                                userMeetID: user.userMeetID,
                                nickName: user.nickName,
                                scores: [],
                                answers: []
                            });
                        }
                        const userData = userScoresMap.get(user.userMeetID);
                        if (answer.score > 0) {
                            userData.scores.push(answer.score);
                        }
                        userData.answers.push({
                            pollId: poll.id,
                            question: poll.question,
                            answer: answer.answer,
                            score: answer.score
                        });
                    });
                });
            }
        });

        const participantsData = Array.from(userScoresMap.values()).map(user => ({
            userMeetID: user.userMeetID,
            averageScore: user.scores.length > 0
                ? (user.scores.reduce((a, b) => a + b, 0) / user.scores.length).toFixed(1)
                : 0,
            totalAnswers: user.answers.length,
            nickName: user.nickName,
            answers: user.answers.slice(0, 3) // فقط 3 پاسخ اول برای نمایش
        }));

        return {
            allQuestions: allQuestionsData,
            optionalQuestions: optionalQuestionsData,
            inputQuestions: inputQuestionsData,
            scores: scoresData,
            sessionScores: sessionScoresData,
            participants: participantsData
        };
    }, [reportData]);

    const exportToCSV = () => {
        if (!reportData?.data) return;

        const csvRows = [];
        csvRows.push(["شناسه سوال", "سوال", "نوع", "گزینه", "امتیاز", "تعداد رای", "درصد", "میانگین امتیاز", "پاسخ‌های کاربران"]);

        reportData.data.forEach(poll => {
            poll.answersReport.forEach(answer => {
                const userAnswersText = answer.userAnswers.map(u => u.content || "بدون متن").join(" | ");
                csvRows.push([
                    poll.id,
                    poll.question,
                    getTypeName(poll.type),
                    answer.answer,
                    answer.score,
                    answer.count,
                    answer.percentage !== null ? `${answer.percentage}%` : "N/A",
                    answer.averageScore !== null ? answer.averageScore : "N/A",
                    userAnswersText
                ]);
            });
        });

        const csvContent = csvRows.map(row => row.map(cell => `"${cell}"`).join(",")).join("\n");
        const blob = new Blob(["\uFEFF" + csvContent], { type: "text/csv;charset=utf-8;" });
        const link = document.createElement("a");
        const url = URL.createObjectURL(blob);
        link.href = url;
        link.setAttribute("download", `poll_report_${meetid}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const exportToPDF = async () => {
        const pollsToExport = filteredPolls && filteredPolls.length > 0 ? filteredPolls : (reportData?.data || []);
        if (!pollsToExport || pollsToExport.length === 0) {
            alert("سوالی برای دریافت گزارش وجود ندارد.");
            return;
        }

        try {
            setIsGeneratingPDF(true);
            await new Promise(resolve => setTimeout(resolve, 150));

            const element = pdfPrintRef.current;
            if (!element) {
                throw new Error("عنصر محتوای PDF یافت نشد.");
            }

            const opt = {
                margin: [10, 10, 10, 10],
                filename: `poll_report_${meetid}.pdf`,
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: {
                    scale: 2,
                    useCORS: true,
                    letterRendering: true,
                    logging: false,
                    scrollX: 0,
                    scrollY: 0
                },
                jsPDF: {
                    unit: 'mm',
                    format: 'a4',
                    orientation: 'portrait'
                },
                pagebreak: {
                    mode: ['avoid-all', 'css', 'legacy']
                }
            };

            const html2pdfLib = typeof window !== 'undefined' && window.html2pdf ? window.html2pdf : html2pdf;
            await html2pdfLib().set(opt).from(element).save();
        } catch (error) {
            console.error("Error generating PDF:", error);
            alert("مشکلی در تولید فایل PDF رخ داد. لطفاً مجدداً تلاش کنید.");
        } finally {
            setIsGeneratingPDF(false);
        }
    };

    // محاسبه آمار کلی
    const totalQuestions = reportData?.data?.length || 0;
    const optionalQuestions = reportData?.data?.filter(p => p.type === 625 || p.type === 627).length || 0;
    const inputQuestions = reportData?.data?.filter(p => p.type === 626).length || 0;

    const avgOverallScore = reportData?.data?.filter(p => p.overallAverageScore !== null).length > 0
        ? (reportData.data.filter(p => p.overallAverageScore !== null).reduce((acc, p) => acc + (p.overallAverageScore || 0), 0) /
            reportData.data.filter(p => p.overallAverageScore !== null).length).toFixed(1)
        : 0;
    const allUserMeetIDs = new Set();
    reportData?.data?.forEach(poll => {
        poll.answersReport?.forEach(answer => {
            answer.userAnswers?.forEach(user => {
                if (user.userMeetID) {
                    allUserMeetIDs.add(user.userMeetID);
                }
            });
        });
    });
    const countAllParticipants = allUserMeetIDs.size;


    // تبدیل نمره به مبنای 20
    const scoreFrom20 = (avgOverallScore / 5).toFixed(1);

    // داده برای چارت دایره‌ای کلی
    const getOverallPieData = () => {
        const allAnswers = [];
        reportData?.data?.forEach(poll => {
            if (poll.type === 625 || poll.type === 627) {
                poll.answersReport.forEach(answer => {
                    allAnswers.push({
                        name: answer.answer,
                        value: answer.count,
                        percentage: answer.percentage,
                        score: answer.score
                    });
                });
            }
        });

        // گروه‌بندی بر اساس پاسخ
        const grouped = {};
        allAnswers.forEach(item => {
            if (!grouped[item.name]) {
                grouped[item.name] = { ...item, value: 0 };
            }
            grouped[item.name].value += item.value;
        });

        return Object.values(grouped).slice(0, 8);
    };

    // داده برای چارت رادار (مقایسه میانگین نمرات سوالات)
    const getRadarData = () => {
        return reportData?.data
            ?.filter(p => p.overallAverageScore !== null)
            .slice(0, 6)
            .map(p => ({
                subject: p.question.length > 20 ? p.question.substring(0, 20) + "..." : p.question,
                score: p.overallAverageScore,
                fullQuestion: p.question
            })) || [];
    };

    // داده برای چارت میله‌ای (تعداد پاسخ‌ها)
    const getBarData = () => {
        const answerCounts = {};
        reportData?.data?.forEach(poll => {
            if (poll.type === 625 || poll.type === 627) {
                poll.answersReport.forEach(answer => {
                    if (!answerCounts[answer.answer]) {
                        answerCounts[answer.answer] = 0;
                    }
                    answerCounts[answer.answer] += answer.count;
                });
            }
        });
        return Object.entries(answerCounts).map(([name, count]) => ({ name, count }));
    };

    // داده برای چارت خطی پیشرفت نمرات
    const getLineData = () => {
        return reportData?.data
            ?.filter(p => p.overallAverageScore !== null)
            .map((p, index) => ({
                name: `سوال ${index + 1}`,
                score: p.overallAverageScore,
                fullQuestion: p.question
            })) || [];
    };

    const openPollDetail = (poll) => {
        setSelectedPoll(poll);
        setShowDetailModal(true);
    };

    const CustomTooltip = ({ active, payload, label }) => {
        if (active && payload && payload.length) {
            return (
                <div className="bg-white p-3 rounded-lg shadow-lg border border-gray-200">
                    <p className="font-bold text-gray-800">{label}</p>
                    {payload.map((p, idx) => (
                        <p key={idx} className="text-sm" style={{ color: p.color }}>
                            {p.name}: {p.value}%
                        </p>
                    ))}
                </div>
            );
        }
        return null;
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-gray-900 to-gray-800 flex items-center justify-center">
                <div className="text-center">
                    <FaSpinner className="animate-spin text-6xl text-blue-500 mx-auto mb-4" />
                    <p className="text-gray-300 text-lg">در حال دریافت گزارش...</p>
                </div>
            </div>
        );
    }

    const pollsForPdf = filteredPolls && filteredPolls.length > 0 ? filteredPolls : (reportData?.data || []);

    return (
        <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900" dir="rtl">
            <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">

                {/* Header with Glass Effect */}
                <div className="mb-8">
                    <div className="flex justify-between items-center flex-wrap gap-4">
                        <div>
                            <button
                                onClick={() => navigate('/')}
                                className="mb-4 flex items-center gap-2 text-slate-400 hover:text-white transition-colors"
                            >
                                <FaArrowRight className="w-4 h-4" />
                                <span>بازگشت به مدیریت اتاق‌ها</span>
                            </button>
                            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-400 to-purple-400 bg-clip-text text-transparent mb-2">
                                گزارش پیشرفته نظرسنجی‌ها
                            </h1>
                            <div className="flex items-center gap-3 flex-wrap">
                                <div className="flex items-center gap-2 bg-slate-800/50 rounded-lg px-3 py-1.5">
                                    <span className="text-slate-400 text-sm">شناسه میتینگ:</span>
                                    <code className="text-blue-400 font-mono text-sm">{meetid}</code>
                                    <button
                                        onClick={copyMeetingLink}
                                        className="p-1 hover:bg-slate-700 rounded transition-colors"
                                    >
                                        {copied ? <FaCheck className="w-3 h-3 text-green-400" /> : <FaCopy className="w-3 h-3 text-slate-400" />}
                                    </button>
                                </div>
                                {filters.qid && (
                                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-blue-500/20 text-blue-400 text-xs rounded-full">
                                        فیلتر سوال: {filters.qid}
                                    </span>
                                )}
                                {filters.userid && (
                                    <span className="inline-flex items-center gap-1 px-2 py-1 bg-purple-500/20 text-purple-400 text-xs rounded-full">
                                        فیلتر کاربر: {filters.userid}
                                    </span>
                                )}
                            </div>
                        </div>
                        <div className="flex items-center gap-3 flex-wrap">
                            <button
                                onClick={exportToPDF}
                                disabled={isGeneratingPDF}
                                className="bg-gradient-to-r from-rose-600 to-red-600 hover:from-rose-700 hover:to-red-700 text-white px-6 py-3 rounded-xl transition-all flex items-center gap-2 shadow-lg disabled:opacity-60 cursor-pointer"
                                title="دانلود سوالات و نتایج نظرسنجی به صورت PDF"
                            >
                                {isGeneratingPDF ? (
                                    <>
                                        <FaSpinner className="animate-spin text-sm" />
                                        <span>در حال تولید PDF...</span>
                                    </>
                                ) : (
                                    <>
                                        <FaFilePdf className="text-sm" />
                                        <span>دانلود PDF</span>
                                    </>
                                )}
                            </button>
                            <button
                                onClick={exportToCSV}
                                className="bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-700 hover:to-emerald-700 text-white px-6 py-3 rounded-xl transition-all flex items-center gap-2 shadow-lg"
                            >
                                <FaDownload className="text-sm" />
                                <span>خروجی Excel</span>
                            </button>
                        </div>
                    </div>
                </div>

                {/* Stats Cards with Gradient */}
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 mb-8">
                    {/* کل سوالات */}
                    <div className="relative bg-gradient-to-br from-blue-500/20 to-blue-600/20 backdrop-blur-sm rounded-2xl p-4 border border-blue-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="لیست تمام سوالات"
                            data={tooltipData.allQuestions}
                            icon={<FaPoll className="text-blue-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="flex items-center justify-between">
                                        <span className="text-slate-300 font-medium truncate max-w-[180px]">{item.question}</span>
                                        <span className="text-xs text-blue-400 whitespace-nowrap mr-2">#{item.id}</span>
                                    </div>
                                    <span className="text-xs text-slate-500">{item.type}</span>
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaPoll className="text-blue-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{totalQuestions}</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">کل سوالات</p>
                    </div>

                    {/* چند گزینه‌ای */}
                    <div className="relative bg-gradient-to-br from-purple-500/20 to-purple-600/20 backdrop-blur-sm rounded-2xl p-4 border border-purple-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="سوالات چند گزینه‌ای"
                            data={tooltipData.optionalQuestions}
                            icon={<FaTasks className="text-purple-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="flex items-center justify-between">
                                        <span className="text-slate-300 font-medium truncate max-w-[180px]">{item.question}</span>
                                        <span className="text-xs text-purple-400 whitespace-nowrap mr-2">{item.overallScore || 0}%</span>
                                    </div>
                                    <div className="flex justify-between text-xs text-slate-500">
                                        <span>#{item.id}</span>
                                        <span>{item.type}</span>
                                        <span>{item.totalAnswers} پاسخ</span>
                                    </div>
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaTasks className="text-purple-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{optionalQuestions}</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">چند گزینه‌ای</p>
                    </div>

                    {/* متن آزاد */}
                    <div className="relative bg-gradient-to-br from-green-500/20 to-green-600/20 backdrop-blur-sm rounded-2xl p-4 border border-green-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="پاسخ‌های متن آزاد"
                            data={tooltipData.inputQuestions}
                            icon={<FaKeyboard className="text-green-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="text-slate-300 font-medium truncate">{item.question}</div>
                                    <div className="mt-1 space-y-1">
                                        {item.answers.slice(0, 3).map((answer, idx) => (
                                            <div key={idx} className="flex items-center justify-between text-xs bg-slate-800/50 p-1 rounded">
                                                <span className="text-slate-400 truncate max-w-[150px]">"{answer.content}"</span>
                                                <span className="text-slate-500 whitespace-nowrap">#{answer.userMeetID}</span>
                                            </div>
                                        ))}
                                        {item.answers.length > 3 && (
                                            <span className="text-xs text-slate-500">+{item.answers.length - 3} پاسخ دیگر</span>
                                        )}
                                    </div>
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaKeyboard className="text-green-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{inputQuestions}</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">متن آزاد</p>
                    </div>

                    {/* میانگین کل نمرات */}
                    <div className="relative bg-gradient-to-br from-amber-500/20 to-amber-600/20 backdrop-blur-sm rounded-2xl p-4 border border-amber-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="نمرات سوالات"
                            data={tooltipData.scores}
                            icon={<FaPercentage className="text-amber-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="flex items-center justify-between">
                                        <span className="text-slate-300 font-medium truncate max-w-[180px]">{item.question}</span>
                                        <span className="text-xs text-amber-400 font-bold whitespace-nowrap mr-2">{item.score}%</span>
                                    </div>
                                    <span className="text-xs text-slate-500">#{item.id}</span>
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaPercentage className="text-amber-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{avgOverallScore}%</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">میانگین کل نمرات</p>
                    </div>

                    {/* نمره جلسه */}
                    <div className="relative bg-gradient-to-br from-rose-500/20 to-rose-600/20 backdrop-blur-sm rounded-2xl p-4 border border-rose-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="نمرات تبدیل شده به ۲۰"
                            data={tooltipData.sessionScores}
                            icon={<FaTrophy className="text-rose-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="flex items-center justify-between">
                                        <span className="text-slate-300 font-medium truncate max-w-[150px]">{item.question}</span>
                                        <div className="text-left">
                                            <span className="text-xs text-rose-400 font-bold block">{item.scoreFrom20}/۲۰</span>
                                            <span className="text-xs text-slate-500">({item.originalScore}%)</span>
                                        </div>
                                    </div>
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaTrophy className="text-rose-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{scoreFrom20}</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">نمره جلسه (از ۲۰)</p>
                    </div>

                    {/* شرکت‌کنندگان */}
                    <div className="relative bg-gradient-to-br from-cyan-500/20 to-cyan-600/20 backdrop-blur-sm rounded-2xl p-4 border border-cyan-500/30
                    hover:scale-[96%] transition-all duration-300
                    ">
                        <StatCardTooltip
                            title="اطلاعات شرکت‌کنندگان"
                            data={tooltipData.participants}
                            icon={<FaUsers className="text-cyan-400" />}
                            renderItem={(item, index) => (
                                <div key={index} className="text-sm p-2 bg-slate-700/30 rounded-lg">
                                    <div className="flex items-center justify-between">
                                        <span className="text-cyan-300 font-mono text-xs cursor-pointer relative group">
                                            {item.nickName}
                                            <span className="absolute bottom-full top-1
                                             right-0 mb-1 hidden group-hover:block 
                                             bg-slate-800 border border-slate-700 text-white text-xs px-2 py-1 rounded whitespace-nowrap z-[99999] shadow-lg">
                                                🆔 {item.userMeetID}
                                            </span> 
                                        </span>
                                        <div className="text-left">
                                            <span className="text-xs text-amber-400 font-bold block">میانگین: {item.averageScore}%</span>
                                            <span className="text-xs text-slate-500 ">{item.totalAnswers} پاسخ</span>
                                        </div>
                                    </div>
                                    {item.answers.length > 0 && (
                                        <div className="mt-1 text-xs text-slate-400 truncate">
                                            {item.answers.map(a => a.answer).join(' • ')}
                                        </div>
                                    )}
                                </div>
                            )}
                        />
                        <div className="flex items-center justify-between mb-2 relative z-10">
                            <FaUsers className="text-cyan-400 text-xl" />
                            <span className="text-2xl font-bold text-white">{countAllParticipants || 0}</span>
                        </div>
                        <p className="text-slate-300 text-sm relative z-10">شرکت‌کنندگان</p>
                    </div>
                </div>
                {/* Charts Section */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                    {/* Pie Chart - Overall Distribution */}
                    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl p-6 border border-slate-700">
                        <h3 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
                            <FaChartPie className="text-purple-400" />
                            توزیع کلی پاسخ‌ها
                        </h3>
                        <ResponsiveContainer width="100%" height={350}>
                            <PieChart>
                                <Pie
                                    data={getOverallPieData()}
                                    cx="50%"
                                    cy="50%"
                                    labelLine={false}
                                    label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(1)}%`}
                                    outerRadius={120}
                                    fill="#8884d8"
                                    dataKey="value"
                                >
                                    {getOverallPieData().map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip content={({ active, payload }) => {
                                    if (active && payload && payload.length) {
                                        return (
                                            <div className="bg-slate-800 p-3 rounded-lg border border-slate-700">
                                                <p className="text-white font-bold">{payload[0].name}</p>
                                                <p className="text-slate-300">تعداد: {payload[0].value}</p>
                                                <p className="text-blue-400">درصد: {payload[0].payload.percentage}%</p>
                                            </div>
                                        );
                                    }
                                    return null;
                                }} />
                            </PieChart>
                        </ResponsiveContainer>
                    </div>

                    {/* Radar Chart */}
                    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl p-6 border border-slate-700">
                        <h3 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
                            <FaChartLine className="text-blue-400" />
                            مقایسه نمرات سوالات
                        </h3>
                        <ResponsiveContainer width="100%" height={350}>
                            <RadarChart data={getRadarData()}>
                                <PolarGrid stroke="#475569" />
                                <PolarAngleAxis dataKey="subject" tick={{ fill: '#94A3B8', fontSize: 10 }} />
                                <PolarRadiusAxis domain={[0, 100]} tick={{ fill: '#94A3B8' }} />
                                <Radar name="نمره" dataKey="score" stroke="#3B82F6" fill="#3B82F6" fillOpacity={0.3} />
                                <Tooltip content={({ active, payload }) => {
                                    if (active && payload && payload.length) {
                                        return (
                                            <div className="bg-slate-800 p-3 rounded-lg border border-slate-700">
                                                <p className="text-white text-sm">{payload[0].payload.fullQuestion}</p>
                                                <p className="text-blue-400">نمره: {payload[0].value}%</p>
                                            </div>
                                        );
                                    }
                                    return null;
                                }} />
                            </RadarChart>
                        </ResponsiveContainer>
                    </div>

                    {/* Line Chart - Score Progress */}
                    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl p-6 border border-slate-700">
                        <h3 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
                            <FaChartArea className="text-green-400" />
                            روند نمرات سوالات
                        </h3>
                        <ResponsiveContainer width="100%" height={300}>
                            <LineChart data={getLineData()}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
                                <XAxis dataKey="name" stroke="#94A3B8" />
                                <YAxis stroke="#94A3B8" domain={[0, 100]} />
                                <Tooltip content={({ active, payload, label }) => {
                                    if (active && payload && payload.length) {
                                        return (
                                            <div className="bg-slate-800 p-3 rounded-lg border border-slate-700">
                                                <p className="text-white font-bold">{label}</p>
                                                <p className="text-green-400">نمره: {payload[0].value}%</p>
                                                <p className="text-slate-400 text-xs">{payload[0].payload.fullQuestion}</p>
                                            </div>
                                        );
                                    }
                                    return null;
                                }} />
                                <Line type="monotone" dataKey="score" stroke="#10B981" strokeWidth={3} dot={{ fill: '#10B981', r: 6 }} />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>

                    {/* Bar Chart - Popular Answers */}
                    <div className="bg-slate-800/50 backdrop-blur-sm rounded-2xl p-6 border border-slate-700">
                        <h3 className="text-xl font-bold text-white mb-4 flex items-center gap-2">
                            <FaChartBar className="text-amber-400" />
                            محبوب‌ترین پاسخ‌ها
                        </h3>
                        <ResponsiveContainer width="100%" height={300}>
                            <BarChart data={getBarData()}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
                                <XAxis dataKey="name" stroke="#94A3B8" />
                                <YAxis stroke="#94A3B8" />
                                <Tooltip content={({ active, payload }) => {
                                    if (active && payload && payload.length) {
                                        return (
                                            <div className="bg-slate-800 p-3 rounded-lg border border-slate-700">
                                                <p className="text-white font-bold">{payload[0].payload.name}</p>
                                                <p className="text-amber-400">تعداد: {payload[0].value}</p>
                                            </div>
                                        );
                                    }
                                    return null;
                                }} />
                                <Bar dataKey="count" fill="#F59E0B" radius={[8, 8, 0, 0]}>
                                    {getBarData().map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                    ))}
                                </Bar>
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Search and Filters */}
                <div className="mb-6 space-y-4">
                    <div className="flex gap-3 flex-wrap">
                        <div className="flex-1 relative min-w-[200px]">
                            <FaSearch className="absolute right-3 top-1/2 transform -translate-y-1/2 text-slate-400" />
                            <input
                                type="text"
                                placeholder="جستجو در سوالات..."
                                value={searchTerm}
                                onChange={(e) => {
                                    setSearchTerm(e.target.value);
                                    setCurrentPage(1);
                                }}
                                className="w-full pr-10 pl-4 py-3 bg-slate-800/50 border border-slate-700 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent text-white placeholder-slate-400"
                            />
                        </div>

                        <select
                            value={filterType}
                            onChange={(e) => {
                                setFilterType(e.target.value);
                                setCurrentPage(1);
                            }}
                            className="px-4 py-3 bg-slate-800/50 border border-slate-700 rounded-xl text-white focus:ring-2 focus:ring-blue-500"
                        >
                            <option value="all">همه انواع</option>
                            <option value="optional">چند گزینه‌ای</option>
                            <option value="input">متن آزاد</option>
                        </select>

                        <button
                            onClick={() => setShowFiltersPanel(!showFiltersPanel)}
                            className={`px-4 py-3 rounded-xl border transition-all flex items-center gap-2 ${showFiltersPanel || filters.qid || filters.userid
                                ? "bg-blue-600 text-white border-blue-600"
                                : "bg-slate-800/50 text-slate-300 border-slate-700 hover:border-blue-500"
                                }`}
                        >
                            <FaFilter className="text-sm" />
                            <span>فیلتر پیشرفته</span>
                        </button>

                        {(filters.qid || filters.userid) && (
                            <button
                                onClick={resetFilters}
                                className="px-4 py-3 rounded-xl border border-slate-700 bg-slate-800/50 text-slate-300 hover:bg-slate-700 transition-all flex items-center gap-2"
                            >
                                <FaTimesCircle className="text-sm" />
                                <span>حذف فیلترها</span>
                            </button>
                        )}
                    </div>

                    {showFiltersPanel && (
                        <div className="bg-slate-800/50 backdrop-blur-sm rounded-xl border border-slate-700 p-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-2">فیلتر بر اساس شناسه سوال (QID)</label>
                                    <input
                                        type="number"
                                        value={pollIdInput}
                                        onChange={(e) => setPollIdInput(e.target.value)}
                                        placeholder="مثال: 525"
                                        className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-lg focus:ring-2 focus:ring-blue-500 text-white"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-slate-300 mb-2">فیلتر بر اساس شناسه کاربر (UserID)</label>
                                    <input
                                        type="text"
                                        value={userIdInput}
                                        onChange={(e) => setUserIdInput(e.target.value)}
                                        placeholder="شناسه کاربر را وارد کنید"
                                        className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-lg focus:ring-2 focus:ring-blue-500 text-white"
                                    />
                                </div>
                            </div>
                            <div className="flex justify-end gap-3 mt-4">
                                <button
                                    onClick={() => setShowFiltersPanel(false)}
                                    className="px-4 py-2 border border-slate-700 rounded-lg text-slate-300 hover:bg-slate-700"
                                >
                                    انصراف
                                </button>
                                <button
                                    onClick={applyFilters}
                                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                                >
                                    اعمال فیلتر
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Polls List with Cards Design */}
                <div className="space-y-4">
                    {paginatedPolls.map((poll, pollIdx) => (
                        <div
                            key={poll.id}
                            className="group bg-slate-800/30 backdrop-blur-sm rounded-2xl border border-slate-700 overflow-hidden hover:border-slate-600 transition-all cursor-pointer"
                            onClick={() => openPollDetail(poll)}
                        >
                            <div className={`p-5 bg-gradient-to-r ${getTypeColor(poll.type)}/10`}>
                                <div className="flex items-start justify-between flex-wrap gap-3">
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2 mb-2 flex-wrap">
                                            <span className={`inline-flex items-center gap-1 px-2 py-1 text-xs rounded-full bg-${poll.type === 625 ? 'blue' : poll.type === 626 ? 'green' : 'purple'}-500/20 text-${poll.type === 625 ? 'blue' : poll.type === 626 ? 'green' : 'purple'}-400`}>
                                                {poll.type === 625 ? <FaStar className="w-3 h-3" /> : poll.type === 626 ? <FaKeyboard className="w-3 h-3" /> : <FaTasks className="w-3 h-3" />}
                                                <span>{getTypeName(poll.type)}</span>
                                            </span>
                                            {poll.overallAverageScore !== null && (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 text-xs rounded-full bg-amber-500/20 text-amber-400">
                                                    <FaAward className="w-3 h-3" />
                                                    <span>میانگین نمره: {poll.overallAverageScore}%</span>
                                                </span>
                                            )}
                                        </div>
                                        <h3 className="text-lg font-bold text-white">{poll.question}</h3>
                                        <p className="text-xs text-slate-500 mt-1">شناسه سوال: {poll.id}</p>
                                    </div>
                                    <button className="text-slate-400 group-hover:text-blue-400 transition-colors p-2">
                                        <FaEye className="w-5 h-5" />
                                    </button>
                                </div>
                            </div>

                            <div className="p-5">
                                {poll.type === 626 ? (
                                    <div className="space-y-3">
                                        <div className="flex justify-between items-center mb-2">
                                            <span className="text-sm font-medium text-slate-300">پاسخ‌های کاربران:</span>
                                            <span className="text-xs text-slate-500">{poll.answersReport[0]?.count || 0} پاسخ</span>
                                        </div>
                                        <div className="flex flex-wrap gap-2 max-h-20 overflow-y-auto">
                                            {poll.answersReport[0]?.userAnswers.slice(0, 5).map((answer, idx) => (
                                                <span key={idx} className="px-3 py-1.5 bg-slate-700/50 rounded-lg text-sm text-slate-300">
                                                    {answer.content || "پاسخ خالی"}
                                                </span>
                                            ))}
                                            {(poll.answersReport[0]?.userAnswers.length || 0) > 5 && (
                                                <span className="px-3 py-1.5 bg-slate-700/50 rounded-lg text-sm text-slate-500">
                                                    +{poll.answersReport[0].userAnswers.length - 5} پاسخ دیگر
                                                </span>
                                            )}
                                        </div>
                                    </div>
                                ) : (
                                    <div className="space-y-4">
                                        {poll.answersReport.map((answer, ansIdx) => (
                                            <div key={answer.id}
                                                className="relative"
                                                onMouseEnter={() => setHoveredData({ pollIdx, ansIdx })}
                                                onMouseLeave={() => setHoveredData(null)}
                                            >
                                                <div className="flex justify-between items-center mb-1">
                                                    <span className="text-sm text-slate-300">{answer.answer}</span>
                                                    <div className="flex gap-3 text-xs">
                                                        {answer.averageScore !== null && (
                                                            <span className="text-amber-400">نمره: {answer.averageScore}</span>
                                                        )}
                                                        <span className="text-slate-500">{answer.count} رای</span>
                                                        <span className="text-blue-400 font-medium">{answer.percentage}%</span>
                                                    </div>
                                                </div>
                                                <div className="w-full bg-slate-700 rounded-full h-2 overflow-hidden">
                                                    <div
                                                        className="rounded-full h-2 transition-all duration-500"
                                                        style={{
                                                            width: `${answer.percentage}%`,
                                                            backgroundColor: COLORS[ansIdx % COLORS.length]
                                                        }}
                                                    />
                                                </div>
                                                {hoveredData?.pollIdx === pollIdx && hoveredData?.ansIdx === ansIdx && (
                                                    <div className="absolute -top-10 left-0 bg-slate-700 px-2 py-1 rounded text-xs text-white whitespace-nowrap">
                                                        {answer.percentage}% ({answer.count} رای)
                                                    </div>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </div>
                    ))}
                </div>

                {filteredPolls.length === 0 && (
                    <div className="text-center py-16">
                        <div className="w-20 h-20 bg-slate-800 rounded-full flex items-center justify-center mx-auto mb-4">
                            <FaSearch className="w-10 h-10 text-slate-600" />
                        </div>
                        <p className="text-slate-400 text-lg">نتیجه‌ای یافت نشد</p>
                        <p className="text-slate-500 text-sm mt-2">لطفاً عبارت جستجو را تغییر دهید یا فیلترها را حذف کنید</p>
                    </div>
                )}

                {/* Pagination */}
                {totalPages > 1 && (
                    <div className="flex justify-center items-center gap-2 py-6 mt-4">
                        <button
                            onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                            disabled={currentPage === 1}
                            className="p-2 rounded-lg border border-slate-700 bg-slate-800/50 text-slate-400 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-700 transition-colors"
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
                                            : "border border-slate-700 bg-slate-800/50 text-slate-400 hover:bg-slate-700"
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
                            className="p-2 rounded-lg border border-slate-700 bg-slate-800/50 text-slate-400 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-slate-700 transition-colors"
                        >
                            <FaChevronLeft className="w-5 h-5" />
                        </button>
                    </div>
                )}
            </div>

            {/* Poll Detail Modal - Keep existing modal but style matching new design */}
            {showDetailModal && selectedPoll && (
                <div className="fixed inset-0 bg-black bg-opacity-80 flex items-center justify-center z-50 p-4 overflow-y-auto">
                    <div className="bg-slate-800 rounded-2xl shadow-2xl max-w-4xl w-full my-8 max-h-[90vh] overflow-y-auto border border-slate-700">
                        <div className="sticky top-0 bg-slate-800 z-10 flex justify-between items-center p-6 border-b border-slate-700">
                            <div className="flex items-center gap-3">
                                <FaPoll className="text-blue-400 text-xl" />
                                <h2 className="text-xl font-bold text-white">جزئیات سوال</h2>
                                <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 text-xs rounded-full bg-${selectedPoll.type === 625 ? 'blue' : selectedPoll.type === 626 ? 'green' : 'purple'}-500/20 text-${selectedPoll.type === 625 ? 'blue' : selectedPoll.type === 626 ? 'green' : 'purple'}-400`}>
                                    {selectedPoll.type === 625 ? <FaStar className="w-3 h-3" /> : selectedPoll.type === 626 ? <FaKeyboard className="w-3 h-3" /> : <FaTasks className="w-3 h-3" />}
                                    <span>{getTypeName(selectedPoll.type)}</span>
                                </span>
                            </div>
                            <button onClick={() => setShowDetailModal(false)} className="text-slate-400 hover:text-white">
                                <FaTimesCircle className="w-6 h-6" />
                            </button>
                        </div>

                        <div className="p-6">
                            <div className="mb-6">
                                <h3 className="text-lg font-bold text-white mb-2">{selectedPoll.question}</h3>
                                <p className="text-xs text-slate-500">شناسه سوال: {selectedPoll.id}</p>
                            </div>

                            {selectedPoll.type === 626 ? (
                                <div>
                                    <h4 className="font-bold text-slate-300 mb-4 flex items-center gap-2">
                                        <FaKeyboard className="text-green-400" />
                                        پاسخ‌های کاربران:
                                    </h4>
                                    <div className="space-y-2">
                                        {selectedPoll.answersReport[0]?.userAnswers.map((answer, idx) => (
                                            <div key={idx} className="bg-slate-700/50 rounded-xl p-4 border border-slate-600">
                                                <div className="flex justify-between items-start mb-2">
                                                    <span className="text-xs text-slate-400">شناسه کاربر: {answer.userMeetID}</span>
                                                    <span className="text-xs text-slate-500">{new Date(answer.dateInsert).toLocaleString('fa-IR')}</span>
                                                </div>
                                                <p className="text-slate-200">{answer.content || "پاسخ خالی"}</p>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            ) : (
                                <div className="space-y-6">
                                    {selectedPoll.answersReport.map((answer, idx) => (
                                        <div key={answer.id} className="border border-slate-700 rounded-xl p-4">
                                            <div className="flex justify-between items-center mb-3">
                                                <div>
                                                    <h4 className="font-bold text-white">{answer.answer}</h4>
                                                    <p className="text-xs text-slate-500">شناسه گزینه: {answer.id}</p>
                                                </div>
                                                <div className="text-left">
                                                    {answer.averageScore !== null && (
                                                        <p className="text-sm text-amber-400">امتیاز: {answer.averageScore}</p>
                                                    )}
                                                    <p className="text-sm text-slate-400">{answer.count} رای</p>
                                                </div>
                                            </div>

                                            <div className="mb-4">
                                                <div className="flex justify-between text-sm mb-1">
                                                    <span className="text-slate-400">درصد رای‌ها</span>
                                                    <span className="text-blue-400 font-bold">{answer.percentage}%</span>
                                                </div>
                                                <div className="w-full bg-slate-700 rounded-full h-3">
                                                    <div
                                                        className="rounded-full h-3 transition-all duration-500"
                                                        style={{ width: `${answer.percentage}%`, backgroundColor: COLORS[idx % COLORS.length] }}
                                                    />
                                                </div>
                                            </div>

                                            {answer.userAnswers.length > 0 && (
                                                <div className="mt-3">
                                                    <p className="text-sm text-slate-400 mb-2">کاربران انتخاب‌کننده:</p>
                                                    <div className="flex flex-wrap gap-2">
                                                        {answer.userAnswers.map((user, uidx) => (
                                                            <span key={uidx} className="px-2 py-1 bg-slate-700 rounded text-xs text-slate-300">
                                                                {user.userMeetID}
                                                            </span>
                                                        ))}
                                                    </div>
                                                </div>
                                            )}
                                        </div>
                                    ))}

                                    {selectedPoll.overallAverageScore !== null && (
                                        <div className="bg-gradient-to-r from-amber-500/20 to-amber-600/20 rounded-xl p-4 border border-amber-500/30">
                                            <div className="flex items-center justify-between">
                                                <div className="flex items-center gap-2">
                                                    <FaCalculator className="text-amber-400" />
                                                    <span className="font-bold text-amber-300">میانگین کل نمرات</span>
                                                </div>
                                                <span className="text-2xl font-bold text-amber-400">{selectedPoll.overallAverageScore}%</span>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>

                        <div className="sticky bottom-0 bg-slate-800 flex justify-end gap-3 p-6 border-t border-slate-700">
                            <button
                                onClick={() => setShowDetailModal(false)}
                                className="px-6 py-2 border border-slate-700 rounded-xl text-slate-300 hover:bg-slate-700"
                            >
                                بستن
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Dedicated Off-Screen Printable Container for PDF Export (Questions and Results ONLY) */}
            <div style={{ position: 'fixed', left: '-99999px', top: 0, pointerEvents: 'none', zIndex: -9999 }}>
                <div
                    ref={pdfPrintRef}
                    style={{
                        width: '750px',
                        backgroundColor: '#ffffff',
                        color: '#1e293b',
                        padding: '24px',
                        direction: 'rtl',
                        fontFamily: 'YekanBakhFaNum, Tahoma, sans-serif',
                        boxSizing: 'border-box',
                    }}
                >
                    {/* Minimal Title Header */}
                    <div
                        style={{
                            borderBottom: '2px solid #2563eb',
                            paddingBottom: '12px',
                            marginBottom: '20px',
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                            direction: 'rtl',
                        }}
                    >
                        <div>
                            <h1 style={{ fontSize: '18px', fontWeight: 'bold', color: '#0f172a', margin: '0 0 4px 0' }}>
                                نتایج نظرسنجی
                            </h1>
                            <p style={{ fontSize: '11px', color: '#64748b', margin: 0 }}>
                                شناسه جلسه: <span style={{ fontFamily: 'monospace' }}>{meetid}</span>
                            </p>
                        </div>
                        <div style={{ textAlign: 'left', fontSize: '11px', color: '#64748b' }}>
                            <div>تاریخ: {new Date().toLocaleDateString('fa-IR')}</div>
                            <div>تعداد سوالات: {pollsForPdf.length}</div>
                        </div>
                    </div>

                    {/* Questions and Results ONLY */}
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
                        {pollsForPdf.map((poll, pollIdx) => (
                            <div
                                key={poll.id || pollIdx}
                                style={{
                                    pageBreakInside: 'avoid',
                                    breakInside: 'avoid',
                                    border: '1px solid #cbd5e1',
                                    borderRadius: '8px',
                                    padding: '14px 16px',
                                    backgroundColor: '#ffffff',
                                }}
                            >
                                {/* Question Title & Badges */}
                                <div
                                    style={{
                                        display: 'flex',
                                        justifyContent: 'space-between',
                                        alignItems: 'flex-start',
                                        borderBottom: '1px solid #e2e8f0',
                                        paddingBottom: '8px',
                                        marginBottom: '12px',
                                    }}
                                >
                                    <div style={{ display: 'flex', alignItems: 'flex-start', gap: '8px', flex: 1 }}>
                                        <span
                                            style={{
                                                backgroundColor: '#2563eb',
                                                color: '#ffffff',
                                                fontSize: '11px',
                                                fontWeight: 'bold',
                                                padding: '2px 8px',
                                                borderRadius: '4px',
                                                whiteSpace: 'nowrap',
                                            }}
                                        >
                                            سوال {pollIdx + 1}
                                        </span>
                                        <span
                                            style={{
                                                fontSize: '13px',
                                                fontWeight: 'bold',
                                                color: '#0f172a',
                                                lineHeight: '1.5',
                                            }}
                                        >
                                            {poll.question}
                                        </span>
                                    </div>
                                    <div style={{ display: 'flex', gap: '6px', alignItems: 'center', whiteSpace: 'nowrap', marginRight: '10px' }}>
                                        <span
                                            style={{
                                                fontSize: '10px',
                                                padding: '2px 6px',
                                                borderRadius: '4px',
                                                backgroundColor: '#f1f5f9',
                                                color: '#475569',
                                                border: '1px solid #e2e8f0',
                                            }}
                                        >
                                            {getTypeName(poll.type)}
                                        </span>
                                        {poll.overallAverageScore !== null && poll.overallAverageScore !== undefined && (
                                            <span
                                                style={{
                                                    fontSize: '10px',
                                                    padding: '2px 6px',
                                                    borderRadius: '4px',
                                                    backgroundColor: '#fef3c7',
                                                    color: '#92400e',
                                                    border: '1px solid #fde68a',
                                                    fontWeight: 'bold',
                                                }}
                                            >
                                                میانگین: {poll.overallAverageScore}%
                                            </span>
                                        )}
                                    </div>
                                </div>

                                {/* Results Section */}
                                {poll.type === 626 ? (
                                    /* Free-Text Results */
                                    <div>
                                        <div style={{ fontSize: '11px', fontWeight: '600', color: '#475569', marginBottom: '8px' }}>
                                            پاسخ‌های کاربران ({poll.answersReport?.[0]?.count || poll.answersReport?.[0]?.userAnswers?.length || 0} پاسخ):
                                        </div>
                                        {poll.answersReport?.[0]?.userAnswers?.length > 0 ? (
                                            <div style={{ display: 'flex', flexDirection: 'column', gap: '6px' }}>
                                                {poll.answersReport[0].userAnswers.map((uAns, uIdx) => (
                                                    <div
                                                        key={uIdx}
                                                        style={{
                                                            padding: '6px 10px',
                                                            backgroundColor: '#f8fafc',
                                                            border: '1px solid #e2e8f0',
                                                            borderRadius: '6px',
                                                            fontSize: '11px',
                                                            color: '#1e293b',
                                                            lineHeight: '1.5',
                                                        }}
                                                    >
                                                        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '2px', color: '#94a3b8', fontSize: '9px' }}>
                                                            <span>پاسخ #{uIdx + 1}</span>
                                                            {uAns.dateInsert && (
                                                                <span>{new Date(uAns.dateInsert).toLocaleDateString('fa-IR')}</span>
                                                            )}
                                                        </div>
                                                        <div>{uAns.content || "پاسخ خالی"}</div>
                                                    </div>
                                                ))}
                                            </div>
                                        ) : (
                                            <div style={{ fontSize: '11px', color: '#94a3b8', fontStyle: 'italic', padding: '8px', backgroundColor: '#f8fafc', borderRadius: '6px' }}>
                                                هیچ پاسخی برای این سوال ثبت نشده است.
                                            </div>
                                        )}
                                    </div>
                                ) : (
                                    /* Multiple-Choice Options Results */
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                                        {poll.answersReport?.map((answer, ansIdx) => {
                                            const percent = typeof answer.percentage === 'number' ? answer.percentage : parseFloat(answer.percentage) || 0;
                                            const barColor = COLORS[ansIdx % COLORS.length] || '#2563eb';
                                            return (
                                                <div key={answer.id || ansIdx} style={{ display: 'flex', flexDirection: 'column', gap: '3px' }}>
                                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '11px' }}>
                                                        <span style={{ fontWeight: '600', color: '#1e293b' }}>
                                                            {answer.answer}
                                                        </span>
                                                        <div style={{ display: 'flex', gap: '10px', alignItems: 'center', color: '#64748b', fontSize: '10px' }}>
                                                            {answer.averageScore !== null && answer.averageScore !== undefined && (
                                                                <span style={{ color: '#d97706', fontWeight: '500' }}>
                                                                    امتیاز: {answer.averageScore}
                                                                </span>
                                                            )}
                                                            <span>{answer.count} رای</span>
                                                            <span style={{ fontWeight: 'bold', color: '#0f172a', minWidth: '32px', textAlign: 'left' }}>
                                                                {percent}%
                                                            </span>
                                                        </div>
                                                    </div>
                                                    {/* Progress Bar */}
                                                    <div
                                                        style={{
                                                            width: '100%',
                                                            backgroundColor: '#f1f5f9',
                                                            height: '7px',
                                                            borderRadius: '4px',
                                                            overflow: 'hidden',
                                                            border: '1px solid #e2e8f0',
                                                        }}
                                                    >
                                                        <div
                                                            style={{
                                                                width: `${Math.min(100, Math.max(0, percent))}%`,
                                                                height: '100%',
                                                                backgroundColor: barColor,
                                                                borderRadius: '4px',
                                                            }}
                                                        />
                                                    </div>
                                                </div>
                                            );
                                        })}
                                        <div
                                            style={{
                                                display: 'flex',
                                                justifyContent: 'flex-end',
                                                fontSize: '10px',
                                                color: '#64748b',
                                                paddingTop: '4px',
                                                borderTop: '1px dashed #e2e8f0',
                                                marginTop: '2px',
                                            }}
                                        >
                                            <span>مجموع کل آرا: {poll.answersReport?.reduce((acc, a) => acc + (a.count || 0), 0) || 0} رای</span>
                                        </div>
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};
// ک

// کامپوننت Tooltip ساده شده با Portal
const StatCardTooltip = ({ title, data, renderItem, maxItems = 5, icon }) => {
    const [isVisible, setIsVisible] = useState(false);
    const [tooltipStyle, setTooltipStyle] = useState({});
    const cardRef = useRef(null);
    const timeoutRef = useRef(null);

    const handleMouseEnter = (e) => {
        clearTimeout(timeoutRef.current);
        setIsVisible(true);

        // محاسبه موقعیت tooltip نسبت به کارت
        const rect = cardRef.current?.getBoundingClientRect();
        if (rect) {
            // نمایش tooltip در سمت راست کارت
            let left = rect.right + 10;
            let top = rect.top - 20;

            // اگر از سمت راست صفحه خارج شد، به چپ منتقل کن
            if (left + 380 > window.innerWidth) {
                left = rect.left - 390;
            }

            // اگر از پایین صفحه خارج شد، به بالا منتقل کن
            if (top + 400 > window.innerHeight) {
                top = window.innerHeight - 410;
            }

            // اگر از بالا خارج شد
            if (top < 10) {
                top = 10;
            }

            setTooltipStyle({
                position: 'fixed',
                left: `${left}px`,
                top: `${top}px`,
                minWidth: '320px',
                maxWidth: '400px',
                maxHeight: '400px',
                zIndex: 999999,
                backgroundColor: '#1e293b',
                border: '1px solid #475569',
                borderRadius: '12px',
                padding: '16px',
                boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.9)',
                backdropFilter: 'blur(12px)',
                overflowY: 'auto',
                pointerEvents: 'auto',
            });
        }
    };

    const handleMouseLeave = () => {
        timeoutRef.current = setTimeout(() => {
            setIsVisible(false);
        }, 150);
    };

    if (!data || data.length === 0) {
        return <div ref={cardRef} className="absolute inset-0 z-10 cursor-default" />;
    }

    const displayData = data.slice(0, maxItems);
    const remaining = data.length - maxItems;

    return (
        <>
            {/* المنت شفاف برای دریافت رویدادهای موس */}
            <div
                ref={cardRef}
                className="absolute inset-0 z-1000 cursor-pointer "
                onMouseEnter={handleMouseEnter}
                onMouseLeave={handleMouseLeave}
            />

            {/* رندر tooltip با Portal به body */}
            {isVisible && createPortal(
                <div
                    style={tooltipStyle}
                    onMouseEnter={() => {
                        clearTimeout(timeoutRef.current);
                        setIsVisible(true);
                    }}
                    onMouseLeave={handleMouseLeave}
                >
                    {/* هدر tooltip */}
                    <div className="flex items-center gap-2 mb-3 pb-2 border-b border-slate-700 sticky top-0 bg-slate-800" style={{ backgroundColor: '#1e293b' }}>
                        <span className="text-lg">{icon}</span>
                        <h4 className="font-bold text-white text-sm">{title}</h4>
                        <span className="text-xs text-slate-500 mr-auto">({data.length})</span>
                    </div>

                    {/* محتوای tooltip */}
                    <div className="space-y-2 max-h-64 overflow-y-auto" style={{ maxHeight: '280px' }}>
                        {displayData.map((item, index) => (
                            <div key={index}>
                                {renderItem(item, index)}
                            </div>
                        ))}
                        {remaining > 0 && (
                            <div className="text-center text-xs text-slate-500 mt-2 pt-2 border-t border-slate-700">
                                و {remaining} مورد دیگر...
                            </div>
                        )}
                    </div>
                </div>,
                document.body
            )}
        </>
    );
};



export default PollReportPage;  