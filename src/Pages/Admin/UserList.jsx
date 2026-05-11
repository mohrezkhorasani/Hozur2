// src/pages/admin/UsersList.jsx
import { useState, useEffect, useMemo } from 'react';
import { FaSearch, FaSort, FaCheckCircle, FaTimesCircle, FaEdit, FaPlus, FaTrash, FaTimes } from 'react-icons/fa';
import { GetUserList, ChangeRole, ChangeStatus } from "../../api/AdminAPI";
import { NewRoomPerUserModal } from './modal/NewRoomPerUserModal';

// ---------- نگاشت نقش‌ها (رشته انگلیسی ↔ عدد) ----------
const roleStringToNumber = {
    'UNKNOWN': 599,
    'GUEST': 501,
    'MODERATOR': 502,
    'ADMINISTRATOR': 503,
    'ADMIN': 504,
    'USER': 505,
    'CREATOR': 506,
    'VIEWER': 507,
    'TEACHER': 508,
    'NORMAL': 505,
};

const roleNumberToString = Object.fromEntries(
    Object.entries(roleStringToNumber).map(([k, v]) => [v, k])
);

const roleDisplayMap = {
    'USER': 'کاربر عادی',
    'ADMIN': 'ادمین',
    'GUEST': 'مهمان',
    'MODERATOR': 'مدیر',
    'ADMINISTRATOR': 'مدیر ارشد',
    'CREATOR': 'سازنده',
    'VIEWER': 'بیننده',
    'TEACHER': 'معلم',
    'UNKNOWN': 'نامشخص',
    'NORMAL': 'معمولی',
};

const roleOptions = Object.entries(roleStringToNumber).map(([roleString, num]) => ({
    value: num,
    label: roleDisplayMap[roleString] || roleString,
}));

const sortOptions = [
    { value: 100, label: 'جدیدترین', field: 'id', order: 'desc' },
    { value: 101, label: 'قدیمی‌ترین', field: 'id', order: 'asc' },
    { value: 102, label: 'نام (ز - الف)', field: 'fullname', order: 'desc' },
    { value: 103, label: 'نام (الف - ز)', field: 'fullname', order: 'asc' },
    { value: 104, label: 'امتیاز (زیاد به کم)', field: 'score', order: 'desc' },
    { value: 105, label: 'امتیاز (کم به زیاد)', field: 'score', order: 'asc' },
];


