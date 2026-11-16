import moment from 'moment-jalaali';

/**
 * Converts Persian and Arabic numbers to English ASCII numbers.
 */
export const persianAndArabicToEnglishNumbers = (str) => {
  if (!str) return '';
  const numberMap = {
    '۰': '0', '۱': '1', '۲': '2', '۳': '3', '۴': '4',
    '۵': '5', '۶': '6', '۷': '7', '۸': '8', '۹': '9',
    '٠': '0', '١': '1', '٢': '2', '٣': '3', '٤': '4',
    '٥': '5', '٦': '6', '٧': '7', '٨': '8', '٩': '9'
  };
  return str.toString().replace(/[۰-۹٠-٩]/g, (match) => numberMap[match]);
};

/**
 * Returns the user system's detected timezone, with 'Asia/Tehran' as fallback.
 */
export function getUserTimeZone() {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || 'Asia/Tehran';
  } catch (e) {
    return 'Asia/Tehran';
  }
}

/**
 * Safely parses any date representation from the server into a standard JavaScript UTC Date.
 * Handles ISO strings with/without 'Z', timestamp numbers, and Date objects.
 */
export function parseUtcDate(input) {
  if (!input) return null;

  const validate = (d) => {
    if (!d || isNaN(d.getTime())) return null;
    const y = d.getUTCFullYear();
    if (y < 1970 || y >= 3000) return null;
    return d;
  };

  if (input instanceof Date) return validate(input);

  if (typeof input === 'number') {
    return validate(new Date(input));
  }

  if (typeof input === 'string') {
    let s = input.trim();
    if (!s) return null;

    // If it already has UTC Z or an explicit timezone offset (+03:30, -05:00, etc.)
    if (/[zZ]|[+-]\d{2}:?\d{2}$/.test(s)) {
      return validate(new Date(s));
    }

    // Server sends UTC string without Z (e.g. "2026-11-08T15:00:00" or "2026-11-08 15:00:00")
    s = s.replace(' ', 'T');
    const d = new Date(`${s}Z`);
    if (!isNaN(d.getTime())) return validate(d);

    // Fallback standard parse
    return validate(new Date(input));
  }

  return null;
}

/**
 * Formats a UTC date into Persian components (date, time, prettDate, full, diffDays)
 * in the user's detected system timezone (or provided timezone).
 */
export function formatPersianDate(utcInput, options = {}) {
  const date = parseUtcDate(utcInput);
  if (!date) return { date: '-', time: '-', prettDate: '-', full: '-', diffDays: null };

  const timeZone = options.timeZone || getUserTimeZone();

  try {
    const now = new Date();

    // Midnight comparison in target timezone to accurately determine today / tomorrow / etc.
    const dayFormatter = new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
      timeZone,
    });

    const targetMidnight = new Date(dayFormatter.format(date));
    const nowMidnight = new Date(dayFormatter.format(now));
    const diffDays = Math.round((targetMidnight.getTime() - nowMidnight.getTime()) / (1000 * 60 * 60 * 24));

    let prettDate = '';
    if (diffDays === 0) {
      prettDate = 'امروز';
    } else if (diffDays === 1) {
      prettDate = 'فردا';
    } else if (diffDays === 2) {
      prettDate = 'پس فردا';
    } else if (diffDays === -1) {
      prettDate = 'دیروز';
    } else if (diffDays > 2 && diffDays <= 7) {
      prettDate = new Intl.DateTimeFormat('fa-IR', { weekday: 'long', timeZone }).format(date);
    } else {
      prettDate = new Intl.DateTimeFormat('fa-IR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        timeZone,
      }).format(date);
    }

    const timeString = new Intl.DateTimeFormat('fa-IR', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone,
    }).format(date);

    const fullDate = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      timeZone,
    }).format(date);

    return {
      time: timeString,
      date: fullDate,
      prettDate,
      full: `${fullDate} ساعت ${timeString}`,
      diffDays,
    };
  } catch (e) {
    console.error('formatPersianDate error:', e);
    return { date: '-', time: '-', prettDate: '-', full: '-', diffDays: null };
  }
}

/**
 * Returns a human-friendly string for upcoming meeting time (e.g. "امروز ساعت ۱۸:۳۰", "فردا ساعت ۱۰:۰۰").
 */
