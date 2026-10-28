// src/api/FileAPI.jsx
import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';

export async function UploadFiles(files, uploadPath = "", baseUrl = "") {
    const formData = new FormData();
    
    files.forEach(file => {
        formData.append("Files", file);
    });
    
    if (uploadPath) {
        formData.append("UploadPath", uploadPath);
    }
    
    if (baseUrl) {
        formData.append("BaseUrl", baseUrl);
    }
    
    const res = await fetch(combinePath(BaseURL, `/File/UploadFileStorage`), {
        method: "POST",
        headers: {
            "vc2AuthNorm": Cookies.get("vc2AuthNorm")
        },
        body: formData
    });
    
    if (!res.ok) throw new Error("Failed to upload files");
    const json = await res.json();
    return json;
}

export async function GetFiles(params = {}) {
    const queryParams = new URLSearchParams();
    
    if (params.fileID) queryParams.append("FileID", params.fileID);
    if (params.userID) queryParams.append("UserID", params.userID);
    if (params.fileType) queryParams.append("FileType", params.fileType);
    if (params.fromDate) queryParams.append("FromDate", params.fromDate);
    if (params.toDate) queryParams.append("ToDate", params.toDate);
    if (params.pageNumber) queryParams.append("PageNumber", params.pageNumber);
    if (params.pageSize) queryParams.append("PageSize", params.pageSize);
    if (params.searchTerm) queryParams.append("SearchTerm", params.searchTerm);
    if (params.sortBy) queryParams.append("SortBy", params.sortBy);
    if (params.sortDescending !== undefined) queryParams.append("SortDescending", params.sortDescending);
    
    const res = await fetch(combinePath(BaseURL, `/File/GetFileStorage?${queryParams}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to fetch files");
    const json = await res.json();
    //console.log(json)

    return json;
}

export async function DeleteFile(fileID, permanentDelete = false) {
    const res = await fetch(combinePath(BaseURL, `/File/DeleteFile`), {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            fileID: fileID,
            permanentDelete: permanentDelete
        })
    });
    
    if (!res.ok) throw new Error("Failed to delete file");
    const json = await res.json();
    return json;
}

export async function UpdateFileInfo(fileID, newTitle, newDescription, status) {
    const res = await fetch(combinePath(BaseURL, `/File/UpdateFileInfo`), {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            fileID: fileID,
            newTitle: newTitle,
            newDescription: newDescription,
            status: status
        })
    });
    
    if (!res.ok) throw new Error("Failed to update file info");
    const json = await res.json();
    return json;
}

export async function GetFileStatistics(params = {}) {
    const queryParams = new URLSearchParams();
    
    if (params.userID) queryParams.append("UserID", params.userID);
    if (params.fromDate) queryParams.append("FromDate", params.fromDate);
    if (params.toDate) queryParams.append("ToDate", params.toDate);
    
    const res = await fetch(combinePath(BaseURL, `/File/GetFileStatistics?${queryParams}`), {
        method: "GET",
        headers: NormalHeaderAPI
    });
    
    if (!res.ok) throw new Error("Failed to fetch statistics");
    const json = await res.json();
    return json;
}