import { useState } from 'react';
import UsersList from './Admin/UserList';
import SlideshowList from './Admin/SlideList';

const AdminPanel = () => {
  const [activeTab, setActiveTab] = useState('users');

  const tabs = [
    { id: 'rooms', label: 'اتاق‌ها' },
    { id: 'users', label: 'کاربران' },
    { id: 'sessions', label: 'جلسات' },
    { id: 'sliders', label: 'اسلاید شو' },
    { id: 'schedule', label: 'برنامه' },
  ];

  const renderTabContent = () => {
    switch (activeTab) {
      case 'rooms':
        return <div className="p-4 bg-white rounded-lg shadow">بخش اتاق‌ها – به زودی</div>;
      case 'users':
        return <UsersList />;
      case 'sessions':
        return <div className="p-4 bg-white rounded-lg shadow">بخش جلسات – به زودی</div>;
      case 'sliders':
        return <SlideshowList/>;
      case 'schedule':
        return <div className="p-4 bg-white rounded-lg shadow">بخش برنامه – به زودی</div>;
      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 rtl" dir="rtl">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h1 className="text-2xl font-bold text-gray-800 mb-6">پنل مدیریت</h1>

        {/* تب‌ها */}
        <div className="border-b border-gray-200 mb-6">
          <nav className="flex space-x-8 space-x-reverse" aria-label="Tabs">
            {tabs.map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`
                  py-2 px-1 text-sm font-medium border-b-2 transition-colors
                  ${activeTab === tab.id
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }
                `}
              >
                {tab.label}
              </button>
            ))}
          </nav>
        </div>

        {/* محتوای تب فعال */}
        {renderTabContent()}
      </div>
    </div>
  );
};

export default AdminPanel;