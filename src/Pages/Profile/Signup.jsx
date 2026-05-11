import React, { useState } from 'react';
import Cookies from 'js-cookie';
import { useNavigate } from 'react-router-dom';
import { SignUp } from '@/api/StartNewMeeting';

function SignUpForm() {
  const [formData, setFormData] = useState({
    name: '',
    username: '',
    password: '',
    email: '',
    role: '' // اضافه کردن فیلد نقش
  });
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');

    // Validation
    if (formData.password !== confirmPassword) {
      setError('رمز عبور و تکرار آن مطابقت ندارند.');
      return;
    }

    if (formData.password.length < 6) {
      setError('رمز عبور باید حداقل 6 کاراکتر باشد.');
      return;
    }

    if (formData.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      setError('فرمت ایمیل وارد شده صحیح نیست.');
      return;
    }

    if (!formData.role) {
      setError('لطفاً نقش خود را انتخاب کنید.');
      return;
    }

    setIsLoading(true);

    // نقش انتخابی را به متن توضیحی تبدیل می‌کنیم
    const roleExplanation = {
      'lecturer': 'استاد جلسات هستم',
      'request_access': 'درخواست دسترسی ساخت جلسه را دارم!',
      'normal_user': 'کاربر معمولی'
    }[formData.role];

    try {
      const response = await SignUp(
        formData.name,
        formData.username,
        formData.password,
        formData.email,
        roleExplanation // ارسال متن توضیحی نقش به سرور
      );
      var data = response.data;
      if (data.token || data.token !== "wrong") {
        if (response.data) {
          Cookies.set('vc2AuthNorm', response.data.token || data.token, {
            expires: 7,
            secure: process.env.NODE_ENV === 'production',
            sameSite: 'Lax'
          });
        }

        console.log(response);
        console.log('Registration successful!');
        navigate('/');
      } else {
        setError(response.message || 'خطا در ثبت نام.');
      }
    } catch (err) {
      setError(err.message || 'خطا در اتصال به سرور.');
      console.error('SignUp API error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  // گزینه‌های نقش
  const roleOptions = [
    { value: 'lecturer', label: 'استاد جلسات هستم' },
    { value: 'request_access', label: 'درخواست دسترسی ساخت جلسه را دارم!' },
    { value: 'normal_user', label: 'کاربر معمولی' }
  ];

  return (
    <div className="max-w-md mx-auto mt-12 p-6 border border-[#d1d5db] rounded-lg shadow-lg bg-[#ffffff] font-sans" dir="rtl">
      <h2 className="text-2xl font-bold text-center mb-6 text-[#1f2937]">ثبت نام در سامانه</h2>

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="name" className="block mb-2 text-sm font-medium text-[#374151]">
            نام و نام خانوادگی:
          </label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="مثال: علی محمدی"
          />
        </div>

        <div className="mb-4">
          <label htmlFor="username" className="block mb-2 text-sm font-medium text-[#374151]">
            نام کاربری:
          </label>
          <input
            type="text"
            id="username"
            name="username"
            value={formData.username}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="نام کاربری خود را وارد کنید"
          />
        </div>

        <div className="mb-4">
          <label htmlFor="email" className="block mb-2 text-sm font-medium text-[#374151]">
            ایمیل:
          </label>
          <input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="example@gmail.com"
          />
          <small className="text-xs text-[#6b7280] mt-1 block">اختیاری</small>
        </div>

        {/* سلکت لیست نقش کاربری */}
        <div className="mb-4">
          <label htmlFor="role" className="block mb-2 text-sm font-medium text-[#374151]">
            نقش خود را انتخاب کنید:
          </label>
          <select
            id="role"
            name="role"
            value={formData.role}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors bg-[#ffffff]"
          >
            <option value="" disabled>انتخاب کنید...</option>
            {roleOptions.map(option => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
          <small className="text-xs text-[#6b7280] mt-1 block">
            {formData.role === 'lecturer' && '✓ شما می‌توانید جلسات جدید ایجاد و مدیریت کنید'}
            {formData.role === 'request_access' && '✓ درخواست شما برای دسترسی ساخت جلسه بررسی خواهد شد'}
            {formData.role === 'normal_user' && '✓ شما می‌توانید در جلسات شرکت کنید'}
          </small>
        </div>

        <div className="mb-4">
          <label htmlFor="password" className="block mb-2 text-sm font-medium text-[#374151]">
            رمز عبور:
          </label>
          <input
            type="password"
            id="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="حداقل 6 کاراکتر"
          />
          <small className="text-xs text-[#6b7280] mt-1 block">حداقل 6 کاراکتر</small>
        </div>

        <div className="mb-6">
          <label htmlFor="confirmPassword" className="block mb-2 text-sm font-medium text-[#374151]">
            تکرار رمز عبور:
          </label>
          <input
            type="password"
            id="confirmPassword"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="رمز عبور را دوباره وارد کنید"
          />
        </div>

        {error && (
          <div className="mb-4 p-3 bg-[#fef2f2] border border-[#fecaca] rounded-md">
            <p
              className="text-[#dc2626] text-sm text-center"
              dangerouslySetInnerHTML={{ __html: error }}
            />
          </div>
        )}

        <button
          type="submit"
          disabled={isLoading}
          className={`
        w-full py-2.5 px-4 rounded-md text-white font-medium
        transition-all duration-200 transform
        ${isLoading
              ? 'bg-[#9ca3af] cursor-not-allowed'
              : 'bg-[#30d4cc] hover:bg-[#2bbfb8] hover:shadow-lg active:scale-95'
            }
      `}
        >
          {isLoading ? (
            <span className="flex items-center justify-center gap-2">
              <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              در حال ثبت نام...
            </span>
          ) : (
            'ثبت نام'
          )}
        </button>

        <div className="text-center mt-4">
          <button
            type="button"
            onClick={() => navigate('/login')}
            className="text-[#30d4cc] hover:text-[#2bbfb8] text-sm transition-colors"
          >
            قبلاً ثبت نام کرده‌اید؟ وارد شوید
          </button>
        </div>
      </form>
    </div>
  );
}

export default SignUpForm;