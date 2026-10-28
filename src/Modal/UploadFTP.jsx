// src/components/FileManager/UploadFileModal.jsx - نسخه با تکه‌تکه آپلود کردن
import React, { useState, useCallback, useRef } from 'react';
import { Upload, X, File, Image, Video, Music, CheckCircle, AlertCircle, Gauge, Zap } from 'lucide-react';
import { combinePath } from '../Tools/tools';
import { BaseURL } from '../api/apiNeed';
import Cookies from 'js-cookie';

const UploadFileModal = ({ isOpen, onClose, onUploadSuccess, uploadPath = "" }) => {
    const [files, setFiles] = useState([]);
    const [uploading, setUploading] = useState(false);
    const [uploadProgress, setUploadProgress] = useState({});
    const [uploadSpeed, setUploadSpeed] = useState({});
    const [remainingTime, setRemainingTime] = useState({});
    const [dragActive, setDragActive] = useState(false);
    
    // تنظیمات بهینه برای سرعت بالا
    const CHUNK_SIZE = 5 * 1024 * 1024; // 5MB per chunk (برای پهنای باند معمولی)
    // const CHUNK_SIZE = 10 * 1024 * 1024; // 10MB for high bandwidth
    const MAX_CONCURRENT_UPLOADS = 3; // تعداد آپلود همزمان
    
    const allowedTypes = ['image/', 'video/', 'audio/'];
    const maxFileSize = 2 * 1024 * 1024 * 1024; // 2GB
    
    const handleDrag = useCallback((e) => {
        e.preventDefault();
        e.stopPropagation();
        if (e.type === "dragenter" || e.type === "dragover") {
            setDragActive(true);
        } else if (e.type === "dragleave") {
            setDragActive(false);
        }
    }, []);
    
    const validateFiles = (fileList) => {
        const validFiles = [];
        const errors = [];
        
        Array.from(fileList).forEach(file => {
            const isValidType = allowedTypes.some(type => file.type.startsWith(type));
            const isValidSize = file.size <= maxFileSize;
            
            if (isValidType && isValidSize) {
                validFiles.push(file);
            } else {
                errors.push({
                    name: file.name,
                    reason: !isValidType ? "فرمت فایل پشتیبانی نمی‌شود" : "حجم فایل بیشتر از 2GB است"
                });
            }
        });
        
        return { validFiles, errors };
    };
    
    const handleDrop = useCallback((e) => {
        e.preventDefault();
        e.stopPropagation();
        setDragActive(false);
        
        const { validFiles, errors } = validateFiles(e.dataTransfer.files);
        
        if (errors.length > 0) {
            console.error("Invalid files:", errors);
        }
        
        setFiles(prev => [...prev, ...validFiles]);
    }, []);
    
    const handleFileSelect = (e) => {
        const { validFiles, errors } = validateFiles(e.target.files);
        
        if (errors.length > 0) {
            console.error("Invalid files:", errors);
        }
        
        setFiles(prev => [...prev, ...validFiles]);
    };
    
    const removeFile = (index) => {
        setFiles(prev => prev.filter((_, i) => i !== index));
        setUploadProgress(prev => {
            const newProgress = { ...prev };
            delete newProgress[index];
            return newProgress;
        });
        setUploadSpeed(prev => {
            const newSpeed = { ...prev };
            delete newSpeed[index];
            return newSpeed;
        });
        setRemainingTime(prev => {
            const newTime = { ...prev };
            delete newTime[index];
            return newTime;
        });
    };
    
    // آپلود تکه‌تکه با قابلیت resume
    const uploadChunk = async (file, chunkIndex, totalChunks, chunk, fileId, index) => {
        const formData = new FormData();
        formData.append("file", chunk);
        formData.append("fileId", fileId);
        formData.append("chunkIndex", chunkIndex);
        formData.append("totalChunks", totalChunks);
        formData.append("fileName", file.name);
        formData.append("uploadPath", uploadPath);
        
        return new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            
            xhr.upload.addEventListener("progress", (event) => {
                if (event.lengthComputable) {
                    // محاسبه پیشرفت کلی فایل
                    const chunkProgress = (chunkIndex + (event.loaded / event.total)) / totalChunks;
                    const totalProgress = Math.round(((chunkIndex + (event.loaded / event.total)) / totalChunks) * 100);
                    setUploadProgress(prev => ({ ...prev, [index]: totalProgress }));
                    
                    // محاسبه سرعت
                    if (uploadSpeed[index]) {
                        // به‌روزرسانی سرعت
                    }
                }
            });
            
            xhr.addEventListener("load", () => {
                if (xhr.status === 200) {
                    resolve(JSON.parse(xhr.responseText));
                } else {
                    reject(new Error(`Chunk ${chunkIndex} failed`));
                }
            });
            
            xhr.addEventListener("error", () => reject(new Error(`Chunk ${chunkIndex} error`)));
            
            xhr.open("POST", combinePath(BaseURL, `/File/UploadChunk`));
            xhr.setRequestHeader("vc2AuthNorm", Cookies.get("vc2AuthNorm"));
            xhr.send(formData);
        });
    };
    
    const uploadFileInChunks = async (file, index) => {
        const fileId = `${Date.now()}_${file.name}_${file.size}`;
        const totalChunks = Math.ceil(file.size / CHUNK_SIZE);
        
        // آپلود همزمان چند تکه
        const uploadPromises = [];
        for (let chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++) {
            const start = chunkIndex * CHUNK_SIZE;
            const end = Math.min(start + CHUNK_SIZE, file.size);
            const chunk = file.slice(start, end);
            
            // محدود کردن تعداد آپلودهای همزمان
            if (uploadPromises.length >= MAX_CONCURRENT_UPLOADS) {
                await Promise.race(uploadPromises);
                // حذف promises تکمیل شده
                const completed = uploadPromises.filter(p => p.status === 'fulfilled');
                completed.forEach(p => {
                    const index = uploadPromises.indexOf(p);
                    if (index > -1) uploadPromises.splice(index, 1);
                });
            }
            
            const promise = uploadChunk(file, chunkIndex, totalChunks, chunk, fileId, index)
                .then(result => ({ status: 'fulfilled', result }))
                .catch(error => ({ status: 'rejected', error }));
            
            uploadPromises.push(promise);
        }
        
        // منتظر موندن برای اتمام همه تکه‌ها
        const results = await Promise.all(uploadPromises);
        const failed = results.filter(r => r.status === 'rejected');
        
        if (failed.length > 0) {
            throw new Error(`${failed.length} chunks failed`);
        }
        
        // درخواست نهایی برای ترکیب تکه‌ها
        const mergeResponse = await fetch(combinePath(BaseURL, `/File/MergeChunks`), {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "vc2AuthNorm": Cookies.get("vc2AuthNorm")
            },
            body: JSON.stringify({
                fileId: fileId,
                fileName: file.name,
                totalChunks: totalChunks,
                uploadPath: uploadPath
            })
        });
        
        return await mergeResponse.json();
    };
    
    // آپلود معمولی با بهینه‌سازی (نسخه ساده‌تر)
    const uploadFileOptimized = async (file, index) => {
        return new Promise((resolve, reject) => {
            const formData = new FormData();
            formData.append("Files", file);
            if (uploadPath) {
                formData.append("UploadPath", uploadPath);
            }
            
            const xhr = new XMLHttpRequest();
            let startTime = Date.now();
            let lastLoaded = 0;
            let lastTime = startTime;
            
            // افزایش timeout برای فایل‌های بزرگ
            xhr.timeout = 30 * 60 * 1000; // 30 minutes
            
            xhr.upload.addEventListener("progress", (event) => {
                if (event.lengthComputable) {
                    const percentComplete = Math.round((event.loaded / event.total) * 100);
                    setUploadProgress(prev => ({ ...prev, [index]: percentComplete }));
                    
                    // محاسبه سرعت لحظه‌ای
                    const now = Date.now();
                    const timeDiff = (now - lastTime) / 1000;
                    const bytesDiff = event.loaded - lastLoaded;
                    
                    if (timeDiff >= 0.5) {
                        const instantSpeed = bytesDiff / timeDiff;
                        setUploadSpeed(prev => ({ ...prev, [index]: instantSpeed }));
                        
                        const remainingBytes = event.total - event.loaded;
                        const eta = remainingBytes / instantSpeed;
                        setRemainingTime(prev => ({ ...prev, [index]: eta }));
                        
                        lastLoaded = event.loaded;
                        lastTime = now;
                    }
                }
            });
            
            xhr.addEventListener("load", () => {
                if (xhr.status === 200) {
                    const totalTime = (Date.now() - startTime) / 1000;
                    const avgSpeed = file.size / totalTime;
                    console.log(`Average speed: ${(avgSpeed / 1024 / 1024).toFixed(2)} MB/s`);
                    resolve(JSON.parse(xhr.responseText));
                } else {
                    reject(new Error(`Upload failed with status ${xhr.status}`));
                }
            });
            
            xhr.addEventListener("error", () => reject(new Error("Upload failed")));
            xhr.addEventListener("timeout", () => reject(new Error("Upload timeout")));
            
            xhr.open("POST", combinePath(BaseURL, `/File/UploadFileStorage`));
            xhr.setRequestHeader("vc2AuthNorm", Cookies.get("vc2AuthNorm"));
            xhr.send(formData);
        });
    };
    
    const handleUpload = async () => {
        if (files.length === 0) return;
        
        setUploading(true);
        const results = [];
        
        // آپلود موازی با محدودیت تعداد همزمان
        const uploadQueue = [...files];
        const activeUploads = new Map();
        
        const uploadNext = async (index) => {
            if (uploadQueue.length === 0) return;
            const file = uploadQueue.shift();
            const fileIndex = files.indexOf(file);
            
            try {
                // انتخاب روش آپلود (برای فایل‌های بزرگ از تکه‌تکه استفاده کن)
                let result;
                if (file.size > 100 * 1024 * 1024) { // فایل بزرگتر از 100MB
                    result = await uploadFileInChunks(file, fileIndex);
                } else {
                    result = await uploadFileOptimized(file, fileIndex);
                }
                results.push(result);
            } catch (error) {
                console.error(`Error uploading ${file.name}:`, error);
                setUploadProgress(prev => ({ ...prev, [fileIndex]: -1 }));
            }
            
            // آپلود فایل بعدی
            await uploadNext(index);
        };
        
        // شروع آپلودهای همزمان
        const initialUploads = [];
        for (let i = 0; i < Math.min(MAX_CONCURRENT_UPLOADS, files.length); i++) {
            initialUploads.push(uploadNext(i));
        }
        
        await Promise.all(initialUploads);
        
        setUploading(false);
        
        if (results.length > 0 && onUploadSuccess) {
            onUploadSuccess(results);
        }
        
        setTimeout(() => {
            onClose();
            setFiles([]);
            setUploadProgress({});
            setUploadSpeed({});
            setRemainingTime({});
        }, 1500);
    };
    
    const getFileIcon = (file) => {
        if (file.type.startsWith('image/')) return <Image className="w-8 h-8 text-blue-500" />;
        if (file.type.startsWith('video/')) return <Video className="w-8 h-8 text-green-500" />;
        if (file.type.startsWith('audio/')) return <Music className="w-8 h-8 text-purple-500" />;
        return <File className="w-8 h-8 text-gray-500" />;
    };
    
    const formatFileSize = (bytes) => {
        if (bytes >= 1024 * 1024 * 1024) return `${(bytes / 1024 / 1024 / 1024).toFixed(2)} GB`;
        if (bytes >= 1024 * 1024) return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
        if (bytes >= 1024) return `${(bytes / 1024).toFixed(2)} KB`;
        return `${bytes} B`;
    };
    
    const formatSpeed = (bytesPerSecond) => {
        if (bytesPerSecond >= 1024 * 1024) {
            return `${(bytesPerSecond / 1024 / 1024).toFixed(1)} MB/s`;
        } else if (bytesPerSecond >= 1024) {
            return `${(bytesPerSecond / 1024).toFixed(1)} KB/s`;
        }
        return `${bytesPerSecond.toFixed(0)} B/s`;
    };
    
    const formatRemainingTime = (seconds) => {
        if (!isFinite(seconds) || seconds <= 0) return '--';
        if (seconds < 60) return `${Math.ceil(seconds)} ثانیه`;
        if (seconds < 3600) return `${Math.floor(seconds / 60)} دقیقه ${Math.ceil(seconds % 60)} ثانیه`;
        return `${Math.floor(seconds / 3600)} ساعت ${Math.floor((seconds % 3600) / 60)} دقیقه`;
    };
    
    if (!isOpen) return null;
    
    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-3xl mx-4 max-h-[90vh] flex flex-col">
                <div className="flex justify-between items-center p-4 border-b">
                    <h2 className="text-xl font-bold text-gray-800">آپلود فایل جدید</h2>
                    <button onClick={onClose} className="text-gray-500 hover:text-gray-700" disabled={uploading}>
                        <X className="w-6 h-6" />
                    </button>
                </div>
                
                <div className="flex-1 overflow-y-auto p-4">
                    {/* Drag & Drop Area */}
                    <div
                        className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
                            dragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300 bg-gray-50'
                        }`}
                        onDragEnter={handleDrag}
                        onDragLeave={handleDrag}
                        onDragOver={handleDrag}
                        onDrop={handleDrop}
                    >
                        <Upload className="w-12 h-12 mx-auto text-gray-400 mb-4" />
                        <p className="text-gray-600 mb-2">فایل‌های خود را اینجا بکشید و رها کنید</p>
                        <p className="text-sm text-gray-500 mb-4">یا</p>
                        <label className="bg-blue-500 text-white px-4 py-2 rounded-lg cursor-pointer hover:bg-blue-600 inline-block">
                            انتخاب فایل از کامپیوتر
                            <input type="file" multiple accept="image/*,video/*,audio/*" onChange={handleFileSelect} className="hidden" disabled={uploading} />
                        </label>
                        <p className="text-xs text-gray-400 mt-4">
                            فرمت‌های مجاز: تصاویر، ویدئوها، فایل‌های صوتی | حداکثر حجم: 2GB
                        </p>
                    </div>
                    
                    {/* File List */}
                    {files.length > 0 && (
                        <div className="mt-6 space-y-3">
                            <div className="flex justify-between items-center">
                                <h3 className="font-semibold text-gray-700">فایل‌های انتخاب شده ({files.length})</h3>
                                <div className="flex items-center gap-2 text-xs text-green-600 bg-green-50 px-2 py-1 rounded">
                                    <Zap className="w-4 h-4" />
                                    <span>آپلود همزمان: {MAX_CONCURRENT_UPLOADS} فایل</span>
                                </div>
                            </div>
                            
                            {files.map((file, index) => (
                                <div key={index} className="bg-gray-50 rounded-lg p-3">
                                    <div className="flex items-center gap-3">
                                        {getFileIcon(file)}
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium text-gray-800 truncate">{file.name}</p>
                                            <p className="text-xs text-gray-500">{formatFileSize(file.size)}</p>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            {uploadProgress[index] !== undefined && uploadProgress[index] !== -1 && uploadProgress[index] !== 100 && (
                                                <div className="flex items-center gap-2 text-xs">
                                                    <Gauge className="w-4 h-4 text-gray-500" />
                                                    <span className="font-medium text-blue-600">{formatSpeed(uploadSpeed[index] || 0)}</span>
                                                    {remainingTime[index] > 0 && (
                                                        <span className="text-gray-500">({formatRemainingTime(remainingTime[index])})</span>
                                                    )}
                                                </div>
                                            )}
                                            {uploadProgress[index] !== undefined ? (
                                                uploadProgress[index] === -1 ? (
                                                    <AlertCircle className="w-5 h-5 text-red-500" />
                                                ) : uploadProgress[index] === 100 ? (
                                                    <CheckCircle className="w-5 h-5 text-green-500" />
                                                ) : (
                                                    <div className="w-20">
                                                        <div className="bg-gray-200 rounded-full h-2">
                                                            <div className="bg-blue-500 rounded-full h-2 transition-all" style={{ width: `${uploadProgress[index]}%` }} />
                                                        </div>
                                                        <p className="text-xs text-gray-600 mt-1 text-center">{uploadProgress[index]}%</p>
                                                    </div>
                                                )
                                            ) : (
                                                !uploading && (
                                                    <button onClick={() => removeFile(index)} className="text-gray-400 hover:text-red-500">
                                                        <X className="w-5 h-5" />
                                                    </button>
                                                )
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
                
                <div className="border-t p-4 flex justify-end gap-3">
                    <button onClick={onClose} className="px-4 py-2 text-gray-600" disabled={uploading}>انصراف</button>
                    <button
                        onClick={handleUpload}
                        disabled={files.length === 0 || uploading}
                        className={`px-6 py-2 rounded-lg transition-colors flex items-center gap-2 ${
                            files.length === 0 || uploading ? 'bg-gray-300 cursor-not-allowed' : 'bg-blue-500 hover:bg-blue-600 text-white'
                        }`}
                    >
                        {uploading ? (
                            <>
                                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                                در حال آپلود...
                            </>
                        ) : (
                            'آپلود'
                        )}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default UploadFileModal;