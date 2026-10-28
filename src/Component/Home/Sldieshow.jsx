import { useState, useRef, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import config from './../../config';

export default function ModernMediaSlider({ items = [], interval = 5000, loading = true }) {
  const [current, setCurrent] = useState(0);
  const [hoveredIndex, setHoveredIndex] = useState(null);
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
    console.log(item)
    if (item.href) {
      let finalUrl = item.href;

      if (item.href.startsWith('http://') || item.href.startsWith('https://')) {
        finalUrl = item.href;
      }
      else if (item.href.startsWith('/')) {
        const baseUrl = config.REACT_BASE_URL || 'https://vc2.hekmateislami.com';
        finalUrl = `${baseUrl}${item.href}`;
      }
      else {
        const baseUrl = config.REACT_BASE_URL || 'https://vc2.hekmateislami.com';
        finalUrl = `${baseUrl}/${item.href}`;
      }

      window.open(finalUrl, '_blank');
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
            onMouseEnter={() => setHoveredIndex(idx)}
            onMouseLeave={() => setHoveredIndex(null)}
            className="w-full h-full shrink-0 flex justify-center items-center cursor-pointer overflow-hidden relative group"
          >
            {item.type === "image" || item.type === "gif" ? (
              <>
                <img
                  src={item.link}
                  className={`w-full h-full object-cover transition-all duration-700 ${hoveredIndex === idx ? 'scale-110 brightness-110' : 'scale-100 brightness-100'
                    }`}
                  loading="lazy"
                  onError={(e) => (e.target.src = "/fallback.jpg")}
                  alt={`اسلاید ${idx + 1}`}
                />

                {/* خط تزئینی پایین */}
                <div className={`absolute bottom-0 left-0 right-0 h-1
                 bg-gradient-to-r from-[#02592A] via-[#30d4cc] to-[#02592A] transition-all duration-500 ${hoveredIndex === idx ? 'opacity-100' : 'opacity-0'
                  }`}></div>



                {/* Border Glow Effect */}
                <div className={`absolute inset-0 border-4 border-[#EC5800]/40 rounded-2xl transition-all duration-500 ${hoveredIndex === idx ? 'opacity-100 scale-95' : 'opacity-0 scale-100'
                  }`}></div>
                {/* Overlay با رنگ گرادیان جذاب */}
                <div className={`absolute inset-0 bg-gradient-to-tr from-[#30D4CC]/60 via-[#E6F7F6]/30 to-transparent transition-opacity duration-500 ${hoveredIndex === idx ? 'opacity-100' : 'opacity-0'
                  }`}></div>

                {/* Slide Number - در گوشه نمایش داده می‌شود */}
                <div className={`absolute bottom-4 right-4 bg-black/50 backdrop-blur-sm text-white text-xs px-3 py-1 rounded-full transition-all duration-500 ${hoveredIndex === idx ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-2'
                  }`}>
                  {idx + 1} / {items.length}
                </div>
              </>
            ) : null}

            {item.type === "video" ? (
              <video
                src={item.link}
                muted
                autoPlay={controlsActive}
                loop
                playsInline
                preload="auto"
                className={`w-full h-full object-cover transition-all duration-700 ${hoveredIndex === idx ? 'scale-110 brightness-110' : 'scale-100 brightness-100'
                  }`}
                onError={(e) => console.log("Video load error:", e)}
              />
            ) : null}
          </div>
        ))}
      </div>

      {/* Buttons */}
      {controlsActive && (
        <>
          {/* Dots */}
          <div className="absolute bottom-0 py-2 sm:py-3 w-full flex justify-center gap-1.5 sm:gap-2 bg-gradient-to-t from-black/50 to-transparent">
            {items.map((_, idx) => (
              <div
                key={idx}
                onClick={() => {
                  setCurrent(idx);
                  startAutoPlay();
                }}
                className={`w-2 h-2 sm:w-3 sm:h-3 rounded-full cursor-pointer transition-all duration-300 ${current === idx
                  ? "bg-white w-6 sm:w-8"
                  : "bg-white/40 hover:bg-white/70"
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