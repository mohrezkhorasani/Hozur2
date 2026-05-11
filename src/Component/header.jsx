// src/components/Header.tsx
import { useState } from 'react';
import { ShoppingCart, User, Search, Menu, X } from 'lucide-react';

function Header() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  return (
    <header className="bg-[#11217a00] text-white z-50 transition-all">
      {/* نوار بالایی (اسکرول میره) */}
      <div className="bg-green-600 text-center py-1.5 text-xs sm:text-sm font-medium px-2">
        ارسال رایگان برای سفارش بالای ۱ میلیون تومان | پشتیبانی ۲۴ ساعته: ۰۹۱۲xxx xxxx
      </div>

      {/* هدر اصلی (sticky) */}
      <div className="sticky top-0 bg-[#11217a]">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-14 sm:h-16 md:h-20">
            {/* لوگو */}
            <a
              href="/"
              className="flex items-center gap-2 sm:gap-3 font-bold text-base sm:text-lg md:text-2xl whitespace-nowrap"
            >
              <span className="text-green-400">پسته</span>ذوالفقاری
              <span className="text-green-400 text-xl md:text-2xl">•</span>
              <span className="hidden sm:inline">پسته اصیل دامغان</span>
            </a>

            {/* منو دسکتاپ */}
            <nav className="hidden md:flex items-center gap-6 lg:gap-8 text-base lg:text-lg">
              <a href="/" className="hover:text-green-400 transition">خانه</a>
              <a href="/products" className="hover:text-green-400 transition">محصولات</a>
              <a href="/about" className="hover:text-green-400 transition">درباره ما</a>
              <a href="/contact" className="hover:text-green-400 transition">تماس با ما</a>
              <a href="/blog" className="hover:text-green-400 transition">بلاگ</a>
            </nav>

            {/* آیکون‌ها */}
            <div className="flex items-center gap-3 sm:gap-4 md:gap-6 shrink-0">
              <button aria-label="جستجو" className="hover:text-green-400 transition">
                <Search size={22} />
              </button>

              <a href="/my-account" aria-label="حساب کاربری" className="hover:text-green-400 transition">
                <User size={22} />
              </a>

              <a href="/cart" className="relative hover:text-green-400 transition">
                <ShoppingCart size={22} />
                <span className="absolute -top-2 -right-2 bg-green-500 text-white text-[10px] rounded-full w-5 h-5 flex items-center justify-center">
                  ۳
                </span>
              </a>

              {/* همبرگر */}
              <button
                className="md:hidden text-white focus:outline-none"
                onClick={() => setIsMenuOpen(!isMenuOpen)}
                aria-label="باز کردن منو"
              >
                {isMenuOpen ? <X size={26} /> : <Menu size={26} />}
              </button>
            </div>
          </div>

          {/* منوی موبایل */}
          {isMenuOpen && (
            <nav className="md:hidden py-4 border-t border-green-700/30">
              <ul className="flex flex-col gap-2 text-center text-base sm:text-lg">
                <li><a href="/" className="block py-2 hover:text-green-400">خانه</a></li>
                <li><a href="/products" className="block py-2 hover:text-green-400">محصولات</a></li>
                <li><a href="/about" className="block py-2 hover:text-green-400">درباره ما</a></li>
                <li><a href="/contact" className="block py-2 hover:text-green-400">تماس با ما</a></li>
                <li><a href="/blog" className="block py-2 hover:text-green-400">بلاگ</a></li>
              </ul>
            </nav>
          )}
        </div>
      </div>
      <hr className="mb-6 border-gray-300 dark:border-gray-700 mx-60" />

    </header>
  );
}

export default Header;