// ---------- کامپوننت اصلی UsersList ----------
const UsersList = () => {
    const [allUsers, setAllUsers] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState(100);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 15;
    const [modalOpen, setModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);

    const fetchAllUsers = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await GetUserList(1, 2000, sortBy);
            if (response.status === 200 && Array.isArray(response.data)) {
                setAllUsers(response.data);
            } else {
                setError('خطا در دریافت اطلاعات کاربران');
            }
        } catch (err) {
            console.error(err);
            setError('مشکل در ارتباط با سرور');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchAllUsers();
    }, []);

    const filteredUsers = useMemo(() => {
        if (!searchTerm.trim()) return allUsers;
        const term = searchTerm.toLowerCase();
        return allUsers.filter(
            user => user.fullname?.toLowerCase().includes(term) ||
                user.username?.toLowerCase().includes(term) ||
                user.email?.toLowerCase().includes(term) ||
                user.uid?.toLowerCase().includes(term)
        );
    }, [allUsers, searchTerm]);

    const sortedUsers = useMemo(() => {
        const option = sortOptions.find(opt => opt.value === sortBy);
        if (!option) return filteredUsers;
        const { field, order } = option;
        const sortField = field === 'score' ? 'id' : field;
        return [...filteredUsers].sort((a, b) => {
            let aVal = a[sortField];
            let bVal = b[sortField];
            if (typeof aVal === 'string') aVal = aVal.localeCompare(bVal);
            if (typeof bVal === 'string') bVal = bVal.localeCompare(aVal);
            if (order === 'asc') return aVal > bVal ? 1 : -1;
            return aVal < bVal ? 1 : -1;
        });
    }, [filteredUsers, sortBy]);

    const totalUsers = sortedUsers.length;
    const totalPages = Math.ceil(totalUsers / itemsPerPage);
    const paginatedUsers = sortedUsers.slice((currentPage - 1) * itemsPerPage, currentPage * itemsPerPage);

    useEffect(() => {
        setCurrentPage(1);
    }, [searchTerm, sortBy]);

    const handleRoleChange = async (userId, newRoleNumber) => {
        try {
            const result = await ChangeRole(userId, newRoleNumber);
            if (result.status === 200) {
                const newRoleString = roleNumberToString[newRoleNumber];
                setAllUsers(prev => prev.map(user => user.id === userId ? { ...user, role: newRoleString } : user));
                alert(`نقش کاربر با موفقیت به ${roleDisplayMap[newRoleString]} تغییر کرد`);
            } else {
                alert('خطا در تغییر نقش');
            }
        } catch (err) {
            console.error(err);
            alert('خطا در ارتباط با سرور');
        }
    };

    const toggleStatus = async (userId, currentIsValid) => {
        const newStatus = !currentIsValid;
        const statusCode = newStatus ? 100 : -100;
        try {
            const result = await ChangeStatus(userId, statusCode);
            if (result.status === 200) {
                setAllUsers(prev => prev.map(user => user.id === userId ? { ...user, isValid: newStatus } : user));
                alert(`وضعیت کاربر به ${newStatus ? 'فعال' : 'غیرفعال'} تغییر کرد`);
            } else {
                alert('خطا در تغییر وضعیت');
            }
        } catch (err) {
            console.error(err);
            alert('خطا در ارتباط با سرور');
        }
    };

    const renderPaginationButtons = () => {
        if (totalPages <= 1) return null;
        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, currentPage + 2);
        if (endPage - startPage < 4) {
            if (startPage === 1) endPage = Math.min(totalPages, startPage + 4);
            else startPage = Math.max(1, endPage - 4);
        }
        const pages = [];
        for (let i = startPage; i <= endPage; i++) pages.push(i);
        return (
            <div className="flex gap-1">
                <button onClick={() => setCurrentPage(p => Math.max(1, p - 1))} disabled={currentPage === 1} className="px-3 py-1 border rounded-md disabled:opacity-50">قبلی</button>
                {startPage > 1 && (<><button onClick={() => setCurrentPage(1)} className="px-3 py-1 border rounded-md">1</button>{startPage > 2 && <span className="px-2">...</span>}</>)}
                {pages.map(p => <button key={p} onClick={() => setCurrentPage(p)} className={`px-3 py-1 border rounded-md ${currentPage === p ? 'bg-blue-500 text-white' : ''}`}>{p}</button>)}
                {endPage < totalPages && (<>{endPage < totalPages - 1 && <span className="px-2">...</span>}<button onClick={() => setCurrentPage(totalPages)} className="px-3 py-1 border rounded-md">{totalPages}</button></>)}
                <button onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))} disabled={currentPage === totalPages} className="px-3 py-1 border rounded-md disabled:opacity-50">بعدی</button>
            </div>
        );
    };

    if (error) return <div className="bg-red-100 text-red-700 p-4 rounded-lg">{error}</div>;

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden" dir="rtl">
            {/* نوار جستجو و مرتب‌سازی */}
            <div className="p-4 border-b border-gray-200 flex flex-col sm:flex-row gap-4 justify-between">
                <div className="relative flex-1">
                    <input type="text" placeholder="جستجو بر اساس نام، نام کاربری، ایمیل یا UID..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)} className="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg" />
                    <FaSearch className="absolute right-3 top-3 text-gray-400" />
                </div>
                <div className="flex items-center gap-2">
                    <FaSort className="text-gray-500" />
                    <select value={sortBy} onChange={e => setSortBy(Number(e.target.value))} className="border border-gray-300 rounded-lg px-3 py-2">
                        {sortOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                    </select>
                </div>
            </div>

            {/* جدول کاربران */}
            {loading ? (
                <div className="text-center py-12">در حال بارگذاری کاربران...</div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">شناسه</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نام کامل</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نام کاربری</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">ایمیل</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نقش</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">وضعیت</th><th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">عملیات</th></tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {paginatedUsers.map(user => {
                                const currentRoleNumber = roleStringToNumber[user.role] || 505;
                                return (
                                    <tr key={user.id} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 text-sm text-gray-900">{user.id}</td>
                                        <td className="px-6 py-4 text-sm font-medium text-gray-900">{user.fullname}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">{user.username || '-'}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">{user.email || '-'}</td>
                                        <td className="px-6 py-4 text-sm">
                                            <select value={currentRoleNumber} onChange={e => handleRoleChange(user.id, Number(e.target.value))} className="border border-gray-300 rounded-md px-2 py-1 text-sm bg-white">
                                                {roleOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                                            </select>
                                        </td>
                                        <td className="px-6 py-4 text-sm">
                                            <button onClick={() => toggleStatus(user.id, user.isValid)} className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium ${user.isValid ? 'bg-green-100 text-green-800 hover:bg-green-200' : 'bg-red-100 text-red-800 hover:bg-red-200'}`}>
                                                {user.isValid ? <FaCheckCircle /> : <FaTimesCircle />}{user.isValid ? 'فعال' : 'غیرفعال'}
                                            </button>
                                        </td>
                                        <td className="px-6 py-4 text-sm">
                                            <button
                                                onClick={() => {
                                                    setSelectedUser({ id: user.id, uid: user.uid });
                                                    setModalOpen(true);
                                                }}
                                                className="text-blue-600 hover:text-blue-800"
                                            >
                                                <FaEdit />
                                            </button>
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                </div>
            )}

            {/* صفحه‌بندی */}
            {!loading && totalPages > 1 && (
                <div className="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
                    <div className="text-sm text-gray-700">نمایش {paginatedUsers.length} از {totalUsers} کاربر</div>
                    {renderPaginationButtons()}
                </div>
            )}

            {/* مودال مدیریت اتاق‌ها */}
            <NewRoomPerUserModal
                isOpen={modalOpen}
                onClose={() => setModalOpen(false)}
                userId={selectedUser?.id}
                userUid={selectedUser?.uid}
            />

        </div>
    );
};

export default UsersList;