import Cookies from "js-cookie";
import LoginModal from "../../Modal/Auth/LoginModal";
import { useNavigate, useOutletContext } from "react-router-dom";
import BreadCrumbParent from "../../Component/BreadCrumbParent";
import DashboardPageItem from "@/Component/Dashboard/DashboardPageItem";
import { useEffect, useState } from "react";
import { useAuth } from "@/Tools/AuthProvider";
import useFetch from "@/Tools/Hooks/UseFetchHook";

export default function ProfileOrders() {
    const navigation = useNavigate();

    const { setBreadcrumb } = useOutletContext(); // گرفتن تابع از Layout
    const { user, UserLoading } = useAuth();
    const breadCrumbs = [
        { label: "پیستاچی هاب", href: "/" },
        { label: "حساب کاربری من", href: "/me/my-account" },
        { label: "سفارش‌ها", href: "/me/orders" },
    ];

    useEffect(() => {
        if (setBreadcrumb) {
            setBreadcrumb(<BreadCrumbParent breadCrumbs={breadCrumbs} />);
        }
    }, []);
    const { data: orders, loading: loadingOrders, error: errorOrders, refetch: refetchOrders } = useFetch(
        'https://api.pistachihub.com/orders',
        []
    );


    if (UserLoading) {
        return <div className="flex flex-col md:flex-col lg:flex-col 2xl:flex-col gap-[2%] w-full px-[2%]">در حال بارگذاری...</div>;
    }
    // if (errorOrders)
    //     return <div>{errorOrders}</div>


    if (!Cookies.get("t1")) {
        return (
            <div dir="rtl" className="min-h-screen bg-white">
                <LoginModal />
            </div>
        )
    }
    return (
        <div className="flex flex-col md:flex-col lg:flex-col 2xl:flex-col gap-[2%] w-full px-[2%]">
            <div className="flex flex-col w-fit">
                <h1 className="text-black font-extrabold text-2xl relative
                 before:content-[''] before:absolute before:right-0 before:bottom-0 before:w-full before:h-[15%]
                 before:bg-linear-to-l before:from-[#11207A] before:to-white before:rounded-3xl pb-[10%]
                 before:bg-gray-400">
                    سفارش ها
                </h1>

            </div>
            {loadingOrders && (
                <div className="flex flex-col md:flex-row lg:flex-row 2xl:flex-row gap-[2%]">
                    در حال بارگیری
                </div>

            )}
            {
                !loadingOrders && orders.length > 0 && (

                    < div className="flex flex-col md:flex-row lg:flex-row 2xl:flex-row gap-[2%]">
                    </div>
                )
            }
            {!loadingOrders && orders.length == 0 && (
                <div
                    className="flex flex-row mt-[2%] text-sm bg-[#FCFDF4] w-full rounded-xl px-[2%] py-[1%] 
                    border-r-4
                    rounded-tr-none border-[#BBD430] before:text-2xl justify-center text-center items-center">

                    <p
                        className="before:inline-block  before:content-['\e028'] before:font-['WooCommerce'] before:text-[#BBD430]"></p>
                    <p className="text-[#515151] text-xs before:absolute mx-5 md:text-sm lg:text-sm 2xl:text-sm
                    
                    "> هیچ سفارشی هنوز ثبت نشده است.
                    </p>
                    <a
                        href="/shop"
                        className={`mt-2 sm:mt-0 bg-[#2B3992] 
            hover:scale-102 rounded-xl text-white text-xs sm:text-base px-3 py-1  hover:rounded-b-none
            font-semibold sm:mr-auto transition-all duration-300`}
                    >
                        مرور محصولات
                    </a>
                </div>
            )}


        </div >
    )
}
