import React, { useState } from "react";

export default function MobileModal({ onNext, dismiss }) { // اضافه شدن dismiss به props
  const [mobile, setMobile] = useState("");

  const handleSendCode = () => {
    if (!mobile.match(/^09\d{9}$/)) {
      alert("شماره موبایل معتبر وارد کنید.");
      return;
    }
    onNext(mobile);
  };

  // تابعی برای هندل کردن کلیک روی پس‌زمینه
  const handleBackgroundClick = (e) => {
    // اطمینان حاصل کنید که کلیک دقیقا روی پس‌زمینه بوده است، نه روی محتوای داخل مودال
    // این کار با بررسی اینکه آیا target همان div والد است انجام می‌شود
    if (e.target === e.currentTarget) {
      dismiss(); // فراخوانی تابع dismiss
    }
  };

  return (
    /*
      - اضافه کردن onClick به این div برای هندل کردن کلیک روی پس‌زمینه
      - اطمینان از اینکه کلیک روی خود div پس‌زمینه انجام شده، نه روی عناصر داخلی آن
    */
    <div
      className="fixed inset-0 z-50 flex items-center justify-center w-full h-screen bg-black/50 backdrop-blur-sm p-4"
      onClick={handleBackgroundClick} // اضافه کردن event handler
    >
      {/*
        برای جلوگیری از بسته شدن مودال هنگام کلیک روی محتوای داخلی،
        یک event handler دیگر روی div داخلی اضافه می‌کنیم که event را متوقف کند.
      */}
      <div
        className="bg-white rounded-2xl w-full max-w-[320px] p-8 shadow-2xl"
        onClick={(e) => e.stopPropagation()} // جلوگیری از انتشار event به والد
      >
        <h2 className="text-xl font-bold mb-6 text-center text-gray-800">
          ورود با شماره موبایل
        </h2>

        <input
          type="tel" // تغییر به tel برای کیبورد بهتر در موبایل
          dir="ltr"  // چپ‌چین کردن شماره برای خوانایی بهتر
          placeholder="09123456789"
          value={mobile}
          onChange={(e) => setMobile(e.target.value)}
          className="w-full p-3 border border-gray-300 rounded-lg mb-4 text-center text-lg tracking-widest focus:ring-2 focus:ring-blue-500 outline-none"
        />

        <button
          onClick={handleSendCode}
          className="w-full bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700 transition-colors font-bold text-lg"
        >
          ارسال کد
        </button>
      </div>
    </div>
  );
}
