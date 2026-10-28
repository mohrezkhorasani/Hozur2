// src/Component/QuickAccessToolbar.jsx
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  FolderOpen, 
  Image, 
  BarChart3, 
  Users, 
  Settings, 
  FileUp,
  Shield,
  MessageSquare,
  Video,
  Calendar,
  UserCheck
} from 'lucide-react';
import { useAuth } from '../Tools/AuthProvider';

const QuickAccessToolbar = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  // آیتم‌های دسترسی سریع بر اساس دسترسی کاربر
  const quickAccessItems = [
    {
      id: 'ftp',
      title: 'مدیریت فایل‌ها',
      icon: <FolderOpen className="w-5 h-5" />,
      path: '/dashboard/ftp-manage',
      permissions: ['ftp', 'manage_admins'],
      color: 'bg-blue-500',
      hoverColor: 'hover:bg-blue-600'
    },
    {
      id: 'slideshow',
      title: 'مدیریت اسلایدشو',
      icon: <Image className="w-5 h-5" />,
      path: '/adminlay/slideshow',
      permissions: ['edit_poll', 'manage_admins'],
      color: 'bg-green-500',
      hoverColor: 'hover:bg-green-600'
    },
    {
      id: 'polls',
      title: 'مدیریت نظرخواهی',
      icon: <BarChart3 className="w-5 h-5" />,
      path: '/adminlay/polls',
      permissions: ['edit_poll', 'manage_admins'],
      color: 'bg-purple-500',
      hoverColor: 'hover:bg-purple-600'
    },
    {
      id: 'rooms',
      title: 'مدیریت گروه‌ها',
      icon: <Users className="w-5 h-5" />,
      path: '/adminlay/rooms',
      permissions: ['edit_poll', 'manage_admins'],
      color: 'bg-orange-500',
      hoverColor: 'hover:bg-orange-600'
    },
    {
      id: 'users',
      title: 'مدیریت کاربران',
      icon: <Users className="w-5 h-5" />,
      path: '/admin/users',
      permissions: ['manage_users', 'manage_admins'],
      color: 'bg-red-500',
      hoverColor: 'hover:bg-red-600'
    },
    {
      id: 'webhooks',
      title: 'مدیریت Webhook',
      icon: <Settings className="w-5 h-5" />,
      path: '/adminlay/webhooks',
      permissions: ['edit_poll', 'manage_admins'],
      color: 'bg-gray-500',
      hoverColor: 'hover:bg-gray-600'
    },
    {
      id: 'reports',
      title: 'گزارشات نظرسنجی',
      icon: <MessageSquare className="w-5 h-5" />,
      path: '/adminlay/polls/reports',
      permissions: ['view_poll_reports', 'manage_admins'],
      color: 'bg-teal-500',
      hoverColor: 'hover:bg-teal-600'
    },
    {
      id: 'meetings',
      title: 'مدیریت جلسات',
      icon: <Video className="w-5 h-5" />,
      path: '/admin/meetings',
      permissions: [],
      color: 'bg-indigo-500',
      hoverColor: 'hover:bg-indigo-600'
    },
    {
      id: 'participants',
      title: 'شرکت‌کنندگان',
      icon: <UserCheck className="w-5 h-5" />,
      path: '/admin/participants',
      permissions: [],
      color: 'bg-emerald-600',
      hoverColor: 'hover:bg-emerald-700'
    }
  ];

  // فیلتر آیتم‌ها بر اساس دسترسی کاربر
  const accessibleItems = quickAccessItems.filter(item => {
    if (!user) return false;
    if (item.permissions.length === 0 || item.id === 'meetings' || item.id === 'participants') return true;
    if (user.permissions?.includes('manage_admins')) return true;
    return item.permissions.some(perm => user.permissions?.includes(perm));
  });

  if (accessibleItems.length === 0) return null;

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 mb-6 p-3">
      <div className="flex items-center justify-between flex-wrap gap-2">
        <div className="flex items-center gap-2">
          <Shield className="w-5 h-5 text-gray-500" />
          <span className="text-sm font-medium text-gray-700">دسترسی سریع:</span>
        </div>
        
        <div className="flex flex-wrap gap-2">
          {accessibleItems.map((item) => (
            <button
              key={item.id}
              onClick={() => navigate(item.path)}
              className={`flex items-center gap-2 ${item.color} text-white px-3 py-2 rounded-lg ${item.hoverColor} transition-all duration-200 transform hover:scale-105 shadow-sm`}
              title={item.title}
            >
              {item.icon}
              <span className="text-sm font-medium whitespace-nowrap">{item.title}</span>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
};

export default QuickAccessToolbar;