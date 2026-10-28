// src/mock/roomDetailMock.js
// داده‌های نمونه برای صفحه اختصاصی اتاق (Room Detail Page)
// پس از آماده شدن API می‌توانید این فایل را با فراخوانی‌های واقعی جایگزین یا با کامپوننت متصل کنید.

export const mockRoomDetail = {
  roomID: "rm-hekmateislami-101",
  friendlyID: "falsafe-islami",
  title: "دوره جامع فلسفه اسلامی و حکمت متعالیه",
  description: "در این اتاق نشست‌های تخصصی، دروس گفت‌وگو محور و همایش‌های علمی پیرامون حکمت اسلامی با حضور استادان برجسته برگزار می‌گردد.",
  avatar: "book",
  coverImage: "https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?auto=format&fit=crop&w=1600&q=80",
  isLive: true,
  liveInfo: {
    meetingID: "live-meet-99",
    title: "بررسی مکتب شیراز در حکمت متعالیه (جلسه آنلاین زنده)",
    countPartis: 142,
    startedAt: "1403/05/10 - 18:00",
  },
  owner: {
    id: "usr-hekmateislami",
    fullname: "مؤسسه حکمت اسلامی",
    username: "hekmateislami",
    avatar: "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?auto=format&fit=crop&w=200&q=80",
    bio: "مرکز پژوهشی و آموزشی علوم و حکمت اسلامی"
  },
  // آمارهای کلیدی اتاق (Stats Cards)
  stats: {
    totalMeetings: 64,
    totalParticipants: 2840,
    totalHours: 128,
    satisfactionRate: "98.5%",
    activePollsCount: 2
  },
  // آخرین جلسه برگزار شده یا در حال برگزاری
  latestMeeting: {
    id: "meet-64",
    title: "جلسه ۶۴: تحلیل اصول فلسفی ملاصدرا در رساله الشواهد الربوبية",
    date: "۱۰ مرداد ۱۴۰۳ - ساعت ۱۸:۳۰",
    relativeTime: "۲ ساعت پیش",
    duration: "۹۰ دقیقه",
    status: "live", // "live" | "completed" | "scheduled"
    teacherName: "دکتر محمدرضا حسینی",
    participantsCount: 142,
    recordingUrl: "https://hekmateislami.com/recordings/meet-64.mp4",
    summary: "در این جلسه به تبین نظریه اصالت وجود و اعتباری بودن ماهیت پرداخته شد و پرسش‌های پژوهشگران مورد پاسخگویی قرار گرفت."
  },
  // لیست استادان / برگزارکنندگان
  teachers: [
    {
      id: "tch-1",
      name: "دکتر محمدرضا حسینی",
      title: "استاد تمام دانشگاه و پژوهشگر حکمت اسلامی",
      avatar: "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80",
      specialization: "فلسفه اسلامی & عرفان نظری",
      bio: "عضو هیئت علمی با بیش از ۱۵ سال سابقه تدریس متون فلسفی ملاصدرا و ابن‌سینا.",
      coursesCount: 18,
      social: { eitaa: "https://eitaa.com", telegram: "https://t.me" }
    },
    {
      id: "tch-2",
      name: "استاد علی‌اصغر مهدوی",
      title: "مدرس حوزه علمیه قم و محقق علوم کلامی",
      avatar: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80",
      specialization: "کلام اسلامی & منطق جدید",
      bio: "مدرس دوره‌های تخصصی منطق شفا و برهان در حوزه‌های علمیه و مراکز دانشگاهی.",
      coursesCount: 12,
      social: { eitaa: "https://eitaa.com" }
    },
    {
      id: "tch-3",
      name: "دکتر سمیه ابراهیمی",
      title: "پژوهشگر تاریخ فلسفه و مطالعات تطبیقی",
      avatar: "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=300&q=80",
      specialization: "مطالعات تطبیقی شرق و غرب",
      bio: "مؤلف کتاب‌های متعدد در زمینه فلسفه اخلاق و معرفت‌شناسی دینی.",
      coursesCount: 9,
      social: { eitaa: "https://eitaa.com" }
    }
  ],
  // لیست نظرسنجی‌های اتاق (Polls Data)
  polls: [
    {
      id: "poll-101",
      question: "ارزیابی کیفیت ارائه جلسه امروز و پاسخگویی به پرسش‌ها چگونه بود؟",
      totalVotes: 118,
      isClosed: false,
      userVoted: false,
      userSelectedOption: null,
      options: [
        { id: 1, text: "عالی و بسیار کاربردی", count: 76, percentage: 64 },
        { id: 2, text: "خوب و مفید", count: 32, percentage: 27 },
        { id: 3, text: "متوسط", count: 8, percentage: 7 },
        { id: 4, text: "نیازمند بهبود", count: 2, percentage: 2 }
      ]
    },
    {
      id: "poll-102",
      question: "بهترین زمان پیشنهادی برای برگزاری جلسه بعدی دوره چه ساعتی است؟",
      totalVotes: 95,
      isClosed: true,
      userVoted: true,
      userSelectedOption: 2,
      options: [
        { id: 1, text: "روزهای زوج ساعت ۱۶:۰۰", count: 20, percentage: 21 },
        { id: 2, text: "روزهای زوج ساعت ۱۸:۳۰", count: 58, percentage: 61 },
        { id: 3, text: "پنج‌شنبه‌ها ساعت ۱۰:۰۰ صبح", count: 17, percentage: 18 }
      ]
    }
  ],
  // تاریخچه نشست‌های برگزار شده
  pastMeetings: [
    {
      id: "meet-63",
      title: "جلسه ۶۳: بررسی براهین اثبات وجود باری‌تعالی",
      date: "۰۸ مرداد ۱۴۰۳",
      duration: "۱۰۰ دقیقه",
      participantsCount: 135,
      teacherName: "دکتر محمدرضا حسینی",
      hasRecording: true,
      recordingUrl: "#"
    },
    {
      id: "meet-62",
      title: "جلسه ۶۲: نظریه علم و معلوم در حکمت متعالیه",
      date: "۰۵ مرداد ۱۴۰۳",
      duration: "۸۵ دقیقه",
      participantsCount: 128,
      teacherName: "استاد علی‌اصغر مهدوی",
      hasRecording: true,
      recordingUrl: "#"
    },
    {
      id: "meet-61",
      title: "جلسه ۶۱: مقدمه‌ای بر حرکت جوهری ملاصدرا",
      date: "۰۱ مرداد ۱۴۰۳",
      duration: "۹۰ دقیقه",
      participantsCount: 152,
      teacherName: "دکتر محمدرضا حسینی",
      hasRecording: true,
      recordingUrl: "#"
    }
  ]
};
