import React, { useState, useEffect, useMemo, useRef } from 'react';
import { FaTimes, FaCalendarAlt, FaClock, FaBookOpen, FaChevronRight, FaChevronLeft, FaUpload, FaImage } from 'react-icons/fa';
import moment from 'moment-jalaali';
import CreatableSelect from 'react-select/creatable';
import { NewPlanRoom } from '../../../api/AdminAPI';

const PersianCalendarModal = ({ isOpen, onClose, onSubmit, room, initialData, teachersList = [] }) => {
  const [formData, setFormData] = useState({
    startDate: initialData?.startDate || '',
    title: initialData?.title || '',
    friendlyID: room?.roomID || '',
    teachers: [],
    scientificSecretary: null,
    banner: null,
    bannerPresentation: null,
    isBannerAndPresentationSame: true,
  });
  const [errors, setErrors] = useState({});
  const [currentMonth, setCurrentMonth] = useState(moment().startOf('jMonth'));
  const [showCalendar, setShowCalendar] = useState(false);
  const [selectedDay, setSelectedDay] = useState(null);
  const [meetingDate, setMeetingDate] = useState('');
  const [meetingTime, setMeetingTime] = useState('');
  const [loading, setLoading] = useState(false);

  const bannerInputRef = useRef(null);
  const presentationInputRef = useRef(null);

  const teacherOptions = useMemo(() => {
    return teachersList.map(teacher => ({
      value: teacher.id || teacher.fullname,
      label: teacher.fullname,
      isNew: false,
    }));
  }, [teachersList]);

  useEffect(() => {
    if (!isOpen) {
      setFormData({
        startDate: '',
        title: '',
        friendlyID: room?.roomID || '',
        teachers: [],
        scientificSecretary: null,
        banner: null,
        bannerPresentation: null,
        isBannerAndPresentationSame: true,
      });
      setMeetingDate('');
      setMeetingTime('');
      setSelectedDay(null);
      setErrors({});
    }
  }, [isOpen, room]);

  const weekDays = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];
  const weekDaysFull = ['شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنجشنبه', 'جمعه'];

  if (room == null) {
    return null;
  }
  const meetingName = room.title;

  const getNextWeekDays = () => {
    const days = [];
    const today = moment();
    for (let i = 0; i < 7; i++) {
      const date = moment(today).add(i, 'days');
      days.push({
        label: weekDaysFull[date.day()],
        date: date.format('jYYYY/jMM/jDD'),
        day: date.date(),
        month: date.jMonth(),
        year: date.jYear(),
      });
    }
    return days;
  };

  const nextWeekDays = getNextWeekDays();

  const getMonthDays = () => {
    const startOfMonth = moment(currentMonth).startOf('jMonth');
    const endOfMonth = moment(currentMonth).endOf('jMonth');
    const startDayOfWeek = startOfMonth.day();

    const days = [];
    const prevMonthDays = startDayOfWeek;

    const prevMonth = moment(currentMonth).subtract(1, 'jMonth');
    const prevMonthEnd = moment(prevMonth).endOf('jMonth');
    for (let i = prevMonthDays - 1; i >= 0; i--) {
      const day = moment(prevMonthEnd).subtract(i, 'days');
      days.push({
        date: day.format('jYYYY/jMM/jDD'),
        day: day.jDate(),
        month: day.jMonth(),
        year: day.jYear(),
        isCurrentMonth: false,
      });
    }

    for (let i = 1; i <= endOfMonth.jDate(); i++) {
      const day = moment(currentMonth).jDate(i);
      days.push({
        date: day.format('jYYYY/jMM/jDD'),
        day: i,
        month: currentMonth.jMonth(),
        year: currentMonth.jYear(),
        isCurrentMonth: true,
        isToday: day.isSame(moment(), 'day'),
      });
    }

    const remainingDays = 42 - days.length;
    for (let i = 1; i <= remainingDays; i++) {
      const day = moment(currentMonth).add(1, 'jMonth').jDate(i);
      days.push({
        date: day.format('jYYYY/jMM/jDD'),
        day: i,
        month: day.jMonth(),
        year: day.jYear(),
        isCurrentMonth: false,
      });
    }

    return days;
  };

  const monthDays = getMonthDays();

  const changeMonth = (increment) => {
    setCurrentMonth(moment(currentMonth).add(increment, 'jMonth'));
    setSelectedDay(null);
  };

  const selectDate = (date) => {
    setMeetingDate(date);
    setSelectedDay(date);
    setShowCalendar(false);
  };

  const selectQuickDate = (date) => {
    setMeetingDate(date);
    setSelectedDay(date);
    const [year, month, day] = date.split('/');
    setCurrentMonth(moment(`${year}/${month}/01`, 'jYYYY/jMM/jDD'));
    setShowCalendar(false);
  };

  const generateTimeOptions = () => {
    const times = [];
    for (let i = 0; i < 24; i++) {
      for (let j = 0; j < 60; j += 30) {
        const hour = i.toString().padStart(2, '0');
        const minute = j.toString().padStart(2, '0');
        times.push(`${hour}:${minute}`);
      }
    }
    return times;
  };

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

  const handleChange = (e) => {
    const { name, value } = e.target;
    if (name === 'title') {
      setFormData(prev => ({ ...prev, title: value }));
    } else if (name === 'meetingTime') {
      setMeetingTime(value);
    } else if (name === 'isBannerAndPresentationSame') {
      setFormData(prev => ({
        ...prev,
        isBannerAndPresentationSame: value === 'true',
        bannerPresentation: value === 'true' ? null : prev.bannerPresentation
      }));
    }
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const handleFileChange = (e, fileType) => {
    const file = e.target.files[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        setErrors(prev => ({ ...prev, [fileType]: 'فایل باید تصویر باشد' }));
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        setErrors(prev => ({ ...prev, [fileType]: 'حجم فایل نباید بیشتر از 5 مگابایت باشد' }));
        return;
      }

      setFormData(prev => ({
        ...prev,
        [fileType]: file,
        ...(fileType === 'banner' && prev.isBannerAndPresentationSame && {
          bannerPresentation: file
        })
      }));

      if (errors[fileType]) {
        setErrors(prev => ({ ...prev, [fileType]: '' }));
      }
    }
  };

  const removeFile = (fileType) => {
    setFormData(prev => ({
      ...prev,
      [fileType]: null,
      ...(fileType === 'bannerPresentation' && prev.isBannerAndPresentationSame && {
        isBannerAndPresentationSame: false
      })
    }));

    if (fileType === 'banner' && bannerInputRef.current) {
      bannerInputRef.current.value = '';
    }
    if (fileType === 'bannerPresentation' && presentationInputRef.current) {
      presentationInputRef.current.value = '';
    }
  };

  const validateForm = () => {
    const newErrors = {};
    if (!meetingDate) newErrors.meetingDate = 'تاریخ جلسه الزامی است';
    if (!meetingTime) newErrors.meetingTime = 'ساعت جلسه الزامی است';
    if (!formData.title) newErrors.title = 'موضوع جلسه الزامی است';
    if (formData.teachers.length === 0) newErrors.teachers = 'حداقل یک استاد انتخاب کنید';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const convertToDateTime = (date, time) => {
    if (!date || !time) return null;
    const [year, month, day] = date.split('/');
    const [hour, minute] = time.split(':');
    
    // Parse using moment-jalaali. This creates a moment object.
    const jalaliDate = moment(`${year}/${month}/${day} ${hour}:${minute}`, 'jYYYY/jMM/jDD HH:mm');
    
    // The user inputs Tehran time. Iran is strictly +03:30.
    // To get the correct UTC string regardless of user's browser timezone:
    // Format the moment date to a standard Gregorian string, then append Tehran's offset, then convert to ISO.
    const gregorianStr = jalaliDate.format('YYYY-MM-DDTHH:mm:00');
    return new Date(`${gregorianStr}+03:30`).toISOString();
  };

  const handleSubmit = async (e) => {
    setLoading(true)
    e.preventDefault();
    if (validateForm()) {
      const formDataToSend = new FormData();

      formDataToSend.append('friendlyID', room.roomID);
      formDataToSend.append('title', formData.title);
      formDataToSend.append('teachers', JSON.stringify(formData.teachers.map(t => ({
        name: t.label,
        id: t.value,
      }))));
      formDataToSend.append('teacher', formData.teachers.map(t => t.label).join('، '));
      formDataToSend.append('dabir', formData.scientificSecretary ? formData.scientificSecretary.label : null);
      formDataToSend.append('startDate', convertToDateTime(meetingDate, meetingTime));

      if (formData.banner) {
        formDataToSend.append('banner', formData.banner);
      }
      if (formData.bannerPresentation && !formData.isBannerAndPresentationSame) {
        formDataToSend.append('bannerPresentation', formData.bannerPresentation);
      }
      formDataToSend.append('isBannerAndPresentationSame', formData.isBannerAndPresentationSame.toString());

      try {

        var resp = await NewPlanRoom(formDataToSend);
        if (resp.status != 200) {
          alert(resp.message)
        } else {
          if (onSubmit) onSubmit(formDataToSend);
          onClose();
        }
      } catch (e) {
          alert(e)

      }

      setLoading(false)


    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#000000]/50 backdrop-blur-sm animate-fadeIn">
      <div className="relative w-full max-w-full bg-[#ffffff] rounded-2xl shadow-2xl overflow-hidden max-h-[95vh]">

        {/* Header */}
        <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-8 py-4 flex justify-between items-center flex-shrink-0">
          <h3 className="text-[#ffffff] text-xl font-bold">تنظیم جلسه جدید</h3>
          <button
            onClick={onClose}
            className="text-[#ffffff]/70 hover:text-[#ffffff] transition-colors p-1 rounded-full hover:bg-white/10"
          >
            <FaTimes className="text-2xl" />
          </button>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="p-6 overflow-hidden">
          {/* نام جلسه - در عرض کامل */}
          <div className="mb-4 p-3 bg-[#f0f7f4] rounded-xl border border-[#30d4cc]/20">
            <p className="text-xs text-[#6b7280] mb-1">نام جلسه</p>
            <p className="text-[#1f2937] font-semibold">{meetingName}</p>
          </div>

          {/* دو ستون اصلی */}
          <div className="grid grid-cols-2 gap-6">
            {/* ستون راست */}
            <div className="space-y-4">
              {/* موضوع جلسه */}
              <div>
                <label className="block text-sm font-medium text-[#374151] mb-1.5">
                  موضوع جلسه <span className="text-[#ef4444]">*</span>
                </label>
                <div className="relative">
                  <FaBookOpen className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af]" />
                  <input
                    type="text"
                    name="title"
                    value={formData.title}
                    onChange={handleChange}
                    placeholder="موضوع جلسه را وارد کنید"
                    className={`w-full pr-10 pl-3 py-2.5 border rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent bg-[#f9fafb]/50 text-[#1f2937] ${errors.title ? 'border-[#ef4444]' : 'border-[#e5e7eb]'
                      }`}
                  />
                </div>
                {errors.title && <p className="text-xs text-[#ef4444] mt-1">{errors.title}</p>}
              </div>

              {/* اساتید */}
              <div>
                <label className="block text-sm font-medium text-[#374151] mb-1.5">
                  اساتید جلسه <span className="text-[#ef4444]">*</span>
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
                      borderColor: errors.teachers ? '#ef4444' : '#e5e7eb',
                    }),
                  }}
                  formatCreateLabel={(inputValue) => `اضافه کردن "${inputValue}"`}
                />
                <span className="text-[11px] text-gray-500 block mt-1">
                  می‌توانید سمت استاد را داخل پرانتز وارد کنید، مانند: علیرضا حسینی(سردبیر روزنامه حکمت)
                </span>
                {errors.teachers && <p className="text-xs text-[#ef4444] mt-1">{errors.teachers}</p>}
              </div>

              {/* دبیر علمی */}
              <div>
                <label className="block text-sm font-medium text-[#374151] mb-1.5">
                  دبیر علمی <span className="text-xs text-gray-500">(اختیاری)</span>
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
                  noOptionsMessage={() => 'نتیجه‌ای یافت نشد'}
                />
                <span className="text-[11px] text-gray-500 block mt-1">
                  می‌توانید سمت دبیر را داخل پرانتز بنویسید، مانند: محمد حسنی(دبیر علمی)
                </span>
              </div>
            </div>

            {/* ستون چپ */}
            <div className="space-y-4">
              {/* تاریخ و ساعت کنار هم */}
              <div className="grid grid-cols-2 gap-3">
                {/* تاریخ */}
                <div className="relative">
                  <label className="block text-sm font-medium text-[#374151] mb-1.5">
                    تاریخ جلسه <span className="text-[#ef4444]">*</span>
                  </label>
                  <div className="relative">
                    <FaCalendarAlt className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af]" />
                    <input
                      type="text"
                      value={meetingDate}
                      onFocus={() => setShowCalendar(true)}
                      placeholder="انتخاب تاریخ"
                      readOnly
                      className={`w-full pr-10 pl-3 py-2.5 border rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent cursor-pointer bg-[#f9fafb]/50 text-[#1f2937] ${errors.meetingDate ? 'border-[#ef4444]' : 'border-[#e5e7eb]'
                        }`}
                    />
                  </div>
                  {errors.meetingDate && <p className="text-xs text-[#ef4444] mt-1">{errors.meetingDate}</p>}

                  {/* تقویم */}
                  {showCalendar && (
                    <div className="absolute top-full left-0 right-0 mt-2 bg-[#ffffff] rounded-xl shadow-xl border border-[#e5e7eb] z-10 p-3" style={{ width: '320px' }}>
                      <div className="flex justify-between items-center mb-3 pb-2 border-b border-[#f3f4f6]">
                        <button
                          type="button"
                          onClick={() => changeMonth(-1)}
                          className="p-1 hover:bg-[#f3f4f6] rounded-lg transition-colors text-[#30d4cc]"
                        >
                          <FaChevronRight />
                        </button>
                        <span className="font-bold text-[#1f2937]">
                          {currentMonth.format('jMMMM jYYYY')}
                        </span>
                        <button
                          type="button"
                          onClick={() => changeMonth(1)}
                          className="p-1 hover:bg-[#f3f4f6] rounded-lg transition-colors text-[#30d4cc]"
                        >
                          <FaChevronLeft />
                        </button>
                      </div>

                      <div className="grid grid-cols-7 gap-1 mb-2">
                        {weekDays.map((day, idx) => (
                          <div key={idx} className="text-center text-xs font-bold text-[#9ca3af] py-1">
                            {day}
                          </div>
                        ))}
                      </div>

                      <div className="grid grid-cols-7 gap-1">
                        {monthDays.map((day, idx) => (
                          <button
                            key={idx}
                            type="button"
                            onClick={() => selectDate(day.date)}
                            className={`
                              text-center py-2 rounded-lg text-sm transition-all
                              ${!day.isCurrentMonth ? 'text-[#d1d5db]' : 'text-[#374151]'}
                              ${day.isToday ? 'border border-[#30d4cc] font-bold' : ''}
                              ${meetingDate === day.date ? 'bg-[#30d4cc] text-[#0b3f32] font-bold shadow-sm' : 'hover:bg-[#f3f4f6]'}
                            `}
                          >
                            {day.day}
                          </button>
                        ))}
                      </div>
                    </div>
                  )}
                </div>

                {/* ساعت */}
                <div>
                  <label className="block text-sm font-medium text-[#374151] mb-1.5">
                    ساعت جلسه <span className="text-[#ef4444]">*</span>
                  </label>
                  <div className="relative">
                    <FaClock className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af]" />
                    <select
                      name="meetingTime"
                      value={meetingTime}
                      onChange={handleChange}
                      className={`w-full pr-10 pl-3 py-2.5 border rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent bg-[#f9fafb]/50 text-[#1f2937] ${errors.meetingTime ? 'border-[#ef4444]' : 'border-[#e5e7eb]'
                        }`}
                    >
                      <option value="">ساعت</option>
                      {generateTimeOptions().map(time => (
                        <option key={time} value={time}>{time}</option>
                      ))}
                    </select>
                  </div>
                  {errors.meetingTime && <p className="text-xs text-[#ef4444] mt-1">{errors.meetingTime}</p>}
                </div>
              </div>

              {/* انتخاب سریع تاریخ */}
              <div>
                <label className="block text-sm font-medium text-[#374151] mb-1.5">انتخاب سریع تاریخ</label>
                <div className="grid grid-cols-7 gap-1.5">
                  {nextWeekDays.map((day, idx) => (
                    <button
                      key={idx}
                      type="button"
                      onClick={() => selectQuickDate(day.date)}
                      className={`py-2 rounded-lg text-center transition-all text-sm font-medium ${meetingDate === day.date
                        ? 'bg-[#30d4cc] text-[#0b3f32] shadow-md'
                        : 'bg-[#f3f4f6] text-[#374151] hover:bg-[#30d4cc]/20'
                        }`}
                    >
                      <div className="font-bold text-xs">{day.label}</div>
                      <div className="text-xs">{day.day}</div>
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* بخش تصاویر - در عرض کامل */}
          <div className="mt-6 pt-4 border-t border-[#e5e7eb]">
            <h4 className="text-sm font-semibold text-[#374151] mb-3 flex items-center gap-2">
              <FaImage className="text-[#30d4cc]" />
              تصاویر جلسه
            </h4>

            <div className="grid grid-cols-2 gap-6">
              {/* بنر */}
              <div>
                <label className="block text-sm font-medium text-[#374151] mb-1.5">
                  تصویر بنر
                </label>
                <div className="flex items-center gap-3">
                  <input
                    ref={bannerInputRef}
                    type="file"
                    accept="image/*"
                    onChange={(e) => handleFileChange(e, 'banner')}
                    className="hidden"
                    id="banner-upload"
                  />
                  <label
                    htmlFor="banner-upload"
                    className="flex items-center gap-2 px-4 py-2.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] rounded-xl cursor-pointer transition-all border border-[#e5e7eb] text-[#374151] text-sm font-medium whitespace-nowrap"
                  >
                    <FaUpload className="text-[#30d4cc]" />
                    انتخاب تصویر
                  </label>
                  {formData.banner && (
                    <div className="flex items-center gap-2 bg-[#f0f7f4] px-3 py-1.5 rounded-xl border border-[#30d4cc]/30 flex-1 min-w-0">
                      <span className="text-sm text-[#0b3f32] truncate">
                        {formData.banner.name}
                      </span>
                      <button
                        type="button"
                        onClick={() => removeFile('banner')}
                        className="text-[#ef4444] hover:text-[#dc2626] flex-shrink-0"
                      >
                        <FaTimes />
                      </button>
                    </div>
                  )}
                </div>
                {errors.banner && <p className="text-xs text-[#ef4444] mt-1">{errors.banner}</p>}
                <p className="text-[10px] text-[#9ca3af] mt-1">حداکثر حجم: ۵ مگابایت - jpg, jpeg, png, gif</p>
              </div>

              {/* ارائه */}
              <div>
                <div className="flex items-center gap-3 mb-1.5">
                  <input
                    type="checkbox"
                    id="isBannerAndPresentationSame"
                    name="isBannerAndPresentationSame"
                    checked={formData.isBannerAndPresentationSame}
                    onChange={(e) => {
                      setFormData(prev => ({
                        ...prev,
                        isBannerAndPresentationSame: e.target.checked,
                        bannerPresentation: e.target.checked ? null : prev.bannerPresentation
                      }));
                    }}
                    className="w-4 h-4 text-[#30d4cc] border-[#d1d5db] rounded focus:ring-[#30d4cc]"
                  />
                  <label htmlFor="isBannerAndPresentationSame" className="text-sm text-[#374151] cursor-pointer">
                    تصویر بنر و ارائه یکسان است
                  </label>
                </div>

                {!formData.isBannerAndPresentationSame && (
                  <>
                    <label className="block text-sm font-medium text-[#374151] mb-1.5">
                      تصویر ارائه
                    </label>
                    <div className="flex items-center gap-3">
                      <input
                        ref={presentationInputRef}
                        type="file"
                        accept="image/*"
                        onChange={(e) => handleFileChange(e, 'bannerPresentation')}
                        className="hidden"
                        id="presentation-upload"
                      />
                      <label
                        htmlFor="presentation-upload"
                        className="flex items-center gap-2 px-4 py-2.5 bg-[#f3f4f6] hover:bg-[#e5e7eb] rounded-xl cursor-pointer transition-all border border-[#e5e7eb] text-[#374151] text-sm font-medium whitespace-nowrap"
                      >
                        <FaUpload className="text-[#30d4cc]" />
                        انتخاب تصویر
                      </label>
                      {formData.bannerPresentation && (
                        <div className="flex items-center gap-2 bg-[#f0f7f4] px-3 py-1.5 rounded-xl border border-[#30d4cc]/30 flex-1 min-w-0">
                          <span className="text-sm text-[#0b3f32] truncate">
                            {formData.bannerPresentation.name}
                          </span>
                          <button
                            type="button"
                            onClick={() => removeFile('bannerPresentation')}
                            className="text-[#ef4444] hover:text-[#dc2626] flex-shrink-0"
                          >
                            <FaTimes />
                          </button>
                        </div>
                      )}
                    </div>
                    {errors.bannerPresentation && <p className="text-xs text-[#ef4444] mt-1">{errors.bannerPresentation}</p>}
                    <p className="text-[10px] text-[#9ca3af] mt-1">حداکثر حجم: ۵ مگابایت - jpg, jpeg, png, gif</p>
                  </>
                )}
              </div>
            </div>
          </div>

          {/* دکمه‌ها */}
          <div className="flex gap-3 mt-6 pt-4 border-t border-[#f3f4f6]">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-3 rounded-xl border border-[#e5e7eb] text-[#6b7280] font-medium hover:bg-[#f9fafb] transition-all"
            >
              انصراف
            </button>
            <button
              type="submit"
              disabled={loading}
              className={`flex-1 py-3 rounded-xl bg-gradient-to-r from-[#0b3f32] to-[#1a6b58] text-[#ffffff] font-bold shadow-md transition-all ${loading
                ? 'opacity-50 cursor-not-allowed'
                : 'hover:from-[#0a3327] hover:to-[#135a4a]'
                }`}
            >
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  در حال ثبت...
                </span>
              ) : (
                'ثبت جلسه'
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PersianCalendarModal;