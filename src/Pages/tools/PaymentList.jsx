// Pages/PaymentList.jsx
import { useState, useEffect } from 'react';
import {
    FaTrash,
    FaPlus,
    FaSpinner,
    FaSearch,
    FaFilter,
    FaSort,
    FaTrashRestore,
} from 'react-icons/fa';
import DatePicker from 'react-multi-date-picker';
import persian from 'react-date-object/calendars/persian';
import persian_fa from 'react-date-object/locales/persian_fa';
import { ChangeStatusOfPayment, GetPaymentList, GetPDFOfPayment } from '../../api/GetRooms';
import generatePaymentPDF from '../../Tools/pdf';
import { Download, File, Printer } from 'lucide-react';

const persianMonths = [
    'فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور',
    'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'
];

const PaymentList = ({ onAddPayment, onAddPaymentSuccess, refreshKey }) => {
    const [data, setData] = useState([]);
    const [filteredData, setFilteredData] = useState([]);
    const [loading, setLoading] = useState(false);
    const [updatingId, setUpdatingId] = useState(null);

    // Filter states
    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState('all');
    const [showDeleted, setShowDeleted] = useState(false);
    const [dateFrom, setDateFrom] = useState(null);
    const [dateTo, setDateTo] = useState(null);
    const [amountMin, setAmountMin] = useState('');
    const [amountMax, setAmountMax] = useState('');
    const [sortBy, setSortBy] = useState('date');
    const [sortOrder, setSortOrder] = useState('desc');
    const [showFilters, setShowFilters] = useState(false);

    // Status options
    const statusOptions = [
        { value: 200, label: 'پرداخت شده', color: 'bg-green-100 text-green-800' },
        { value: 201, label: 'در انتظار پرداخت', color: 'bg-yellow-100 text-yellow-800' },
        { value: 0, label: 'لغو شده', color: 'bg-gray-100 text-gray-800' },
        { value: -400, label: 'حذف شده', color: 'bg-red-100 text-red-800' },
    ];

    const getStatusInfo = (status) => {
        return statusOptions.find((opt) => opt.value === status) || statusOptions[1];
    };
    useEffect(() => {
        loadData();
    }, [refreshKey]);

    // Load data
    const loadData = async () => {
        setLoading(true);
        try {
            const result = await GetPaymentList();
            setData(result);
        } catch (error) {
            console.error('Error loading data:', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    // Apply filters and sorting
    useEffect(() => {
        let result = [...data];

        // Show deleted or not
        if (!showDeleted) {
            result = result.filter((item) => item.status !== -400);
        }

        // Status filter
        if (statusFilter !== 'all') {
            result = result.filter((item) => item.status === parseInt(statusFilter));
        }

        // Search
        let monthFilter = null;
        if (searchTerm.trim()) {
            const term = searchTerm.trim().toLowerCase();

            // Check if search term contains a Persian month name
            for (let i = 0; i < persianMonths.length; i++) {
                if (term.includes(persianMonths[i])) {
                    monthFilter = i + 1; // month number 1-12
                    break;
                }
            }

            // Regular search on other fields
            result = result.filter(
                (item) =>
                    item.subject?.toLowerCase().includes(term) ||
                    item.source?.toLowerCase().includes(term) ||
                    item.to?.toLowerCase().includes(term) ||
                    item.documentNumber?.toLowerCase().includes(term) ||
                    item.amount?.toString().includes(term) ||
                    item.date?.includes(term)
            );
        }

        // Apply month filter if found
        if (monthFilter) {
            result = result.filter((item) => {
                if (!item.date) return false;
                const parts = item.date.split('/');
                if (parts.length !== 3) return false;
                const month = parseInt(parts[1], 10);
                return month === monthFilter;
            });
        }

        // Date range
        if (dateFrom || dateTo) {
            result = result.filter((item) => {
                const itemDate = item.date?.replace(/\//g, '') || '';
                const from = dateFrom
                    ? `${dateFrom.year}${String(dateFrom.month).padStart(2, '0')}${String(
                        dateFrom.day
                    ).padStart(2, '0')}`
                    : '';
                const to = dateTo
                    ? `${dateTo.year}${String(dateTo.month).padStart(2, '0')}${String(
                        dateTo.day
                    ).padStart(2, '0')}`
                    : '';

                if (from && to) return itemDate >= from && itemDate <= to;
                if (from) return itemDate >= from;
                if (to) return itemDate <= to;
                return true;
            });
        }

        // Amount range
        if (amountMin) {
            const min = parseInt(amountMin.replace(/,/g, ''));
            result = result.filter((item) => item.amount >= min);
        }
        if (amountMax) {
            const max = parseInt(amountMax.replace(/,/g, ''));
            result = result.filter((item) => item.amount <= max);
        }

        // Sorting
        result.sort((a, b) => {
            let valA = a[sortBy];
            let valB = b[sortBy];

            if (sortBy === 'amount' || sortBy === 'netAmount') {
                valA = parseInt(valA) || 0;
                valB = parseInt(valB) || 0;
            }
            if (sortBy === 'date') {
                valA = parseInt(valA?.replace(/\//g, '')) || 0;
                valB = parseInt(valB?.replace(/\//g, '')) || 0;
            }
            if (typeof valA === 'string') valA = valA.toLowerCase();
            if (typeof valB === 'string') valB = valB.toLowerCase();

            if (sortOrder === 'asc') {
                return valA > valB ? 1 : -1;
            } else {
                return valA < valB ? 1 : -1;
            }
        });

        setFilteredData(result);
    }, [data, searchTerm, statusFilter, showDeleted, dateFrom, dateTo, amountMin, amountMax, sortBy, sortOrder]);

    // Update status
    const updateStatus = async (id, status) => {
        setUpdatingId(id);
        try {
            await ChangeStatusOfPayment({ id, status });
            setData((prev) =>
                prev.map((item) => (item.id === id ? { ...item, status } : item))
            );
        } catch (error) {
            console.error('Error updating status:', error);
            alert('خطا در بروزرسانی وضعیت');
        } finally {
            setUpdatingId(null);
        }
    };

    // Delete item (move to trash)
    const deleteItem = async (id) => {
        // if (!window.confirm('آیا از حذف این رکورد اطمینان دارید؟')) return;
        setUpdatingId(id);
        try {
            await ChangeStatusOfPayment({ id, status: -400 });
            setData((prev) =>
                prev.map((item) => (item.id === id ? { ...item, status: -400 } : item))
            );
        } catch (error) {
            console.error('Error deleting item:', error);
            alert('خطا در حذف رکورد');
        } finally {
            setUpdatingId(null);
        }
    };

    // Restore item
    const restoreItem = async (id) => {
        setUpdatingId(id);
        try {
            await ChangeStatusOfPayment({ id, status: 201 });
            setData((prev) =>
                prev.map((item) => (item.id === id ? { ...item, status: 201 } : item))
            );
        } catch (error) {
            console.error('Error restoring item:', error);
            alert('خطا در بازیابی رکورد');
        } finally {
            setUpdatingId(null);
        }
    };

    const formatNumber = (number) => {
        if (!number && number !== 0) return '۰';
        return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    };

    const clearFilters = () => {
        setSearchTerm('');
        setStatusFilter('all');
        setDateFrom(null);
        setDateTo(null);
        setAmountMin('');
        setAmountMax('');
        setSortBy('date');
        setSortOrder('desc');
        setShowDeleted(false);
    };

    return (
        <div className="min-h-screen bg-gray-100 p-4" dir="rtl">
            <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-7xl mx-auto">
                {/* Header */}
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4 gap-4">
                    <h2 className="text-2xl font-bold text-gray-800">لیست پرداخت‌ها</h2>
                    <button
                        onClick={onAddPayment}
                        className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition duration-200 flex items-center gap-2 text-sm"
                    >
                        <FaPlus />
                        افزودن پرداختی
                    </button>
                </div>

                {/* Search Bar */}
                <div className="flex flex-wrap gap-3 mb-4">
                    <div className="flex-1 min-w-[200px] relative">
                        <input
                            type="text"
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            placeholder="جستجو در همه فیلدها... (مثلاً نام برج)"
                            className="w-full p-2.5 pl-10 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                        />
                        <FaSearch className="absolute left-3 top-3.5 text-gray-400" />
                    </div>

                    <button
                        onClick={() => setShowFilters(!showFilters)}
                        className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition flex items-center gap-2 text-sm"
                    >
                        <FaFilter />
                        فیلترها
                    </button>

                    <button
                        onClick={clearFilters}
                        className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition text-sm"
                    >
                        پاک کردن فیلترها
                    </button>
                </div>

                {/* Filters Panel */}
                {showFilters && (
                    <div className="bg-gray-50 p-4 rounded-lg mb-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                        {/* Status */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                وضعیت
                            </label>
                            <select
                                value={statusFilter}
                                onChange={(e) => setStatusFilter(e.target.value)}
                                className="w-full p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                            >
                                <option value="all">همه</option>
                                {statusOptions.map((opt) => (
                                    <option key={opt.value} value={opt.value}>
                                        {opt.label}
                                    </option>
                                ))}
                            </select>
                        </div>

                        {/* Date from */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                از تاریخ
                            </label>
                            <DatePicker
                                calendar={persian}
                                locale={persian_fa}
                                value={dateFrom}
                                onChange={setDateFrom}
                                format="YYYY/MM/DD"
                                placeholder="انتخاب تاریخ"
                                containerClassName="w-full"
                                inputClass="w-full p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                            />
                        </div>

                        {/* Date to */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                تا تاریخ
                            </label>
                            <DatePicker
                                calendar={persian}
                                locale={persian_fa}
                                value={dateTo}
                                onChange={setDateTo}
                                format="YYYY/MM/DD"
                                placeholder="انتخاب تاریخ"
                                containerClassName="w-full"
                                inputClass="w-full p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                            />
                        </div>

                        {/* Amount range */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                محدوده مبلغ
                            </label>
                            <div className="flex gap-2">
                                <input
                                    type="text"
                                    value={amountMin}
                                    onChange={(e) => setAmountMin(e.target.value.replace(/,/g, ''))}
                                    placeholder="حداقل"
                                    className="w-1/2 p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                                />
                                <input
                                    type="text"
                                    value={amountMax}
                                    onChange={(e) => setAmountMax(e.target.value.replace(/,/g, ''))}
                                    placeholder="حداکثر"
                                    className="w-1/2 p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                                />
                            </div>
                        </div>

                        {/* Sort */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                مرتب‌سازی
                            </label>
                            <div className="flex gap-2">
                                <select
                                    value={sortBy}
                                    onChange={(e) => setSortBy(e.target.value)}
                                    className="flex-1 p-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition text-sm"
                                >
                                    <option value="date">تاریخ</option>
                                    <option value="amount">مبلغ</option>
                                    <option value="netAmount">خالص پرداختی</option>
                                    <option value="subject">بابت</option>
                                </select>
                                <button
                                    onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                                    className="p-2 border border-gray-300 rounded-lg hover:bg-gray-100 transition"
                                >
                                    <FaSort className={`transform ${sortOrder === 'desc' ? 'rotate-180' : ''}`} />
                                </button>
                            </div>
                        </div>

                        {/* Show deleted */}
                        <div className="flex items-center">
                            <label className="flex items-center gap-2 text-sm font-medium text-gray-700">
                                <input
                                    type="checkbox"
                                    checked={showDeleted}
                                    onChange={(e) => setShowDeleted(e.target.checked)}
                                    className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                                />
                                نمایش حذف شده‌ها
                            </label>
                        </div>
                    </div>
                )}

                {/* Table */}
                {loading ? (
                    <div className="flex justify-center items-center py-12">
                        <FaSpinner className="animate-spin text-blue-600 text-3xl" />
                    </div>
                ) : filteredData.length === 0 ? (
                    <div className="text-center py-12 text-gray-500">هیچ رکوردی یافت نشد</div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="w-full border-collapse">
                            <thead>
                                <tr className="bg-gray-50 border-b border-gray-200">
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">ردیف</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">مبلغ (ریال)</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">بابت</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">از محل</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">تاریخ</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">مالیات</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">خالص پرداختی</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">سند شماره</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">به</th>
                                    <th className="px-4 py-3 text-right text-sm font-semibold text-gray-600">وضعیت</th>
                                    <th className="px-4 py-3 text-center text-sm font-semibold text-gray-600">عملیات</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredData.map((item, index) => {
                                    const statusInfo = getStatusInfo(item.status);
                                    const isDeleted = item.status === -400;
                                    return (
                                        <tr
                                            key={item.id}
                                            className={`border-b cursor-pointer border-gray-100 hover:bg-gray-50 transition-colors ${isDeleted ? 'opacity-60 bg-red-50' : ''
                                                }`}
                                            onClick={async () => {
                                                // setUpdatingId(item.id)
                                                // const path = await GetPDFOfPayment(item.id)
                                                // if (path) {
                                                //     window.open(path, '_blank');
                                                // }
                                                // setUpdatingId(null)
                                            }}
                                        >
                                            <td className="px-4 py-3 text-sm">{index + 1}</td>
                                            <td className="px-4 py-3 text-sm font-medium">{formatNumber(item.amount)}</td>
                                            <td className="px-4 py-3 text-sm">{item.subject}</td>
                                            <td className="px-4 py-3 text-sm">{item.source}</td>
                                            <td className="px-4 py-3 text-sm">{item.date}</td>
                                            <td className="px-4 py-3 text-sm">{item.taxPercent}%</td>
                                            <td className="px-4 py-3 text-sm font-medium">{formatNumber(item.netAmount)}</td>
                                            <td className="px-4 py-3 text-sm">{item.documentNumber}</td>
                                            <td className="px-4 py-3 text-sm">{item.to}</td>
                                            <td className="px-4 py-3">
                                                {!isDeleted ? (
                                                    <select
                                                        value={item.status}
                                                        onChange={(e) => updateStatus(item.id, parseInt(e.target.value))}
                                                        disabled={updatingId === item.id}
                                                        className={`px-3 py-1 rounded-full text-xs font-medium border-0 focus:ring-2 focus:ring-blue-500 outline-none transition cursor-pointer ${statusInfo.color}`}
                                                    >
                                                        {statusOptions
                                                            .filter((opt) => opt.value !== -400)
                                                            .map((opt) => (
                                                                <option key={opt.value} value={opt.value} className="text-gray-800">
                                                                    {opt.label}
                                                                </option>
                                                            ))}
                                                    </select>
                                                ) : (
                                                    <span className="px-3 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                                                        حذف شده
                                                    </span>
                                                )}
                                                {updatingId === item.id && (
                                                    <FaSpinner className="inline-block animate-spin mr-2 text-sm" />
                                                )}
                                            </td>
                                            <td className="px-4 py-3 text-center gap-6">
                                                <button
                                                    onClick={async () => {
                                                        setUpdatingId(item.id)
                                                        const path = await GetPDFOfPayment(item.id)
                                                        if (path) {
                                                            // window.open(path, '_blank');
                                                            const iframe = document.createElement('iframe');
                                                            iframe.style.display = 'none';
                                                            iframe.src = path;
                                                            document.body.appendChild(iframe);

                                                            // پس از بارگذاری PDF، پرینت بگیر
                                                            iframe.onload = () => {
                                                                iframe.contentWindow.print();

                                                                // بعد از اجرای پرینت، iframe را حذف می‌کنیم (اختیاری)
                                                                setTimeout(() => {
                                                                    document.body.removeChild(iframe);
                                                                }, 1000);
                                                            }

                                                            setUpdatingId(null)
                                                        }
                                                    }
                                                    }
                                                    disabled={updatingId === item.id}
                                                    className="text-green-600 hover:text-green-800 
                                                    transition-colors "
                                                    title="پرینت فایل"
                                                >

                                                    <Printer className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={async () => {
                                                        setUpdatingId(item.id)
                                                        const path = await GetPDFOfPayment(item.id)
                                                        if (path) {
                                                            window.open(path, '_blank');

                                                        }
                                                        setUpdatingId(null)
                                                    }
                                                    }
                                                    disabled={updatingId === item.id}
                                                    className="text-[#44444493] hover:text-blue-900 
                                                    transition-colors mx-5"
                                                    title="پرینت فایل"
                                                >

                                                    <Download className="w-4 h-4" />
                                                </button>
                                                {!isDeleted ? (

                                                    <button
                                                        onClick={() => deleteItem(item.id)}
                                                        disabled={updatingId === item.id}
                                                        className="text-red-600 hover:text-red-800 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                                        title="حذف"
                                                    >
                                                        <FaTrash className="w-4 h-4" />
                                                    </button>
                                                ) : (
                                                    <button
                                                        onClick={() => restoreItem(item.id)}
                                                        disabled={updatingId === item.id}
                                                        className="text-green-600 hover:text-green-800 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                                                        title="بازیابی"
                                                    >
                                                        <FaTrashRestore className="w-4 h-4" />
                                                    </button>
                                                )}

                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                )}

                {/* Footer */}
                <div className="mt-4 flex flex-wrap justify-between items-center text-sm text-gray-500 border-t border-gray-200 pt-4 gap-2">
                    <span>تعداد کل: {filteredData.length} رکورد</span>
                    <div className="flex gap-4">
                        <span>نمایش: {filteredData.filter((i) => i.status !== -400).length} فعال</span>
                        {showDeleted && (
                            <span>حذف شده: {filteredData.filter((i) => i.status === -400).length}</span>
                        )}
                    </div>
                    <button
                        onClick={loadData}
                        className="text-blue-600 hover:text-blue-800 transition-colors"
                    >
                        بروزرسانی
                    </button>
                </div>
            </div>
        </div>
    );
};

export default PaymentList; 