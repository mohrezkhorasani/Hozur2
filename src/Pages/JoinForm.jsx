import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { joinMeeting, VerifyToken, SendOTPConfirmation, ConfirmOTP, VerifyCertificateUser, ConfirmCertVerification, SendOTPRoomSubscribedUser, ConfirmOTPRoomSubscribedUser, DirectRoomSubscribedRegistration } from '@/api/StartNewMeeting';
import { fetchRoom, fetchRoomLivesPublic, fetchRoomLivesPublicWithID, GetReservation, NewReservation, GetRoomDetails } from '../api/GetRooms';
import Cookies from 'js-cookie';
import { useAuth } from '../Tools/AuthProvider';
import { useToast } from '../Tools/ToastProvider';
import {
  setCookieWithConfig,
  syncUserTokens,
  removeAuthToken,
  removeRegisteredUserToken,
  getEffectiveUserToken
} from '../api/apiNeed';

// Helper to read a cookie by name
function getCookie(name) {
  const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
  return match ? decodeURIComponent(match[2]) : null;
}

// Helper to set a cookie with expiration in days
function setCookie(name, value, days = 30) {
  const expires = new Date();
  expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
  document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=/`;
}

// Helper to convert Persian/Arabic digits to English digits
function toEnglishDigits(str) {
  if (!str) return '';
  return String(str)
    .replace(/[۰-۹]/g, (d) => '۰۱۲۳۴۵۶۷۸۹'.indexOf(d))
    .replace(/[٠-٩]/g, (d) => '٠١٢٣٤٥٦٧٨٩'.indexOf(d));
}

function JoinForm() {
  const { Mid } = useParams();
  const toast = useToast();
  const navigate = useNavigate();
  const { user, loadingAuth } = useAuth();

  // --- Meeting and Room States ---
  const [displayName, setDisplayName] = useState('');
  const [loading, setLoading] = useState(false);
  const [meet, setMeet] = useState('');
  const [roomDetails, setRoomDetails] = useState(null);
  const [isSubscribedByUser, setIsSubscribedByUser] = useState(null);
  const [subscriptionStatus, setSubscriptionStatus] = useState(null);
  const [reservation, setReservation] = useState(null);
  const [roomLive, setRoomLive] = useState([]);
  const [isMeetingLive, setIsMeetingLive] = useState(false);
  const [nextSessionInfo, setNextSessionInfo] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);

  // --- Reservation states (when meeting is not live) ---
  const [reservationName, setReservationName] = useState('');
  const [reservationPhone, setReservationPhone] = useState('');
  const [reservationNationalCode, setReservationNationalCode] = useState('');
  const [reservationLoading, setReservationLoading] = useState(false);
  const [showReservationForm, setShowReservationForm] = useState(false);

  // --- Certificate info states (when meeting is live) ---
  const [showCertForm, setShowCertForm] = useState(false);
  const [certPhone, setCertPhone] = useState('');
  const [certNationalCode, setCertNationalCode] = useState('');
  const [wantCertificate, setWantCertificate] = useState(false);
  const [certInfoSaved, setCertInfoSaved] = useState(false);
  const [nameError, setNameError] = useState('');

  // --- Certificate OTP verification states (for non-member attendees) ---
  const [isCertOtpSent, setIsCertOtpSent] = useState(false);
  const [certOtpCode, setCertOtpCode] = useState('');
  const [certOtpGuid, setCertOtpGuid] = useState('');
  const [certOtpTimer, setCertOtpTimer] = useState(0);
  const [certOtpLoading, setCertOtpLoading] = useState(false);
  const [certVerifyError, setCertVerifyError] = useState('');
  const [certVerificationLoading, setCertVerificationLoading] = useState(false);
  const [certMaskedPhone, setCertMaskedPhone] = useState('');

  // --- Member verification states (VIP Assembly Members) ---
  const [isMember, setIsMember] = useState(false);
  const [memberNationalCode, setMemberNationalCode] = useState('');
  const [maskedPhone, setMaskedPhone] = useState('');
  const [otpCode, setOtpCode] = useState('');
  const [otpGuid, setOtpGuid] = useState('');
  const [isOtpSent, setIsOtpSent] = useState(false);
  const [isOtpVerified, setIsOtpVerified] = useState(false);
  const [otpTimer, setOtpTimer] = useState(0);
  const [otpLoading, setOtpLoading] = useState(false);
  const [memberVerifyError, setMemberVerifyError] = useState('');
  const [memberVerificationLoading, setMemberVerificationLoading] = useState(false);
  const [isMemberModeActive, setIsMemberModeActive] = useState(false);

  // --- Subscribed Room Verification states ---
  const [subNationalCode, setSubNationalCode] = useState('');
  const [isSubOtpSent, setIsSubOtpSent] = useState(false);
  const [subOtpCode, setSubOtpCode] = useState('');
  const [subOtpGuid, setSubOtpGuid] = useState('');
  const [subOtpLoading, setSubOtpLoading] = useState(false);
  const [subVerifyError, setSubVerifyError] = useState('');
  const [subVerificationLoading, setSubVerificationLoading] = useState(false);
  const [subMaskedPhone, setSubMaskedPhone] = useState('');

  // --- Initialize default values from cookies or auth context ---
  useEffect(() => {
    const defaultName = (user && user.fullname) || getCookie('joinName') || '';
    const defaultPhone = (user && user.phone) || getCookie('joinPhone') || '';
    const defaultNationalCode = (user && user.nationalCode) || getCookie('joinNationalCode') || '';
    const defaultWantCert = getCookie('wantCertificate') === 'true';
    const defaultCertPhone = getCookie('certPhone') || '';
    const defaultCertNationalCode = getCookie('certNationalCode') || '';

    if (defaultName && !displayName) setDisplayName(defaultName);
    if (defaultPhone) setReservationPhone(defaultPhone);
    if (defaultNationalCode) {
      setReservationNationalCode(defaultNationalCode);
      setMemberNationalCode(defaultNationalCode);
    }
    if (defaultWantCert) setWantCertificate(true);
    if (defaultCertPhone) setCertPhone(defaultCertPhone);
    if (defaultCertNationalCode) setCertNationalCode(defaultCertNationalCode);
    if (defaultWantCert && defaultCertPhone && defaultCertNationalCode) setCertInfoSaved(true);
  }, [user]);

  // Handle logged-in VIP member automatically
  useEffect(() => {
    if (user != null && user.isVIPMember) {
      setIsMember(true);
      setDisplayName(user.fullname);
      setIsMemberModeActive(true);
      setIsOtpVerified(true);
      if (user.nationalCode) {
        setMemberNationalCode(user.nationalCode);
        setCertNationalCode(user.nationalCode);
      }
      if (user.phone || user.phoneNumber) {
        setCertPhone(user.phone || user.phoneNumber);
      }
      setWantCertificate(true);
      setCertInfoSaved(true);
    }
  }, [user]);

  // When member OTP is verified, automatically check and lock certificate info
  useEffect(() => {
    if (isOtpVerified) {
      setWantCertificate(true);
      setCertInfoSaved(true);
      setShowCertForm(false);
      setIsCertOtpSent(false);
      setCertOtpCode('');
      setCertVerifyError('');
      if (memberNationalCode && !certNationalCode) {
        setCertNationalCode(memberNationalCode);
      }
      if ((user?.phone || user?.phoneNumber) && !certPhone) {
        setCertPhone(user.phone || user.phoneNumber);
      }
    }
  }, [isOtpVerified, memberNationalCode, user]);

  // --- OTP Timer countdown (Member) ---
  useEffect(() => {
    let interval = null;
    if (otpTimer > 0) {
      interval = setInterval(() => {
        setOtpTimer((prev) => prev - 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [otpTimer]);

  // --- Certificate OTP Timer countdown ---
  useEffect(() => {
    let interval = null;
    if (certOtpTimer > 0) {
      interval = setInterval(() => {
        setCertOtpTimer((prev) => prev - 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [certOtpTimer]);

  // --- Parse next session info ---
  useEffect(() => {
    if (meet?.nextMeet) {
      const nextMeetDate = new Date(meet.nextMeet);
      const now = new Date();

      const nextUTCDate = new Date(Date.UTC(
        nextMeetDate.getUTCFullYear(),
        nextMeetDate.getUTCMonth(),
        nextMeetDate.getUTCDate()
      ));

      const nowUTCDate = new Date(Date.UTC(
        now.getUTCFullYear(),
        now.getUTCMonth(),
        now.getUTCDate()
      ));

      let dateString = '';
      if (nextUTCDate.getTime() === nowUTCDate.getTime()) {
        dateString = 'امروز';
      } else {
        const daysDifference = Math.ceil((nextUTCDate - nowUTCDate) / (1000 * 60 * 60 * 24));
        if (daysDifference === 1) {
          dateString = 'فردا';
        } else if (daysDifference > 0 && daysDifference <= 30) {
          const persianDate = new Intl.DateTimeFormat('fa-IR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
          }).format(nextMeetDate);
          dateString = persianDate;
        }
      }

      if (dateString) {
        const timeString = nextMeetDate.toLocaleTimeString('fa-IR', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: false,
          timeZone: 'UTC'
        });
        setNextSessionInfo(`${dateString} ساعت ${timeString}`);
      } else {
        setNextSessionInfo('');
      }
    }
  }, [meet]);

  // --- Fetch Meeting and Live status ---
  const fetchMeetingInfo = async () => {
    try {
      const data = await fetchRoom(Mid);
      const details = await GetRoomDetails(Mid);
      CheckLives();
      setMeet(data);
      setRoomDetails({
        ...details,
        subscriptionStatus: data.subscriptionStatus,
        isSubscribedByUser: data.isSubscribedByUser,


      });
      if (data?.title) {
        document.title = `مجمع عالی حکمت اسلامی | ${data.title}`;
      }
    } catch (error) {
      console.error('Error fetching meeting info:', error);
      setMeet(null);
      setRoomDetails(null);
    }
  };

  useEffect(() => {
    fetchMeetingInfo();
    const intervalId = setInterval(CheckLives, 15000);
    return () => clearInterval(intervalId);
  }, [Mid]);

  useEffect(() => {
    const liveStatus = roomLive.some((room) => room.roomID === Mid);
    setIsMeetingLive(liveStatus);
  }, [roomLive, Mid]);

  useEffect(() => {
    if (meet?.nextPlan) {
      fetchReservation(meet.nextPlan.id);
    }
  }, [meet]);

  async function CheckLives() {
    try {
      let livedata = await fetchRoomLivesPublicWithID(Mid);
      if (!Array.isArray(livedata) || livedata.length === 0) {
        livedata = await fetchRoomLivesPublic();
      }
      setRoomLive(livedata || []);
    } catch (error) {
      console.error('Error fetching live rooms:', error);
    }
  }

  const fetchReservation = async (pid) => {
    try {
      const data = await GetReservation(pid);
      if (data && data.nickName) {
        setReservation(data);
        setReservationName(data.nickName);
        setReservationPhone(data.phone || '');
        setReservationNationalCode(data.nationalCode || '');
      } else {
        setReservation(null);
      }
    } catch (error) {
      console.error('Error fetching reservation info:', error);
      setReservation(null);
    }
  };

  // --- Validation Helpers ---
  const validatePersianName = (name) => {
    if (!name || name.trim() === '') {
      return { isValid: false, message: 'لطفاً نام و نام خانوادگی خود را وارد کنید' };
    }
    const cleanName = name.trim();
    const words = cleanName.split(/\s+/);
    if (words.length < 2) {
      return { isValid: false, message: 'لطفاً نام و نام خانوادگی خود را به صورت کامل وارد کنید' };
    }
    const persianRegex = /^[\u0600-\u06FF\s]+$/;
    if (!persianRegex.test(cleanName)) {
      return { isValid: false, message: 'نام باید با حروف فارسی وارد شود' };
    }
    return { isValid: true, message: '' };
  };

  const validateDisplayName = (name) => {
    if (wantCertificate && certInfoSaved && name.trim()) {
      const res = validatePersianName(name);
      if (!res.isValid) {
        setNameError(res.message);
        return false;
      }
    }
    setNameError('');
    return true;
  };

  // --- Member OTP Verification Handlers (via National Code) ---
  const handleSendOTP = async () => {
    const rawCode = toEnglishDigits(memberNationalCode).trim();

    if (!rawCode) {
      setMemberVerifyError('لطفاً کد ملی ۱۰ رقمی خود را وارد نمایید');
      return;
    }

    if (rawCode.length !== 10 || !/^\d{10}$/.test(rawCode)) {
      setMemberVerifyError('کد ملی باید دقیقاً ۱۰ رقم عددی باشد');
      return;
    }

    setOtpLoading(true);
    setMemberVerifyError('');

    try {
      const response = await SendOTPConfirmation({
        nationalCode: rawCode,
        phone: ''
      });

      if (response.status === 200 && response.guid) {
        setOtpGuid(response.guid);
        setIsOtpSent(true);
        setOtpTimer(120); // 2 minutes countdown
        setMaskedPhone(response.maskedPhone || response.phone || '');
        setMemberVerifyError('');
        toast?.success?.('کد تایید پیامکی با موفقیت ارسال شد');
      } else {
        setMemberVerifyError(response.message || 'کد ملی در لیست اعضای مجمع یافت نشد');
      }
    } catch (error) {
      console.error('Error sending OTP:', error);
      setMemberVerifyError('خطا در ارسال کد تایید. لطفاً اتصال اینترنت خود را بررسی نمایید.');
    } finally {
      setOtpLoading(false);
    }
  };

  const handleConfirmOTP = async () => {
    const cleanOtp = toEnglishDigits(otpCode).trim();

    if (!cleanOtp || cleanOtp.length < 4) {
      setMemberVerifyError('لطفاً کد تایید ۴ رقمی پیامک‌شده را وارد کنید');
      return;
    }

    setMemberVerificationLoading(true);
    setMemberVerifyError('');

    try {
      const response = await ConfirmOTP({
        guid: otpGuid,
        code: cleanOtp
      });

      if (response.status === 200 && response.token) {
        setCookieWithConfig('vc2AuthNorm', response.token, 365 * 10);
        syncUserTokens();

        const verifiedName = response.fullname || displayName || 'عضو مجمع';
        const verifiedNatCode = response.nationalCode || memberNationalCode;
        const verifiedPhone = response.phone || '';

        setCookie('joinName', verifiedName, 30);
        setCookie('joinNationalCode', verifiedNatCode, 30);

        setDisplayName(verifiedName);
        setIsOtpVerified(true);
        setIsMemberModeActive(true);
        setIsMember(true);
        setMemberVerifyError('');

        // Automatically check and lock certificate info for verified member
        setWantCertificate(true);
        setCertInfoSaved(true);
        setShowCertForm(false);
        setCertNationalCode(verifiedNatCode);
        setCookie('wantCertificate', 'true', 30);
        setCookie('certNationalCode', verifiedNatCode, 30);
        if (verifiedPhone) {
          setCertPhone(verifiedPhone);
          setCookie('certPhone', verifiedPhone, 30);
        }

        toast?.success?.(`خوش آمدید، جناب آقای/سرکار خانم ${verifiedName}`);
        setTimeout(() => {
          window.location.reload();
        }, 1000);
      } else {
        setMemberVerifyError(response.message || 'کد تایید وارد شده نادرست یا منقضی شده است');
      }
    } catch (error) {
      console.error('Error confirming OTP:', error);
      setMemberVerifyError('خطا در تایید کد. لطفاً دوباره تلاش کنید.');
    } finally {
      setMemberVerificationLoading(false);
    }
  };

  const handleResendOTP = () => {
    if (otpTimer === 0) {
      setOtpCode('');
      handleSendOTP();
    }
  };

  const handleToggleMemberMode = (checked) => {
    if (!checked) {
      setIsMember(false);
      setIsOtpSent(false);
      setIsOtpVerified(false);
      setIsMemberModeActive(false);
      setOtpCode('');
      setOtpGuid('');
      setOtpTimer(0);
      setMaskedPhone('');
      setMemberVerifyError('');
      removeAuthToken();
      removeRegisteredUserToken();
      // Unlock certificate section
      setCertInfoSaved(false);
      setWantCertificate(false);
      setCookie('wantCertificate', 'false', 30);
    } else {
      setIsMember(true);
    }
  };

  // --- Next Session Reservation Handler ---
  const handleCreateReservation = async () => {
    if (!reservationName.trim()) {
      alert('لطفاً نام و نام خانوادگی خود را وارد کنید');
      return;
    }
    const cleanPhone = toEnglishDigits(reservationPhone).trim();
    if (!cleanPhone || !/^(\+98|0|0098)?9\d{9}$/.test(cleanPhone)) {
      alert('لطفاً شماره همراه معتبر وارد کنید (مثال: 09123456789)');
      return;
    }
    const cleanNationalCode = toEnglishDigits(reservationNationalCode).trim();
    if (!cleanNationalCode || cleanNationalCode.length !== 10) {
      alert('لطفاً کد ملی ۱۰ رقمی معتبر وارد کنید');
      return;
    }

    setReservationLoading(true);
    try {
      const data = await NewReservation(meet.nextPlan.id, reservationName.trim(), cleanPhone, cleanNationalCode);
      if (data && data.status === 200) {
        await fetchReservation(meet.nextPlan.id);
        setShowReservationForm(false);
        setCookie('joinName', reservationName, 30);
        setCookie('joinPhone', cleanPhone, 30);
        setCookie('joinNationalCode', cleanNationalCode, 30);
        toast?.success?.('رزرو شما با موفقیت ثبت گردید.');
      } else {
        alert(data?.message || 'خطا در ثبت رزرو');
      }
    } catch (error) {
      console.error('Error creating reservation:', error);
      alert('خطا در برقراری ارتباط با سرور.');
    } finally {
      setReservationLoading(false);
    }
  };

  // --- Certificate Info OTP Verification Handlers (for non-member attendees) ---
  const handleRequestCertVerification = async () => {
    const cleanPhone = toEnglishDigits(certPhone).trim();
    const cleanNationalCode = toEnglishDigits(certNationalCode).trim();

    if (!cleanPhone && !cleanNationalCode) {
      setCertVerifyError('لطفاً شماره همراه یا کد ملی خود را وارد نمایید');
      return;
    }

    if (cleanPhone && !/^(\+98|0|0098)?9\d{9}$/.test(cleanPhone)) {
      setCertVerifyError('لطفاً شماره همراه معتبر وارد کنید (مثال: 09123456789)');
      return;
    }

    if (cleanNationalCode && (cleanNationalCode.length !== 10 || !/^\d{10}$/.test(cleanNationalCode))) {
      setCertVerifyError('کد ملی باید دقیقاً ۱۰ رقم عددی باشد');
      return;
    }

    if (!cleanPhone) {
      setCertVerifyError('جهت ارسال پیامک تایید، وارد کردن شماره همراه الزامی است');
      return;
    }

    setCertOtpLoading(true);
    setCertVerifyError('');

    try {
      const response = await VerifyCertificateUser({
        phone: cleanPhone,
        nationalCode: cleanNationalCode,
        name: displayName.trim(),
        uuid: getEffectiveUserToken()
      });

      if (response.status === 200 && response.guid) {
        setCertOtpGuid(response.guid);
        setIsCertOtpSent(true);
        setCertOtpTimer(120); // 2 minutes countdown
        setCertMaskedPhone(response.maskedPhone || cleanPhone);
        if (response.phone) {
          setCertPhone(response.phone);
        }
        setCertVerifyError('');
        toast?.success?.('کد تایید پیامکی اطلاعات گواهی ارسال شد');
      } else {
        setCertVerifyError(response.message || 'خطا در بررسی اطلاعات گواهی');
      }
    } catch (error) {
      console.error('Error requesting cert verification:', error);
      setCertVerifyError('خطا در برقراری ارتباط با سرور. لطفاً مجدداً تلاش نمایید.');
    } finally {
      setCertOtpLoading(false);
    }
  };

  const handleConfirmCertOTP = async () => {
    const cleanOtp = toEnglishDigits(certOtpCode).trim();

    if (!cleanOtp || cleanOtp.length < 4) {
      setCertVerifyError('لطفاً کد تایید ۴ رقمی پیامک‌شده را وارد نمایید');
      return;
    }

    const cleanPhone = toEnglishDigits(certPhone).trim();
    const cleanNationalCode = toEnglishDigits(certNationalCode).trim();

    setCertVerificationLoading(true);
    setCertVerifyError('');

    try {
      const response = await ConfirmCertVerification({
        guid: certOtpGuid,
        code: cleanOtp,
        phone: cleanPhone,
        nationalCode: cleanNationalCode,
        name: displayName.trim()
      });

      if (response.status === 200 && response.token) {
        // Save vc2AuthNorm cookie so user is now officially authenticated & signed up
        setCookieWithConfig('vc2AuthNorm', response.token, 365 * 10);
        syncUserTokens();

        const savedPhone = response.phone || cleanPhone;
        const savedNatCode = response.nationalCode || cleanNationalCode;
        const verifiedName = response.fullname || displayName.trim();

        if (verifiedName && !displayName) {
          setDisplayName(verifiedName);
        }

        setCertPhone(savedPhone);
        setCertNationalCode(savedNatCode);
        setCookie('wantCertificate', 'true', 30);
        setCookie('certPhone', savedPhone, 30);
        setCookie('certNationalCode', savedNatCode, 30);

        setCertInfoSaved(true);
        setIsCertOtpSent(false);
        setCertOtpCode('');
        setCertVerifyError('');
        setShowCertForm(false);
        toast?.success?.('اطلاعات گواهی شما با موفقیت تایید و ثبت گردید');
      } else {
        setCertVerifyError(response.message || 'کد تایید وارد شده نادرست یا منقضی شده است');
      }
    } catch (error) {
      console.error('Error confirming cert OTP:', error);
      setCertVerifyError('خطا در تایید کد. لطفاً دوباره تلاش فرمایید.');
    } finally {
      setCertVerificationLoading(false);
    }
  };

  const handleResendCertOTP = () => {
    if (certOtpTimer === 0) {
      setCertOtpCode('');
      handleRequestCertVerification();
    }
  };

  // --- Join Meeting Execution ---
  const handleJoinMeeting = async () => {
    if (!displayName.trim()) {
      alert('لطفاً نام نمایشی خود را وارد کنید.');
      return;
    }

    if (wantCertificate && certInfoSaved) {
      const checkName = validatePersianName(displayName);
      if (!checkName.isValid) {
        alert(checkName.message);
        return;
      }
    }

    setLoading(true);

    try {
      const livedata = await fetchRoomLivesPublicWithID(Mid);
      setRoomLive(livedata || []);
      console.log(livedata, Mid)
      const liveStatus = (livedata || []).some((room) => room.roomID === Mid);

      if (!liveStatus) {
        setLoading(false);
        alert('جلسه هنوز آغاز نشده است. لطفاً پس از شروع جلسه مجدداً تلاش نمایید.');
        return;
      }

      setCookie('joinName', displayName.trim(), 30);

      let nationalCodeParam = isOtpVerified ? memberNationalCode : null;
      let phoneParam = null;

      if (wantCertificate && certInfoSaved) {
        nationalCodeParam = certNationalCode;
        phoneParam = certPhone;
      }

      const res = await joinMeeting(
        Mid,
        displayName.trim(),
        isAdmin,
        '123',
        nationalCodeParam,
        phoneParam
      );

      if (res.status !== 200) {
        if (res.status === 500) {
          window.location.reload(true);
        } else {
          toast?.error?.(res.message || 'خطا در ورود به جلسه');
        }
      } else if (res.link) {
        window.open(res.link, '_blank');
      }
    } catch (error) {
      console.error('Error joining meeting:', error);
      alert('خطا در ورود به جلسه. لطفاً مجدداً تلاش فرمایید.');
    } finally {
      setLoading(false);
    }
  };

  // --- Subscribed Room OTP Verification Handlers ---
  const handleDirectRequest = async () => {
    try {
      const price = roomDetails?.metadatas?.price;
      if (price && Number(price) > 0) {
        navigate(`/rooms/${Mid}/sub/request`);
        return;
      }
      toast?.success?.('در حال ثبت درخواست...');
      const response = await DirectRoomSubscribedRegistration({ RoomId: Mid });
      if (response && response.status === 200) {
        toast?.success?.('درخواست عضویت شما با موفقیت ثبت شد.');
        setTimeout(() => {
          window.location.reload();
        }, 1500);
      } else {
        toast?.error?.(response?.message || 'خطا در ثبت درخواست');
      }
    } catch (error) {
      console.error('Error direct request:', error);
      toast?.error?.('خطا در برقراری ارتباط با سرور');
    }
  };
  const handleSendSubOTP = async () => {
    const rawCode = toEnglishDigits(subNationalCode).trim();
    if (!rawCode || rawCode.length !== 10 || !/^\d{10}$/.test(rawCode)) {
      setSubVerifyError('لطفاً کد ملی ۱۰ رقمی معتبر وارد نمایید');
      return;
    }
    setSubOtpLoading(true);
    setSubVerifyError('');
    try {
      const response = await SendOTPRoomSubscribedUser({ nationalCode: rawCode });
      if (response.status === 200 && response.guid) {
        setSubOtpGuid(response.guid);
        setIsSubOtpSent(true);
        setSubMaskedPhone(response.maskedPhone || response.phone || '');
        setSubVerifyError('');
        toast?.success?.('کد تایید پیامکی با موفقیت ارسال شد');
      } else {
        setSubVerifyError(response.message || 'خطا در ارسال کد تایید');
      }
    } catch (error) {
      console.error('Error sending OTP:', error);
      setSubVerifyError('خطا در ارسال کد تایید. لطفاً مجدداً تلاش نمایید.');
    } finally {
      setSubOtpLoading(false);
    }
  };

  const handleConfirmSubOTP = async () => {
    const cleanOtp = toEnglishDigits(subOtpCode).trim();
    if (!cleanOtp || cleanOtp.length < 4) {
      setSubVerifyError('لطفاً کد تایید ۴ رقمی پیامک‌شده را وارد کنید');
      return;
    }
    setSubVerificationLoading(true);
    setSubVerifyError('');
    try {
      const response = await ConfirmOTPRoomSubscribedUser({ guid: subOtpGuid, code: cleanOtp });
      if (response.status === 200 && response.token) {
        setCookieWithConfig('vc2AuthNorm', response.token, 365 * 10);
        syncUserTokens();
        setIsSubOtpSent(false);
        setSubVerifyError('');
        toast?.success?.('احراز هویت با موفقیت انجام شد');
        window.location.reload();
      } else {
        setSubVerifyError(response.message || 'کد تایید وارد شده نادرست است');
      }
    } catch (error) {
      console.error('Error confirming OTP:', error);
      setSubVerifyError('خطا در تایید کد. لطفاً دوباره تلاش کنید.');
    } finally {
      setSubVerificationLoading(false);
    }
  };

  if (loadingAuth) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-emerald-400 border-t-emerald-700 rounded-full animate-spin"></div>
      </div>
    );
  }

  const bgImage = roomDetails?.metadatas?.image_cover;
  const bgStyle = bgImage
    ? { backgroundImage: `url(${bgImage})`, backgroundSize: 'cover', backgroundPosition: 'center', backgroundRepeat: 'no-repeat' }
    : {};

  return (
    <div
      className={`min-h-screen flex items-center justify-center px-4 py-8 sm:py-12 relative overflow-hidden ${!bgImage ? 'bg-gradient-to-br from-[#06241c] via-[#093529] to-[#0f4d3d]' : ''}`}
      style={bgStyle}
      dir="rtl"
    >

      {/* Overlay if there's a background image to ensure readability */}
      {bgImage && <div className="absolute inset-0 bg-black/60 z-0"></div>}

      {/* عناصر تزئینی و هندسه اسلامی در پس‌زمینه */}
      {!bgImage && (
        <>
          <div className="absolute -top-32 -right-32 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none"></div>
          <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-teal-500/10 rounded-full blur-3xl pointer-events-none"></div>
          <div className="absolute inset-0 bg-[radial-gradient(#ffffff0a_1px,transparent_1px)] [background-size:20px_20px] pointer-events-none"></div>
        </>
      )}

      <div className="w-full max-w-lg mx-auto bg-white/98 backdrop-blur-xl rounded-3xl shadow-2xl shadow-black/40 border border-white/20 p-6 sm:p-8 relative z-10 transition-all duration-300">

        {/* هدر صفحه و لوگوی مجمع */}
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-tr from-[#0b3f32] to-[#166534] rounded-2xl mb-3 shadow-lg shadow-emerald-950/20 border-2 border-emerald-400/30 transform transition-transform hover:scale-105 overflow-hidden">
            {roomDetails?.logo ? (
              <img src={roomDetails.logo} alt="logo" className="w-16 h-16 object-contain" />
            ) : (
              <span className="text-3xl">🏛️</span>
            )}
          </div>
          <h1 className="text-xl sm:text-2xl font-black text-gray-900 tracking-tight">
            {meet?.title || 'مجمع عالی حکمت اسلامی'}
          </h1>
          <p className="text-xs sm:text-sm text-gray-500 mt-1 font-medium"
            onClick={() => {
              // alert(JSON.stringify(meet?.isAdmin))
            }}>
            سامانه ورود به تالار جلسات و کلاس‌های تخصصی
          </p>
        </div>

        {meet?.isSubscribed && !meet?.isAdmin ? (
          user && roomDetails?.isSubscribedByUser ? (
            roomDetails?.subscriptionStatus === 100 ? (
              <div className="bg-white p-5 rounded-2xl border-2 border-emerald-200 text-center space-y-5 shadow-sm mt-4">
                <div className="text-5xl drop-shadow-md">🔒</div>
                <p className="font-black text-lg text-emerald-700 leading-snug">شما عضو تایید شده این کلاس هستید</p>
                <div className="space-y-4 pt-2">
                  <input
                    type="text"
                    value={user.fullname || displayName}
                    disabled
                    className="w-full px-4 py-3.5 bg-gray-100 border-2 border-gray-200 rounded-xl text-center font-bold text-lg text-gray-500 cursor-not-allowed outline-none"
                  />
                  <div className="relative group w-full">
                    <button
                      onClick={handleJoinMeeting}
                      disabled={loading || !isMeetingLive}
                      className="w-full py-3.5 bg-emerald-700 bg-gradient-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800 text-white font-black rounded-xl disabled:opacity-50 transition-all shadow-md flex items-center justify-center gap-2"
                    >
                      {loading ? 'در حال اتصال...' : 'ورود به کلاس 🚀'}
                    </button>

                    {/* Tooltip فقط وقتی جلسه لایو نیست */}
                    {!isMeetingLive && !loading && (
                      <div
                        className="pointer-events-none absolute bottom-full right-1/2 translate-x-1/2 mb-2 
                 opacity-0 group-hover:opacity-100 transition-opacity duration-200
                 bg-gray-900 text-white text-xs font-bold px-3 py-2 rounded-lg shadow-lg whitespace-nowrap z-50"
                      >
                        پخش زنده ای در حال اجرا نمیباشد
                        {/* فلش پایین tooltip */}
                        <span className="absolute top-full right-1/2 translate-x-1/2 -mt-1 
                       border-4 border-transparent border-t-gray-900"></span>
                      </div>
                    )}
                  </div>
                </div>
                <div className="pt-3 border-t border-emerald-100 mt-4">
                  <button
                    onClick={() => navigate(`/rooms/${Mid}`)}
                    className="w-full py-3 bg-white hover:bg-emerald-50 text-emerald-700 font-bold rounded-xl transition-colors border border-emerald-200 text-xs flex items-center justify-center gap-2"
                  >
                    <span>ℹ️</span> مشاهده جزئیات و اطلاعات کامل کلاس
                  </button>
                </div>
              </div>
            ) : roomDetails?.subscriptionStatus === 98 ? (
              <div className="bg-white p-5 rounded-2xl border-2 border-blue-200 text-center space-y-5 shadow-sm mt-4">
                <div className="text-5xl drop-shadow-md">⏳</div>
                <p className="font-black text-lg text-blue-700 leading-snug">شما در این کلاس ثبت‌نام کرده‌اید</p>
                <p className="text-sm text-gray-600 font-medium">درخواست شما در لیست انتظار و در حال بررسی توسط مدیر می‌باشد.</p>
              </div>
            ) : (
              <div className="bg-white p-5 rounded-2xl border-2 border-red-200 text-center space-y-5 shadow-sm mt-4">
                <div className="text-5xl drop-shadow-md">❌</div>
                <p className="font-black text-lg text-red-700 leading-snug">درخواست عضویت شما رد شده است</p>
                <p className="text-sm text-gray-600 font-medium">متاسفانه مدیر کلاس درخواست شما را تایید نکرده است.</p>
              </div>
            )
          ) : user && !roomDetails?.isSubscribedByUser ? (
            <div className="bg-white p-5 rounded-2xl border-2 border-orange-200 text-center space-y-5 shadow-sm mt-4">
              <div className="text-5xl drop-shadow-md">⚠️</div>
              <p className="font-black text-lg text-orange-700 leading-snug">شما در این کلاس ثبت‌نام نکرده‌اید</p>
              <p className="text-sm text-gray-600 font-medium">برای شرکت در این کلاس، ابتدا باید درخواست عضویت خود را ثبت کنید.</p>
              <div className="pt-2 border-t border-orange-100 mt-2">
                <button
                  onClick={handleDirectRequest}
                  className="w-full py-3 bg-orange-50 hover:bg-orange-100 text-orange-700 font-bold rounded-xl transition-colors border border-orange-200"
                >
                  ثبت درخواست عضویت در کلاس
                </button>
              </div>
            </div>
          ) : (
            <div className="bg-white p-5 rounded-2xl border-2 border-red-200 text-center space-y-5 shadow-sm mt-4">
              <div className="text-5xl drop-shadow-md">🔒</div>
              <p className="font-black text-lg text-red-700 leading-snug">این کلاس خصوصی است و فقط برای اعضای کلاس قابل دسترس می‌باشد</p>
              <p className="text-sm text-gray-600 font-medium">برای احراز هویت، لطفاً کد ملی خود را وارد نمایید</p>

              {!isSubOtpSent ? (
                <div className="space-y-4 pt-2">
                  <input
                    type="text"
                    value={subNationalCode}
                    onChange={(e) => setSubNationalCode(e.target.value)}
                    placeholder="کد ملی ۱۰ رقمی"
                    className="w-full px-4 py-3.5 bg-gray-50 border-2 border-gray-200 rounded-xl text-center font-mono font-bold text-lg focus:border-red-400 focus:bg-white transition-all outline-none"
                    maxLength={10}
                    dir="ltr"
                  />
                  {subVerifyError && <p className="text-xs text-red-600 font-bold bg-red-50 p-2 rounded-lg border border-red-100">{subVerifyError}</p>}
                  <button
                    onClick={handleSendSubOTP}
                    disabled={subOtpLoading || !subNationalCode}
                    className="w-full py-3.5 bg-red-600 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-black rounded-xl disabled:opacity-50 transition-all shadow-md flex items-center justify-center gap-2"
                  >
                    {subOtpLoading ? 'در حال ارسال...' : 'ارسال کد تایید ✉️'}
                  </button>

                  <div className="pt-2 border-t border-red-100 mt-2">
                    <p className="text-xs text-gray-500 mb-2">در این کلاس عضو نیستید؟</p>
                    <button
                      onClick={() => navigate(`/rooms/${Mid}/sub/request`)}
                      className="w-full py-3 bg-red-50 hover:bg-red-100 text-red-700 font-bold rounded-xl transition-colors border border-red-200"
                    >
                      ثبت درخواست عضویت در کلاس
                    </button>
                  </div>
                </div>
              ) : (
                <div className="space-y-4 pt-2 animate-in fade-in">
                  <p className="text-xs text-emerald-800 font-bold bg-emerald-50 p-3 rounded-xl border border-emerald-200 flex items-center justify-center gap-2 shadow-inner">
                    <span>📱</span> کد تایید به شماره <span className="font-mono text-sm" dir="ltr">{subMaskedPhone}</span> ارسال شد.
                  </p>
                  <input
                    type="text"
                    value={subOtpCode}
                    onChange={(e) => setSubOtpCode(e.target.value)}
                    placeholder="کد تایید ۴ رقمی"
                    className="w-full px-4 py-3.5 bg-gray-50 border-2 border-gray-200 rounded-xl text-center font-mono text-2xl tracking-[0.5em] font-black focus:border-red-400 focus:bg-white transition-all outline-none"
                    maxLength={4}
                    dir="ltr"
                    autoFocus
                  />
                  {subVerifyError && <p className="text-xs text-red-600 font-bold bg-red-50 p-2 rounded-lg border border-red-100">{subVerifyError}</p>}
                  <div className="flex gap-2">
                    <button
                      onClick={handleConfirmSubOTP}
                      disabled={subVerificationLoading || !subOtpCode || subOtpCode.length < 4}
                      className="flex-[2] py-3.5 bg-red-600 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-black rounded-xl disabled:opacity-50 transition-all shadow-md"
                    >
                      {subVerificationLoading ? 'در حال بررسی...' : 'تایید کد ✓'}
                    </button>
                    <button
                      onClick={() => {
                        setIsSubOtpSent(false);
                        setSubOtpCode('');
                        setSubVerifyError('');
                      }}
                      className="flex-1 py-3.5 border-2 border-gray-200 text-gray-600 hover:bg-gray-50 rounded-xl font-bold transition-all text-xs"
                    >
                      تغییر کدملی
                    </button>
                  </div>
                </div>
              )}
              <div className="pt-3 border-t border-red-100 mt-4">
                <button
                  onClick={() => navigate(`/rooms/${Mid}`)}
                  className="w-full py-3 bg-white hover:bg-gray-50 text-gray-700 font-bold rounded-xl transition-colors border border-gray-200 text-xs flex items-center justify-center gap-2"
                >
                  <span>ℹ️</span> مشاهده جزئیات و اطلاعات کامل کلاس
                </button>
              </div>
            </div>
          )
        ) : (
          <>
            {/* کارت مشخصات اتاق/جلسه */}
            <div className="bg-gradient-to-l from-emerald-50/90 via-teal-50/60 to-white rounded-2xl p-4 mb-5 border border-emerald-200/80 shadow-sm relative overflow-hidden">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <span className="text-[11px] font-bold text-emerald-800 uppercase tracking-wider block mb-1">
                    عنوان تالار / جلسه
                  </span>
                  <h2 className="text-base sm:text-lg font-black text-gray-900 leading-snug break-words">
                    {meet?.title || 'در حال بارگذاری اطلاعات جلسه...'}
                  </h2>
                </div>
                {isMeetingLive && (
                  <span className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-red-500 text-white rounded-full text-xs font-black shadow-md animate-pulse flex-shrink-0">
                    <span className="w-2 h-2 rounded-full bg-white"></span>
                    زنده (آنلاین)
                  </span>
                )}
              </div>

              {meet?.owner?.fullname && (
                <div className="mt-3 pt-2.5 border-t border-emerald-200/50 flex items-center gap-2 text-xs text-gray-600">
                  <span className="text-emerald-700 font-bold">برگزارکننده:</span>
                  <span className="font-semibold text-gray-800">{meet.owner.fullname}</span>
                </div>
              )}

              <div className="mt-3 pt-2.5 border-t border-emerald-200/50">
                <button
                  onClick={() => navigate(`/rooms/${Mid}`)}
                  className="w-full py-2 bg-white hover:bg-emerald-50 text-emerald-800 font-bold rounded-xl transition-colors border border-emerald-200 shadow-sm text-xs flex items-center justify-center gap-2"
                >
                  <span>ℹ️</span> مشاهده جزئیات و اطلاعات کامل کلاس
                </button>
              </div>
            </div>

            {/* بنر وضعیت جلسه در صورت لایو بودن */}
            {isMeetingLive ? (
              <div className="bg-gradient-to-r from-emerald-600 to-teal-700 text-white p-4 rounded-2xl mb-5 shadow-lg shadow-emerald-900/20 flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-white/20 backdrop-blur flex items-center justify-center text-xl flex-shrink-0">
                  🎙️
                </div>
                <div className="min-w-0 flex-1">
                  <p className="font-black text-sm">جلسه هم‌اکنون در حال برگزاری است</p>
                  {meet?.latestTitle ? (
                    <p className="text-xs text-emerald-100 mt-0.5 truncate font-medium">
                      موضوع جلسه: {meet.latestTitle}
                    </p>
                  ) : (
                    <p className="text-xs text-emerald-100 mt-0.5 font-medium">
                      برای حضور در تالار گفتگو، فرم زیر را تکمیل نمایید.
                    </p>
                  )}
                </div>
              </div>
            ) : (
              /* بنر وضعیت عدم برگزاری جلسه و نمایش جلسه آینده */
              <div className="bg-gray-50 border border-gray-200 rounded-2xl p-4 mb-5 text-gray-700 space-y-2.5">
                <div className="flex items-center gap-2 text-amber-700 font-bold text-xs sm:text-sm">
                  <span>⏳</span>
                  <span>در حال حاضر جلسه‌ای در این اتاق در جریان نیست.</span>
                </div>

                {nextSessionInfo && (
                  <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm text-xs space-y-1">
                    <p className="text-gray-500 font-medium">زمان برگزاری جلسه بعدی:</p>
                    <p className="font-bold text-gray-800 text-sm text-emerald-800">{nextSessionInfo}</p>
                    {meet?.latestTitle && (
                      <p className="text-gray-600 text-xs mt-1 font-medium">موضوع: {meet.latestTitle}</p>
                    )}
                  </div>
                )}

                {/* کارت رزرو جلسه بعدی */}
                {meet?.nextPlan && (
                  <div className="mt-3 pt-3 border-t border-gray-200">
                    {reservation ? (
                      <div className="bg-emerald-50 rounded-xl p-3 border border-emerald-200 text-xs space-y-1">
                        <p className="font-black text-emerald-800 flex items-center gap-1.5">
                          <span>✓</span> ثبت‌نام و رزرو شما برای جلسه آینده تایید شده است
                        </p>
                        <p className="text-gray-700">نام: {reservation.nickName}</p>
                        {reservation.phone && <p className="text-gray-700">تلفن: {reservation.phone}</p>}
                      </div>
                    ) : !showReservationForm ? (
                      <button
                        onClick={() => setShowReservationForm(true)}
                        className="w-full py-2.5 px-4 bg-emerald-800 bg-gradient-to-r from-emerald-800 to-teal-800 hover:from-emerald-900 hover:to-teal-900 text-white rounded-xl font-bold text-xs shadow-md transition-all flex items-center justify-center gap-1.5"
                      >
                        <span>➕</span> پیش‌ثبت‌نام و رزرو جلسه بعدی
                      </button>
                    ) : (
                      <div className="bg-white p-4 rounded-xl border border-gray-200 space-y-3 mt-2 shadow-sm">
                        <p className="text-xs font-bold text-gray-800">مشخصات متقاضی برای جلسه آینده:</p>
                        <input
                          type="text"
                          value={reservationName}
                          onChange={(e) => setReservationName(e.target.value)}
                          placeholder="نام و نام خانوادگی"
                          className="w-full px-3 py-2 border border-gray-200 rounded-xl text-xs focus:ring-2 focus:ring-emerald-500 outline-none"
                        />
                        <input
                          type="tel"
                          value={reservationPhone}
                          onChange={(e) => setReservationPhone(e.target.value)}
                          placeholder="شماره همراه (مثال: 09123456789)"
                          className="w-full px-3 py-2 border border-gray-200 rounded-xl text-xs focus:ring-2 focus:ring-emerald-500 outline-none "
                          dir="ltr"
                        />
                        <input
                          type="text"
                          value={reservationNationalCode}
                          onChange={(e) => setReservationNationalCode(e.target.value)}
                          placeholder="کد ملی ۱۰ رقمی"
                          className="w-full px-3 py-2 border border-gray-200 rounded-xl text-xs focus:ring-2 focus:ring-emerald-500 outline-none "
                          dir="ltr"
                          maxLength={10}
                        />
                        <div className="flex gap-2 pt-1">
                          <button
                            onClick={handleCreateReservation}
                            disabled={reservationLoading}
                            className="flex-1 py-2 bg-emerald-700 hover:bg-emerald-800 text-white rounded-xl text-xs font-bold transition-all disabled:opacity-50"
                          >
                            {reservationLoading ? 'در حال ثبت...' : 'ثبت نام قطعی'}
                          </button>
                          <button
                            onClick={() => setShowReservationForm(false)}
                            className="px-3 py-2 border border-gray-200 text-gray-600 rounded-xl text-xs font-bold hover:bg-gray-100 transition-all"
                          >
                            انصراف
                          </button>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}

            {/* فرم ورود به جلسه (تنها زمانی که جلسه آنلاین است) */}
            {isMeetingLive && (
              <div className="space-y-4">

                {/* بخش احراز هویت اعضای مجمع عالی حکمت (کد ملی + ارسال کد به موبایل) */}
                <div className="bg-gradient-to-br from-amber-50/80 via-orange-50/40 to-emerald-50/60 rounded-2xl p-4 border border-amber-200/80 shadow-sm">
                  <div className="flex items-center justify-between">
                    <label htmlFor="isMemberCheckbox" className="flex items-center gap-2.5 cursor-pointer select-none">
                      <input
                        type="checkbox"
                        id="isMemberCheckbox"
                        checked={isMember}
                        onChange={(e) => handleToggleMemberMode(e.target.checked)}
                        disabled={isOtpVerified}
                        className="w-5 h-5 rounded-lg border-2 border-amber-400 text-amber-600 focus:ring-amber-500 accent-amber-600 cursor-pointer"
                      />
                      <div>
                        <span className="font-black text-sm text-gray-900 flex items-center gap-1.5">
                          <span>⭐</span> من از اعضاء مجمع عالی حکمت اسلامی هستم
                        </span>
                        <span className="text-[11px] text-gray-500 block mt-0.5">
                          احراز هویت سریع از طریق کد ملی و پیامک تایید
                        </span>
                      </div>
                    </label>

                    {isOtpVerified && (
                      <span className="px-2.5 py-1 bg-emerald-100 text-emerald-800 rounded-full text-xs font-bold flex items-center gap-1">
                        <span>✓</span> تایید شده
                      </span>
                    )}
                  </div>

                  {/* فرم ورود کد ملی و پیامک تایید */}
                  {isMember && !isOtpVerified && (
                    <div className="mt-3.5 pt-3 border-t border-amber-200/60 space-y-3 animate-in fade-in">
                      {!isOtpSent ? (
                        /* مرحله ۱: دریافت کد ملی */
                        <div>
                          <label className="block text-xs font-bold text-gray-700 mb-1">
                            کد ملی عضویت (۱۰ رقم):
                          </label>
                          <div className="flex gap-2">
                            <input
                              type="text"
                              value={memberNationalCode}
                              onChange={(e) => setMemberNationalCode(e.target.value)}
                              placeholder="مثال: 0012345678"
                              maxLength={10}
                              disabled={otpLoading}
                              className="flex-1 px-3.5 py-2.5 bg-white border border-amber-300 rounded-xl text-sm focus:ring-2 focus:ring-amber-500 outline-none shadow-sm"
                              dir="ltr"
                            />
                            <button
                              type="button"
                              onClick={handleSendOTP}
                              disabled={otpLoading || !memberNationalCode.trim()}
                              className="px-4 py-2.5 bg-amber-600 bg-gradient-to-r from-amber-600 to-amber-700 hover:from-amber-700 hover:to-amber-800 text-white rounded-xl text-xs font-black shadow-md transition-all disabled:opacity-50 whitespace-nowrap flex items-center gap-1.5"
                            >
                              {otpLoading ? (
                                <>
                                  <svg className="animate-spin h-4 w-4 text-white" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                                  </svg>
                                  در حال بررسی...
                                </>
                              ) : (
                                'ارسال کد تایید ✉️'
                              )}
                            </button>
                          </div>
                          <p className="text-[11px] text-gray-500 mt-1.5">
                            کد تایید به شماره همراه ثبت‌شده برای این کد ملی در مجمع ارسال خواهد شد.
                          </p>
                        </div>
                      ) : (
                        /* مرحله ۲: دریافت کد تایید با نمایش شماره موبایل ماسک شده */
                        <div className="space-y-3 bg-white p-3.5 rounded-xl border border-amber-200">
                          <div className="bg-amber-50 p-2.5 rounded-lg border border-amber-200 text-xs text-amber-900 flex items-center justify-between">
                            <div className="flex items-center gap-1.5">
                              <span>📱</span>
                              <span>کد به شماره <strong className=" text-sm font-black text-amber-950">{maskedPhone || 'همراه شما'}</strong> ارسال شد</span>
                            </div>
                            <button
                              type="button"
                              onClick={() => {
                                setIsOtpSent(false);
                                setOtpCode('');
                                setMemberVerifyError('');
                              }}
                              className="text-[11px] text-amber-700 hover:text-amber-900 underline"
                            >
                              تغییر کد ملی
                            </button>
                          </div>

                          <div>
                            <label className="block text-xs font-bold text-gray-700 mb-1">
                              کد تایید ۴ رقمی:
                            </label>
                            <input
                              type="text"
                              value={otpCode}
                              onChange={(e) => setOtpCode(e.target.value)}
                              placeholder="کد ۴ رقمی پیامک شده"
                              maxLength={6}
                              className="w-full px-3.5 py-2.5 bg-gray-50 border border-gray-300 rounded-xl text-center text-lg font-mono font-bold tracking-widest focus:ring-2 focus:ring-emerald-500 outline-none"
                              dir="ltr"
                              autoFocus
                            />
                          </div>

                          <div className="flex items-center justify-between pt-1">
                            <div className="text-xs text-gray-600">
                              {otpTimer > 0 ? (
                                <span>
                                  زمان باقیمانده: <strong className="font-mono text-amber-700 font-bold">{Math.floor(otpTimer / 60)}:{String(otpTimer % 60).padStart(2, '0')}</strong>
                                </span>
                              ) : (
                                <button
                                  type="button"
                                  onClick={handleResendOTP}
                                  className="text-xs text-emerald-700 font-bold hover:underline"
                                >
                                  🔄 ارسال مجدد کد تایید
                                </button>
                              )}
                            </div>

                            <button
                              type="button"
                              onClick={handleConfirmOTP}
                              disabled={memberVerificationLoading || !otpCode.trim()}
                              className="px-5 py-2 bg-emerald-700 bg-gradient-to-r from-emerald-600 to-teal-700 hover:from-emerald-700 hover:to-teal-800 text-white rounded-xl text-xs font-black shadow-md transition-all disabled:opacity-50"
                            >
                              {memberVerificationLoading ? 'در حال بررسی...' : '✓ تایید و ثبت ورود'}
                            </button>
                          </div>
                        </div>
                      )}

                      {memberVerifyError && (
                        <div className="bg-red-50 text-red-600 p-2.5 rounded-xl border border-red-200 text-xs font-bold flex items-center gap-1.5">
                          <span>⚠️</span>
                          <span>{memberVerifyError}</span>
                        </div>
                      )}
                    </div>
                  )}

                  {/* وضعیت عضو تایید شده */}
                  {isOtpVerified && (
                    <div className="mt-3 pt-3 border-t border-amber-200/60 flex items-center justify-between text-xs">
                      <div className="flex items-center gap-2 text-emerald-800 font-bold">
                        <span>🛡️</span>
                        <span>هویت شما به عنوان عضو رسمی مجمع با موفقیت احراز شد.</span>
                      </div>
                      <button
                        type="button"
                        onClick={() => {
                          if (window.confirm('آیا مایل به خروج از حساب عضویت مجمع هستید؟')) {
                            handleToggleMemberMode(false);
                          }
                        }}
                        className="text-red-500 hover:text-red-700 font-medium underline"
                      >
                        خروج از عضویت
                      </button>
                    </div>
                  )}
                </div>

                {/* فیلد نام نمایشی شرکت‌کننده */}
                <div>
                  <label htmlFor="displayNameInput" className="block text-xs font-bold text-gray-700 mb-1.5">
                    نام و نام خانوادگی شما در جلسه: <span className="text-red-500">*</span>
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      id="displayNameInput"
                      value={displayName}
                      onChange={(e) => {
                        setDisplayName(e.target.value);
                        validateDisplayName(e.target.value);
                      }}
                      disabled={isOtpVerified}
                      placeholder={isOtpVerified ? 'نام شما از طریق عضویت مجمع ثبت شده است' : 'مثلاً: دکتر علی رضایی'}
                      className={`w-full px-4 py-3 rounded-2xl border text-sm font-medium outline-none transition-all ${isOtpVerified
                          ? 'bg-emerald-50/70 border-emerald-300 text-emerald-950 font-bold cursor-not-allowed'
                          : 'bg-gray-50 border-gray-200 text-gray-900 focus:bg-white focus:ring-2 focus:ring-emerald-500 shadow-inner'
                        }`}
                      required
                    />
                    {isOtpVerified && (
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-emerald-600 text-sm font-bold">
                        🔒 ثبت شده
                      </span>
                    )}
                  </div>
                  {nameError && <p className="text-xs text-red-500 font-bold mt-1">⚠️ {nameError}</p>}
                </div>

                {/* بخش دریافت گواهی حضور */}
                <div className="bg-gray-50/80 rounded-2xl border border-gray-200 overflow-hidden">
                  {!certInfoSaved ? (
                    <div>
                      <button
                        type="button"
                        onClick={() => setShowCertForm((prev) => !prev)}
                        className="w-full px-4 py-3 flex items-center justify-between text-xs font-bold text-gray-700 hover:bg-gray-100 transition-all"
                      >
                        <div className="flex items-center gap-2">
                          <span className="text-base">📜</span>
                          <span>مایل به دریافت گواهی رسمی حضور در جلسه هستم</span>
                        </div>
                        <span className="text-gray-400 font-mono text-sm">{showCertForm ? '▲' : '▼'}</span>
                      </button>

                      {showCertForm && (
                        <div className="p-4 pt-1 border-t border-gray-200 space-y-3 animate-in fade-in">
                          <div className="flex items-center justify-between">
                            <label className={`flex items-center gap-2 text-xs font-bold text-gray-800 ${isOtpVerified ? 'cursor-not-allowed opacity-80' : 'cursor-pointer'}`}>
                              <input
                                type="checkbox"
                                checked={wantCertificate}
                                onChange={(e) => {
                                  if (!isOtpVerified) {
                                    setWantCertificate(e.target.checked);
                                    if (!e.target.checked) {
                                      setIsCertOtpSent(false);
                                      setCertOtpCode('');
                                      setCertVerifyError('');
                                    }
                                  }
                                }}
                                disabled={isOtpVerified}
                                className="w-4 h-4 rounded text-emerald-600 focus:ring-emerald-500 disabled:opacity-75"
                              />
                              <span>اطلاعات گواهی حضور را تکمیل می‌نمایم</span>
                            </label>
                            {isOtpVerified && (
                              <span className="text-[11px] text-emerald-700 font-bold flex items-center gap-1 bg-emerald-50 px-2 py-0.5 rounded-md border border-emerald-200">
                                <span>🔒</span> قفل شده با عضویت مجمع
                              </span>
                            )}
                          </div>

                          {wantCertificate && !isOtpVerified && (
                            <div className="space-y-2.5 pt-2 border-t border-gray-100">
                              {!isCertOtpSent ? (
                                /* مرحله ۱: دریافت شماره همراه و کد ملی */
                                <>
                                  <div>
                                    <label className="block text-[11px] font-bold text-gray-600 mb-1">
                                      شماره همراه جهت ارسال پیامک احراز: <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                      type="tel"
                                      value={certPhone}
                                      onChange={(e) => setCertPhone(e.target.value)}
                                      placeholder="مثلاً: 09123456789"
                                      className="w-full px-3 py-2 bg-white border border-gray-200 rounded-xl text-xs font-mono outline-none focus:ring-2 focus:ring-emerald-500"
                                      dir="ltr"
                                      disabled={certOtpLoading}
                                    />
                                  </div>
                                  <div>
                                    <label className="block text-[11px] font-bold text-gray-600 mb-1">
                                      کد ملی ۱۰ رقمی جهت درج در گواهی رسمی:
                                    </label>
                                    <input
                                      type="text"
                                      value={certNationalCode}
                                      onChange={(e) => setCertNationalCode(e.target.value)}
                                      placeholder="مثلاً: 0012345678"
                                      maxLength={10}
                                      className="w-full px-3 py-2 bg-white border border-gray-200 rounded-xl text-xs font-mono outline-none focus:ring-2 focus:ring-emerald-500"
                                      dir="ltr"
                                      disabled={certOtpLoading}
                                    />
                                  </div>

                                  {certVerifyError && (
                                    <div className="bg-red-50 text-red-600 p-2.5 rounded-xl border border-red-200 text-xs font-bold flex items-center gap-1.5">
                                      <span>⚠️</span>
                                      <span>{certVerifyError}</span>
                                    </div>
                                  )}

                                  <button
                                    type="button"
                                    onClick={handleRequestCertVerification}
                                    disabled={certOtpLoading || !certPhone.trim()}
                                    className="w-full py-2.5 bg-emerald-700 text-white rounded-xl text-xs font-bold hover:bg-emerald-800 shadow-sm transition-all disabled:opacity-50 flex items-center justify-center gap-2"
                                  >
                                    {certOtpLoading ? (
                                      <>
                                        <svg className="animate-spin h-4 w-4 text-white" viewBox="0 0 24 24">
                                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                                        </svg>
                                        در حال ارسال پیامک تایید...
                                      </>
                                    ) : (
                                      '✓ تایید اطلاعات گواهی'
                                    )}
                                  </button>
                                </>
                              ) : (
                                /* مرحله ۲: ورود کد تایید پیامک‌شده برای گواهی */
                                <div className="space-y-3 bg-white p-3 rounded-xl border border-emerald-200">
                                  <div className="bg-emerald-50 p-2.5 rounded-lg border border-emerald-200 text-xs text-emerald-900 flex items-center justify-between">
                                    <div className="flex items-center gap-1.5">
                                      <span>📱</span>
                                      <span>کد به شماره <strong className="font-mono text-sm font-bold text-emerald-950">{certMaskedPhone || certPhone}</strong> ارسال شد</span>
                                    </div>
                                    <button
                                      type="button"
                                      onClick={() => {
                                        setIsCertOtpSent(false);
                                        setCertOtpCode('');
                                        setCertVerifyError('');
                                      }}
                                      className="text-[11px] text-emerald-700 hover:text-emerald-900 underline"
                                    >
                                      تغییر شماره
                                    </button>
                                  </div>

                                  <div>
                                    <label className="block text-xs font-bold text-gray-700 mb-1">
                                      کد تایید ۴ رقمی پیامک شده:
                                    </label>
                                    <input
                                      type="text"
                                      value={certOtpCode}
                                      onChange={(e) => setCertOtpCode(e.target.value)}
                                      placeholder="کد ۴ رقمی"
                                      maxLength={6}
                                      className="w-full px-3 py-2 bg-gray-50 border border-gray-300 rounded-xl text-center text-lg font-mono font-bold tracking-widest focus:ring-2 focus:ring-emerald-500 outline-none"
                                      dir="ltr"
                                      autoFocus
                                    />
                                  </div>

                                  {certVerifyError && (
                                    <div className="bg-red-50 text-red-600 p-2 rounded-lg border border-red-200 text-xs font-bold flex items-center gap-1">
                                      <span>⚠️</span>
                                      <span>{certVerifyError}</span>
                                    </div>
                                  )}

                                  <div className="flex items-center justify-between pt-1">
                                    <div className="text-xs text-gray-600">
                                      {certOtpTimer > 0 ? (
                                        <span>
                                          زمان: <strong className="font-mono text-emerald-700 font-bold">{Math.floor(certOtpTimer / 60)}:{String(certOtpTimer % 60).padStart(2, '0')}</strong>
                                        </span>
                                      ) : (
                                        <button
                                          type="button"
                                          onClick={handleResendCertOTP}
                                          className="text-xs text-emerald-700 font-bold hover:underline"
                                        >
                                          🔄 ارسال مجدد کد
                                        </button>
                                      )}
                                    </div>

                                    <button
                                      type="button"
                                      onClick={handleConfirmCertOTP}
                                      disabled={certVerificationLoading || !certOtpCode.trim()}
                                      className="px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-white rounded-xl text-xs font-bold shadow-md transition-all disabled:opacity-50"
                                    >
                                      {certVerificationLoading ? 'در حال تایید...' : '✓ ثبت نهایی گواهی'}
                                    </button>
                                  </div>
                                </div>
                              )}
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  ) : (
                    /* وضعیت تایید شده گواهی */
                    <div className="p-3.5 bg-emerald-50 flex items-center justify-between text-xs">
                      <div className="flex items-center gap-2 text-emerald-900 font-bold">
                        <span>📜</span>
                        <span>
                          گواهی پس از اتمام جلسه برای {certNationalCode ? `کد ملی «${certNationalCode}»` : ''} {certPhone ? `و شماره همراه «${certPhone}»` : ''} صادر می‌گردد.
                        </span>
                      </div>
                      {isOtpVerified ? (
                        <span className="text-[11px] text-emerald-800 font-bold flex items-center gap-1 bg-emerald-100/80 px-2.5 py-1 rounded-lg border border-emerald-300">
                          <span>🔒</span> قفل شده با عضویت مجمع
                        </span>
                      ) : (
                        <button
                          type="button"
                          onClick={() => {
                            setWantCertificate(false);
                            setCertInfoSaved(false);
                            setIsCertOtpSent(false);
                            setCertOtpCode('');
                            setCertVerifyError('');
                            setCookie('wantCertificate', 'false', 30);
                          }}
                          className="text-xs text-red-500 hover:text-red-700 underline"
                        >
                          ویرایش
                        </button>
                      )}
                    </div>
                  )}
                </div>

                {/* حالت ورود به عنوان مدیر جلسه */}
                {meet?.isAdmin === true && (
                  <div className="flex items-center gap-2 pt-1">
                    <input
                      type="checkbox"
                      id="adminLoginCheckbox"
                      checked={isAdmin}
                      onChange={(e) => setIsAdmin(e.target.checked)}
                      className="w-4 h-4 rounded text-emerald-600 focus:ring-emerald-500"
                    />
                    <label htmlFor="adminLoginCheckbox" className="text-xs font-bold text-gray-700 cursor-pointer">
                      ورود با سطح دسترسی مدیر جلسه (ارائه‌دهنده)
                    </label>
                  </div>
                )}

                {/* دکمه پیوستن به تالار جلسه */}
                <button
                  type="button"
                  onClick={handleJoinMeeting}
                  disabled={loading || (isMember && !isOtpVerified)}
                  className={`w-full py-3.5 rounded-2xl font-black text-sm md:text-base flex items-center justify-center gap-2 shadow-lg transition-all duration-300 transform hover:scale-[1.01] active:scale-98 ${loading || (isMember && !isOtpVerified)
                      ? 'bg-gray-300 text-gray-500 cursor-not-allowed shadow-none'
                      : 'bg-emerald-700 bg-gradient-to-r from-emerald-700 via-teal-700 to-emerald-800 hover:from-emerald-800 hover:to-teal-800 text-white shadow-emerald-900/30'
                    }`}
                >
                  {loading ? (
                    <>
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                      در حال اتصال به جلسه...
                    </>
                  ) : (
                    <>
                      <span>🚀</span>
                      <span>ورود به جلسه آنلاین</span>
                    </>
                  )}
                </button>

                {isMember && !isOtpVerified && (
                  <p className="text-center text-xs text-amber-700 font-bold">
                    ⚠️ لطفاً ابتدا کد تایید ارسالی به همراه خود را تایید نمایید.
                  </p>
                )}
              </div>
            )}
          </>
        )}

        {/* فوتر صفحه */}
        <div className="mt-6 pt-4 border-t border-gray-100 text-center">
          <p className="text-[11px] text-gray-400 font-medium">
            مجمع عالی حکمت اسلامی — کلیه حقوق محفوظ است.
          </p>
        </div>
      </div>
    </div>
  );
}

export default JoinForm;