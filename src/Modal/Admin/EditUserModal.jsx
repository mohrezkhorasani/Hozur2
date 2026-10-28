// src/Modal/Admin/EditUserModal.jsx
import React, { useState, useEffect } from 'react';
import { FaTimes, FaUserEdit, FaCheckCircle, FaUpload, FaImage, FaTrash } from 'react-icons/fa';
import { EditUserInfo } from '../../api/AdminAPI';
import { useToast } from '../../Tools/ToastProvider';

export default function EditUserModal({
    isOpen,
    onClose,
    initialUser = null, // { id, firstname, lastname, isHekmatShobe, isMemberVIP, status, avatarPath, fullname }
    onSuccess = null
}) {
    const toast = useToast();

    const [firstname, setFirstname] = useState('');
    const [lastname, setLastname] = useState('');
    const [isHekmatShobe, setIsHekmatShobe] = useState(false);
    const [isMemberVIP, setIsMemberVIP] = useState(false);
    const [status, setStatus] = useState(100); // 100 = Active, -100 = Disabled, -403 = Banned
    const [avatarFile, setAvatarFile] = useState(null);
    const [previewUrl, setPreviewUrl] = useState('');
    const [isDragging, setIsDragging] = useState(false);

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState('');
    const [successMessage, setSuccessMessage] = useState('');

    useEffect(() => {
        if (isOpen && initialUser) {
            setFirstname(initialUser.firstname || initialUser.Firstname || '');
            setLastname(initialUser.lastname || initialUser.LastName || '');
            setIsHekmatShobe(Boolean(initialUser.isHekmatShobe ?? initialUser.IsHekmatShobe ?? false));
            setIsMemberVIP(Boolean(initialUser.isMemberVIP ?? initialUser.IsMemberVIP ?? initialUser.isVIPMember ?? false));
            
            const userStatus = initialUser.status ?? initialUser.Status ?? (initialUser.isValid === false ? -100 : 100);
            setStatus(Number(userStatus));
            
            const existingAvatar = initialUser.avatarPath || initialUser.AvatarPath || initialUser.avatar || '';
            setPreviewUrl(existingAvatar);
            setAvatarFile(null);
            setError('');
            setSuccessMessage('');
        }
    }, [isOpen, initialUser]);

    if (!isOpen) return null;

    const handleFileSelected = (file) => {
        if (file && file.type.startsWith('image/')) {
            setAvatarFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        } else {
            setError('لطفاً یک فایل تصویری معتبر انتخاب کنید');
        }
    };

    const handleDragOver = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setIsDragging(true);
    };

    const handleDragLeave = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setIsDragging(false);
    };

    const handleDrop = (e) => {
        e.preventDefault();
        e.stopPropagation();
        setIsDragging(false);
        const files = e.dataTransfer.files;
        if (files && files.length > 0) {
            handleFileSelected(files[0]);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!initialUser?.id) {
            setError('شناسه کاربر معتبر نیست');
            return;
        }

        setIsSubmitting(true);
        setError('');
        setSuccessMessage('');

        try {
            const res = await EditUserInfo({
                userId: initialUser.id,
                firstname: firstname.trim(),
                lastname: lastname.trim(),
                isHekmatShobe: isHekmatShobe,
                isMemberVIP: isMemberVIP,
                status: Number(status),
                avatarFile: avatarFile,
                avatarPath: avatarFile ? null : previewUrl
            });

            if (res && (res.status === 200 || res.status === 201)) {
                const msg = res.message || 'اطلاعات کاربر با موفقیت بروزرسانی شد';
                setSuccessMessage(msg);
                if (toast?.success) toast.success(msg);

                const newAvatar = res.data?.avatarPath || (avatarFile ? previewUrl : initialUser?.avatarPath);

                if (onSuccess) {
                    onSuccess({
                        id: initialUser.id,
                        firstname: firstname.trim(),
                        lastname: lastname.trim(),
                        fullname: `${firstname.trim()} ${lastname.trim()}`.trim() || initialUser.username,
                        isHekmatShobe,
                        isMemberVIP,
                        status: Number(status),
                        isValid: Number(status) === 100,
                        avatarPath: newAvatar
                    });
                }

                setTimeout(() => {
                    onClose();
                }, 1000);
            } else {
                setError(res?.message || 'خطا در ویرایش اطلاعات کاربر');
            }
        } catch (err) {
            console.error(err);
            setError('مشکل در ارتباط با سرور');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        /* پس‌زمینه نیمه‌شفاف به جای مشکی کامل */
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4 overflow-y-auto" dir="rtl">
            <div className="bg-white rounded-xl shadow-2xl w-full max-w-lg overflow-hidden border border-gray-100 transform transition-all my-8">
                
                {/* هدر مدال */}
                <div className="flex items-center justify-between px-6 py-4 bg-gradient-to-r from-blue-600 to-indigo-700 text-white">
                    <div className="flex items-center gap-2">
                        <FaUserEdit className="text-xl" />
                        <h3 className="font-bold text-lg">ویرایش اطلاعات کاربر</h3>
                    </div>
                    <button
                        onClick={onClose}
                        className="text-white/80 hover:text-white p-1 rounded-full hover:bg-white/10 transition"
                    >
                        <FaTimes size={18} />
                    </button>
                </div>

                {/* فرم مدال */}
                <form onSubmit={handleSubmit} className="p-6 space-y-4">
                    {error && (
                        <div className="bg-red-50 text-red-700 p-3 rounded-lg text-sm border border-red-200">
                            {error}
                        </div>
                    )}
                    {successMessage && (
                        <div className="bg-green-50 text-green-700 p-3 rounded-lg text-sm border border-green-200 flex items-center gap-2">
                            <FaCheckCircle /> {successMessage}
                        </div>
                    )}

                    {/* شناسه کاربر */}
                    <div className="text-xs text-gray-600 bg-gray-50 p-2.5 rounded-lg border border-gray-200 flex justify-between items-center">
                        <span>شناسه کاربر: <strong className="text-gray-800">{initialUser?.id}</strong></span>
                        {initialUser?.username && (
                            <span>نام کاربری: <strong className="text-gray-800">{initialUser.username}</strong></span>
                        )}
                    </div>

                    {/* نام و نام خانوادگی */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-xs font-semibold text-gray-700 mb-1">نام</label>
                            <input
                                type="text"
                                value={firstname}
                                onChange={(e) => setFirstname(e.target.value)}
                                placeholder="نام کاربر..."
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                            />
                        </div>
                        <div>
                            <label className="block text-xs font-semibold text-gray-700 mb-1">نام خانوادگی</label>
                            <input
                                type="text"
                                value={lastname}
                                onChange={(e) => setLastname(e.target.value)}
                                placeholder="نام خانوادگی کاربر..."
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                            />
                        </div>
                    </div>

                    {/* انتخاب و کشیدن و رها کردن تصویر آواتار (Drag and Drop) */}
                    <div>
                        <label className="block text-xs font-semibold text-gray-700 mb-1">تصویر پروفایل (آواتار)</label>
                        <div
                            onDragOver={handleDragOver}
                            onDragLeave={handleDragLeave}
                            onDrop={handleDrop}
                            className={`border-2 border-dashed rounded-xl p-4 text-center cursor-pointer transition ${
                                isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-blue-400 bg-gray-50/50'
                            }`}
                        >
                            <input
                                type="file"
                                accept="image/*"
                                id="avatar-file-input"
                                className="hidden"
                                onChange={(e) => {
                                    if (e.target.files?.[0]) handleFileSelected(e.target.files[0]);
                                }}
                            />

                            {previewUrl ? (
                                <div className="flex flex-col items-center gap-2">
                                    <div className="relative w-20 h-20 rounded-full overflow-hidden border-2 border-blue-500 shadow-sm">
                                        <img src={previewUrl} alt="Avatar Preview" className="w-full h-full object-cover" />
                                    </div>
                                    <div className="flex items-center gap-2 mt-1">
                                        <label
                                            htmlFor="avatar-file-input"
                                            className="text-xs text-blue-600 hover:underline cursor-pointer font-medium"
                                        >
                                            تغییر تصویر
                                        </label>
                                        <span className="text-gray-300">|</span>
                                        <button
                                            type="button"
                                            onClick={() => {
                                                setAvatarFile(null);
                                                setPreviewUrl('');
                                            }}
                                            className="text-xs text-red-500 hover:underline flex items-center gap-1"
                                        >
                                            <FaTrash size={10} /> حذف تصویر
                                        </button>
                                    </div>
                                </div>
                            ) : (
                                <label htmlFor="avatar-file-input" className="cursor-pointer block">
                                    <FaUpload className="mx-auto text-gray-400 text-2xl mb-1" />
                                    <p className="text-xs font-medium text-gray-700">
                                        تصویر را اینجا بکشید و رها کنید یا <span className="text-blue-600 underline">انتخاب فایل</span>
                                    </p>
                                    <p className="text-[11px] text-gray-400 mt-0.5">فرمت‌های مجاز: PNG, JPG, JPEG</p>
                                </label>
                            )}
                        </div>
                    </div>

                    {/* وضعیت کاربر */}
                    <div>
                        <label className="block text-xs font-semibold text-gray-700 mb-1">وضعیت حساب کاربر (Status)</label>
                        <select
                            value={status}
                            onChange={(e) => setStatus(Number(e.target.value))}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                            <option value={100}>فعال (Active - 100)</option>
                            <option value={-100}>غیرفعال (Disabled - -100)</option>
                            <option value={-400}>حذف شده (Deleted - -400)</option>
                            <option value={-403}>مسدود شده (Banned - -403)</option>
                            <option value={0}>نامشخص (Default - 0)</option>
                        </select>
                    </div>

                    {/* چک‌باکس‌ها */}
                    <div className="bg-blue-50/40 border border-blue-100 p-3.5 rounded-lg space-y-2.5">
                        <label className="flex items-center gap-2 cursor-pointer text-sm font-medium text-gray-800">
                            <input
                                type="checkbox"
                                checked={isHekmatShobe}
                                onChange={(e) => setIsHekmatShobe(e.target.checked)}
                                className="w-4 h-4 text-blue-600 rounded border-gray-300 focus:ring-blue-500"
                            />
                            <span>عضو شعبه حکمت (IsHekmatShobe)</span>
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer text-sm font-medium text-gray-800">
                            <input
                                type="checkbox"
                                checked={isMemberVIP}
                                onChange={(e) => setIsMemberVIP(e.target.checked)}
                                className="w-4 h-4 text-amber-500 rounded border-gray-300 focus:ring-amber-500"
                            />
                            <span className="flex items-center gap-1 text-amber-900">
                                کاربر VIP (IsMemberVIP)
                            </span>
                        </label>
                    </div>

                    {/* دکمه‌ها */}
                    <div className="flex justify-end gap-3 pt-3 border-t border-gray-100">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 text-sm text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
                        >
                            انصراف
                        </button>
                        <button
                            type="submit"
                            disabled={isSubmitting}
                            className="px-5 py-2 text-sm text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition disabled:opacity-50 font-medium"
                        >
                            {isSubmitting ? 'در حال ذخیره...' : 'ذخیره تغییرات'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
