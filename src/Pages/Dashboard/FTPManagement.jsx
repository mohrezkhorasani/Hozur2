// src/pages/FileManager/FileManagerPage.jsx
import React, { useState, useEffect } from 'react';
import { 
    Upload, 
    Search, 
    Filter, 
    Trash2, 
    Edit2, 
    Download, 
    Eye,
    Grid,
    List,
    Image as ImageIcon,
    Video,
    Music,
    File,
    X,
    Check,
    RefreshCw
} from 'lucide-react';
import UploadFileModal from '../../Modal/UploadFTP';
import { GetFiles, DeleteFile, UpdateFileInfo, GetFileStatistics } from '../../api/FTP';
const FileManagerPage = () => {
    const [files, setFiles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedType, setSelectedType] = useState('all');
    const [viewMode, setViewMode] = useState('grid'); // grid or list
    const [pagination, setPagination] = useState({
        pageNumber: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0
    });
    const [editingFile, setEditingFile] = useState(null);
    const [editTitle, setEditTitle] = useState('');
    const [statistics, setStatistics] = useState(null);
    
    const loadFiles = async () => {
        setLoading(true);
        try {
            const params = {
                pageNumber: pagination.pageNumber,
                pageSize: pagination.pageSize,
                searchTerm: searchTerm || undefined,
                fileType: selectedType !== 'all' ? selectedType : undefined,
                sortBy: 'DateInsert',
                sortDescending: true
            };
            
            const response = await GetFiles(params);
            
            if (response.status === 200) {
                setFiles(response.files || []);
                setPagination(prev => ({
                    ...prev,
                    totalCount: response.totalCount,
                    totalPages: response.totalPages
                }));
            }
        } catch (error) {
            console.error('Error loading files:', error);
        } finally {
            setLoading(false);
        }
    };
    
    const loadStatistics = async () => {
        try {
            const response = await GetFileStatistics();
            if (response.status === 200) {
                setStatistics(response.stats);
            }
        } catch (error) {
            console.error('Error loading statistics:', error);
        }
    };
    
    useEffect(() => {
        loadFiles();
        loadStatistics();
    }, [pagination.pageNumber, pagination.pageSize, selectedType]);
    
    useEffect(() => {
        const debounce = setTimeout(() => {
            if (pagination.pageNumber === 1) {
                loadFiles();
            } else {
                setPagination(prev => ({ ...prev, pageNumber: 1 }));
            }
        }, 500);
        
        return () => clearTimeout(debounce);
    }, [searchTerm]);
    
    const handleDeleteFile = async (fileID) => {
        if (!confirm('آیا از حذف این فایل اطمینان دارید؟')) return;
        
        try {
            const response = await DeleteFile(fileID, false);
            if (response.status === 200) {
                await loadFiles();
                await loadStatistics();
            }
        } catch (error) {
            console.error('Error deleting file:', error);
            alert('خطا در حذف فایل');
        }
    };
    
    const handleUpdateFile = async (fileID) => {
        if (!editTitle.trim()) return;
        
        try {
            const response = await UpdateFileInfo(fileID, editTitle, null, null);
            if (response.status === 200) {
                setEditingFile(null);
                await loadFiles();
            }
        } catch (error) {
            console.error('Error updating file:', error);
            alert('خطا در ویرایش فایل');
        }
    };
    
    const handleUploadSuccess = () => {
        loadFiles();
        loadStatistics();
    };
    
    const getFileTypeIcon = (fileType) => {
        switch (fileType) {
            case 'images': return <ImageIcon className="w-5 h-5 text-blue-500" />;
            case 'videos': return <Video className="w-5 h-5 text-green-500" />;
            case 'audios': return <Music className="w-5 h-5 text-purple-500" />;
            default: return <File className="w-5 h-5 text-gray-500" />;
        }
    };
    
    const getFileTypeBadge = (fileType) => {
        const badges = {
            images: 'bg-blue-100 text-blue-700',
            videos: 'bg-green-100 text-green-700',
            audios: 'bg-purple-100 text-purple-700'
        };
        const labels = {
            images: 'تصویر',
            videos: 'ویدئو',
            audios: 'صوت'
        };
        
        return (
            <span className={`px-2 py-1 rounded-full text-xs font-medium ${badges[fileType] || 'bg-gray-100 text-gray-700'}`}>
                {labels[fileType] || 'سایر'}
            </span>
        );
    };
    
    return (
        <div className="min-h-screen bg-gray-50" dir="rtl">
            {/* Header */}
            <div className="bg-white shadow-sm border-b">
                <div className="max-w-7xl mx-auto px-4 py-4">
                    <div className="flex justify-between items-center">
                        <h1 className="text-2xl font-bold text-gray-800">مدیریت فایل‌ها</h1>
                        <button
                            onClick={() => setIsUploadModalOpen(true)}
                            className="flex items-center gap-2 bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition-colors"
                        >
                            <Upload className="w-5 h-5" />
                            آپلود جدید
                        </button>
                    </div>
                </div>
            </div>
            
            {/* Statistics Cards */}
            {statistics && (
                <div className="max-w-7xl mx-auto px-4 py-6">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                        <div className="bg-white rounded-lg shadow p-4">
                            <p className="text-sm text-gray-600">تعداد کل فایل‌ها</p>
                            <p className="text-2xl font-bold text-gray-800">{statistics.totalFiles}</p>
                        </div>
                        <div className="bg-white rounded-lg shadow p-4">
                            <p className="text-sm text-gray-600">حجم کل</p>
                            <p className="text-2xl font-bold text-gray-800">{statistics.totalSizeFormatted}</p>
                        </div>
                        <div className="bg-white rounded-lg shadow p-4">
                            <p className="text-sm text-gray-600">تصاویر</p>
                            <p className="text-2xl font-bold text-blue-600">{statistics.filesByType?.images || 0}</p>
                        </div>
                        <div className="bg-white rounded-lg shadow p-4">
                            <p className="text-sm text-gray-600">ویدئوها</p>
                            <p className="text-2xl font-bold text-green-600">{statistics.filesByType?.videos || 0}</p>
                        </div>
                    </div>
                </div>
            )}
            
            {/* Filters */}
            <div className="max-w-7xl mx-auto px-4 py-4">
                <div className="bg-white rounded-lg shadow p-4">
                    <div className="flex flex-wrap gap-4 items-center justify-between">
                        <div className="flex gap-2">
                            <button
                                onClick={() => setViewMode('grid')}
                                className={`p-2 rounded-lg transition-colors ${
                                    viewMode === 'grid' ? 'bg-blue-500 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                                }`}
                            >
                                <Grid className="w-5 h-5" />
                            </button>
                            <button
                                onClick={() => setViewMode('list')}
                                className={`p-2 rounded-lg transition-colors ${
                                    viewMode === 'list' ? 'bg-blue-500 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                                }`}
                            >
                                <List className="w-5 h-5" />
                            </button>
                        </div>
                        
                        <div className="flex-1 max-w-md">
                            <div className="relative">
                                <Search className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                                <input
                                    type="text"
                                    placeholder="جستجوی فایل‌ها..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="w-full pr-10 pl-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                                />
                            </div>
                        </div>
                        
                        <div className="flex gap-2">
                            {['all', 'images', 'videos', 'audios'].map((type) => (
                                <button
                                    key={type}
                                    onClick={() => setSelectedType(type)}
                                    className={`px-3 py-1 rounded-lg transition-colors ${
                                        selectedType === type
                                            ? 'bg-blue-500 text-white'
                                            : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                                    }`}
                                >
                                    {type === 'all' ? 'همه' : type === 'images' ? 'تصاویر' : type === 'videos' ? 'ویدئوها' : 'صوت‌ها'}
                                </button>
                            ))}
                        </div>
                        
                        <button
                            onClick={loadFiles}
                            className="p-2 rounded-lg bg-gray-100 text-gray-600 hover:bg-gray-200 transition-colors"
                        >
                            <RefreshCw className="w-5 h-5" />
                        </button>
                    </div>
                </div>
            </div>
            
            {/* File Grid/List */}
            <div className="max-w-7xl mx-auto px-4 py-6">
                {loading ? (
                    <div className="flex justify-center items-center py-12">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
                    </div>
                ) : files.length === 0 ? (
                    <div className="bg-white rounded-lg shadow p-12 text-center">
                        <File className="w-16 h-16 mx-auto text-gray-400 mb-4" />
                        <p className="text-gray-600">هیچ فایلی یافت نشد</p>
                        <button
                            onClick={() => setIsUploadModalOpen(true)}
                            className="mt-4 text-blue-500 hover:text-blue-600"
                        >
                            اولین فایل خود را آپلود کنید
                        </button>
                    </div>
                ) : viewMode === 'grid' ? (
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                        {files.map((file) => (
                            <div key={file.id} className="bg-white rounded-lg shadow hover:shadow-lg transition-shadow">
                                {/* File Preview */}
                                <div className="aspect-video bg-gray-100 rounded-t-lg overflow-hidden relative group">
                                    {file.fileType === 'images' ? (
                                        <img
                                            src={file.url}
                                            alt={file.title}
                                            className="w-full h-full object-cover"
                                        />
                                    ) : (
                                        <div className="w-full h-full flex items-center justify-center">
                                            {getFileTypeIcon(file.fileType)}
                                            <span className="mr-2 text-gray-600">{file.fileName}</span>
                                        </div>
                                    )}
                                    
                                    {/* Action Buttons Overlay */}
                                    <div className="absolute inset-0 bg-black bg-opacity-50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                                        <a
                                            href={file.url}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="p-2 bg-white rounded-lg hover:bg-gray-100 transition-colors"
                                        >
                                            <Eye className="w-5 h-5 text-gray-700" />
                                        </a>
                                        <a
                                            href={file.url}
                                            download
                                            className="p-2 bg-white rounded-lg hover:bg-gray-100 transition-colors"
                                        >
                                            <Download className="w-5 h-5 text-gray-700" />
                                        </a>
                                        <button
                                            onClick={() => {
                                                setEditingFile(file.id);
                                                setEditTitle(file.title);
                                            }}
                                            className="p-2 bg-white rounded-lg hover:bg-gray-100 transition-colors"
                                        >
                                            <Edit2 className="w-5 h-5 text-gray-700" />
                                        </button>
                                        <button
                                            onClick={() => handleDeleteFile(file.id)}
                                            className="p-2 bg-red-500 rounded-lg hover:bg-red-600 transition-colors"
                                        >
                                            <Trash2 className="w-5 h-5 text-white" />
                                        </button>
                                    </div>
                                </div>
                                
                                {/* File Info */}
                                <div className="p-3">
                                    {editingFile === file.id ? (
                                        <div className="flex gap-2">
                                            <input
                                                type="text"
                                                value={editTitle}
                                                onChange={(e) => setEditTitle(e.target.value)}
                                                className="flex-1 px-2 py-1 border border-gray-300 rounded-lg text-sm"
                                                autoFocus
                                            />
                                            <button
                                                onClick={() => handleUpdateFile(file.id)}
                                                className="p-1 bg-green-500 text-white rounded-lg hover:bg-green-600"
                                            >
                                                <Check className="w-4 h-4" />
                                            </button>
                                            <button
                                                onClick={() => setEditingFile(null)}
                                                className="p-1 bg-gray-500 text-white rounded-lg hover:bg-gray-600"
                                            >
                                                <X className="w-4 h-4" />
                                            </button>
                                        </div>
                                    ) : (
                                        <h3 className="font-medium text-gray-800 truncate">{file.title}</h3>
                                    )}
                                    <div className="flex items-center justify-between mt-2">
                                        {getFileTypeBadge(file.fileType)}
                                        <span className="text-xs text-gray-500">{file.fileSizeFormatted}</span>
                                    </div>
                                    {file.uploadedBy && (
                                        <div className="mt-2 text-xs text-gray-500">
                                            ارسال شده توسط: {file.uploadedBy.fullName}
                                        </div>
                                    )}
                                    <div className="mt-1 text-xs text-gray-400">
                                        {new Date(file.dateInsert).toLocaleDateString('fa-IR')}
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                ) : (
                    <div className="bg-white rounded-lg shadow overflow-hidden">
                        <table className="w-full">
                            <thead className="bg-gray-50 border-b">
                                <tr>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">پیش‌نمایش</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">عنوان</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">نوع</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">حجم</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">ارسال کننده</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">تاریخ</th>
                                    <th className="px-4 py-3 text-right text-sm font-medium text-gray-600">عملیات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y">
                                {files.map((file) => (
                                    <tr key={file.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-4 py-3">
                                            <div className="w-10 h-10 bg-gray-100 rounded flex items-center justify-center">
                                                {file.fileType === 'images' ? (
                                                    <img
                                                        src={file.url}
                                                        alt={file.title}
                                                        className="w-full h-full object-cover rounded"
                                                    />
                                                ) : (
                                                    getFileTypeIcon(file.fileType)
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-4 py-3">
                                            {editingFile === file.id ? (
                                                <div className="flex gap-2">
                                                    <input
                                                        type="text"
                                                        value={editTitle}
                                                        onChange={(e) => setEditTitle(e.target.value)}
                                                        className="px-2 py-1 border border-gray-300 rounded-lg text-sm"
                                                        autoFocus
                                                    />
                                                    <button
                                                        onClick={() => handleUpdateFile(file.id)}
                                                        className="text-green-600 hover:text-green-700"
                                                    >
                                                        <Check className="w-4 h-4" />
                                                    </button>
                                                    <button
                                                        onClick={() => setEditingFile(null)}
                                                        className="text-gray-600 hover:text-gray-700"
                                                    >
                                                        <X className="w-4 h-4" />
                                                    </button>
                                                </div>
                                            ) : (
                                                <span className="text-sm text-gray-800">{file.title}</span>
                                            )}
                                        </td>
                                        <td className="px-4 py-3">{getFileTypeBadge(file.fileType)}</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">{file.fileSizeFormatted}</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">{file.uploadedBy?.fullName || '-'}</td>
                                        <td className="px-4 py-3 text-sm text-gray-600">
                                            {new Date(file.dateInsert).toLocaleDateString('fa-IR')}
                                        </td>
                                        <td className="px-4 py-3">
                                            <div className="flex gap-2">
                                                <a
                                                    href={file.url}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="text-blue-600 hover:text-blue-700"
                                                >
                                                    <Eye className="w-4 h-4" />
                                                </a>
                                                <a
                                                    href={file.url}
                                                    download
                                                    className="text-green-600 hover:text-green-700"
                                                >
                                                    <Download className="w-4 h-4" />
                                                </a>
                                                <button
                                                    onClick={() => {
                                                        setEditingFile(file.id);
                                                        setEditTitle(file.title);
                                                    }}
                                                    className="text-yellow-600 hover:text-yellow-700"
                                                >
                                                    <Edit2 className="w-4 h-4" />
                                                </button>
                                                <button
                                                    onClick={() => handleDeleteFile(file.id)}
                                                    className="text-red-600 hover:text-red-700"
                                                >
                                                    <Trash2 className="w-4 h-4" />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
                
                {/* Pagination */}
                {pagination.totalPages > 1 && (
                    <div className="flex justify-center gap-2 mt-6">
                        <button
                            onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber - 1 }))}
                            disabled={pagination.pageNumber === 1}
                            className="px-3 py-1 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                        >
                            قبلی
                        </button>
                        <span className="px-3 py-1">
                            صفحه {pagination.pageNumber} از {pagination.totalPages}
                        </span>
                        <button
                            onClick={() => setPagination(prev => ({ ...prev, pageNumber: prev.pageNumber + 1 }))}
                            disabled={pagination.pageNumber === pagination.totalPages}
                            className="px-3 py-1 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100"
                        >
                            بعدی
                        </button>
                    </div>
                )}
            </div>
            
            {/* Upload Modal */}
            <UploadFileModal
                isOpen={isUploadModalOpen}
                onClose={() => setIsUploadModalOpen(false)}
                onUploadSuccess={handleUploadSuccess}
                uploadPath="uploads"
            />
        </div>
    );
};

export default FileManagerPage;