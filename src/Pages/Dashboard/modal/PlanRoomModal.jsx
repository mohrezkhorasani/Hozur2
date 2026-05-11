import React, { useState, useEffect } from 'react';
import { FaTimes, FaChalkboardTeacher, FaCalendarAlt, FaClock, FaSpinner, FaTrashAlt } from 'react-icons/fa';
import moment from 'moment-jalaali';
import { GetPlanRoom, DeletePlanRoom } from '../../../api/AdminAPI';

const PlanRoomModal = ({ isOpen, onClose, roomId, roomTitle }) => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [deletingId, setDeletingId] = useState(null);
  const itemsPerPage = 10;

  useEffect(() => {
    if (isOpen && roomId) {
      fetchPlans();
    }
  }, [isOpen, roomId, page]);

  const fetchPlans = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await GetPlanRoom(roomId, page, itemsPerPage, 100);
      console.log('API Result:', result);
      
      if (result && result.data) {
        setPlans(result.data);
        setTotalCount(result.data.length);
      } else if (result && result.status === 200) {
        setPlans(result.data || []);
        setTotalCount(result.data?.length || 0);
      } else {
        setPlans([]);
        setError(result?.message || 'خطا در دریافت اطلاعات');
      }
    } catch (err) {
      console.error('Error fetching plans:', err);
      setError('متاسفانه در دریافت برنامه جلسات مشکلی پیش آمده است');
      setPlans([]);
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePlan = async (planId, planTitle) => {
    if (!window.confirm(`آیا از حذف جلسه "${planTitle}" اطمینان دارید؟`)) {
      return;
    }

    setDeletingId(planId);
    try {
      const result = await DeletePlanRoom(planId);
      console.log('Delete result:', result);
      
      if (result && result.status === 200) {
        // حذف موفقیت‌آمیز - رفرش لیست
        await fetchPlans();
      } else {
        setError(result?.message || 'خطا در حذف جلسه');
      }
    } catch (err) {
      console.error('Error deleting plan:', err);
      setError('متاسفانه در حذف جلسه مشکلی پیش آمده است');
    } finally {
      setDeletingId(null);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const momentDate = moment(dateString);
    return momentDate.format('jYYYY/jMM/jDD HH:mm');
  };

  const formatTimeOnly = (dateString) => {
    if (!dateString) return '-';
    const momentDate = moment(dateString);
    return momentDate.format('HH:mm');
  };

  const formatDateOnly = (dateString) => {
    if (!dateString) return '-';
    const momentDate = moment(dateString);
    return momentDate.format('jYYYY/jMM/jDD');
  };

  const getPlanStatus = (startAt) => {
    const now = moment();
    const planTime = moment(startAt);
    
    if (planTime.isBefore(now)) {
      return { text: 'گذشته', color: 'text-[#9ca3af]', bg: 'bg-[#f3f4f6]' };
    } else if (planTime.diff(now, 'hours') < 24) {
      return { text: 'به زودی', color: 'text-[#eab308]', bg: 'bg-[#fefce8]' };
    } else {
      return { text: 'برگزار می‌شود', color: 'text-[#22c55e]', bg: 'bg-[#f0fdf4]' };
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#000000]/50 backdrop-blur-sm animate-fadeIn">
      <div className="relative w-full max-w-2xl bg-[#ffffff] rounded-2xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
        
        {/* Header */}
        <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-6 py-4 flex justify-between items-center">
          <div>
            <h3 className="text-[#ffffff] text-lg font-bold">برنامه جلسات آینده</h3>
            <p className="text-[#30d4cc] text-sm mt-0.5">{roomTitle}</p>
          </div>
          <button
            onClick={onClose}
            className="text-[#ffffff]/70 hover:text-[#ffffff] transition-colors p-1 rounded-full hover:bg-white/10"
          >
            <FaTimes className="text-xl" />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-6">
          {loading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <FaSpinner className="animate-spin text-[#30d4cc] text-4xl mb-3" />
              <p className="text-[#6b7280]">در حال بارگذاری برنامه جلسات...</p>
            </div>
          ) : error ? (
            <div className="text-center py-12">
              <div className="w-16 h-16 mx-auto bg-[#fef2f2] rounded-full flex items-center justify-center mb-4">
                <svg className="w-8 h-8 text-[#ef4444]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <p className="text-[#ef4444] mb-2">{error}</p>
              <button
                onClick={fetchPlans}
                className="mt-3 px-4 py-2 bg-[#30d4cc] text-[#0b3f32] rounded-lg text-sm font-medium hover:bg-[#26b8b1] transition-colors"
              >
                تلاش مجدد
              </button>
            </div>
          ) : plans.length === 0 ? (
            <div className="text-center py-12">
              <div className="w-16 h-16 mx-auto bg-[#f3f4f6] rounded-full flex items-center justify-center mb-4">
                <FaCalendarAlt className="w-8 h-8 text-[#9ca3af]" />
              </div>
              <p className="text-[#6b7280] text-lg font-medium">برنامه‌ای برای نمایش وجود ندارد</p>
              <p className="text-[#9ca3af] text-sm mt-1">هنوز جلسه‌ای برای این اتاق برنامه‌ریزی نشده است</p>
            </div>
          ) : (
            <div className="space-y-3">
              {/* آمار */}
              <div className="mb-4 p-3 bg-[#f0f7f4] rounded-xl flex justify-between items-center">
                <span className="text-[#6b7280] text-sm">تعداد جلسات برنامه‌ریزی شده:</span>
                <span className="text-[#0b3f32] font-bold text-lg">{totalCount}</span>
              </div>

              {/* لیست جلسات */}
              {plans.map((plan, idx) => {
                const status = getPlanStatus(plan.startAt);
                const isDeleting = deletingId === plan.id;
                const canDelete = moment(plan.startAt).isAfter(moment()); // فقط جلسات آینده قابل حذف هستند

                return (
                  <div
                    key={plan.id || idx}
                    className="bg-[#f9fafb] rounded-xl p-4 border border-[#e5e7eb] hover:border-[#30d4cc]/30 hover:shadow-md transition-all duration-200"
                  >
                    <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-3">
                      {/* سمت راست - اطلاعات اصلی */}
                      <div className="flex-1">
                        <div className="flex items-start gap-2 mb-2">
                          <div className="w-1 h-8 bg-[#30d4cc] rounded-full mt-1"></div>
                          <div>
                            <h4 className="font-bold text-[#1f2937] text-base">
                              {plan.title || 'بدون عنوان'}
                            </h4>
                            <div className="flex flex-wrap items-center gap-3 mt-1.5">
                              <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                <FaChalkboardTeacher className="text-[#30d4cc] text-xs" />
                                {plan.teacher || 'نامشخص'}
                              </span>
                              <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                <FaCalendarAlt className="text-[#30d4cc] text-xs" />
                                {formatDateOnly(plan.startAt)}
                              </span>
                              <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                <FaClock className="text-[#30d4cc] text-xs" />
                                {formatTimeOnly(plan.startAt)}
                              </span>
                            </div>
                          </div>
                        </div>
                      </div>

                      {/* سمت چپ - وضعیت و دکمه حذف */}
                      <div className="flex items-center gap-2">
                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${status.bg} ${status.color}`}>
                          {status.text}
                        </span>
                        
                        {/* دکمه حذف - فقط برای جلسات آینده */}
                        {canDelete && (
                          <button
                            onClick={() => handleDeletePlan(plan.id, plan.title)}
                            disabled={isDeleting}
                            className="p-1.5 rounded-lg text-[#ef4444] hover:bg-[#fef2f2] transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                            title="حذف جلسه"
                          >
                            {isDeleting ? (
                              <FaSpinner className="animate-spin text-sm" />
                            ) : (
                              <FaTrashAlt className="text-sm" />
                            )}
                          </button>
                        )}
                      </div>
                    </div>

                    {/* تاریخ کامل برای موبایل */}
                    <div className="mt-2 pt-2 border-t border-[#e5e7eb] sm:hidden">
                      <p className="text-xs text-[#9ca3af]">
                        تاریخ و ساعت: {formatDate(plan.startAt)}
                      </p>
                    </div>
                  </div>
                );
              })}

              {/* Pagination - اگر بیشتر از 10 آیتم داشته باشیم */}
              {totalCount > itemsPerPage && (
                <div className="flex justify-center gap-2 mt-6 pt-4 border-t border-[#e5e7eb]">
                  <button
                    onClick={() => setPage(p => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="px-3 py-1.5 rounded-lg border border-[#e5e7eb] text-[#6b7280] disabled:opacity-50 disabled:cursor-not-allowed hover:border-[#30d4cc] hover:text-[#30d4cc] transition-colors"
                  >
                    قبلی
                  </button>
                  <span className="px-3 py-1.5 text-[#1f2937] text-sm">
                    صفحه {page}
                  </span>
                  <button
                    onClick={() => setPage(p => p + 1)}
                    disabled={plans.length < itemsPerPage}
                    className="px-3 py-1.5 rounded-lg border border-[#e5e7eb] text-[#6b7280] disabled:opacity-50 disabled:cursor-not-allowed hover:border-[#30d4cc] hover:text-[#30d4cc] transition-colors"
                  >
                    بعدی
                  </button>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="border-t border-[#f3f4f6] px-6 py-4 bg-[#f9fafb]">
          <button
            onClick={onClose}
            className="w-full py-2.5 rounded-xl border border-[#e5e7eb] text-[#6b7280] font-medium hover:bg-[#f3f4f6] transition-all"
          >
            بستن
          </button>
        </div>
      </div>
    </div>
  );
};

export default PlanRoomModal;   