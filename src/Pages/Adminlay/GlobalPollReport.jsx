import React, { useState, useEffect, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
  FaPoll,
  FaChartBar,
  FaChartPie,
  FaUsers,
  FaVideo,
  FaBuilding,
  FaSearch,
  FaFilter,
  FaDownload,
  FaSyncAlt,
  FaStar,
  FaTimes,
  FaEye,
  FaCalendarAlt,
  FaCheckCircle,
  FaCommentDots,
  FaExternalLinkAlt,
  FaUndo
} from "react-icons/fa";
import {
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer
} from "recharts";
import { GetGlobalPollReport } from "../../api/AdminAPI";

const CHART_COLORS = [
  "#0284c7",
  "#10b981",
  "#f59e0b",
  "#8b5cf6",
  "#ef4444",
  "#ec4899",
  "#06b6d4",
  "#84cc16"
];

const GlobalPollReportSkeleton = () => (
  <div className="space-y-6 animate-pulse" dir="rtl">
    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-5 sm:p-6 flex flex-wrap justify-between gap-4">
      <div className="space-y-2">
        <div className="h-7 w-56 bg-[#e5e7eb] rounded-lg" />
        <div className="h-4 w-72 bg-[#f3f4f6] rounded" />
      </div>
      <div className="flex gap-2">
        <div className="h-10 w-28 bg-[#e5e7eb] rounded-xl" />
        <div className="h-10 w-32 bg-[#e5e7eb] rounded-xl" />
      </div>
    </div>

    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
      {[1, 2, 3, 4, 5, 6].map((i) => (
        <div key={i} className="bg-white rounded-2xl border border-[#e5e7eb] p-4 space-y-2">
          <div className="h-4 w-20 bg-[#f3f4f6] rounded" />
          <div className="h-7 w-16 bg-[#e5e7eb] rounded-lg" />
        </div>
      ))}
    </div>

    <div className="bg-white rounded-2xl border border-[#e5e7eb] p-5 space-y-4">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-10 bg-[#f3f4f6] rounded-xl" />
        ))}
      </div>
    </div>

    <div className="space-y-4">
      {[1, 2].map((q) => (
        <div key={q} className="bg-white rounded-2xl border border-[#e5e7eb] p-5 space-y-4">
          <div className="h-6 w-2/3 bg-[#e5e7eb] rounded-lg" />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <div className="space-y-3">
              {[1, 2, 3].map((o) => (
                <div key={o} className="h-12 bg-[#f3f4f6] rounded-xl" />
              ))}
            </div>
            <div className="h-52 bg-[#f9fafb] rounded-xl" />
          </div>
        </div>
      ))}
    </div>
  </div>
);

const getQuestionTypeMeta = (type) => {
  switch (type) {
    case 625:
      return {
        label: "چند گزینه‌ای (نمره‌دار)",
        badgeClass: "bg-[#dbeafe] text-[#1d4ed8] border-[#bfdbfe]"
      };
    case 626:
      return {
        label: "متن آزاد",
        badgeClass: "bg-[#dcfce7] text-[#15803d] border-[#bbf7d0]"
      };
    case 627:
      return {
        label: "چند گزینه‌ای (بدون نمره)",
        badgeClass: "bg-[#f3e8ff] text-[#7e22ce] border-[#e9d5ff]"
      };
    default:
      return {
        label: "نامشخص",
        badgeClass: "bg-[#f3f4f6] text-[#4b5563] border-[#e5e7eb]"
      };
  }
};

const formatDateTimeFa = (dateVal) => {
  if (!dateVal) return "-";
  try {
    return new Date(dateVal).toLocaleString("fa-IR", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit"
    });
  } catch {
    return String(dateVal);
  }
};

