import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  joinMeeting,
  VerifyCertificateUser,
  ConfirmCertVerification,
  SendOTPRoomSubscribedUser,
  ConfirmOTPRoomSubscribedUser,
  DirectRoomSubscribedRegistration
} from '@/api/StartNewMeeting';
import {
  fetchRoom,
  fetchRoomLivesPublic,
  fetchRoomLivesPublicWithID,
  GetReservation,
  NewReservation,
  GetRoomDetails
} from '../api/GetRooms';
import Cookies from 'js-cookie';
import { useAuth } from '../Tools/AuthProvider';
import { useToast } from '../Tools/ToastProvider';
import {
  setCookieWithConfig,
  syncUserTokens,
  removeAuthToken,
  removeRegisteredUserToken,
  getEffectiveUserToken,
  BaseURL,
  BaseURLFiles
} from '../api/apiNeed';
import EmojiIcon from '../Component/EmojiIcon';
import { combinePath } from '../Tools/tools';
import { GetRoomMetadata } from '../api/AdminAPI';

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
  const { user, loadingAuth, verifyAndSetUser } = useAuth();

  // --- Meeting & Room States ---
  const [loading, setLoading] = useState(false);
  const [meet, setMeet] = useState(null);
  const [roomDetails, setRoomDetails] = useState(null);
  const [roomLive, setRoomLive] = useState([]);
  const [isMeetingLive, setIsMeetingLive] = useState(false);
  const [isCheckingLive, setIsCheckingLive] = useState(true);
  const [roomCover, setRoomCover] = useState('');
  const [nextSessionInfo, setNextSessionInfo] = useState('');
  const [isAdmin, setIsAdmin] = useState(false);

  // --- Active Tab: 'guest' | 'account' ---
  const [activeTab, setActiveTab] = useState('guest');

  // --- Section 1: Guest States ---
  const [guestName, setGuestName] = useState('');
  const [guestWantCert, setGuestWantCert] = useState(false);
  const [guestNationalCode, setGuestNationalCode] = useState('');
  const [guestPhone, setGuestPhone] = useState('');
  const [guestError, setGuestError] = useState('');

  // --- Section 2: Account Authentication States ---
  const [accountPhone, setAccountPhone] = useState('');
  const [accountNationalCode, setAccountNationalCode] = useState('');
  const [accountName, setAccountName] = useState('');
  const [isAccountOtpSent, setIsAccountOtpSent] = useState(false);
  const [accountOtpGuid, setAccountOtpGuid] = useState('');
  const [accountMaskedPhone, setAccountMaskedPhone] = useState('');
  const [accountOtpCode, setAccountOtpCode] = useState('');
  const [accountOtpTimer, setAccountOtpTimer] = useState(0);
  const [accountOtpLoading, setAccountOtpLoading] = useState(false);
  const [accountVerifyLoading, setAccountVerifyLoading] = useState(false);
  const [accountError, setAccountError] = useState('');

  // Account Join Name override (for logged in user)
  const [userDisplayName, setUserDisplayName] = useState('');

  // --- Subscribed Room States ---
  const [subNationalCode, setSubNationalCode] = useState('');
  const [isSubOtpSent, setIsSubOtpSent] = useState(false);
  const [subOtpCode, setSubOtpCode] = useState('');
  const [subOtpGuid, setSubOtpGuid] = useState('');
  const [subOtpLoading, setSubOtpLoading] = useState(false);
  const [subVerifyError, setSubVerifyError] = useState('');
  const [subVerificationLoading, setSubVerificationLoading] = useState(false);
  const [subMaskedPhone, setSubMaskedPhone] = useState('');

  // --- Next Session Reservation States ---
  const [reservation, setReservation] = useState(null);
  const [reservationName, setReservationName] = useState('');
  const [reservationPhone, setReservationPhone] = useState('');
  const [reservationNationalCode, setReservationNationalCode] = useState('');
  const [reservationLoading, setReservationLoading] = useState(false);
  const [showReservationForm, setShowReservationForm] = useState(false);

  // --- Initialize defaults from cookies or auth context ---
  useEffect(() => {
    const defaultName = getCookie('joinName') || '';
    const defaultPhone = getCookie('joinPhone') || '';
    const defaultNationalCode = getCookie('joinNationalCode') || '';
    const defaultWantCert = getCookie('wantCertificate') === 'true';

    if (defaultName) {
      setGuestName(defaultName);
      setAccountName(defaultName);
      setReservationName(defaultName);
    }
    if (defaultPhone) {
      setGuestPhone(defaultPhone);
      setAccountPhone(defaultPhone);
      setReservationPhone(defaultPhone);
    }
    if (defaultNationalCode) {
      setGuestNationalCode(defaultNationalCode);
      setAccountNationalCode(defaultNationalCode);
      setReservationNationalCode(defaultNationalCode);
    }
    if (defaultWantCert) {
      setGuestWantCert(true);
    }
  }, []);

  // Update states when user logs in or is already authenticated
  useEffect(() => {
    if (user) {
      setActiveTab('account');
      setUserDisplayName(user.fullname || user.name || guestName || '');
      if (user.phone || user.phoneNumber) {
        setAccountPhone(user.phone || user.phoneNumber);
      }
      if (user.nationalCode) {
        setAccountNationalCode(user.nationalCode);
      }
    }
  }, [user]);

  // Account OTP Countdown Timer
  useEffect(() => {
    let interval = null;
    if (accountOtpTimer > 0) {
      interval = setInterval(() => {
        setAccountOtpTimer((prev) => prev - 1);
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [accountOtpTimer]);

  // --- Fetch Room & Live status ---
  const fetchMeetingInfo = async () => {
    try {
      const data = await fetchRoom(Mid);
      if (data) {
        setMeet(data);
        if (data.title) {
          document.title = `مجمع عالی حکمت اسلامی | ${data.title}`;
        }
      }

      try {
        const details = await GetRoomDetails(Mid);
        setRoomDetails({
          ...details,
          subscriptionStatus: data?.subscriptionStatus ?? details?.subscriptionStatus,
          isSubscribedByUser: data?.isSubscribedByUser ?? details?.isSubscribedByUser,
        });
      } catch (detailErr) {
        console.warn('Could not fetch extra room details:', detailErr);
        if (data) {
          setRoomDetails({
            ...data,
            subscriptionStatus: data.subscriptionStatus,
            isSubscribedByUser: data.isSubscribedByUser,
          });
        }
      }

      // Fetch image_cover metadata if available
      try {
        const metaRes = await GetRoomMetadata(Mid, 'image_cover');
        if (metaRes && (metaRes.value || metaRes.data)) {
          setRoomCover(metaRes.value || metaRes.data);
        }
      } catch (metaErr) {
        // Non-blocking fallback
      }
    } catch (error) {
      console.error('Error fetching meeting info:', error);
      setMeet(null);
      setRoomDetails(null);
    }
  };

  const checkLives = async (isInitial = false) => {
    try {
      let livedata = await fetchRoomLivesPublicWithID(Mid);
      if (!Array.isArray(livedata) || livedata.length === 0) {
        livedata = await fetchRoomLivesPublic();
      }
      const list = livedata || [];
      setRoomLive(list);
      const liveStatus = list.some((room) => room.roomID === Mid || room.RoomID === Mid || room.friendlyID === Mid);
      setIsMeetingLive(liveStatus);
    } catch (error) {
      console.error('Error fetching live rooms:', error);
    } finally {
      if (isInitial) {
        setIsCheckingLive(false);
      }
    }
  };

  useEffect(() => {
    setIsCheckingLive(true);
    // 1. Immediately check live status in parallel on mount so user doesn't wait
    checkLives(true);
    // 2. Fetch room details in parallel
    fetchMeetingInfo();

    const intervalId = setInterval(() => checkLives(false), 15000);
    return () => clearInterval(intervalId);
  }, [Mid]);

  useEffect(() => {
    if (roomLive && roomLive.length > 0) {
      const liveStatus = roomLive.some((room) => room.roomID === Mid || room.RoomID === Mid || room.friendlyID === Mid);
      setIsMeetingLive(liveStatus);
    }
  }, [roomLive, Mid]);

  useEffect(() => {
    if (meet?.nextPlan) {
      fetchReservation(meet.nextPlan.id);
    }
  }, [meet]);

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

  // --- Next Session String Formatting ---
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
          dateString = new Intl.DateTimeFormat('fa-IR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
          }).format(nextMeetDate);
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

  // --- Name Validation Helper ---
  const validatePersianName = (name) => {
    if (!name || name.trim() === '') {
      return { isValid: false, message: 'لطفاً نام و نام خانوادگی را وارد کنید' };
    }
    const cleanName = name.trim();
    const words = cleanName.split(/\s+/);
    if (words.length < 2) {
      return { isValid: false, message: 'لطفاً نام و نام خانوادگی را به صورت کامل (حداقل دو بخش) وارد کنید' };
    }
    const persianRegex = /^[\u0600-\u06FF\s]+$/;
    if (!persianRegex.test(cleanName)) {
      return { isValid: false, message: 'نام و نام خانوادگی باید با حروف فارسی وارد شود' };
    }
    return { isValid: true, message: '' };
  };

  // ==========================================
  // Section 1: Guest Join Handler
  // ==========================================
  const handleJoinAsGuest = async () => {
    setGuestError('');
    const cleanName = guestName.trim();

    if (!cleanName) {
      setGuestError('لطفاً نام و نام خانوادگی خود را وارد نمایید');
      return;
    }

    let natCode = null;
    let phone = null;

    if (guestWantCert) {
      const nameCheck = validatePersianName(cleanName);
      if (!nameCheck.isValid) {
        setGuestError(nameCheck.message);
        return;
      }

      const cleanNatCode = toEnglishDigits(guestNationalCode).trim();
      if (!cleanNatCode || cleanNatCode.length !== 10 || !/^\d{10}$/.test(cleanNatCode)) {
        setGuestError('جهت صدور گواهی، وارد کردن کد ملی معتبر (۱۰ رقم) الزامی است');
        return;
      }
      natCode = cleanNatCode;

      const cleanPhone = toEnglishDigits(guestPhone).trim();
      if (cleanPhone) {
        if (!/^(\+98|0|0098)?9\d{9}$/.test(cleanPhone)) {
          setGuestError('شماره همراه وارد شده نامعتبر است (مثال: 09123456789)');
          return;
        }
        phone = cleanPhone;
      }

      // Save cookie state
      setCookie('wantCertificate', 'true', 30);
      setCookie('joinNationalCode', cleanNatCode, 30);
      if (cleanPhone) setCookie('joinPhone', cleanPhone, 30);
    } else {
      setCookie('wantCertificate', 'false', 30);
    }

    setCookie('joinName', cleanName, 30);

    setLoading(true);
    try {
      const livedata = await fetchRoomLivesPublicWithID(Mid);
      setRoomLive(livedata || []);
      const liveStatus = (livedata || []).some((room) => room.roomID === Mid);

      if (!liveStatus) {
        setLoading(false);
        alert('جلسه هنوز آغاز نشده است. لطفاً پس از شروع جلسه مجدداً تلاش نمایید.');
        return;
      }

      const res = await joinMeeting(
        Mid,
        cleanName,
        isAdmin,
        '123',
        natCode,
        phone
      );

      if (res.status !== 200) {
        if (res.status === 500) {
          window.location.reload();
        } else {
          toast?.error?.(res.message || 'خطا در ورود به جلسه');
        }
      } else if (res.link) {
        window.open(res.link, '_blank');
      }
    } catch (error) {
      console.error('Error joining meeting as guest:', error);
      alert('خطا در ورود به جلسه. لطفاً مجدداً تلاش فرمایید.');
    } finally {
      setLoading(false);
    }
  };

  // ==========================================
  // Section 2: Account Authentication Handlers
  // ==========================================
  const handleSendAccountOTP = async () => {
    setAccountError('');
    const cleanPhone = toEnglishDigits(accountPhone).trim();
    const cleanNatCode = toEnglishDigits(accountNationalCode).trim();
    const cleanName = accountName.trim();

    if (!cleanPhone || !/^(\+98|0|0098)?9\d{9}$/.test(cleanPhone)) {
      setAccountError('لطفاً شماره همراه معتبر وارد کنید (مثال: 09123456789)');
      return;
    }

    if (!cleanNatCode || cleanNatCode.length !== 10 || !/^\d{10}$/.test(cleanNatCode)) {
      setAccountError('کد ملی باید دقیقاً ۱۰ رقم عددی باشد');
      return;
    }

    setAccountOtpLoading(true);
    try {
      const response = await VerifyCertificateUser({
        phone: cleanPhone,
        nationalCode: cleanNatCode,
        name: cleanName,
        uuid: getEffectiveUserToken()
      });

      if (response.status === 200 && response.guid) {
        setAccountOtpGuid(response.guid);
        setIsAccountOtpSent(true);
        setAccountOtpTimer(120);
        setAccountMaskedPhone(response.maskedPhone || cleanPhone);
        if (response.phone) {
          setAccountPhone(response.phone);
        }
        setAccountError('');
        toast?.success?.('کد تایید پیامکی با موفقیت ارسال شد');
      } else {
        setAccountError(response.message || 'خطا در بررسی اطلاعات یا ارسال کد تایید');
      }
    } catch (error) {
      console.error('Error sending account OTP:', error);
      setAccountError('خطا در برقراری ارتباط با سرور. لطفاً مجدداً تلاش نمایید.');
    } finally {
      setAccountOtpLoading(false);
    }
  };

  const handleConfirmAccountOTP = async () => {
    setAccountError('');
    const cleanOtp = toEnglishDigits(accountOtpCode).trim();

    if (!cleanOtp || cleanOtp.length < 4) {
      setAccountError('لطفاً کد تایید ۴ رقمی پیامک‌شده را وارد کنید');
      return;
    }

    const cleanPhone = toEnglishDigits(accountPhone).trim();
    const cleanNatCode = toEnglishDigits(accountNationalCode).trim();
    const cleanName = accountName.trim();

    setAccountVerifyLoading(true);
    try {
      const response = await ConfirmCertVerification({
        guid: accountOtpGuid,
        code: cleanOtp,
        phone: cleanPhone,
        nationalCode: cleanNatCode,
        name: cleanName
      });

      if (response.status === 200 && response.token) {
        setCookieWithConfig('vc2AuthNorm', response.token, 365 * 10);
        syncUserTokens();

        const verifiedName = response.fullname || cleanName;
        const verifiedPhone = response.phone || cleanPhone;
        const verifiedNatCode = response.nationalCode || cleanNatCode;

        setCookie('joinName', verifiedName, 30);
        setCookie('joinPhone', verifiedPhone, 30);
        setCookie('joinNationalCode', verifiedNatCode, 30);
        setCookie('wantCertificate', 'true', 30);

        setUserDisplayName(verifiedName);
        setIsAccountOtpSent(false);
        setAccountOtpCode('');
        setAccountError('');

        toast?.success?.(`خوش آمدید، ${verifiedName}`);

        if (verifyAndSetUser) {
          await verifyAndSetUser();
        } else {
          window.location.reload();
        }
      } else {
        setAccountError(response.message || 'کد تایید وارد شده نادرست یا منقضی شده است');
      }
    } catch (error) {
      console.error('Error confirming account OTP:', error);
      setAccountError('خطا در تایید کد. لطفاً دوباره تلاش نمایید.');
    } finally {
      setAccountVerifyLoading(false);
    }
  };

  const handleResendAccountOTP = () => {
    if (accountOtpTimer === 0) {
      setAccountOtpCode('');
      handleSendAccountOTP();
    }
  };

  const handleLogoutAccount = () => {
    if (window.confirm('آیا از خروج از این حساب کاربری اطمینان دارید؟')) {
      removeAuthToken();
      removeRegisteredUserToken();
      Cookies.remove('vc2AuthNorm');
      Cookies.remove('joinName');
      Cookies.remove('joinPhone');
      Cookies.remove('joinNationalCode');
      setIsAccountOtpSent(false);
      setAccountOtpCode('');
      setAccountError('');
      window.location.reload();
    }
  };

  // Join meeting with Authenticated Account
  const handleJoinAsAccount = async () => {
    const finalDisplayName = (userDisplayName || user?.fullname || accountName || 'عضو سامانه').trim();

    if (!finalDisplayName) {
      alert('لطفاً نام نمایشی خود را وارد فرمایید.');
      return;
    }

    setLoading(true);
    try {
      const livedata = await fetchRoomLivesPublicWithID(Mid);
      setRoomLive(livedata || []);
      const liveStatus = (livedata || []).some((room) => room.roomID === Mid);

      if (!liveStatus) {
        setLoading(false);
        alert('جلسه هنوز آغاز نشده است. لطفاً پس از شروع جلسه مجدداً تلاش نمایید.');
        return;
      }

      setCookie('joinName', finalDisplayName, 30);

      const finalNatCode = user?.nationalCode || accountNationalCode || null;
      const finalPhone = user?.phone || user?.phoneNumber || accountPhone || null;

      const res = await joinMeeting(
        Mid,
        finalDisplayName,
        isAdmin,
        '123',
        finalNatCode,
        finalPhone
      );

      if (res.status !== 200) {
        if (res.status === 500) {
          window.location.reload();
        } else {
          toast?.error?.(res.message || 'خطا در ورود به جلسه');
        }
      } else if (res.link) {
        window.open(res.link, '_blank');
      }
    } catch (error) {
      console.error('Error joining meeting with account:', error);
      alert('خطا در ورود به جلسه. لطفاً مجدداً تلاش فرمایید.');
    } finally {
      setLoading(false);
    }
  };

  // ==========================================
  // Private Room Subscription Handlers
  // ==========================================
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
        setTimeout(() => window.location.reload(), 1500);
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
      console.error('Error sending Sub OTP:', error);
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
      console.error('Error confirming Sub OTP:', error);
      setSubVerifyError('خطا در تایید کد. لطفاً دوباره تلاش کنید.');
    } finally {
      setSubVerificationLoading(false);
    }
  };

  // ==========================================
  // Reservation Handler
  // ==========================================
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

  if (loadingAuth) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-emerald-400 border-t-emerald-700 rounded-full animate-spin"></div>
      </div>
    );
  }

  // Helper to safely resolve relative or absolute image URLs
  const resolveImageUrl = (img) => {
    if (!img || typeof img !== 'string' || img.trim() === '') return '';
    const clean = img.trim();
    if (
      clean.startsWith('http://') ||
      clean.startsWith('https://') ||
      clean.startsWith('data:') ||
      clean.startsWith('blob:')
    ) {
      return clean;
    }
    if (clean.startsWith('/uploads/') || clean.startsWith('/img/') || clean.startsWith('/custom/')) {
      return combinePath(BaseURLFiles, clean);
    }
    return combinePath(BaseURL, clean);
  };

  // Determine background image from metadata or cover
  const rawBgImage =
    roomCover ||
    roomDetails?.metadatas?.image_cover ||
    roomDetails?.metadatas?.['image_cover'] ||
    roomDetails?.coverImage ||
    meet?.coverImage;

  const bgImageUrl = resolveImageUrl(rawBgImage);
  const bgStyle = bgImageUrl
    ? {
        backgroundImage: `url(${bgImageUrl})`,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundRepeat: 'no-repeat',
      }
    : {};

  // Determine dynamic room logo or avatar
  const roomLogo = roomDetails?.logo || meet?.logo;
  const roomAvatar = roomDetails?.avatar || meet?.avatar;

  const isUserVIP = Boolean(user && (user.isVIPMember || user.isMemberVIP));

  return (
    <div
      className={`min-h-screen flex items-center justify-center px-4 py-8 sm:py-12 relative overflow-hidden ${
        !bgImageUrl ? 'bg-gradient-to-br from-[#06241c] via-[#093529] to-[#0f4d3d]' : 'bg-[#06241c]'
      }`}
      style={bgStyle}
      dir="rtl"
    >
      {/* Background Overlay */}
      {bgImageUrl && (
        <div className="absolute inset-0 bg-gradient-to-br from-black/80 via-[#06241c]/75 to-black/85 backdrop-blur-[2px] z-0 pointer-events-none"></div>
      )}

      {!bgImageUrl && (
        <>
          <div className="absolute -top-32 -right-32 w-96 h-96 bg-emerald-500/10 rounded-full blur-3xl pointer-events-none"></div>
          <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-teal-500/10 rounded-full blur-3xl pointer-events-none"></div>
          <div className="absolute inset-0 bg-[radial-gradient(#ffffff0a_1px,transparent_1px)] [background-size:20px_20px] pointer-events-none"></div>
        </>
      )}

      <div className="w-full max-w-lg mx-auto bg-white/98 backdrop-blur-xl rounded-3xl shadow-2xl shadow-black/40 border border-white/20 p-6 sm:p-8 relative z-10 transition-all duration-300">

        {/* Header and Logo */}
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-tr from-[#0b3f32] to-[#166534] rounded-2xl mb-3 shadow-lg shadow-emerald-950/20 border-2 border-emerald-400/30 transform transition-transform hover:scale-105 overflow-hidden">
            {roomLogo ? (
              <img
                src={resolveImageUrl(roomLogo)}
                alt={meet?.title || 'لوگوی جلسه'}
                className="w-full h-full object-contain p-2"
                onError={(e) => {
                  e.currentTarget.style.display = 'none';
                  if (e.currentTarget.nextSibling) {
                    e.currentTarget.nextSibling.style.display = 'flex';
                  }
                }}
              />
            ) : roomAvatar ? (
              <EmojiIcon
                name={roomAvatar}
                className="w-14 h-14 object-contain"
                style={{ width: '3.5rem', height: '3.5rem' }}
              />
            ) : (
              <span className="text-3xl">🏛️</span>
            )}
            {roomLogo && (
              <span className="text-3xl hidden items-center justify-center w-full h-full">🏛️</span>
            )}
          </div>
          <h1 className="text-xl sm:text-2xl font-black text-gray-900 tracking-tight">
            {meet?.title || 'مجمع عالی حکمت اسلامی'}
          </h1>
          <p className="text-xs sm:text-sm text-gray-500 mt-1 font-medium">
            سامانه ورود به تالار نشست‌ها و کلاس‌های تخصصی
          </p>
        </div>

        {/* Room Specific Check: Subscribed Room */}
        {meet?.isSubscribed && !meet?.isAdmin ? (
          user && roomDetails?.isSubscribedByUser ? (
            roomDetails?.subscriptionStatus === 100 ? (
              <div className="bg-white p-5 rounded-2xl border-2 border-emerald-200 text-center space-y-5 shadow-sm mt-4">
                <div className="text-5xl drop-shadow-md">🔒</div>
                <p className="font-black text-lg text-emerald-700 leading-snug">شما عضو تایید شده این کلاس هستید</p>
                <div className="space-y-4 pt-2">
                  <input
                    type="text"
                    value={user.fullname || userDisplayName}
                    disabled
                    className="w-full px-4 py-3.5 bg-gray-100 border-2 border-gray-200 rounded-xl text-center font-bold text-lg text-gray-500 cursor-not-allowed outline-none"
                  />
                  <div className="relative group w-full">
                    <button
                      onClick={handleJoinAsAccount}
                      disabled={loading || !isMeetingLive}
                      className="w-full py-3.5 bg-emerald-700 bg-gradient-to-r from-emerald-600 to-emerald-700 hover:from-emerald-700 hover:to-emerald-800 text-white font-black rounded-xl disabled:opacity-50 transition-all shadow-md flex items-center justify-center gap-2"
                    >
                      {loading ? 'در حال اتصال...' : 'ورود به کلاس 🚀'}
                    </button>

                    {!isMeetingLive && !loading && (
                      <div
                        className="pointer-events-none absolute bottom-full right-1/2 translate-x-1/2 mb-2 
                 opacity-0 group-hover:opacity-100 transition-opacity duration-200
                 bg-gray-900 text-white text-xs font-bold px-3 py-2 rounded-lg shadow-lg whitespace-nowrap z-50"
                      >
                        پخش زنده‌ای در حال اجرا نمی‌باشد
                        <span className="absolute top-full right-1/2 translate-x-1/2 -mt-1 border-4 border-transparent border-t-gray-900"></span>
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
            {/* Room Information Card */}
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
                {isCheckingLive ? (
                  <span className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-amber-50 text-amber-800 rounded-full text-xs font-bold border border-amber-200/80 shadow-sm flex-shrink-0 animate-pulse">
                    <span className="w-2 h-2 rounded-full bg-amber-500 animate-ping"></span>
                    در حال بررسی وضعیت...
                  </span>
                ) : isMeetingLive ? (
                  <span className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-red-500 text-white rounded-full text-xs font-black shadow-md animate-pulse flex-shrink-0">
                    <span className="w-2 h-2 rounded-full bg-white"></span>
                    زنده (آنلاین)
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-gray-100 text-gray-600 rounded-full text-xs font-semibold border border-gray-200 flex-shrink-0">
                    غیرفعال
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

            {/* Live Banner or Next Session Card */}
            {isCheckingLive ? (
              <div className="bg-gradient-to-r from-amber-50 to-orange-50/60 border border-amber-200/80 rounded-2xl p-4 mb-5 text-amber-900 shadow-sm flex items-center gap-3 animate-pulse">
                <div className="w-10 h-10 rounded-xl bg-amber-100 flex items-center justify-center text-lg flex-shrink-0 text-amber-700">
                  <svg className="animate-spin h-5 w-5 text-amber-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path>
                  </svg>
                </div>
                <div className="min-w-0 flex-1">
                  <p className="font-bold text-xs sm:text-sm text-amber-950">در حال بررسی وضعیت زنده جلسه...</p>
                  <p className="text-xs text-amber-700/80 mt-0.5 font-medium">لطفاً چند لحظه شکیبا باشید</p>
                </div>
              </div>
            ) : isMeetingLive ? (
              <div className="bg-gradient-to-r from-emerald-600 to-teal-700 text-white p-4 rounded-2xl mb-5 shadow-lg shadow-emerald-900/20 flex items-center gap-3 animate-fadeIn">
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
                      جهت حضور در نشست، مشخصات خود را در کادر زیر وارد فرمایید.
                    </p>
                  )}
                </div>
              </div>
            ) : (
              <div className="bg-gray-50 border border-gray-200 rounded-2xl p-4 mb-5 text-gray-700 space-y-2.5 animate-fadeIn">
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

                {/* Pre-registration / Reservation Card */}
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
                          className="w-full px-3 py-2 border border-gray-200 rounded-xl text-xs focus:ring-2 focus:ring-emerald-500 outline-none"
                          dir="ltr"
                        />
                        <input
                          type="text"
                          value={reservationNationalCode}
                          onChange={(e) => setReservationNationalCode(e.target.value)}
                          placeholder="کد ملی ۱۰ رقمی"
                          className="w-full px-3 py-2 border border-gray-200 rounded-xl text-xs focus:ring-2 focus:ring-emerald-500 outline-none"
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

            {/* ONLY RENDER THE JOIN FORM SECTIONS IF MEETING IS LIVE */}
            {isCheckingLive ? (
              <div className="p-8 bg-gray-50/70 rounded-2xl border border-gray-200/80 flex flex-col items-center justify-center gap-3 text-gray-500 my-4 animate-pulse">
                <div className="w-8 h-8 border-3 border-emerald-600 border-t-transparent rounded-full animate-spin"></div>
                <span className="text-xs font-bold text-gray-600">در حال بررسی و دریافت وضعیت نشست آنلاین...</span>
              </div>
            ) : isMeetingLive && (
              <div className="space-y-5">

                {/* Segmented Tab Bar: 2 Simplified Sections */}
                <div className="grid grid-cols-2 p-1.5 bg-gray-100/90 rounded-2xl border border-gray-200">
                  <button
                    type="button"
                    onClick={() => setActiveTab('guest')}
                    className={`py-2.5 px-3 rounded-xl font-black text-xs sm:text-sm transition-all flex items-center justify-center gap-1.5 ${
                      activeTab === 'guest'
                        ? 'bg-white text-emerald-900 shadow-md border border-gray-200/80 scale-[1.02]'
                        : 'text-gray-500 hover:text-gray-800'
                    }`}
                  >
                    <span>👤</span>
                    <span>ورود به عنوان مهمان</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => setActiveTab('account')}
                    className={`py-2.5 px-3 rounded-xl font-black text-xs sm:text-sm transition-all flex items-center justify-center gap-1.5 ${
                      activeTab === 'account'
                        ? 'bg-white text-emerald-900 shadow-md border border-gray-200/80 scale-[1.02]'
                        : 'text-gray-500 hover:text-gray-800'
                    }`}
                  >
                    <span>🔐</span>
                    <span>{user ? 'حساب کاربری من' : 'ورود / ثبت‌نام حساب'}</span>
                    {user && (
                      <span className="w-2 h-2 rounded-full bg-emerald-500"></span>
                    )}
                  </button>
                </div>

                {/* ======================================================== */}
                {/* SECTION 1: ورود به عنوان مهمان (فقط نام + تیک گواهی)     */}
                {/* ======================================================== */}
                {activeTab === 'guest' && (
                  <div className="space-y-4 animate-in fade-in duration-200">
                    <div>
                      <label className="block text-xs font-bold text-gray-700 mb-1.5">
                        نام و نام خانوادگی شما در جلسه: <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="text"
                        value={guestName}
                        onChange={(e) => {
                          setGuestName(e.target.value);
                          if (guestError) setGuestError('');
                        }}
                        placeholder="مثال: علی رضایی"
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-2xl text-sm font-medium text-gray-900 focus:bg-white focus:ring-2 focus:ring-emerald-500 outline-none transition-all shadow-inner"
                        autoFocus
                      />
                    </div>

                    {/* Checkbox: مایل به دریافت گواهی هستم */}
                    <div className="bg-gray-50/90 rounded-2xl border border-gray-200/90 p-4 transition-all">
                      <label className="flex items-center gap-2.5 cursor-pointer select-none">
                        <input
                          type="checkbox"
                          checked={guestWantCert}
                          onChange={(e) => {
                            setGuestWantCert(e.target.checked);
                            if (guestError) setGuestError('');
                          }}
                          className="w-5 h-5 rounded-lg border-2 border-emerald-500 text-emerald-600 focus:ring-emerald-500 accent-emerald-600 cursor-pointer"
                        />
                        <div>
                          <span className="font-bold text-xs sm:text-sm text-gray-900 flex items-center gap-1.5">
                            <span>📜</span> مایل به دریافت گواهی رسمی حضور در جلسه هستم
                          </span>
                          <span className="text-[11px] text-gray-500 block mt-0.5">
                            ثبت کد ملی جهت صدور گواهی معتبر پس از پایان جلسه
                          </span>
                        </div>
                      </label>

                      {/* Expanded inputs for certificate */}
                      {guestWantCert && (
                        <div className="mt-3.5 pt-3 border-t border-gray-200 space-y-3 animate-in fade-in">
                          <div>
                            <label className="block text-[11px] font-bold text-gray-700 mb-1">
                              کد ملی ۱۰ رقمی (جهت درج در گواهی): <span className="text-red-500">*</span>
                            </label>
                            <input
                              type="text"
                              value={guestNationalCode}
                              onChange={(e) => {
                                setGuestNationalCode(e.target.value);
                                if (guestError) setGuestError('');
                              }}
                              placeholder="مثال: 0012345678"
                              maxLength={10}
                              className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-xl text-sm font-mono focus:ring-2 focus:ring-emerald-500 outline-none"
                              dir="ltr"
                            />
                          </div>

                          <div>
                            <label className="block text-[11px] font-bold text-gray-700 mb-1">
                              شماره همراه (اختیاری - جهت اطلاع‌رسانی پیامکی):
                            </label>
                            <input
                              type="tel"
                              value={guestPhone}
                              onChange={(e) => {
                                setGuestPhone(e.target.value);
                                if (guestError) setGuestError('');
                              }}
                              placeholder="مثال: 09123456789"
                              className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-xl text-sm font-mono focus:ring-2 focus:ring-emerald-500 outline-none"
                              dir="ltr"
                            />
                          </div>
                        </div>
                      )}
                    </div>

                    {guestError && (
                      <div className="bg-red-50 text-red-600 p-2.5 rounded-xl border border-red-200 text-xs font-bold flex items-center gap-1.5">
                        <span>⚠️</span>
                        <span>{guestError}</span>
                      </div>
                    )}

                    {/* Admin Checkbox if Applicable */}
                    {meet?.isAdmin === true && (
                      <div className="flex items-center gap-2 pt-1">
                        <input
                          type="checkbox"
                          id="adminGuestCheckbox"
                          checked={isAdmin}
                          onChange={(e) => setIsAdmin(e.target.checked)}
                          className="w-4 h-4 rounded text-emerald-600 focus:ring-emerald-500"
                        />
                        <label htmlFor="adminGuestCheckbox" className="text-xs font-bold text-gray-700 cursor-pointer">
                          ورود با دسترسی مدیر جلسه (ارائه‌دهنده)
                        </label>
                      </div>
                    )}

                    {/* One-Click Guest Join Button */}
                    <button
                      type="button"
                      onClick={handleJoinAsGuest}
                      disabled={loading || !guestName.trim()}
                      className={`w-full py-3.5 rounded-2xl font-black text-sm md:text-base flex items-center justify-center gap-2 shadow-lg transition-all duration-300 transform hover:scale-[1.01] active:scale-98 ${
                        loading || !guestName.trim()
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
                  </div>
                )}

                {/* ======================================================== */}
                {/* SECTION 2: ثبت‌نام یا ورود با حساب کاربری                */}
                {/* ======================================================== */}
                {activeTab === 'account' && (
                  <div className="space-y-4 animate-in fade-in duration-200">

                    {/* Case 2A: User is ALREADY Authenticated */}
                    {user ? (
                      <div className="bg-gradient-to-br from-emerald-50/90 via-teal-50/50 to-white rounded-2xl p-4 sm:p-5 border border-emerald-200 shadow-sm space-y-4">
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <div className="w-11 h-11 rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-700 text-white flex items-center justify-center font-black text-lg shadow-sm">
                              {user.fullname ? user.fullname.trim()[0] : '👤'}
                            </div>
                            <div>
                              <p className="font-black text-sm text-gray-900">
                                {user.fullname || 'کاربر احراز هویت شده'}
                              </p>
                              <p className="text-[11px] text-gray-500 font-mono mt-0.5" dir="ltr">
                                {user.phone || user.phoneNumber || accountPhone}
                              </p>
                            </div>
                          </div>

                          <button
                            type="button"
                            onClick={handleLogoutAccount}
                            className="text-xs text-red-500 hover:text-red-700 font-bold underline px-2 py-1"
                          >
                            خروج از حساب
                          </button>
                        </div>

                        {/* Badges: Member VIP & Certificate */}
                        <div className="flex flex-wrap gap-2 pt-1 border-t border-emerald-100">
                          {isUserVIP ? (
                            <span className="inline-flex items-center gap-1.5 px-3 py-1 bg-amber-100/90 text-amber-900 border border-amber-300 rounded-full text-xs font-black shadow-xs">
                              <span>⭐</span> عضو رسمی مجمع عالی حکمت اسلامی
                            </span>
                          ) : (
                            <span className="inline-flex items-center gap-1.5 px-3 py-1 bg-emerald-100 text-emerald-800 border border-emerald-300 rounded-full text-xs font-bold">
                              <span>✓</span> کاربر ثبت‌نام شده سامانه
                            </span>
                          )}

                          <span className="inline-flex items-center gap-1 px-2.5 py-1 bg-teal-50 text-teal-800 border border-teal-200 rounded-full text-[11px] font-bold">
                            <span>📜</span> صدور خودکار گواهی به کد ملی {user.nationalCode || accountNationalCode || 'ثبت شده'}
                          </span>
                        </div>

                        {/* Editable Display Name inside meeting */}
                        <div className="pt-2">
                          <label className="block text-xs font-bold text-gray-700 mb-1">
                            نام نمایشی شما در جلسه:
                          </label>
                          <input
                            type="text"
                            value={userDisplayName}
                            onChange={(e) => setUserDisplayName(e.target.value)}
                            placeholder="نام و نام خانوادگی در جلسه"
                            className="w-full px-3.5 py-2.5 bg-white border border-emerald-300 rounded-xl text-sm font-bold text-emerald-950 focus:ring-2 focus:ring-emerald-500 outline-none"
                          />
                        </div>

                        {/* Admin Checkbox if Applicable */}
                        {meet?.isAdmin === true && (
                          <div className="flex items-center gap-2 pt-1">
                            <input
                              type="checkbox"
                              id="adminAccountCheckbox"
                              checked={isAdmin}
                              onChange={(e) => setIsAdmin(e.target.checked)}
                              className="w-4 h-4 rounded text-emerald-600 focus:ring-emerald-500"
                            />
                            <label htmlFor="adminAccountCheckbox" className="text-xs font-bold text-gray-700 cursor-pointer">
                              ورود با دسترسی مدیر جلسه (ارائه‌دهنده)
                            </label>
                          </div>
                        )}

                        {/* One-Click Join with Account */}
                        <button
                          type="button"
                          onClick={handleJoinAsAccount}
                          disabled={loading}
                          className="w-full py-3.5 rounded-2xl font-black text-sm md:text-base flex items-center justify-center gap-2 shadow-lg transition-all duration-300 transform hover:scale-[1.01] active:scale-98 bg-emerald-700 bg-gradient-to-r from-emerald-700 via-teal-700 to-emerald-800 hover:from-emerald-800 hover:to-teal-800 text-white shadow-emerald-900/30"
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
                              <span>ورود به جلسه با این حساب کاربری</span>
                            </>
                          )}
                        </button>
                      </div>
                    ) : (
                      /* Case 2B: User is NOT Authenticated -> Phone + National Code Flow */
                      <div className="bg-gray-50/90 rounded-2xl border border-gray-200 p-4 sm:p-5 space-y-3.5">
                        <div className="text-xs text-gray-600 leading-relaxed font-medium">
                          💡 با ثبت شماره همراه و کد ملی، حساب شما شناسایی و ایجاد خواهد شد و گواهی‌ها به صورت خودکار صادر می‌گردند.
                        </div>

                        {!isAccountOtpSent ? (
                          /* STEP 1: Phone + National Code + Optional Name */
                          <div className="space-y-3 pt-1">
                            <div>
                              <label className="block text-xs font-bold text-gray-700 mb-1">
                                شماره همراه: <span className="text-red-500">*</span>
                              </label>
                              <input
                                type="tel"
                                value={accountPhone}
                                onChange={(e) => {
                                  setAccountPhone(e.target.value);
                                  if (accountError) setAccountError('');
                                }}
                                placeholder="مثلاً: 09123456789"
                                className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-xl text-sm font-mono focus:ring-2 focus:ring-emerald-500 outline-none"
                                dir="ltr"
                                autoFocus
                              />
                            </div>

                            <div>
                              <label className="block text-xs font-bold text-gray-700 mb-1">
                                کد ملی (۱۰ رقم): <span className="text-red-500">*</span>
                              </label>
                              <input
                                type="text"
                                value={accountNationalCode}
                                onChange={(e) => {
                                  setAccountNationalCode(e.target.value);
                                  if (accountError) setAccountError('');
                                }}
                                placeholder="مثلاً: 0012345678"
                                maxLength={10}
                                className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-xl text-sm font-mono focus:ring-2 focus:ring-emerald-500 outline-none"
                                dir="ltr"
                              />
                            </div>

                            <div>
                              <label className="block text-xs font-bold text-gray-700 mb-1">
                                نام و نام خانوادگی (جهت ثبت حساب جدید):
                              </label>
                              <input
                                type="text"
                                value={accountName}
                                onChange={(e) => {
                                  setAccountName(e.target.value);
                                  if (accountError) setAccountError('');
                                }}
                                placeholder="مثلاً: دکتر علی حسینی"
                                className="w-full px-3.5 py-2.5 bg-white border border-gray-300 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500 outline-none"
                              />
                            </div>

                            {accountError && (
                              <div className="bg-red-50 text-red-600 p-2.5 rounded-xl border border-red-200 text-xs font-bold flex items-center gap-1.5">
                                <span>⚠️</span>
                                <span>{accountError}</span>
                              </div>
                            )}

                            <button
                              type="button"
                              onClick={handleSendAccountOTP}
                              disabled={accountOtpLoading || !accountPhone.trim() || !accountNationalCode.trim()}
                              className="w-full py-3 bg-emerald-700 bg-gradient-to-r from-emerald-600 to-teal-700 hover:from-emerald-700 hover:to-teal-800 text-white rounded-xl text-xs sm:text-sm font-black shadow-md transition-all disabled:opacity-50 flex items-center justify-center gap-2"
                            >
                              {accountOtpLoading ? (
                                <>
                                  <svg className="animate-spin h-4 w-4 text-white" viewBox="0 0 24 24">
                                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                                  </svg>
                                  در حال ارسال کد تایید...
                                </>
                              ) : (
                                'دریافت کد تایید پیامکی ✉️'
                              )}
                            </button>
                          </div>
                        ) : (
                          /* STEP 2: Enter 4-Digit OTP Code */
                          <div className="space-y-3 bg-white p-3.5 rounded-xl border border-emerald-200 animate-in fade-in">
                            <div className="bg-emerald-50 p-2.5 rounded-lg border border-emerald-200 text-xs text-emerald-950 flex items-center justify-between">
                              <div className="flex items-center gap-1.5">
                                <span>📱</span>
                                <span>کد به شماره <strong className="font-mono text-sm font-black" dir="ltr">{accountMaskedPhone || accountPhone}</strong> ارسال شد</span>
                              </div>
                              <button
                                type="button"
                                onClick={() => {
                                  setIsAccountOtpSent(false);
                                  setAccountOtpCode('');
                                  setAccountError('');
                                }}
                                className="text-[11px] text-emerald-700 hover:text-emerald-900 underline font-bold"
                              >
                                ویرایش مشخصات
                              </button>
                            </div>

                            <div>
                              <label className="block text-xs font-bold text-gray-700 mb-1">
                                کد تایید ۴ رقمی:
                              </label>
                              <input
                                type="text"
                                value={accountOtpCode}
                                onChange={(e) => {
                                  setAccountOtpCode(e.target.value);
                                  if (accountError) setAccountError('');
                                }}
                                placeholder="کد ۴ رقمی"
                                maxLength={6}
                                className="w-full px-3.5 py-2.5 bg-gray-50 border border-gray-300 rounded-xl text-center text-xl font-mono font-bold tracking-widest focus:ring-2 focus:ring-emerald-500 outline-none"
                                dir="ltr"
                                autoFocus
                              />
                            </div>

                            {accountError && (
                              <div className="bg-red-50 text-red-600 p-2 rounded-lg border border-red-200 text-xs font-bold flex items-center gap-1">
                                <span>⚠️</span>
                                <span>{accountError}</span>
                              </div>
                            )}

                            <div className="flex items-center justify-between pt-1">
                              <div className="text-xs text-gray-600">
                                {accountOtpTimer > 0 ? (
                                  <span>
                                    زمان باقیمانده: <strong className="font-mono text-emerald-700 font-bold">{Math.floor(accountOtpTimer / 60)}:{String(accountOtpTimer % 60).padStart(2, '0')}</strong>
                                  </span>
                                ) : (
                                  <button
                                    type="button"
                                    onClick={handleResendAccountOTP}
                                    className="text-xs text-emerald-700 font-bold hover:underline"
                                  >
                                    🔄 ارسال مجدد کد
                                  </button>
                                )}
                              </div>

                              <button
                                type="button"
                                onClick={handleConfirmAccountOTP}
                                disabled={accountVerifyLoading || !accountOtpCode.trim()}
                                className="px-5 py-2.5 bg-emerald-700 bg-gradient-to-r from-emerald-600 to-teal-700 hover:from-emerald-700 hover:to-teal-800 text-white rounded-xl text-xs sm:text-sm font-black shadow-md transition-all disabled:opacity-50"
                              >
                                {accountVerifyLoading ? 'در حال تایید...' : '✓ تایید و ورود'}
                              </button>
                            </div>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                )}
              </div>
            )}
          </>
        )}

        {/* Footer */}
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
