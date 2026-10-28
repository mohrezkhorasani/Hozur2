import { useState, useEffect, useMemo } from 'react';
import { FaSearch, FaSort, FaFilter, FaCheckCircle, FaTimesCircle, FaEdit, FaUserEdit, FaPlus, FaTrash, FaTimes, FaUser } from 'react-icons/fa';
import { GetUserList, ChangeRole, ChangeStatus } from "../../api/AdminAPI";
import { NewRoomPerUserModal } from './modal/NewRoomPerUserModal';
import { useToast } from '../../Tools/ToastProvider';
import { IoAddSharp } from 'react-icons/io5';
import AttachRoomsModal from '../../Modal/Admin/AddRoomToUserModal';
import EditRoomModal from '../../Modal/Admin/EditRoomModal';
import EditUserModal from '../../Modal/Admin/EditUserModal';

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
    const [selectedRoleFilter, setSelectedRoleFilter] = useState('ALL');
    const [sortBy, setSortBy] = useState(100);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 15;
    const [modalOpen, setModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState(null);
    const [isAttachModalOpen, setIsAttachModalOpen] = useState(false);
    const [isEditModalOpen, setIsEditModalOpen] = useState(false);
    const [selectedRoomToEdit, setSelectedRoomToEdit] = useState(null);
    const [isEditUserModalOpen, setIsEditUserModalOpen] = useState(false);
    const [userToEdit, setUserToEdit] = useState(null);
    const toast = useToast();

    const fetchAllUsers = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await GetUserList(1, 20000, sortBy);
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
        let result = allUsers;

        if (selectedRoleFilter !== 'ALL') {
            result = result.filter(user => {
                if (user.role === undefined || user.role === null) return false;
                const userRoleStr = typeof user.role === 'number'
                    ? (roleNumberToString[user.role] || '')
                    : String(user.role).toUpperCase();
                return userRoleStr === selectedRoleFilter;
            });
        }

        if (searchTerm.trim()) {
            const term = searchTerm.toLowerCase();
            result = result.filter(
                user => user.fullname?.toLowerCase().includes(term) ||
                    user.username?.toLowerCase().includes(term) ||
                    user.email?.toLowerCase().includes(term) ||
                    user.uid?.toLowerCase().includes(term)
            );
        }

        return result;
    }, [allUsers, searchTerm, selectedRoleFilter]);

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
    }, [searchTerm, sortBy, selectedRoleFilter]);

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
            {/* نوار جستجو، فیلتر نقش و مرتب‌سازی */}
            <div className="p-4 border-b border-gray-200 flex flex-col sm:flex-row gap-4 justify-between items-center">
                <div className="relative flex-1 w-full">
                    <input type="text" placeholder="جستجو بر اساس نام، نام کاربری، ایمیل یا UID..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)} className="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg" />
                    <FaSearch className="absolute right-3 top-3 text-gray-400" />
                </div>
                <div className="flex flex-wrap items-center gap-3 w-full sm:w-auto">
                    <div className="flex items-center gap-1">
                        <FaFilter className="text-gray-500" />
                        <select
                            value={selectedRoleFilter}
                            onChange={e => setSelectedRoleFilter(e.target.value)}
                            className="border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white"
                        >
                            <option value="ALL">همه نقش‌ها</option>
                            {Object.entries(roleDisplayMap).map(([roleKey, label]) => (
                                <option key={roleKey} value={roleKey}>{label}</option>
                            ))}
                        </select>
                    </div>
                    <div className="flex items-center gap-1">
                        <FaSort className="text-gray-500" />
                        <select value={sortBy} onChange={e => setSortBy(Number(e.target.value))} className="border border-gray-300 rounded-lg px-3 py-2 text-sm bg-white">
                            {sortOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                        </select>
                    </div>
                </div>
            </div>

            {/* جدول کاربران */}
            {loading ? (
                <div className="text-center py-12">در حال بارگذاری کاربران...</div>
            ) : (
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">شناسه</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">آواتار</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نام کامل</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نام کاربری</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">ایمیل</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">نقش</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">وضعیت</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">عملیات</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {paginatedUsers.map(user => {
                                const currentRoleNumber = roleStringToNumber[user.role] || 505;
                                const avatarSrc = user.avatarPath || user.AvatarPath || user.avatar;
                                return (
                                    <tr key={user.id} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 text-sm text-gray-900">{user.id}</td>
                                        <td className="px-6 py-4 text-sm text-gray-900">
                                            <div className="flex items-center">
                                                {avatarSrc ? (
                                                    <img
                                                        src={avatarSrc}
                                                        alt={user.fullname || 'آواتار'}
                                                        className="w-10 h-10 rounded-full object-cover border border-gray-200 shadow-sm"
                                                        onError={(e) => {
                                                            e.currentTarget.style.display = 'none';
                                                            if (e.currentTarget.nextSibling) {
                                                                e.currentTarget.nextSibling.style.display = 'flex';
                                                            }
                                                        }}
                                                    />
                                                ) : null}
                                                <div
                                                    className={`w-10 h-10 rounded-full bg-gradient-to-br from-blue-50 to-indigo-100 border border-blue-200 items-center justify-center text-blue-700 font-bold text-sm shadow-sm ${avatarSrc ? 'hidden' : 'flex'}`}
                                                >
                                                    {user.fullname ? user.fullname.trim().charAt(0) : <FaUser className="text-blue-500 text-xs" />}
                                                </div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-sm font-medium text-gray-900">{user.fullname}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">{user.username || '-'}</td>
                                        <td className="px-6 py-4 text-sm text-gray-500">{user.email || '-'}</td>
                                        <td className="px-6 py-4 text-sm">
                                            <select value={currentRoleNumber} onChange={e => handleRoleChange(user.id, Number(e.target.value))} className="border border-gray-300 rounded-md px-2 py-1 text-sm bg-white">
                                                {roleOptions.map(opt => <option key={opt.value} value={opt.value}>{opt.label}</option>)}
                                            </select>
                                        </td>
                                        <td className="px-6 py-4 text-sm">
                                            <div className="flex flex-col gap-1 items-start">
                                                <button onClick={() => toggleStatus(user.id, user.isValid)} className={`flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${user.isValid ? 'bg-green-100 text-green-800 hover:bg-green-200' : 'bg-red-100 text-red-800 hover:bg-red-200'}`}>
                                                    {user.isValid ? <FaCheckCircle /> : <FaTimesCircle />}{user.isValid ? 'فعال' : 'غیرفعال'}
                                                </button>
                                                {user.status !== undefined && user.status !== null && (
                                                    <span className="text-[10px] text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded font-mono">
                                                        وضعیت: {user.status}
                                                    </span>
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-sm flex items-center gap-3">
                                            <button
                                                onClick={() => {
                                                    setUserToEdit(user);
                                                    setIsEditUserModalOpen(true);
                                                }}
                                                className="text-purple-600 hover:text-purple-800 transition"
                                                title="ویرایش اطلاعات کاربر (نام، نام خانوادگی، وضعیت، VIP، آواتار)"
                                            >
                                                <FaUserEdit size={18} />
                                            </button>
                                            <button
                                                onClick={() => {
                                                    setSelectedUser({ id: user.id, uid: user.uid });
                                                    setModalOpen(true);
                                                }}
                                                className="text-blue-600 hover:text-blue-800 transition"
                                                title="مدیریت و افزودن اتاق‌های این کاربر"
                                            >
                                                <FaEdit size={16} />
                                            </button>
                                            <button
                                                onClick={() => {
                                                    setSelectedUser({ id: user.id, uid: user.uid });
                                                    setIsAttachModalOpen(true);
                                                }}
                                                className="text-emerald-600 hover:text-emerald-800 transition"
                                                title="اتصال اتاق‌ها به این کاربر (AddRoomToUserModal)"
                                            >
                                                <IoAddSharp size={20} />
                                            </button>
                                            <button
                                                onClick={() => {
                                                    setSelectedRoomToEdit({ friendlyID: user.username || user.uid || '', roomTitle: user.fullname || '' });
                                                    setIsEditModalOpen(true);
                                                }}
                                                className="text-teal-600 hover:text-teal-800 transition text-xs font-semibold bg-teal-50 px-2 py-1 rounded-md border border-teal-200"
                                                title="ویرایش اتاق"
                                            >
                                                ویرایش اتاق
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

            {/* مودال‌های مدیریت کاربر و اتاق */}
            <EditUserModal
                isOpen={isEditUserModalOpen}
                onClose={() => setIsEditUserModalOpen(false)}
                initialUser={userToEdit}
                onSuccess={(updatedUser) => {
                    setAllUsers(prev => prev.map(u => u.id === updatedUser.id ? { ...u, ...updatedUser } : u));
                }}
            />
            <NewRoomPerUserModal
                isOpen={modalOpen}
                onClose={() => setModalOpen(false)}
                userId={selectedUser?.id}
                userUid={selectedUser?.uid}
            />
            <AttachRoomsModal
                isOpen={isAttachModalOpen}
                onClose={() => setIsAttachModalOpen(false)}
                userId={selectedUser?.id}
                onAttachRooms={() => {
                    setIsAttachModalOpen(false)
                    toast.success("گروه‌ها اضافه شدند")
                }}
            />
            <EditRoomModal
                isOpen={isEditModalOpen}
                onClose={() => setIsEditModalOpen(false)}
                initialRoom={selectedRoomToEdit}
                onSuccess={() => {
                    setIsEditModalOpen(false);
                    toast.success("اطلاعات اتاق با موفقیت به‌روزرسانی شد");
                }}
            />
        </div>
    );
};

export default UsersList;