// src/components/MobileEmojiModal.jsx
import React, { useState, useEffect, useCallback } from 'react';
import { getEmojis } from '../api/GetRooms';
import { combinePath } from '../Tools/tools';
import { BaseURLFiles } from '../api/apiNeed';

// حذف eager: true برای لود تنبل
const emojiModules = import.meta.glob('/src/assets/Emoji-pack/Mobile Emoji/**/*.png');

// کش برای ذخیره ایموجی‌های لود شده
const emojiCache = {};

const MobileEmojiModal = ({ isOpen, onClose, onSelectEmoji, roomid }) => {
  const [activeTab, setActiveTab] = useState('HTC');
  const [emojiList, setEmojiList] = useState([]);
  const [loading, setLoading] = useState(false);

  const tabs = ['Apple', 'HTC', 'LG', 'Samsung'];

  // تابع لود ایموجی‌ها با کش

  const loadEmojis = useCallback(async (brand) => {
    // اگر در کش موجود باشه، مستقیم برگردون
    const emojis = [];
    if (emojiCache[brand]) {
      setEmojiList(emojiCache[brand]);
      return;
    }
    try {
      var em = await getEmojis(brand)
      console.log("emoji loaded")
      if (em) {
        for (const enums in em[brand]) {
      console.log(em[brand][enums])
          let e =em[brand][enums]
          emojis.push({
            name: e.fileName,
            fullPath: `${combinePath(BaseURLFiles, `/img/rooms/icons/${e.path}/${e.fileName}`)}`
          })
        }

      }
    } catch (error) { 

      console.error('Error loading emojis:', error);
      setEmojiList([]);
    }
    finally {
      setLoading(false)
    }
    emojiCache[brand] = emojis;
    setEmojiList(emojis);

  }, []);



  // بارگذاری اولیه و هنگام تغییر تب
  useEffect(() => {
    if (isOpen) {
      loadEmojis(activeTab);
    }
  }, [isOpen, activeTab, loadEmojis]);

  // هندل کلیک روی ایموجی
  const handleEmojiClick = (emojiName) => {
    if (onSelectEmoji) {
      onSelectEmoji(emojiName.split(".")[0], activeTab, roomid);
    }
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-6xl mx-4 overflow-hidden">
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b">
          <h2 className="text-xl font-bold text-gray-800">انتخاب ایموجی موبایل</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b">
          {tabs.map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex-1 py-3 text-center font-medium transition-all duration-200 ${activeTab === tab
                ? 'text-[#30d4cc] border-b-2 border-[#30d4cc]'
                : 'text-gray-500 hover:text-gray-700 hover:bg-gray-50'
                }`}
            >
              {tab}
            </button>
          ))}
        </div>

        {/* Emoji Grid */}
        <div className="p-4 max-h-96 overflow-y-auto">
          {loading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-[#30d4cc]"></div>
              <p className="mt-3 text-gray-500 text-sm">در حال بارگذاری ایموجی‌ها...</p>
            </div>
          ) : emojiList.length === 0 ? (
            <div className="text-center py-12">
              <svg className="w-16 h-16 mx-auto text-gray-300 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-gray-400">هیچ ایموجی در این بخش یافت نشد</p>
            </div>
          ) : (
            <div className="grid grid-cols-4 sm:grid-cols-5 gap-3">
              {emojiList.map((emoji, index) => (
                <button
                  key={`${activeTab}-${emoji.name}-${index}`}
                  onClick={() => handleEmojiClick(emoji.name)}
                  className="group flex flex-col items-center p-3 rounded-lg hover:bg-gradient-to-br hover:from-[#30d4cc]/10 hover:to-[#2bc4bc]/10 transition-all duration-200 transform hover:scale-105"
                >
                  <div className="w-14 h-14 flex items-center justify-center bg-gray-50 rounded-lg group-hover:bg-white transition-colors">
                    <img
                      src={emoji.fullPath}
                      alt={emoji.name}
                      className="w-10 h-10 object-contain"
                      loading="lazy"
                    />
                  </div>
                  <span className="text-xs text-gray-500 mt-2 truncate w-full text-center group-hover:text-[#30d4cc] transition-colors">
                    {emoji.name}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="p-3 border-t bg-gray-50">
          <p className="text-center text-xs text-gray-500">
            برای استفاده، روی ایموجی کلیک کنید
          </p>
        </div>
      </div>
    </div>
  );
};

export default MobileEmojiModal;