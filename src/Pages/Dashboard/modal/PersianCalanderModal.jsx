import React, { useState, useEffect, useMemo } from 'react';
import { FaTimes, FaCalendarAlt, FaClock, FaChalkboardTeacher, FaBookOpen, FaChevronRight, FaChevronLeft } from 'react-icons/fa';
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
  });
  const [errors, setErrors] = useState({});
  const [currentMonth, setCurrentMonth] = useState(moment().startOf('jMonth'));
  const [showCalendar, setShowCalendar] = useState(false);
  const [selectedDay, setSelectedDay] = useState(null);
  const [meetingDate, setMeetingDate] = useState('');
  const [meetingTime, setMeetingTime] = useState('');

  // تبدیل لیست اساتید به فرمت react-select
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
      });
      setMeetingDate('');
      setMeetingTime('');
      setSelectedDay(null);
      setErrors({});
    }
  }, [isOpen, room]);

  // روزهای هفته به فارسی
  const weekDays = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];
  const weekDaysFull = ['شنبه', 'یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنجشنبه', 'جمعه'];

  if (room == null) {
    return null;
  }
  const meetingName = room.title;

  // دکمه‌های هفته آینده (از امروز تا 7 روز بعد)
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

  // گرفتن روزهای ماه جاری
  const getMonthDays = () => {
    const startOfMonth = moment(currentMonth).startOf('jMonth');
    const endOfMonth = moment(currentMonth).endOf('jMonth');
    const startDayOfWeek = startOfMonth.day();

    const days = [];
    const prevMonthDays = startDayOfWeek;

    // روزهای ماه قبل
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

    // روزهای ماه جاری
    for (let i = 1; i <= endOfMonth.jDate(); i++) {
      const day = moment(currentMonth).date(i);
      days.push({
        date: day.format('jYYYY/jMM/jDD'),
        day: i,
        month: currentMonth.jMonth(),
        year: currentMonth.jYear(),
        isCurrentMonth: true,
        isToday: day.isSame(moment(), 'day'),
      });
    }

    // روزهای ماه بعد
    const remainingDays = 42 - days.length;
    for (let i = 1; i <= remainingDays; i++) {
      const day = moment(currentMonth).add(1, 'jMonth').date(i);
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
    }
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
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

  // تبدیل تاریخ شمسی و ساعت به DateTime برای دات نت
  const convertToDateTime = (date, time) => {
    if (!date || !time) return null;
    const [year, month, day] = date.split('/');
    const [hour, minute] = time.split(':');
    const jalaliDate = moment(`${year}/${month}/${day} ${hour}:${minute}`, 'jYYYY/jMM/jDD HH:mm');
    return jalaliDate.toDate().toISOString();
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (validateForm()) {
      const payload = {
        friendlyID: room.roomID,
        title: formData.title,
        teacher: formData.teachers.map(t => t.label).join('، '),
        teachers: formData.teachers.map(t => ({
          name: t.label,
          id: t.value,
        })),
        dabir: formData.scientificSecretary ? formData.scientificSecretary.label : null,
        startDate: convertToDateTime(meetingDate, meetingTime)
      };
      console.log(payload);
      NewPlanRoom(payload);
      if (onSubmit) onSubmit(payload);
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-[#000000]/50 backdrop-blur-sm animate-fadeIn">
      <div className="relative w-full max-w-lg bg-[#ffffff] rounded-2xl shadow-2xl overflow-hidden">

        {/* Header */}
        <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-6 py-4 flex justify-between items-center">
          <h3 className="text-[#ffffff] text-lg font-bold">تنظیم جلسه جدید</h3>
          <button
            onClick={onClose}
            className="text-[#ffffff]/70 hover:text-[#ffffff] transition-colors p-1 rounded-full hover:bg-white/10"
          >
            <FaTimes className="text-xl" />
          </button>
        </div>

        {/* Body */}
        <form onSubmit={handleSubmit} className="p-6 max-h-[80vh] overflow-y-auto">

          {/* نام جلسه (غیرقابل ویرایش) */}
          <div className="mb-5 p-3 bg-[#f0f7f4] rounded-xl border border-[#30d4cc]/20">
            <p className="text-xs text-[#6b7280] mb-1">نام جلسه</p>
            <p className="text-[#1f2937] font-semibold">{meetingName}</p>
          </div>

          {/* موضوع جلسه */}
          <div className="mb-4">
            <label className="block text-sm font-medium text-[#374151] mb-1.5">
              موضوع جلسه <span className="text-[#ef4444]">*</span>
            </label>
            <div className="relative">
              <FaBookOpen className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-sm" />
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleChange}
                placeholder="مثال: جلسه تفسیر قرآن - جزء ۳۰"
                className={`w-full pr-10 pl-3 py-2.5 border rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent transition-all bg-[#f9fafb]/50 text-[#1f2937] ${errors.title ? 'border-[#ef4444]' : 'border-[#e5e7eb]'
                  }`}
              />
            </div>
            {errors.title && <p className="text-xs text-[#ef4444] mt-1">{errors.title}</p>}
          </div>

          {/* اساتید (چندتایی) */}
          <div className="mb-4">
            <label className="block text-sm font-medium text-[#374151] mb-1.5">
              اساتید جلسه <span className="text-[#ef4444]">*</span>
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
                  borderColor: errors.teachers ? '#ef4444' : '#e5e7eb',
                }),
              }}
              formatCreateLabel={(inputValue) => `اضافه کردن "${inputValue}"`}
            />
            {errors.teachers && <p className="text-xs text-[#ef4444] mt-1">{errors.teachers}</p>}
          </div>

          {/* دبیر علمی (تک انتخابی) */}
          <div className="mb-4">
            <label className="block text-sm font-medium text-[#374151] mb-1.5">
              دبیر علمی
              <span className="text-xs text-gray-500 mr-2">(اختیاری)</span>
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
          </div>

          {/* انتخاب سریع روزهای هفته */}
          <div className="mb-5">
            <label className="block text-sm font-medium text-[#374151] mb-2">انتخاب سریع تاریخ</label>
            <div className="grid grid-cols-7 gap-1.5">
              {nextWeekDays.map((day, idx) => (
                <button
                  key={idx}
                  type="button"
                  onClick={() => selectQuickDate(day.date)}
                  className={`py-2 rounded-lg text-center transition-all text-xs font-medium ${meetingDate === day.date
                    ? 'bg-[#30d4cc] text-[#0b3f32] shadow-md'
                    : 'bg-[#f3f4f6] text-[#374151] hover:bg-[#30d4cc]/20 hover:text-[#0b3f32]'
                    }`}
                >
                  <div className="font-bold">{day.label}</div>
                  <div className="text-[10px]">{day.day}</div>
                </button>
              ))}
            </div>
          </div>

          {/* انتخاب تاریخ با تقویم */}
          <div className="mb-4 relative">
            <label className="block text-sm font-medium text-[#374151] mb-1.5">
              تاریخ جلسه <span className="text-[#ef4444]">*</span>
            </label>
            <div className="relative">
              <FaCalendarAlt className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-sm" />
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

            {/* تقویم شمسی */}
            {showCalendar && (
              <div className="absolute top-full left-0 right-0 mt-2 bg-[#ffffff] rounded-xl shadow-xl border border-[#e5e7eb] z-10 p-3">
                {/* header تقویم */}
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

                {/* روزهای هفته */}
                <div className="grid grid-cols-7 gap-1 mb-2">
                  {weekDays.map((day, idx) => (
                    <div key={idx} className="text-center text-xs font-bold text-[#9ca3af] py-1">
                      {day}
                    </div>
                  ))}
                </div>

                {/* روزهای ماه */}
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

          {/* انتخاب ساعت */}
          <div className="mb-6">
            <label className="block text-sm font-medium text-[#374151] mb-1.5">
              ساعت جلسه <span className="text-[#ef4444]">*</span>
            </label>
            <div className="relative">
              <FaClock className="absolute right-3 top-1/2 -translate-y-1/2 text-[#9ca3af] text-sm" />
              <select
                name="meetingTime"
                value={meetingTime}
                onChange={handleChange}
                className={`w-full pr-10 pl-3 py-2.5 border rounded-xl focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent bg-[#f9fafb]/50 text-[#1f2937] ${errors.meetingTime ? 'border-[#ef4444]' : 'border-[#e5e7eb]'
                  }`}
              >
                <option value="">ساعت را انتخاب کنید</option>
                {generateTimeOptions().map(time => (
                  <option key={time} value={time}>{time}</option>
                ))}
              </select>
            </div>
            {errors.meetingTime && <p className="text-xs text-[#ef4444] mt-1">{errors.meetingTime}</p>}
          </div>

          {/* دکمه‌های اقدام */}
          <div className="flex gap-3 pt-3 border-t border-[#f3f4f6]">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2.5 rounded-xl border border-[#e5e7eb] text-[#6b7280] font-medium hover:bg-[#f9fafb] transition-all"
            >
              انصراف
            </button>
            <button
              type="submit"
              className="flex-1 py-2.5 rounded-xl bg-gradient-to-r from-[#0b3f32] to-[#1a6b58] text-[#ffffff] font-bold hover:from-[#0a3327] hover:to-[#135a4a] transition-all shadow-md"
            >
              ثبت جلسه
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PersianCalendarModal;