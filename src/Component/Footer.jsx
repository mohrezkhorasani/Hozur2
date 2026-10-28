import React from 'react';

const Footer = () => {
  return (
    <footer className="bg-[#003117] text-white py-8 mt-auto" dir="rtl">
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* بخش اول: درباره ما */}
          <div>
            <h3 className="text-xl font-bold mb-4 text-amber-400">درباره ما</h3>
            <p className="text-gray-400 text-sm leading-relaxed">
در زمستان ۱۳۸۲، به دستور مقام معظم رهبری مبنی بر تشکیل مرکزی برای علوم عقلی در حوزه، جمعی از اساتید با نظارت آیات جوادی آملی، سبحانی و مصباح یزدی، اساسنامه را تدوین و هیئت مؤسس را تعیین کردند.
پس از تأیید مراجع و تشکیل مجمع عمومی، مجمع عالی حکمت اسلامی فعالیت رسمی خود را از اسفند ۱۳۸۴ با برگزاری نخستین نشست کرسی نظریه‌پردازی آغاز کرد.
            </p>
          </div>

          {/* بخش دوم: لینک‌های مفید */}
          <div>
            <h3 className="text-xl font-bold mb-4 text-amber-400">لینک‌های مفید</h3>
            <ul className="space-y-2 text-sm">
              <li>
                <a href="#" className="text-gray-400 hover:text-amber-400 transition-colors duration-300">
                  صفحه اصلی
                </a>
              </li>
              <li>
                <a href="https://shop.hekmateislami.com" className="text-gray-400 hover:text-amber-400 transition-colors duration-300">
                  محصولات انتشارات مجمع عالی
                </a>
              </li>
              <li>
                <a href="https://hekmateislami.com" className="text-gray-400 hover:text-amber-400 transition-colors duration-300">
                  پایگاه اینترنتی
                </a>
              </li>
              <li>
                <a href="contact-form" className="text-gray-400 hover:text-amber-400 transition-colors duration-300">
                  تماس با ما
                </a>
              </li>
            </ul>
          </div>

          {/* بخش سوم: اطلاعات تماس */}
          <div>
            <h3 className="text-xl font-bold mb-4 text-amber-400">تماس با ما</h3>
            <ul className="space-y-2 text-sm text-gray-400">
              <li className="flex items-center gap-2">
                <span>📧</span>
                <span>info@hekmateislami.com</span>
              </li>
              <li className="flex items-center gap-2">
                <span>📞</span>
                <span>025۳۷۷۵۷۶۱۰</span>
              </li>
              <li className="flex items-center gap-2">
                <span>📍</span>
                <span>نشانی: قم- خیابان ۱۹ دی (باجک) کوچه ۱۰ فرعی اول سمت چپ پلاک ۵</span>
              </li>
            </ul>
          </div>
        </div>

        {/* خط جداکننده و کپی‌رایت */}
        <div className="border-t border-gray-800 mt-8 pt-6 text-center text-gray-500 text-sm">
          <p>© {new Date().getFullYear()} تمامی حقوق مادی و معنوی این سایت متعلق به مجمع عالی حکمت اسلامی می باشد.
.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;