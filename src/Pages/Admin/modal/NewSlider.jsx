import { useState, useEffect, useMemo, useRef } from 'react';
import { FaSearch, FaSort, FaPlus, FaTrash, FaLink, FaChevronDown, FaTimes, FaUpload, FaImage, FaVideo } from 'react-icons/fa';
import { EditSlideShow, InsertSlideShow } from "../../../api/AdminAPI";
import { getSlideshows } from '../../../api/GetRooms';
// ---------- کامپوننت مودال افزودن اسلاید ----------
export const AddSlideshowModal = ({ isOpen, onClose, onSuccess }) => {
    const [formData, setFormData] = useState({
        title: '',
        href: '',
        position: 1,
    });
    const [selectedFile, setSelectedFile] = useState(null);
    const [preview, setPreview] = useState(null);
    const [uploading, setUploading] = useState(false);
    const [error, setError] = useState('');
    const fileInputRef = useRef(null);

    // ریست فرم هنگام بسته شدن
    useEffect(() => {
        if (!isOpen) {
            setFormData({ title: '', href: '', position: 1 });
            setSelectedFile(null);
            setPreview(null);
            setError('');
        }
    }, [isOpen]);

    const handleFileChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            // اعتبارسنجی نوع فایل
            const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
            if (!validTypes.includes(file.type)) {
                setError('فرمت فایل مجاز نیست. فقط تصاویر (jpg, jpeg, png, gif, webp) قابل قبول هستند');
                setSelectedFile(null);
                setPreview(null);
                return;
            }
            
            // اعتبارسنجی حجم فایل (حداکثر 5 مگابایت)
            if (file.size > 5 * 1024 * 1024) {
                setError('حجم فایل نباید بیشتر از 5 مگابایت باشد');
                setSelectedFile(null);
                setPreview(null);
                return;
            }
            
            setSelectedFile(file);
            setError('');
            
            // ایجاد preview
            const reader = new FileReader();
            reader.onloadend = () => {
                setPreview(reader.result);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        // اعتبارسنجی
        if (!selectedFile) {
            setError('لطفاً یک فایل تصویری انتخاب کنید');
            return;
        }
        
        if (!formData.title.trim()) {
            setError('لطفاً عنوان اسلاید را وارد کنید');
            return;
        }
        
        setUploading(true);
        setError('');
        
        try {
            // ساخت FormData برای ارسال فایل
            const submitData = new FormData();
            submitData.append('Position', formData.position.toString());
            submitData.append('Title', formData.title);
            submitData.append('Href', formData.href || '');
            submitData.append('File', selectedFile);
            
            const result = await InsertSlideShow(submitData);
            
            if (result.status === 200) {
                alert('اسلاید با موفقیت اضافه شد');
                onSuccess(); // رفرش لیست
                onClose(); // بستن مودال
            } else {
                setError(result.message || 'خطا در افزودن اسلاید');
            }
        } catch (err) {
            console.error(err);
            setError(err.message || 'خطا در ارتباط با سرور');
        } finally {
            setUploading(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-fadeIn">
            <div className="relative w-full max-w-lg bg-white rounded-2xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
                {/* Header */}
                <div className="bg-gradient-to-r from-[#0b3f32] to-[#0a352a] px-6 py-4 flex justify-between items-center">
                    <h3 className="text-white text-lg font-bold flex items-center gap-2">
                        <FaPlus className="text-[#30d4cc]" />
                        افزودن اسلاید جدید
                    </h3>
                    <button onClick={onClose} className="text-white/70 hover:text-white transition">
                        <FaTimes className="text-xl" />
                    </button>
                </div>

                {/* Body */}
                <div className="flex-1 overflow-y-auto p-6">
                    <form onSubmit={handleSubmit} className="space-y-5">
                        {/* پیش‌نمایش فایل */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                تصویر اسلاید <span className="text-red-500">*</span>
                            </label>
                            <div 
                                onClick={() => fileInputRef.current?.click()}
                                className={`border-2 border-dashed rounded-xl p-6 text-center cursor-pointer transition-all
                                    ${preview ? 'border-[#30d4cc] bg-[#f0fdfa]' : 'border-gray-300 hover:border-[#30d4cc] hover:bg-gray-50'}`}
                            >
                                <input
                                    ref={fileInputRef}
                                    type="file"
                                    accept="image/*"
                                    onChange={handleFileChange}
                                    className="hidden"
                                />
                                {preview ? (
                                    <div className="space-y-3">
                                        <img src={preview} alt="پیش‌نمایش" className="max-w-full h-40 object-contain rounded-lg mx-auto" />
                                        <p className="text-sm text-gray-500">برای تغییر کلیک کنید</p>
                                    </div>
                                ) : (
                                    <div className="space-y-2">
                                        <FaUpload className="text-4xl text-gray-400 mx-auto" />
                                        <p className="text-gray-600">برای آپلود تصویر کلیک کنید</p>
                                        <p className="text-xs text-gray-400">jpg, jpeg, png, gif, webp (حداکثر 5 مگابایت)</p>
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* عنوان */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                عنوان <span className="text-red-500">*</span>
                            </label>
                            <input
                                type="text"
                                value={formData.title}
                                onChange={e => setFormData(prev => ({ ...prev, title: e.target.value }))}
                                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent"
                                placeholder="مثال: همایش ملی حکمت اسلامی"
                                required
                            />
                        </div>

                        {/* لینک هدف */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                لینک هدف (اختیاری)
                            </label>
                            <input
                                type="text"
                                value={formData.href}
                                onChange={e => setFormData(prev => ({ ...prev, href: e.target.value }))}
                                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent"
                                placeholder="/rooms/xxx/join یا https://example.com"
                            />
                            <p className="text-xs text-gray-400 mt-1">در صورت کلیک روی اسلاید، کاربر به این لینک هدایت می‌شود</p>
                        </div>

                        {/* پوزیشن */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                اولویت نمایش
                            </label>
                            <select
                                value={formData.position}
                                onChange={e => setFormData(prev => ({ ...prev, position: parseInt(e.target.value) }))}
                                className="w-full border border-gray-300 rounded-lg px-3 py-2 focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent"
                            >
                                <option value="1">1 - بالاترین اولویت</option>
                                {[2,3,4,5,6,7,8,9,10].map(num => (
                                    <option key={num} value={num}>{num}</option>
                                ))}
                            </select>
                            <p className="text-xs text-gray-400 mt-1">اعداد کوچکتر در اولویت بالاتری نمایش داده می‌شوند</p>
                        </div>

                        {/* خطا */}
                        {error && (
                            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg text-sm">
                                {error}
                            </div>
                        )}

                        {/* دکمه‌ها */}
                        <div className="flex gap-3 pt-2">
                            <button
                                type="button"
                                onClick={onClose}
                                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition"
                            >
                                انصراف
                            </button>
                            <button
                                type="submit"
                                disabled={uploading}
                                className="flex-1 px-4 py-2 bg-gradient-to-r from-[#0b3f32] to-[#0a352a] text-white rounded-lg hover:opacity-90 transition disabled:opacity-50 flex items-center justify-center gap-2"
                            >
                                {uploading ? (
                                    <>
                                        <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                        در حال آپلود...
                                    </>
                                ) : (
                                    'افزودن اسلاید'
                                )}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

