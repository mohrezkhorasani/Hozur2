// LoginForm.jsx with Tailwind
import React, { useState } from 'react';
import Cookies from 'js-cookie';
import { useNavigate } from 'react-router-dom';
import { Login } from '@/api/StartNewMeeting';

function LoginForm() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      const response = await Login(username, password);
      if (response.data.token != null) {
        Cookies.set('vc2AuthNorm', response.data.token, {
          expires: 7,
          secure: process.env.NODE_ENV === 'production',
          sameSite: 'Lax'
        });

        console.log('Login successful!');
        navigate('/');
        window.location.href = "/";
      } else {
        setError(response.message || 'خطا در ورود.');
      }
    } catch (err) {
      setError(err.message || 'خطا در اتصال به سرور.');
      console.error('Login API error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-12 p-6 border border-[#d1d5db] rounded-lg shadow-lg bg-[#ffffff] font-sans" dir="rtl">
      <h2 className="text-2xl font-bold text-center mb-6 text-[#1f2937]">ورود به حساب کاربری</h2>

      <form onSubmit={handleSubmit}>
        <div className="mb-4">
          <label htmlFor="username" className="block mb-2 text-sm font-medium text-[#374151]">
            نام کاربری:
          </label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="نام کاربری خود را وارد کنید"
          />
        </div>

        <div className="mb-6">
          <label htmlFor="password" className="block mb-2 text-sm font-medium text-[#374151]">
            رمز عبور:
          </label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            className="w-full px-3 py-2 border border-[#d1d5db] rounded-md focus:outline-none focus:ring-2 focus:ring-[#30d4cc] focus:border-[#30d4cc] transition-colors"
            placeholder="رمز عبور خود را وارد کنید"
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
              در حال ورود...
            </span>
          ) : (
            'ورود'
          )}
        </button>

        <div className="text-center mt-4">
          <button
            type="button"
            onClick={() => navigate('/signup')}
            className="text-[#30d4cc] hover:text-[#2bbfb8] text-sm transition-colors"
          >
            حساب کاربری ندارید؟ ثبت نام کنید
          </button>
        </div>
      </form>
    </div>
  );
}

export default LoginForm;