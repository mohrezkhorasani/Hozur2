import React, { useState, useEffect, useRef } from 'react';
import { FaTimes, FaChalkboardTeacher, FaCalendarAlt, FaClock, FaSpinner, FaTrashAlt, FaEdit, FaSave, FaUpload, FaCheck, FaUserTie } from 'react-icons/fa';
import moment from 'moment-jalaali';
import { GetPlanRoom, DeletePlanRoom, UpdatePlanRoom } from '../../../api/AdminAPI';

const PlanRoomModal = ({ isOpen, onClose, roomId, roomTitle }) => {
  const [plans, setPlans] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [deletingId, setDeletingId] = useState(null);
  const itemsPerPage = 10;

  // Edit Plan State
  const [editingPlan, setEditingPlan] = useState(null);
  const [editFormData, setEditFormData] = useState({
    title: '',
    teacher: '',
    dabir: '',
    date: '',
    time: '',
    banner: null
  });
  const [bannerPreview, setBannerPreview] = useState(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const [updateError, setUpdateError] = useState(null);
  const [updateSuccess, setUpdateSuccess] = useState(null);
  const bannerInputRef = useRef(null);

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
      
      if (result && result.status === 200) {
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

  const handleStartEdit = (plan) => {
    setEditingPlan(plan);
    setUpdateError(null);
    setUpdateSuccess(null);
    setBannerPreview(null);
    const m = moment(plan.startAt);
    setEditFormData({
      title: plan.title || '',
      teacher: plan.teacher || '',
      dabir: plan.dabir || '',
      date: plan.date || plan.startDateJalali || (plan.startAt ? m.format('jYYYY/jMM/jDD') : ''),
      time: plan.time || plan.startTimeTehran || (plan.startAt ? m.format('HH:mm') : '10:00'),
      banner: null
    });
  };

  const handleCancelEdit = () => {
    setEditingPlan(null);
    setUpdateError(null);
    setUpdateSuccess(null);
    setBannerPreview(null);
  };

  const handleBannerChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      setEditFormData(prev => ({ ...prev, banner: file }));
      const reader = new FileReader();
      reader.onloadend = () => {
        setBannerPreview(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSaveEdit = async (e) => {
    e.preventDefault();
    if (!editFormData.title.trim()) {
      setUpdateError('موضوع جلسه الزامی است');
      return;
    }
    if (!editFormData.date.trim()) {
      setUpdateError('تاریخ جلسه الزامی است');
      return;
    }

    setIsUpdating(true);
    setUpdateError(null);
    setUpdateSuccess(null);

    try {
      const formDataToSend = new FormData();
      formDataToSend.append('idPlan', editingPlan.id);
      formDataToSend.append('title', editFormData.title.trim());
      formDataToSend.append('teacher', editFormData.teacher.trim());
      formDataToSend.append('teachers', editFormData.teacher.trim());
      formDataToSend.append('dabir', editFormData.dabir ? editFormData.dabir.trim() : '');
      formDataToSend.append('date', editFormData.date.trim());
      formDataToSend.append('time', editFormData.time ? editFormData.time.trim() : '10:00');
      formDataToSend.append('isBannerAndPresentationSame', 'true');

      if (editFormData.banner) {
        formDataToSend.append('banner', editFormData.banner);
      }

      const res = await UpdatePlanRoom(formDataToSend);
      if (res && res.status === 200) {
        setUpdateSuccess('جلسه با موفقیت ویرایش شد');
        await fetchPlans();
        setTimeout(() => {
          setEditingPlan(null);
          setUpdateSuccess(null);
        }, 1200);
      } else {
        setUpdateError(res?.message || 'خطا در ویرایش جلسه');
      }
    } catch (err) {
      console.error('Error updating plan:', err);
      setUpdateError(err.message || 'متاسفانه در ویرایش جلسه مشکلی پیش آمده است');
    } finally {
      setIsUpdating(false);
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
          {/* Form ویرایش جلسه */}
          {editingPlan ? (
            <div className="bg-[#f9fafb] border-2 border-[#30d4cc]/40 rounded-2xl p-5 mb-6 shadow-sm animate-fadeIn">
              <div className="flex justify-between items-center mb-4 pb-3 border-b border-[#e5e7eb]">
                <div className="flex items-center gap-2">
                  <FaEdit className="text-[#0b3f32] text-lg" />
                  <h4 className="font-bold text-[#0b3f32] text-base">ویرایش جلسه</h4>
                </div>
                <button
                  type="button"
                  onClick={handleCancelEdit}
                  className="text-xs text-[#6b7280] hover:text-[#ef4444] transition-colors"
                >
                  انصراف
                </button>
              </div>

              {updateError && (
                <div className="p-3 mb-4 bg-[#fef2f2] border border-[#ef4444]/30 rounded-xl text-xs text-[#ef4444] font-medium">
                  {updateError}
                </div>
              )}

              {updateSuccess && (
                <div className="p-3 mb-4 bg-[#f0fdf4] border border-[#22c55e]/30 rounded-xl text-xs text-[#166534] font-medium flex items-center gap-2">
                  <FaCheck className="text-sm" />
                  {updateSuccess}
                </div>
              )}

              <form onSubmit={handleSaveEdit} className="space-y-4">
                {/* موضوع جلسه */}
                <div>
                  <label className="block text-xs font-semibold text-[#374151] mb-1">
                    موضوع جلسه <span className="text-[#ef4444]">*</span>
                  </label>
                  <input
                    type="text"
                    value={editFormData.title}
                    onChange={(e) => setEditFormData({ ...editFormData, title: e.target.value })}
                    placeholder="موضوع نشست یا جلسه را وارد کنید..."
                    className="w-full px-3 py-2 text-sm rounded-xl border border-[#d1d5db] focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] outline-none"
                    required
                  />
                </div>

                {/* اساتید جلسه */}
                <div>
                  <label className="block text-xs font-semibold text-[#374151] mb-1">
                    اساتید جلسه <span className="text-gray-400 font-normal">(برای ثبت سمت: نام استاد(سمت))</span>
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      value={editFormData.teacher}
                      onChange={(e) => setEditFormData({ ...editFormData, teacher: e.target.value })}
                      placeholder="مثال: دکتر علی حسینی(سردبیر)، مهندس رضایی"
                      className="w-full px-3 py-2 text-sm rounded-xl border border-[#d1d5db] focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] outline-none pl-9"
                    />
                    <FaChalkboardTeacher className="absolute left-3 top-2.5 text-[#9ca3af] text-sm" />
                  </div>
                </div>

                {/* دبیر علمی */}
                <div>
                  <label className="block text-xs font-semibold text-[#374151] mb-1">
                    دبیر علمی <span className="text-gray-400 font-normal">(اختیاری)</span>
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      value={editFormData.dabir}
                      onChange={(e) => setEditFormData({ ...editFormData, dabir: e.target.value })}
                      placeholder="نام دبیر علمی..."
                      className="w-full px-3 py-2 text-sm rounded-xl border border-[#d1d5db] focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] outline-none pl-9"
                    />
                    <FaUserTie className="absolute left-3 top-2.5 text-[#9ca3af] text-sm" />
                  </div>
                </div>

                {/* تاریخ و ساعت */}
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-semibold text-[#374151] mb-1">
                      تاریخ برگزاری (شمسی) <span className="text-[#ef4444]">*</span>
                    </label>
                    <div className="relative">
                      <input
                        type="text"
                        value={editFormData.date}
                        onChange={(e) => setEditFormData({ ...editFormData, date: e.target.value })}
                        placeholder="1405/08/13"
                        className="w-full px-3 py-2 text-sm rounded-xl border border-[#d1d5db] focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] outline-none text-left pl-9 dir-ltr"
                        required
                      />
                      <FaCalendarAlt className="absolute left-3 top-2.5 text-[#9ca3af] text-sm" />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-[#374151] mb-1">
                      ساعت برگزاری <span className="text-[#ef4444]">*</span>
                    </label>
                    <div className="relative">
                      <input
                        type="text"
                        value={editFormData.time}
                        onChange={(e) => setEditFormData({ ...editFormData, time: e.target.value })}
                        placeholder="10:30"
                        className="w-full px-3 py-2 text-sm rounded-xl border border-[#d1d5db] focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] outline-none text-left pl-9 dir-ltr"
                        required
                      />
                      <FaClock className="absolute left-3 top-2.5 text-[#9ca3af] text-sm" />
                    </div>
                  </div>
                </div>

                {/* تغییر بنر */}
                <div>
                  <label className="block text-xs font-semibold text-[#374151] mb-1">
                    تصویر بنر جدید <span className="text-gray-400 font-normal">(در صورت تمایل به تغییر)</span>
                  </label>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => bannerInputRef.current?.click()}
                      className="px-3 py-2 bg-white border border-[#d1d5db] rounded-xl text-xs font-medium text-[#374151] hover:border-[#30d4cc] hover:text-[#0b3f32] flex items-center gap-2 transition-colors"
                    >
                      <FaUpload className="text-[#30d4cc]" />
                      انتخاب تصویر جدید
                    </button>
                    <input
                      ref={bannerInputRef}
                      type="file"
                      accept="image/*"
                      onChange={handleBannerChange}
                      className="hidden"
                    />
                    {editFormData.banner && (
                      <span className="text-xs text-[#0b3f32] font-medium truncate max-w-[200px]">
                        {editFormData.banner.name}
                      </span>
                    )}
                  </div>
                  {bannerPreview && (
                    <div className="mt-2 w-28 h-16 rounded-lg overflow-hidden border border-[#d1d5db]">
                      <img src={bannerPreview} alt="Preview" className="w-full h-full object-cover" />
                    </div>
                  )}
                </div>

                {/* دکمه‌های فرم */}
                <div className="flex justify-end gap-2 pt-2 border-t border-[#e5e7eb]">
                  <button
                    type="button"
                    onClick={handleCancelEdit}
                    disabled={isUpdating}
                    className="px-4 py-2 rounded-xl border border-[#d1d5db] text-xs font-medium text-[#6b7280] hover:bg-white transition-colors"
                  >
                    انصراف
                  </button>
                  <button
                    type="submit"
                    disabled={isUpdating}
                    className="px-5 py-2 rounded-xl bg-gradient-to-r from-[#0b3f32] to-[#0a352a] text-[#ffffff] text-xs font-bold hover:shadow-md transition-all flex items-center gap-1.5 disabled:opacity-50"
                  >
                    {isUpdating ? (
                      <>
                        <FaSpinner className="animate-spin text-xs" />
                        در حال ذخیره...
                      </>
                    ) : (
                      <>
                        <FaSave className="text-xs" />
                        ذخیره تغییرات
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          ) : null}

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
                              {plan.dabir && (
                                <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                  <FaUserTie className="text-[#30d4cc] text-xs" />
                                  {plan.dabir}
                                </span>
                              )}
                              <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                <FaCalendarAlt className="text-[#30d4cc] text-xs" />
                                {plan.date || plan.startDateJalali || formatDateOnly(plan.startAt)}
                              </span>
                              <span className="flex items-center gap-1.5 text-xs text-[#6b7280]">
                                <FaClock className="text-[#30d4cc] text-xs" />
                                {plan.time || plan.startTimeTehran || formatTimeOnly(plan.startAt)}
                              </span>
                            </div>
                          </div>
                        </div>
                      </div>

                      {/* سمت چپ - وضعیت و دکمه‌های ویرایش و حذف */}
                      <div className="flex items-center gap-2">
                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${status.bg} ${status.color}`}>
                          {status.text}
                        </span>

                        {/* دکمه ویرایش */}
                        <button
                          onClick={() => handleStartEdit(plan)}
                          className="p-1.5 rounded-lg text-[#0b3f32] hover:bg-[#30d4cc]/15 border border-[#30d4cc]/30 transition-all duration-200"
                          title="ویرایش جلسه"
                        >
                          <FaEdit className="text-sm" />
                        </button>
                        
                        {/* دکمه حذف - فقط برای جلسات آینده */}
                        {canDelete && (
                          <button
                            onClick={() => handleDeletePlan(plan.id, plan.title)}
                            disabled={isDeleting}
                            className="p-1.5 rounded-lg text-[#ef4444] hover:bg-[#fef2f2] border border-[#ef4444]/20 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
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
