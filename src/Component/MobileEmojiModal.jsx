// src/components/MobileEmojiModal.jsx
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { getEmojis } from '../api/GetRooms';
import { ChangeAvatarOfRoom } from '../api/AdminAPI';
import { combinePath } from '../Tools/tools';
import { BaseURLFiles } from '../api/apiNeed';

// Cache for loaded emojis
const emojiCache = {};

// Helper to resize image client-side to icon dimensions (max 256px) and return data URL
const resizeImageToDataUrl = (file, maxDimension = 256) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const img = new Image();
      img.onload = () => {
        let width = img.width;
        let height = img.height;
        if (width > maxDimension || height > maxDimension) {
          if (width > height) {
            height = Math.round((height * maxDimension) / width);
            width = maxDimension;
          } else {
            width = Math.round((width * maxDimension) / height);
            height = maxDimension;
          }
        }
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0, width, height);
        const isPng = file.type === 'image/png' || file.type === 'image/svg+xml';
        const dataUrl = canvas.toDataURL(isPng ? 'image/png' : 'image/jpeg', 0.88);
        resolve(dataUrl);
      };
      img.onerror = () => reject(new Error('خطا در پردازش تصویر'));
      img.src = e.target.result;
    };
    reader.onerror = () => reject(new Error('خطا در خواندن فایل'));
    reader.readAsDataURL(file);
  });
};

