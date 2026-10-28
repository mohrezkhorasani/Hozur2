// src/Pages/Admin/Dashboard.jsx
import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { 
  FaUsers, FaVideo, FaCalendarAlt, FaImage, FaPoll, 
  FaCog, FaBook, FaComments, FaChartLine, FaFileAlt,
  FaQuestionCircle, FaTrophy, FaBell, FaEnvelope, 
  FaMoneyBill, FaDatabase, FaShieldAlt, FaPalette,
  FaGlobe, FaMobile, FaSearch, FaThLarge, FaList,
  FaChevronLeft, FaChevronRight, FaPlus, FaEdit,
  FaTrash, FaEye, FaDownload, FaUpload, FaPrint,
  FaUserGraduate, FaChalkboardTeacher, FaRegBuilding,
  FaNewspaper, FaTag, FaTags, FaShoppingCart,
  FaBox, FaTruck, FaWarehouse, FaStore, FaCreditCard,
  FaChartPie, FaChartBar, FaChartLine as FaChartLine2,
  FaRobot, FaCloud, FaLock, FaKey, FaUserShield,
  FaWifi, FaPlug, FaDatabase as FaDatabase2
} from "react-icons/fa";

const AdminDashboard = () => {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [viewMode, setViewMode] = useState("grid"); // grid or list
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 12;

  // Define all menu items
  const menuItems = [
    // مدیریت اصلی
    { id: 1, name: "مدیریت کاربران", icon: <FaUsers />, path: "/admin/users", category: "مدیریت اصلی", color: "from-blue-500 to-blue-600" },
    { id: 2, name: "مدیریت نظرسنجی‌ها", icon: <FaPoll />, path: "/admin/polls", category: "مدیریت اصلی", color: "from-green-500 to-green-600" },
    { id: 3, name: "مدیریت اسلایدشو", icon: <FaImage />, path: "/admin/slideshow", category: "مدیریت اصلی", color: "from-purple-500 to-purple-600" },
    { id: 4, name: "مدیریت اتاق‌ها", icon: <FaVideo />, path: "/admin/rooms", category: "مدیریت اصلی", color: "from-red-500 to-red-600" },
    { id: 5, name: "مدیریت جلسات", icon: <FaCalendarAlt />, path: "/admin/sessions", category: "مدیریت اصلی", color: "from-orange-500 to-orange-600" },
    
    // محتوا
    { id: 6, name: "مدیریت مقالات", icon: <FaNewspaper />, path: "/admin/articles", category: "مدیریت محتوا", color: "from-teal-500 to-teal-600" },
    { id: 7, name: "مدیریت برچسب‌ها", icon: <FaTags />, path: "/admin/tags", category: "مدیریت محتوا", color: "from-cyan-500 to-cyan-600" },
    { id: 8, name: "مدیریت نظرات", icon: <FaComments />, path: "/admin/comments", category: "مدیریت محتوا", color: "from-indigo-500 to-indigo-600" },
    { id: 9, name: "مدیریت فایل‌ها", icon: <FaFileAlt />, path: "/admin/files", category: "مدیریت محتوا", color: "from-pink-500 to-pink-600" },
    { id: 10, name: "مدیریت سوالات متداول", icon: <FaQuestionCircle />, path: "/admin/faq", category: "مدیریت محتوا", color: "from-yellow-500 to-yellow-600" },
    
    // آموزشی
    { id: 11, name: "مدیریت دوره‌ها", icon: <FaBook />, path: "/admin/courses", category: "آموزشی", color: "from-emerald-500 to-emerald-600" },
    { id: 12, name: "مدیریت اساتید", icon: <FaChalkboardTeacher />, path: "/admin/teachers", category: "آموزشی", color: "from-sky-500 to-sky-600" },
    { id: 13, name: "مدیریت دانشجویان", icon: <FaUserGraduate />, path: "/admin/students", category: "آموزشی", color: "from-violet-500 to-violet-600" },
    { id: 14, name: "مدیریت آزمون‌ها", icon: <FaTrophy />, path: "/admin/exams", category: "آموزشی", color: "from-rose-500 to-rose-600" },
    { id: 15, name: "مدیریت گواهینامه‌ها", icon: <FaRegBuilding />, path: "/admin/certificates", category: "آموزشی", color: "from-fuchsia-500 to-fuchsia-600" },
    
    // ارتباطات
    { id: 16, name: "مدیریت پیام‌ها", icon: <FaEnvelope />, path: "/admin/messages", category: "ارتباطات", color: "from-blue-500 to-blue-600" },
    { id: 17, name: "مدیریت نوتیفیکیشن‌ها", icon: <FaBell />, path: "/admin/notifications", category: "ارتباطات", color: "from-green-500 to-green-600" },
    { id: 18, name: "مدیریت خبرنامه", icon: <FaNewspaper />, path: "/admin/newsletter", category: "ارتباطات", color: "from-purple-500 to-purple-600" },
    
    // فروشگاهی
    { id: 19, name: "مدیریت محصولات", icon: <FaShoppingCart />, path: "/admin/products", category: "فروشگاهی", color: "from-red-500 to-red-600" },
    { id: 20, name: "مدیریت سفارشات", icon: <FaBox />, path: "/admin/orders", category: "فروشگاهی", color: "from-orange-500 to-orange-600" },
    { id: 21, name: "مدیریت مشتریان", icon: <FaUsers />, path: "/admin/customers", category: "فروشگاهی", color: "from-teal-500 to-teal-600" },
    { id: 22, name: "مدیریت تخفیف‌ها", icon: <FaTag />, path: "/admin/discounts", category: "فروشگاهی", color: "from-cyan-500 to-cyan-600" },
    { id: 23, name: "مدیریت انبار", icon: <FaWarehouse />, path: "/admin/inventory", category: "فروشگاهی", color: "from-indigo-500 to-indigo-600" },
    { id: 24, name: "مدیریت حمل و نقل", icon: <FaTruck />, path: "/admin/shipping", category: "فروشگاهی", color: "from-pink-500 to-pink-600" },
    
    // گزارشات
    { id: 25, name: "گزارشات کاربران", icon: <FaChartLine />, path: "/admin/reports/users", category: "گزارشات", color: "from-yellow-500 to-yellow-600" },
    { id: 26, name: "گزارشات مالی", icon: <FaMoneyBill />, path: "/admin/reports/financial", category: "گزارشات", color: "from-emerald-500 to-emerald-600" },
    { id: 27, name: "گزارشات فروش", icon: <FaChartBar />, path: "/admin/reports/sales", category: "گزارشات", color: "from-sky-500 to-sky-600" },
    { id: 28, name: "گزارشات بازدید", icon: <FaChartPie />, path: "/admin/reports/visits", category: "گزارشات", color: "from-violet-500 to-violet-600" },
    
    // تنظیمات
    { id: 29, name: "تنظیمات عمومی", icon: <FaCog />, path: "/admin/settings/general", category: "تنظیمات", color: "from-rose-500 to-rose-600" },
    { id: 30, name: "تنظیمات امنیتی", icon: <FaShieldAlt />, path: "/admin/settings/security", category: "تنظیمات", color: "from-fuchsia-500 to-fuchsia-600" },
    { id: 31, name: "تنظیمات ظاهری", icon: <FaPalette />, path: "/admin/settings/appearance", category: "تنظیمات", color: "from-blue-500 to-blue-600" },
    { id: 32, name: "تنظیمات سئو", icon: <FaGlobe />, path: "/admin/settings/seo", category: "تنظیمات", color: "from-green-500 to-green-600" },
    { id: 33, name: "مدیریت API", icon: <FaPlug />, path: "/admin/settings/api", category: "تنظیمات", color: "from-purple-500 to-purple-600" },
    { id: 34, name: "پشتیبان‌گیری", icon: <FaDatabase />, path: "/admin/backup", category: "تنظیمات", color: "from-red-500 to-red-600" },
    
    // ابزارها
    { id: 35, name: "لاگ‌های سیستم", icon: <FaDatabase2 />, path: "/admin/logs", category: "ابزارها", color: "from-orange-500 to-orange-600" },
    { id: 36, name: "مدیریت کش", icon: <FaCloud />, path: "/admin/cache", category: "ابزارها", color: "from-teal-500 to-teal-600" },
    { id: 37, name: "مدیریت Queue", icon: <FaList />, path: "/admin/queue", category: "ابزارها", color: "from-cyan-500 to-cyan-600" },
    { id: 38, name: "ابزارهای SEO", icon: <FaSearch />, path: "/admin/seo-tools", category: "ابزارها", color: "from-indigo-500 to-indigo-600" },
    
    // یکپارچه‌سازی
    { id: 39, name: "اتصال به شبکه‌های اجتماعی", icon: <FaMobile />, path: "/admin/integrations/social", category: "یکپارچه‌سازی", color: "from-pink-500 to-pink-600" },
    { id: 40, name: "درگاه‌های پرداخت", icon: <FaCreditCard />, path: "/admin/integrations/payments", category: "یکپارچه‌سازی", color: "from-yellow-500 to-yellow-600" },
    { id: 41, name: "سیستم‌های خارجی", icon: <FaRobot />, path: "/admin/integrations/external", category: "یکپارچه‌سازی", color: "from-emerald-500 to-emerald-600" },
  ];

  // Group menu items by category
  const groupedItems = menuItems.reduce((acc, item) => {
    if (!acc[item.category]) {
      acc[item.category] = [];
    }
    acc[item.category].push(item);
    return acc;
  }, {});

  // Filter items based on search
  const filterItems = (items) => {
    if (!searchTerm) return items;
    return items.filter(item => 
      item.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      item.category.toLowerCase().includes(searchTerm.toLowerCase())
    );
  };

  // Pagination
  const getPaginatedItems = (items) => {
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    return items.slice(startIndex, endIndex);
  };

  // Get total filtered items count
  const getAllFilteredItems = () => {
    let allItems = [];
    Object.keys(groupedItems).forEach(category => {
      allItems = [...allItems, ...filterItems(groupedItems[category])];
    });
    return allItems;
  };

  const totalFilteredItems = getAllFilteredItems().length;
  const totalPages = Math.ceil(totalFilteredItems / itemsPerPage);

  // Reset page when search changes
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100" dir="rtl">
      <div className="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">داشبورد مدیریت</h1>
          <p className="text-gray-600">به پنل مدیریت خوش آمدید. از طریق بخش‌های زیر می‌توانید محتوا و تنظیمات را مدیریت کنید.</p>
        </div>

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
                <FaEye className="text-purple-600" />
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
              className={`p-2 rounded-lg transition-all ${viewMode === "grid" ? "bg-blue-600 text-white" : "bg-white text-gray-600 hover:bg-gray-100"}`}
            >
              <FaThLarge className="w-5 h-5" />
            </button>
            <button
              onClick={() => setViewMode("list")}
              className={`p-2 rounded-lg transition-all ${viewMode === "list" ? "bg-blue-600 text-white" : "bg-white text-gray-600 hover:bg-gray-100"}`}
            >
              <FaList className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Menu Items - Grouped by Category */}
        {Object.keys(groupedItems).map((category) => {
          const filteredCategoryItems = filterItems(groupedItems[category]);
          if (filteredCategoryItems.length === 0) return null;
          
          const paginatedItems = getPaginatedItems(filteredCategoryItems);
          
          return (
            <div key={category} className="mb-8">
              <h2 className="text-xl font-bold text-gray-800 mb-4 pb-2 border-b-2 border-gray-200">
                {category}
                <span className="text-sm font-normal text-gray-500 mr-2">({filteredCategoryItems.length})</span>
              </h2>
              
              {viewMode === "grid" ? (
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
                      className={`w-full flex items-center justify-between p-4 hover:bg-gray-50 transition-colors ${
                        index !== paginatedItems.length - 1 ? "border-b border-gray-100" : ""
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
              )}
            </div>
          );
        })}

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
                    className={`min-w-[40px] h-10 rounded-lg transition-colors ${
                      currentPage === pageNum
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
      </div>
    </div>
  );
};

export default AdminDashboard;