import React, { useState, useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import { FaDownload, FaUsers, FaCertificate, FaBackward, FaPlay, FaUpload, FaFileAudio, FaFileVideo, FaExternalLinkAlt } from 'react-icons/fa';
import { GetMembersOfMeet } from '../../api/GetRooms';

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

  if (!isOpen) return null;

  const fetchAttendees = async (meetId) => {
    setLoadingAttendees(true);
    try {
        console.log(meetId)
      
        let data =await GetMembersOfMeet(meetId)
        setAttendees(data)       
        console.log(data) 
    } finally {
      setLoadingAttendees(false);
    }
  };

  const fetchCertificates = async (meetId) => {
    setLoadingCertificates(true);
    try {
      await new Promise(r => setTimeout(r, 500));
      setCertificates([
        { fullname: 'علی رضایی', issuedAt: '1404/12/01', fileUrl: '/cert/1.pdf' },
        { fullname: 'مریم حسینی', issuedAt: '1404/12/01', fileUrl: '/cert/2.pdf' },
      ]);
    } finally {
      setLoadingCertificates(false);
    }
  };

  const handleShowAttendees = async (meeting) => {
    setSelectedMeeting(meeting);
    setShowAttendeesPanel(true);
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
                          <FaCertificate size={12} /> گواهی‌ها
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
                <button
                  onClick={() => handleExportExcel(showAttendeesPanel ? 'attendees' : 'certificates')}
                  className="text-xs bg-green-600 text-white px-2 py-1 rounded flex items-center gap-1"
                >
                  <FaDownload /> خروجی اکسل
                </button>
              </div>

              <div className="p-4">
                {loadingAttendees || loadingCertificates ? (
                  <div className="flex justify-center py-10">در حال بارگذاری...</div>
                ) : (
                  showAttendeesPanel ? (
                    <table className="w-full text-sm">
                      <thead className="border-b">
                        <tr>
                          <th className="text-right p-2">نام</th>
                          <th className="text-right p-2">نقش</th>
                          <th className="text-right p-2">ورود</th>
                          <th className="text-right p-2">خروج</th>
                          <th className="text-right p-2">مدت (دقیقه)</th>
                        </tr>
                      </thead>
                      <tbody>
                        {attendees!=null && attendees.map((a, i) => (
                          <tr key={i} className="border-b">
                            <td className="p-2">{a.name}</td>
                            <td className="p-2">{a.role === 'VIEWER' ? 'شرکت‌کننده' :a.role!=="Teacher"?a.role : 'استاد'}</td>
                            <td className="p-2">{a.enteredAt?a.enteredAt:"-"}</td>
                            <td className="p-2">{a.leftAt?a.leftAt:"-"}</td>
                            <td className={`p-2 ${a.timer==" ثانیه."?"text-[#d83232]":""}`}>{a.timer==" ثانیه."?"نامعلوم" :a.timer}</td>
                          </tr>
                        ))}
                      </tbody> 
                    </table>
                  ) : (
                    <table className="w-full text-sm">
                      <thead className="border-b">
                        <tr>
                          <th className="text-right p-2">نام</th>
                          <th className="text-right p-2">تاریخ صدور</th>
                          <th className="text-right p-2">عملیات</th>
                        </tr>
                      </thead>
                      <tbody>
                        {certificates.map((c, i) => (
                          <tr key={i} className="border-b">
                            <td className="p-2">{c.fullname}</td>
                            <td className="p-2">{c.issuedAt}</td>
                            <td className="p-2">
                              <a href={c.fileUrl} target="_blank" className="text-blue-500 text-xs">مشاهده</a>
                            </td>
                          </tr> 
                        ))}
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
    </div>
  );
};

export default DayMeetingsModal;