// src/Component/Home/Counter/Counter.tsx
import React, { useEffect, useRef, useState } from 'react';

interface CounterProps {
  end: number;
  duration?: number;
  suffix?: string;
  label: string;
  icon?: React.ReactNode;
  start?: number;
}

const Counter: React.FC<CounterProps> = ({
  end,
  duration = 2000,
  suffix = '',
  label,
  icon,
  start = 0,
}) => {
  const [count, setCount] = useState(start);
  const [isVisible, setIsVisible] = useState(false);
  const counterRef = useRef<HTMLDivElement>(null);
  const animationRef = useRef<number | null>(null);
  const hasAnimated = useRef(false);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        const [entry] = entries;
        if (entry.isIntersecting && !hasAnimated.current) {
          setIsVisible(true);
          hasAnimated.current = true;
          startAnimation();
        }
      },
      {
        threshold: 0.3,
        rootMargin: '0px',
      }
    );

    const currentRef = counterRef.current;
    if (currentRef) {
      observer.observe(currentRef);
    }

    return () => {
      if (currentRef) {
        observer.unobserve(currentRef);
      }
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
    };
  }, []);

  const startAnimation = () => {
    const startTime = performance.now();
    const startValue = start;
    const endValue = end;

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);
      
      // Ease out cubic
      const easeOut = 1 - Math.pow(1 - progress, 3);
      const currentCount = Math.round(startValue + (endValue - startValue) * easeOut);
      
      setCount(currentCount);

      if (progress < 1) {
        animationRef.current = requestAnimationFrame(animate);
      } else {
        setCount(endValue);
        animationRef.current = null;
      }
    };

    animationRef.current = requestAnimationFrame(animate);
  };

  const formatNumber = (num: number) => {
    return num.toLocaleString('fa-IR');
  };

  return (
    <div ref={counterRef} className="text-center group">
      {icon && (
        <div className="text-4xl mb-3 text-[#30d4cc] group-hover:scale-110 transition-transform duration-300">
          {icon}
        </div>
      )}
      <div className="text-4xl sm:text-5xl lg:text-6xl font-extrabold text-[#02592A] mb-2 font-sans">
        {formatNumber(count)}
        {suffix}
      </div>
      <div className="text-sm sm:text-base text-[#6b7280] font-medium">
        {label}
      </div>
    </div>
  );
};

export default Counter;