import moment from 'moment-jalaali';



export const SVG_ROOMCARD_WAVE = "M0,192L8.6,186.7C17.1,181,34,171,51,192C68.6,213,86,267,103,261.3C120,256,137,192,154,149.3C171.4,107,189,85,206,80C222.9,75,240,85,257,96C274.3,107,291,117,309,149.3C325.7,181,343,235,360,261.3C377.1,288,394,288,411,240C428.6,192,446,96,463,101.3C480,107,497,213,514,218.7C531.4,224,549,128,566,122.7C582.9,117,600,203,617,202.7C634.3,203,651,117,669,80C685.7,43,703,53,720,74.7C737.1,96,754,128,771,133.3C788.6,139,806,117,823,112C840,107,857,117,874,106.7C891.4,96,909,64,926,69.3C942.9,75,960,117,977,165.3C994.3,213,1011,267,1029,282.7C1045.7,299,1063,277,1080,224C1097.1,171,1114,85,1131,80C1148.6,75,1166,149,1183,170.7C1200,192,1217,160,1234,138.7C1251.4,117,1269,107,1286,90.7C1302.9,75,1320,53,1337,53.3C1354.3,53,1371,75,1389,106.7C1405.7,139,1423,181,1431,202.7L1440,224L1440,0L1431.4,0C1422.9,0,1406,0,1389,0C1371.4,0,1354,0,1337,0C1320,0,1303,0,1286,0C1268.6,0,1251,0,1234,0C1217.1,0,1200,0,1183,0C1165.7,0,1149,0,1131,0C1114.3,0,1097,0,1080,0C1062.9,0,1046,0,1029,0C1011.4,0,994,0,977,0C960,0,943,0,926,0C908.6,0,891,0,874,0C857.1,0,840,0,823,0C805.7,0,789,0,771,0C754.3,0,737,0,720,0C702.9,0,686,0,669,0C651.4,0,634,0,617,0C600,0,583,0,566,0C548.6,0,531,0,514,0C497.1,0,480,0,463,0C445.7,0,429,0,411,0C394.3,0,377,0,360,0C342.9,0,326,0,309,0C291.4,0,274,0,257,0C240,0,223,0,206,0C188.6,0,171,0,154,0C137.1,0,120,0,103,0C85.7,0,69,0,51,0C34.3,0,17,0,9,0L0,0Z";
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

  try {
    const date = new Date(dateString);
    const now = new Date();

    const formatter = new Intl.DateTimeFormat('en-US', {
      year: 'numeric', month: 'numeric', day: 'numeric', timeZone: 'Asia/Tehran'
    });
    
    const targetMidnight = new Date(formatter.format(date));
    const nowMidnight = new Date(formatter.format(now));
    const diffDays = Math.round((targetMidnight - nowMidnight) / (1000 * 60 * 60 * 24));

    let prettDate = '';

    if (diffDays === 0) {
      prettDate = 'امروز';
    } else if (diffDays === 1) {
      prettDate = 'فردا';
    } else if (diffDays === 2) {
      prettDate = 'پس فردا';
    } else if (diffDays > 2 && diffDays <= 7) {
      prettDate = new Intl.DateTimeFormat('fa-IR', { weekday: 'long', timeZone: 'Asia/Tehran' }).format(date);
    } else {
      prettDate = new Intl.DateTimeFormat('fa-IR', {
        year: 'numeric', month: 'long', day: 'numeric', timeZone: 'Asia/Tehran'
      }).format(date);
    }

    const timeString = new Intl.DateTimeFormat('fa-IR', {
      hour: '2-digit', minute: '2-digit', hour12: false, timeZone: 'Asia/Tehran'
    }).format(date);

    const fullDate = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric', month: 'long', day: 'numeric', timeZone: 'Asia/Tehran'
    }).format(date);

    return {
      time: timeString,
      date: fullDate,
      prettDate: prettDate
    };
  } catch (e) {
    return { date: '-', time: '-', prettDate: '-' };
  }
};

// تابع ساده تر برای استفاده در جاهای مختلف
export const getMeetingDateTime = (dateString) => {
  const result = formatPersianDate(dateString);
  return `${result.prettDate} ساعت ${result.time}`;
};

export const convertDateTimeUTCToTehranTIMEONLY = (dateString) => {
  if (!dateString) return '-';
  try {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-GB', {
      hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false, timeZone: 'Asia/Tehran'
    }).format(date);
  } catch {
    return dateString;
  }
};


