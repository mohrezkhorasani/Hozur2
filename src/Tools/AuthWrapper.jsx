// Tools/ProtectedRoute.jsx
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/Tools/AuthProvider';

const ProtectedRoute = ({ requiredPermissions = [] }) => {
  const { user, loading, isAuthenticated } = useAuth();
  const location = useLocation();

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    // Store the current URL (including query params) in state
    return <Navigate to="/login" state={{ from: location.pathname + location.search }} replace />;
  }

  // Check for required permissions
  if (requiredPermissions.length > 0) {
    const hasPermissions = requiredPermissions.every(permission =>
      user?.permissions?.includes(permission)
    );
    
    if (!hasPermissions) {
      // صفحه دسترسی ندارم با طراحی زیبا
      return (
        <div className="min-h-screen bg-gradient-to-r from-red-50 to-orange-50 flex items-center justify-center p-4">
          <div className="max-w-md w-full bg-white rounded-2xl shadow-xl p-8 text-center">
            {/* آیکون قفل یا هشدار */}
            <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg 
                className="w-10 h-10 text-red-600" 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24" 
                xmlns="http://www.w3.org/2000/svg"
              >
                <path 
                  strokeLinecap="round" 
                  strokeLinejoin="round" 
                  strokeWidth={2} 
                  d="M12 15v2m0 0v2m0-2h2m-2 0H9m3-3V9m0 0V6m0 0h2m-2 0H9m12 0a9 9 0 11-18 0 9 9 0 0118 0z" 
                />
              </svg>
            </div>
            
            {/* عنوان */}
            <h1 className="text-2xl font-bold text-gray-800 mb-3">
              دسترسی غیرمجاز
            </h1>
            
            {/* توضیحات */}
            <p className="text-gray-600 mb-6">
              شما دسترسی لازم برای مشاهده این صفحه را ندارید.
            </p>
            
            {/* لیست دسترسی‌های مورد نیاز */}
            {requiredPermissions.length > 0 && (
              <div className="bg-gray-50 rounded-lg p-4 mb-6 text-right">
                <p className="text-sm font-medium text-gray-700 mb-2">
                  دسترسی‌های مورد نیاز:
                </p>
                <div className="flex flex-wrap gap-2 justify-center">
                  {requiredPermissions.map((perm, index) => (
                    <span 
                      key={index}
                      className="px-2 py-1 bg-red-100 text-red-700 rounded-full text-xs font-medium"
                    >
                      {perm === 'ftp' && 'مدیریت فایل‌ها'}
                      {perm === 'manage_users' && 'مدیریت کاربران'}
                      {perm === 'edit_poll' && 'مدیریت نظرسنجی‌ها'}
                      {perm === 'manage_admins' && 'مدیریت ادمین‌ها'}
                      {perm === 'create_meeting' && 'ایجاد جلسه'}
                      {perm === 'edit_meeting' && 'ویرایش جلسه'}
                      {perm === 'delete_meeting' && 'حذف جلسه'}
                      {perm === 'view_poll_reports' && 'مشاهده گزارشات'}
                      {perm || perm}
                    </span>
                  ))}
                </div>
              </div>
            )}
            
            {/* دکمه بازگشت */}
            <div className="space-y-3">
              <button
                onClick={() => window.history.back()}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
              >
                بازگشت به صفحه قبل
              </button>
              <button
                onClick={() => window.location.href = '/rooms'}
                className="w-full px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors"
              >
                رفتن به صفحه اصلی
              </button>
            </div>
          </div>
        </div>
      );
    }
  }

  return <Outlet />;
};

export default ProtectedRoute;