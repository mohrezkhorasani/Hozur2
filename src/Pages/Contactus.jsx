// Pages/ContactPage.jsx
import { useState } from 'react';
import { FaUser, FaEnvelope, FaPhone, FaComment, FaPaperPlane, FaCheckCircle, FaArrowRight } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';

const ContactPage = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    fullname: '',
    email: '',
    phone: '',
    message: ''
  });
  
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: '' }));
    }
  };

  const validateForm = () => {
    const newErrors = {};
    
    if (!formData.fullname.trim()) {
      newErrors.fullname = 'نام و نام خانوادگی الزامی است';
    } else if (formData.fullname.length < 3) {
      newErrors.fullname = 'نام و نام خانوادگی باید حداقل 3 کاراکتر باشد';
    }
    
    if (!formData.email.trim()) {
      newErrors.email = 'ایمیل الزامی است';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      newErrors.email = 'ایمیل معتبر نیست';
    }
    
    if (formData.phone && !/^09[0-9]{9}$/.test(formData.phone)) {
      newErrors.phone = 'شماره موبایل معتبر نیست';
    }
    
    if (!formData.message.trim()) {
      newErrors.message = 'متن پیام الزامی است';
    } else if (formData.message.length < 10) {
      newErrors.message = 'پیام باید حداقل 10 کاراکتر باشد';
    }
    
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    const validationErrors = validateForm();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    
    setIsSubmitting(true);
    
    // API endpoint placeholder - replace with your actual API
    try {
      // const response = await fetch('/api/contact', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify(formData)
      // });
      
      // Simulate API call (remove this in production)
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      setIsSuccess(true);
      setFormData({ fullname: '', email: '', phone: '', message: '' });
      
      // Redirect after success
      setTimeout(() => {
        navigate('/');
      }, 3000);
    } catch (error) {
      console.error('Error sending message:', error);
      setErrors({ submit: 'خطا در ارسال پیام. لطفا مجددا تلاش کنید.' });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div dir="rtl" className="min-h-screen bg-gradient-to-br from-[#0b3f32] to-[#06261e] flex items-center justify-center p-4">
      {/* Back Button */}
      <button
        onClick={() => navigate(-1)}
        className="fixed top-6 right-6 z-10 flex items-center gap-2 bg-white/10 hover:bg-white/20 text-white px-4 py-2 rounded-lg transition-all duration-300 backdrop-blur-sm"
      >
        <FaArrowRight />
        <span>بازگشت</span>
      </button>

      {/* Contact Form Container */}
      <div className="w-full max-w-2xl bg-[#0b3f32] rounded-2xl shadow-2xl overflow-hidden">
        {/* Header */}
        <div className="bg-gradient-to-l from-[#30d4cc] to-[#0b3f32] p-6 text-white">
          <h1 className="text-2xl md:text-3xl font-bold mb-2">تماس با ما</h1>
          <p className="text-white/80 text-sm md:text-base">
            ما مشتاق شنیدن نظرات و پیشنهادات شما هستیم
          </p>
        </div>

        {/* Form Body */}
        <div className="p-6 md:p-8">
          {/* Success Message */}
          {isSuccess && (
            <div className="mb-6 bg-green-500/10 border border-green-500 rounded-lg p-4 flex items-center gap-3 text-green-500">
              <FaCheckCircle className="text-xl flex-shrink-0" />
              <span>پیام شما با موفقیت ارسال شد. در حال انتقال به صفحه اصلی...</span>
            </div>
          )}
          
          {/* Submit Error */}
          {errors.submit && (
            <div className="mb-6 bg-red-500/10 border border-red-500 rounded-lg p-4 text-red-500 text-sm">
              {errors.submit}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Full Name Field */}
            <div>
              <label className="block text-white mb-2 text-sm font-medium">
                نام و نام خانوادگی <span className="text-[#30d4cc]">*</span>
              </label>
              <div className="relative">
                <FaUser className="absolute right-3 top-1/2 -translate-y-1/2 text-[#30d4cc]" />
                <input
                  type="text"
                  name="fullname"
                  value={formData.fullname}
                  onChange={handleChange}
                  placeholder="مثال: محمد رضایی"
                  className={`w-full bg-white/10 border ${errors.fullname ? 'border-red-500' : 'border-white/20'} rounded-lg py-3 pr-10 pl-4 text-white placeholder-white/50 outline-none transition-all duration-300 focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc]`}
                />
              </div>
              {errors.fullname && (
                <p className="text-red-500 text-xs mt-1">{errors.fullname}</p>
              )}
            </div>

            {/* Email Field */}
            <div>
              <label className="block text-white mb-2 text-sm font-medium">
                ایمیل <span className="text-[#30d4cc]">*</span>
              </label>
              <div className="relative">
                <FaEnvelope className="absolute right-3 top-1/2 -translate-y-1/2 text-[#30d4cc]" />
                <input
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="example@gmail.com"
                  className={`w-full bg-white/10 border ${errors.email ? 'border-red-500' : 'border-white/20'} rounded-lg py-3 pr-10 pl-4 text-white placeholder-white/50 outline-none transition-all duration-300 focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc]`}
                />
              </div>
              {errors.email && (
                <p className="text-red-500 text-xs mt-1">{errors.email}</p>
              )}
            </div>

            {/* Phone Field (Optional) */}
            <div>
              <label className="block text-white mb-2 text-sm font-medium">
                شماره موبایل <span className="text-white/50 text-xs">(اختیاری)</span>
              </label>
              <div className="relative">
                <FaPhone className="absolute right-3 top-1/2 -translate-y-1/2 text-[#30d4cc]" />
                <input
                  type="tel"
                  name="phone"
                  value={formData.phone}
                  onChange={handleChange}
                  placeholder="09123456789"
                  className={`w-full bg-white/10 border ${errors.phone ? 'border-red-500' : 'border-white/20'} rounded-lg py-3 pr-10 pl-4 text-white placeholder-white/50 outline-none transition-all duration-300 focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc]`}
                />
              </div>
              {errors.phone && (
                <p className="text-red-500 text-xs mt-1">{errors.phone}</p>
              )}
            </div>

            {/* Message Field */}
            <div>
              <label className="block text-white mb-2 text-sm font-medium">
                متن پیام <span className="text-[#30d4cc]">*</span>
              </label>
              <div className="relative">
                <FaComment className="absolute right-3 top-4 text-[#30d4cc]" />
                <textarea
                  name="message"
                  value={formData.message}
                  onChange={handleChange}
                  rows="6"
                  placeholder="پیام خود را بنویسید..."
                  className={`w-full bg-white/10 border ${errors.message ? 'border-red-500' : 'border-white/20'} rounded-lg py-3 pr-10 pl-4 text-white placeholder-white/50 outline-none transition-all duration-300 focus:border-[#30d4cc] focus:ring-1 focus:ring-[#30d4cc] resize-none`}
                />
              </div>
              {errors.message && (
                <p className="text-red-500 text-xs mt-1">{errors.message}</p>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full bg-[#30d4cc] hover:bg-[#30d4cc]/80 text-[#0b3f32] font-bold py-3 px-6 rounded-lg transition-all duration-300 transform hover:scale-105 hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center gap-2 text-lg"
            >
              {isSubmitting ? (
                <>
                  <div className="w-5 h-5 border-2 border-[#0b3f32] border-t-transparent rounded-full animate-spin" />
                  در حال ارسال...
                </>
              ) : (
                <>
                  <FaPaperPlane />
                  ارسال پیام
                </>
              )}
            </button>
          </form>

          {/* Contact Info */}
          <div className="mt-8 pt-6 border-t border-white/10">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-center text-white/70 text-sm">
              <div>
                <p className="text-[#30d4cc] mb-1">تلفن تماس</p>
                <p>025-۳۷۷۵۷۶۱۰</p>
              </div>
              <div>
                <p className="text-[#30d4cc] mb-1">ایمیل</p>
                <p>info@hekmateislami.ir</p>
              </div>
              <div>
                <p className="text-[#30d4cc] mb-1">آدرس</p>
                <p>قم- خیابان ۱۹ دی (باجک) کوچه ۱۰ فرعی اول سمت چپ پلاک </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ContactPage;