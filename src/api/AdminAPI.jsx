import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';



export async function GetUserList(page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/AdminGetUserList?page=${page - 1}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function ChangeRole(userID, roleNumber) {
    const url = combinePath(BaseURL, `/user/AdminChangeRole`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify({
            Role: roleNumber,   // توجه: با حرف بزرگ R
            UserId: userID      // توجه: با حرف بزرگ U
        })
    });

    if (!res.ok) {
        const errorText = await res.text();
        throw new Error(`Failed to change role: ${res.status} ${errorText}`);
    }

    return await res.json();
}

export async function ChangeStatus(userID, status) {
    const url = combinePath(BaseURL, `/user/AdminChangeStatus`);

    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI,
        body: JSON.stringify({
            status: status, // masalan 100 = Enabled -100 = disable
            //              , Pin = 999
            //  , Disable = -100
            //  , Canceled = -101
            //  , Delete = -400
            //  , Forbidden = -403
            //  , Ban = -403 ,

            userId: userID
        })
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

export async function GetRoomListOfUser(uid, page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/AdminGetUserRoomList?uid=${uid}&page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    console.log(res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}
export async function RemoveAdminGetUserRoomList(uid, roomID) {
    const url = combinePath(BaseURL, `/user/RemoveAdminGetUserRoomList?uid=${uid}&rid=${roomID}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function AdminCreateRoom(roomData) {
    const url = combinePath(BaseURL, `/user/AdminCreateRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(roomData)
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}


export async function NewPlanRoom(data) {
    console.log(data)
    const url = combinePath(BaseURL, `/user/NewPlanRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اتاق");
    return json;
}
export async function DeletePlanRoom(idplan) {
    console.log(`Deleting plan room with ID: ${idplan}`);
    const url = combinePath(BaseURL, `/user/DeletePlanRoom/${idplan}`);
    const res = await fetch(url, {
        method: "DELETE",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
    });
    if (!res.ok) throw new Error("Failed to delete plan room");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در حذف جلسه");
    return json;
}


export async function GetPlanRoom(uid, page, count, order = 100) {
    const url = combinePath(BaseURL, `/user/GetPlanRoom?friendlyID=${uid}&page=${page}&count=${count}&o=${order}`);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });
    console.log("getplans", res)
    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}


export async function InsertSlideShow(formData) {
    const url = combinePath(BaseURL, `/user/InsertSlideShow`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            // حذف 'Content-Type' تا مرورگر خودکار آن را برای FormData تنظیم کند
        },
        body: formData // ارسال مستقیم FormData
    });
    if (!res.ok) throw new Error("Failed to create slideshow");
    const json = await res.json();
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد اسلایدشو");
    return json;
}

export async function EditSlideShow(data) {
    console.log(data)
    const url = combinePath(BaseURL, `/user/EditSlideShow`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    console.log(res)

    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    console.log(json)
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}



export async function ChangeAvatarOfRoom(data) {
    console.log(data)
    const url = combinePath(BaseURL, `/user/ChangeAvatarOfRoom`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify(data)
    });
    console.log(res)

    if (!res.ok) throw new Error("Failed to create room");
    const json = await res.json();
    console.log(json)
    if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}