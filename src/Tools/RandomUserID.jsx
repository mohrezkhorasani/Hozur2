// src/components/RandomUserId.jsx
import React, { useState, useEffect } from 'react';

const RandomUserId = ({ onUserIdGenerated, prefix = "user_" }) => {
  const [userId, setUserId] = useState('');
  const [timestamp, setTimestamp] = useState(null);

  // روش‌های مختلف تولید ID یکتا
  const generateRandomId = () => {
    const methods = {
      // روش 1: UUID v4
      uuid: () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          const r = Math.random() * 16 | 0;
          const v = c === 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
      },
      
      // روش 2: ترکیب timestamp + random
      timestamp: () => {
        return `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
      },
      
      // روش 3: NanoID style (11 کاراکتر)
      nanoid: () => {
        const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        let result = '';
        for (let i = 0; i < 11; i++) {
          result += chars.charAt(Math.floor(Math.random() * chars.length));
        }
        return result;
      },
      
      // روش 4: ترکیب高性能
     高性能: () => {
        return `${Date.now()}_${Math.random().toString(36).substr(2, 9)}_${performance.now().toString(36).substr(2, 5)}`;
      },
      
      // روش 5: GUID با تاریخ
      guid: () => {
        return `${Date.now().toString(36)}-${Math.random().toString(36).substr(2, 9)}-${Math.random().toString(36).substr(2, 9)}`;
      }
    };
    
    return methods.高性能();
  };
  
  // روش با Crypto API (امن‌تر)
  const generateCryptoId = () => {
    if (window.crypto && window.crypto.randomUUID) {
      return window.crypto.randomUUID();
    }
    // fallback
    return generateRandomId();
  };
  
  // روش ترکیبی با استفاده از localStorage برای جلوگیری از تکرار
  const generateUniqueId = () => {
    const existingIds = JSON.parse(localStorage.getItem('generated_user_ids') || '[]');
    let newId;
    let attempts = 0;
    const maxAttempts = 10;
    
    do {
      newId = `${prefix}${generateCryptoId()}`;
      attempts++;
      if (attempts > maxAttempts) {
        // اگر بعد از 10 تلاش still تکراری بود، timestamp اضافه کن
        newId = `${prefix}${generateCryptoId()}_${Date.now()}`;
        break;
      }
    } while (existingIds.includes(newId));
    
    // ذخیره در localStorage برای جلوگیری از تکرار در جلسات آینده
    existingIds.push(newId);
    localStorage.setItem('generated_user_ids', JSON.stringify(existingIds.slice(-100))); // فقط 100 تای آخر را نگه دار
    
    return newId;
  };
  
  useEffect(() => {
    const newUserId = generateUniqueId();
    setUserId(newUserId);
    setTimestamp(new Date());
    if (onUserIdGenerated) {
      onUserIdGenerated(newUserId);
    }
  }, []);
  
  const regenerateId = () => {
    const newUserId = generateUniqueId();
    setUserId(newUserId);
    setTimestamp(new Date());
    if (onUserIdGenerated) {
      onUserIdGenerated(newUserId);
    }
  };
  
  return (
    <div className="p-4 bg-gray-100 rounded-lg">
      <div className="mb-3">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          User ID تولید شده:
        </label>
        <div className="flex items-center gap-2">
          <code className="flex-1 px-3 py-2 bg-white border border-gray-300 rounded-md text-sm font-mono">
            {userId || 'در حال تولید...'}
          </code>
          <button
            onClick={regenerateId}
            className="px-3 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
          >
            تولید مجدد
          </button>
        </div>
      </div>
      
      {timestamp && (
        <p className="text-xs text-gray-500">
          تولید شده در: {timestamp.toLocaleTimeString('fa-IR')}
        </p>
      )}
      
      <div className="mt-3 text-xs text-gray-500">
        <p>احتمال تکرار: بسیار کم (∼ 1 در 10^36)</p>
      </div>
    </div>
  );
};

// هوک برای استفاده در هر کامپوننتی
export const useRandomUserId = (prefix = "user_") => {
  const [userId, setUserId] = useState(null);
  
  useEffect(() => {
    const generate = () => {
      const id = `${prefix}${crypto.randomUUID?.() || 
        `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`}`;
      setUserId(id);
    };
    generate();
  }, [prefix]);
  
  return userId;
};

// تابع ساده برای استفاده در هر جایی
export const generateSimpleUserId = (prefix = "") => {
  if (crypto.randomUUID) {
    return `${prefix}${crypto.randomUUID()}`;
  }
  return `${prefix}${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
};

export default RandomUserId;