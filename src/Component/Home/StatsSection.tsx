// src/Component/Home/StatsSection/StatsSection.tsx
import React from 'react';
import Counter from '../../Tools/Counter';
import { FaChalkboardTeacher, FaUsers, FaCalendarAlt } from 'react-icons/fa';
import { GiBookshelf } from 'react-icons/gi';

const StatsSection: React.FC = () => {
  const stats = [
    { 
      end: 1500, 
      suffix: '+', 
      label: 'جلسه علمی برگزار شده', 
      icon: FaCalendarAlt, 
      duration: 2500,
      bgGradient: 'from-[#02592A] to-[#30d4cc]',
      hoverBg: 'hover:shadow-[#02592A]/20'
    },
    { 
      end: 100, 
      suffix: '+', 
      label: 'استاد و مدرس', 
      icon: FaChalkboardTeacher, 
      duration: 2000,
      bgGradient: 'from-[#0b3f32] to-[#30d4cc]',
      hoverBg: 'hover:shadow-[#0b3f32]/20'
    },
    { 
      end: 10000, 
      suffix: '+', 
      label: 'شرکت‌کننده در جلسات', 
      icon: FaUsers, 
      duration: 3000,
      bgGradient: 'from-[#30d4cc] to-[#02592A]',
      hoverBg: 'hover:shadow-[#30d4cc]/20'
    },
    { 
      end: 20, 
      suffix: '+', 
      label: 'موضوع تخصصی', 
      icon: GiBookshelf, 
      duration: 1800,
      bgGradient: 'from-[#1a6b3e] to-[#30d4cc]',
      hoverBg: 'hover:shadow-[#1a6b3e]/20'
    },
  ];

  return (
    <section className="relative py-16 sm:py-20 lg:py-24 overflow-hidden">
      <div className="absolute inset-0 "></div>
      

      <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-12 sm:mb-16">
          <div className="inline-flex items-center gap-2 bg-white/80 backdrop-blur-sm px-5 py-2 rounded-full mb-4 border border-[#30d4cc]/20 shadow-sm">
            <span className="text-[#02592A] text-sm font-semibold tracking-wide">📊 آمار و ارقام</span>
          </div>
          <h2 className="text-2xl sm:text-3xl lg:text-4xl font-extrabold text-[#111827] mb-3">
            مجمع حکمت اسلامی در یک نگاه
          </h2>
          <p className="text-[#6b7280] text-sm sm:text-base max-w-2xl mx-auto">
            دستاوردهای مجمع عالی حکمت اسلامی در طول سال‌های فعالیت
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 sm:gap-8">
          {stats.map((stat, index) => {
            const Icon = stat.icon;
            return (
              <div
                key={index}
                className={`group relative bg-white rounded-2xl shadow-lg ${stat.hoverBg} hover:shadow-2xl transition-all duration-500 p-6 sm:p-8 hover:-translate-y-2 border border-white/50 overflow-hidden`}
              >
                {/* Gradient background on hover */}
                <div className={`absolute inset-0 bg-gradient-to-br ${stat.bgGradient} opacity-0 group-hover:opacity-10 transition-opacity duration-500 rounded-2xl`}></div>
                
                {/* Decorative line */}
                <div className={`absolute top-0 left-0 right-0 h-1 bg-gradient-to-r ${stat.bgGradient} opacity-0 group-hover:opacity-100 transition-opacity duration-500`}></div>
                
                {/* Content */}
                <div className="relative">
                  <div className="flex justify-center mb-4">
                    <div className={`p-3 rounded-xl bg-gradient-to-br ${stat.bgGradient} bg-opacity-10 group-hover:scale-110 transition-transform duration-300`}>
                      <Icon className="text-white text-2xl" />
                    </div>
                  </div>
                  <Counter
                    end={stat.end}
                    duration={stat.duration}
                    suffix={stat.suffix}
                    label={stat.label}
                    icon={null}
                  />
                </div>
              </div>
            );
          })}
        </div>

        <div className="text-center mt-10 sm:mt-12">
          <div className="inline-block bg-white/80 backdrop-blur-sm rounded-2xl px-6 py-3 border border-[#30d4cc]/20 shadow-sm">
            <p className="text-sm text-[#6b7280]">
              ✦ این آمار نشان‌دهنده تلاش‌های مستمر مجمع عالی حکمت اسلامی در راستای ترویج معارف اسلامی و علوم انسانی است ✦
            </p>
          </div>
        </div>
      </div>
    </section>
  );
};

export default StatsSection;