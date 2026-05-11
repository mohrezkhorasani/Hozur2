import { useState, useRef, useEffect } from "react";
import { useNavigate } from 'react-router-dom';

export default function ModernMediaSlider({ items = [], interval = 5000 ,loading=true}) {
  const [current, setCurrent] = useState(0);
  const touchStartX = useRef(null);
  const autoPlayRef = useRef(null);
  const navigate = useNavigate();

  const nextSlide = () => {
    if (items.length > 1) {
      setCurrent((prev) => (prev + 1) % items.length);
    }
  };

  const prevSlide = () => {
    if (items.length > 1) {
      setCurrent((prev) => (prev - 1 + items.length) % items.length);
    }
  };

  const handleTouchStart = (e) => {
    if (items.length > 1) {
      touchStartX.current = e.touches[0].clientX;
    }
  };

  const handleTouchEnd = (e) => {
    if (items.length > 1 && touchStartX.current !== null) {
      const endX = e.changedTouches[0].clientX;
      if (touchStartX.current - endX > 50) {
        nextSlide();
      } else if (endX - touchStartX.current > 50) {
        prevSlide();
      }
      startAutoPlay();
    }
    touchStartX.current = null;
  };

  useEffect(() => {
    if (items.length > 1) {
      startAutoPlay();
    }
    return () => stopAutoPlay();
  }, [items.length, interval]);

  const startAutoPlay = () => {
    stopAutoPlay();
    if (items.length > 1) {
      autoPlayRef.current = setInterval(nextSlide, interval);
    }
  };

  const stopAutoPlay = () => {
    if (autoPlayRef.current) {
      clearInterval(autoPlayRef.current);
      autoPlayRef.current = null;
    }
  };

 function onClickBox(item) {
    if (item.href) {
        // بررسی می‌کنیم آیا لینک کامل است یا نسبی
        let finalUrl = item.href;
        
        // اگر لینک با http:// یا https:// شروع می‌شود، همان لینک کامل است
        if (item.href.startsWith('http://') || item.href.startsWith('https://')) {
            finalUrl = item.href;
        } 
        // اگر لینک با / شروع می‌شود (نسبی)، آن را با آدرس پایه ترکیب می‌کنیم
        else if (item.href.startsWith('/')) {
            // آدرس پایه را از environment variable یا ثابت تعریف کنید
            const baseUrl = process.env.REACT_APP_BASE_URL || 'https://vc2.hekmateislami.com';
            finalUrl = `${baseUrl}${item.href}`;
        }
        // در غیر این صورت (لینک بدون / در ابتدا)
        else {
            const baseUrl = process.env.REACT_APP_BASE_URL || 'https://vc2.hekmateislami.com';
            finalUrl = `${baseUrl}/${item.href}`;
        }
        
        // باز کردن لینک در تب جدید یا همان صفحه (بسته به نیاز)
        window.open(finalUrl, '_blank');
        // یا اگر می‌خواهید در همان صفحه باز شود: navigate(finalUrl);
    }
}

  const controlsActive = items.length > 1;

  return (
    <div className="relative w-full h-full overflow-hidden rounded-2xl shadow-xl bg-black">
      {/* Slides */}
      <div
        className="flex transition-transform duration-500 h-full"
        style={{ transform: `translateX(${current * 100}%)` }}
        onTouchStart={controlsActive ? handleTouchStart : null}
        onTouchEnd={controlsActive ? handleTouchEnd : null}
      >
        {items.map((item, idx) => (
          <div
            key={idx}
            onClick={() => onClickBox(item)}
            className="w-full h-fill shrink-0 flex justify-center items-center cursor-pointer overflow-hidden"
          >
            {item.type === "image" || item.type === "gif" ? (
              <img
                src={item.link}
                className="w-full h-full "
                loading="lazy"
                onError={(e) => (e.target.src = "/fallback.jpg")}
                alt={`اسلاید ${idx + 1}`}
              />
            ) : null}

            {item.type === "video" ? (
              <video
                src={item.link}
                muted
                autoPlay={controlsActive}
                loop
                playsInline
                preload="auto"
                className="w-full h-full object-cover"
                onError={(e) => console.log("Video load error:", e)}
              />
            ) : null}
          </div>
        ))}
      </div>

      {/* Buttons */}
      {controlsActive && (
        <>
          <button
            onClick={() => {
              nextSlide();
              startAutoPlay();
            }}
            className="absolute top-1/2 left-3 -translate-y-1/2 bg-[#0B3F32]/20 backdrop-blur-md text-[#0B3F32] p-2 sm:p-3 rounded-full hover:bg-white/40 transition text-xl sm:text-2xl font-bold"
            aria-label="اسلاید قبلی"
          >
            ›
          </button>

          <button
            onClick={() => {
              prevSlide();
              startAutoPlay();
            }}
            className="absolute top-1/2 right-3 -translate-y-1/2 bg-[#0B3F32]/20 backdrop-blur-md text-[#0B3F32] p-2 sm:p-3 rounded-full hover:bg-white/40 transition text-xl sm:text-2xl font-bold"
            aria-label="اسلاید بعدی"
          >
            ‹
          </button>

          {/* Dots */}
          <div className="absolute bottom-0 py-2 sm:py-3 w-full flex justify-center gap-1.5 sm:gap-2 bg-[#0b3f3234]">
            {items.map((_, idx) => (
              <div
                key={idx}
                onClick={() => {
                  setCurrent(idx);
                  startAutoPlay();
                }}
                className={`w-2 h-2 sm:w-3 sm:h-3 rounded-full cursor-pointer transition ${current === idx ? "bg-[#0B3F32]" : "bg-[#0B3F32]/40"
                  }`}
                aria-label={`رفتن به اسلاید ${idx + 1}`}
              ></div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}