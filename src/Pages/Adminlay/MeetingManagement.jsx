import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    FaSearch, FaCalendarAlt, FaUsers, FaVideo, FaFilter, FaSync,
    FaEye, FaExternalLinkAlt, FaCopy, FaChartBar, FaInfoCircle, FaTimes,
    FaCheckCircle, FaClock, FaUser, FaDoorOpen, FaHistory, FaSort,
    FaChevronLeft, FaChevronRight, FaPlay, FaRegClock, FaLayerGroup,
    FaChalkboardTeacher, FaUpload, FaVolumeUp, FaFileAlt, FaLink,
    FaTrash, FaEdit, FaSave, FaCheck, FaSpinner, FaMusic, FaExchangeAlt,
    FaPlus, FaCheckDouble, FaCloudUploadAlt, FaHeadphones, FaDownload, FaRecordVinyl, FaEyeSlash, FaCog, FaUserLock, FaGlobe, FaAward, FaExclamationTriangle, FaUserCheck
} from 'react-icons/fa';
import { GetAdminMeetings, UpdateMeetingMedia, SaveMeetingMediaLog, GetMeetingStats, GetMeetingRecordings, SaveRecordingSettings, PublishRecording, DeleteRecording, GetRecordingTracks } from '../../api/AdminAPI';
import { GetMembersOfMeet, GenerateCertificateWithUserMeetID } from '../../api/GetRooms';
import { UploadFiles } from '../../api/FTP';
import { BaseURLFiles } from '../../api/apiNeed';
import { combinePath } from '../../Tools/tools';
import { useToast } from '../../Tools/ToastProvider';
import { useAuth } from '../../Tools/AuthProvider';

// Types mapping
const MEETING_TYPES = [
    { value: 100, label: 'نشست گرین‌لایت (100)', badgeClass: 'bg-emerald-100 text-emerald-800 border-emerald-300' },
    { value: 101, label: 'وبینار API (101)', badgeClass: 'bg-purple-100 text-purple-800 border-purple-300' },
    { value: 1, label: 'عادی (1)', badgeClass: 'bg-blue-100 text-blue-800 border-blue-300' },
    { value: -99, label: 'نادیده‌گرفته‌شده (-99)', badgeClass: 'bg-gray-100 text-gray-700 border-gray-300' },
];

const LOG_TYPES = {
    DOWNLOADABLE: 102,       // MEETLOG_DOWNLOADABLE
    DOWNLOAD_FORBIDDEN: 103, // MEETLOG_DOWNLOAD_FORBIDDEN
    NORMAL: 1                // MEETLOG_NORMAL
};

