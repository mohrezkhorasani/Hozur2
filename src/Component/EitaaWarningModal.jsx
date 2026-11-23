import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import {
  FaExclamationTriangle,
  FaChrome,
  FaCopy,
  FaCheck,
  FaTimes,
  FaExternalLinkAlt,
  FaEllipsisV,
  FaCompass
} from 'react-icons/fa';
import { BaseURL } from '../api/apiNeed';

export default function EitaaWarningModal() {
  const [isOpen, setIsOpen] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let isMounted = true;

    const detectEnvironment = async () => {
      try {
        const ua = (navigator.userAgent || '').toLowerCase();
        const ref = (document.referrer || '').toLowerCase();
        const search = window.location.search || '';
        const urlParams = new URLSearchParams(search);

        // 1. تست دستی جهت بررسی سریع توسعه‌دهنده با ?testEitaa=1
        if (urlParams.get('testEitaa') === '1') {
          if (isMounted) setIsOpen(true);
          return;
        }

        // 2. بررسی مستقیم در کلاینت (User-Agent و Referrer و پارامترها)
        const isClientEitaa =
          ua.includes('ir.eitaa.messenger') ||
          ua.includes('eitaa') ||
          ref.includes('ir.eitaa.messenger') ||
          ref.includes('eitaa.com') ||
          urlParams.get('from') === 'eitaa' ||
          urlParams.get('utm_source') === 'eitaa';

        // 3. بررسی برندهای Client Hints مرورگر برای Android WebView
        const brands = navigator.userAgentData?.brands || [];
        const isClientWebView = brands.some(
          (b) =>
            b.brand === 'Android WebView' ||
            (b.brand && b.brand.toLowerCase().includes('webview'))
        );

        // 4. الگوهای کلاسیک Android WebView
        const isClassicWebView =
          ua.includes('; wv') ||
          (ua.includes('android') && ua.includes('version/') && ua.includes('chrome/'));

        if (isClientEitaa || isClientWebView || isClassicWebView) {
          if (isMounted) setIsOpen(true);
          return;
        }

        // 5. بررسی سروری با هدر X-Requested-With (در صورتی که ایتا هدر کلاینت را پنهان کرده باشد)
        try {
          const res = await fetch(`${BaseURL}user/check-client`, {
            method: 'GET',
            headers: { Accept: 'application/json' }
          });
          if (res.ok) {
            const data = await res.json();
            if (data?.isEitaa || data?.isWebView) {
              if (isMounted) setIsOpen(true);
            }
          }
        } catch {
          // در صورت بروز خطای شبکه سرور، ارزیابی کلاینت کافیست
        }
      } catch (err) {
        console.warn('Eitaa detection error:', err);
      }
    };

    detectEnvironment();

    return () => {
      isMounted = false;
    };
  }, []);

  const handleCopyLink = () => {
    try {
      const currentUrl = window.location.href;
      navigator.clipboard.writeText(currentUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 3000);
    } catch {
      // Fallback برای محیط‌های بدون مجوز کلیپ‌بورد
      const textarea = document.createElement('textarea');
      textarea.value = window.location.href;
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand('copy');
      document.body.removeChild(textarea);
      setCopied(true);
      setTimeout(() => setCopied(false), 3000);
    }
  };

  const handleOpenChrome = () => {
    try {
      const host = window.location.host;
      const pathAndSearch = window.location.pathname + window.location.search + window.location.hash;
      // پروتکل اختصاصی Intent اندروید برای هدایت مستقیم به مرورگر Chrome
      const chromeIntent = `intent://${host}${pathAndSearch}#Intent;scheme=https;package=com.android.chrome;end`;
      window.location.href = chromeIntent;
    } catch (e) {
      handleCopyLink();
    }
  };

  if (!isOpen) return null;

  const modalContent = (
    <div
      className="fixed inset-0 z-[999999] bg-slate-950/85 backdrop-blur-md flex items-center justify-center p-3 sm:p-5 overflow-y-auto"
      dir="rtl"
    >
      <div
        className="bg-white rounded-3xl max-w-xl w-full border-4 border-amber-500 shadow-[0_0_60px_rgba(245,158,11,0.45)] overflow-hidden animate-in fade-in zoom-in-95 duration-200 text-right my-auto"
        onClick={(e) => e.stopPropagation()}
      >
        {/* نوار بالایی فوق‌العاده بولد و پررنگ */}
        <div className="bg-gradient-to-r from-amber-600 via-orange-600 to-red-600 p-5 text-white flex items-center justify-between shadow-md">
          <div className="flex items-center gap-3">
            <span className="p-3 bg-white/20 backdrop-blur-md rounded-2xl border border-white/30 text-white animate-bounce">
              <FaExclamationTriangle size={26} />
            </span>
            <div>
              <h3 className="text-lg sm:text-xl font-black tracking-tight drop-shadow-sm">
                هشدار مهم: ورود از پیام‌رسان ایتا
              </h3>
              <p className="text-xs text-amber-100 font-bold mt-0.5">
                پخش زنده و کلاس مجازی در این مرورگر مسدود است
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={() => setIsOpen(false)}
            className="p-2 text-white/80 hover:text-white hover:bg-white/10 rounded-xl transition cursor-pointer"
            title="بستن اخطار"
          >
            <FaTimes size={18} />
          </button>
        </div>

        {/* محتوای بدنه اخطار */}
        <div className="p-5 sm:p-6 space-y-4">
          {/* باکس متن بولد اصلی */}
          <div className="bg-gradient-to-b from-amber-50 to-orange-50 border-2 border-amber-300 rounded-2xl p-4 sm:p-5 text-amber-950 shadow-inner space-y-2.5">
            <div className="flex items-start gap-2.5">
              <span className="w-3 h-3 rounded-full bg-red-600 animate-ping mt-1.5 shrink-0" />
              <p className="text-sm sm:text-base font-extrabold leading-relaxed text-red-900">
                مرورگر داخلی ایتا امکانات صدا و تصویر بیگ‌بلو‌باتن (WebRTC) را پشتیبانی نمی‌کند و صفحه برای شما <span className="underline decoration-red-600 decoration-2 font-black text-red-700">روی صفحه آبی قفل خواهد شد</span>!
              </p>
            </div>
            <p className="text-xs sm:text-sm font-bold text-gray-800 leading-relaxed pr-5">
              برای ورود سریع و بدون مشکل، حتماً باید این صفحه را با <span className="font-black text-blue-700 underline">مرورگر اصلی گوشی (مثل Chrome یا سامسونگ)</span> باز کنید.
            </p>
          </div>

          {/* راهنمای تصویری ۳ مرحله‌ای */}
          <div className="bg-slate-50 border border-slate-200 rounded-2xl p-4 space-y-3">
            <div className="text-xs font-black text-slate-700 flex items-center gap-2">
              <FaCompass className="text-indigo-600" size={14} />
              <span>چگونه از مرورگر داخلی ایتا خارج شویم؟</span>
            </div>

            <div className="space-y-2.5 text-xs text-slate-800 font-bold">
              <div className="flex items-center gap-2.5 bg-white p-2.5 rounded-xl border border-slate-200 shadow-xs">
                <span className="w-6 h-6 rounded-lg bg-indigo-600 text-white flex items-center justify-center font-black text-xs shrink-0">
                  ۱
                </span>
                <span>
                  روی علامت <strong className="text-indigo-700 font-black inline-flex items-center gap-1 mx-1"><FaEllipsisV size={11} /> سه نقطه</strong> در گوشه بالای صفحه کلیک کنید.
                </span>
              </div>

              <div className="flex items-center gap-2.5 bg-white p-2.5 rounded-xl border border-slate-200 shadow-xs">
                <span className="w-6 h-6 rounded-lg bg-indigo-600 text-white flex items-center justify-center font-black text-xs shrink-0">
                  ۲
                </span>
                <span>
                  گزینه <strong className="text-emerald-700 font-black inline-flex items-center gap-1 mx-1"><FaExternalLinkAlt size={10} /> «باز کردن در مرورگر»</strong> یا <strong>«Open in browser»</strong> را انتخاب فرمایید.
                </span>
              </div>
            </div>
          </div>

          {/* دکمه‌های عملیات سریع و بسیار بولد */}
          <div className="pt-2 space-y-2.5">
            {/* دکمه باز کردن خودکار در کروم */}
            <button
              type="button"
              onClick={handleOpenChrome}
              className="w-full py-3.5 px-4 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white font-black text-sm rounded-2xl transition shadow-lg hover:shadow-xl active:scale-[0.98] flex items-center justify-center gap-2.5 cursor-pointer border border-blue-400"
            >
              <FaChrome size={20} className="animate-spin text-amber-300" style={{ animationDuration: '6s' }} />
              <span>باز کردن خودکار در مرورگر Chrome</span>
            </button>

            {/* دکمه کپی لینک */}
            <button
              type="button"
              onClick={handleCopyLink}
              className={`w-full py-3 px-4 font-black text-xs sm:text-sm rounded-2xl transition shadow-sm active:scale-[0.98] flex items-center justify-center gap-2 cursor-pointer border ${
                copied
                  ? 'bg-emerald-600 text-white border-emerald-700 ring-2 ring-emerald-300'
                  : 'bg-amber-100 hover:bg-amber-200 text-amber-950 border-amber-300'
              }`}
            >
              {copied ? (
                <>
                  <FaCheck size={16} className="text-white" />
                  <span>آدرس با موفقیت کپی شد! در مرورگر کروم باز کنید</span>
                </>
              ) : (
                <>
                  <FaCopy size={15} className="text-amber-700" />
                  <span>کپی کردن آدرس صفحه (جهت باز کردن در کروم)</span>
                </>
              )}
            </button>

            {/* بستن و ادامه با مسئولیت کاربر */}
            <button
              type="button"
              onClick={() => setIsOpen(false)}
              className="w-full py-2 text-center text-xs font-bold text-gray-500 hover:text-gray-800 transition cursor-pointer"
            >
              متوجه شدم، بستن اخطار و ادامه در همین صفحه
            </button>
          </div>
        </div>
      </div>
    </div>
  );

  return createPortal(modalContent, document.body);
}
