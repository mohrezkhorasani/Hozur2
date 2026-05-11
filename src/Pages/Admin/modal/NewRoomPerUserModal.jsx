import { useEffect, useState } from "react";
import { FaEdit, FaSearch, FaTimes, FaTrash, FaPlus, FaCheck, FaBan } from "react-icons/fa";
import { AdminCreateRoom, GetRoomListOfUser, RemoveAdminGetUserRoomList } from "../../../api/AdminAPI";

export const NewRoomPerUserModal = ({ isOpen, onClose, userId, userUid }) => {
    const [rooms, setRooms] = useState([]);
    const [searchTerm, setSearchTerm] = useState('');
    const [formData, setFormData] = useState({
        title: '',
        capacity: '1001',
        description: '',
        isPublic: false,
        isRequiredLogin: false,
        isRequiredAccept: false,
        isRecordable: false,
    });
    const [editingId, setEditingId] = useState(null);
    const [loading, setLoading] = useState(false);
    const [activeTab, setActiveTab] = useState('add');
    const [fetchLoading, setFetchLoading] = useState(false);

    // دریافت لیست اتاق‌ها از سرور
    const fetchRooms = async () => {
        if (!userId) return;
        setFetchLoading(true);
        try {
            const response = await GetRoomListOfUser(userId, 0, 100, 100); // order = جدیدترین
            if (response.status === 200 && Array.isArray(response.data)) {
                // فرض می‌کنیم response.data آرایه‌ای از اتاق‌هاست با فیلدهای id, title, capacity, description, isPublic, etc.
                const roomsData = response.data.map(room => ({
                    id: room.id,
                    name: room.title || room.name,
                    capacity: room.capacity,
                    description: room.description,
                    isPublic: room.isPublic,
                    isRequiredLogin: room.isRequiredLogin,
                    isRequiredAccept: room.isRequiredAccept,
                    isRecordable: room.isRecordable,
                }));
                setRooms(roomsData);
            } else {
                setRooms([]);
            }
        } catch (err) {
            console.error("Error fetching rooms:", err);
            setRooms([]);
        } finally {
            setFetchLoading(false);
        }
    };

    // هر بار مودال باز می‌شه، لیست رو دریافت کن
    useEffect(() => {
        console.log(isOpen, userUid)
        if (isOpen && userUid) {
            fetchRooms();
        }
    }, [isOpen, userUid]);

    if (!isOpen) return null;

    const filteredRooms = rooms.filter(room =>
        room.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (room.description && room.description.toLowerCase().includes(searchTerm.toLowerCase()))
    );

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!formData.title || !formData.capacity) {
            alert('عنوان و ظرفیت الزامی است');
            return;
        }

        const newRoom = {
            Title: formData.title,
            Capacity: Number(formData.capacity),   // اضافه شد
            Description: formData.description,     // اضافه شد
            IsPublic: formData.isPublic,
            IsRequiredLogin: formData.isRequiredLogin,
            IsRequiredAccept: formData.isRequiredAccept,
            IsRecordable: formData.isRecordable,
            UserID: userId,
        };

        // اگر در حالت ویرایش هستیم، ID اتاق را اضافه کن
        if (editingId) {
            newRoom.ID = editingId;
        }

        setLoading(true);
        try {
            const result = await AdminCreateRoom(newRoom);
            if (result.status === 200) {
                alert(editingId ? 'اتاق با موفقیت ویرایش شد' : 'اتاق با موفقیت ایجاد شد');
                setFormData({
                    title: '', capacity: '', description: '',
                    isPublic: false, isRequiredLogin: false, isRequiredAccept: false, isRecordable: false
                });
                setEditingId(null);
                fetchRooms(); // به‌روزرسانی لیست
                setActiveTab('list');
            } else {
                alert(editingId ? 'خطا در ویرایش اتاق' : 'خطا در ایجاد اتاق');
            }
        } catch (err) {
            console.error(err);
            alert(err.message || 'خطا در ارتباط با سرور');
        } finally {
            setLoading(false);
        }
    };
    const handleEdit = (room) => {
        setFormData({
            title: room.name,
            capacity: room.capacity,
            description: room.description || '',
            isPublic: room.isPublic || false,
            isRequiredLogin: room.isRequiredLogin || false,
            isRequiredAccept: room.isRequiredAccept || false,
            isRecordable: room.isRecordable || false,
        });
        setEditingId(room.id);
        setActiveTab('add');
    };

    const handleDelete = async (roomId) => {
        if (!window.confirm('آیا از حذف این اتاق مطمئن هستید؟')) return;
        try {
            const result = await RemoveAdminGetUserRoomList(userUid, roomId);
            if (result.status === 200) {
                alert('اتاق با موفقیت حذف شد');
                fetchRooms(); // به‌روزرسانی لیست
            } else {
                alert('خطا در حذف اتاق');
            }
        } catch (err) {
            console.error(err);
            alert('خطا در ارتباط با سرور');
        }
    };

    const cancelEdit = () => {
        setFormData({
            title: '', capacity: '', description: '',
            isPublic: false, isRequiredLogin: false, isRequiredAccept: false, isRecordable: false
        });
        setEditingId(null);
    };

    return (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 transition-all" onClick={onClose}>
            <div className="bg-white rounded-2xl shadow-2xl w-full max-w-5xl max-h-[90vh] overflow-hidden" onClick={e => e.stopPropagation()}>
                {/* هدر با گرادینت */}
                <div className="bg-gradient-to-r from-[#0b3f32] to-[#1379ff] text-white px-6 py-4 flex justify-between items-center">
                    <h2 className="text-xl font-bold tracking-tight">مدیریت اتاق‌ها</h2>
                    <button onClick={onClose} className="text-white/80 hover:text-white transition-all hover:rotate-90 duration-200">
                        <FaTimes size={22} />
                    </button>
                </div>

                {/* تب‌های داخلی */}
                <div className="flex border-b border-gray-200 px-6 pt-3">
                    <button
                        onClick={() => setActiveTab('add')}
                        className={`pb-2 px-4 font-medium transition-all ${activeTab === 'add' ? 'text-[#1379ff] border-b-2 border-[#1379ff]' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        <FaPlus className="inline ml-1" size={14} /> افزودن اتاق جدید
                    </button>
                    <button
                        onClick={() => setActiveTab('list')}
                        className={`pb-2 px-4 font-medium transition-all ${activeTab === 'list' ? 'text-[#1379ff] border-b-2 border-[#1379ff]' : 'text-gray-500 hover:text-gray-700'}`}
                    >
                        📋 لیست اتاق‌ها
                    </button>
                </div>

                <div className="p-6 overflow-y-auto max-h-[calc(90vh-120px)]">
                    {/* تب افزودن */}
                    {activeTab === 'add' && (
                        <div className="bg-gray-50 rounded-xl p-6 shadow-inner">
                            <h3 className="text-md font-semibold text-[#0b3f32] mb-4">
                                {editingId ? '✏️ ویرایش اتاق' : '➕ اطلاعات اتاق جدید'}
                            </h3>
                            <form onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">عنوان اتاق *</label>
                                        <input
                                            type="text" name="title" value={formData.title} onChange={handleInputChange}
                                            className="w-full border-gray-300 rounded-lg shadow-sm focus:ring-[#1379ff] focus:border-[#1379ff]"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">ظرفیت *</label>
                                        <input
                                            type="number" name="capacity" 
                                            value={formData.capacity? formData.capacity:1000}
                                            defaultValue={1000}
                                             onChange={handleInputChange}
                                            className="w-full border-gray-300 rounded-lg shadow-sm focus:ring-[#1379ff] focus:border-[#1379ff]"
                                            required
                                        />
                                    </div>
                                    <div className="md:col-span-2">
                                        <label className="block text-sm font-medium text-gray-700 mb-1">توضیحات (اختیاری)</label>
                                        <input
                                            type="text" name="description" value={formData.description} onChange={handleInputChange}
                                            className="w-full border-gray-300 rounded-lg shadow-sm focus:ring-[#1379ff] focus:border-[#1379ff]"
                                        />
                                    </div>
                                </div>

                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 pt-2">
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" name="isPublic" checked={formData.isPublic} onChange={handleInputChange} className="rounded text-[#1379ff] focus:ring-[#30d4cc]" />
                                        🌍 عمومی
                                    </label>
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" name="isRequiredLogin" checked={formData.isRequiredLogin} onChange={handleInputChange} className="rounded text-[#1379ff] focus:ring-[#30d4cc]" />
                                        🔐 نیاز به ورود
                                    </label>
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" name="isRequiredAccept" checked={formData.isRequiredAccept} onChange={handleInputChange} className="rounded text-[#1379ff] focus:ring-[#30d4cc]" />
                                        ✅ نیاز به تأیید
                                    </label>
                                    <label className="flex items-center gap-2 text-sm text-gray-700">
                                        <input type="checkbox" name="isRecordable" checked={formData.isRecordable} onChange={handleInputChange} className="rounded text-[#1379ff] focus:ring-[#30d4cc]" />
                                        🎥 قابل ضبط
                                    </label>
                                </div>

                                <div className="flex gap-3 pt-3">
                                    <button
                                        type="submit"
                                        disabled={loading}
                                        className="bg-[#1379ff] text-white px-6 py-2 rounded-lg hover:bg-[#0b5fcc] transition shadow-md disabled:opacity-50"
                                    >
                                        {loading ? 'در حال ثبت...' : (editingId ? 'به‌روزرسانی' : 'ذخیره اتاق')}
                                    </button>
                                    {editingId && (
                                        <button
                                            type="button"
                                            onClick={cancelEdit}
                                            className="bg-gray-300 text-gray-800 px-5 py-2 rounded-lg hover:bg-gray-400 transition"
                                        >
                                            انصراف
                                        </button>
                                    )}
                                </div>
                            </form>
                        </div>
                    )}

                    {/* تب لیست اتاق‌ها */}
                    {activeTab === 'list' && (
                        <div>
                            <div className="relative mb-5">
                                <input
                                    type="text"
                                    placeholder="جستجو در عنوان یا توضیحات اتاق..."
                                    value={searchTerm}
                                    onChange={e => setSearchTerm(e.target.value)}
                                    className="w-full pr-10 pl-4 py-2.5 border border-gray-200 rounded-xl focus:ring-2 focus:ring-[#30d4cc] focus:border-transparent shadow-sm"
                                />
                                <FaSearch className="absolute right-3 top-3.5 text-gray-400" />
                            </div>

                            {fetchLoading ? (
                                <div className="text-center py-8 text-gray-500">در حال بارگیری اتاق‌ها...</div>
                            ) : (
                                <div className="overflow-x-auto rounded-xl border border-gray-100 shadow">
                                    <table className="min-w-full bg-white">
                                        <thead className="bg-[#0b3f32] text-white">
                                            <tr>
                                                <th className="p-3 text-right rounded-tr-xl">شناسه</th>
                                                <th className="p-3 text-right">عنوان</th>
                                                <th className="p-3 text-right">ظرفیت</th>
                                                <th className="p-3 text-right">عمومی</th>
                                                <th className="p-3 text-right">ورود</th>
                                                <th className="p-3 text-right">تأیید</th>
                                                <th className="p-3 text-right">ضبط</th>
                                                <th className="p-3 text-right rounded-tl-xl">عملیات</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {filteredRooms.length === 0 ? (
                                                <tr>
                                                    <td colSpan="8" className="text-center py-8 text-gray-500">هیچ اتاقی یافت نشد</td>
                                                </tr>
                                            ) : (
                                                filteredRooms.map((room, idx) => (
                                                    <tr key={room.id} className={`border-b ${idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'} hover:bg-[#30d4cc]/5 transition`}>
                                                        <td className="p-3 text-sm">{room.id}</td>
                                                        <td className="p-3 font-medium text-gray-800">{room.name}</td>
                                                        <td className="p-3">{room.capacity}</td>
                                                        <td className="p-3">{room.isPublic ? <FaCheck className="text-green-600" /> : <FaBan className="text-red-400" />}</td>
                                                        <td className="p-3">{room.isRequiredLogin ? <FaCheck className="text-green-600" /> : <FaBan className="text-red-400" />}</td>
                                                        <td className="p-3">{room.isRequiredAccept ? <FaCheck className="text-green-600" /> : <FaBan className="text-red-400" />}</td>
                                                        <td className="p-3">{room.isRecordable ? <FaCheck className="text-green-600" /> : <FaBan className="text-red-400" />}</td>
                                                        <td className="p-3 flex gap-3">
                                                            <button onClick={() => handleEdit(room)} className="text-[#1379ff] hover:text-[#0b5fcc] transition text-lg">
                                                                <FaEdit />
                                                            </button>
                                                            <button onClick={() => handleDelete(room.id)} className="text-red-500 hover:text-red-700 transition text-lg">
                                                                <FaTrash />
                                                            </button>
                                                        </td>
                                                    </tr>
                                                ))
                                            )}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};