const MeetingManagement = () => {
    const navigate = useNavigate();
    const toast = useToast();
    const { user } = useAuth();

    // Check if user has USER_MODERATOR or above permissions
    const isModeratorOrAbove = useMemo(() => {
        if (!user) return false;
        const perms = user.permissions || user.Permissions || [];
        return perms.includes("create_rooms") ||
               perms.includes("view_poll_reports") ||
               user.type === 502 || // USER_MODERATOR
               user.type === 504 || // USER_ADMIN
               user.type === 503;   // USER_ADMINSTRATOR
    }, [user]);

    // Meetings State
    const [meetings, setMeetings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [totalCount, setTotalCount] = useState(0);

    // Filters & Pagination
    const [searchTerm, setSearchTerm] = useState('');
    const [typeFilter, setTypeFilter] = useState('100_101'); // '100_101', '100', '101', 'all'
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(15);
    const [totalPages, setTotalPages] = useState(1);
    const [viewMode, setViewMode] = useState('table'); // 'table', 'grid'

    // 4 Stats Banner State (from API)
    const [stats, setStats] = useState({
        totalMeetings: 0,
        withVoiceCount: 0,
        withVideoCount: 0,
        withReportCount: 0
    });

    // Owner and Teacher Filters
    const [selectedOwnerId, setSelectedOwnerId] = useState('');
    const [selectedOwnerName, setSelectedOwnerName] = useState('');
    const [selectedTeacherId, setSelectedTeacherId] = useState('');
    const [selectedTeacherName, setSelectedTeacherName] = useState('');
    const [availableOwners, setAvailableOwners] = useState([]);
    const [availableTeachers, setAvailableTeachers] = useState([]);

    // Attendees Modal State
    const [attendeesModalOpen, setAttendeesModalOpen] = useState(false);
    const [selectedAttendeesMeeting, setSelectedAttendeesMeeting] = useState(null);
    const [attendeesList, setAttendeesList] = useState([]);
    const [loadingAttendees, setLoadingAttendees] = useState(false);
    const [attendeesSearch, setAttendeesSearch] = useState('');
    const [attendeesRoleFilter, setAttendeesRoleFilter] = useState('all');
    const [certModalOpen, setCertModalOpen] = useState(false);
    const [selectedAttendeeForCert, setSelectedAttendeeForCert] = useState(null);
    const [certName, setCertName] = useState('');
    const [certNationalCode, setCertNationalCode] = useState('');
    const [certPhone, setCertPhone] = useState('');
    const [certIsMale, setCertIsMale] = useState(true);
    const [issuingCert, setIssuingCert] = useState(false);
    const [issuedCertLink, setIssuedCertLink] = useState('');

    // Inline Type Updating State: meetingId -> boolean
    const [updatingTypeMap, setUpdatingTypeMap] = useState({});

    // Dedicated Sound Modal State
    const [soundModalOpen, setSoundModalOpen] = useState(false);
    const [selectedSoundMeeting, setSelectedSoundMeeting] = useState(null);
    const [soundUrl, setSoundUrl] = useState('');
    const [soundType, setSoundType] = useState(LOG_TYPES.DOWNLOADABLE);
    const [soundExplain, setSoundExplain] = useState('');
    const [savingSound, setSavingSound] = useState(false);
    const [uploadingSound, setUploadingSound] = useState(false);

    // Dedicated Video Modal State
    const [videoModalOpen, setVideoModalOpen] = useState(false);
    const [selectedVideoMeeting, setSelectedVideoMeeting] = useState(null);
    const [videoUrl, setVideoUrl] = useState('');
    const [videoType, setVideoType] = useState(LOG_TYPES.DOWNLOADABLE);
    const [videoExplain, setVideoExplain] = useState('');
    const [savingVideo, setSavingVideo] = useState(false);
    const [uploadingVideoOnly, setUploadingVideoOnly] = useState(false);

    // Dedicated Report Modal State
    const [reportModalOpen, setReportModalOpen] = useState(false);
    const [selectedReportMeeting, setSelectedReportMeeting] = useState(null);
    const [reportUrl, setReportUrl] = useState('');
    const [reportType, setReportType] = useState(LOG_TYPES.DOWNLOADABLE);
    const [reportExplain, setReportExplain] = useState('');
    const [savingReport, setSavingReport] = useState(false);

    // Legacy Media Modal State (Optional Quick Access)
    const [mediaModalOpen, setMediaModalOpen] = useState(false);
    const [activeMeeting, setActiveMeeting] = useState(null);
    const [mediaTab, setMediaTab] = useState('voice');
    const [voiceInputUrl, setVoiceInputUrl] = useState('');
    const [videoInputUrl, setVideoInputUrl] = useState('');
    const [reportInputUrl, setReportInputUrl] = useState('');
    const [uploadingVoice, setUploadingVoice] = useState(false);
    const [uploadingVideo, setUploadingVideo] = useState(false);
    const [savingMedia, setSavingMedia] = useState(false);

    // Details Modal
    const [selectedMeeting, setSelectedMeeting] = useState(null);
    const [showDetailsModal, setShowDetailsModal] = useState(false);

    // File input refs
    const voiceFileRef = useRef(null);
    const videoFileRef = useRef(null);
    const soundModalFileRef = useRef(null);
    const videoModalFileRef = useRef(null);

    // Helper to format full media URL
    const getFullMediaUrl = (url) => {
        if (!url) return '';
        if (url.startsWith('http://') || url.startsWith('https://')) return url;
        return combinePath(BaseURLFiles, url);
    };

    // Helper to format external redirect URL
    const getFullReportUrl = (url) => {
        if (!url) return '';
        if (url.startsWith('http://') || url.startsWith('https://')) return url;
        return `https://${url}`;
    };

    // Fetch Meetings from backend
    const fetchMeetings = async (
        page = currentPage,
        q = searchTerm,
        typeMode = typeFilter,
        ownerId = selectedOwnerId,
        teacherId = selectedTeacherId
    ) => {
        setLoading(true);
        setError(null);
        try {
            let backendTypeParam = null;
            if (typeMode === '100') backendTypeParam = 100;
            else if (typeMode === '101') backendTypeParam = 101;
            else if (typeMode === 'all') backendTypeParam = null;
            else backendTypeParam = null; // default in backend is (Type == 100 || Type == 101)

            const res = await GetAdminMeetings(page, itemsPerPage, q, backendTypeParam, ownerId || null, teacherId || null);
            if (res) {
                const items = res.items || (Array.isArray(res.data) ? res.data : []);
                setMeetings(items);
                setTotalCount(res.totalCount || items.length);
                setTotalPages(res.totalPages || Math.ceil((res.totalCount || items.length) / itemsPerPage) || 1);

                if (res.stats) {
                    setStats(res.stats);
                } else {
                    try {
                        const statsRes = await GetMeetingStats(backendTypeParam, q, ownerId || null, teacherId || null);
                        if (statsRes) setStats(statsRes);
                    } catch (e) {
                        console.warn("Failed to fetch meeting stats:", e);
                    }
                }

                // Collect available owners and teachers for dropdown filters
                setAvailableOwners(prev => {
                    const map = new Map(prev.map(o => [o.id, o]));
                    items.forEach(m => {
                        if (m.owner && m.owner.id) map.set(m.owner.id, m.owner);
                    });
                    return Array.from(map.values());
                });

                setAvailableTeachers(prev => {
                    const map = new Map(prev.map(t => [t.id, t]));
                    items.forEach(m => {
                        if (m.teacher && m.teacher.id) map.set(m.teacher.id, m.teacher);
                    });
                    return Array.from(map.values());
                });
            } else {
                setMeetings([]);
                setTotalCount(0);
                setTotalPages(1);
            }
        } catch (err) {
            console.error("Error fetching admin meetings:", err);
            setError("خطا در برقراری ارتباط با سرور و دریافت لیست نشست‌ها");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchMeetings(currentPage, searchTerm, typeFilter, selectedOwnerId, selectedTeacherId);
    }, [currentPage, itemsPerPage, typeFilter, selectedOwnerId, selectedTeacherId]);

    // Handle search input with debouncing or on Enter / button click
    const handleSearchSubmit = (e) => {
        if (e) e.preventDefault();
        setCurrentPage(1);
        fetchMeetings(1, searchTerm, typeFilter, selectedOwnerId, selectedTeacherId);
    };

    const handleFilterByOwner = (ownerId, ownerName) => {
        const idStr = ownerId ? String(ownerId) : '';
        setSelectedOwnerId(idStr);
        setSelectedOwnerName(ownerName || '');
        setCurrentPage(1);
    };

    const handleClearOwnerFilter = () => {
        setSelectedOwnerId('');
        setSelectedOwnerName('');
        setCurrentPage(1);
    };

    const handleFilterByTeacher = (teacherId, teacherName) => {
        const idStr = teacherId ? String(teacherId) : '';
        setSelectedTeacherId(idStr);
        setSelectedTeacherName(teacherName || '');
        setCurrentPage(1);
    };

    const handleClearTeacherFilter = () => {
        setSelectedTeacherId('');
        setSelectedTeacherName('');
        setCurrentPage(1);
    };

    const handleCopyMeetId = (meetId) => {
        if (!meetId) return;
        navigator.clipboard.writeText(meetId);
        if (toast?.success) {
            toast.success(`شناسه جلسه ${meetId} کپی شد`);
        }
    };

    const openAttendeesModal = async (meeting) => {
        setSelectedAttendeesMeeting(meeting);
        setAttendeesModalOpen(true);
        setLoadingAttendees(true);
        setAttendeesSearch('');
        setAttendeesRoleFilter('all');
        try {
            const data = await GetMembersOfMeet(meeting.meetID);
            const members = Array.isArray(data) ? data : (data?.members || data?.data || []);
            setAttendeesList(members);
        } catch (err) {
            console.error("Error loading attendees:", err);
            if (toast?.error) {
                toast.error("خطا در دریافت لیست حضار این جلسه");
            }
            setAttendeesList([]);
        } finally {
            setLoadingAttendees(false);
        }
    };

    // Filtered Attendees for Attendees Modal
    const filteredAttendees = useMemo(() => {
        if (!attendeesList || !Array.isArray(attendeesList)) return [];
        return attendeesList.filter(a => {
            if (!a) return false;
            const term = (attendeesSearch || '').trim().toLowerCase();
            const matchesSearch = !term ||
                (a.name && a.name.toLowerCase().includes(term)) ||
                (a.id && String(a.id).includes(term)) ||
                (a.nationalCode && a.nationalCode.toLowerCase().includes(term)) ||
                (a.phone && a.phone.toLowerCase().includes(term)) ||
                (a.email && a.email.toLowerCase().includes(term)) ||
                (a.role && a.role.toLowerCase().includes(term));
            if (!matchesSearch) return false;

            if (attendeesRoleFilter === 'moderator' || attendeesRoleFilter === 'teacher') {
                return a.isTeacher || (a.role || '').toLowerCase().includes('teach') || (a.role || '').toLowerCase().includes('mod') || (a.role || '').toLowerCase().includes('present');
            }
            if (attendeesRoleFilter === 'vip') {
                return !!a.isMemberVIP;
            }
            if (attendeesRoleFilter === 'cert') {
                return !!(a.hasTakenCertificate || a.isCertificated || a.certificateURL);
            }
            if (attendeesRoleFilter === 'account') { return !!(a.hasAccount || (a.email && a.email.trim())); }
            if (false) { if (false) {
            }


            }
            if (attendeesRoleFilter === 'viewer') {

                const isT = a.isTeacher || (a.role || '').toLowerCase().includes('teach') || (a.role || '').toLowerCase().includes('mod');
                return !isT;
            }
            return true;
        });
    }, [attendeesList, attendeesSearch, attendeesRoleFilter]);

    const handleOpenCertModal = (att) => {
        setSelectedAttendeeForCert(att);
        setCertName(att.name || '');
        setCertNationalCode(att.nationalCode || '');
        setCertPhone(att.phone || '');
        setCertIsMale(att.gender === 'خانم' ? false : true);
        setIssuedCertLink(att.certificateURL || (att.hasTakenCertificate ? 'exists' : ''));
        setCertModalOpen(true);
    };

    const handleIssueCertificateForAttendee = async () => {
        if (!selectedAttendeeForCert || !selectedAttendeesMeeting) return;
        const natCode = (certNationalCode || '').trim();
        const phone = (certPhone || '').trim();
        if (!natCode || !phone) {
            if (toast?.error) toast.error("ثبت هر دو فیلد کد ملی و شماره موبایل جهت صدور گواهی الزامی است");
            return;
        }

        setIssuingCert(true);
        try {
            const link = await GenerateCertificateWithUserMeetID(selectedAttendeeForCert.id, {
                nationalCode: natCode,
                phone: phone,
                isMale: certIsMale,
                meetID: selectedAttendeesMeeting.meetID,
                name: (certName || '').trim() || selectedAttendeeForCert.name
            });

            if (link) {
                setIssuedCertLink(link);
                if (toast?.success) toast.success("گواهینامه حضور با موفقیت صادر گردید");

                setAttendeesList(prev => prev.map(a => a.id === selectedAttendeeForCert.id ? {
                    ...a,
                    hasTakenCertificate: true,
                    isCertificated: true,
                    certificateURL: link,
                    nationalCode: natCode,
                    phone: phone,
                    name: (certName || '').trim() || a.name
                } : a));
            }
        } catch (err) {
            console.error("Error issuing certificate:", err);
            if (toast?.error) toast.error(err.message || "خطا در صدور گواهینامه");
        } finally {
            setIssuingCert(false);
        }
    };

    const formatDuration = (seconds) => {
        if (!seconds && seconds !== 0) return '-';
        const s = Math.round(Number(seconds));
        if (isNaN(s)) return '-';
        const m = Math.floor(s / 60);
        const remS = s % 60;
        if (m === 0) return `${remS} ثانیه`;
        return `${m} دقیقه و ${remS} ثانیه`;
    };

    // ==========================================
    // BBB Recording Modal State & Handlers
    // ==========================================
    const [recordModalOpen, setRecordModalOpen] = useState(false);
    const [selectedRecordMeeting, setSelectedRecordMeeting] = useState(null);
    const [recordingsLoading, setRecordingsLoading] = useState(false);
    const [recordingData, setRecordingData] = useState(null);
    const [recordViewPermission, setRecordViewPermission] = useState('all');
    const [recordIsDownloadable, setRecordIsDownloadable] = useState(false);
    const [selectedRecordId, setSelectedRecordId] = useState('');
    const [savingRecordSettings, setSavingRecordSettings] = useState(false);
    const [togglingPublish, setTogglingPublish] = useState(false);
    const [deletingRecordId, setDeletingRecordId] = useState(null);
    const [copiedPlaybackUrl, setCopiedPlaybackUrl] = useState(false);

    const openRecordModal = async (meeting) => {
        setSelectedRecordMeeting(meeting);
        setRecordModalOpen(true);
        setRecordingsLoading(true);
        setRecordingData(null);
        setCopiedPlaybackUrl(false);

        try {
            const res = await GetMeetingRecordings(meeting.id);
            if (res && res.status === 200 && res.data) {
                const data = res.data;
                setRecordingData(data);
                const settings = data.settings || {};
                setRecordViewPermission(settings.viewPermission || 'all');
                setRecordIsDownloadable(settings.isDownloadable || false);
                const initRecId = settings.recordId || (data.recordings?.[0]?.recordId || '');
                setSelectedRecordId(initRecId);
            } else {
                if (toast?.error) toast.error(res?.message || 'خطا در دریافت اطلاعات رکورد');
            }
        } catch (err) {
            console.error('Error fetching recordings:', err);
            if (toast?.error) toast.error(err.message || 'خطا در برقراری ارتباط با سرور');
        } finally {
            setRecordingsLoading(false);
        }
    };

    const handleSaveRecordSettings = async () => {
        if (!selectedRecordMeeting) return;
        setSavingRecordSettings(true);
        try {
            const activeRecId = selectedRecordId || recordingData?.recordings?.[0]?.recordId;
            const payload = {
                meetingId: selectedRecordMeeting.id,
                recordId: activeRecId,
                viewPermission: recordViewPermission,
                isDownloadable: recordIsDownloadable
            };
            const res = await SaveRecordingSettings(payload);
            if (res && res.status === 200) {
                if (toast?.success) toast.success('تنظیمات رکورد با موفقیت ذخیره شد');
                const pbUrl = res.data?.playbackUrl || (recordingData?.recordings?.find(r => r.recordId === activeRecId)?.playbackUrl);
                setMeetings(prev => prev.map(m => {
                    if (m.id === selectedRecordMeeting.id) {
                        return {
                            ...m,
                            isRecorded: true,
                            recordingPlaybackUrl: pbUrl || m.recordingPlaybackUrl,
                            recordingViewPermission: recordViewPermission,
                            recordingDownloadable: recordIsDownloadable,
                            recordingRecordId: activeRecId
                        };
                    }
                    return m;
                }));
                setRecordingData(prev => prev ? ({
                    ...prev,
                    settings: {
                        ...prev.settings,
                        viewPermission: recordViewPermission,
                        isDownloadable: recordIsDownloadable,
                        recordId: activeRecId,
                        playbackUrl: pbUrl || prev.settings?.playbackUrl
                    }
                }) : prev);
            } else {
                if (toast?.error) toast.error(res?.message || 'خطا در ذخیره تنظیمات رکورد');
            }
        } catch (err) {
            console.error('Error saving record settings:', err);
            if (toast?.error) toast.error(err.message || 'خطا در ذخیره تنظیمات');
        } finally {
            setSavingRecordSettings(false);
        }
    };

    const handleTogglePublish = async (recordId, currentPublishState) => {
        if (!selectedRecordMeeting || !recordId) return;
        setTogglingPublish(true);
        try {
            const nextState = !currentPublishState;
            const res = await PublishRecording({
                meetingId: selectedRecordMeeting.id,
                recordId: recordId,
                publish: nextState
            });
            if (res && res.status === 200) {
                if (toast?.success) toast.success(nextState ? 'رکورد با موفقیت منتشر شد' : 'انتشار رکورد لغو شد');
                setRecordingData(prev => {
                    if (!prev) return prev;
                    return {
                        ...prev,
                        recordings: prev.recordings.map(r => r.recordId === recordId ? { ...r, published: nextState } : r),
                        settings: { ...prev.settings, published: nextState }
                    };
                });
            } else {
                if (toast?.error) toast.error(res?.message || 'خطا در تغییر وضعیت انتشار');
            }
        } catch (err) {
            console.error('Error toggling publish:', err);
            if (toast?.error) toast.error(err.message || 'خطا در تغییر وضعیت انتشار');
        } finally {
            setTogglingPublish(false);
        }
    };

    const handleDeleteRecording = async (recordId) => {
        if (!selectedRecordMeeting || !recordId) return;
        if (!window.confirm("آیا از حذف کامل این رکورد از سرور بیگ‌بلو‌باتن اطمینان دارید؟ این عملیات غیرقابل بازگشت است.")) return;

        setDeletingRecordId(recordId);
        try {
            const res = await DeleteRecording({
                meetingId: selectedRecordMeeting.id,
                recordId: recordId
            });
            if (res && res.status === 200) {
                if (toast?.success) toast.success('رکورد با موفقیت از سرور حذف شد');
                setRecordingData(prev => {
                    if (!prev) return prev;
                    const filtered = prev.recordings.filter(r => r.recordId !== recordId);
                    return {
                        ...prev,
                        recordings: filtered,
                        hasRecordings: filtered.length > 0
                    };
                });
                setMeetings(prev => prev.map(m => {
                    if (m.id === selectedRecordMeeting.id) {
                        return {
                            ...m,
                            recordingPlaybackUrl: null,
                            recordingRecordId: null
                        };
                    }
                    return m;
                }));
            } else {
                if (toast?.error) toast.error(res?.message || 'خطا در حذف رکورد');
            }
        } catch (err) {
            console.error('Error deleting recording:', err);
            if (toast?.error) toast.error(err.message || 'خطا در حذف رکورد');
        } finally {
            setDeletingRecordId(null);
        }
    };

    const copyRecordingLink = (url) => {
        if (!url) return;
        navigator.clipboard.writeText(url);
        setCopiedPlaybackUrl(true);
        if (toast?.success) toast.success('لینک تماشای رکورد کپی شد');
        setTimeout(() => setCopiedPlaybackUrl(false), 2500);
    };


    // Handle Inline Type Change
    const handleTypeChange = async (meetingId, newTypeValue) => {
        const typeInt = parseInt(newTypeValue, 10);
        setUpdatingTypeMap(prev => ({ ...prev, [meetingId]: true }));
        try {
            await UpdateMeetingMedia({
                meetingId: meetingId,
                type: typeInt
            });

            // Update meeting in local state
            setMeetings(prev => prev.map(m => {
                if (m.id === meetingId) {
                    const matchedType = MEETING_TYPES.find(t => t.value === typeInt);
                    return {
                        ...m,
                        type: typeInt,
                        typeTitle: matchedType ? matchedType.label : `نوع ${typeInt}`
                    };
                }
                return m;
            }));

            if (toast?.success) {
                toast.success(`نوع جلسه با شناسه ${meetingId} با موفقیت بهروزرسانی شد`);
            }
        } catch (err) {
            console.error("Failed to update meeting type:", err);
            if (toast?.error) {
                toast.error("خطا در تغییر نوع جلسه. لطفاً مجدداً تلاش کنید.");
            } else {
                alert("خطا در تغییر نوع جلسه");
            }
        } finally {
            setUpdatingTypeMap(prev => ({ ...prev, [meetingId]: false }));
        }
    };

    // Open Media Modal for a specific meeting
    const openMediaModal = (meeting, initialTab = 'voice') => {
        setActiveMeeting(meeting);
        setMediaTab(initialTab);
        setVoiceInputUrl(meeting.voiceUrl || '');
        setVideoInputUrl(meeting.videoUrl || '');
        setReportInputUrl(meeting.reportUrl || '');
        setMediaModalOpen(true);
    };

    // Voice file upload handler
    const handleVoiceFileUpload = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setUploadingVoice(true);
        try {
            const uploadRes = await UploadFiles([file], "meetings/audio", BaseURLFiles);
            let url = "";
            if (uploadRes) {
                if (uploadRes.urls && uploadRes.urls[0]) url = uploadRes.urls[0];
                else if (uploadRes.URLs && uploadRes.URLs[0]) url = uploadRes.URLs[0];
                else if (uploadRes.urLs && uploadRes.urLs[0]) url = uploadRes.urLs[0];
                else if (uploadRes.data?.urls?.[0]) url = uploadRes.data.urls[0];
                else if (Array.isArray(uploadRes.data) && uploadRes.data[0]) url = uploadRes.data[0];
            }

            if (!url) {
                throw new Error("آدرس فایل از سرور بازگردانده نشد");
            }

            const fullUrl = getFullMediaUrl(url);
            setVoiceInputUrl(fullUrl);

            // Directly save to meeting
            if (activeMeeting) {
                await UpdateMeetingMedia({
                    meetingId: activeMeeting.id,
                    voiceUrl: fullUrl
                });

                setActiveMeeting(prev => ({ ...prev, voiceUrl: fullUrl }));
                setMeetings(prev => prev.map(m => m.id === activeMeeting.id ? { ...m, voiceUrl: fullUrl } : m));
            }

            if (toast?.success) {
                toast.success("فایل صوتی با موفقیت آپلود و ذخیره شد");
            }
        } catch (err) {
            console.error("Audio upload error:", err);
            if (toast?.error) {
                toast.error(err.message || "خطا در آپلود فایل صوتی");
            } else {
                alert("خطا در آپلود فایل صوتی");
            }
        } finally {
            setUploadingVoice(false);
            if (voiceFileRef.current) voiceFileRef.current.value = "";
        }
    };

    // Video file upload handler
    const handleVideoFileUpload = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setUploadingVideo(true);
        try {
            const uploadRes = await UploadFiles([file], "meetings/video", BaseURLFiles);
            let url = "";
            if (uploadRes) {
                if (uploadRes.urls && uploadRes.urls[0]) url = uploadRes.urls[0];
                else if (uploadRes.URLs && uploadRes.URLs[0]) url = uploadRes.URLs[0];
                else if (uploadRes.urLs && uploadRes.urLs[0]) url = uploadRes.urLs[0];
                else if (uploadRes.data?.urls?.[0]) url = uploadRes.data.urls[0];
                else if (Array.isArray(uploadRes.data) && uploadRes.data[0]) url = uploadRes.data[0];
            }

            if (!url) {
                throw new Error("آدرس فایل ویدیو از سرور بازگردانده نشد");
            }

            const fullUrl = getFullMediaUrl(url);
            setVideoInputUrl(fullUrl);

            // Directly save to meeting
            if (activeMeeting) {
                await UpdateMeetingMedia({
                    meetingId: activeMeeting.id,
                    videoUrl: fullUrl
                });

                setActiveMeeting(prev => ({ ...prev, videoUrl: fullUrl }));
                setMeetings(prev => prev.map(m => m.id === activeMeeting.id ? { ...m, videoUrl: fullUrl } : m));
            }

            if (toast?.success) {
                toast.success("فایل ویدیو با موفقیت آپلود و ذخیره شد");
            }
        } catch (err) {
            console.error("Video upload error:", err);
            if (toast?.error) {
                toast.error(err.message || "خطا در آپلود فایل ویدیو");
            } else {
                alert("خطا در آپلود فایل ویدیو");
            }
        } finally {
            setUploadingVideo(false);
            if (videoFileRef.current) videoFileRef.current.value = "";
        }
    };

    // Save All Media URLs from modal
    const handleSaveAllMedia = async () => {
        if (!activeMeeting) return;
        setSavingMedia(true);
        try {
            await UpdateMeetingMedia({
                meetingId: activeMeeting.id,
                voiceUrl: voiceInputUrl.trim(),
                videoUrl: videoInputUrl.trim(),
                reportUrl: reportInputUrl.trim()
            });

            // Update in meetings list
            setMeetings(prev => prev.map(m => {
                if (m.id === activeMeeting.id) {
                    return {
                        ...m,
                        voiceUrl: voiceInputUrl.trim(),
                        videoUrl: videoInputUrl.trim(),
                        reportUrl: reportInputUrl.trim()
                    };
                }
                return m;
            }));

            if (toast?.success) {
                toast.success("اطلاعات رسانه‌ای و لینک گزارش جلسه با موفقیت ذخیره شد");
            }
            setMediaModalOpen(false);
        } catch (err) {
            console.error("Failed to save media URLs:", err);
            if (toast?.error) {
                toast.error("خطا در ذخیره اطلاعات رسانه‌ای");
            } else {
                alert("خطا در ذخیره اطلاعات رسانه‌ای");
            }
        } finally {
            setSavingMedia(false);
        }
    };

    // Quick remove media URL helper
    const handleQuickRemoveMedia = async (meetingId, mediaType) => {
        if (!window.confirm(`آیا از حذف این ${mediaType === 'voice' ? 'صوت' : mediaType === 'video' ? 'ویدیو' : 'لینک گزارش'} اطمینان دارید؟`)) {
            return;
        }

        try {
            const payload = { meetingId };
            if (mediaType === 'voice') payload.voiceUrl = '';
            if (mediaType === 'video') payload.videoUrl = '';
            if (mediaType === 'report') payload.reportUrl = '';

            await UpdateMeetingMedia(payload);

            setMeetings(prev => prev.map(m => {
                if (m.id === meetingId) {
                    return {
                        ...m,
                        [mediaType === 'voice' ? 'voiceUrl' : mediaType === 'video' ? 'videoUrl' : 'reportUrl']: ''
                    };
                }
                return m;
            }));

            if (toast?.success) {
                toast.success("عملیات با موفقیت انجام شد");
            }
        } catch (err) {
            console.error("Error removing media:", err);
            if (toast?.error) toast.error("خطا در حذف مدیا");
        }
    };

    // ==========================================
    // Separate Modal Handlers: Sound, Video, Report
    // ==========================================
    const openSoundModal = (meeting) => {
        setSelectedSoundMeeting(meeting);
        setSoundUrl(meeting.voiceUrl || '');
        setSoundType(meeting.voiceType || LOG_TYPES.DOWNLOADABLE);
        setSoundExplain(meeting.voiceExplain || '');
        setSoundModalOpen(true);
    };

    const handleSoundFileUpload = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setUploadingSound(true);
        try {
            const uploadRes = await UploadFiles([file], "meetings/audio", BaseURLFiles);
            let url = "";
            if (uploadRes) {
                if (uploadRes.urls && uploadRes.urls[0]) url = uploadRes.urls[0];
                else if (uploadRes.URLs && uploadRes.URLs[0]) url = uploadRes.URLs[0];
                else if (uploadRes.urLs && uploadRes.urLs[0]) url = uploadRes.urLs[0];
                else if (uploadRes.data?.urls?.[0]) url = uploadRes.data.urls[0];
                else if (Array.isArray(uploadRes.data) && uploadRes.data[0]) url = uploadRes.data[0];
            }
            if (!url) throw new Error("آدرس فایل صوتی از سرور دریافت نشد");
            const fullUrl = getFullMediaUrl(url);
            setSoundUrl(fullUrl);
            if (toast?.success) toast.success("فایل صوتی با موفقیت آپلود شد");
        } catch (err) {
            console.error("Sound upload error:", err);
            if (toast?.error) toast.error(err.message || "خطا در آپلود صوت");
        } finally {
            setUploadingSound(false);
            if (soundModalFileRef.current) soundModalFileRef.current.value = "";
        }
    };

    const handleSaveSound = async () => {
        if (!selectedSoundMeeting) return;
        setSavingSound(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedSoundMeeting.id,
                key: "sound_link",
                value: soundUrl.trim(),
                type: parseInt(soundType),
                explain: soundExplain.trim()
            });

            setMeetings(prev => prev.map(m => m.id === selectedSoundMeeting.id ? {
                ...m,
                voiceUrl: soundUrl.trim(),
                voiceType: parseInt(soundType),
                voiceExplain: soundExplain.trim()
            } : m));

            if (toast?.success) toast.success("اطلاعات صوت با موفقیت ثبت شد");
            setSoundModalOpen(false);
        } catch (err) {
            console.error("Error saving sound log:", err);
            if (toast?.error) toast.error("خطا در ذخیره اطلاعات صوت");
        } finally {
            setSavingSound(false);
        }
    };

    const handleDeleteSound = async () => {
        if (!selectedSoundMeeting) return;
        if (!window.confirm("آیا از حذف فایل صوتی این جلسه اطمینان دارید؟")) return;
        setSavingSound(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedSoundMeeting.id,
                key: "sound_link",
                value: "",
                type: 102
            });

            setMeetings(prev => prev.map(m => m.id === selectedSoundMeeting.id ? {
                ...m,
                voiceUrl: '',
                voiceType: LOG_TYPES.DOWNLOADABLE,
                voiceExplain: ''
            } : m));

            setSoundUrl('');
            setSoundExplain('');
            if (toast?.success) toast.success("صوت جلسه حذف گردید");
            setSoundModalOpen(false);
        } catch (err) {
            console.error("Error deleting sound log:", err);
            if (toast?.error) toast.error("خطا در حذف صوت جلسه");
        } finally {
            setSavingSound(false);
        }
    };

    // Video Handlers
    const openVideoModal = (meeting) => {
        setSelectedVideoMeeting(meeting);
        setVideoUrl(meeting.videoUrl || '');
        setVideoType(meeting.videoType || LOG_TYPES.DOWNLOADABLE);
        setVideoExplain(meeting.videoExplain || '');
        setVideoModalOpen(true);
    };

    const handleVideoFileUploadOnly = async (e) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setUploadingVideoOnly(true);
        try {
            const uploadRes = await UploadFiles([file], "meetings/video", BaseURLFiles);
            let url = "";
            if (uploadRes) {
                if (uploadRes.urls && uploadRes.urls[0]) url = uploadRes.urls[0];
                else if (uploadRes.URLs && uploadRes.URLs[0]) url = uploadRes.URLs[0];
                else if (uploadRes.urLs && uploadRes.urLs[0]) url = uploadRes.urLs[0];
                else if (uploadRes.data?.urls?.[0]) url = uploadRes.data.urls[0];
                else if (Array.isArray(uploadRes.data) && uploadRes.data[0]) url = uploadRes.data[0];
            }
            if (!url) throw new Error("آدرس فایل ویدیویی دریافت نشد");
            const fullUrl = getFullMediaUrl(url);
            setVideoUrl(fullUrl);
            if (toast?.success) toast.success("فایل ویدیو با موفقیت آپلود شد");
        } catch (err) {
            console.error("Video upload error:", err);
            if (toast?.error) toast.error(err.message || "خطا در آپلود ویدیو");
        } finally {
            setUploadingVideoOnly(false);
            if (videoModalFileRef.current) videoModalFileRef.current.value = "";
        }
    };

    const handleSaveVideo = async () => {
        if (!selectedVideoMeeting) return;
        setSavingVideo(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedVideoMeeting.id,
                key: "video_link",
                value: videoUrl.trim(),
                type: parseInt(videoType),
                explain: videoExplain.trim()
            });

            setMeetings(prev => prev.map(m => m.id === selectedVideoMeeting.id ? {
                ...m,
                videoUrl: videoUrl.trim(),
                videoType: parseInt(videoType),
                videoExplain: videoExplain.trim()
            } : m));

            if (toast?.success) toast.success("اطلاعات ویدیو با موفقیت ثبت شد");
            setVideoModalOpen(false);
        } catch (err) {
            console.error("Error saving video log:", err);
            if (toast?.error) toast.error("خطا در ذخیره اطلاعات فیلم");
        } finally {
            setSavingVideo(false);
        }
    };

    const handleDeleteVideo = async () => {
        if (!selectedVideoMeeting) return;
        if (!window.confirm("آیا از حذف فایل ویدیویی این جلسه اطمینان دارید؟")) return;
        setSavingVideo(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedVideoMeeting.id,
                key: "video_link",
                value: "",
                type: 102
            });

            setMeetings(prev => prev.map(m => m.id === selectedVideoMeeting.id ? {
                ...m,
                videoUrl: '',
                videoType: LOG_TYPES.DOWNLOADABLE,
                videoExplain: ''
            } : m));

            setVideoUrl('');
            setVideoExplain('');
            if (toast?.success) toast.success("فیلم جلسه حذف گردید");
            setVideoModalOpen(false);
        } catch (err) {
            console.error("Error deleting video log:", err);
            if (toast?.error) toast.error("خطا در حذف فیلم جلسه");
        } finally {
            setSavingVideo(false);
        }
    };

    // Report Handlers
    const openReportModal = (meeting) => {
        setSelectedReportMeeting(meeting);
        setReportUrl(meeting.reportUrl || '');
        setReportType(meeting.reportType || LOG_TYPES.DOWNLOADABLE);
        setReportExplain(meeting.reportExplain || '');
        setReportModalOpen(true);
    };

    const handleSaveReport = async () => {
        if (!selectedReportMeeting) return;
        setSavingReport(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedReportMeeting.id,
                key: "report_link",
                value: reportUrl.trim(),
                type: parseInt(reportType),
                explain: reportExplain.trim()
            });

            setMeetings(prev => prev.map(m => m.id === selectedReportMeeting.id ? {
                ...m,
                reportUrl: reportUrl.trim(),
                reportType: parseInt(reportType),
                reportExplain: reportExplain.trim()
            } : m));

            if (toast?.success) toast.success("اطلاعات گزارش جلسه با موفقیت ثبت شد");
            setReportModalOpen(false);
        } catch (err) {
            console.error("Error saving report log:", err);
            if (toast?.error) toast.error("خطا در ذخیره اطلاعات گزارش");
        } finally {
            setSavingReport(false);
        }
    };

    const handleDeleteReport = async () => {
        if (!selectedReportMeeting) return;
        if (!window.confirm("آیا از حذف لینک گزارش این جلسه اطمینان دارید؟")) return;
        setSavingReport(true);
        try {
            await SaveMeetingMediaLog({
                meetingId: selectedReportMeeting.id,
                key: "report_link",
                value: "",
                type: 102
            });

            setMeetings(prev => prev.map(m => m.id === selectedReportMeeting.id ? {
                ...m,
                reportUrl: '',
                reportType: LOG_TYPES.DOWNLOADABLE,
                reportExplain: ''
            } : m));

            setReportUrl('');
            setReportExplain('');
            if (toast?.success) toast.success("گزارش جلسه حذف گردید");
            setReportModalOpen(false);
        } catch (err) {
            console.error("Error deleting report log:", err);
            if (toast?.error) toast.error("خطا در حذف گزارش جلسه");
        } finally {
            setSavingReport(false);
        }
    };

    const copyJoinLink = (friendlyID) => {
        if (!friendlyID) return;
        const joinUrl = `${window.location.origin}/rooms/${friendlyID}/join`;
        navigator.clipboard.writeText(joinUrl);
        if (toast?.success) {
            toast.success("لینک ورود به جلسه کپی شد");
        } else {
            alert("لینک ورود کپی شد: " + joinUrl);
        }
    };

    return (
        <div className="p-4 sm:p-6 lg:p-8 bg-gray-50 min-h-screen rounded-2xl" dir="rtl">
            {/* Header section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
                <div>
                    <h2 className="text-2xl font-black text-gray-900 flex items-center gap-3">
                        <span className="p-2.5 bg-gradient-to-r from-orange-500 to-amber-500 text-white rounded-xl shadow-md">
                            <FaCalendarAlt size={22} />
                        </span>
                        مدیریت جلسات و نشست‌ها
                    </h2>
                    <p className="text-sm text-gray-500 mt-1 font-medium">
                        مدیریت جلسات نوع ۱۰۰ (گرین‌لایت) و ۱۰۱ (وبینار API)، آپلود صوت، ویدیو، لینک شرح جلسه و ویرایش تایپ
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        onClick={() => fetchMeetings(currentPage, searchTerm, typeFilter, selectedOwnerId, selectedTeacherId)}
                        disabled={loading}
                        className="flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-200 text-gray-700 rounded-xl hover:bg-gray-100 transition shadow-sm font-bold text-sm disabled:opacity-50"
                    >
                        <FaSync className={loading ? "animate-spin text-orange-500" : "text-gray-500"} />
                        <span>بروزرسانی لیست</span>
                    </button>
                </div>
            </div>

            {/* Quick Stats Banner */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">تعداد جلسات یافته‌شده</p>
                        <p className="text-2xl font-black text-gray-900">
                            {stats.totalMeetings !== undefined ? stats.totalMeetings : totalCount}
                        </p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-orange-50 text-orange-600 flex items-center justify-center font-bold">
                        <FaCalendarAlt size={22} />
                    </div>
                </div>

                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">دارای فایل صوتی</p>
                        <p className="text-2xl font-black text-blue-600">
                            {stats.withVoiceCount !== undefined ? stats.withVoiceCount : meetings.filter(m => !!m.voiceUrl).length}
                        </p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center font-bold">
                        <FaVolumeUp size={20} />
                    </div>
                </div>

                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">دارای فیلم جلسه</p>
                        <p className="text-2xl font-black text-emerald-600">
                            {stats.withVideoCount !== undefined ? stats.withVideoCount : meetings.filter(m => !!m.videoUrl).length}
                        </p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold">
                        <FaVideo size={20} />
                    </div>
                </div>

                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm flex items-center justify-between">
                    <div>
                        <p className="text-xs text-gray-500 font-bold mb-1">دارای گزارش و شرح</p>
                        <p className="text-2xl font-black text-amber-600">
                            {stats.withReportCount !== undefined ? stats.withReportCount : meetings.filter(m => !!m.reportUrl).length}
                        </p>
                    </div>
                    <div className="w-12 h-12 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center font-bold">
                        <FaFileAlt size={20} />
                    </div>
                </div>
            </div>

            {/* Filter Controls Bar */}
            <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-sm mb-4 flex flex-col lg:flex-row gap-4 justify-between items-center">
                {/* Search Bar */}
                <form onSubmit={handleSearchSubmit} className="flex-1 w-full">
                    <div className="relative">
                        <input
                            type="text"
                            placeholder="جستجو در عنوان جلسه، شناسه، اتاق یا نام استاد..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pr-10 pl-24 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:bg-white focus:ring-2 focus:ring-orange-500 focus:outline-none transition"
                        />
                        <FaSearch className="absolute right-3.5 top-3.5 text-gray-400" />
                        <button
                            type="submit"
                            className="absolute left-1.5 top-1.5 bottom-1.5 px-4 bg-orange-500 hover:bg-orange-600 text-white rounded-lg text-xs font-bold transition"
                        >
                            جستجو
                        </button>
                    </div>
                </form>

                {/* Filters */}
                <div className="flex flex-wrap items-center gap-3 w-full lg:w-auto justify-end">
                    {/* Type Filter */}
                    <div className="flex items-center gap-2 bg-gray-50 px-3 py-2 border border-gray-200 rounded-xl">
                        <FaFilter className="text-gray-500 text-xs" />
                        <span className="text-xs text-gray-500 font-bold">فیلتر نوع:</span>
                        <select
                            value={typeFilter}
                            onChange={(e) => {
                                setTypeFilter(e.target.value);
                                setCurrentPage(1);
                            }}
                            className="bg-transparent text-xs font-bold text-gray-800 focus:outline-none cursor-pointer"
                        >
                            <option value="100_101">فقط نوع ۱۰۰ و ۱۰۱ (پیش‌فرض)</option>
                            <option value="100">فقط نوع ۱۰۰ (نشست گرین‌لایت)</option>
                            <option value="101">فقط نوع ۱۰۱ (وبینار API)</option>
                            <option value="all">همه انواع جلسات</option>
                        </select>
                    </div>

                    {/* Owner Filter Dropdown */}
                    <div className="flex items-center gap-2 bg-gray-50 px-3 py-2 border border-gray-200 rounded-xl">
                        <FaUser className="text-gray-500 text-xs" />
                        <span className="text-xs text-gray-500 font-bold">مالک:</span>
                        <select
                            value={selectedOwnerId}
                            onChange={(e) => {
                                const val = e.target.value;
                                const found = availableOwners.find(o => String(o.id) === String(val));
                                handleFilterByOwner(val, found ? (found.fullname || found.name) : '');
                            }}
                            className="bg-transparent text-xs font-bold text-gray-800 focus:outline-none cursor-pointer max-w-[140px] truncate"
                        >
                            <option value="">همه مالکین</option>
                            {availableOwners.map(o => (
                                <option key={o.id} value={o.id}>
                                    {o.fullname || o.name || `مالک #${o.id}`}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* Teacher Filter Dropdown */}
                    <div className="flex items-center gap-2 bg-gray-50 px-3 py-2 border border-gray-200 rounded-xl">
                        <FaChalkboardTeacher className="text-gray-500 text-xs" />
                        <span className="text-xs text-gray-500 font-bold">استاد:</span>
                        <select
                            value={selectedTeacherId}
                            onChange={(e) => {
                                const val = e.target.value;
                                const found = availableTeachers.find(t => String(t.id) === String(val));
                                handleFilterByTeacher(val, found ? (found.fullname || found.name) : '');
                            }}
                            className="bg-transparent text-xs font-bold text-gray-800 focus:outline-none cursor-pointer max-w-[140px] truncate"
                        >
                            <option value="">همه اساتید</option>
                            {availableTeachers.map(t => (
                                <option key={t.id} value={t.id}>
                                    {t.fullname || t.name || `استاد #${t.id}`}
                                </option>
                            ))}
                        </select>
                    </div>

                    {/* View Switcher */}
                    <div className="flex items-center gap-1 bg-gray-100 p-1 rounded-xl">
                        <button
                            onClick={() => setViewMode('table')}
                            className={`px-3 py-1.5 rounded-lg text-xs font-bold transition ${viewMode === 'table' ? 'bg-white shadow text-orange-600' : 'text-gray-600 hover:text-gray-900'}`}
                        >
                            جدول
                        </button>
                        <button
                            onClick={() => setViewMode('grid')}
                            className={`px-3 py-1.5 rounded-lg text-xs font-bold transition ${viewMode === 'grid' ? 'bg-white shadow text-orange-600' : 'text-gray-600 hover:text-gray-900'}`}
                        >
                            کارت‌ها
                        </button>
                    </div>
                </div>
            </div>

            {/* Active Filter Chips */}
            {(selectedOwnerId || selectedTeacherId) && (
                <div className="flex flex-wrap items-center gap-2 mb-6 bg-orange-50/60 p-2.5 rounded-xl border border-orange-200 text-xs">
                    <span className="font-bold text-gray-600">فیلترهای فعال:</span>
                    {selectedOwnerId && (
                        <span className="inline-flex items-center gap-1.5 bg-white text-amber-800 border border-amber-300 px-2.5 py-1 rounded-lg font-bold shadow-xs">
                            <FaUser size={10} className="text-amber-600" />
                            <span>مالک: {selectedOwnerName || `#${selectedOwnerId}`}</span>
                            <button
                                onClick={handleClearOwnerFilter}
                                className="hover:text-red-500 transition mr-1"
                                title="حذف فیلتر مالک"
                            >
                                <FaTimes size={11} />
                            </button>
                        </span>
                    )}
                    {selectedTeacherId && (
                        <span className="inline-flex items-center gap-1.5 bg-white text-purple-800 border border-purple-300 px-2.5 py-1 rounded-lg font-bold shadow-xs">
                            <FaChalkboardTeacher size={11} className="text-purple-600" />
                            <span>استاد: {selectedTeacherName || `#${selectedTeacherId}`}</span>
                            <button
                                onClick={handleClearTeacherFilter}
                                className="hover:text-red-500 transition mr-1"
                                title="حذف فیلتر استاد"
                            >
                                <FaTimes size={11} />
                            </button>
                        </span>
                    )}
                    <button
                        onClick={() => {
                            handleClearOwnerFilter();
                            handleClearTeacherFilter();
                        }}
                        className="text-gray-500 hover:text-red-600 underline font-semibold mr-auto text-[11px]"
                    >
                        پاک کردن همه فیلترها
                    </button>
                </div>
            )}

            {/* Error Banner */}
            {error && (
                <div className="mb-6 p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm flex items-center gap-2">
                    <FaTimes size={16} />
                    <span>{error}</span>
                </div>
            )}

            {/* Main Content Area */}
            {loading ? (
                <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center shadow-sm">
                    <FaSync className="animate-spin text-orange-500 mx-auto text-3xl mb-3" />
                    <p className="text-gray-600 font-bold">در حال بارگذاری لیست جلسات...</p>
                </div>
            ) : meetings.length === 0 ? (
                <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center shadow-sm">
                    <FaCalendarAlt className="text-gray-300 mx-auto text-5xl mb-4" />
                    <h3 className="text-lg font-black text-gray-800 mb-1">هیچ جلسه‌ای یافت نشد</h3>
                    <p className="text-sm text-gray-500">جلسه‌ای با مشخصات و فیلترهای انتخابی در سیستم ثبت نشده است.</p>
                </div>
            ) : viewMode === 'table' ? (
                /* Table View */
                <div className="bg-white rounded-2xl border border-gray-200 shadow-sm overflow-hidden mb-6">
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50/80">
                                <tr>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">شناسه</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">عنوان جلسه و استاد</th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">اتاق</th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600 min-w-[190px]">
                                        نوع جلسه (قابل ویرایش)
                                    </th>
                                    <th className="px-4 py-3.5 text-right text-xs font-black text-gray-600">تاریخ ثبت</th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600 min-w-[280px]">
                                        رسانه‌ها (صوت / ویدیو / شرح)
                                    </th>
                                    <th className="px-4 py-3.5 text-center text-xs font-black text-gray-600">عملیات جلسه</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-100 text-sm">
                                {meetings.map((item) => {
                                    const isUpdatingThisType = !!updatingTypeMap[item.id];
                                    const voiceUrl = getFullMediaUrl(item.voiceUrl);
                                    const videoUrl = getFullMediaUrl(item.videoUrl);
                                    const reportUrl = getFullReportUrl(item.reportUrl);

                                    return (
                                        <tr key={item.id} className="hover:bg-orange-50/30 transition-colors">
                                            {/* شناسه و مالک */}
                                            <td className="px-4 py-4 text-xs">
                                                <div className="flex flex-col gap-1.5 items-start">
                                                    <span className="font-mono font-black text-gray-700 bg-gray-100 px-1.5 py-0.5 rounded text-[11px]">
                                                        #{item.id}
                                                    </span>

                                                    {/* رشته meetID با قابلیت کپی با یک کلیک */}
                                                    {item.meetID && (
                                                        <button
                                                            type="button"
                                                            onClick={() => handleCopyMeetId(item.meetID)}
                                                            className="group flex items-center gap-1 font-mono text-[10px] text-gray-500 hover:text-orange-600 bg-gray-50 hover:bg-orange-50 border border-gray-200 hover:border-orange-300 px-2 py-0.5 rounded transition text-right"
                                                            title="کلیک برای کپی شناسه جلسه (meetID)"
                                                        >
                                                            <FaCopy size={10} className="shrink-0 text-gray-400 group-hover:text-orange-500" />
                                                            <span className="truncate max-w-[110px]" dir="ltr">{item.meetID}</span>
                                                        </button>
                                                    )}

                                                    {/* مالک اتاق با قابلیت کلیک جهت فیلتر */}
                                                    {item.owner && (item.owner.name || item.owner.fullname || item.owner.id) && (
                                                        <button
                                                            type="button"
                                                            onClick={() => handleFilterByOwner(item.owner.id, item.owner.fullname || item.owner.name)}
                                                            className="flex items-center gap-1 text-[11px] font-bold text-amber-800 bg-amber-50 hover:bg-amber-100 border border-amber-200 px-2 py-0.5 rounded-lg transition"
                                                            title={`کلیک برای فیلتر جلسات بر اساس مالک (${item.owner.fullname || item.owner.name || item.owner.id})`}
                                                        >
                                                            <FaUser size={10} className="text-amber-600 shrink-0" />
                                                            <span className="truncate max-w-[110px]">
                                                                {item.owner.fullname || item.owner.name || `مالک #${item.owner.id}`}
                                                            </span>
                                                        </button>
                                                    )}
                                                </div>
                                            </td>

                                            {/* عنوان جلسه و استاد */}
                                            <td className="px-4 py-4">
                                                <div className="flex flex-col gap-1 max-w-xs">
                                                    <span className="font-bold text-gray-900 leading-snug">
                                                        {item.title || 'بدون عنوان'}
                                                    </span>
                                                    {item.teacher && (item.teacher.name || item.teacher.fullname) && (
                                                        <button
                                                            type="button"
                                                            onClick={() => handleFilterByTeacher(item.teacher.id, item.teacher.name || item.teacher.fullname)}
                                                            className="flex items-center gap-1.5 text-xs text-purple-700 hover:text-purple-900 font-medium bg-purple-50 hover:bg-purple-100 border border-purple-200 px-2 py-0.5 rounded-lg w-max transition text-right"
                                                            title={`کلیک برای فیلتر جلسات بر اساس استاد (${item.teacher.name || item.teacher.fullname})`}
                                                        >
                                                            <FaChalkboardTeacher size={12} className="shrink-0" />
                                                            <span>{item.teacher.name || item.teacher.fullname}</span>
                                                        </button>
                                                    )}
                                                </div>
                                            </td>

                                            {/* اتاق */}
                                            <td className="px-4 py-4">
                                                <div className="flex flex-col">
                                                    <span className="font-bold text-gray-800 text-xs">{item.room?.title || '-'}</span>
                                                    {item.room?.friendlyID && (
                                                        <span className="text-[11px] font-mono text-orange-600 bg-orange-50 px-1.5 py-0.5 rounded w-max mt-0.5">
                                                            {item.room.friendlyID}
                                                        </span>
                                                    )}
                                                </div>
                                            </td>

                                            {/* نوع جلسه با دراپ‌داون قابل ویرایش */}
                                            <td className="px-4 py-4 text-center">
                                                <div className="inline-flex items-center gap-1.5 relative">
                                                    <select
                                                        value={item.type ?? 1}
                                                        disabled={isUpdatingThisType}
                                                        onChange={(e) => handleTypeChange(item.id, e.target.value)}
                                                        className={`text-xs font-black px-2.5 py-1.5 rounded-xl border appearance-none pr-7 pl-3 focus:outline-none focus:ring-2 focus:ring-orange-400 cursor-pointer transition shadow-sm ${
                                                            item.type === 100
                                                                ? 'bg-emerald-50 text-emerald-800 border-emerald-300'
                                                                : item.type === 101
                                                                ? 'bg-purple-50 text-purple-800 border-purple-300'
                                                                : item.type === 1
                                                                ? 'bg-blue-50 text-blue-800 border-blue-300'
                                                                : 'bg-gray-100 text-gray-700 border-gray-300'
                                                        }`}
                                                    >
                                                        {MEETING_TYPES.map(opt => (
                                                            <option key={opt.value} value={opt.value}>
                                                                {opt.label}
                                                            </option>
                                                        ))}
                                                    </select>
                                                    {isUpdatingThisType ? (
                                                        <FaSpinner className="animate-spin text-orange-500 absolute right-2 pointer-events-none text-xs" />
                                                    ) : (
                                                        <FaExchangeAlt className="text-gray-400 absolute right-2 pointer-events-none text-[10px]" />
                                                    )}
                                                </div>
                                            </td>

                                            {/* تاریخ ثبت */}
                                            <td className="px-4 py-4 text-gray-600 text-xs whitespace-nowrap">
                                                <div className="font-semibold">{item.jalaliDate || '-'}</div>
                                            </td>

                                            {/* عملیات رسانه‌ها (صوت، ویدیو، شرح جلسه، رکورد کامل) */}
                                            <td className="px-4 py-4">
                                                <div className="grid grid-cols-2 gap-2 min-w-[340px] max-w-[420px]">
                                                    {/* صوت */}
                                                    <div className="flex items-center gap-1.5 w-full justify-between bg-blue-50/50 p-2 rounded-xl border border-blue-100">
                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                            <FaVolumeUp className={item.voiceUrl ? "text-blue-600" : "text-gray-400"} size={13} />
                                                            <span className="text-xs font-bold text-gray-700">صوت:</span>
                                                            {item.voiceUrl ? (
                                                                <>
                                                                    <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${item.voiceType === 103 ? 'bg-purple-100 text-purple-800' : 'bg-blue-100 text-blue-800'}`}>
                                                                        {item.voiceType === 103 ? 'فقط استریم' : 'دانلود'}
                                                                    </span>
                                                                    <a
                                                                        href={voiceUrl}
                                                                        target="_blank"
                                                                        rel="noopener noreferrer"
                                                                        download={item.voiceType !== 103}
                                                                        className="text-[11px] text-blue-700 font-extrabold hover:underline flex items-center gap-0.5"
                                                                        title="پخش یا دانلود صوت"
                                                                    >
                                                                        <FaExternalLinkAlt size={9} />
                                                                    </a>
                                                                </>
                                                            ) : (
                                                                <span className="text-[11px] text-gray-400">ندارد</span>
                                                            )}
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button
                                                                onClick={() => openSoundModal(item)}
                                                                className="px-2 py-1 text-[11px] font-bold rounded-lg bg-blue-600 hover:bg-blue-700 text-white transition shadow-xs flex items-center gap-1"
                                                                title="مدیریت و ثبت فایل صوتی"
                                                            >
                                                                {item.voiceUrl ? <FaEdit size={10} /> : <FaPlus size={9} />}
                                                                <span>{item.voiceUrl ? 'ویرایش' : 'ثبت'}</span>
                                                            </button>
                                                        </div>
                                                    </div>

                                                    {/* ویدیو */}
                                                    <div className="flex items-center gap-1.5 w-full justify-between bg-emerald-50/50 p-2 rounded-xl border border-emerald-100">
                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                            <FaVideo className={item.videoUrl ? "text-emerald-600" : "text-gray-400"} size={13} />
                                                            <span className="text-xs font-bold text-gray-700">ویدیو:</span>
                                                            {item.videoUrl ? (
                                                                <>
                                                                    <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${item.videoType === 103 ? 'bg-purple-100 text-purple-800' : 'bg-emerald-100 text-emerald-800'}`}>
                                                                        {item.videoType === 103 ? 'فقط استریم' : 'دانلود'}
                                                                    </span>
                                                                    <a
                                                                        href={videoUrl}
                                                                        target="_blank"
                                                                        rel="noopener noreferrer"
                                                                        download={item.videoType !== 103}
                                                                        className="text-[11px] text-emerald-700 font-extrabold hover:underline flex items-center gap-0.5"
                                                                        title="مشاهده یا دانلود فیلم"
                                                                    >
                                                                        <FaExternalLinkAlt size={9} />
                                                                    </a>
                                                                </>
                                                            ) : (
                                                                <span className="text-[11px] text-gray-400">ندارد</span>
                                                            )}
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button
                                                                onClick={() => openVideoModal(item)}
                                                                className="px-2 py-1 text-[11px] font-bold rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white transition shadow-xs flex items-center gap-1"
                                                                title="مدیریت و ثبت فیلم جلسه"
                                                            >
                                                                {item.videoUrl ? <FaEdit size={10} /> : <FaPlus size={9} />}
                                                                <span>{item.videoUrl ? 'ویرایش' : 'ثبت'}</span>
                                                            </button>
                                                        </div>
                                                    </div>

                                                    {/* گزارش و شرح جلسه */}
                                                    <div className="flex items-center gap-1.5 w-full justify-between bg-amber-50/50 p-2 rounded-xl border border-amber-100">
                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                            <FaFileAlt className={item.reportUrl ? "text-amber-600" : "text-gray-400"} size={13} />
                                                            <span className="text-xs font-bold text-gray-700">گزارش:</span>
                                                            {item.reportUrl ? (
                                                                <>
                                                                    <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${item.reportType === 102 ? 'bg-amber-100 text-amber-800' : 'bg-purple-100 text-purple-800'}`}>
                                                                        {item.reportType === 102 ? 'دانلود' : 'وب'}
                                                                    </span>
                                                                    <a
                                                                        href={reportUrl}
                                                                        target="_blank"
                                                                        rel="noopener noreferrer"
                                                                        download={item.reportType === 102}
                                                                        className="text-[11px] text-amber-800 font-extrabold hover:underline flex items-center gap-0.5"
                                                                        title="مشاهده یا دانلود گزارش"
                                                                    >
                                                                        <FaExternalLinkAlt size={9} />
                                                                    </a>
                                                                </>
                                                            ) : (
                                                                <span className="text-[11px] text-gray-400">ندارد</span>
                                                            )}
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button
                                                                onClick={() => openReportModal(item)}
                                                                className="px-2 py-1 text-[11px] font-bold rounded-lg bg-amber-600 hover:bg-amber-700 text-white transition shadow-xs flex items-center gap-1"
                                                                title="مدیریت لینک گزارش جلسه"
                                                            >
                                                                {item.reportUrl ? <FaEdit size={10} /> : <FaPlus size={9} />}
                                                                <span>{item.reportUrl ? 'ویرایش' : 'ثبت'}</span>
                                                            </button>
                                                        </div>
                                                    </div>

                                                    {/* رکورد کامل (بیگ‌بلو‌باتن) */}
                                                    <div className="flex items-center gap-1.5 w-full justify-between bg-rose-50/60 p-2 rounded-xl border border-rose-100">
                                                        <div className="flex items-center gap-1.5 flex-wrap">
                                                            <FaRecordVinyl className={item.recordingPlaybackUrl || item.isRecorded ? "text-rose-600 animate-pulse" : "text-gray-400"} size={13} />
                                                            <span className="text-xs font-bold text-gray-700">رکورد:</span>
                                                            {item.recordingPlaybackUrl ? (
                                                                <>
                                                                    <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${item.recordingDownloadable ? 'bg-emerald-100 text-emerald-800' : 'bg-purple-100 text-purple-800'}`}>
                                                                        {item.recordingDownloadable ? 'دانلود' : 'استریم'}
                                                                    </span>
                                                                    <a
                                                                        href={item.recordingPlaybackUrl}
                                                                        target="_blank"
                                                                        rel="noopener noreferrer"
                                                                        className="text-[11px] text-rose-700 font-extrabold hover:underline flex items-center gap-0.5"
                                                                        title="تماشای آنلاین رکورد"
                                                                    >
                                                                        <FaExternalLinkAlt size={9} />
                                                                    </a>
                                                                </>
                                                            ) : item.isRecorded ? (
                                                                <span className="text-[10px] font-bold px-1.5 py-0.5 rounded bg-rose-100 text-rose-800">
                                                                    دارد
                                                                </span>
                                                            ) : (
                                                                <span className="text-[11px] text-gray-400">ندارد</span>
                                                            )}
                                                        </div>
                                                        <div className="flex items-center gap-1">
                                                            <button
                                                                onClick={() => openRecordModal(item)}
                                                                className="px-2 py-1 text-[11px] font-bold rounded-lg bg-rose-600 hover:bg-rose-700 text-white transition shadow-xs flex items-center gap-1"
                                                                title="مشاهده رکورد کامل و تنظیمات دسترسی"
                                                            >
                                                                <FaEye size={10} />
                                                                <span>رکورد</span>
                                                            </button>
                                                        </div>
                                                    </div>
                                                </div>
                                            </td>

                                            {/* عملیات جلسه */}
                                            <td className="px-4 py-4 text-center">
                                                <div className="flex items-center justify-center gap-1.5">
                                                    <button
                                                        onClick={() => openMediaModal(item, 'voice')}
                                                        className="p-2 text-indigo-600 hover:bg-indigo-50 rounded-xl transition"
                                                        title="مدیریت تمام رسانه‌ها (صوت، فیلم، گزارش)"
                                                    >
                                                        <FaCloudUploadAlt size={17} />
                                                    </button>

                                                    {/* دکمه لیست حضار با نشانگر و هاور دقیق */}
                                                    <button
                                                        onClick={() => openAttendeesModal(item)}
                                                        className="relative p-2 text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-xl transition flex items-center justify-center"
                                                        title={`${item.participantsCount || 0} نفر حضار ثبت شده`}
                                                    >
                                                        <FaUsers size={16} />
                                                        <span
                                                            className="absolute -top-1 -right-1 bg-blue-600 text-white text-[9px] font-black px-1.5 py-0.2 rounded-full min-w-[18px] text-center shadow-xs"
                                                            title={`${item.participantsCount || 0} نفر حضار ثبت شده`}
                                                        >
                                                            {item.participantsCount || 0}
                                                        </span>
                                                    </button>

                                                    {item.room?.friendlyID && (
                                                        <button
                                                            onClick={() => navigate(`/rooms/${item.room.friendlyID}`)}
                                                            className="p-2 text-emerald-600 hover:bg-emerald-50 rounded-xl transition"
                                                            title="مشاهده صفحه اتاق"
                                                        >
                                                            <FaDoorOpen size={16} />
                                                        </button>
                                                    )}

                                                    {item.meetID && (
                                                        <button
                                                            onClick={() => navigate(`/polls/${item.meetID}/report`)}
                                                            className="p-2 text-purple-600 hover:bg-purple-50 rounded-xl transition"
                                                            title="گزارش نظرسنجی جلسه"
                                                        >
                                                            <FaChartBar size={15} />
                                                        </button>
                                                    )}

                                                    <button
                                                        onClick={() => {
                                                            setSelectedMeeting(item);
                                                            setShowDetailsModal(true);
                                                        }}
                                                        className="p-2 text-gray-500 hover:text-gray-800 hover:bg-gray-100 rounded-xl transition"
                                                        title="جزئیات بیشتر"
                                                    >
                                                        <FaInfoCircle size={15} />
                                                    </button>
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                </div>
            ) : (
                /* Grid View */
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5 mb-6">
                    {meetings.map((item) => {
                        const isUpdatingThisType = !!updatingTypeMap[item.id];
                        const voiceUrl = getFullMediaUrl(item.voiceUrl);
                        const videoUrl = getFullMediaUrl(item.videoUrl);
                        const reportUrl = getFullReportUrl(item.reportUrl);

                        return (
                            <div key={item.id} className="bg-white rounded-2xl border border-gray-200 p-5 shadow-sm hover:shadow-md transition flex flex-col justify-between">
                                <div>
                                    <div className="flex items-start justify-between gap-2 mb-2">
                                        <h3 className="font-black text-gray-900 text-base leading-snug">{item.title || 'بدون عنوان'}</h3>
                                        <span className="text-xs font-mono font-bold text-gray-400">#{item.id}</span>
                                    </div>

                                    {/* MeetID Copyable */}
                                    {item.meetID && (
                                        <div className="mb-3">
                                            <button
                                                type="button"
                                                onClick={() => handleCopyMeetId(item.meetID)}
                                                className="group flex items-center gap-1 font-mono text-[10px] text-gray-500 hover:text-orange-600 bg-gray-50 hover:bg-orange-50 border border-gray-200 hover:border-orange-300 px-2 py-0.5 rounded transition"
                                                title="کلیک برای کپی شناسه جلسه (meetID)"
                                            >
                                                <FaCopy size={10} className="shrink-0 text-gray-400 group-hover:text-orange-500" />
                                                <span className="truncate max-w-[180px]" dir="ltr">{item.meetID}</span>
                                            </button>
                                        </div>
                                    )}

                                    {/* Type Dropdown */}
                                    <div className="mb-4">
                                        <label className="text-xs font-bold text-gray-500 block mb-1">نوع جلسه:</label>
                                        <div className="relative">
                                            <select
                                                value={item.type ?? 1}
                                                disabled={isUpdatingThisType}
                                                onChange={(e) => handleTypeChange(item.id, e.target.value)}
                                                className="w-full text-xs font-bold p-2 rounded-xl border border-gray-200 bg-gray-50 focus:bg-white focus:ring-2 focus:ring-orange-400 focus:outline-none cursor-pointer"
                                            >
                                                {MEETING_TYPES.map(opt => (
                                                    <option key={opt.value} value={opt.value}>
                                                        {opt.label}
                                                    </option>
                                                ))}
                                            </select>
                                            {isUpdatingThisType && (
                                                <FaSpinner className="animate-spin text-orange-500 absolute left-3 top-3 text-xs" />
                                            )}
                                        </div>
                                    </div>

                                    {/* Info Box */}
                                    <div className="space-y-2 text-xs text-gray-600 mb-4 bg-gray-50 p-3 rounded-xl border border-gray-100">
                                        <div className="flex items-center justify-between">
                                            <span className="text-gray-400">اتاق:</span>
                                            <span className="font-bold text-gray-800">{item.room?.title} ({item.room?.friendlyID})</span>
                                        </div>
                                        {item.owner && (item.owner.name || item.owner.fullname || item.owner.id) && (
                                            <div className="flex items-center justify-between">
                                                <span className="text-gray-400">مالک:</span>
                                                <button
                                                    type="button"
                                                    onClick={() => handleFilterByOwner(item.owner.id, item.owner.fullname || item.owner.name)}
                                                    className="font-bold text-amber-700 hover:underline flex items-center gap-1"
                                                    title={`کلیک برای فیلتر بر اساس مالک (${item.owner.fullname || item.owner.name})`}
                                                >
                                                    <FaUser size={10} />
                                                    <span>{item.owner.fullname || item.owner.name || `مالک #${item.owner.id}`}</span>
                                                </button>
                                            </div>
                                        )}
                                        {item.teacher && (
                                            <div className="flex items-center justify-between">
                                                <span className="text-gray-400">استاد:</span>
                                                <button
                                                    type="button"
                                                    onClick={() => handleFilterByTeacher(item.teacher.id, item.teacher.name || item.teacher.fullname)}
                                                    className="font-bold text-purple-700 hover:underline flex items-center gap-1"
                                                    title={`کلیک برای فیلتر بر اساس استاد (${item.teacher.name || item.teacher.fullname})`}
                                                >
                                                    <FaChalkboardTeacher size={11} />
                                                    <span>{item.teacher.name || item.teacher.fullname}</span>
                                                </button>
                                            </div>
                                        )}
                                        <div className="flex items-center justify-between">
                                            <span className="text-gray-400">تاریخ:</span>
                                            <span className="font-semibold text-gray-800">{item.jalaliDate || '-'}</span>
                                        </div>
                                    </div>

                                    {/* Media Status Buttons */}
                                    <div className="grid grid-cols-3 gap-2 mb-4">
                                        <button
                                            type="button"
                                            onClick={() => openSoundModal(item)}
                                            className={`p-2 rounded-xl text-center border text-xs font-bold transition hover:opacity-90 ${item.voiceUrl ? 'bg-blue-50 text-blue-700 border-blue-200' : 'bg-gray-50 text-gray-400 border-gray-200'}`}
                                            title="مدیریت صوت"
                                        >
                                            <FaVolumeUp className="mx-auto mb-1" />
                                            <span className="block truncate">{item.voiceUrl ? (item.voiceType === 103 ? 'صوت (استریم)' : 'صوت (دانلود)') : 'ثبت صوت'}</span>
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => openVideoModal(item)}
                                            className={`p-2 rounded-xl text-center border text-xs font-bold transition hover:opacity-90 ${item.videoUrl ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-gray-50 text-gray-400 border-gray-200'}`}
                                            title="مدیریت ویدیو"
                                        >
                                            <FaVideo className="mx-auto mb-1" />
                                            <span className="block truncate">{item.videoUrl ? (item.videoType === 103 ? 'فیلم (استریم)' : 'فیلم (دانلود)') : 'ثبت فیلم'}</span>
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => openReportModal(item)}
                                            className={`p-2 rounded-xl text-center border text-xs font-bold transition hover:opacity-90 ${item.reportUrl ? 'bg-amber-50 text-amber-700 border-amber-200' : 'bg-gray-50 text-gray-400 border-gray-200'}`}
                                            title="مدیریت گزارش"
                                        >
                                            <FaFileAlt className="mx-auto mb-1" />
                                            <span className="block truncate">{item.reportUrl ? (item.reportType === 102 ? 'گزارش (فایل)' : 'گزارش (لینک)') : 'ثبت گزارش'}</span>
                                        </button>
                                    </div>
                                </div>

                                {/* Operations Bottom */}
                                <div className="border-t border-gray-100 pt-3 flex items-center justify-between gap-2">
                                    <button
                                        onClick={() => openMediaModal(item, 'voice')}
                                        className="flex-1 py-2 bg-orange-50 hover:bg-orange-100 text-orange-700 rounded-xl text-xs font-bold transition flex items-center justify-center gap-1.5"
                                    >
                                        <FaCloudUploadAlt size={15} />
                                        <span>مدیریت فایل‌ها</span>
                                    </button>
                                    <button
                                        onClick={() => openAttendeesModal(item)}
                                        className="px-3 py-2 bg-blue-50 hover:bg-blue-100 text-blue-700 rounded-xl text-xs transition flex items-center gap-1.5 font-bold"
                                        title={`${item.participantsCount || 0} نفر حضار ثبت شده`}
                                    >
                                        <FaUsers size={14} />
                                        <span>حضار</span>
                                        <span className="bg-blue-600 text-white text-[10px] font-black px-1.5 py-0.2 rounded-full shadow-xs">
                                            {item.participantsCount || 0}
                                        </span>
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}

            {/* Pagination Controls */}
            {!loading && totalPages > 1 && (
                <div className="bg-white p-4 rounded-2xl border border-gray-200 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-sm">
                    <p className="text-xs text-gray-500 font-bold">
                        نمایش صفحه <strong className="text-gray-900">{currentPage}</strong> از <strong className="text-gray-900">{totalPages}</strong> (مجموعاً {totalCount} نشست)
                    </p>

                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                            disabled={currentPage === 1}
                            className="p-2 border border-gray-200 rounded-xl bg-white disabled:opacity-40 text-gray-600 hover:bg-gray-50 transition"
                        >
                            <FaChevronRight size={14} />
                        </button>

                        <div className="flex items-center gap-1">
                            {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                                let pageNum;
                                if (totalPages <= 5) pageNum = i + 1;
                                else if (currentPage <= 3) pageNum = i + 1;
                                else if (currentPage >= totalPages - 2) pageNum = totalPages - 4 + i;
                                else pageNum = currentPage - 2 + i;

                                return (
                                    <button
                                        key={pageNum}
                                        onClick={() => setCurrentPage(pageNum)}
                                        className={`w-8 h-8 rounded-xl text-xs font-black transition ${currentPage === pageNum ? 'bg-orange-500 text-white shadow' : 'border border-gray-200 text-gray-700 hover:bg-gray-50'}`}
                                    >
                                        {pageNum}
                                    </button>
                                );
                            })}
                        </div>

                        <button
                            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                            disabled={currentPage === totalPages}
                            className="p-2 border border-gray-200 rounded-xl bg-white disabled:opacity-40 text-gray-600 hover:bg-gray-50 transition"
                        >
                            <FaChevronLeft size={14} />
                        </button>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* 1. MODAL: مدیریت و آپلود رسانه‌ها (صوت، ویدیو، شرح جلسه) */}
            {/* ==================================================== */}
            {mediaModalOpen && activeMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn" dir="rtl">
                        {/* Header */}
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-6">
                            <div>
                                <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                    <span className="p-2 bg-orange-100 text-orange-600 rounded-xl">
                                        <FaCloudUploadAlt size={18} />
                                    </span>
                                    مدیریت فایل‌ها و رسانه‌های جلسه
                                </h3>
                                <p className="text-xs text-gray-500 mt-1 font-bold">
                                    جلسه: {activeMeeting.title || 'بدون عنوان'} (شناسه: {activeMeeting.id})
                                </p>
                            </div>
                            <button
                                onClick={() => setMediaModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        {/* Tabs */}
                        <div className="flex items-center gap-2 border-b border-gray-100 pb-3 mb-6">
                            <button
                                onClick={() => setMediaTab('voice')}
                                className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-black transition ${mediaTab === 'voice' ? 'bg-blue-600 text-white shadow-md' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
                            >
                                <FaVolumeUp />
                                <span>صوت جلسه</span>
                                {voiceInputUrl && <span className="w-2 h-2 rounded-full bg-emerald-400"></span>}
                            </button>
                            <button
                                onClick={() => setMediaTab('video')}
                                className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-black transition ${mediaTab === 'video' ? 'bg-emerald-600 text-white shadow-md' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
                            >
                                <FaVideo />
                                <span>فیلم جلسه</span>
                                {videoInputUrl && <span className="w-2 h-2 rounded-full bg-emerald-400"></span>}
                            </button>
                            <button
                                onClick={() => setMediaTab('report')}
                                className={`flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-black transition ${mediaTab === 'report' ? 'bg-amber-600 text-white shadow-md' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}
                            >
                                <FaFileAlt />
                                <span>گزارش و شرح جلسه</span>
                                {reportInputUrl && <span className="w-2 h-2 rounded-full bg-emerald-400"></span>}
                            </button>
                        </div>

                        {/* Tab 1: Voice */}
                        {mediaTab === 'voice' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-blue-50/60 rounded-2xl border border-blue-100">
                                    <p className="text-xs text-blue-900 font-extrabold mb-1">آپلود یا ثبت لینک صوت جلسه</p>
                                    <p className="text-xs text-blue-700">
                                        می‌توانید فایل صوتی (MP3, WAV, M4A, OGG) را مستقیماً از سیستم آپلود کرده یا آدرس URL آن را وارد نمایید.
                                    </p>
                                </div>

                                {/* Upload File Option */}
                                <div>
                                    <input
                                        type="file"
                                        ref={voiceFileRef}
                                        accept="audio/*"
                                        onChange={handleVoiceFileUpload}
                                        className="hidden"
                                    />
                                    <button
                                        type="button"
                                        disabled={uploadingVoice}
                                        onClick={() => voiceFileRef.current?.click()}
                                        className="w-full py-4 px-4 border-2 border-dashed border-blue-300 hover:border-blue-500 rounded-2xl bg-blue-50/30 hover:bg-blue-50 transition flex flex-col items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
                                    >
                                        {uploadingVoice ? (
                                            <>
                                                <FaSpinner className="animate-spin text-blue-600 text-2xl" />
                                                <span className="text-xs font-bold text-blue-700">در حال آپلود فایل صوتی به سرور...</span>
                                            </>
                                        ) : (
                                            <>
                                                <FaHeadphones className="text-blue-500 text-2xl" />
                                                <span className="text-xs font-bold text-blue-900">کلیک کنید تا فایل صوتی را انتخاب و آپلود کنید</span>
                                                <span className="text-[11px] text-gray-400">فرمت‌های مجاز: MP3, WAV, OGG, M4A</span>
                                            </>
                                        )}
                                    </button>
                                </div>

                                {/* Direct URL Option */}
                                <div>
                                    <label className="text-xs font-bold text-gray-700 block mb-1.5">یا وارد کردن مستقیم آدرس صوت (URL):</label>
                                    <div className="flex gap-2">
                                        <input
                                            type="text"
                                            value={voiceInputUrl}
                                            onChange={(e) => setVoiceInputUrl(e.target.value)}
                                            placeholder="https://vcfile.hekmateislami.com/..."
                                            dir="ltr"
                                            className="flex-1 px-3 py-2 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
                                        />
                                        {voiceInputUrl && (
                                            <button
                                                type="button"
                                                onClick={() => setVoiceInputUrl('')}
                                                className="px-3 py-2 text-red-500 hover:bg-red-50 rounded-xl text-xs font-bold transition"
                                                title="پاک کردن"
                                            >
                                                <FaTrash />
                                            </button>
                                        )}
                                    </div>
                                </div>

                                {/* Audio Preview */}
                                {voiceInputUrl && (
                                    <div className="p-3 bg-gray-50 rounded-xl border border-gray-200">
                                        <p className="text-xs font-bold text-gray-600 mb-2">پیش‌نمایش صوت:</p>
                                        <audio controls src={voiceInputUrl} className="w-full h-10" />
                                    </div>
                                )}
                            </div>
                        )}

                        {/* Tab 2: Video */}
                        {mediaTab === 'video' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-emerald-50/60 rounded-2xl border border-emerald-100">
                                    <p className="text-xs text-emerald-900 font-extrabold mb-1">آپلود یا ثبت لینک فیلم جلسه</p>
                                    <p className="text-xs text-emerald-700">
                                        می‌توانید فایل ویدیو (MP4, MKV, WEBM) را آپلود کرده یا آدرس بازپخش مستقیم را وارد نمایید.
                                    </p>
                                </div>

                                {/* Upload Video Option */}
                                <div>
                                    <input
                                        type="file"
                                        ref={videoFileRef}
                                        accept="video/*"
                                        onChange={handleVideoFileUpload}
                                        className="hidden"
                                    />
                                    <button
                                        type="button"
                                        disabled={uploadingVideo}
                                        onClick={() => videoFileRef.current?.click()}
                                        className="w-full py-4 px-4 border-2 border-dashed border-emerald-300 hover:border-emerald-500 rounded-2xl bg-emerald-50/30 hover:bg-emerald-50 transition flex flex-col items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
                                    >
                                        {uploadingVideo ? (
                                            <>
                                                <FaSpinner className="animate-spin text-emerald-600 text-2xl" />
                                                <span className="text-xs font-bold text-emerald-700">در حال آپلود فایل ویدیو به سرور...</span>
                                            </>
                                        ) : (
                                            <>
                                                <FaVideo className="text-emerald-500 text-2xl" />
                                                <span className="text-xs font-bold text-emerald-900">کلیک کنید تا فایل فیلم جلسه را انتخاب و آپلود کنید</span>
                                                <span className="text-[11px] text-gray-400">فرمت‌های مجاز: MP4, WEBM, MKV</span>
                                            </>
                                        )}
                                    </button>
                                </div>

                                {/* Direct Video URL Option */}
                                <div>
                                    <label className="text-xs font-bold text-gray-700 block mb-1.5">یا وارد کردن مستقیم آدرس فیلم (URL):</label>
                                    <div className="flex gap-2">
                                        <input
                                            type="text"
                                            value={videoInputUrl}
                                            onChange={(e) => setVideoInputUrl(e.target.value)}
                                            placeholder="https://..."
                                            dir="ltr"
                                            className="flex-1 px-3 py-2 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                                        />
                                        {videoInputUrl && (
                                            <button
                                                type="button"
                                                onClick={() => setVideoInputUrl('')}
                                                className="px-3 py-2 text-red-500 hover:bg-red-50 rounded-xl text-xs font-bold transition"
                                                title="پاک کردن"
                                            >
                                                <FaTrash />
                                            </button>
                                        )}
                                    </div>
                                </div>

                                {/* Video Preview / Link */}
                                {videoInputUrl && (
                                    <div className="p-3 bg-gray-50 rounded-xl border border-gray-200">
                                        <p className="text-xs font-bold text-gray-600 mb-2">پیش‌نمایش ویدیو:</p>
                                        <video controls src={videoInputUrl} className="w-full max-h-48 rounded-lg bg-black" />
                                    </div>
                                )}
                            </div>
                        )}

                        {/* Tab 3: Report & Description Link */}
                        {mediaTab === 'report' && (
                            <div className="space-y-4">
                                <div className="p-4 bg-amber-50/60 rounded-2xl border border-amber-100">
                                    <p className="text-xs text-amber-900 font-extrabold mb-1">لینک گزارش و شرح جلسه (سایت مقصد)</p>
                                    <p className="text-xs text-amber-700">
                                        آدرس اینترنتی صفحه‌ای که گزارش مشروح این جلسه در آن منتشر شده است را وارد نمایید. کاربران در صفحه اتاق با کلیک روی «گزارش و شرح جلسه» مستقیماً به این صفحه هدایت خواهند شد.
                                    </p>
                                </div>

                                <div>
                                    <label className="text-xs font-bold text-gray-700 block mb-1.5">آدرس اینترنتی صفحه گزارش (URL):</label>
                                    <div className="flex gap-2">
                                        <input
                                            type="text"
                                            value={reportInputUrl}
                                            onChange={(e) => setReportInputUrl(e.target.value)}
                                            placeholder="https://example.com/reports/session-123"
                                            dir="ltr"
                                            className="flex-1 px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none font-mono"
                                        />
                                        {reportInputUrl && (
                                            <a
                                                href={getFullReportUrl(reportInputUrl)}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="px-3.5 py-2.5 bg-amber-100 text-amber-800 hover:bg-amber-200 rounded-xl text-xs font-bold transition flex items-center gap-1.5"
                                                title="تست و باز کردن لینک"
                                            >
                                                <span>تست لینک</span>
                                                <FaExternalLinkAlt size={11} />
                                            </a>
                                        )}
                                        {reportInputUrl && (
                                            <button
                                                type="button"
                                                onClick={() => setReportInputUrl('')}
                                                className="px-3 py-2 text-red-500 hover:bg-red-50 rounded-xl text-xs font-bold transition"
                                                title="پاک کردن"
                                            >
                                                <FaTrash />
                                            </button>
                                        )}
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Modal Footer */}
                        <div className="mt-8 flex items-center justify-end gap-3 border-t border-gray-100 pt-5">
                            <button
                                type="button"
                                onClick={() => setMediaModalOpen(false)}
                                className="px-5 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-black transition"
                            >
                                انصراف
                            </button>
                            <button
                                type="button"
                                disabled={savingMedia}
                                onClick={handleSaveAllMedia}
                                className="px-6 py-2.5 bg-gradient-to-r from-orange-500 to-amber-500 hover:from-orange-600 hover:to-amber-600 text-white rounded-xl text-xs font-black transition shadow-md flex items-center gap-2 disabled:opacity-50"
                            >
                                {savingMedia ? (
                                    <>
                                        <FaSpinner className="animate-spin text-sm" />
                                        <span>در حال ذخیره تغییرات...</span>
                                    </>
                                ) : (
                                    <>
                                        <FaSave size={14} />
                                        <span>ذخیره تغییرات رسانه</span>
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* 2. MODAL: جزئیات کامل جلسه */}
            {/* ==================================================== */}
            {showDetailsModal && selectedMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn" dir="rtl">
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4">
                            <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                <FaCalendarAlt className="text-orange-500" />
                                جزئیات نشست شناسه {selectedMeeting.id}
                            </h3>
                            <button
                                onClick={() => setShowDetailsModal(false)}
                                className="text-gray-400 hover:text-gray-600 p-1.5 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        <div className="space-y-3 text-xs text-gray-700">
                            <div className="bg-orange-50/60 p-3 rounded-2xl border border-orange-100">
                                <p className="text-xs text-orange-600 font-bold mb-1">عنوان نشست</p>
                                <p className="font-extrabold text-gray-900 text-sm">{selectedMeeting.title || 'بدون عنوان'}</p>
                            </div>

                            <div className="grid grid-cols-2 gap-3">
                                <div className="bg-gray-50 p-3 rounded-2xl border border-gray-100">
                                    <p className="text-gray-400 font-bold mb-0.5">Meet ID</p>
                                    <p className="font-mono font-bold text-gray-800 text-xs">{selectedMeeting.meetID || '-'}</p>
                                </div>
                                <div className="bg-gray-50 p-3 rounded-2xl border border-gray-100">
                                    <p className="text-gray-400 font-bold mb-0.5">نوع نشست</p>
                                    <p className="font-black text-indigo-700">{selectedMeeting.typeTitle || selectedMeeting.type}</p>
                                </div>
                            </div>

                            <div className="bg-gray-50 p-3 rounded-2xl border border-gray-100 space-y-2">
                                <p className="font-bold text-gray-500 mb-1">اطلاعات اتاق</p>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">عنوان اتاق:</span>
                                    <span className="font-bold text-gray-800">{selectedMeeting.room?.title || '-'}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">شناسه اختصاصی:</span>
                                    <span className="font-mono text-orange-600 bg-orange-50 px-1 rounded">{selectedMeeting.room?.friendlyID || '-'}</span>
                                </div>
                            </div>

                            {/* Media Status */}
                            <div className="bg-gray-50 p-3 rounded-2xl border border-gray-100 space-y-2">
                                <p className="font-bold text-gray-500 mb-1">فایل‌ها و رسانه‌ها</p>
                                <div className="flex justify-between items-center">
                                    <span className="text-gray-400">فایل صوتی:</span>
                                    <span className={`font-bold ${selectedMeeting.voiceUrl ? 'text-blue-600' : 'text-gray-400'}`}>
                                        {selectedMeeting.voiceUrl ? 'ثبت شده' : 'ثبت نشده'}
                                    </span>
                                </div>
                                <div className="flex justify-between items-center">
                                    <span className="text-gray-400">فیلم و ویدیو:</span>
                                    <span className={`font-bold ${selectedMeeting.videoUrl ? 'text-emerald-600' : 'text-gray-400'}`}>
                                        {selectedMeeting.videoUrl ? 'ثبت شده' : 'ثبت نشده'}
                                    </span>
                                </div>
                                <div className="flex justify-between items-center">
                                    <span className="text-gray-400">لینک شرح جلسه:</span>
                                    <span className={`font-bold ${selectedMeeting.reportUrl ? 'text-amber-600' : 'text-gray-400'}`}>
                                        {selectedMeeting.reportUrl ? 'ثبت شده' : 'ثبت نشده'}
                                    </span>
                                </div>
                            </div>
                        </div>

                        <div className="mt-6 flex items-center justify-end gap-2 border-t border-gray-100 pt-4">
                            <button
                                onClick={() => copyJoinLink(selectedMeeting.room?.friendlyID)}
                                className="px-4 py-2 bg-orange-50 hover:bg-orange-100 text-orange-700 rounded-xl text-xs font-bold transition flex items-center gap-1.5"
                            >
                                <FaCopy size={13} /> کپی لینک ورود
                            </button>
                            <button
                                onClick={() => setShowDetailsModal(false)}
                                className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                            >
                                بستن
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* 3. MODAL: مدیریت اختصاصی صوت جلسه */}
            {/* ==================================================== */}
            {soundModalOpen && selectedSoundMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn" dir="rtl">
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4">
                            <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                <span className="p-2 bg-blue-100 text-blue-700 rounded-xl">
                                    <FaVolumeUp size={16} />
                                </span>
                                <span>مدیریت فایل صوتی نشست {selectedSoundMeeting.id}</span>
                            </h3>
                            <button
                                onClick={() => setSoundModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-1.5 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        <div className="space-y-4 text-xs">
                            <div className="bg-blue-50/60 p-3 rounded-2xl border border-blue-100">
                                <p className="font-extrabold text-blue-900 text-sm mb-1">{selectedSoundMeeting.title || 'بدون عنوان'}</p>
                                <p className="text-blue-700 text-xs">اتاق: {selectedSoundMeeting.room?.title || '-'} ({selectedSoundMeeting.room?.friendlyID || '-'})</p>
                            </div>

                            {/* آپلود فایل صوتی */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">انتخاب و آپلود فایل صوتی:</label>
                                <input
                                    type="file"
                                    ref={soundModalFileRef}
                                    accept="audio/*"
                                    onChange={handleSoundFileUpload}
                                    className="hidden"
                                />
                                <button
                                    type="button"
                                    onClick={() => soundModalFileRef.current?.click()}
                                    disabled={uploadingSound}
                                    className="w-full py-3 bg-blue-50 hover:bg-blue-100 text-blue-700 border-2 border-dashed border-blue-300 rounded-2xl font-bold flex items-center justify-center gap-2 transition disabled:opacity-50"
                                >
                                    {uploadingSound ? (
                                        <>
                                            <FaSpinner className="animate-spin text-sm" />
                                            <span>در حال آپلود صوت روی سرور...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaUpload size={14} />
                                            <span>انتخاب فایل صوتی از دستگاه و آپلود</span>
                                        </>
                                    )}
                                </button>
                            </div>

                            {/* آدرس مستقیم فایل صوتی */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">یا وارد کردن آدرس مستقیم فایل صوتی (URL):</label>
                                <input
                                    type="text"
                                    value={soundUrl}
                                    onChange={(e) => setSoundUrl(e.target.value)}
                                    placeholder="https://example.com/audio/meeting-123.mp3"
                                    dir="ltr"
                                    className="w-full px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono"
                                />
                            </div>

                            {/* تعیین نوع لاگ صوت (MEETLOG_DOWNLOADABLE vs MEETLOG_DOWNLOAD_FORBIDDEN) */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">نوع دسترسی و پخش فایل صوتی:</label>
                                <select
                                    value={soundType}
                                    onChange={(e) => setSoundType(parseInt(e.target.value))}
                                    className="w-full px-3.5 py-2.5 text-xs font-bold bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
                                >
                                    <option value={LOG_TYPES.DOWNLOADABLE}>
                                        قابل دانلود مستقیم توسط کاربران (کد ۱۰۲ - MEETLOG_DOWNLOADABLE)
                                    </option>
                                    <option value={LOG_TYPES.DOWNLOAD_FORBIDDEN}>
                                        فقط استریم آنلاین - بدون قابلیت دانلود (کد ۱۰۳ - MEETLOG_DOWNLOAD_FORBIDDEN)
                                    </option>
                                </select>
                                <p className="text-[11px] text-gray-500 mt-1">
                                    {soundType === LOG_TYPES.DOWNLOAD_FORBIDDEN
                                        ? 'کاربران فقط می‌توانند صوت را به صورت آنلاین گوش دهند و دکمه دانلود غیرفعال خواهد بود.'
                                        : 'کاربران امکان دانلود فایل صوتی با لینک مستقیم را خواهند داشت.'}
                                </p>
                            </div>

                            {/* توضیحات / شرح */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">توضیحات تکمیلی (اختیاری):</label>
                                <input
                                    type="text"
                                    value={soundExplain}
                                    onChange={(e) => setSoundExplain(e.target.value)}
                                    placeholder="توضیحاتی در مورد این فایل صوتی..."
                                    className="w-full px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
                                />
                            </div>

                            {/* پیش‌نمایش صوت */}
                            {soundUrl && (
                                <div className="bg-gray-50 p-3 rounded-2xl border border-gray-200">
                                    <p className="text-xs font-bold text-gray-700 mb-2 flex items-center gap-1.5">
                                        <FaHeadphones className="text-blue-600" />
                                        <span>پیش‌نمایش صوت ({soundType === LOG_TYPES.DOWNLOAD_FORBIDDEN ? 'فقط استریم' : 'قابل دانلود'}):</span>
                                    </p>
                                    <audio
                                        controls
                                        controlsList={soundType === LOG_TYPES.DOWNLOAD_FORBIDDEN ? "nodownload" : undefined}
                                        onContextMenu={(e) => soundType === LOG_TYPES.DOWNLOAD_FORBIDDEN && e.preventDefault()}
                                        src={soundUrl}
                                        className="w-full"
                                    />
                                </div>
                            )}
                        </div>

                        {/* دکمه‌های اقدام */}
                        <div className="mt-6 flex items-center justify-between gap-2 border-t border-gray-100 pt-4">
                            {selectedSoundMeeting.voiceUrl ? (
                                <button
                                    type="button"
                                    disabled={savingSound}
                                    onClick={handleDeleteSound}
                                    className="px-3.5 py-2.5 bg-red-50 hover:bg-red-100 text-red-600 rounded-xl text-xs font-bold transition flex items-center gap-1.5 disabled:opacity-50"
                                >
                                    <FaTrash size={12} />
                                    <span>حذف صوت</span>
                                </button>
                            ) : <div></div>}

                            <div className="flex items-center gap-2">
                                <button
                                    type="button"
                                    onClick={() => setSoundModalOpen(false)}
                                    className="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="button"
                                    disabled={savingSound || !soundUrl.trim()}
                                    onClick={handleSaveSound}
                                    className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-2 shadow-md disabled:opacity-50"
                                >
                                    {savingSound ? (
                                        <>
                                            <FaSpinner className="animate-spin text-sm" />
                                            <span>در حال ذخیره...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaSave size={13} />
                                            <span>ذخیره اطلاعات صوت</span>
                                        </>
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* 4. MODAL: مدیریت اختصاصی ویدیو / فیلم جلسه */}
            {/* ==================================================== */}
            {videoModalOpen && selectedVideoMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn" dir="rtl">
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4">
                            <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                <span className="p-2 bg-emerald-100 text-emerald-700 rounded-xl">
                                    <FaVideo size={16} />
                                </span>
                                <span>مدیریت فیلم نشست {selectedVideoMeeting.id}</span>
                            </h3>
                            <button
                                onClick={() => setVideoModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-1.5 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        <div className="space-y-4 text-xs">
                            <div className="bg-emerald-50/60 p-3 rounded-2xl border border-emerald-100">
                                <p className="font-extrabold text-emerald-900 text-sm mb-1">{selectedVideoMeeting.title || 'بدون عنوان'}</p>
                                <p className="text-emerald-700 text-xs">اتاق: {selectedVideoMeeting.room?.title || '-'} ({selectedVideoMeeting.room?.friendlyID || '-'})</p>
                            </div>

                            {/* آپلود فایل ویدیویی */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">انتخاب و آپلود فایل ویدیو:</label>
                                <input
                                    type="file"
                                    ref={videoModalFileRef}
                                    accept="video/*"
                                    onChange={handleVideoFileUploadOnly}
                                    className="hidden"
                                />
                                <button
                                    type="button"
                                    onClick={() => videoModalFileRef.current?.click()}
                                    disabled={uploadingVideoOnly}
                                    className="w-full py-3 bg-emerald-50 hover:bg-emerald-100 text-emerald-700 border-2 border-dashed border-emerald-300 rounded-2xl font-bold flex items-center justify-center gap-2 transition disabled:opacity-50"
                                >
                                    {uploadingVideoOnly ? (
                                        <>
                                            <FaSpinner className="animate-spin text-sm" />
                                            <span>در حال آپلود ویدیو روی سرور...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaUpload size={14} />
                                            <span>انتخاب فایل ویدیو از دستگاه و آپلود</span>
                                        </>
                                    )}
                                </button>
                            </div>

                            {/* آدرس مستقیم فایل ویدیویی */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">یا وارد کردن آدرس مستقیم فایل ویدیو (URL):</label>
                                <input
                                    type="text"
                                    value={videoUrl}
                                    onChange={(e) => setVideoUrl(e.target.value)}
                                    placeholder="https://example.com/video/meeting-123.mp4"
                                    dir="ltr"
                                    className="w-full px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:outline-none font-mono"
                                />
                            </div>

                            {/* تعیین نوع لاگ فیلم (MEETLOG_DOWNLOADABLE vs MEETLOG_DOWNLOAD_FORBIDDEN) */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">نوع دسترسی و پخش ویدیو:</label>
                                <select
                                    value={videoType}
                                    onChange={(e) => setVideoType(parseInt(e.target.value))}
                                    className="w-full px-3.5 py-2.5 text-xs font-bold bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                                >
                                    <option value={LOG_TYPES.DOWNLOADABLE}>
                                        قابل دانلود مستقیم توسط کاربران (کد ۱۰۲ - MEETLOG_DOWNLOADABLE)
                                    </option>
                                    <option value={LOG_TYPES.DOWNLOAD_FORBIDDEN}>
                                        فقط استریم آنلاین - بدون قابلیت دانلود (کد ۱۰۳ - MEETLOG_DOWNLOAD_FORBIDDEN)
                                    </option>
                                </select>
                                <p className="text-[11px] text-gray-500 mt-1">
                                    {videoType === LOG_TYPES.DOWNLOAD_FORBIDDEN
                                        ? 'کاربران فقط می‌توانند فیلم را به صورت آنلاین تماشا کنند و دکمه دانلود غیرفعال خواهد بود.'
                                        : 'کاربران امکان دانلود فایل ویدیو با لینک مستقیم را خواهند داشت.'}
                                </p>
                            </div>

                            {/* توضیحات / شرح */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">توضیحات تکمیلی (اختیاری):</label>
                                <input
                                    type="text"
                                    value={videoExplain}
                                    onChange={(e) => setVideoExplain(e.target.value)}
                                    placeholder="توضیحاتی در مورد این ویدیو..."
                                    className="w-full px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                                />
                            </div>

                            {/* پیش‌نمایش ویدیو */}
                            {videoUrl && (
                                <div className="bg-black/5 p-3 rounded-2xl border border-gray-200">
                                    <p className="text-xs font-bold text-gray-700 mb-2 flex items-center gap-1.5">
                                        <FaVideo className="text-emerald-600" />
                                        <span>پیش‌نمایش ویدیو ({videoType === LOG_TYPES.DOWNLOAD_FORBIDDEN ? 'فقط استریم' : 'قابل دانلود'}):</span>
                                    </p>
                                    <video
                                        controls
                                        controlsList={videoType === LOG_TYPES.DOWNLOAD_FORBIDDEN ? "nodownload" : undefined}
                                        onContextMenu={(e) => videoType === LOG_TYPES.DOWNLOAD_FORBIDDEN && e.preventDefault()}
                                        src={videoUrl}
                                        className="w-full rounded-xl max-h-56 bg-black"
                                    />
                                </div>
                            )}
                        </div>

                        {/* دکمه‌های اقدام */}
                        <div className="mt-6 flex items-center justify-between gap-2 border-t border-gray-100 pt-4">
                            {selectedVideoMeeting.videoUrl ? (
                                <button
                                    type="button"
                                    disabled={savingVideo}
                                    onClick={handleDeleteVideo}
                                    className="px-3.5 py-2.5 bg-red-50 hover:bg-red-100 text-red-600 rounded-xl text-xs font-bold transition flex items-center gap-1.5 disabled:opacity-50"
                                >
                                    <FaTrash size={12} />
                                    <span>حذف فیلم</span>
                                </button>
                            ) : <div></div>}

                            <div className="flex items-center gap-2">
                                <button
                                    type="button"
                                    onClick={() => setVideoModalOpen(false)}
                                    className="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="button"
                                    disabled={savingVideo || !videoUrl.trim()}
                                    onClick={handleSaveVideo}
                                    className="px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-2 shadow-md disabled:opacity-50"
                                >
                                    {savingVideo ? (
                                        <>
                                            <FaSpinner className="animate-spin text-sm" />
                                            <span>در حال ذخیره...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaSave size={13} />
                                            <span>ذخیره اطلاعات فیلم</span>
                                        </>
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
            {/* 5. MODAL: مدیریت اختصاصی لینک گزارش و شرح جلسه */}
            {/* ==================================================== */}
            {reportModalOpen && selectedReportMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-lg w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn" dir="rtl">
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4">
                            <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                <span className="p-2 bg-amber-100 text-amber-800 rounded-xl">
                                    <FaFileAlt size={16} />
                                </span>
                                <span>مدیریت گزارش نشست {selectedReportMeeting.id}</span>
                            </h3>
                            <button
                                onClick={() => setReportModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-1.5 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        <div className="space-y-4 text-xs">
                            <div className="bg-amber-50/60 p-3 rounded-2xl border border-amber-100">
                                <p className="font-extrabold text-amber-900 text-sm mb-1">{selectedReportMeeting.title || 'بدون عنوان'}</p>
                                <p className="text-amber-700 text-xs">اتاق: {selectedReportMeeting.room?.title || '-'} ({selectedReportMeeting.room?.friendlyID || '-'})</p>
                            </div>

                            {/* آدرس گزارش */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">آدرس اینترنتی یا لینک فایل گزارش (URL):</label>
                                <div className="flex gap-2">
                                    <input
                                        type="text"
                                        value={reportUrl}
                                        onChange={(e) => setReportUrl(e.target.value)}
                                        placeholder="https://example.com/reports/meeting-123.pdf یا صفحه وب"
                                        dir="ltr"
                                        className="flex-1 px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none font-mono"
                                    />
                                    {reportUrl && (
                                        <a
                                            href={getFullReportUrl(reportUrl)}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="px-3.5 py-2.5 bg-amber-100 text-amber-800 hover:bg-amber-200 rounded-xl text-xs font-bold transition flex items-center gap-1.5 shrink-0"
                                            title="تست لینک"
                                        >
                                            <span>تست لینک</span>
                                            <FaExternalLinkAlt size={11} />
                                        </a>
                                    )}
                                </div>
                            </div>

                            {/* تعیین نوع گزارش (دانلود مستقیم فایل vs ریدایرکت به صفحه وب) */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">ماهیت و نحوه باز شدن گزارش:</label>
                                <select
                                    value={reportType}
                                    onChange={(e) => setReportType(parseInt(e.target.value))}
                                    className="w-full px-3.5 py-2.5 text-xs font-bold bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none"
                                >
                                    <option value={LOG_TYPES.DOWNLOAD_FORBIDDEN}>
                                        لینک وب‌سایت / ریدایرکت و هدایت به صفحه (کد ۱۰۳ - MEETLOG_DOWNLOAD_FORBIDDEN)
                                    </option>
                                    <option value={LOG_TYPES.DOWNLOADABLE}>
                                        فایل ضمیمه جهت دانلود مستقیم (کد ۱۰۲ - MEETLOG_DOWNLOADABLE)
                                    </option>
                                </select>
                                <p className="text-[11px] text-gray-500 mt-1">
                                    {reportType === LOG_TYPES.DOWNLOADABLE
                                        ? 'لینک به عنوان فایل دانلودی (مانند PDF) در نظر گرفته شده و مستقیماً دانلود می‌شود.'
                                        : 'کاربر با کلیک بر روی لینک مستقیماً به صفحه اینترنتی مقصد ریدایرکت خواهد شد.'}
                                </p>
                            </div>

                            {/* توضیحات / شرح */}
                            <div>
                                <label className="text-xs font-bold text-gray-700 block mb-1.5">توضیحات تکمیلی (اختیاری):</label>
                                <input
                                    type="text"
                                    value={reportExplain}
                                    onChange={(e) => setReportExplain(e.target.value)}
                                    placeholder="شرح مختصری در مورد گزارش جلسه..."
                                    className="w-full px-3.5 py-2.5 text-xs bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-amber-500 focus:outline-none"
                                />
                            </div>
                        </div>

                        {/* دکمه‌های اقدام */}
                        <div className="mt-6 flex items-center justify-between gap-2 border-t border-gray-100 pt-4">
                            {selectedReportMeeting.reportUrl ? (
                                <button
                                    type="button"
                                    disabled={savingReport}
                                    onClick={handleDeleteReport}
                                    className="px-3.5 py-2.5 bg-red-50 hover:bg-red-100 text-red-600 rounded-xl text-xs font-bold transition flex items-center gap-1.5 disabled:opacity-50"
                                >
                                    <FaTrash size={12} />
                                    <span>حذف گزارش</span>
                                </button>
                            ) : <div></div>}

                            <div className="flex items-center gap-2">
                                <button
                                    type="button"
                                    onClick={() => setReportModalOpen(false)}
                                    className="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                                >
                                    انصراف
                                </button>
                                <button
                                    type="button"
                                    disabled={savingReport || !reportUrl.trim()}
                                    onClick={handleSaveReport}
                                    className="px-5 py-2.5 bg-amber-600 hover:bg-amber-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-2 shadow-md disabled:opacity-50"
                                >
                                    {savingReport ? (
                                        <>
                                            <FaSpinner className="animate-spin text-sm" />
                                            <span>در حال ذخیره...</span>
                                        </>
                                    ) : (
                                        <>
                                            <FaSave size={13} />
                                            <span>ذخیره اطلاعات گزارش</span>
                                        </>
                                    )}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* ==================================================== */}
                        {/* ==================================================== */}
            {/* 7. MODAL: مشاهده رکورد کامل و تنظیمات رکورد (BigBlueButton) */}
            {/* ==================================================== */}
            {recordModalOpen && selectedRecordMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-2xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn flex flex-col max-h-[90vh] overflow-y-auto" dir="rtl">
                        {/* Header */}
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4 shrink-0">
                            <div className="flex items-center gap-3">
                                <span className="p-2.5 bg-rose-100 text-rose-700 rounded-xl shadow-xs">
                                    <FaRecordVinyl size={20} />
                                </span>
                                <div>
                                    <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                        <span>مشاهده و مدیریت رکورد کامل جلسه</span>
                                        <span className="text-xs font-bold px-2 py-0.5 rounded-full bg-rose-50 text-rose-700 border border-rose-200">
                                            #{selectedRecordMeeting.id}
                                        </span>
                                    </h3>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        {selectedRecordMeeting.title || 'بدون عنوان'} — اتاق: {selectedRecordMeeting.room?.title || selectedRecordMeeting.roomTitle || '-'}
                                    </p>
                                </div>
                            </div>
                            <button
                                onClick={() => setRecordModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        {/* Meeting Info Pill */}
                        <div className="bg-gray-50 border border-gray-200/80 rounded-2xl p-3.5 mb-5 flex flex-wrap items-center justify-between gap-3 text-xs">
                            <div className="flex items-center gap-2">
                                <span className="text-gray-500 font-bold">شناسه جلسه (MeetID):</span>
                                <code className="bg-white px-2 py-0.5 rounded-lg border border-gray-200 text-rose-700 font-mono font-bold" dir="ltr">
                                    {selectedRecordMeeting.meetID || selectedRecordMeeting.MeetID || '-'}
                                </code>
                            </div>
                            <div className="flex items-center gap-2">
                                <span className="text-gray-500 font-bold">اتاق بیگ‌بلو‌باتن:</span>
                                <code className="bg-white px-2 py-0.5 rounded-lg border border-gray-200 text-gray-700 font-mono" dir="ltr">
                                    {selectedRecordMeeting.bbbMeetID || recordingData?.bbbMeetId || selectedRecordMeeting.room?.friendlyID || '-'}
                                </code>
                            </div>
                            <button
                                onClick={() => openRecordModal(selectedRecordMeeting)}
                                disabled={recordingsLoading}
                                className="text-blue-600 hover:text-blue-800 font-bold flex items-center gap-1 transition"
                                title="استعلام مجدد از سرور بیگ‌بلو‌باتن"
                            >
                                <FaSync className={recordingsLoading ? "animate-spin" : ""} size={11} />
                                <span>استعلام مجدد</span>
                            </button>
                        </div>

                        {/* Loading State */}
                        {recordingsLoading ? (
                            <div className="py-12 flex flex-col items-center justify-center gap-3">
                                <FaSpinner className="animate-spin text-rose-600 text-3xl" />
                                <span className="text-xs font-bold text-gray-600">در حال دریافت وضعیت رکورد از سرور بیگ‌بلو‌باتن (vc.hekmateislami.com)...</span>
                            </div>
                        ) : (
                            <div className="space-y-5">
                                {/* Recordings List / Status */}
                                {(!recordingData || !recordingData.recordings || recordingData.recordings.length === 0) ? (
                                    <div className="bg-amber-50/70 border border-amber-200 rounded-2xl p-5 text-center space-y-2">
                                        <FaInfoCircle className="text-amber-500 text-3xl mx-auto" />
                                        <h4 className="text-sm font-black text-amber-900">هیچ فایلی از رکورد در سرور بیگ‌بلو‌باتن یافت نشد</h4>
                                        <p className="text-xs text-amber-800 leading-relaxed max-w-md mx-auto">
                                            چنانچه این جلسه اخیراً ضبط شده است، لطفاً توجه داشته باشید فرآیند پردازش، ترکیب و انتشار فایل رکورد در بیگ‌بلو‌باتن ممکن است پس از اتمام جلسه چند دقیقه به طول بینجامد.
                                        </p>
                                    </div>
                                ) : (
                                    <div className="space-y-3">
                                        <label className="text-xs font-extrabold text-gray-700 block">فایل‌های رکورد ثبت‌شده در سرور بیگ‌بلو‌باتن:</label>
                                        {recordingData.recordings.map((rec, index) => {
                                            const isSelected = selectedRecordId === rec.recordId || (!selectedRecordId && index === 0);
                                            return (
                                                <div
                                                    key={rec.recordId}
                                                    onClick={() => setSelectedRecordId(rec.recordId)}
                                                    className={`p-4 rounded-2xl border transition cursor-pointer ${isSelected ? 'bg-rose-50/40 border-rose-300 ring-2 ring-rose-200' : 'bg-white border-gray-200 hover:border-gray-300'}`}
                                                >
                                                    <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
                                                        <div className="flex items-center gap-3">
                                                            <div className="w-10 h-10 rounded-xl bg-rose-100 text-rose-600 flex items-center justify-center shrink-0">
                                                                <FaRecordVinyl size={20} />
                                                            </div>
                                                            <div>
                                                                <div className="flex items-center gap-2 flex-wrap">
                                                                    <span className="font-extrabold text-gray-900 text-xs">
                                                                        {rec.name || `رکورد شماره ${index + 1}`}
                                                                    </span>
                                                                    <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${rec.published ? 'bg-emerald-100 text-emerald-800' : 'bg-gray-100 text-gray-700'}`}>
                                                                        {rec.published ? 'منتشر شده' : 'پیش‌نویس / غیرانتشار'}
                                                                    </span>
                                                                    <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-blue-100 text-blue-800">
                                                                        قالب: {rec.playbackFormat || 'presentation'}
                                                                    </span>
                                                                </div>
                                                                <div className="text-[11px] text-gray-500 mt-1 flex items-center gap-3 flex-wrap">
                                                                    <span>مدت زمان: <strong>{rec.formattedDuration || `${rec.durationMinutes} دقیقه`}</strong></span>
                                                                    <span>حجم فایل: <strong>{rec.formattedSize || '-'}</strong></span>
                                                                    {rec.participantsCount > 0 && (
                                                                        <span>تعداد حضار: <strong>{rec.participantsCount} نفر</strong></span>
                                                                    )}
                                                                </div>
                                                            </div>
                                                        </div>

                                                        {/* Primary View Action */}
                                                        {rec.playbackUrl && (
                                                            <div className="flex items-center gap-2 self-end sm:self-center shrink-0">
                                                                <a
                                                                    href={rec.playbackUrl}
                                                                    target="_blank"
                                                                    rel="noopener noreferrer"
                                                                    className="px-4 py-2 bg-gradient-to-r from-rose-600 to-pink-600 hover:from-rose-700 hover:to-pink-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-1.5 shadow-md"
                                                                >
                                                                    <FaPlay size={10} />
                                                                    <span>مشاهده رکورد</span>
                                                                    <FaExternalLinkAlt size={9} />
                                                                </a>
                                                            </div>
                                                        )}
                                                    </div>

                                                    {/* Recording Meta & Actions Bar */}
                                                    <div className="mt-3 pt-3 border-t border-gray-100 flex flex-wrap items-center justify-between gap-2 text-xs">
                                                        <div className="flex items-center gap-2 text-gray-500 font-mono text-[11px]" dir="ltr">
                                                            <span className="text-gray-400 font-sans text-xs">شناسه رکورد:</span>
                                                            <span className="truncate max-w-[200px]" title={rec.recordId}>{rec.recordId}</span>
                                                        </div>

                                                        <div className="flex items-center gap-2 flex-wrap">
                                                            {rec.playbackUrl && (
                                                                <button
                                                                    type="button"
                                                                    onClick={(e) => { e.stopPropagation(); copyRecordingLink(rec.playbackUrl); }}
                                                                    className="px-2.5 py-1 text-gray-600 hover:text-gray-900 bg-gray-100 hover:bg-gray-200 rounded-lg text-xs font-bold transition flex items-center gap-1"
                                                                    title="کپی لینک مستقیم پخش"
                                                                >
                                                                    {copiedPlaybackUrl ? <FaCheck className="text-green-600" size={11} /> : <FaCopy size={11} />}
                                                                    <span>{copiedPlaybackUrl ? 'کپی شد' : 'کپی لینک'}</span>
                                                                </button>
                                                            )}
                                                            <button
                                                                type="button"
                                                                disabled={togglingPublish}
                                                                onClick={(e) => { e.stopPropagation(); handleTogglePublish(rec.recordId, rec.published); }}
                                                                className={`px-2.5 py-1 rounded-lg text-xs font-bold transition flex items-center gap-1 ${rec.published ? 'text-amber-700 bg-amber-50 hover:bg-amber-100' : 'text-emerald-700 bg-emerald-50 hover:bg-emerald-100'}`}
                                                                title="تغییر وضعیت انتشار در بیگ‌بلو‌باتن"
                                                            >
                                                                {rec.published ? <FaEyeSlash size={11} /> : <FaEye size={11} />}
                                                                <span>{rec.published ? 'عدم انتشار' : 'انتشار'}</span>
                                                            </button>
                                                            <button
                                                                type="button"
                                                                disabled={deletingRecordId === rec.recordId}
                                                                onClick={(e) => { e.stopPropagation(); handleDeleteRecording(rec.recordId); }}
                                                                className="px-2.5 py-1 text-red-600 hover:text-red-800 bg-red-50 hover:bg-red-100 rounded-lg text-xs font-bold transition flex items-center gap-1 disabled:opacity-50"
                                                                title="حذف کامل رکورد از سرور"
                                                            >
                                                                {deletingRecordId === rec.recordId ? <FaSpinner className="animate-spin" size={11} /> : <FaTrash size={11} />}
                                                                <span>حذف</span>
                                                            </button>
                                                        </div>
                                                    </div>
                                                </div>
                                            );
                                        })}
                                    </div>
                                )}

                                {/* Settings Section */}
                                <div className="bg-gradient-to-br from-gray-50 to-rose-50/20 border border-gray-200 rounded-2xl p-5 space-y-4">
                                    <div className="flex items-center gap-2 border-b border-gray-200/70 pb-3">
                                        <FaCog className="text-rose-600" size={16} />
                                        <h4 className="text-sm font-black text-gray-900">تنظیمات رکورد</h4>
                                    </div>

                                    {/* 1. امکان تماشای رکورد */}
                                    <div>
                                        <label className="text-xs font-bold text-gray-700 block mb-2">
                                            امکان تماشای رکورد برای:
                                        </label>
                                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                                            {[
                                                { id: 'all', label: 'همه (عمومی)', desc: 'هر کسی می‌تواند ببیند', icon: <FaGlobe size={13} />, color: 'blue' },
                                                { id: 'members', label: 'اعضا', desc: 'فقط کاربران عضو اتاق', icon: <FaUsers size={13} />, color: 'emerald' },
                                                { id: 'admins', label: 'مدیران', desc: 'فقط مدیران و مدرس', icon: <FaShieldAlt size={13} />, color: 'purple' },
                                                { id: 'none', label: 'هیچکس', desc: 'غیرفعال / خصوصی', icon: <FaUserLock size={13} />, color: 'red' },
                                            ].map(opt => {
                                                const isSel = recordViewPermission === opt.id;
                                                return (
                                                    <div
                                                        key={opt.id}
                                                        onClick={() => setRecordViewPermission(opt.id)}
                                                        className={`p-3 rounded-xl border text-center cursor-pointer transition flex flex-col items-center gap-1.5 ${isSel ? 'bg-white border-rose-500 shadow-sm ring-2 ring-rose-200' : 'bg-white/80 border-gray-200 hover:border-gray-300'}`}
                                                    >
                                                        <div className={`p-1.5 rounded-lg ${isSel ? 'bg-rose-100 text-rose-700' : 'bg-gray-100 text-gray-500'}`}>
                                                            {opt.icon}
                                                        </div>
                                                        <span className="font-extrabold text-xs text-gray-800">{opt.label}</span>
                                                        <span className="text-[10px] text-gray-500">{opt.desc}</span>
                                                    </div>
                                                );
                                            })}
                                        </div>
                                    </div>

                                    {/* 2. رکورد قابل دانلود */}
                                    <div className="pt-2">
                                        <label className="text-xs font-bold text-gray-700 block mb-2">
                                            رکورد قابل دانلود:
                                        </label>
                                        <div className="grid grid-cols-2 gap-3">
                                            <div
                                                onClick={() => setRecordIsDownloadable(true)}
                                                className={`p-3 rounded-xl border cursor-pointer transition flex items-center gap-2.5 ${recordIsDownloadable ? 'bg-white border-emerald-500 shadow-sm ring-2 ring-emerald-200' : 'bg-white/80 border-gray-200 hover:border-gray-300'}`}
                                            >
                                                <div className={`p-2 rounded-lg ${recordIsDownloadable ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
                                                    <FaDownload size={14} />
                                                </div>
                                                <div>
                                                    <span className="font-extrabold text-xs text-gray-900 block">باشد (قابل دانلود)</span>
                                                    <span className="text-[10px] text-gray-500">امکان دانلود مستقیم فایل توسط کاربران مجاز</span>
                                                </div>
                                            </div>

                                            <div
                                                onClick={() => setRecordIsDownloadable(false)}
                                                className={`p-3 rounded-xl border cursor-pointer transition flex items-center gap-2.5 ${!recordIsDownloadable ? 'bg-white border-purple-500 shadow-sm ring-2 ring-purple-200' : 'bg-white/80 border-gray-200 hover:border-gray-300'}`}
                                            >
                                                <div className={`p-2 rounded-lg ${!recordIsDownloadable ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-500'}`}>
                                                    <FaPlay size={14} />
                                                </div>
                                                <div>
                                                    <span className="font-extrabold text-xs text-gray-900 block">نباشد (فقط پخش آنلاین)</span>
                                                    <span className="text-[10px] text-gray-500">پخش زنده و محافظت‌شده در پلیر بیگ‌بلو‌باتن</span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}

                        {/* Modal Footer */}
                        <div className="mt-6 pt-4 border-t border-gray-100 flex items-center justify-between shrink-0">
                            <div className="flex items-center gap-2">
                                <button
                                    type="button"
                                    onClick={() => setRecordModalOpen(false)}
                                    className="px-4 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                                >
                                    بستن
                                </button>
                            </div>

                            <button
                                type="button"
                                onClick={handleSaveRecordSettings}
                                disabled={savingRecordSettings || recordingsLoading}
                                className="px-5 py-2.5 bg-gradient-to-r from-rose-600 to-pink-600 hover:from-rose-700 hover:to-pink-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-2 shadow-md disabled:opacity-50"
                            >
                                {savingRecordSettings ? (
                                    <>
                                        <FaSpinner className="animate-spin text-sm" />
                                        <span>در حال ذخیره تنظیمات...</span>
                                    </>
                                ) : (
                                    <>
                                        <FaSave size={13} />
                                        <span>ذخیره تنظیمات رکورد</span>
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* 6. MODAL: لیست حضار جلسه */}
            {/* ==================================================== */}
            {attendeesModalOpen && selectedAttendeesMeeting && (
                <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                    <div className="bg-white rounded-3xl max-w-4xl w-full p-6 sm:p-8 shadow-2xl border border-gray-100 relative animate-fadeIn flex flex-col max-h-[90vh]" dir="rtl">
                        {/* Header */}
                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4 shrink-0">
                            <div className="flex items-center gap-3">
                                <span className="p-2.5 bg-blue-100 text-blue-700 rounded-xl">
                                    <FaUsers size={20} />
                                </span>
                                <div>
                                    <h3 className="text-lg font-black text-gray-900 flex items-center gap-2">
                                        <span>لیست حضار جلسه #{selectedAttendeesMeeting.id}</span>
                                        <span className="text-xs font-bold px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 border border-blue-200">
                                            {attendeesList.length} نفر حضور یافته
                                        </span>
                                    </h3>
                                    <p className="text-xs text-gray-500 mt-0.5">
                                        {selectedAttendeesMeeting.title || 'بدون عنوان'} — اتاق: {selectedAttendeesMeeting.room?.title || '-'} ({selectedAttendeesMeeting.room?.friendlyID || '-'})
                                    </p>
                                </div>
                            </div>
                            <button
                                onClick={() => setAttendeesModalOpen(false)}
                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                            >
                                <FaTimes size={18} />
                            </button>
                        </div>

                        {/* Search & Filter Bar inside Modal */}
                        <div className="flex flex-col sm:flex-row items-center gap-3 mb-4 shrink-0">
                            <div className="relative flex-1 w-full">
                                <input
                                    type="text"
                                    placeholder="جستجو بر اساس نام، شناسه، کد ملی، موبایل، ایمیل..."
                                    value={attendeesSearch}
                                    onChange={(e) => setAttendeesSearch(e.target.value)}
                                    className="w-full pr-9 pl-4 py-2 bg-gray-50 border border-gray-200 rounded-xl text-xs focus:bg-white focus:ring-2 focus:ring-blue-500 focus:outline-none"
                                />
                                <FaSearch className="absolute right-3 top-2.5 text-gray-400 text-xs" />
                            </div>

                            <div className="flex items-center gap-1 bg-gray-100 p-1 rounded-xl shrink-0">
                                <button
                                    onClick={() => setAttendeesRoleFilter('all')}
                                    className={`px-3 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'all' ? 'bg-white shadow text-blue-600' : 'text-gray-600'}`}
                                >
                                    همه ({attendeesList.length})
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('teacher')}
                                    className={`px-2.5 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'teacher' ? 'bg-white shadow text-purple-600' : 'text-gray-600'}`}
                                >
                                    اساتید ({attendeesList.filter(a => a.isTeacher || (a.role || '').toLowerCase().includes('teach') || a.role === 'Teacher' || a.role === 'استاد').length})
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('vip')}
                                    className={`px-2.5 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'vip' ? 'bg-white shadow text-amber-600' : 'text-gray-600'}`}
                                >
                                    عضو مجمع VIP ({attendeesList.filter(a => !!a.isMemberVIP).length})
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('cert')}
                                    className={`px-2.5 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'cert' ? 'bg-white shadow text-emerald-600' : 'text-gray-600'}`}
                                >
                                    دارای گواهی ({attendeesList.filter(a => !!(a.hasTakenCertificate || a.isCertificated || a.certificateURL)).length})
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('account')}
                                    className={`px-2.5 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'account' ? 'bg-white shadow text-indigo-600' : 'text-gray-600'}`}
                                >
                                    دارای حساب ({attendeesList.filter(a => !!(a.hasAccount || (a.email && a.email.trim()))).length})
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('moderator')}
                                    className={`px-3 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'moderator' ? 'bg-white shadow text-purple-600' : 'text-gray-600'}`}
                                >
                                    اساتید / مدیران
                                </button>
                                <button
                                    onClick={() => setAttendeesRoleFilter('viewer')}
                                    className={`px-3 py-1 rounded-lg text-xs font-bold transition ${attendeesRoleFilter === 'viewer' ? 'bg-white shadow text-blue-600' : 'text-gray-600'}`}
                                >
                                    شرکت‌کنندگان
                                </button>
                            </div>
                        </div>

                        {/* Attendees Content Area */}
                        <div className="flex-1 overflow-y-auto min-h-0 border border-gray-100 rounded-2xl">
                            {loadingAttendees ? (
                                <div className="p-12 text-center">
                                    <FaSync className="animate-spin text-blue-600 mx-auto text-3xl mb-3" />
                                    <p className="text-gray-600 text-xs font-bold">در حال دریافت لیست حضار...</p>
                                </div>
                            ) : filteredAttendees.length === 0 ? (
                                <div className="p-12 text-center">
                                    <FaUsers className="text-gray-300 mx-auto text-4xl mb-3" />
                                    <p className="text-gray-600 text-sm font-bold">حضوری با این مشخصات یافت نشد</p>
                                    <p className="text-gray-400 text-xs mt-1">تعداد کل حضار ثبت شده: {attendeesList.length} نفر</p>
                                </div>
                            ) : (
                                <table className="min-w-full divide-y divide-gray-100 text-xs">
                                    <thead className="bg-gray-50/80 sticky top-0">
                                        <tr>
                                            <th className="px-3 py-3 text-right font-black text-gray-600">ردیف</th>
                                            <th className="px-3 py-3 text-right font-black text-gray-600">نام شرکت‌کننده</th>
                                            <th className="px-3 py-3 text-right font-black text-gray-600">تگ‌ها و وضعیت</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">مدت زمان حضور</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">زمان ورود</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">زمان خروج</th>
                                            <th className="px-3 py-3 text-center font-black text-gray-600">عملیات گواهینامه</th>
                                        </tr>
                                    </thead>
                                    <tbody className="divide-y divide-gray-100 bg-white">
                                        {filteredAttendees.map((att, idx) => {
                                            const roleStr = (att.role || '').toLowerCase();
                                            const isTeacher = att.isTeacher || roleStr.includes('teach') || att.role === 'Teacher' || att.role === 'استاد';
                                            const isVip = !!att.isMemberVIP;
                                            const hasAccount = !!(att.hasAccount || (att.email && att.email.trim()));
                                            const hasCert = !!(att.hasTakenCertificate || att.isCertificated || att.certificateURL);
                                            const isMod = roleStr.includes('mod') || roleStr.includes('present') || isTeacher;
                                            return (
                                                <tr key={att.id || idx} className="hover:bg-blue-50/30 transition-colors">
                                                    <td className="px-3 py-3 font-mono font-bold text-gray-400">{idx + 1}</td>
                                                    <td className="px-3 py-3">
                                                        <div className="flex items-center gap-2">
                                                            <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold text-xs shrink-0 ${isTeacher ? 'bg-purple-100 text-purple-700' : isVip ? 'bg-amber-100 text-amber-800' : 'bg-blue-100 text-blue-700'}`}>
                                                                {isTeacher ? <FaChalkboardTeacher size={13} /> : <FaUser size={11} />}
                                                            </div>
                                                            <div>
                                                                <span className="font-extrabold text-gray-900 block">{att.name || 'بدون نام'}</span>
                                                                <div className="flex items-center gap-2 mt-0.5 text-[10px] text-gray-500 font-mono flex-wrap">
                                                                    {att.id && <span>شناسه: {att.id}</span>}
                                                                    {att.phone && <span>موبایل: {att.phone}</span>}
                                                                    {att.nationalCode && <span>کدملی: {att.nationalCode}</span>}
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </td>
                                                    <td className="px-3 py-3">
                                                        <div className="flex flex-wrap items-center gap-1.5">
                                                            {isTeacher && (
                                                                <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-purple-100 text-purple-800 border border-purple-200 flex items-center gap-1">
                                                                    <FaChalkboardTeacher size={10} />
                                                                    <span>استاد</span>
                                                                </span>
                                                            )}
                                                            {isVip && (
                                                                <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-900 border border-amber-300 flex items-center gap-1" title="عضو مجمع عالی حکمت اسلامی (VIP)">
                                                                    <FaAward size={10} className="text-amber-600" />
                                                                    <span>عضو مجمع عالی</span>
                                                                </span>
                                                            )}
                                                            {hasAccount ? (
                                                                <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-blue-50 text-blue-800 border border-blue-200 flex items-center gap-1" title={att.email ? `ایمیل: ${att.email}` : "دارای حساب کاربری ثبت‌شده"}>
                                                                    <FaUserCheck size={10} className="text-blue-600" />
                                                                    <span>دارای حساب کاربری</span>
                                                                </span>
                                                            ) : (
                                                                <span className="px-1.5 py-0.5 rounded-full text-[10px] font-medium bg-gray-100 text-gray-500 border border-gray-200">
                                                                    مهمان
                                                                </span>
                                                            )}
                                                            {hasCert && (
                                                                <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 border border-emerald-200 flex items-center gap-1" title="گواهینامه این جلسه صادر شده است">
                                                                    <FaCheckCircle size={10} className="text-emerald-600" />
                                                                    <span>گواهینامه دریافت کرده</span>
                                                                </span>
                                                            )}
                                                        </div>
                                                        {false && isMod ? (
                                                            <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-purple-100 text-purple-800 border border-purple-200">
                                                                {att.role === 'Teacher' ? 'استاد' : 'ارائه‌دهنده / مدیر'}
                                                            </span>
                                                        ) : (
                                                            <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-blue-50 text-blue-700 border border-blue-200">
                                                                {att.role === 'VIEWER' ? 'شرکت‌کننده' : (att.role || 'شرکت‌کننده')}
                                                            </span>
                                                        )}
                                                    </td>
                                                    <td className="px-3 py-3 text-center font-semibold text-gray-700">
                                                        {formatDuration(att.duration || att.timer)}
                                                    </td>
                                                    <td className="px-3 py-3 text-center text-gray-500 font-mono text-[11px]" dir="ltr">
                                                        {att.enteredAt || '-'}
                                                    </td>
                                                    <td className="px-3 py-3 text-center text-gray-500 font-mono text-[11px]" dir="ltr">
                                                        {att.leftAt || '-'}
                                                    </td>
                                                    <td className="px-3 py-3 text-center">
                                                        <div className="flex items-center justify-center gap-1.5">
                                                            {hasCert && att.certificateURL && (
                                                                <a
                                                                    href={att.certificateURL}
                                                                    target="_blank"
                                                                    rel="noopener noreferrer"
                                                                    className="p-1.5 bg-emerald-50 hover:bg-emerald-100 text-emerald-800 border border-emerald-200 rounded-lg text-xs font-bold transition flex items-center gap-1"
                                                                    title="دانلود فایل PDF گواهینامه"
                                                                >
                                                                    <FaDownload size={11} />
                                                                    <span className="hidden sm:inline">دانلود</span>
                                                                </a>
                                                            )}
                                                            <button
                                                                type="button"
                                                                onClick={() => handleOpenCertModal(att)}
                                                                className={`px-2.5 py-1.5 rounded-lg text-xs font-bold transition flex items-center gap-1 shadow-sm ${
                                                                    hasCert
                                                                        ? 'bg-amber-50 hover:bg-amber-100 text-amber-800 border border-amber-300'
                                                                        : 'bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white'
                                                                }`}
                                                                title={hasCert ? "صدور مجدد گواهینامه" : "صدور گواهینامه حضور در این جلسه"}
                                                            >
                                                                <FaAward size={12} />
                                                                <span>{hasCert ? 'صدور مجدد' : 'صدور گواهی'}</span>
                                                            </button>
                                                        </div>
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                    </tbody>
                                </table>
                            )}
                        </div>

                        {/* Footer */}
                        <div className="mt-4 pt-3 border-t border-gray-100 flex items-center justify-between shrink-0">
                            <span className="text-xs text-gray-500 font-bold">
                                نمایش {filteredAttendees.length} نفر از مجموع {attendeesList.length} نفر
                            </span>

                            {/* ==================================================== */}
                            {/* 7. MODAL: صدور گواهینامه حضور در جلسه */}
                            {/* ==================================================== */}
                            {certModalOpen && selectedAttendeeForCert && selectedAttendeesMeeting && (
                                <div className="fixed inset-0 z-[60] bg-black/60 backdrop-blur-sm flex items-center justify-center p-4">
                                    <div className="bg-white rounded-3xl max-w-xl w-full p-6 sm:p-7 shadow-2xl border border-gray-100 relative animate-fadeIn flex flex-col max-h-[90vh]" dir="rtl">
                                        {/* Header */}
                                        <div className="flex items-center justify-between border-b border-gray-100 pb-4 mb-4 shrink-0">
                                            <div className="flex items-center gap-3">
                                                <span className="p-2.5 bg-amber-100 text-amber-800 rounded-xl shadow-sm">
                                                    <FaAward size={22} />
                                                </span>
                                                <div>
                                                    <h3 className="text-base sm:text-lg font-black text-gray-900">
                                                        صدور گواهینامه حضور در جلسه
                                                    </h3>
                                                    <p className="text-xs text-amber-800 font-bold mt-0.5">
                                                        برای: {selectedAttendeesMeeting.title || 'فضای مجازی مجمع عالی حکمت اسلامی'} ({selectedAttendeesMeeting.id})
                                                    </p>
                                                </div>
                                            </div>
                                            <button
                                                onClick={() => setCertModalOpen(false)}
                                                className="text-gray-400 hover:text-gray-600 p-2 rounded-xl hover:bg-gray-100 transition"
                                            >
                                                <FaTimes size={18} />
                                            </button>
                                        </div>

                                        {/* Modal Body */}
                                        <div className="flex-1 overflow-y-auto space-y-4 text-xs pr-1">
                                            {/* Identity Warning Alert if NationalCode or Phone missing */}
                                            {(!certNationalCode || !certPhone) ? (
                                                <div className="p-3.5 bg-red-50 border border-red-200 rounded-2xl flex items-start gap-3">
                                                    <FaExclamationTriangle className="text-red-500 mt-0.5 shrink-0" size={17} />
                                                    <div className="text-xs text-red-800 leading-relaxed">
                                                        <strong className="block font-black mb-1">عدم امکان صدور گواهینامه بدون کد ملی و موبایل</strong>
                                                        کد ملی و شماره موبایل این شرکت‌کننده در سیستم تکمیل نشده است. طبق ضوابط سامانه، جهت صدور گواهینامه معتبر، ثبت هر دوی کد ملی و شماره موبایل الزامی می‌باشد. لطفاً مقادیر را در فیلدهای زیر تکمیل فرمایید.
                                                    </div>
                                                </div>
                                            ) : (
                                                <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-2xl flex items-center justify-between text-xs text-emerald-800">
                                                    <div className="flex items-center gap-2">
                                                        <FaCheckCircle className="text-emerald-600 shrink-0" size={15} />
                                                        <span className="font-bold">مشخصات هویتی کاربر جهت صدور گواهی تأیید است.</span>
                                                    </div>
                                                    <div className="flex items-center gap-3 font-mono font-bold text-[11px]">
                                                        <span>کد ملی: {certNationalCode}</span>
                                                        <span>موبایل: {certPhone}</span>
                                                    </div>
                                                </div>
                                            )}

                                            {/* Form fields */}
                                            <div className="bg-gray-50 p-4 rounded-2xl border border-gray-200 space-y-3">
                                                <div>
                                                    <label className="block text-gray-700 font-bold mb-1">نام و نام خانوادگی در گواهینامه:</label>
                                                    <input
                                                        type="text"
                                                        value={certName}
                                                        onChange={(e) => setCertName(e.target.value)}
                                                        placeholder="نام و نام خانوادگی شرکت‌کننده..."
                                                        className="w-full px-3 py-2 bg-white border border-gray-300 rounded-xl text-xs font-bold text-gray-800 focus:outline-none focus:ring-2 focus:ring-amber-500"
                                                    />
                                                </div>

                                                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                                                    <div>
                                                        <label className="block text-gray-700 font-bold mb-1">
                                                            کد ملی: <span className="text-red-500">*</span>
                                                        </label>
                                                        <input
                                                            type="text"
                                                            dir="ltr"
                                                            value={certNationalCode}
                                                            onChange={(e) => setCertNationalCode(e.target.value)}
                                                            placeholder="۱۰ رقم کد ملی"
                                                            className={`w-full px-3 py-2 bg-white border rounded-xl text-xs font-mono font-bold focus:outline-none focus:ring-2 focus:ring-amber-500 ${
                                                                !certNationalCode ? 'border-red-300 bg-red-50/30' : 'border-gray-300'
                                                            }`}
                                                        />
                                                    </div>

                                                    <div>
                                                        <label className="block text-gray-700 font-bold mb-1">
                                                            شماره موبایل: <span className="text-red-500">*</span>
                                                        </label>
                                                        <input
                                                            type="text"
                                                            dir="ltr"
                                                            value={certPhone}
                                                            onChange={(e) => setCertPhone(e.target.value)}
                                                            placeholder="۰۹..."
                                                            className={`w-full px-3 py-2 bg-white border rounded-xl text-xs font-mono font-bold focus:outline-none focus:ring-2 focus:ring-amber-500 ${
                                                                !certPhone ? 'border-red-300 bg-red-50/30' : 'border-gray-300'
                                                            }`}
                                                        />
                                                    </div>
                                                </div>

                                                <div>
                                                    <label className="block text-gray-700 font-bold mb-1">جنسیت (عنوان در متن گواهی):</label>
                                                    <div className="flex items-center gap-4">
                                                        <label className="flex items-center gap-2 cursor-pointer text-xs font-bold text-gray-800">
                                                            <input
                                                                type="radio"
                                                                name="certGender"
                                                                checked={certIsMale === true}
                                                                onChange={() => setCertIsMale(true)}
                                                                className="text-amber-600 focus:ring-amber-500"
                                                            />
                                                            <span>آقا (جناب آقای)</span>
                                                        </label>
                                                        <label className="flex items-center gap-2 cursor-pointer text-xs font-bold text-gray-800">
                                                            <input
                                                                type="radio"
                                                                name="certGender"
                                                                checked={certIsMale === false}
                                                                onChange={() => setCertIsMale(false)}
                                                                className="text-amber-600 focus:ring-amber-500"
                                                            />
                                                            <span>خانم (سرکار خانم)</span>
                                                        </label>
                                                    </div>
                                                </div>
                                            </div>

                                            {/* If already has certificate or newly issued link */}
                                            {issuedCertLink && issuedCertLink.startsWith('http') && (
                                                <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-2xl flex items-center justify-between">
                                                    <div className="flex items-center gap-2 text-emerald-800 font-bold">
                                                        <FaCheckCircle className="text-emerald-600" size={15} />
                                                        <span>فایل گواهینامه آماده دریافت است:</span>
                                                    </div>
                                                    <a
                                                        href={issuedCertLink}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="px-3.5 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-1.5 shadow"
                                                    >
                                                        <FaDownload size={12} />
                                                        <span>دانلود گواهی صادرشده</span>
                                                    </a>
                                                </div>
                                            )}
                                        </div>

                                        {/* Modal Footer */}
                                        <div className="mt-5 pt-3 border-t border-gray-100 flex items-center justify-between shrink-0">
                                            <button
                                                type="button"
                                                onClick={() => setCertModalOpen(false)}
                                                className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                                            >
                                                انصراف
                                            </button>

                                            <button
                                                type="button"
                                                disabled={!certNationalCode || !certPhone || issuingCert}
                                                onClick={handleIssueCertificateForAttendee}
                                                className="px-5 py-2.5 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white rounded-xl text-xs font-bold transition flex items-center gap-2 shadow disabled:opacity-50 disabled:cursor-not-allowed"
                                            >
                                                {issuingCert ? (
                                                    <>
                                                        <FaSpinner className="animate-spin text-sm" />
                                                        <span>در حال صدور گواهینامه...</span>
                                                    </>
                                                ) : (
                                                    <>
                                                        <FaAward size={14} />
                                                        <span>{issuedCertLink ? 'صدور مجدد گواهینامه' : 'تأیید و صدور گواهینامه'}</span>
                                                    </>
                                                )}
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            )}
                            <span className="text-xs text-gray-500 font-bold">
                            </span>
                            <button
                                type="button"
                                onClick={() => setAttendeesModalOpen(false)}
                                className="px-5 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-xl text-xs font-bold transition"
                            >
                                بستن
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default MeetingManagement;
