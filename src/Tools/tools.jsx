import {
  getUserTimeZone,
  parseUtcDate,
  formatPersianDate,
  getMeetingDateTime,
  formatPersianDateTime,
  isFutureDate,
  findNextPlan,
  timeAgo,
  jalaliToUtcIso,
  utcToJalali,
  convertDateTimeUTCToTehranTIMEONLY,
  persianAndArabicToEnglishNumbers
} from './dateTimeHelper';

export {
  getUserTimeZone,
  parseUtcDate,
  formatPersianDate,
  getMeetingDateTime,
  formatPersianDateTime,
  isFutureDate,
  findNextPlan,
  timeAgo,
  jalaliToUtcIso,
  utcToJalali,
  convertDateTimeUTCToTehranTIMEONLY,
  persianAndArabicToEnglishNumbers
};

export const SVG_ROOMCARD_WAVE = "M0,192L8.6,186.7C17.1,181,34,171,51,192C68.6,213,86,267,103,261.3C120,256,137,192,154,149.3C171.4,107,189,85,206,80C222.9,75,240,85,257,96C274.3,107,291,117,309,149.3C325.7,181,343,235,360,261.3C377.1,288,394,288,411,240C428.6,192,446,96,463,101.3C480,107,497,213,514,218.7C531.4,224,549,128,566,122.7C582.9,117,600,203,617,202.7C634.3,203,651,117,669,80C685.7,43,703,53,720,74.7C737.1,96,754,128,771,133.3C788.6,139,806,117,823,112C840,107,857,117,874,106.7C891.4,96,909,64,926,69.3C942.9,75,960,117,977,165.3C994.3,213,1011,267,1029,282.7C1045.7,299,1063,277,1080,224C1097.1,171,1114,85,1131,80C1148.6,75,1166,149,1183,170.7C1200,192,1217,160,1234,138.7C1251.4,117,1269,107,1286,90.7C1302.9,75,1320,53,1337,53.3C1354.3,53,1371,75,1389,106.7C1405.7,139,1423,181,1431,202.7L1440,224L1440,0L1431.4,0C1422.9,0,1406,0,1389,0C1371.4,0,1354,0,1337,0C1320,0,1303,0,1286,0C1268.6,0,1251,0,1234,0C1217.1,0,1200,0,1183,0C1165.7,0,1149,0,1131,0C1114.3,0,1097,0,1080,0C1062.9,0,1046,0,1029,0C1011.4,0,994,0,977,0C960,0,943,0,926,0C908.6,0,891,0,874,0C857.1,0,840,0,823,0C805.7,0,789,0,771,0C754.3,0,737,0,720,0C702.9,0,686,0,669,0C651.4,0,634,0,617,0C600,0,583,0,566,0C548.6,0,531,0,514,0C497.1,0,480,0,463,0C445.7,0,429,0,411,0C394.3,0,377,0,360,0C342.9,0,326,0,309,0C291.4,0,274,0,257,0C240,0,223,0,206,0C188.6,0,171,0,154,0C137.1,0,120,0,103,0C85.7,0,69,0,51,0C34.3,0,17,0,9,0L0,0Z";

export function combinePath(base, relative) {
  const cleanBase = base.replace(/\/+$/, '');
  const cleanRelative = relative.replace(/^\/+/, '');
  return `${cleanBase}/${cleanRelative}`;
}

export function toPersianDigits(number) {
  const persianDigits = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
  return String(number).replace(/\d/g, d => persianDigits[parseInt(d)]);
}

/**
 * تبدیل تاریخ UTC به شمسی با منطقه زمانی سیستم کاربر (یا تهران)
 */
export function convertUTCToPersian(utcDateStr) {
  return formatPersianDateTime(utcDateStr);
}

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
