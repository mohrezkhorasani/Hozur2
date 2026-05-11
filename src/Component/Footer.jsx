import React from "react";
import ImageBackground from "@assets/ax/ups/2025/02/footerbg.png";

export default function Footer() {
    return (
        <section
            id="footer"
            className="mt-2 text-xs relative flex flex-col overflow-hidden rounded-t-4xl lg:mx-60 pt-2 justify-center px-4 sm:px-10 md:mx-60"
        >
            {/* تصویر پس‌زمینه */}
            <div
                className="absolute inset-0 bg-cover bg-center"
                style={{ backgroundImage: `url(${ImageBackground})` }}
            ></div>

            {/* هاله / overlay آبی */}
            <div className="absolute inset-0 bg-[#0b3f32e3] opacity-80"></div>

            {/* محتوای اصلی روی overlay */}
            <div className="flex flex-col relative z-10" >
                <div className="flex flex-col items-center 
                sm:flex-row sm:items-start sm:text-right p-4 sm:p-8 text-white flex-wrap gap-4 text-xl">
                    {/* لوگو */}
                    <img
                        src="https://hekmateislami.com/wp-content/uploads/2025/12/logo-majma-low.png"
                        className="w-15 sm:w-3xs  mb-4 sm:mb-0 "
                    />

                    {/* لینک‌ها */}
                    <div className="flex flex-col items-center sm:items-start sm:mr-8 mb-4 sm:mb-0 ">
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="https://hekmateislami.com">پایگاه اینترنتی</a>
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="https://shop.hekmateislami.com/">فروشگاه کتاب</a>
                    </div>

                    <div className="flex flex-col items-center sm:items-start sm:mr-4 mb-4 sm:mb-0">
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="#">درخواست اشتراک کلاس های مجازی</a>
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="#">قوانین اشتراک کلاس های مجازی</a>
                    </div>

                    <div className="flex flex-col items-center sm:items-start sm:mr-4 mb-4 sm:mb-0">
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="#">شعبات مجمع عالی</a>
                        <a className="hover:text-[#30d4cc] transition-all duration-300" href="#">جدول زمانبندی</a>
                    </div>

                </div>

                {/* Footer Bottom */}
                <div className="flex flex-col sm:flex-row items-center mt-4 sm:mt-0 bg-myg z-50 mx-auto sm:mx-[30%] py-2 rounded-t-2xl text-myb px-4 sm:px-5 text-xs">
                    <p className="text-right w-full sm:w-auto mb-1 sm:mb-0">
                        کلیه حقوق متعلق به مجمع عالی حکمت اسلامی میباشد.
                    </p>

                    <div className="flex-1 mx-0 sm:mx-2 border-t sm:border-t-0 border-gray-400 sm:border-l"></div>

                    <p className="text-left w-full sm:w-auto">
                        
                    </p>
                </div>
            </div>
        </section >
    );
}