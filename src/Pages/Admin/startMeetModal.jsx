// src/components/StartMeetingModal.jsx
import React, { useState, useEffect, useMemo } from 'react';
import CreatableSelect from 'react-select/creatable';

const StartMeetingModal = ({ isOpen, onClose, onStartMeeting, onStartWithoutInfo, room, teachersList = [] }) => {
  const [formData, setFormData] = useState({
    title: '',
    teachers: [],
    scientificSecretary: null,
    isPublic: true,
  });
  const [isTest, setIsTest] = useState(false);

  // تبدیل لیست اساتید به فرمت react-select (برای هر دو فیلد)
  const teacherOptions = useMemo(() => {
    return teachersList.map(teacher => ({
      value: teacher.id || teacher.fullname,
      label: teacher.fullname,
      isNew: false,
    }));
  }, [teachersList]);

  // ریست فرم هنگام بسته شدن
  useEffect(() => {
    if (!isOpen) {
      setFormData({
        title: '',
        teachers: [],
        scientificSecretary: null,
        isPublic: true,
      });
      setIsTest(false);
    }
  }, [isOpen]);

  const handleTeacherChange = (selectedOptions) => {
    setFormData(prev => ({
      ...prev,
      teachers: selectedOptions || [],
    }));
  };

  const handleSecretaryChange = (selectedOption) => {
    setFormData(prev => ({
      ...prev,
      scientificSecretary: selectedOption || null,
    }));
  };

  const handleSubmit = (e) => {
    if (e) e.preventDefault();
    
    // validation
    let titleToSubmit = formData.title.trim();
    if (!titleToSubmit) {
      if (isTest) {
        titleToSubmit = `جلسه تستی ${room?.title ? `(${room.title})` : ''}`.trim();
      } else {
        alert('لطفاً عنوان جلسه را وارد کنید');
        return;
      }
    }

    const submitData = {
      roomId: room?.id || room?._id || room?.roomID,
      title: titleToSubmit,
      teachers: formData.teachers.map(t => ({
        name: t.label,
        id: t.value,
      })),
      scientificSecretary: formData.scientificSecretary ? {
        name: formData.scientificSecretary.label,
        id: formData.scientificSecretary.value,
      } : null,
      isPublic: formData.isPublic,
      isTest: isTest,
      type: isTest ? -99 : null,
    };
    
    onStartMeeting(submitData);
    onClose();
  };

  const handleStartWithoutInfo = () => {
    onStartWithoutInfo({
      roomId: room?.id || room?._id || room?.roomID,
      startNow: true,
      isTest: isTest,
      type: isTest ? -99 : null,
    });
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="sticky top-0 bg-white border-b px-6 py-4 rounded-t-2xl">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-bold text-gray-800">
              شروع جلسه جدید
              {room && <span className="text-sm text-gray-500 mr-2">({room.title})</span>}
            </h2>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="p-6 space-y-5">
          {/* دکمه / انتخابگر جلسه تستی */}
          <div
            onClick={() => setIsTest(prev => !prev)}
            className={`p-3.5 rounded-xl border transition-all cursor-pointer flex items-center justify-between select-none ${
              isTest
                ? 'bg-amber-50 border-amber-300 shadow-xs'
                : 'bg-gray-50 border-gray-200 hover:bg-gray-100/70'
            }`}
          >
            <div className="flex items-center gap-3">
              <div
                className={`w-9 h-9 rounded-lg flex items-center justify-center text-lg transition-colors ${
                  isTest ? 'bg-amber-100 text-amber-700' : 'bg-gray-200 text-gray-500'
                }`}
              >
                🧪
              </div>
              <div>
                <div className="flex items-center gap-2">
                  <span className={`text-sm font-bold ${isTest ? 'text-amber-900' : 'text-gray-800'}`}>
                    جلسه تستی
                  </span>
                  {isTest ? (
                    <span className="text-[11px] px-2 py-0.5 rounded-full font-bold bg-amber-200 text-amber-900 border border-amber-300">
                      فعال (تایپ ۹۹-)
                    </span>
                  ) : (
                    <span className="text-[11px] px-2 py-0.5 rounded-full text-gray-500 bg-gray-200/70">
                      عادی
                    </span>
                  )}
                </div>
                <p className="text-xs text-gray-500 mt-0.5">
                  با فعال‌سازی این گزینه، جلسه به صورت آزمایشی ایجاد می‌شود و در آمار و محاسبات لحاظ نخواهد شد.
                </p>
              </div>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={isTest}
              onClick={(e) => {
                e.stopPropagation();
                setIsTest(prev => !prev);
              }}
              className={`relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors ${
                isTest ? 'bg-amber-500' : 'bg-gray-300'
              }`}
            >
              <span
                className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-xs transition-transform ${
                  isTest ? 'translate-x-6' : 'translate-x-1'
                }`}
              />
            </button>
          </div>

          {/* عنوان جلسه */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              عنوان جلسه <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={formData.title}
              onChange={(e) => setFormData(prev => ({ ...prev, title: e.target.value }))}
              placeholder="مثال: جلسه تفسیر قرآن - سوره مبارکه حمد"
              className="w-full px-4 py-2.5 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all"
              required
            />
          </div>

          {/* اساتید (چندتایی) */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              نام اساتید
              <span className="text-xs text-gray-500 mr-2">(می‌توانید چند استاد انتخاب کنید یا اسم جدید وارد کنید)</span>
            </label>
            <CreatableSelect
              isMulti
              value={formData.teachers}
              onChange={handleTeacherChange}
              options={teacherOptions}
              placeholder="جستجو یا وارد کنید..."
              isClearable
              className="react-select-container"
              classNamePrefix="react-select"
              styles={{
                control: (base) => ({
                  ...base,
                  borderRadius: '0.75rem',
                  padding: '0.125rem 0',
                  borderColor: '#e5e7eb',
                }),
              }}
              formatCreateLabel={(inputValue) => `اضافه کردن "${inputValue}"`}
            />
            <p className="text-xs text-gray-400 mt-1">
              می‌توانید نام جدید وارد کنید، در صورت نبودن در لیست اضافه می‌شود
            </p>
          </div>

          {/* دبیر علمی (تک انتخابی با سرچ) - استفاده از همان teacherOptions */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-1">
              دبیر علمی
            </label>
            <CreatableSelect
              value={formData.scientificSecretary}
              onChange={handleSecretaryChange}
              options={teacherOptions}
              placeholder="جستجو یا وارد کنید..."
              isClearable
              className="react-select-container"
              classNamePrefix="react-select"
              styles={{
                control: (base) => ({
                  ...base,
                  borderRadius: '0.75rem',
                  padding: '0.125rem 0',
                  borderColor: '#e5e7eb',
                }),
              }}
              formatCreateLabel={(inputValue) => `اضافه کردن "${inputValue}"`}
              noOptionsMessage={() => 'نتیجه‌ای یافت نشد، می‌توانید اسم جدید وارد کنید'}
            />
            <p className="text-xs text-gray-400 mt-1">
              از لیست اساتید انتخاب کنید یا نام جدید وارد کنید
            </p>
          </div>

          {/* تیک شرکت برای عموم آزاد باشد */}
          <div className="flex items-center justify-between p-3 bg-gray-50 rounded-xl">
            <label className="text-sm font-semibold text-gray-700 cursor-pointer">
              شرکت برای عموم آزاد باشد
            </label>
            <button
              type="button"
              onClick={() => setFormData(prev => ({ ...prev, isPublic: !prev.isPublic }))}
              className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors ${
                formData.isPublic ? 'bg-[#30d4cc]' : 'bg-gray-300'
              }`}
            >
              <span
                className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                  formData.isPublic ? 'translate-x-6' : 'translate-x-1'
                }`}
              />
            </button>
          </div>
        </form>

        {/* Footer - دکمه‌ها */}
        <div className="sticky bottom-0 bg-white border-t px-6 py-4 rounded-b-2xl space-y-3">
          <div className="flex gap-3">
            <button
              onClick={handleSubmit}
              className={`flex-1 py-2.5 rounded-xl font-semibold hover:shadow-lg transition-all ${
                isTest
                  ? 'bg-amber-400 hover:bg-amber-500 text-amber-950 font-bold shadow-amber-200'
                  : 'bg-linear-to-r from-[#30d4cc] to-[#2bc4bc] text-[#000000]'
              }`}
            >
              {isTest ? '🧪 شروع جلسه تستی با اطلاعات بالا' : '🚀 شروع جلسه با اطلاعات بالا'}
            </button>
            <button
              onClick={handleStartWithoutInfo}
              className={`flex-1 py-2.5 rounded-xl font-semibold transition-all ${
                isTest
                  ? 'bg-amber-100 text-amber-900 hover:bg-amber-200'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              {isTest ? '⚡ شروع تستی بدون اطلاعات' : '⚡ شروع جلسه بدون اطلاعات'}
            </button>
          </div>
          <button
            onClick={onClose}
            className="w-full text-gray-400 text-sm py-2 hover:text-gray-600 transition-colors"
          >
            انصراف
          </button>
        </div>
      </div>
    </div>
  );
};

export default StartMeetingModal;