const MobileEmojiModal = ({ isOpen, onClose, onSelectEmoji, roomid }) => {
  const [activeTab, setActiveTab] = useState('HTC');
  const [emojiList, setEmojiList] = useState([]);
  const [loading, setLoading] = useState(false);

  // Custom Image Upload states
  const [customFile, setCustomFile] = useState(null);
  const [customPreview, setCustomPreview] = useState(null);
  const [uploadLoading, setUploadLoading] = useState(false);
  const [uploadError, setUploadError] = useState('');
  const [isDragging, setIsDragging] = useState(false);
  const fileInputRef = useRef(null);

  const tabs = [
    { id: 'custom', label: 'تصویر دلخواه 📤' },
    { id: 'HTC', label: 'HTC' },
    { id: 'Apple', label: 'Apple' },
    { id: 'Samsung', label: 'Samsung' },
    { id: 'LG', label: 'LG' },
    { id: 'icon8', label: 'icon8' }
  ];

  // Function to load emojis with cache
  const loadEmojis = useCallback(async (brand) => {
    if (brand === 'custom') return;

    if (emojiCache[brand]) {
      setEmojiList(emojiCache[brand]);
      return;
    }

    setLoading(true);
    const emojis = [];
    try {
      const em = await getEmojis(brand);
      if (em && em[brand]) {
        for (const enums in em[brand]) {
          const e = em[brand][enums];
          emojis.push({
            name: e.fileName,
            fullPath: `${combinePath(BaseURLFiles, `/img/rooms/icons/${e.path}/${e.fileName}`)}`
          });
        }
      }
    } catch (error) {
      console.error('Error loading emojis:', error);
      setEmojiList([]);
    } finally {
      setLoading(false);
    }
    emojiCache[brand] = emojis;
    setEmojiList(emojis);
  }, []);

  // Initial load or tab change
  useEffect(() => {
    if (isOpen && activeTab !== 'custom') {
      loadEmojis(activeTab);
    }
  }, [isOpen, activeTab, loadEmojis]);

  // Reset custom upload state when modal closes
  useEffect(() => {
    if (!isOpen) {
      setCustomFile(null);
      if (customPreview) {
        URL.revokeObjectURL(customPreview);
      }
      setCustomPreview(null);
      setUploadError('');
      setUploadLoading(false);
    }
  }, [isOpen]);

  // Handle emoji click
  const handleEmojiClick = (emojiName) => {
    if (onSelectEmoji) {
      onSelectEmoji(emojiName.split('.')[0], activeTab, roomid);
    }
    onClose();
  };

  // Handle file selection
  const handleFileChange = (file) => {
    setUploadError('');
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      setUploadError('لطفاً تنها یک فایل تصویری (PNG, JPG, WEBP, SVG) انتخاب فرمایید.');
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      setUploadError('حجم فایل انتخاب شده نباید بیشتر از ۱۰ مگابایت باشد.');
      return;
    }

    setCustomFile(file);
    const previewUrl = URL.createObjectURL(file);
    setCustomPreview(previewUrl);
  };

  // Drag and drop handlers
  const handleDragOver = (e) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFileChange(e.dataTransfer.files[0]);
    }
  };

  // Submit custom image upload
  const handleUploadCustomImage = async () => {
    if (!customFile) {
      setUploadError('لطفاً ابتدا یک تصویر انتخاب نمایید.');
      return;
    }

    if (!roomid) {
      setUploadError('شناسه اتاق مشخص نیست.');
      return;
    }

    setUploadLoading(true);
    setUploadError('');

    try {
      // 1. Resize image client-side to optimal icon dimensions (max 256px) and get Data URL
      const dataUrl = await resizeImageToDataUrl(customFile, 256);

      // 2. Send via JSON to avoid 415 Unsupported Media Type across all servers/proxies
      const res = await ChangeAvatarOfRoom({
        roomID: roomid,
        avatarPath: dataUrl
      });

      if (res && res.status === 200) {
        if (onSelectEmoji) {
          onSelectEmoji(res.avatarPath || dataUrl, 'custom', roomid);
        }
        onClose();
      } else {
        setUploadError(res?.message || 'خطا در بارگذاری تصویر');
      }
    } catch (err) {
      console.error('Error uploading custom room avatar:', err);
      setUploadError(err?.message || 'خطا در برقراری ارتباط با سرور هنگام آپلود تصویر.');
    } finally {
      setUploadLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4" dir="rtl">
      <div className="bg-white rounded-3xl shadow-2xl w-full max-w-4xl overflow-hidden border border-gray-100 flex flex-col max-h-[90vh] animate-in fade-in duration-200">
        
        {/* Header */}
        <div className="flex justify-between items-center p-5 border-b bg-gradient-to-r from-gray-50 to-white">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-2xl bg-[#30d4cc]/15 text-[#005868] flex items-center justify-center text-xl font-bold">
              🎨
            </div>
            <div>
              <h2 className="text-lg font-black text-gray-900">تنظیم آیکون و تصویر تالار</h2>
              <p className="text-xs text-gray-500 mt-0.5">انتخاب از بین ایموجی‌های موبایل یا آپلود عکس دلخواه</p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="w-9 h-9 rounded-full bg-gray-100 hover:bg-gray-200 text-gray-500 hover:text-gray-800 transition-colors flex items-center justify-center"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Tabs Bar */}
        <div className="flex border-b bg-gray-50/70 overflow-x-auto no-scrollbar">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`py-3.5 px-5 text-center font-bold text-xs sm:text-sm whitespace-nowrap transition-all duration-200 flex items-center gap-1.5 border-b-2 ${
                activeTab === tab.id
                  ? 'text-[#005868] border-[#30d4cc] bg-white shadow-xs'
                  : 'text-gray-500 border-transparent hover:text-gray-800 hover:bg-gray-100/60'
              }`}
            >
              <span>{tab.label}</span>
            </button>
          ))}
        </div>

        {/* Content Body */}
        <div className="p-5 overflow-y-auto flex-1">
          {activeTab === 'custom' ? (
            /* ========================================= */
            /* TAB: آپلود تصویر دلخواه                    */
            /* ========================================= */
            <div className="max-w-xl mx-auto py-4 space-y-6">
              <div className="text-center">
                <h3 className="text-base font-black text-gray-900 mb-1">بارگذاری تصویر دلخواه برای آیکون اتاق</h3>
                <p className="text-xs text-gray-500">
                  تصویر انتخابی در کنار عنوان اتاق در صفحه اصلی و جزئیات تالار نمایش داده می‌شود.
                </p>
              </div>

              {/* Upload Dropzone */}
              <div
                onDragOver={handleDragOver}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
                className={`relative border-2 border-dashed rounded-3xl p-6 text-center cursor-pointer transition-all duration-200 flex flex-col items-center justify-center min-h-[220px] ${
                  isDragging
                    ? 'border-[#30d4cc] bg-[#30d4cc]/10 scale-[1.01]'
                    : customPreview
                    ? 'border-emerald-300 bg-emerald-50/40 hover:bg-emerald-50/60'
                    : 'border-gray-300 hover:border-[#30d4cc] bg-gray-50/60 hover:bg-white'
                }`}
              >
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/png, image/jpeg, image/jpg, image/webp, image/svg+xml"
                  onChange={(e) => handleFileChange(e.target.files?.[0])}
                  className="hidden"
                />

                {customPreview ? (
                  <div className="space-y-3 flex flex-col items-center" onClick={(e) => e.stopPropagation()}>
                    <div className="relative group">
                      <div className="w-24 h-24 rounded-2xl overflow-hidden border-2 border-emerald-500 shadow-lg bg-white flex items-center justify-center p-1">
                        <img
                          src={customPreview}
                          alt="پیش‌نمایش آیکون"
                          className="w-full h-full object-contain rounded-xl"
                        />
                      </div>
                      <button
                        type="button"
                        onClick={() => {
                          setCustomFile(null);
                          if (customPreview) URL.revokeObjectURL(customPreview);
                          setCustomPreview(null);
                        }}
                        className="absolute -top-2 -right-2 bg-red-500 hover:bg-red-600 text-white rounded-full p-1.5 shadow-md transition-transform hover:scale-110"
                        title="حذف تصویر"
                      >
                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>

                    <div className="text-center">
                      <p className="text-xs font-bold text-gray-800 truncate max-w-xs">{customFile?.name}</p>
                      <p className="text-[11px] text-gray-500 mt-0.5">
                        {customFile ? `${(customFile.size / 1024).toFixed(1)} کیلوبایت` : ''}
                      </p>
                      <button
                        type="button"
                        onClick={() => fileInputRef.current?.click()}
                        className="text-xs text-[#005868] hover:underline font-bold mt-2"
                      >
                        🔄 انتخاب تصویر دیگر
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-3">
                    <div className="w-16 h-16 rounded-3xl bg-[#30d4cc]/15 text-[#005868] mx-auto flex items-center justify-center text-3xl shadow-xs">
                      📸
                    </div>
                    <div>
                      <p className="font-bold text-sm text-gray-800">
                        برای انتخاب عکس کلیک کنید یا آن را به اینجا بکشید
                      </p>
                      <p className="text-xs text-gray-400 mt-1">
                        پشتیبانی از فرمت‌های PNG، JPG، WEBP، SVG (حداکثر ۱۰ مگابایت)
                      </p>
                    </div>
                  </div>
                )}
              </div>

              {uploadError && (
                <div className="bg-red-50 text-red-600 p-3 rounded-2xl border border-red-200 text-xs font-bold flex items-center gap-2">
                  <span>⚠️</span>
                  <span>{uploadError}</span>
                </div>
              )}

              {/* Action Button */}
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={handleUploadCustomImage}
                  disabled={uploadLoading || !customFile}
                  className="flex-1 py-3.5 px-6 rounded-2xl font-black text-sm text-white bg-gradient-to-r from-[#005868] via-[#097184] to-[#30d4cc] hover:from-[#004754] hover:to-[#26b8b1] shadow-lg shadow-teal-900/20 disabled:opacity-50 transition-all flex items-center justify-center gap-2"
                >
                  {uploadLoading ? (
                    <>
                      <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                      در حال آپلود و ذخیره آیکون...
                    </>
                  ) : (
                    <>
                      <span>🚀</span>
                      <span>آپلود و تنظیم به عنوان آیکون تالار</span>
                    </>
                  )}
                </button>

                <button
                  type="button"
                  onClick={onClose}
                  className="px-5 py-3.5 rounded-2xl border border-gray-200 text-gray-600 font-bold text-xs hover:bg-gray-100 transition-colors"
                >
                  انصراف
                </button>
              </div>
            </div>
          ) : (
            /* ========================================= */
            /* TAB: ایموجی‌های برندها                     */
            /* ========================================= */
            <div>
              {loading ? (
                <div className="flex flex-col items-center justify-center py-16">
                  <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-[#30d4cc]"></div>
                  <p className="mt-3 text-gray-500 text-xs font-bold">در حال بارگذاری ایموجی‌های {activeTab}...</p>
                </div>
              ) : emojiList.length === 0 ? (
                <div className="text-center py-16">
                  <span className="text-4xl block mb-2">🔍</span>
                  <p className="text-gray-500 font-bold text-sm">هیچ ایموجی در این بخش یافت نشد</p>
                </div>
              ) : (
                <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3">
                  {emojiList.map((emoji, index) => (
                    <button
                      key={`${activeTab}-${emoji.name}-${index}`}
                      onClick={() => handleEmojiClick(emoji.name)}
                      className="group flex flex-col items-center p-2.5 rounded-2xl hover:bg-emerald-50 hover:border-emerald-200 border border-transparent transition-all duration-200 transform hover:scale-105"
                      title={emoji.name}
                    >
                      <div className="w-12 h-12 flex items-center justify-center bg-gray-50 rounded-xl group-hover:bg-white group-hover:shadow-md transition-all">
                        <img
                          src={emoji.fullPath}
                          alt={emoji.name}
                          className="w-8 h-8 object-contain"
                          loading="lazy"
                        />
                      </div>
                      <span className="text-[10px] text-gray-500 mt-1.5 truncate w-full text-center group-hover:text-[#005868] font-mono">
                        {emoji.name.split('.')[0]}
                      </span>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-3.5 border-t bg-gray-50 flex items-center justify-between text-xs text-gray-500 px-5">
          <span>
            {activeTab === 'custom'
              ? 'تصویر با هر سایزی به تناسب آیکون اتاق تطبیق داده خواهد شد.'
              : 'برای انتخاب و تنظیم مستقیم، روی ایموجی مورد نظر کلیک کنید.'}
          </span>
          {activeTab !== 'custom' && (
            <button
              onClick={() => setActiveTab('custom')}
              className="text-[#005868] font-bold hover:underline flex items-center gap-1"
            >
              <span>📤</span>
              <span>می‌خواهید عکس دلخواه آپلود کنید؟</span>
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default MobileEmojiModal;