export const convertNumberToWords = (num) => {
  // واحدها
  const ones = [
    '', 'یک', 'دو', 'سه', 'چهار', 'پنج', 'شش', 'هفت', 'هشت', 'نه',
    'ده', 'یازده', 'دوازده', 'سیزده', 'چهارده', 'پانزده', 'شانزده',
    'هفده', 'هجده', 'نوزده'
  ];

  const tens = [
    '', '', 'بیست', 'سی', 'چهل', 'پنجاه', 'شصت', 'هفتاد', 'هشتاد', 'نود'
  ];

  const hundreds = [
    '', 'صد', 'دویست', 'سیصد', 'چهارصد', 'پانصد', 'ششصد', 'هفتصد', 'هشتصد', 'نهصد'
  ];

  const scales = ['', 'هزار', 'میلیون', 'میلیارد'];

  // تبدیل بخش ۳ رقمی به کلمات
  const convertThreeDigits = (n) => {
    if (n === 0) return '';

    let result = '';
    const hundred = Math.floor(n / 100);
    const remainder = n % 100;

    if (hundred > 0) {
      result += hundreds[hundred];
      if (remainder > 0) result += ' و ';
    }

    if (remainder > 0) {
      if (remainder < 20) {
        result += ones[remainder];
      } else {
        const ten = Math.floor(remainder / 10);
        const one = remainder % 10;
        result += tens[ten];
        if (one > 0) {
          result += ' و ' + ones[one];
        }
      }
    }

    return result;
  };

  // مدیریت ورودی‌های نامعتبر
  if (typeof num !== 'number' || isNaN(num) || num < 0) {
    return 'عدد معتبر وارد کنید';
  }

  // محدودیت عدد
  if (num > 100000000000) {
    return 'عدد بیش از ۱۰۰ میلیارد است';
  }

  if (num === 0) return 'صفر';

  let result = '';
  let scaleIndex = 0;
  let tempNum = num;

  while (tempNum > 0) {
    const threeDigits = tempNum % 1000;
    if (threeDigits > 0) {
      const threeDigitsStr = convertThreeDigits(threeDigits);
      let scaleStr = scales[scaleIndex];

      // اصلاح پسوند برای اعداد بزرگتر
      if (scaleIndex > 0) {
        if (threeDigits === 1) {
          // برای عدد ۱ هزار، میلیون، میلیارد
          scaleStr = scales[scaleIndex];
        }
      }

      // اضافه کردن بخش به نتیجه
      if (result) {
        result = threeDigitsStr + ' ' + scaleStr + ' و ' + result;
      } else {
        result = threeDigitsStr + ' ' + scaleStr;
      }
    }
    tempNum = Math.floor(tempNum / 1000);
    scaleIndex++;
  }

  return result.trim();
};


/**
 * تبدیل تاریخ UTC به شمسی با منطقه زمانی تهران
 */
export function convertUTCToPersian(utcDateStr) {
  if (!utcDateStr) return null;

  try {
    const date = new Date(utcDateStr);
    // تنظیم منطقه زمانی تهران (UTC+3:30)
    const tehranOffset = 3.5 * 60 * 60 * 1000;
    const tehranTime = new Date(date.getTime() + tehranOffset);

    // تبدیل به تاریخ شمسی
    const persianDate = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone: 'Asia/Tehran'
    }).format(date);

    return persianDate;
  } catch (error) {
    console.error('Error converting date:', error);
    return null;
  }
}
export function toPersianDigits(number) {
  const persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
  return String(number).replace(/\d/g, d => persianDigits[parseInt(d)]);
}
/**
 * بررسی اینکه تاریخ از الان گذشته باشد یا نه
 */
export function isFutureDate(utcDateStr) {
  if (!utcDateStr) return false;
  try {
    const date = new Date(utcDateStr);
    const now = new Date();
    return date.getTime() > now.getTime();
  } catch (error) {
    return false;
  }
}

/**
 * پیدا کردن نزدیک‌ترین plan آینده
 */
export function findNextPlan(plans) {
  if (!plans || plans.length === 0) return null;

  // فیلتر plans آینده
  const futurePlans = plans
    .filter(plan => plan.startAt && isFutureDate(plan.startAt))
    .sort((a, b) => new Date(a.startAt) - new Date(b.startAt));

  return futurePlans.length > 0 ? futurePlans[0] : null;
}

/**
 * فرمت کردن تاریخ به صورت شمسی با ساعت
 */
export function formatPersianDateTime(utcDateStr) {
  if (!utcDateStr) return 'تاریخ نامشخص';

  try {
    const date = new Date(utcDateStr);
    const formatter = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone: 'Asia/Tehran'
    });

    const parts = formatter.formatToParts(date);
    const year = parts.find(p => p.type === 'year')?.value || '';
    const month = parts.find(p => p.type === 'month')?.value || '';
    const day = parts.find(p => p.type === 'day')?.value || '';
    const hour = parts.find(p => p.type === 'hour')?.value || '';
    const minute = parts.find(p => p.type === 'minute')?.value || '';

    return `${year}/${month}/${day} ${hour}:${minute}`;
  } catch (error) {
    console.error('Error formatting date:', error);
    return 'تاریخ نامشخص';
  }
}