const GlobalPollReport = () => {
  const navigate = useNavigate();

  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Server-side filters
  const [filters, setFilters] = useState({
    qid: "",
    roomFriendlyId: "",
    meetId: "",
    fromDate: "",
    toDate: "",
    search: ""
  });

  // Client-side quick view state
  const [activeTab, setActiveTab] = useState("questions"); // questions | meetings | rooms
  const [typeFilter, setTypeFilter] = useState("all"); // all | 625 | 627 | 626
  const [detailModal, setDetailModal] = useState(null); // { question, votes }
  const [detailSearch, setDetailSearch] = useState("");

  const fetchReport = async (overrideFilters = null) => {
    const active = overrideFilters || filters;
    try {
      setLoading(true);
      setError("");
      const data = await GetGlobalPollReport({
        qid: active.qid || null,
        roomFriendlyId: active.roomFriendlyId || null,
        meetId: active.meetId || null,
        fromDate: active.fromDate || null,
        toDate: active.toDate || null,
        search: active.search || null
      });

      if (data?.status === 403 || data?.status === 400) {
        setError(data?.message || "شما دسترسی مشاهده این گزارش را ندارید.");
        setReport(null);
      } else {
        setReport(data);
      }
    } catch (err) {
      console.error("Error fetching global poll report:", err);
      setError("خطا در دریافت گزارش کلی نظرسنجی‌ها. لطفاً مجدداً تلاش کنید.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    document.title = "گزارش کلی نظرسنجی‌ها | داشبورد مدیریت";
    fetchReport();
  }, []);

  const handleApplyFilters = (e) => {
    if (e) e.preventDefault();
    fetchReport(filters);
  };

  const handleResetFilters = () => {
    const empty = {
      qid: "",
      roomFriendlyId: "",
      meetId: "",
      fromDate: "",
      toDate: "",
      search: ""
    };
    setFilters(empty);
    setTypeFilter("all");
    fetchReport(empty);
  };

  const handleQuickFilterByMeeting = (meetId) => {
    const updated = { ...filters, meetId };
    setFilters(updated);
    setActiveTab("questions");
    fetchReport(updated);
  };

  const handleQuickFilterByRoom = (roomFriendlyId) => {
    const updated = { ...filters, roomFriendlyId, meetId: "" };
    setFilters(updated);
    setActiveTab("questions");
    fetchReport(updated);
  };

  const summary = report?.summary || {};
  const allQuestions = report?.questions || [];
  const meetingsBreakdown = report?.meetingsBreakdown || [];
  const roomsBreakdown = report?.roomsBreakdown || [];
  const filterOptions = report?.filterOptions || { questions: [], rooms: [] };

  const visibleQuestions = useMemo(() => {
    if (typeFilter === "all") return allQuestions;
    const targetType = Number(typeFilter);
    return allQuestions.filter((q) => q.type === targetType);
  }, [allQuestions, typeFilter]);

  const openQuestionDetails = (question) => {
    const allVotes = (question.answersReport || []).flatMap((ans) =>
      (ans.userAnswers || []).map((ua) => ({
        ...ua,
        optionAnswer: ans.answer,
        optionScore: ans.score
      }))
    );
    setDetailSearch("");
    setDetailModal({ question, votes: allVotes });
  };

  const filteredModalVotes = useMemo(() => {
    if (!detailModal) return [];
    const term = detailSearch.trim().toLowerCase();
    if (!term) return detailModal.votes;
    return detailModal.votes.filter(
      (v) =>
        (v.nickName || "").toLowerCase().includes(term) ||
        (v.phone || "").toLowerCase().includes(term) ||
        (v.meetingTitle || "").toLowerCase().includes(term) ||
        (v.roomTitle || "").toLowerCase().includes(term) ||
        (v.optionAnswer || "").toLowerCase().includes(term) ||
        (v.content || "").toLowerCase().includes(term)
    );
  }, [detailModal, detailSearch]);

  const exportToCSV = () => {
    if (!visibleQuestions.length) return;
    const rows = [
      [
        "شناسه سوال",
        "متن سوال",
        "نوع سوال",
        "گزینه",
        "امتیاز گزینه",
        "تعداد رای",
        "درصد",
        "نام پاسخ‌دهنده",
        "شماره تماس",
        "نام اتاق",
        "عنوان جلسه",
        "تاریخ جلسه",
        "پاسخ متنی",
        "زمان ثبت رای"
      ]
    ];

    visibleQuestions.forEach((q) => {
      const typeLabel = getQuestionTypeMeta(q.type).label;
      if (!q.answersReport || q.answersReport.length === 0) {
        rows.push([q.id, q.question, typeLabel, "-", 0, 0, "0%", "-", "-", "-", "-", "-", "-", "-"]);
        return;
      }

      q.answersReport.forEach((ans) => {
        if (!ans.userAnswers || ans.userAnswers.length === 0) {
          rows.push([
            q.id,
            q.question,
            typeLabel,
            ans.answer,
            ans.score,
            ans.count,
            `${ans.percentage ?? 0}%`,
            "-",
            "-",
            "-",
            "-",
            "-",
            "-",
            "-"
          ]);
        } else {
          ans.userAnswers.forEach((ua) => {
            rows.push([
              q.id,
              q.question,
              typeLabel,
              ans.answer,
              ans.score,
              ans.count,
              `${ans.percentage ?? 0}%`,
              ua.nickName || "",
              ua.phone || "",
              ua.roomTitle || ua.roomFriendlyID || "",
              ua.meetingTitle || ua.meetID || "",
              ua.meetingJalaliDate || "",
              ua.content || "",
              formatDateTimeFa(ua.dateInsert)
            ]);
          });
        }
      });
    });

    const csv = rows
      .map((r) => r.map((cell) => `"${String(cell ?? "").replace(/"/g, '""')}"`).join(","))
      .join("\n");
    const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", `global_poll_report_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  if (loading && !report) {
    return <GlobalPollReportSkeleton />;
  }

  if (error && !report) {
    return (
      <div className="bg-white rounded-2xl border border-[#fecaca] p-8 text-center shadow-sm" dir="rtl">
        <p className="text-[#dc2626] font-bold text-lg mb-4">{error}</p>
        <button
          onClick={() => fetchReport()}
          className="px-5 py-2.5 bg-[#0284c7] hover:bg-[#0369a1] text-white rounded-xl font-bold text-sm transition cursor-pointer"
        >
          تلاش مجدد
        </button>
      </div>
    );
  }

  const scoreOutOf20 = summary.overallAverageScore
    ? (summary.overallAverageScore / 5).toFixed(1)
    : "0.0";

  return (
    <div className="space-y-6" dir="rtl">
      {/* هدر گزارش کلی */}
      <div className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm p-5 sm:p-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-start gap-3">
          <div className="p-3 bg-gradient-to-br from-[#10b981] to-[#059669] text-white rounded-2xl shadow-sm shrink-0">
            <FaPoll size={24} />
          </div>
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h2 className="text-xl sm:text-2xl font-black text-[#1f2937]">
                گزارش کلی نظرسنجی‌ها
              </h2>
              <span className="text-[11px] font-bold px-2.5 py-0.5 rounded-full bg-[#ecfdf5] text-[#047857] border border-[#a7f3d0]">
                سوالات عمومی (بدون ID میت)
              </span>
            </div>
            <p className="text-xs sm:text-sm text-[#6b7280] mt-1">
              تجمیع کامل پاسخ‌ها و امتیازات تمامی سوالات عمومی در سراسر جلسات و اتاق‌ها
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => fetchReport()}
            disabled={loading}
            className="inline-flex items-center gap-2 px-4 py-2.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl font-bold text-xs sm:text-sm transition cursor-pointer disabled:opacity-60"
          >
            <FaSyncAlt className={loading ? "animate-spin" : ""} />
            <span>بروزرسانی</span>
          </button>

          <button
            type="button"
            onClick={exportToCSV}
            disabled={!visibleQuestions.length}
            className="inline-flex items-center gap-2 px-4 py-2.5 bg-[#059669] hover:bg-[#047857] text-white rounded-xl font-bold text-xs sm:text-sm transition shadow-sm cursor-pointer disabled:opacity-50"
          >
            <FaDownload />
            <span>خروجی اکسل (CSV)</span>
          </button>
        </div>
      </div>

      {/* کارت‌های شاخص کلیدی (KPI) */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3 sm:gap-4">
        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>سوالات عمومی</span>
            <FaPoll className="text-[#3b82f6]" />
          </div>
          <div className="text-2xl font-black text-[#1f2937] tabular-nums">
            {summary.totalQuestions ?? 0}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>مجموع آراء</span>
            <FaCheckCircle className="text-[#10b981]" />
          </div>
          <div className="text-2xl font-black text-[#1f2937] tabular-nums">
            {summary.totalVotes ?? 0}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>شرکت‌کنندگان یکتا</span>
            <FaUsers className="text-[#8b5cf6]" />
          </div>
          <div className="text-2xl font-black text-[#1f2937] tabular-nums">
            {summary.uniqueRespondents ?? 0}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>میانگین امتیاز کل</span>
            <FaStar className="text-[#f59e0b]" />
          </div>
          <div className="flex items-baseline gap-1.5">
            <span className="text-2xl font-black text-[#d97706] tabular-nums">
              {summary.overallAverageScore ?? 0}٪
            </span>
            <span className="text-[11px] text-[#6b7280] font-semibold">
              ({scoreOutOf20} از ۲۰)
            </span>
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>جلسات مشارکت‌کننده</span>
            <FaVideo className="text-[#0284c7]" />
          </div>
          <div className="text-2xl font-black text-[#1f2937] tabular-nums">
            {summary.uniqueMeetingsCount ?? 0}
          </div>
        </div>

        <div className="bg-white rounded-2xl border border-[#e5e7eb] p-4 shadow-2xs">
          <div className="flex items-center justify-between text-[#6b7280] text-xs font-medium mb-1">
            <span>اتاق‌های فعال</span>
            <FaBuilding className="text-[#ec4899]" />
          </div>
          <div className="text-2xl font-black text-[#1f2937] tabular-nums">
            {summary.uniqueRoomsCount ?? 0}
          </div>
        </div>
      </div>

      {/* بخش فیلترهای پیشرفته */}
      <form
        onSubmit={handleApplyFilters}
        className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm p-4 sm:p-5 space-y-4"
      >
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex items-center gap-2 text-sm font-bold text-[#1f2937]">
            <FaFilter className="text-[#0284c7]" />
            <span>فیلترهای گزارش</span>
            {filters.meetId && (
              <span className="inline-flex items-center gap-1 text-xs bg-[#e0f2fe] text-[#0369a1] px-2.5 py-0.5 rounded-full">
                جلسه: {filters.meetId}
                <button
                  type="button"
                  onClick={() => handleQuickFilterByMeeting("")}
                  className="hover:text-[#dc2626] cursor-pointer"
                  title="حذف فیلتر جلسه"
                >
                  <FaTimes size={10} />
                </button>
              </span>
            )}
          </div>

          <div className="flex flex-wrap gap-1.5">
            {[
              { id: "all", label: `همه (${allQuestions.length})` },
              { id: "625", label: `نمره‌دار (${summary.optionalScoredCount ?? 0})` },
              { id: "627", label: `بدون نمره (${summary.optionalNoScoreCount ?? 0})` },
              { id: "626", label: `متن آزاد (${summary.TextInputCount ?? summary.textInputCount ?? 0})` }
            ].map((t) => (
              <button
                key={t.id}
                type="button"
                onClick={() => setTypeFilter(t.id)}
                className={`px-3 py-1.5 rounded-xl text-xs font-bold transition cursor-pointer ${
                  typeFilter === t.id
                    ? "bg-[#0284c7] text-white shadow-xs"
                    : "bg-[#f3f4f6] text-[#4b5563] hover:bg-[#e5e7eb]"
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
          {/* جستجو */}
          <div className="relative">
            <FaSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-xs" />
            <input
              type="text"
              value={filters.search}
              onChange={(e) => setFilters((prev) => ({ ...prev, search: e.target.value }))}
              placeholder="جستجو در سوال یا پاسخ..."
              className="w-full pr-9 pl-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
            />
          </div>

          {/* انتخاب سوال */}
          <select
            value={filters.qid}
            onChange={(e) => setFilters((prev) => ({ ...prev, qid: e.target.value }))}
            className="w-full px-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
          >
            <option value="">همه سوالات عمومی</option>
            {filterOptions.questions.map((q) => (
              <option key={q.id} value={q.id}>
                #{q.id} - {q.question}
              </option>
            ))}
          </select>

          {/* انتخاب اتاق */}
          <select
            value={filters.roomFriendlyId}
            onChange={(e) => setFilters((prev) => ({ ...prev, roomFriendlyId: e.target.value }))}
            className="w-full px-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
          >
            <option value="">همه اتاق‌ها / گروه‌ها</option>
            {filterOptions.rooms.map((r) => (
              <option key={r.friendlyID} value={r.friendlyID}>
                {r.title || r.friendlyID}
              </option>
            ))}
          </select>

          {/* از تاریخ */}
          <div className="flex items-center gap-1.5">
            <span className="text-[11px] text-[#6b7280] shrink-0">از:</span>
            <input
              type="date"
              value={filters.fromDate}
              onChange={(e) => setFilters((prev) => ({ ...prev, fromDate: e.target.value }))}
              className="w-full px-2.5 py-2 text-xs border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
            />
          </div>

          {/* تا تاریخ + دکمه‌ها */}
          <div className="flex items-center gap-1.5">
            <span className="text-[11px] text-[#6b7280] shrink-0">تا:</span>
            <input
              type="date"
              value={filters.toDate}
              onChange={(e) => setFilters((prev) => ({ ...prev, toDate: e.target.value }))}
              className="w-full px-2.5 py-2 text-xs border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
            />
          </div>
        </div>

        <div className="flex flex-wrap items-center justify-end gap-2 pt-1">
          <button
            type="button"
            onClick={handleResetFilters}
            className="inline-flex items-center gap-1.5 px-4 py-2 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl text-xs font-bold transition cursor-pointer"
          >
            <FaUndo size={11} />
            <span>پاک کردن فیلترها</span>
          </button>
          <button
            type="submit"
            className="inline-flex items-center gap-1.5 px-5 py-2 bg-[#0284c7] hover:bg-[#0369a1] text-white rounded-xl text-xs font-bold transition shadow-xs cursor-pointer"
          >
            <FaFilter size={11} />
            <span>اعمال فیلتر</span>
          </button>
        </div>
      </form>

      {/* تب‌های گزارش */}
      <div className="flex items-center gap-2 overflow-x-auto pb-1">
        {[
          {
            id: "questions",
            label: `تحلیل سوالات (${visibleQuestions.length})`,
            icon: <FaChartBar />
          },
          {
            id: "meetings",
            label: `تفکیک جلسات (${meetingsBreakdown.length})`,
            icon: <FaVideo />
          },
          {
            id: "rooms",
            label: `تفکیک اتاق‌ها (${roomsBreakdown.length})`,
            icon: <FaBuilding />
          }
        ].map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setActiveTab(tab.id)}
            className={`inline-flex items-center gap-2 px-4 py-2.5 rounded-2xl text-xs sm:text-sm font-bold whitespace-nowrap transition cursor-pointer ${
              activeTab === tab.id
                ? "bg-[#1f2937] text-white shadow-sm"
                : "bg-white text-[#4b5563] border border-[#e5e7eb] hover:bg-[#f9fafb]"
            }`}
          >
            {tab.icon}
            <span>{tab.label}</span>
          </button>
        ))}
      </div>

      {/* محتوای تب ۱: تحلیل سوالات */}
      {activeTab === "questions" && (
        <div className="space-y-5">
          {visibleQuestions.length === 0 ? (
            <div className="bg-white rounded-2xl border border-[#e5e7eb] p-10 text-center text-[#6b7280]">
              هیچ سوال عمومی مطابق با فیلترهای انتخابی یافت نشد.
            </div>
          ) : (
            visibleQuestions.map((q) => {
              const typeMeta = getQuestionTypeMeta(q.type);
              const isOptional = q.type === 625 || q.type === 627;
              const chartData = (q.answersReport || []).map((a) => ({
                name: a.answer?.length > 22 ? a.answer.slice(0, 22) + "..." : a.answer,
                fullName: a.answer,
                count: a.count,
                percentage: a.percentage ?? 0,
                score: a.score
              }));

              const allTextEntries = !isOptional
                ? (q.answersReport || []).flatMap((a) => a.userAnswers || [])
                : [];

              return (
                <div
                  key={q.id}
                  className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm p-5 sm:p-6 space-y-5"
                >
                  {/* هدر سوال */}
                  <div className="flex flex-col md:flex-row md:items-start justify-between gap-3 pb-4 border-b border-[#f3f4f6]">
                    <div className="space-y-2">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="px-2.5 py-0.5 rounded-lg bg-[#f1f5f9] text-[#334155] text-xs font-mono font-bold">
                          #{q.id}
                        </span>
                        <span className={`px-2.5 py-0.5 rounded-full text-xs font-bold border ${typeMeta.badgeClass}`}>
                          {typeMeta.label}
                        </span>
                        <span className="px-2.5 py-0.5 rounded-full text-[11px] font-bold bg-[#ecfeff] text-[#0e7490] border border-[#a5f3fc]">
                          سوال عمومی (بدون ID میت)
                        </span>
                      </div>
                      <h3 className="text-base sm:text-lg font-extrabold text-[#111827] leading-relaxed">
                        {q.question}
                      </h3>
                    </div>

                    <div className="flex flex-wrap items-center gap-2 shrink-0">
                      <div className="px-3 py-1.5 rounded-xl bg-[#f9fafb] border border-[#e5e7eb] text-xs font-bold text-[#374151]">
                        آراء: <span className="text-[#0284c7] tabular-nums">{q.totalVotes}</span>
                      </div>
                      <div className="px-3 py-1.5 rounded-xl bg-[#f9fafb] border border-[#e5e7eb] text-xs font-bold text-[#374151]">
                        کاربران: <span className="text-[#7c3aed] tabular-nums">{q.uniqueRespondents}</span>
                      </div>
                      <div className="px-3 py-1.5 rounded-xl bg-[#f9fafb] border border-[#e5e7eb] text-xs font-bold text-[#374151]">
                        جلسات: <span className="text-[#059669] tabular-nums">{q.uniqueMeetingsCount}</span>
                      </div>
                      {q.overallAverageScore !== null && q.overallAverageScore !== undefined && (
                        <div className="px-3 py-1.5 rounded-xl bg-[#fef3c7] border border-[#fde68a] text-xs font-black text-[#b45309]">
                          میانگین: {q.overallAverageScore}٪
                        </div>
                      )}
                      <button
                        type="button"
                        onClick={() => openQuestionDetails(q)}
                        className="inline-flex items-center gap-1.5 px-3.5 py-1.5 bg-[#0284c7] hover:bg-[#0369a1] text-white rounded-xl text-xs font-bold transition cursor-pointer"
                      >
                        <FaEye size={12} />
                        <span>جزئیات پاسخ‌دهندگان</span>
                      </button>
                    </div>
                  </div>

                  {/* بدنه سوال: گزینه‌ای یا متن آزاد */}
                  {isOptional ? (
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-center">
                      {/* لیست گزینه‌ها با نوار پیشرفت */}
                      <div className="space-y-3">
                        {(q.answersReport || []).map((ans, idx) => (
                          <div
                            key={ans.id}
                            className="p-3.5 rounded-xl bg-[#f9fafb] border border-[#e5e7eb] space-y-2"
                          >
                            <div className="flex flex-wrap items-center justify-between gap-2 text-xs sm:text-sm">
                              <div className="flex items-center gap-2 font-bold text-[#1f2937]">
                                <span
                                  className="w-3 h-3 rounded-full shrink-0"
                                  style={{ backgroundColor: CHART_COLORS[idx % CHART_COLORS.length] }}
                                />
                                <span>{ans.answer}</span>
                                {q.type === 625 && (
                                  <span className="text-[11px] px-2 py-0.5 rounded-md bg-[#eff6ff] text-[#1d4ed8] font-semibold">
                                    امتیاز: {ans.score}
                                  </span>
                                )}
                              </div>
                              <div className="flex items-center gap-2 font-bold tabular-nums">
                                <span className="text-[#374151]">{ans.count} رأی</span>
                                <span className="px-2 py-0.5 rounded-md bg-white border border-[#e5e7eb] text-[#0284c7] text-xs">
                                  {ans.percentage}%
                                </span>
                              </div>
                            </div>
                            <div className="w-full h-2.5 bg-[#e5e7eb] rounded-full overflow-hidden">
                              <div
                                className="h-full rounded-full transition-all duration-500"
                                style={{
                                  width: `${Math.min(100, Math.max(0, ans.percentage || 0))}%`,
                                  backgroundColor: CHART_COLORS[idx % CHART_COLORS.length]
                                }}
                              />
                            </div>
                          </div>
                        ))}
                      </div>

                      {/* نمودار میله‌ای و دایره‌ای */}
                      {q.totalVotes > 0 ? (
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 bg-[#f9fafb] rounded-2xl p-4 border border-[#f3f4f6]">
                          <div className="h-56 w-full" dir="ltr">
                            <ResponsiveContainer width="100%" height="100%">
                              <BarChart data={chartData} margin={{ top: 10, right: 10, left: -15, bottom: 20 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                                <XAxis dataKey="name" tick={{ fontSize: 10 }} interval={0} />
                                <YAxis allowDecimals={false} tick={{ fontSize: 10 }} />
                                <Tooltip
                                  formatter={(val) => [`${val} رأی`, "تعداد"]}
                                  labelFormatter={(_, payload) => payload?.[0]?.payload?.fullName || ""}
                                />
                                <Bar dataKey="count" radius={[6, 6, 0, 0]}>
                                  {chartData.map((_, i) => (
                                    <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                                  ))}
                                </Bar>
                              </BarChart>
                            </ResponsiveContainer>
                          </div>

                          <div className="h-56 w-full" dir="ltr">
                            <ResponsiveContainer width="100%" height="100%">
                              <PieChart>
                                <Pie
                                  data={chartData.filter((d) => d.count > 0)}
                                  dataKey="count"
                                  nameKey="fullName"
                                  cx="50%"
                                  cy="50%"
                                  innerRadius={38}
                                  outerRadius={70}
                                  paddingAngle={3}
                                >
                                  {chartData
                                    .filter((d) => d.count > 0)
                                    .map((_, i) => (
                                      <Cell key={i} fill={CHART_COLORS[i % CHART_COLORS.length]} />
                                    ))}
                                </Pie>
                                <Tooltip formatter={(val, name) => [`${val} رأی`, name]} />
                              </PieChart>
                            </ResponsiveContainer>
                          </div>
                        </div>
                      ) : (
                        <div className="h-44 flex items-center justify-center bg-[#f9fafb] rounded-2xl border border-dashed border-[#d1d5db] text-xs text-[#6b7280]">
                          هنوز هیچ رأیی برای این سوال در بازه/فیلتر فعلی ثبت نشده است.
                        </div>
                      )}
                    </div>
                  ) : (
                    /* پاسخ‌های متنی آزاد */
                    <div className="space-y-3">
                      {allTextEntries.length === 0 ? (
                        <div className="p-6 text-center bg-[#f9fafb] rounded-xl text-xs text-[#6b7280]">
                          هنوز پاسخ متنی برای این سوال ثبت نشده است.
                        </div>
                      ) : (
                        <>
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                            {allTextEntries.slice(0, 6).map((item, i) => (
                              <div
                                key={`${item.userMeetID}-${i}`}
                                className="p-3.5 rounded-xl bg-[#f9fafb] border border-[#e5e7eb] space-y-2"
                              >
                                <div className="flex items-start gap-2 text-xs sm:text-sm text-[#1f2937] font-medium leading-relaxed">
                                  <FaCommentDots className="text-[#10b981] mt-1 shrink-0" />
                                  <span>{item.content || "بدون متن"}</span>
                                </div>
                                <div className="flex flex-wrap items-center justify-between gap-2 text-[11px] text-[#6b7280] pt-2 border-t border-[#e5e7eb]">
                                  <span className="font-bold text-[#374151]">
                                    {item.nickName || "ناشناس"}
                                  </span>
                                  <span>
                                    {item.roomTitle || item.roomFriendlyID || "-"} | {item.meetingTitle || item.meetID || "-"}
                                  </span>
                                </div>
                              </div>
                            ))}
                          </div>
                          {allTextEntries.length > 6 && (
                            <div className="text-center pt-1">
                              <button
                                type="button"
                                onClick={() => openQuestionDetails(q)}
                                className="text-xs font-bold text-[#0284c7] hover:underline cursor-pointer"
                              >
                                مشاهده همه {allTextEntries.length} پاسخ متنی...
                              </button>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  )}
                </div>
              );
            })
          )}
        </div>
      )}

      {/* محتوای تب ۲: تفکیک جلسات */}
      {activeTab === "meetings" && (
        <div className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm overflow-hidden">
          {meetingsBreakdown.length === 0 ? (
            <div className="p-10 text-center text-[#6b7280] text-sm">
              هیچ جلسه‌ای با پاسخ نظرسنجی عمومی یافت نشد.
            </div>
          ) : (
            <>
              {/* جدول دسکتاپ */}
              <div className="hidden md:block overflow-x-auto">
                <table className="w-full text-right text-sm">
                  <thead>
                    <tr className="bg-[#f9fafb] text-[#4b5563] text-xs font-bold border-b border-[#e5e7eb]">
                      <th className="py-3.5 px-4">عنوان جلسه</th>
                      <th className="py-3.5 px-4">گروه / اتاق</th>
                      <th className="py-3.5 px-4">تاریخ شمسی</th>
                      <th className="py-3.5 px-4 text-center">تعداد آراء</th>
                      <th className="py-3.5 px-4 text-center">کاربران یکتا</th>
                      <th className="py-3.5 px-4 text-center">میانگین امتیاز</th>
                      <th className="py-3.5 px-4 text-center">عملیات</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-[#f3f4f6]">
                    {meetingsBreakdown.map((m) => (
                      <tr key={m.meetID} className="hover:bg-[#f9fafb] transition">
                        <td className="py-3.5 px-4 font-bold text-[#1f2937]">
                          <div>{m.meetingTitle}</div>
                          <div className="text-[11px] font-mono text-[#9ca3af]">{m.meetID}</div>
                        </td>
                        <td className="py-3.5 px-4 text-xs font-semibold text-[#4b5563]">
                          {m.roomTitle || m.roomFriendlyID || "-"}
                        </td>
                        <td className="py-3.5 px-4 text-xs font-mono text-[#6b7280]">
                          {m.jalaliDate || "-"}
                        </td>
                        <td className="py-3.5 px-4 text-center font-bold text-[#0284c7] tabular-nums">
                          {m.votesCount}
                        </td>
                        <td className="py-3.5 px-4 text-center font-bold text-[#7c3aed] tabular-nums">
                          {m.uniqueRespondents}
                        </td>
                        <td className="py-3.5 px-4 text-center">
                          {m.averageScore !== null && m.averageScore !== undefined ? (
                            <span className="px-2.5 py-1 rounded-full bg-[#fef3c7] text-[#b45309] text-xs font-black">
                              {m.averageScore}٪
                            </span>
                          ) : (
                            <span className="text-xs text-[#9ca3af]">-</span>
                          )}
                        </td>
                        <td className="py-3.5 px-4">
                          <div className="flex items-center justify-center gap-2">
                            <button
                              type="button"
                              onClick={() => handleQuickFilterByMeeting(m.meetID)}
                              className="px-3 py-1.5 bg-[#eff6ff] hover:bg-[#dbeafe] text-[#1d4ed8] rounded-xl text-xs font-bold transition cursor-pointer"
                            >
                              فیلتر سوالات
                            </button>
                            <button
                              type="button"
                              onClick={() => navigate(`/polls/${m.meetID}/report`)}
                              className="inline-flex items-center gap-1 px-3 py-1.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] text-[#374151] rounded-xl text-xs font-bold transition cursor-pointer"
                            >
                              <span>گزارش کامل جلسه</span>
                              <FaExternalLinkAlt size={10} />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* کارت موبایل */}
              <div className="block md:hidden divide-y divide-[#f3f4f6]">
                {meetingsBreakdown.map((m) => (
                  <div key={m.meetID} className="p-4 space-y-2.5">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <h4 className="font-bold text-sm text-[#1f2937]">{m.meetingTitle}</h4>
                        <p className="text-xs text-[#6b7280] mt-0.5">
                          {m.roomTitle || m.roomFriendlyID || "-"} | {m.jalaliDate || "-"}
                        </p>
                      </div>
                      {m.averageScore !== null && m.averageScore !== undefined && (
                        <span className="px-2.5 py-0.5 rounded-full bg-[#fef3c7] text-[#b45309] text-xs font-black shrink-0">
                          {m.averageScore}٪
                        </span>
                      )}
                    </div>
                    <div className="flex items-center gap-3 text-xs text-[#4b5563]">
                      <span>آراء: <strong className="text-[#0284c7]">{m.votesCount}</strong></span>
                      <span>کاربران: <strong className="text-[#7c3aed]">{m.uniqueRespondents}</strong></span>
                    </div>
                    <div className="flex gap-2 pt-1">
                      <button
                        type="button"
                        onClick={() => handleQuickFilterByMeeting(m.meetID)}
                        className="flex-1 py-2 bg-[#eff6ff] text-[#1d4ed8] rounded-xl text-xs font-bold cursor-pointer"
                      >
                        فیلتر سوالات
                      </button>
                      <button
                        type="button"
                        onClick={() => navigate(`/polls/${m.meetID}/report`)}
                        className="flex-1 py-2 bg-[#f3f4f6] text-[#374151] rounded-xl text-xs font-bold cursor-pointer"
                      >
                        گزارش جلسه
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      )}

      {/* محتوای تب ۳: تفکیک اتاق‌ها */}
      {activeTab === "rooms" && (
        <div className="bg-white rounded-2xl border border-[#e5e7eb] shadow-sm overflow-hidden">
          {roomsBreakdown.length === 0 ? (
            <div className="p-10 text-center text-[#6b7280] text-sm">
              هیچ اتاقی با پاسخ نظرسنجی عمومی یافت نشد.
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 p-4 sm:p-5">
              {roomsBreakdown.map((r) => (
                <div
                  key={r.roomFriendlyID}
                  className="rounded-2xl border border-[#e5e7eb] bg-[#f9fafb] p-4 flex flex-col justify-between gap-3"
                >
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <h4 className="font-bold text-sm sm:text-base text-[#1f2937]">
                        {r.roomTitle || r.roomFriendlyID}
                      </h4>
                      <span className="text-[11px] font-mono text-[#6b7280]">
                        {r.roomFriendlyID}
                      </span>
                    </div>
                    {r.averageScore !== null && r.averageScore !== undefined && (
                      <span className="px-2.5 py-0.5 rounded-full bg-[#fef3c7] text-[#b45309] text-xs font-black shrink-0">
                        {r.averageScore}٪
                      </span>
                    )}
                  </div>

                  <div className="grid grid-cols-3 gap-2 text-center bg-white rounded-xl p-2.5 border border-[#e5e7eb]">
                    <div>
                      <div className="text-[10px] text-[#6b7280]">جلسات</div>
                      <div className="font-black text-sm text-[#1f2937] tabular-nums">{r.meetingsCount}</div>
                    </div>
                    <div>
                      <div className="text-[10px] text-[#6b7280]">کل آراء</div>
                      <div className="font-black text-sm text-[#0284c7] tabular-nums">{r.votesCount}</div>
                    </div>
                    <div>
                      <div className="text-[10px] text-[#6b7280]">کاربران</div>
                      <div className="font-black text-sm text-[#7c3aed] tabular-nums">{r.uniqueRespondents}</div>
                    </div>
                  </div>

                  <button
                    type="button"
                    onClick={() => handleQuickFilterByRoom(r.roomFriendlyID)}
                    className="w-full py-2 bg-[#0284c7] hover:bg-[#0369a1] text-white rounded-xl text-xs font-bold transition cursor-pointer"
                  >
                    مشاهده گزارش این اتاق
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* مودال جزئیات پاسخ‌دهندگان یک سوال */}
      {detailModal && (
        <div
          className="fixed inset-0 z-50 bg-black/50 backdrop-blur-xs flex items-center justify-center p-3 sm:p-4"
          onClick={() => setDetailModal(null)}
        >
          <div
            className="bg-white rounded-3xl w-full max-w-4xl max-h-[88vh] flex flex-col shadow-2xl overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="p-4 sm:p-5 border-b border-[#e5e7eb] flex items-start justify-between gap-3 bg-[#f9fafb]">
              <div>
                <h3 className="text-base sm:text-lg font-black text-[#1f2937]">
                  جزئیات آراء سوال #{detailModal.question.id}
                </h3>
                <p className="text-xs sm:text-sm text-[#4b5563] mt-1">
                  {detailModal.question.question}
                </p>
              </div>
              <button
                type="button"
                onClick={() => setDetailModal(null)}
                className="p-2 rounded-xl hover:bg-[#e5e7eb] text-[#6b7280] cursor-pointer"
              >
                <FaTimes />
              </button>
            </div>

            <div className="p-4 border-b border-[#f3f4f6]">
              <div className="relative">
                <FaSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-xs" />
                <input
                  type="text"
                  value={detailSearch}
                  onChange={(e) => setDetailSearch(e.target.value)}
                  placeholder="جستجو بر اساس نام کاربر، موبایل، اتاق، جلسه یا متن پاسخ..."
                  className="w-full pr-9 pl-3 py-2 text-xs sm:text-sm border border-[#d1d5db] rounded-xl focus:outline-none focus:ring-2 focus:ring-[#0284c7]"
                />
              </div>
            </div>

            <div className="flex-1 overflow-y-auto p-4">
              {filteredModalVotes.length === 0 ? (
                <div className="py-12 text-center text-sm text-[#6b7280]">
                  موردی برای نمایش یافت نشد.
                </div>
              ) : (
                <div className="space-y-2.5">
                  {filteredModalVotes.map((v, idx) => (
                    <div
                      key={`${v.userMeetID}-${idx}`}
                      className="p-3.5 rounded-2xl border border-[#e5e7eb] bg-[#f9fafb] flex flex-col sm:flex-row sm:items-center justify-between gap-3"
                    >
                      <div className="space-y-1">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="font-bold text-sm text-[#1f2937]">
                            {v.nickName || "کاربر ناشناس"}
                          </span>
                          {v.phone && (
                            <span className="text-xs font-mono text-[#6b7280] bg-white px-2 py-0.5 rounded-md border border-[#e5e7eb]">
                              {v.phone}
                            </span>
                          )}
                          <span className="text-xs font-bold px-2.5 py-0.5 rounded-full bg-[#dbeafe] text-[#1d4ed8]">
                            {v.content || v.optionAnswer}
                          </span>
                        </div>
                        <div className="flex flex-wrap items-center gap-2 text-[11px] text-[#6b7280]">
                          <span>اتاق: <strong>{v.roomTitle || v.roomFriendlyID || "-"}</strong></span>
                          <span>•</span>
                          <span>جلسه: <strong>{v.meetingTitle || v.meetID || "-"}</strong></span>
                          {v.meetingJalaliDate && (
                            <>
                              <span>•</span>
                              <span className="font-mono">{v.meetingJalaliDate}</span>
                            </>
                          )}
                        </div>
                      </div>

                      <div className="flex items-center justify-between sm:justify-end gap-2 shrink-0">
                        <span className="text-[11px] text-[#9ca3af]">
                          {formatDateTimeFa(v.dateInsert)}
                        </span>
                        {v.meetID && (
                          <button
                            type="button"
                            onClick={() => {
                              setDetailModal(null);
                              navigate(`/polls/${v.meetID}/report`);
                            }}
                            className="px-2.5 py-1 bg-white hover:bg-[#eff6ff] border border-[#d1d5db] text-[#0284c7] rounded-lg text-[11px] font-bold cursor-pointer"
                          >
                            گزارش جلسه
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default GlobalPollReport;
