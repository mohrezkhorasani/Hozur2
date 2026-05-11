// src/pages/admin/SlideshowList.jsx
import { useState, useEffect, useMemo } from 'react';
import { FaSearch, FaSort, FaPlus, FaTrash, FaLink, FaChevronDown } from 'react-icons/fa';
import { getSlideshows } from '../../api/GetRooms';
import { EditSlideShow } from '../../api/AdminAPI';
import { AddSlideshowModal } from './modal/NewSlider';
// ---------- گزینه‌های مرتب‌سازی ----------
const sortOptions = [
    { value: 100, label: 'جدیدترین', field: 'id', order: 'desc' },
    { value: 101, label: 'قدیمی‌ترین', field: 'id', order: 'asc' },
    { value: 102, label: 'عنوان (ی - الف)', field: 'title', order: 'desc' },
    { value: 103, label: 'عنوان (الف - ی)', field: 'title', order: 'asc' },
];

// ---------- کامپوننت اصلی SlideshowList ----------
const SlideshowList = () => {
    const [allSlides, setAllSlides] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState(100);
    const [currentPage, setCurrentPage] = useState(1);
    const [showAddModal, setShowAddModal] = useState(false);
    const itemsPerPage = 10;

    // دریافت اسلایدشوها
    const fetchSlideshows = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await getSlideshows("admin");
            if (Array.isArray(response)) {
                setAllSlides(response);
            } else if (response && response.status === 200 && Array.isArray(response.data)) {
                setAllSlides(response.data);
            } else {
                setError('خطا در دریافت اطلاعات اسلایدشوها');
            }
        } catch (err) {
            console.error(err);
            setError('مشکل در ارتباط با سرور');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSlideshows();
    }, []);

    // جستجو
    const filteredSlides = useMemo(() => {
        if (!searchTerm.trim()) return allSlides;
        const term = searchTerm.toLowerCase();
        return allSlides.filter(
            slide => slide.title?.toLowerCase().includes(term) ||
                slide.href?.toLowerCase().includes(term) ||
                slide.id?.toString().includes(term)
        );
    }, [allSlides, searchTerm]);

    // مرتب‌سازی
    const sortedSlides = useMemo(() => {
        const option = sortOptions.find(opt => opt.value === sortBy);
        if (!option) return filteredSlides;
        const { field, order } = option;
        return [...filteredSlides].sort((a, b) => {
            let aVal = a[field];
            let bVal = b[field];
            if (typeof aVal === 'string') aVal = aVal.localeCompare(bVal);
            if (typeof bVal === 'string') bVal = bVal.localeCompare(aVal);
            if (order === 'asc') return aVal > bVal ? 1 : -1;
            return aVal < bVal ? 1 : -1;
        });
    }, [filteredSlides, sortBy]);

    const totalSlides = sortedSlides.length;
    const totalPages = Math.ceil(totalSlides / itemsPerPage);
    const paginatedSlides = sortedSlides.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    useEffect(() => {
        setCurrentPage(1);
    }, [searchTerm, sortBy]);

    // تابع ویرایش (برای تغییر پوزیشن یا حذف)
    const handleEditSlide = async (idSlideshow, position, href = null) => {
        try {
            const editData = {
                idSlideshow: idSlideshow,
                position: position,
                ...(href !== null && { href: href })
            };
            
            const result = await EditSlideShow(editData);
            if (result.status === 200) {
                if (position === 0) {
                    setAllSlides(prev => prev.filter(slide => slide.id !== idSlideshow));
                    alert('اسلاید با موفقیت حذف شد');
                } else {
                    setAllSlides(prev => prev.map(slide => 
                        slide.id === idSlideshow ? { ...slide, position: position } : slide
                    ));
                    alert(`پوزیشن اسلاید با موفقیت به ${position} تغییر کرد`);
                }
                await fetchSlideshows();
            } else {
                alert(result.message || 'خطا در ویرایش اسلاید');
            }
        } catch (err) {
            console.error(err);
            alert('خطا در ارتباط با سرور');
        }
    };

    // تابع حذف با تایید
    const handleDeleteWithConfirm = (idSlideshow, title) => {
        if (window.confirm(`آیا از حذف اسلاید "${title}" اطمینان دارید؟`)) {
            handleEditSlide(idSlideshow, 0);
        }
    };

    // تابع تغییر پوزیشن
    const handlePositionChange = (idSlideshow, newPosition) => {
        handleEditSlide(idSlideshow, newPosition);
    };

    // صفحه‌بندی
    const renderPaginationButtons = () => {
        if (totalPages <= 1) return null;
        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, currentPage + 2);
        if (endPage - startPage < 4) {
            if (startPage === 1) endPage = Math.min(totalPages, startPage + 4);
            else startPage = Math.max(1, endPage - 4);
        }
        const pages = [];
        for (let i = startPage; i <= endPage; i++) pages.push(i);
        return (
            <div className="flex gap-1">
                <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1} className="px-3 py-1 border rounded-md disabled:opacity-50">قبلی</button>
                {startPage > 1 && (<><button onClick={() => setCurrentPage(1)} className="px-3 py-1 border rounded-md">1</button>{startPage > 2 && <span className="px-2">...</span>}</>)}
                {pages.map(p => <button key={p} onClick={() => setCurrentPage(p)} className={`px-3 py-1 border rounded-md ${currentPage === p ? 'bg-blue-500 text-white' : ''}`}>{p}</button>)}
                {endPage < totalPages && (<>{endPage < totalPages - 1 && <span className="px-2">...</span>}<button onClick={() => setCurrentPage(totalPages)} className="px-3 py-1 border rounded-md">{totalPages}</button></>)}
                <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages} className="px-3 py-1 border rounded-md disabled:opacity-50">بعدی</button>
            </div>
        );
    };

    if (error) return <div className="bg-red-100 text-red-700 p-4 rounded-lg">{error}</div>;

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden" dir="rtl">
            {/* نوار جستجو و مرتب‌سازی و دکمه افزودن */}
            <div className="p-4 border-b border-gray-200 flex flex-col sm:flex-row gap-4 justify-between">
                <div className="relative flex-1">
                    <input type="text" placeholder="جستجو بر اساس عنوان، لینک یا ID..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)} className="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg" />
                    <FaSearch className="absolute right-3 top-3 text-gray-400" />
                </div>
                <div className="flex items-center gap-2">
                    <FaSort className="text-gray-500" />
                    <select value={sortBy} onChange={e => setSortBy(Number(e.target.value))} className="border border-gray-300 rounded-lg px-3 py-2">
                        {sortOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                    </select>
                </div>
                <button
                    onClick={() => setShowAddModal(true)}
                    className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] hover:opacity-90 text-white px-5 py-2 rounded-lg flex items-center gap-2 transition shadow-sm"
                >
                    <FaPlus /> اسلاید جدید
                </button>
            </div>

            {/* جدول اسلایدشوها */}
            {loading ? (
                <div className="text-center py-12">
                    <div className="w-10 h-10 border-4 border-gray-200 border-t-[#0b3f32] rounded-full animate-spin mx-auto"></div>
                    <p className="text-gray-500 mt-3">در حال بارگذاری اسلایدها...</p>
                </div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">شناسه</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">تصویر</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">عنوان</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">لینک هدف</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">پوزیشن</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">عملیات</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {paginatedSlides.length === 0 ? (
                                <tr>
                                    <td colSpan="6" className="px-6 py-12 text-center text-gray-500">
                                        هیچ اسلایدی یافت نشد
                                    </td>
                                </tr>
                            ) : (
                                paginatedSlides.map(slide => (
                                    <tr key={slide.id} className="hover:bg-gray-50 transition">
                                        <td className="px-6 py-4 text-sm text-gray-900">{slide.id}</td>
                                        <td className="px-6 py-4">
                                            <img src={slide.link} alt={slide.title} className="w-16 h-16 object-cover rounded-md shadow-sm" />
                                        </td>
                                        <td className="px-6 py-4 text-sm font-medium text-gray-900 max-w-xs truncate">{slide.title || 'بدون عنوان'}</td>
                                        <td className="px-6 py-4 text-sm text-blue-600 max-w-xs truncate">
                                            {slide.href ? (
                                                <a href={slide.href} target="_blank" rel="noopener noreferrer" className="flex items-center gap-1 hover:underline">
                                                    <FaLink className="text-xs" /> {slide.href}
                                                </a>
                                            ) : '-'}
                                        </td>
                                        <td className="px-6 py-4 text-sm">
                                            <div className="relative inline-block">
                                                <select
                                                    value={slide.position || 0}
                                                    onChange={(e) => handlePositionChange(slide.id, parseInt(e.target.value))}
                                                    className="appearance-none bg-white border border-gray-300 rounded-md px-3 py-1.5 pr-8 text-sm focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent cursor-pointer"
                                                >
                                                    <option value="0">غیرفعال</option>
                                                    {[1,2,3,4,5,6,7,8,9,10].map(num => (
                                                        <option key={num} value={num}>{num}</option>
                                                    ))}
                                                </select>
                                                <FaChevronDown className="absolute left-2 top-1/2 transform -translate-y-1/2 text-gray-400 pointer-events-none text-xs" />
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-sm">
                                            <button
                                                onClick={() => handleDeleteWithConfirm(slide.id, slide.title)}
                                                className="text-red-600 hover:text-red-800 transition flex items-center gap-1.5 bg-red-50 hover:bg-red-100 px-3 py-1.5 rounded-lg"
                                            >
                                                <FaTrash className="text-sm" /> حذف
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            )}

            {/* صفحه‌بندی */}
            {!loading && totalPages > 1 && (
                <div className="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
                    <div className="text-sm text-gray-700">نمایش {paginatedSlides.length} از {totalSlides} اسلاید</div>
                    {renderPaginationButtons()}
                </div>
            )}

            {/* مودال افزودن اسلاید */}
            <AddSlideshowModal
                isOpen={showAddModal}
                onClose={() => setShowAddModal(false)}
                onSuccess={fetchSlideshows}
            />
        </div>
    );
};

export default SlideshowList;
