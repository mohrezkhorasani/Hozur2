// Pages/LoadingPage.jsx (ساده)
import { useState, useEffect } from 'react';
import { FaSpinner } from 'react-icons/fa';

const LoadingPage = () => {
  const [message, setMessage] = useState('در حال آماده سازی...');
  const [dots, setDots] = useState('');

  useEffect(() => {
    const messages = [
      'در حال بررسی سیستم...',
      'اتصال به سرور...',
      'بررسی امنیتی...',
      'آماده سازی محیط...',
      'به زودی آماده می‌شود...'
    ];
    
    let messageIndex = 0;
    const messageInterval = setInterval(() => {
      messageIndex = (messageIndex + 1) % messages.length;
      setMessage(messages[messageIndex]);
    }, 2000);
    
    const dotInterval = setInterval(() => {
      setDots(prev => prev.length >= 3 ? '' : prev + '.');
    }, 500);
    
    return () => {
      clearInterval(messageInterval);
      clearInterval(dotInterval);
    };
  }, []);

  return (
    <div dir="rtl" className="min-h-screen bg-gradient-to-br from-[#0b3f32] to-[#06261e] flex items-center justify-center p-4">
      <div className="text-center">
        {/* Animated Loader */}
        <div className="relative w-24 h-24 mx-auto mb-8">
          <div className="absolute inset-0 border-4 border-[#30d4cc]/20 rounded-full"></div>
          <div className="absolute inset-0 border-4 border-t-[#30d4cc] border-r-[#30d4cc] border-b-transparent border-l-transparent rounded-full animate-spin"></div>
          <div className="absolute inset-2 border-4 border-t-transparent border-r-transparent border-b-[#30d4cc] border-l-[#30d4cc] rounded-full animate-spin animation-delay-500"></div>
        </div>
        
        {/* Text */}
        <h2 className="text-xl font-bold text-white mb-2">
          مجمع عالی حکمت اسلامی
        </h2>
        <p className="text-[#30d4cc] text-sm">
          {message}
          <span className="inline-block w-6 text-right">{dots}</span>
        </p>
        
        {/* Progress Bar */}
        <div className="w-64 mx-auto mt-6 h-1 bg-white/10 rounded-full overflow-hidden">
          <div className="h-full bg-[#30d4cc] rounded-full animate-loading"></div>
        </div>
      </div>
      
      <style jsx>{`
        @keyframes loading {
          0% {
            width: 0%;
          }
          100% {
            width: 100%;
          }
        }
        .animate-loading {
          animation: loading 3s ease-in-out infinite;
        }
        .animation-delay-500 {
          animation-delay: 0.5s;
        }
      `}</style>
    </div>
  );
};

export default LoadingPage;