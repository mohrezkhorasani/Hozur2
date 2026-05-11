import Cookies from "js-cookie";
import LoginModal from "../Modal/Auth/LoginModal";
import { useNavigate, useOutletContext } from "react-router-dom";
import BreadCrumbParent from "../Component/BreadCrumbParent";
import DashboardPageItem from "@/Component/Dashboard/DashboardPageItem";
import { useEffect } from "react";
import { useAuth } from "@/Tools/AuthProvider";

export default function DashboardPage() {

    const { setBreadcrumb } = useOutletContext(); // گرفتن تابع از Layout
    const { user, UserLoading } = useAuth();
    console.log(user,)
    if (UserLoading) {
        return <div className="flex flex-col md:flex-col lg:flex-col 2xl:flex-col gap-[2%] w-full px-[2%]">در حال بارگذاری...</div>;
    }
    const navigation = useNavigate();
    const breadCrumbs = [
        { label: "پیستاچی هاب", href: "/" },
        { label: "حساب کاربری من", href: "/my-account" },
    ];
    useEffect(() => {
        if (setBreadcrumb) {
            setBreadcrumb(<BreadCrumbParent breadCrumbs={breadCrumbs} />);
        }
    }, []);
    if (!Cookies.get("t1")) {
        return (
            <div dir="rtl" className="min-h-screen bg-white">
                <LoginModal />
            </div>
        )
    }
    return (
        <div className="flex flex-col md:flex-col lg:flex-col 2xl:flex-col gap-[2%] w-full px-[2%]">
            <div className="flex flex-col md:flex-row lg:flex-row 2xl:flex-row gap-[2%]">
                <DashboardPageItem
                    icon={"icon-shopping-cart"}
                    title={"سبد خرید"}
                    count={3}
                />
                <DashboardPageItem
                    icon={"icon-check-circle"}
                    title={"سفارشات تکمیل شده"}
                    count={0}
                />
                <DashboardPageItem
                    icon={"icon-check-circle"}
                    title={"سفارشات درحال پردازش"}
                    count={0}
                />
                <DashboardPageItem
                    icon={"icon-file-text"}
                    title={"سفارشات در دست بررسی"}
                    count={1}
                />
            </div>

            <div
                className="mt-[5%] text-sm"
            >
                <p> سلام {user.name} ({user.name} نیستید؟ <a href="#" className="text-myb hover:underline hover:underline-offset-8
                transition-all duration-600">خارج شوید</a>) </p>
                <p className="flex flex-row">
                    از طریق پیشخوان حساب کاربری‌تان، می‌توانید
                    <a href="#" className="group relative flex flex-col text-myb flex-shrink-0 px-1">
                        <span>
                            سفارش‌های اخیرتان
                        </span>
                        <span className="pointer-events-none absolute bottom-0 left-1/2 h-[1px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-full">
                        </span>
                    </a>

                    را مشاهده،
                    <a href="#" className="group relative flex flex-col text-myb flex-shrink-0 px-1">
                        <span>
                            آدرس‌های حمل و نقل و صورتحساب‌تان
                        </span>
                        <span className="pointer-events-none absolute bottom-0 left-1/2 h-[1px] w-0 bg-current -translate-x-1/2 transition-all duration-300 group-hover:w-full">
                        </span>
                    </a>
                    را مدیریت و
                    <a href="#" className="group relative flex flex-col text-myb flex-shrink-0 px-1">
                        <span
                        >جزییات حساب کاربری و کلمه عبور خود را ویرایش کنید
                        </span>
                        <span className="pointer-events-none absolute bottom-0 left-1/2 h-[1px] w-0 bg-current -translate-x-1/2 transition-all duration-500 group-hover:w-full">
                        </span>
                    </a>
                </p>
            </div>

        </div>
    )
}