export function getMeetingDateTime(utcInput, options = {}) {
  const result = formatPersianDate(utcInput, options);
  if (!result || result.prettDate === '-') return '';
  return `${result.prettDate} ساعت ${result.time}`;
}

/**
 * Formats a UTC date string into Jalali numeric date with time (e.g. "1405/08/17 18:30").
 */
export function formatPersianDateTime(utcInput, options = {}) {
  const date = parseUtcDate(utcInput);
  if (!date) return 'تاریخ نامشخص';
  const timeZone = options.timeZone || getUserTimeZone();

  try {
    const formatter = new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone,
    });

    const parts = formatter.formatToParts(date);
    const year = parts.find(p => p.type === 'year')?.value || '';
    const month = parts.find(p => p.type === 'month')?.value || '';
    const day = parts.find(p => p.type === 'day')?.value || '';
    const hour = parts.find(p => p.type === 'hour')?.value || '';
    const minute = parts.find(p => p.type === 'minute')?.value || '';

    return `${year}/${month}/${day} ${hour}:${minute}`;
  } catch (e) {
    console.error('formatPersianDateTime error:', e);
    return 'تاریخ نامشخص';
  }
}

/**
 * Checks if a given UTC date is in the future.
 */
export function isFutureDate(utcInput) {
  const date = parseUtcDate(utcInput);
  if (!date) return false;
  return date.getTime() > Date.now();
}

/**
 * Finds the nearest future plan from an array of plans.
 */
export function findNextPlan(plans) {
  if (!plans || !Array.isArray(plans) || plans.length === 0) return null;

  const futurePlans = plans
    .map(p => ({ plan: p, date: parseUtcDate(p.startAt || p.startDate || p.StartAt || p.StartDate) }))
    .filter(item => item.date && item.date.getTime() > Date.now())
    .sort((a, b) => a.date.getTime() - b.date.getTime());

  return futurePlans.length > 0 ? futurePlans[0].plan : null;
}

/**
 * Returns a relative "time ago" string from a UTC date.
 */
export function timeAgo(inputDate) {
  const date = parseUtcDate(inputDate);
  if (!date || date.getFullYear() < 1970) return "بدون جلسه";

  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  if (diffMs < 0) return "به تازگی";

  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHour = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHour / 24);
  const diffMonth = Math.floor(diffDay / 30);
  const diffYear = Math.floor(diffDay / 365);

  if (diffSec < 60) return "چند ثانیه قبل";
  if (diffMin < 60) return `${diffMin} دقیقه قبل`;
  if (diffHour < 24) return `${diffHour} ساعت قبل`;
  if (diffDay < 30) return `${diffDay} روز پیش`;
  if (diffMonth < 12) return `${diffMonth} ماه قبل`;
  return `${diffYear} سال قبل`;
}

/**
 * Converts a Jalali date string and optional time string to an ISO 8601 UTC string.
 * Example: jalaliToUtcIso("1405/08/17", "18:30") => "2026-11-08T15:00:00.000Z" (Tehran UTC+03:30)
 */
export function jalaliToUtcIso(jDateStr, timeStr = '00:00') {
  if (!jDateStr) return null;
  try {
    const cleanJDate = persianAndArabicToEnglishNumbers(jDateStr).replace(/[^\d/]/g, '');
    const cleanTime = persianAndArabicToEnglishNumbers(timeStr).replace(/[^\d:]/g, '') || '00:00';
    
    // moment-jalaali parse
    const m = moment(`${cleanJDate} ${cleanTime}`, 'jYYYY/jMM/jDD HH:mm');
    if (!m.isValid()) return null;
    return m.utc().toISOString();
  } catch (e) {
    console.error('jalaliToUtcIso error:', e);
    return null;
  }
}

/**
 * Converts a UTC date to Jalali formatted string.
 */
export function utcToJalali(utcInput, format = 'jYYYY/jMM/jDD HH:mm') {
  const date = parseUtcDate(utcInput);
  if (!date) return '';
  return moment(date).format(format);
}

/**
 * Formats time only in Tehran timezone (or user system timezone).
 */
export function convertDateTimeUTCToTehranTIMEONLY(utcInput) {
  const date = parseUtcDate(utcInput);
  if (!date) return '-';
  try {
    return new Intl.DateTimeFormat('en-GB', {
      hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false, timeZone: 'Asia/Tehran'
    }).format(date);
  } catch {
    return '-';
  }
}
