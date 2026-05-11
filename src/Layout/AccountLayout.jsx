import { Outlet } from "react-router-dom";
import HeaderStickTop from "../Component/HeaderStickTop";
import Footer from "../Component/Footer";
import FooterUpper from "../Component/FooterUpper";
import BreadCrumbParent from "@/Component/BreadCrumbParent";
import { useState } from "react";
import { useNavigate, useOutletContext } from "react-router-dom";

const AccountLayout = () => {



    const [breadcrumb, setBreadcrumb] = useState(null);
    return (
        <div className="min-h-screen bg-white"
            dir="rtl"

        >
            <HeaderStickTop />

            {/* محتوای هر صفحه اینجا رندر می‌شود */}
            <main className="min-h-screen">
                <section id="bread-crumb" className="flex w-full px-4 md:px-60 mt-[4%]">
                    {breadcrumb } 
                </section>
                <div className="flex flex-col bg-white shadow-2xl shadow-gray-300 rounded-lg mx-3 lg:mx-60 mt-3">
                    <div className="flex flex-col lg:flex-row divide-y lg:divide-y-0 lg:divide-x divide-gray-200 p-4">

                        {/* پروفایل */}
                        <div className="flex flex-row px-2 sm:px-5 items-center justify-center lg:justify-start w-full lg:w-fit mb-4 lg:mb-0 ">
                            <img
                                src="https://secure.gravatar.com/avatar/?s=150&d=mm&r=g"
                                className="rounded-2xl w-14 h-14 sm:w-16 sm:h-16 my-5"
                                style={{ boxShadow: "-10px -3px 20px rgba(17, 32, 122, 0.5)" }}
                            />
                            <div className="flex flex-col mr-4 sm:mr-5 lg:w-75 md:w-55">
                                <h1 className="font-semibold text-sm sm:text-base truncate lg:ml-[25%] md:ml-[25%] 2xl:ml-[25%]">محمدرضا خراسانی شیخ اهوازی </h1>
                                <p className="font-light text-xs sm:text-sm text-gray-500">
                                    4 ساعت قبل
                                </p>
                            </div>
                        </div>

                        {/* منو */}
                        <div className="
                        w-full
                        grid grid-cols-2 gap-3
                        sm:grid-cols-3
                        md:grid-cols-4
                        lg:flex lg:items-center lg:overflow-x-auto lg:gap-0
                        text-[#11207A]
                        ">

                            {/* item */}
                            <a href="/me/my-account" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Discovery text-[28px] mb-1 transition-colors group-hover:text-[#11207A]">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className={`text-sm  ${window.location.pathname.includes("/my-account") ? "text-myb" : "text-black"}`}>پیشخوان</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            <a href="/me/orders" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Document text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className={`text-sm   ${window.location.pathname.includes("/orders") ? "text-myb" : "text-black"}`}>سفارش‌ها</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            <a href="#" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Heart text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                    <span className="path3"></span>
                                </span>
                                <span className={`text-sm  ${window.location.pathname === "/favorites" ? "text-myb" : "text-black"}`}>لیست علاقه‌مندی</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            <a href="#" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Download text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className={`text-sm  ${window.location.pathname === "/d" ? "text-myb" : "text-black"}`}>دانلودها</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            <a href="#" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Location text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className={`text-sm  ${window.location.pathname === "/address" ? "text-myb" : "text-black"}`}>نشانی</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            <a href="#" className="group relative flex flex-col items-center justify-center text-center py-3 lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Profile text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className={`text-sm  ${window.location.pathname === "/details" ? "text-myb" : "text-black"}`}>جزئیات حساب</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                            {/* logout */}
                            <a href="#" className="group relative flex flex-col items-center justify-center text-center py-3 text-rose-500 hover:text-rose-700 lg:mr-auto lg:w-[100px] flex-shrink-0">
                                <span className="profile-nav-item-icon iconlyBulk-Logout text-[28px] mb-1 transition-colors">
                                    <span className="path1"></span>
                                    <span className="path2"></span>
                                </span>
                                <span className="text-sm">بیرون رفتن</span>
                                <span className="pointer-events-none absolute bottom-0 left-1/2 h-[2px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-10"></span>
                            </a>

                        </div>
                    </div>
                    <div className="flex mx-4 my-7">
                        <Outlet context={{ setBreadcrumb }} />
                    </div>
                </div>
            </main>

            <FooterUpper />
            <Footer />
        </div>
    );
};

export default AccountLayout;