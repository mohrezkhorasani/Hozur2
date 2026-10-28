import React, { useState, useEffect, useRef, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { FaDownload, FaUsers, FaCertificate, FaBackward, FaPlay, FaUpload, FaFileAudio, FaFileVideo, FaExternalLinkAlt, FaChartArea, FaTrash } from 'react-icons/fa';
import { GenerateCertificateWithUserMeetID, GetMembersOfMeet, GetMembersOfMeetOnlyCertificated } from '../../api/GetRooms';
import { useNavigate } from 'react-router-dom';
import { Logs } from 'lucide-react';

const TitleWithTooltip = ({ title }) => {
  const [showTooltip, setShowTooltip] = useState(false);
  const [tooltipPosition, setTooltipPosition] = useState({ top: 0, left: 0 });
  const spanRef = useRef(null);

  if (!title) return <span className="text-gray-400">بدون عنوان</span>;

  const truncated = title.length > 30 ? title.slice(0, 30) + '...' : title;

  const updateTooltipPosition = () => {
    if (spanRef.current) {
      const rect = spanRef.current.getBoundingClientRect();
      setTooltipPosition({
        top: rect.bottom + window.scrollY + 5,
        left: rect.left + window.scrollX + rect.width / 2,
      });
    }
  };

  const handleMouseEnter = () => {
    updateTooltipPosition();
    setShowTooltip(true);
  };

  const handleMouseLeave = () => setShowTooltip(false);

  useEffect(() => {
    if (showTooltip) {
      window.addEventListener('scroll', updateTooltipPosition);
      window.addEventListener('resize', updateTooltipPosition);
      return () => {
        window.removeEventListener('scroll', updateTooltipPosition);
        window.removeEventListener('resize', updateTooltipPosition);
      };
    }
  }, [showTooltip]);

  return (
    <>
      <span
        ref={spanRef}
        onMouseEnter={handleMouseEnter}
        onMouseLeave={handleMouseLeave}
        className="cursor-help"
      >
        {truncated}
      </span>
      {showTooltip && createPortal(
        <div
          className="fixed z-[100] bg-gray-800 text-white text-xs rounded px-2 py-1 whitespace-nowrap shadow-lg"
          style={{ top: tooltipPosition.top, left: tooltipPosition.left, transform: 'translateX(-50%)' }}
        >
          {title}
          <div className="absolute -top-1 left-1/2 transform -translate-x-1/2 w-2 h-2 bg-gray-800 rotate-45"></div>
        </div>,
        document.body
      )}
    </>
  );
};

const DayMeetingsModal = ({ isOpen, onClose, meetings, persianMonth, day, year }) => {
  const [selectedMeeting, setSelectedMeeting] = useState(null);
  const [showAttendeesPanel, setShowAttendeesPanel] = useState(false);
  const [attendees, setAttendees] = useState([]);
  const [loadingAttendees, setLoadingAttendees] = useState(false);
  const [showCertificatesPanel, setShowCertificatesPanel] = useState(false);
  const [certificates, setCertificates] = useState([]);
  const [loadingCertificates, setLoadingCertificates] = useState(false);
  const [loadingGenerateCert, setloadingGenerateCert] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('all');
  const navigate = useNavigate();

  const [certModalOpen, setCertModalOpen] = useState(false);
  const [selectedUserForCert, setSelectedUserForCert] = useState(null);
  const [certNationalCode, setCertNationalCode] = useState('');
  const [certPhone, setCertPhone] = useState('');
  const [certIsMale, setCertIsMale] = useState(true);
  const [certUserName, setCertUserName] = useState('');
  const [generatingCert, setGeneratingCert] = useState(false);
  const [certError, setCertError] = useState('');
  const [certSuccessLink, setCertSuccessLink] = useState('');

  const handleOpenGenerateCertModal = (attendee = null) => {
    setSelectedUserForCert(attendee);
    setCertError('');
    setCertSuccessLink('');
    if (attendee) {
      setCertNationalCode(attendee.nationalCode || '');
      setCertPhone(attendee.phone || '');
      setCertUserName(attendee.name || '');
      setCertIsMale(attendee.gender === 'خانم' ? false : true);
      if (attendee.hasTakenCertificate && attendee.certificateURL) {
        setCertSuccessLink(attendee.certificateURL);
      }
    } else {
      setCertNationalCode('');
      setCertPhone('');
      setCertUserName('');
      setCertIsMale(true);
    }
    setCertModalOpen(true);
  };

  const handleGenerateCertificateSubmit = async (e) => {
    e.preventDefault();
    if (!certNationalCode.trim() && !certPhone.trim()) {
      setCertError('لطفاً حداقل کد ملی یا شماره موبایل را وارد نمایید');
      return;
    }
    if (certNationalCode.trim() && certNationalCode.trim().length !== 10) {
      setCertError('کد ملی باید ۱۰ رقم باشد');
      return;
    }
    if (certPhone.trim() && certPhone.trim().length < 10) {
      setCertError('شماره موبایل معتبر نیست');
      return;
    }

    setGeneratingCert(true);
    setCertError('');
    setCertSuccessLink('');

    try {
      const link = await GenerateCertificateWithUserMeetID(selectedUserForCert?.id || 0, {
        nationalCode: certNationalCode.trim(),
        phone: certPhone.trim(),
        isMale: certIsMale,
        meetID: selectedMeeting?.meetID || '',
        name: certUserName.trim()
      });

      setCertSuccessLink(link);

      // به‌روزرسانی زنده وضعیت کاربر در جدول حضار
      setAttendees(prev => prev.map(item => {
        if ((selectedUserForCert && item.id === selectedUserForCert.id) || 
            (certNationalCode && item.nationalCode === certNationalCode.trim()) ||
            (certPhone && item.phone === certPhone.trim())) {
          return {
            ...item,
            hasTakenCertificate: true,
            nationalCode: certNationalCode.trim() || item.nationalCode,
            phone: certPhone.trim() || item.phone,
            gender: certIsMale ? 'آقا' : 'خانم',
            certificateURL: link
          };
        }
        return item;
      }));

      // به‌روزرسانی لیست گواهی‌ها
      if (selectedMeeting?.meetID) {
        fetchCertificates(selectedMeeting.meetID);
      }
    } catch (err) {
      console.error(err);
      setCertError(err.message || 'خطا در صدور گواهینامه. لطفاً مجدداً تلاش کنید.');
    } finally {
      setGeneratingCert(false);
    }
  };

  const filteredAttendees = useMemo(() => {
    if (!attendees) return [];

    return attendees.filter((a) => {
      // جستجو در نام، شناسه، کد ملی، شماره موبایل و نقش
      const matchesSearch = searchTerm === '' ||
        a.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        a.id?.toString().includes(searchTerm) ||
        a.role?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (a.nationalCode && a.nationalCode.toString().includes(searchTerm)) ||
        (a.phone && a.phone.toString().includes(searchTerm));

      // فیلتر بر اساس نقش
      let matchesRole = true;
      if (roleFilter !== 'all') {
        if (roleFilter === 'VIEWER') {
          matchesRole = a.role === 'VIEWER';
        } else if (roleFilter === 'Teacher') {
          matchesRole = a.role === 'Teacher';
        } else if (roleFilter === 'student') {
          matchesRole = a.role === 'student';
        } else if (roleFilter === 'other') {
          matchesRole = a.role !== 'VIEWER' && a.role !== 'Teacher' && a.role !== 'student';
        }
      }

      return matchesSearch && matchesRole;
    });
  }, [attendees, searchTerm, roleFilter]);
  const resetFilters = () => {
    setSearchTerm('');
    setRoleFilter('all');
  };

  if (!isOpen) return null;

  const fetchAttendees = async (meetId) => {
    setLoadingAttendees(true);
    try {
      console.log(meetId)

      let data = await GetMembersOfMeet(meetId)
      setAttendees(data)
      const groupedData = data.reduce((acc, curr) => {
        const key = curr.name;

        if (!acc[key]) {
          // اولین بار است که این نام را می‌بینیم
          acc[key] = {
            ...curr,
            occurrences: 1,
            totalDuration: curr.duration || 0,
            enterTimes: [curr.enteredAt],
            exitTimes: [curr.leftAt]
          };
        } else {
          // نام تکراری است، اطلاعات را به‌روزرسانی کن
          acc[key].occurrences++;
          acc[key].totalDuration = (acc[key].totalDuration || 0) + (curr.duration || 0);
          if (curr.enteredAt) acc[key].enterTimes.push(curr.enteredAt);
          if (curr.leftAt) acc[key].exitTimes.push(curr.leftAt);

          // به‌روزرسانی زمان ورود و خروج (آخرین یا اولین)
          acc[key].enteredAt = acc[key].enterTimes[0]; // اولین ورود
          acc[key].leftAt = acc[key].exitTimes[acc[key].exitTimes.length - 1]; // آخرین خروج

          // میانگین یا مجموع مدت زمان
          acc[key].duration = acc[key].totalDuration;
        }

        return acc;
      }, {});

      const uniqueAttendees = Object.values(groupedData);
      setAttendees(uniqueAttendees);

      console.log(data)
    } finally {
      setLoadingAttendees(false);
    }
  };

  const fetchCertificates = async (meetId) => {
    setLoadingCertificates(true);
    try {
      var r = await GetMembersOfMeetOnlyCertificated(meetId)
      // await new Promise(r => setTimeout(r, 500));

      setCertificates(r);
    } finally {
      setLoadingCertificates(false);
    }
  };

  const handleShowAttendees = async (meeting) => {
    setSelectedMeeting(meeting);
    setShowAttendeesPanel(true);
    await fetchCertificates(meeting.meetID);
    await fetchAttendees(meeting.meetID);
  };

  const handleShowCertificates = async (meeting) => {
    setSelectedMeeting(meeting);
    setShowCertificatesPanel(true);
    await fetchCertificates(meeting.meetID);
  };

  const handleExportExcel = (type) => {
    console.log(`Export ${type} as excel`);
  };

  const handleBack = () => {
    setShowAttendeesPanel(false);
    setShowCertificatesPanel(false);
    setSelectedMeeting(null);
  };

  const handleProfileClick = (userId) => {
    console.log('Go to profile', userId);
  };

  const getTeachersAndDabir = (meet) => {
    const teachers = meet.teacher?.filter(t => t.role !== 'DABIR') || [];
    const dabir = meet.teacher?.find(t => t.role === 'DABIR');
    return { teachers, dabir };
  };

  return (
    <div className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl w-full max-w-6xl max-h-[90vh] flex flex-col shadow-2xl relative">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b shrink-0">
          <h3 className="text-lg font-bold">
            جلسات روز {day}-{persianMonth}-{year}
          </h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-2xl">&times;</button>
        </div>

        {/* محتوای اصلی با اسکرول جداگانه */}
        <div className="flex-1 overflow-y-auto">
          {!showAttendeesPanel && !showCertificatesPanel && (
            <div className="p-4 space-y-4">
              {meetings.map((meet, idx) => {
                const { teachers, dabir } = getTeachersAndDabir(meet);
                return (
                  <div key={idx} className="border rounded-xl p-4 hover:shadow-md transition">
                    <div className="grid grid-cols-12 gap-3 items-start">
                      <div className="col-span-3">
                        <div className="font-bold text-gray-800">
                          <TitleWithTooltip title={meet.title} />
                        </div>
                        <div className="text-xs text-gray-500 mt-1">⏰ {meet.startedTime}</div>
                      </div>
                      <div className="col-span-3">
                        {teachers.length > 0 && (
                          <div className="text-sm">
                            <span className="font-semibold">اساتید: </span>
                            {teachers.map((t, i) => (
                              <button
                                key={i}
                                onClick={() => handleProfileClick(t.id)}
                                className="text-blue-600 hover:underline ml-1"
                              >
                                {t.username || t.fullname}
                              </button>
                            ))}
                          </div>
                        )}
                        {dabir && (
                          <div className="text-sm mt-1">
                            <span className="font-semibold">دبیر علمی: </span>
                            <button
                              onClick={() => handleProfileClick(dabir.id)}
                              className="text-blue-600 hover:underline"
                            >
                              {dabir.username || dabir.fullname}
                            </button>
                          </div>
                        )}
                      </div>
                      <div className="col-span-2">
                        {meet.soundFile ? (
                          <audio controls className="w-full h-8">
                            <source src={meet.soundFile} />
                          </audio>
                        ) : (
                          <button className="text-xs bg-gray-100 px-2 py-1 rounded flex items-center gap-1 hover:bg-gray-200">
                            <FaUpload size={10} /> بارگذاری صوت
                          </button>
                        )}
                      </div>
                      <div className="col-span-2">
                        {meet.videoFile ? (
                          <a href={meet.videoFile} target="_blank" rel="noopener noreferrer" className="text-blue-500 text-xs flex items-center gap-1">
                            <FaFileVideo /> مشاهده ویدئو <FaExternalLinkAlt size={10} />
                          </a>
                        ) : (
                          <button className="text-xs bg-gray-100 px-2 py-1 rounded flex items-center gap-1 hover:bg-gray-200">
                            <FaUpload size={10} /> بارگذاری ویدئو
                          </button>
                        )}
                      </div>
                      <div className="col-span-2 flex flex-col gap-2">
                        <button
                          onClick={() => handleShowAttendees(meet)}
                          className="text-xs bg-[#30d4cc]/10 text-[#30d4cc] px-2 py-1 rounded flex items-center gap-1 justify-center hover:bg-[#30d4cc]/20"
                        >
                          <FaUsers size={12} /> {meet.countPartis || 0} حضار
                        </button>
                        <button
                          onClick={() => handleShowCertificates(meet)}
                          className="text-xs bg-amber-50 text-amber-600 px-2 py-1 rounded flex items-center gap-1 justify-center hover:bg-amber-100"
                        >
                          <FaCertificate size={12} /> گواهی‌ها ({certificates.filter(e => e.parti.hasCert === true).length} نفر )
                        </button>
                        <button
                          onClick={() =>
                            navigate(`/polls/${meet.meetID}/report`)
                          }
                          className="text-xs bg-[#cdffbe] text-[rgb(0,82,24)] px-2 py-1 rounded flex items-center gap-1 justify-center hover:bg-amber-100"
                        >
                          <FaChartArea size={12} /> گزارش نظرخواهی
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
              {meetings.length === 0 && (
                <div className="text-center text-gray-500 py-10">هیچ جلسه‌ای در این روز برگزار نشده است.</div>
              )}
            </div>
          )}

          {(showAttendeesPanel || showCertificatesPanel) && (
            <div className="flex flex-col h-full">
              <div className="sticky top-0 bg-gray-50 p-4 border-b z-10 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <button onClick={handleBack} className="p-1 rounded-full hover:bg-gray-200">
                    <FaBackward />
                  </button>
                  <h4 className="font-semibold">
                    {showAttendeesPanel ? 'لیست حضار' : 'گواهی‌های صادر شده'} - {selectedMeeting?.title || 'جلسه'}
                  </h4>
                </div>
                <div className="flex items-center gap-2">
                  {showAttendeesPanel && (
                    <button
                      onClick={() => handleOpenGenerateCertModal(null)}
                      className="text-xs bg-emerald-600 hover:bg-emerald-700 text-white px-3 py-1.5 rounded-lg flex items-center gap-1.5 shadow-sm transition"
                    >
                      <FaCertificate size={12} /> صدور گواهینامه با کد ملی و موبایل
                    </button>
                  )}
                  <button
                    onClick={() => handleExportExcel(showAttendeesPanel ? 'attendees' : 'certificates')}
                    className="text-xs bg-green-600 text-white px-2 py-1 rounded flex items-center gap-1"
                  >
                    <FaDownload /> خروجی اکسل
                  </button>
                </div>
              </div>

              <div className="p-4">
                {loadingAttendees || loadingCertificates ? (
                  <div className="flex justify-center py-10">در حال بارگذاری...</div>
                ) : (
                  showAttendeesPanel ? (
                    <div className="w-full">
                      {/* بخش سرچ و فیلتر */}
                      <div className="mb-4 p-4 bg-gray-50 rounded-lg">
                        <div className="flex flex-col md:flex-row gap-3">
                          {/* باکس جستجو */}
                          <div className="flex-1">
                            <div className="relative">
                              <input
                                type="text"
                                placeholder="جستجو بر اساس نام، شناسه، کد ملی، شماره موبایل یا نقش..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="w-full p-2 pr-8 border border-gray-300 rounded-lg focus:outline-none focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc]"
                              />
                              <svg
                                className="absolute right-2 top-2.5 w-4 h-4 text-gray-400"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                              >
                                <path
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                  strokeWidth={2}
                                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                                />
                              </svg>
                              {searchTerm && (
                                <button
                                  onClick={() => setSearchTerm('')}
                                  className="absolute left-2 top-2 text-gray-400 hover:text-gray-600"
                                >
                                  ✕
                                </button>
                              )}
                            </div>
                          </div>

                          {/* فیلتر نقش */}
                          <div className="flex gap-2">
                            <select
                              value={roleFilter}
                              onChange={(e) => setRoleFilter(e.target.value)}
                              className="p-2 border border-gray-300 rounded-lg focus:outline-none focus:border-[#30d4cc]"
                            >
                              <option value="all">همه نقش‌ها</option>
                              <option value="VIEWER">شرکت‌کننده</option>
                              <option value="Teacher">استاد</option>
                              <option value="student">دانشجو</option>
                              <option value="other">سایر</option>
                            </select>

                            <button
                              onClick={resetFilters}
                              className="px-3 py-2 text-sm text-gray-600 hover:text-gray-800 border border-gray-300 rounded-lg hover:bg-gray-100 transition-colors"
                            >
                              حذف فیلترها
                            </button>
                          </div>
                        </div>

                        {/* نمایش تعداد نتایج */}
                        <div className="mt-2 text-xs text-gray-500">
                          {filteredAttendees.length} از {attendees?.length || 0} نفر
                        </div>
                      </div>

                      {/* جدول */}
                      <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                          <thead className="border-b bg-gray-50">
                            <tr>
                              <th className="text-right p-2">نام</th>
                              <th className="text-right p-2">شناسه</th>
                              <th className="text-right p-2">نقش</th>
                              <th className="text-right p-2">ورود</th>
                              <th className="text-right p-2">خروج</th>
                              <th className="text-right p-2">مدت (دقیقه)</th>
                              <th className="text-right p-2">عملیات</th>
                            </tr>
                          </thead>
                          <tbody>
                            {filteredAttendees.length > 0 ? (
                              filteredAttendees.map((a, i) => {
                                let mtimer = certificates[0]?.meet?.timer || "0 ساعت";
                                let cert = certificates.find(e => e.parti.id === a.id);
                                let ptimer = mtimer;

                                if (a.role === "VIEWER") {
                                  ptimer = "15 دقیقه";
                                }
                                if (cert) {
                                  ptimer = cert.parti.timer;
                                }

                                // تابع تبدیل زمان به ثانیه
                                const timeToSeconds = (timeStr) => {
                                  if (!timeStr || timeStr === 'نامشخص') return 0;
                                  let str = timeStr;
                                  str = str.replace(/دو/g, "2").replace(/سه/g, "3").replace(/یک/g, "1").replace(/چهار/g, "4")
                                    .replace(/پنج/g, "5").replace(/شش/g, "6").replace(/هفت/g, "7").replace(/هشت/g, "8")
                                    .replace(/نه/g, "9").replace(/ده/g, "10")
                                    .replace(/[۰-۹]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 1728))
                                    .replace(/[٠-٩]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 1632));
                                  let hours = 0;
                                  let minutes = 0;
                                  const fullMatch = str.match(/(\d+)\s*ساعت(?:\s*و\s*)?(\d+)\s*دقیقه/);
                                  if (fullMatch) {
                                    hours = parseInt(fullMatch[1]);
                                    minutes = parseInt(fullMatch[2]);
                                  } else {
                                    const hourOnly = str.match(/(\d+)\s*ساعت(?!\s*و)/);
                                    if (hourOnly) {
                                      hours = parseInt(hourOnly[1]);
                                      minutes = 0;
                                    }
                                    const minuteOnly = str.match(/(\d+)\s*دقیقه/);
                                    if (minuteOnly && !fullMatch) {
                                      hours = 0;
                                      minutes = parseInt(minuteOnly[1]);
                                    }
                                  }
                                  if (hours === 0 && minutes === 0) {
                                    const simpleNumber = str.match(/(\d+)/);
                                    if (simpleNumber) {
                                      minutes = parseInt(simpleNumber[1]);
                                    }
                                  }
                                  return (hours * 3600) + (minutes * 60);
                                };

                                let msec = timeToSeconds(mtimer);
                                let psec = timeToSeconds(ptimer);
                                let percentage = 0;
                                if (msec > 0) {
                                  percentage = Math.ceil((psec * 100) / msec);
                                  percentage = Math.min(percentage, 100);
                                }

                                const getAttendanceStatus = (percentage) => {
                                  if (percentage >= 66) {
                                    return { color: 'text-green-600', bg: 'bg-green-100', text: 'تایید شده', icon: '✅' };
                                  } else if (percentage >= 30) {
                                    return { color: 'text-orange-600', bg: 'bg-orange-100', text: 'نیاز به بررسی', icon: '⚠️' };
                                  } else {
                                    return { color: 'text-red-600', bg: 'bg-red-100', text: 'تایید نشده', icon: '❌' };
                                  }
                                };

                                const status = getAttendanceStatus(percentage);

                                return (
                                  <tr key={a.id || i} className="border-b hover:bg-gray-50 transition-colors">
                                    <td className="p-2">
                                      <div className="flex items-center gap-2 flex-wrap">
                                        <span className="font-medium">{a.name}</span>

                                        {/* وضعیت حضور */}
                                        <span className={`text-xs px-1.5 py-0.5 rounded-full ${status.bg} ${status.color}`}>
                                          {status.icon} {status.text}
                                        </span>

                                        {/* گواهی دریافت شده - طراحی خاص طلایی */}
                                        {a.hasTakenCertificate && (
                                          <div className="relative group">
                                            <div className="flex items-center gap-2 px-2 py-0.5 rounded-full bg-gradient-to-r from-amber-400 via-yellow-400 to-amber-500 text-white shadow-lg">
                                              <div className="relative">
                                                <svg className="w-4 h-4 animate-pulse" fill="currentColor" viewBox="0 0 24 24">
                                                  <path d="M12 2L15 8.5L22 9.5L17 14L18.5 21L12 17.5L5.5 21L7 14L2 9.5L9 8.5L12 2Z" />
                                                </svg>
                                              </div>
                                              <span className="text-xs font-bold">گواهینامه حضور</span>
                                              <div className="w-4 h-4 rounded-full bg-yellow-300 flex items-center justify-center">
                                                <svg className="w-2.5 h-2.5 text-amber-600" fill="currentColor" viewBox="0 0 24 24">
                                                  <path d="M5 13l4 4L19 7" stroke="currentColor" strokeWidth="2" fill="none" />
                                                </svg>
                                              </div>
                                            </div>
                                            {/* Tooltip */}
                                            <div className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-1 px-2 py-1 bg-gray-800 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap">
                                              این کاربر گواهی طلایی دریافت کرده است
                                            </div>
                                          </div>
                                        )}

                                        {/* واجد دریافت گواهی */}
                                        {!a.hasTakenCertificate && a.canTakeCertificate && (
                                          <div className="relative group">
                                            <div className="flex items-center gap-1 px-2 py-0.5 rounded-full bg-blue-50 border border-blue-300 text-blue-700">
                                              <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                              </svg>
                                              <span className="text-xs">واجد دریافت گواهی</span>
                                            </div>
                                          </div>
                                        )}

                                        {/* عدم احراز شرایط */}
                                        {!a.hasTakenCertificate && !a.canTakeCertificate && (
                                          <div className="flex items-center gap-1 px-2 py-0.5 rounded-full bg-gray-50 border border-gray-200 text-gray-500">
                                            <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                                d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                            </svg>
                                            <span className="text-xs">عدم احراز شرایط گواهی</span>
                                          </div>
                                        )}
                                      </div>
                                    </td>
                                    <td className="p-2 text-xs text-gray-500">{a.id}</td>
                                    <td className="p-2">
                                      {a.role === 'VIEWER' ? 'شرکت‌کننده' :
                                        a.role === "Teacher" ? 'استاد' :
                                          a.role === "student" ? 'دانشجو' : a.role}
                                    </td>
                                    <td className="p-2">{a.enteredAt ? a.enteredAt : "-"}</td>
                                    <td className="p-2">{a.leftAt ? a.leftAt : "-"}</td>
                                    <td className="p-2">
                                      <div className="flex items-center gap-3">
                                        <div className="relative w-12 h-12">
                                          <svg className="w-12 h-12 transform -rotate-90">
                                            <circle
                                              cx="24"
                                              cy="24"
                                              r="20"
                                              stroke="#e5e7eb"
                                              strokeWidth="3"
                                              fill="none"
                                            />
                                            <circle
                                              cx="24"
                                              cy="24"
                                              r="20"
                                              stroke={percentage >= 66 ? '#22c55e' : percentage >= 30 ? '#eab308' : '#ef4444'}
                                              strokeWidth="3"
                                              fill="none"
                                              strokeDasharray={`${2 * Math.PI * 20}`}
                                              strokeDashoffset={`${2 * Math.PI * 20 * (1 - percentage / 100)}`}
                                              className="transition-all duration-500"
                                            />
                                          </svg>
                                          <span className="absolute inset-0 flex items-center justify-center text-xs font-bold">
                                            {percentage}%
                                          </span>
                                        </div>
                                        <div>
                                          <div className="text-sm">{ptimer}</div>
                                          <div className="text-xs text-gray-500">
                                            {100 - percentage}% باقی مونده
                                          </div>
                                        </div>
                                      </div>
                                    </td>
                                    <td className="p-2">
                                      <div className="flex items-center gap-1.5 flex-wrap">
                                        <button
                                          onClick={() => handleOpenGenerateCertModal(a)}
                                          className={`text-xs px-2.5 py-1.5 rounded-lg flex items-center gap-1 transition ${
                                            a.hasTakenCertificate
                                              ? 'bg-amber-50 text-amber-700 hover:bg-amber-100 border border-amber-300'
                                              : 'bg-emerald-50 text-emerald-700 hover:bg-emerald-100 border border-emerald-300'
                                          }`}
                                          title={a.hasTakenCertificate ? 'مدیریت / صدور مجدد گواهینامه' : 'صدور گواهینامه با کد ملی و موبایل'}
                                        >
                                          <FaCertificate className={a.hasTakenCertificate ? 'text-amber-600' : 'text-emerald-600'} size={12} />
                                          <span>{a.hasTakenCertificate ? 'مدیریت گواهی' : 'صدور گواهی'}</span>
                                        </button>

                                        <a href={`/usermeet/logs/${a.id}`}>
                                          <button
                                            className="text-[#474747] hover:text-[#000000] transition 
                                            flex items-center gap-1.5 hover:bg-[#afafaf] px-2.5 py-1.5 rounded-lg text-xs"
                                          >
                                            <Logs className="text-xs" size={13} /> لاگ ها ({a.logCount || 0})
                                          </button>
                                        </a>

                                        <button
                                          onClick={() => {}}
                                          className="text-red-600 hover:text-red-800 transition
                                           flex items-center gap-1.5 hover:bg-red-100 px-2 py-1.5 rounded-lg text-xs"
                                        >
                                          <FaTrash size={12} />
                                        </button>
                                      </div>
                                    </td>
                                  </tr>
                                );
                              })
                            ) : ( 
                              <tr>
                                <td colSpan="6" className="p-8 text-center text-gray-500">
                                  <div className="flex flex-col items-center gap-2">
                                    <svg className="w-12 h-12 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <span>نتیجه‌ای یافت نشد</span>
                                    <button onClick={resetFilters} className="text-sm text-[#30d4cc] hover:underline">
                                      حذف فیلترها
                                    </button>
                                  </div>
                                </td>
                              </tr>
                            )}
                          </tbody>
                        </table>
                      </div>
                    </div>

                  ) : (
                    <table className="w-full text-sm">
                      <thead className="border-b">
                        <tr>
                          <th className="text-right p-2">نام</th>
                          <th className="text-right p-2">شناسه گواهینامه</th>
                          <th className="text-right p-2">عنوان</th>
                          <th className="text-right p-2">حضور</th>
                          <th className="text-right p-2">عملیات</th>
                        </tr>
                      </thead>
                      <tbody>
                        {certificates.filter(c => c.parti.hasCert === true).map((c, i) => {
                          let mtimer = c.meet.timer
                          let ptimer = c.parti.timer

                          // تابع تبدیل زمان به ثانیه
                          const timeToSeconds = (timeStr) => {
                            if (!timeStr || timeStr === 'نامشخص') return 0;

                            let str = timeStr;

                            // تبدیل اعداد نوشتاری و فارسی
                            str = str.replace(/دو/g, "2").replace(/سه/g, "3").replace(/یک/g, "1").replace(/چهار/g, "4")
                              .replace(/پنج/g, "5").replace(/شش/g, "6").replace(/هفت/g, "7").replace(/هشت/g, "8")
                              .replace(/نه/g, "9").replace(/ده/g, "10")
                              .replace(/[۰-۹]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 1728))
                              .replace(/[٠-٩]/g, (d) => String.fromCharCode(d.charCodeAt(0) - 1632));

                            let hours = 0;
                            let minutes = 0;

                            // الگوی کامل: عدد ساعت و عدد دقیقه
                            const fullMatch = str.match(/(\d+)\s*ساعت(?:\s*و\s*)?(\d+)\s*دقیقه/);
                            if (fullMatch) {
                              hours = parseInt(fullMatch[1]);
                              minutes = parseInt(fullMatch[2]);
                            } else {
                              // فقط ساعت
                              const hourOnly = str.match(/(\d+)\s*ساعت(?!\s*و)/);
                              if (hourOnly) {
                                hours = parseInt(hourOnly[1]);
                                minutes = 0;
                              }
                              // فقط دقیقه
                              const minuteOnly = str.match(/(\d+)\s*دقیقه/);
                              if (minuteOnly && !fullMatch) {
                                hours = 0;
                                minutes = parseInt(minuteOnly[1]);
                              }
                            }

                            // اگر هیچ الگویی پیدا نشد، عدد ساده را به عنوان دقیقه در نظر بگیر
                            if (hours === 0 && minutes === 0) {
                              const simpleNumber = str.match(/(\d+)/);
                              if (simpleNumber) {
                                minutes = parseInt(simpleNumber[1]);
                              }
                            }

                            return (hours * 3600) + (minutes * 60);
                          }
                          c.load = false
                          let msec = timeToSeconds(mtimer);
                          let psec = timeToSeconds(ptimer);
                          let percentage = 0;
                          if (msec > 0) {
                            percentage = Math.ceil((psec * 100) / msec);
                            percentage = Math.min(percentage, 100);
                          }
                          const getAttendanceStatus = (percentage) => {
                            if (percentage >= 66) {
                              return {
                                color: 'text-green-600',
                                bg: 'bg-green-100',
                                border: 'border-green-500',
                                text: 'تایید شده',
                                icon: '✅',
                                gradient: '#86efac'
                              };
                            } else if (percentage >= 30) {
                              return {
                                color: 'text-orange-600',
                                bg: 'bg-orange-100',
                                border: 'border-orange-500',
                                text: 'نیاز به بررسی',
                                icon: '⚠️',
                                gradient: '#fdba74'
                              };
                            } else {
                              return {
                                color: 'text-red-600',
                                bg: 'bg-red-100',
                                border: 'border-red-500',
                                text: 'تایید نشده',
                                icon: '❌',
                                gradient: '#fca5a5'
                              };
                            }
                          };
                          const status = getAttendanceStatus(percentage);

                          const getRowColor = (index, percentage) => {
                            const colors = [
                              '#9be9a8', // سبز روشن
                              '#40c463', // سبز متوسط
                              '#30d4cc', // فیروزه‌ای
                              '#a8e6cf', // سبز کمرنگ
                              '#7ec8e3'  // آبی روشن
                            ];

                            return `linear-gradient(to left, ${colors[index % colors.length]} ${percentage}%, transparent ${percentage}%)`;
                          };
                          return (
                            <tr key={i} className="border-b">
                              <td className="p-2">{c.parti.name}</td>
                              <td className="p-2">{c.parti.id}</td>
                              <td className="p-2">{c.parti.type}</td>
                              <td className="p-2">
                                <div className="flex flex-col gap-2">
                                  {/* زمان و وضعیت */}
                                  <div className="flex justify-between items-center">
                                    <span className="text-sm font-medium">{c.parti.timer}</span>
                                    <div className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-bold ${status.bg} ${status.color}`}>
                                      <span>{status.icon}</span>
                                      <span>{status.text}</span>
                                    </div>
                                  </div>

                                  {/* نوار پیشرفت */}
                                  <div className="relative">
                                    <div className="flex justify-between text-xs mb-1">
                                      <span>درصد حضور</span>
                                      <span className={status.color}>{percentage}%</span>
                                    </div>
                                    <div className="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                                      <div
                                        className="h-full rounded-full transition-all duration-500"
                                        style={{
                                          width: `${percentage}%`,
                                          background: percentage >= 66 ? '#22c55e' : percentage >= 30 ? '#f97316' : '#ef4444'
                                        }}
                                      />
                                    </div>
                                  </div>

                                  {/* توضیح وضعیت */}
                                  <div className="text-xs text-gray-500">
                                    {percentage >= 66 ? '✓ حضور تایید شد' :
                                      percentage >= 30 ? '⚠️ حضور نیاز به تایید مدیر دارد' :
                                        '✗ حضور تایید نشد'}
                                  </div>
                                </div>
                              </td>
                              <td className="p-2">
                                {loadingGenerateCert ? (
                                  <div>در حال بارگیری.... (ممکن است زمان ببرد)</div>
                                ) : (
                                  <a onClick={async () => {
                                    setloadingGenerateCert(true)
                                    var l = await GenerateCertificateWithUserMeetID(c.parti.id)
                                    window.open(l, '_blank');
                                    setloadingGenerateCert(false)
                                  }} target="_blank" className="text-blue-500 text-xs cursor-pointer">مشاهده</a>
                                )}

                              </td>
                            </tr>
                          )
                        })}
                      </tbody>
                    </table>
                  )
                )}
              </div>
            </div>
          )}
        </div>

        {/* فوتر */}
        <div className="p-3 border-t shrink-0 flex justify-end">
          <button onClick={onClose} className="px-4 py-2 bg-gray-200 rounded-lg">بستن</button>
        </div>
      </div>

      {/* مودال صدور گواهینامه برای شرکت‌کننده */}
      {certModalOpen && createPortal(
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[9999] flex items-center justify-center p-4" dir="rtl">
          <div className="bg-white rounded-2xl w-full max-w-lg shadow-2xl overflow-hidden border border-gray-100 animate-fadeIn">
            {/* سربرگ */}
            <div className="bg-gradient-to-r from-emerald-600 to-teal-700 p-4 text-white flex justify-between items-center">
              <div className="flex items-center gap-2.5">
                <div className="p-2 bg-white/20 rounded-lg">
                  <FaCertificate className="text-amber-300 text-lg" />
                </div>
                <div>
                  <h4 className="font-bold text-base">صدور گواهینامه حضور در جلسه</h4>
                  <p className="text-xs text-emerald-100 mt-0.5">
                    {selectedUserForCert ? `برای: ${selectedUserForCert.name}` : (selectedMeeting?.title ? `جلسه: ${selectedMeeting.title}` : 'صدور گواهینامه دستی')}
                  </p>
                </div>
              </div>
              <button
                onClick={() => { setCertModalOpen(false); setCertError(''); setCertSuccessLink(''); }}
                className="text-white/80 hover:text-white text-2xl p-1 leading-none transition"
              >
                &times;
              </button>
            </div>

            {/* فرم اطلاعات */}
            <form onSubmit={handleGenerateCertificateSubmit} className="p-5 space-y-4">
              {selectedUserForCert && (
                <div className="p-3 bg-gray-50 rounded-xl border border-gray-200 text-xs grid grid-cols-2 gap-2">
                  <div><span className="text-gray-500">نام شرکت‌کننده: </span><span className="font-semibold text-gray-800">{selectedUserForCert.name}</span></div>
                  <div><span className="text-gray-500">شناسه: </span><span className="font-mono text-gray-800">{selectedUserForCert.id}</span></div>
                  <div><span className="text-gray-500">نقش: </span><span className="text-gray-800">{selectedUserForCert.role === 'VIEWER' ? 'شرکت‌کننده' : selectedUserForCert.role}</span></div>
                  <div><span className="text-gray-500">مدت حضور: </span><span className="text-gray-800">{selectedUserForCert.timer || '-'}</span></div>
                </div>
              )}

              {!selectedUserForCert && (
                <div>
                  <label className="block text-xs font-semibold text-gray-700 mb-1">
                    نام و نام خانوادگی شرکت‌کننده <span className="text-gray-400 font-normal">(اختیاری)</span>
                  </label>
                  <input
                    type="text"
                    value={certUserName}
                    onChange={(e) => setCertUserName(e.target.value)}
                    placeholder="مثلاً: علی حسینی"
                    className="w-full p-2.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none"
                  />
                </div>
              )}

              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  کد ملی ده رقمی <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={certNationalCode}
                  onChange={(e) => setCertNationalCode(e.target.value)}
                  placeholder="مثلاً: 0012345678"
                  maxLength={10}
                  dir="ltr"
                  className="w-full p-2.5 text-sm font-mono tracking-wider border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none"
                />
                <p className="text-[11px] text-gray-500 mt-1">کد ملی مستقیماً بر روی سند رسمی گواهینامه چاپ می‌شود.</p>
              </div>

              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1">
                  شماره تلفن همراه <span className="text-gray-400 font-normal">(اختیاری جهت تطبیق و پیامک)</span>
                </label>
                <input
                  type="text"
                  value={certPhone}
                  onChange={(e) => setCertPhone(e.target.value)}
                  placeholder="مثلاً: 09123456789"
                  maxLength={11}
                  dir="ltr"
                  className="w-full p-2.5 text-sm font-mono tracking-wider border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-2">عنوان و جنسیت (جهت درج در گواهی)</label>
                <div className="flex gap-4">
                  <label className={`flex-1 flex items-center justify-center gap-2 p-2.5 border rounded-lg cursor-pointer text-xs transition ${
                    certIsMale ? 'bg-emerald-50 border-emerald-500 text-emerald-800 font-bold shadow-sm' : 'bg-gray-50 border-gray-200 text-gray-600'
                  }`}>
                    <input
                      type="radio"
                      name="certGender"
                      checked={certIsMale === true}
                      onChange={() => setCertIsMale(true)}
                      className="hidden"
                    />
                    <span>جناب آقای (مرد)</span>
                  </label>
                  <label className={`flex-1 flex items-center justify-center gap-2 p-2.5 border rounded-lg cursor-pointer text-xs transition ${
                    !certIsMale ? 'bg-emerald-50 border-emerald-500 text-emerald-800 font-bold shadow-sm' : 'bg-gray-50 border-gray-200 text-gray-600'
                  }`}>
                    <input
                      type="radio"
                      name="certGender"
                      checked={certIsMale === false}
                      onChange={() => setCertIsMale(false)}
                      className="hidden"
                    />
                    <span>سرکار خانم (زن)</span>
                  </label>
                </div>
              </div>

              {/* هشدار در صورت وجود گواهی قبلی */}
              {selectedUserForCert?.hasTakenCertificate && !certSuccessLink && (
                <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-800 text-xs flex items-center gap-2">
                  <span className="text-base">⚠️</span>
                  <span>این شرکت‌کننده قبلاً گواهینامه دریافت کرده است. صدور مجدد، فایل قبلی را بازتولید می‌کند.</span>
                </div>
              )}

              {/* نمایش خطای احتمالی */}
              {certError && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-xs flex items-center gap-2">
                  <span className="text-base">❌</span>
                  <span>{certError}</span>
                </div>
              )}

              {/* نمایش موفقیت و لینک دانلود */}
              {certSuccessLink && (
                <div className="p-3.5 bg-emerald-50 border border-emerald-300 text-emerald-800 rounded-xl space-y-2.5 animate-fadeIn">
                  <div className="flex items-center gap-2 font-bold text-xs">
                    <span className="text-base">🎉</span>
                    <span>گواهینامه با موفقیت صادر شد!</span>
                  </div>
                  <div className="flex gap-2">
                    <a
                      href={certSuccessLink}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white text-xs py-2 px-3 rounded-lg text-center font-semibold transition flex items-center justify-center gap-1.5 shadow-sm"
                    >
                      <FaExternalLinkAlt size={11} />
                      <span>مشاهده و دانلود فایل PDF</span>
                    </a>
                    <button
                      type="button"
                      onClick={() => {
                        navigator.clipboard?.writeText(certSuccessLink);
                        alert('لینک گواهینامه در حافظه کپی شد');
                      }}
                      className="bg-white border border-emerald-300 hover:bg-emerald-100 text-emerald-700 text-xs py-2 px-3 rounded-lg font-medium transition"
                    >
                      کپی لینک
                    </button>
                  </div>
                </div>
              )}

              {/* دکمه‌های اقدام */}
              <div className="pt-3 flex justify-end gap-2 border-t">
                <button
                  type="button"
                  onClick={() => { setCertModalOpen(false); setCertError(''); setCertSuccessLink(''); }}
                  className="px-4 py-2 text-xs text-gray-600 hover:bg-gray-100 rounded-lg transition font-medium"
                >
                  بستن
                </button>
                <button
                  type="submit"
                  disabled={generatingCert}
                  className="px-4 py-2 text-xs bg-emerald-600 hover:bg-emerald-700 text-white font-semibold rounded-lg shadow-sm transition disabled:opacity-50 flex items-center gap-1.5"
                >
                  {generatingCert ? (
                    <>
                      <div className="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                      <span>در حال صدور...</span>
                    </>
                  ) : (
                    <>
                      <FaCertificate size={12} />
                      <span>{selectedUserForCert?.hasTakenCertificate ? 'صدور مجدد گواهینامه' : 'صدور و دریافت گواهینامه'}</span>
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>,
        document.body
      )}
    </div >
  );
};

export default DayMeetingsModal;