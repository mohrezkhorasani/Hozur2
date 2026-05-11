import React, { useState } from 'react';

function HomeNormalUser() {
  const [meetingLink, setMeetingLink] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLinkChange = (event) => {
    setMeetingLink(event.target.value);
  };

  const handleJoinClick = () => {
    if (!meetingLink) {
      alert('لطفا لینک ورود به جلسه را وارد کنید.');
      return;
    }

    // در اینجا می‌توانید بررسی کنید که آیا لینک معتبر است یا خیر
    // برای سادگی، فرض می‌کنیم لینک همیشه معتبر است

    setLoading(true);

    // شبیه‌سازی فرآیند ورود به جلسه (مثلاً با باز کردن لینک در تب جدید)
    setTimeout(() => {
      // در یک برنامه واقعی، ممکن است بخواهید با API بررسی کنید یا مستقیماً کاربر را هدایت کنید
      window.open(meetingLink, '_blank');
      setLoading(false);
    }, 1500); // تاخیر برای شبیه‌سازی فرآیند بارگذاری
  };

  return (
    <div className="flex justify-center items-center min-h-screen bg-[#f3f6fb] p-4">
      <div className="w-full max-w-md mx-auto bg-white rounded-xl shadow-xl shadow-blue-gray-900/50 p-8">
        <div className="text-center text-xl font-semibold mb-5 text-gray-800">ورود به جلسه</div>

        <div className="mb-5">
          <label htmlFor="meetingLink" className="block mb-2 text-sm text-gray-700">
            لینک ورود به جلسه
          </label>
          <input
            type="url" // استفاده از type="url" برای اعتبارسنجی اولیه مرورگر
            id="meetingLink"
            value={meetingLink}
            onChange={handleLinkChange}
            placeholder="https://example.com/join/meeting-id"
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#0b3f32e3]"
            required
          />
        </div>

        <button
          onClick={handleJoinClick}
          disabled={loading}
          className={`w-full py-3 rounded-xl text-white font-medium text-base flex items-center justify-center transition-colors duration-200
            ${loading
              ? 'bg-gray-400 cursor-not-allowed'
              : 'bg-[#0078B5] hover:bg-[#005f8d]'
            }`}
        >
          {loading ? "در حال ورود..." : "ورود به جلسه"}
        </button>
      </div>
    </div>
  );
}

export default HomeNormalUser;
