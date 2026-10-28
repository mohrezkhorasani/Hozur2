// src/Layouts/AdminLayout.jsx
import React, { useState, useEffect } from "react";
import { Outlet, useNavigate, useLocation } from "react-router-dom";
import {
  FaUsers, FaVideo, FaCalendarAlt, FaImage, FaPoll,
  FaCog, FaBook, FaComments, FaChartLine, FaFileAlt,
  FaQuestionCircle, FaTrophy, FaBell, FaEnvelope,
  FaMoneyBill, FaDatabase, FaShieldAlt, FaPalette,
  FaGlobe, FaMobile, FaSearch, FaThLarge, FaList,
  FaChevronLeft, FaChevronRight, FaHome, FaArrowRight,
  FaTags,
  FaChalkboardTeacher,
  FaUserGraduate,
  FaNewspaper,
  FaShoppingCart,
  FaBox,
  FaChartBar,
  FaCloud,
  FaLink
} from "react-icons/fa";

const AdminLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchTerm, setSearchTerm] = useState("");
  const [viewMode, setViewMode] = useState("grid");
  const [currentPage, setCurrentPage] = useState(1);
  const [selectedCategory, setSelectedCategory] = useState("all");
  const itemsPerPage = 8;

  // Define all menu items
  const menuItems = [
    { id: 1, name: "مدیریت کاربران", icon: <FaUsers />, path: "/adminlay/users", category: "مدیریت اصلی", color: "from-blue-500 to-blue-600" },
    { id: 27, name: "مدیریت هوک‌ها", icon: <FaLink />, path: "/adminlay/webhooks", category: "مدیریت اصلی", color: "from-indigo-500 to-indigo-600" },
    { id: 2, name: "مدیریت نظرسنجی‌ها", icon: <FaPoll />, path: "/adminlay/polls", category: "مدیریت اصلی", color: "from-green-500 to-green-600" },
    { id: 3, name: "مدیریت اسلایدشو", icon: <FaImage />, path: "/adminlay/slideshow", category: "مدیریت اصلی", color: "from-purple-500 to-purple-600" },
    { id: 4, name: "مدیریت اتاق‌ها", icon: <FaVideo />, path: "/adminlay/rooms", category: "مدیریت اصلی", color: "from-red-500 to-red-600" },
    { id: 5, name: "مدیریت جلسات", icon: <FaCalendarAlt />, path: "/adminlay/meetings", category: "مدیریت اصلی", color: "from-orange-500 to-orange-600" },
    { id: 28, name: "شرکت‌کنندگان جلسات", icon: <FaUsers />, path: "/adminlay/participants", category: "مدیریت اصلی", color: "from-teal-500 to-teal-600" },
    { id: 6, name: "مدیریت مقالات", icon: <FaFileAlt />, path: "/adminlay/articles", category: "مدیریت محتوا", color: "from-teal-500 to-teal-600" },
    { id: 7, name: "مدیریت نظرات", icon: <FaComments />, path: "/adminlay/comments", category: "مدیریت محتوا", color: "from-indigo-500 to-indigo-600" },
    { id: 8, name: "مدیریت برچسب‌ها", icon: <FaTags />, path: "/adminlay/tags", category: "مدیریت محتوا", color: "from-cyan-500 to-cyan-600" },
    { id: 9, name: "مدیریت سوالات متداول", icon: <FaQuestionCircle />, path: "/adminlay/faq", category: "مدیریت محتوا", color: "from-yellow-500 to-yellow-600" },
    { id: 10, name: "مدیریت دوره‌ها", icon: <FaBook />, path: "/adminlay/courses", category: "آموزشی", color: "from-emerald-500 to-emerald-600" },
    { id: 11, name: "مدیریت اساتید", icon: <FaChalkboardTeacher />, path: "/adminlay/teachers", category: "آموزشی", color: "from-sky-500 to-sky-600" },
    { id: 12, name: "مدیریت دانشجویان", icon: <FaUserGraduate />, path: "/adminlay/students", category: "آموزشی", color: "from-violet-500 to-violet-600" },
    { id: 13, name: "مدیریت پیام‌ها", icon: <FaEnvelope />, path: "/adminlay/messages", category: "ارتباطات", color: "from-blue-500 to-blue-600" },
    { id: 14, name: "مدیریت نوتیفیکیشن‌ها", icon: <FaBell />, path: "/adminlay/notifications", category: "ارتباطات", color: "from-green-500 to-green-600" },
    { id: 15, name: "مدیریت خبرنامه", icon: <FaNewspaper />, path: "/adminlay/newsletter", category: "ارتباطات", color: "from-purple-500 to-purple-600" },
    { id: 16, name: "مدیریت محصولات", icon: <FaShoppingCart />, path: "/adminlay/products", category: "فروشگاهی", color: "from-red-500 to-red-600" },
    { id: 17, name: "مدیریت سفارشات", icon: <FaBox />, path: "/adminlay/orders", category: "فروشگاهی", color: "from-orange-500 to-orange-600" },
    { id: 18, name: "گزارشات کاربران", icon: <FaChartLine />, path: "/adminlay/reports/users", category: "گزارشات", color: "from-yellow-500 to-yellow-600" },
    { id: 19, name: "گزارشات مالی", icon: <FaMoneyBill />, path: "/adminlay/reports/financial", category: "گزارشات", color: "from-emerald-500 to-emerald-600" },
    { id: 20, name: "گزارشات فروش", icon: <FaChartBar />, path: "/adminlay/reports/sales", category: "گزارشات", color: "from-sky-500 to-sky-600" },
    { id: 21, name: "تنظیمات عمومی", icon: <FaCog />, path: "/adminlay/settings/general", category: "تنظیمات", color: "from-rose-500 to-rose-600" },
    { id: 22, name: "تنظیمات امنیتی", icon: <FaShieldAlt />, path: "/adminlay/settings/security", category: "تنظیمات", color: "from-fuchsia-500 to-fuchsia-600" },
    { id: 23, name: "تنظیمات ظاهری", icon: <FaPalette />, path: "/adminlay/settings/appearance", category: "تنظیمات", color: "from-blue-500 to-blue-600" },
    { id: 24, name: "پشتیبان‌گیری", icon: <FaDatabase />, path: "/adminlay/backup", category: "تنظیمات", color: "from-red-500 to-red-600" },
    { id: 25, name: "لاگ‌های سیستم", icon: <FaDatabase />, path: "/adminlay/logs", category: "ابزارها", color: "from-orange-500 to-orange-600" },
    { id: 26, name: "مدیریت کش", icon: <FaCloud />, path: "/adminlay/cache", category: "ابزارها", color: "from-teal-500 to-teal-600" },
  ];

  // Get unique categories
  const categories = ["all", ...new Set(menuItems.map(item => item.category))];

  // Group menu items by category
  const groupedItems = menuItems.reduce((acc, item) => {
    if (!acc[item.category]) {
      acc[item.category] = [];
    }
    acc[item.category].push(item);
    return acc;
  }, {});

  // Filter items based on search and category
  const filterItems = (items) => {
    let filtered = items;
    if (searchTerm) {
      filtered = filtered.filter(item =>
        item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.category.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
    return filtered;
  };

  const getFilteredItemsForCategory = (category) => {
    if (category === "all") {
      let allItems = [];
      Object.keys(groupedItems).forEach(cat => {
        allItems = [...allItems, ...filterItems(groupedItems[cat])];
      });
      return allItems;
    }
    return filterItems(groupedItems[category] || []);
  };

  const currentItems = getFilteredItemsForCategory(selectedCategory);
  const totalPages = Math.ceil(currentItems.length / itemsPerPage);
  const paginatedItems = currentItems.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

  // Reset page when filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectedCategory]);

  // Check if we're on a child route
  const isChildRoute = location.pathname !== "/adminlay";

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100" dir="rtl">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">

        {/* Header with Back Button */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-800 mb-2">داشبورد مدیریت</h1>
              <p className="text-gray-600">
                {isChildRoute ? "مدیریت بخش مورد نظر" : "از طریق بخش‌های زیر می‌توانید محتوا و تنظیمات را مدیریت کنید."}
              </p>
            </div>
            {isChildRoute && (
              <button
                onClick={() => navigate("/adminlay")}
                className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50 transition-all"
              >
                <FaArrowRight className="w-4 h-4" />
                <span>بازگشت به داشبورد</span>
              </button>
            )}
          </div>
        </div>

        {/* Show menu grid only on main admin page */}
        {!isChildRoute ? (
          <>
            {/* Stats Overview */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
              <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-500 text-sm">تعداد کل بخش‌ها</p>
                    <p className="text-2xl font-bold text-gray-800">{menuItems.length}</p>
                  </div>
                  <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                    <FaThLarge className="text-blue-600" />
                  </div>
                </div>
              </div>
              <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-500 text-sm">دسته‌بندی‌ها</p>
                    <p className="text-2xl font-bold text-gray-800">{Object.keys(groupedItems).length}</p>
                  </div>
                  <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
                    <FaList className="text-green-600" />
                  </div>
                </div>
              </div>
              <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-500 text-sm">فعال امروز</p>
                    <p className="text-2xl font-bold text-gray-800">24</p>
                  </div>
                  <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                    <FaHome className="text-purple-600" />
                  </div>
                </div>
              </div>
              <div className="bg-white rounded-xl shadow-sm p-4 border border-gray-100">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-gray-500 text-sm">آخرین بروزرسانی</p>
                    <p className="text-sm font-bold text-gray-800">امروز</p>
                  </div>
                  <div className="w-10 h-10 bg-orange-100 rounded-lg flex items-center justify-center">
                    <FaCog className="text-orange-600" />
                  </div>
                </div>
              </div>
            </div>

            {/* Category Tabs */}
            <div className="mb-6 overflow-x-auto">
              <div className="flex gap-2 pb-2">
                {categories.map((category) => (
                  <button
                    key={category}
                    onClick={() => setSelectedCategory(category)}
                    className={`px-4 py-2 rounded-lg whitespace-nowrap transition-all ${selectedCategory === category
                        ? "bg-blue-600 text-white shadow-md"
                        : "bg-white text-gray-600 hover:bg-gray-50 border border-gray-200"
                      }`}
                  >
                    {category === "all" ? "همه" : category}
                    <span className="mr-1 text-xs">
                      ({category === "all" ? menuItems.length : groupedItems[category]?.length || 0})
                    </span>
                  </button>
                ))}
              </div>
            </div>

            {/* Search and View Controls */}
            <div className="mb-6 flex flex-col sm:flex-row gap-4 justify-between items-center">
              <div className="relative w-full sm:w-96">
                <FaSearch className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="جستجو در بخش‌ها..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pr-10 pl-4 py-2.5 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white"
                />
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => setViewMode("grid")}
                  className={`p-2 rounded-lg transition-all ${viewMode === "grid" ? "bg-blue-600 text-white" : "bg-white text-gray-600 hover:bg-gray-100"
                    }`}
                >
                  <FaThLarge className="w-5 h-5" />
                </button>
                <button
                  onClick={() => setViewMode("list")}
                  className={`p-2 rounded-lg transition-all ${viewMode === "list" ? "bg-blue-600 text-white" : "bg-white text-gray-600 hover:bg-gray-100"
                    }`}
                >
                  <FaList className="w-5 h-5" />
                </button>
              </div>
            </div>

            {/* Menu Items Grid/List */}
            {paginatedItems.length > 0 ? (
              viewMode === "grid" ? (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                  {paginatedItems.map((item) => (
                    <button
                      key={item.id}
                      onClick={() => navigate(item.path)}
                      className="group bg-white rounded-xl shadow-sm hover:shadow-xl transition-all duration-300 p-5 text-right border border-gray-100 hover:border-transparent transform hover:-translate-y-1"
                    >
                      <div className="flex items-center gap-3 mb-3">
                        <div className={`w-12 h-12 bg-gradient-to-r ${item.color} rounded-lg flex items-center justify-center text-white text-xl group-hover:scale-110 transition-transform duration-300`}>
                          {item.icon}
                        </div>
                        <h3 className="font-bold text-gray-800 text-base">{item.name}</h3>
                      </div>
                      <div className="flex justify-between items-center text-xs text-gray-400">
                        <span>مدیریت</span>
                        <FaChevronLeft className="w-3 h-3 group-hover:translate-x-1 transition-transform" />
                      </div>
                    </button>
                  ))}
                </div>
              ) : (
                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                  {paginatedItems.map((item, index) => (
                    <button
                      key={item.id}
                      onClick={() => navigate(item.path)}
                      className={`w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors ${index !== paginatedItems.length - 1 ? "border-b border-gray-100" : ""
                        }`}
                    >
                      <div className="flex items-center gap-3">
                        <div className={`w-10 h-10 bg-gradient-to-r ${item.color} rounded-lg flex items-center justify-center text-white`}>
                          {item.icon}
                        </div>
                        <span className="text-gray-800 font-medium">{item.name}</span>
                      </div>
                      <FaChevronLeft className="w-4 h-4 text-gray-400" />
                    </button>
                  ))}
                </div>
              )
            ) : (
              <div className="text-center py-12 bg-white rounded-xl">
                <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <FaSearch className="w-10 h-10 text-gray-400" />
                </div>
                <p className="text-gray-600 text-lg">نتیجه‌ای یافت نشد</p>
                <p className="text-gray-400 text-sm mt-2">لطفاً عبارت جستجو را تغییر دهید</p>
              </div>
            )}

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex justify-center items-center gap-2 mt-8">
                <button
                  onClick={() => setCurrentPage(prev => Math.max(1, prev - 1))}
                  disabled={currentPage === 1}
                  className="p-2 rounded-lg border border-gray-200 bg-white text-gray-600 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                >
                  <FaChevronRight className="w-5 h-5" />
                </button>

                <div className="flex gap-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (currentPage <= 3) {
                      pageNum = i + 1;
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = currentPage - 2 + i;
                    }

                    return (
                      <button
                        key={pageNum}
                        onClick={() => setCurrentPage(pageNum)}
                        className={`min-w-[40px] h-10 rounded-lg transition-colors ${currentPage === pageNum
                            ? "bg-blue-600 text-white"
                            : "border border-gray-200 bg-white text-gray-600 hover:bg-gray-50"
                          }`}
                      >
                        {pageNum}
                      </button>
                    );
                  })}
                </div>

                <button
                  onClick={() => setCurrentPage(prev => Math.min(totalPages, prev + 1))}
                  disabled={currentPage === totalPages}
                  className="p-2 rounded-lg border border-gray-200 bg-white text-gray-600 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50 transition-colors"
                >
                  <FaChevronLeft className="w-5 h-5" />
                </button>
              </div>
            )}
          </>
        ) : (
          /* Render child routes */
          <div className="bg-white rounded-2xl shadow-sm">
            <Outlet />
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminLayout;