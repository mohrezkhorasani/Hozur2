import moment from 'moment-jalaali';




export function combinePath(base, relative) {
  // Remove trailing slash from base, then add one slash + relative without leading slash
  const cleanBase = base.replace(/\/+$/, '');
  const cleanRelative = relative.replace(/^\/+/, '');
  return `${cleanBase}/${cleanRelative}`;
}
export const persianAndArabicToEnglishNumbers = (str) => {
  if (!str) return '';
  
  const numberMap = {
    // Persian numbers
    '۰': '0', '۱': '1', '۲': '2', '۳': '3', '۴': '4',
    '۵': '5', '۶': '6', '۷': '7', '۸': '8', '۹': '9',
    // Arabic numbers
    '٠': '0', '١': '1', '٢': '2', '٣': '3', '٤': '4',
    '٥': '5', '٦': '6', '٧': '7', '٨': '8', '٩': '9'
  };
  
  return str.toString().replace(/[۰-۹٠-٩]/g, (match) => numberMap[match]);
};

export function timeAgo(inputDate) {
    const now = new Date();
    const date = new Date(inputDate);

    const diffMs = now - date;
    const diffSec = Math.floor(diffMs / 1000);
    const diffMin = Math.floor(diffSec / 60);
    const diffHour = Math.floor(diffMin / 60);
    const diffDay = Math.floor(diffHour / 24);
    const diffMonth = Math.floor(diffDay / 30);
    const diffYear = Math.floor(diffDay / 365);

    if (diffSec < 60) {
        return "چند ثانیه قبل";
    }

    if (diffMin < 60) {
        return `${diffMin} دقیقه قبل`;
    }

    if (diffHour < 24) {
        return `${diffHour} ساعت قبل`;
    }

    if (diffDay < 30) {
        return `${diffDay} روز پیش`;
    }

    if (diffMonth < 12) {
        return `${diffMonth} ماه قبل`;
    }

    return `${diffYear} سال قبل`;
}


// tools.js یا همون فایل tools.js که timeAgo توش هست

// تابع فرمت تاریخ و ساعت به صورت حرفه‌ای
export const formatPersianDate = (dateString) => {
  if (!dateString) return { date: '-', time: '-', prettDate: '-' };
  
  const date = new Date(dateString);
  const now = new Date();
  
  // تنظیم برای مقایسه فقط تاریخ (بدون ساعت)
  const targetDate = new Date(Date.UTC(
    date.getUTCFullYear(),
    date.getUTCMonth(),
    date.getUTCDate()
  ));
  
  const today = new Date(Date.UTC(
    now.getUTCFullYear(),
    now.getUTCMonth(),
    now.getUTCDate()
  ));
  
  // محاسبه اختلاف روزها
  const diffDays = Math.ceil((targetDate - today) / (1000 * 60 * 60 * 24));
  
  let prettDate = '';
  
  if (diffDays === 0) {
    prettDate = 'امروز';
  } else if (diffDays === 1) {
    prettDate = 'فردا';
  } else if (diffDays === 2) {
    prettDate = 'پس فردا';
  } else if (diffDays > 2 && diffDays <= 7) {
    // نمایش نام روز هفته
    const weekDays = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
    const dayOfWeek = date.getUTCDay();
    prettDate = weekDays[dayOfWeek];
  } else {
    // نمایش تاریخ شمسی
    const persianDate = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    }).format(date);
    prettDate = persianDate;
  }
  
  // فرمت ساعت (UTC)
  const timeString = date.toLocaleTimeString('fa-IR', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
    timeZone: 'UTC'
  });
  
  // تاریخ کامل برای نمایش در جاهای دیگر
  const fullDate = new Intl.DateTimeFormat('fa-IR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date);
  
  return {
    time: timeString,
    date: fullDate,
    prettDate: prettDate
  };
};

// تابع ساده تر برای استفاده در جاهای مختلف
export const getMeetingDateTime = (dateString) => {
  const result = formatPersianDate(dateString);
  return `${result.prettDate} ساعت ${result.time}`;
};