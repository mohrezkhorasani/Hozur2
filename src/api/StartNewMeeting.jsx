import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';



export async function startNewMeet(roomID, name, title = "", teachers = [], dabir = null, idplan = -1, hasCert = false) {

    //console.log(teachers)
    let tStr = ""
    teachers.map((t) => {
        tStr += "Teachers=" + t + "&"
        //console.log(tStr)
    })
    //console.log("startNewMeeting", idplan)
    const url = combinePath(BaseURL, `/user/StartNewMeet/${roomID}?MeetTitle=${title}&Name=${name}&${dabir ? `Dabir=${dabir}` : ""}&${tStr}RequiredToEnterName=false&IDPlan=${idplan}&HasCertificate=${hasCert}`);
    //console.log(url)
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

export async function EndMeet(roomID, isTest = false) {

    const url = combinePath(BaseURL, `/user/EndTheMeetingWithAPI`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json', // تضمین نوع محتوا
        },
        body: JSON.stringify({
            friendlyID: roomID,
            type: isTest ? -99 : 0
        })
    });
    //console.log(res)

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

export async function joinMeeting(roomID, name, modir = false, pass = "123", nationalCode = null, phoneNumber = null) {
    // Encode each parameter properly
    const encodedName = encodeURIComponent(name);
    const encodedRoomID = encodeURIComponent(roomID);
    const encodedPass = encodeURIComponent(pass);
    const encodedNationalCode = nationalCode ? encodeURIComponent(nationalCode) : '';
    const encodedPhoneNumber = phoneNumber ? encodeURIComponent(phoneNumber) : '';
    
    let url = combinePath(BaseURL, `/user/GetMeetingURL?nick=${encodedName}&meetid=${encodedRoomID}&modir=${modir}&pass=${encodedPass}`);
    
    // اضافه کردن کد ملی و تلفن به URL
    if (nationalCode) {
        url += `&nationalCode=${encodedNationalCode}`;
    }
    if (phoneNumber) {
        url += `&phoneNumber=${encodedPhoneNumber}`;
    }
    
    //console.log(url);

    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    let json = await res.json();
    //console.log(json);
    return json;
}

export async function Login(uname, pass) {
    const url = combinePath(BaseURL, `/user/LoginUserPanel`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            username: uname,
            password: pass
        })
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to login");
    let json = await res.json()
    if (json.status != 200)
        throw new Error(json.message)
    return json;
}

export async function SignUp(nick, uname, pass, mail, role) {
    const url = combinePath(BaseURL, `/user/SignInUserPanel`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Name: nick,
            Email: mail,
            username: uname,
            password: pass,
            explain: role
        })
    });
    //console.log(res)
    if (!res.ok) throw new Error("Failed to login");
    let json = await res.json()
    if (json.status != 200)
        throw new Error(json.message)
    return json;
}


export async function VerifyToken() {
    const url = combinePath(BaseURL, `/user/VerifyToken`);

    const res = await fetch(url, {
        method: "POST",
        headers: {
            "accept": "*/*",
            "Content-Type": "application/json",
            "vc2AuthNorm": Cookies.get("vc2AuthNorm")
        },
        body: JSON.stringify({
        })
    });
    if (!res.ok)
        if (res.status == 401)
            return {
                status: 401
            }
        else
            throw new Error("Failed to login");
    let json = await res.json()
    return json;
}

export async function SendOTPConfirmation(data) {
    const url = combinePath(BaseURL, `/user/SendOTPConfirmation`);
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
    // if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}

export async function ConfirmOTP(data) {
    const url = combinePath(BaseURL, `/user/ConfirmOTP`);
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
    // if (json.status !== 200) throw new Error(json.message || "خطا در ایجاد sاسلایدشو");
    return json;
}

export async function SendOTPRoomSubscribedUser(data) {
    const url = combinePath(BaseURL, `/user/SendOTPRoomSubscribedUser`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to send OTP for subscribed user");
    return await res.json();
}

export async function ConfirmOTPRoomSubscribedUser(data) {
    const url = combinePath(BaseURL, `/user/ConfirmOTPRoomSubscribedUser`);
    const res = await fetch(url, { 
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to confirm OTP for subscribed user");
    return await res.json();
}

export async function SendOTPRoomSubscribedRegistration(data) {
    const url = combinePath(BaseURL, `/user/SendOTPRoomSubscribedRegistration`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to send OTP for registration");
    return await res.json();
}

export async function DirectRoomSubscribedRegistration(data) {
    const url = combinePath(BaseURL, `/user/DirectRoomSubscribedRegistration`);
    const res = await fetch(url, { 
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });
    return await res.json();
}

export async function ConfirmOTPRoomSubscribedRegistration(data) {
    const url = combinePath(BaseURL, `/user/ConfirmOTPRoomSubscribedRegistration`);
    const res = await fetch(url, { 
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error("Failed to confirm OTP for registration");
    return await res.json();
}

export async function VerifyCertificateUser(data) {
    const url = combinePath(BaseURL, `/user/VerifyCertificateUser`);
    const res = await fetch(url, {
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) throw new Error("Failed to verify certificate user");
    return await res.json();
}

export async function ConfirmCertVerification(data) {
    const url = combinePath(BaseURL, `/user/ConfirmCertVerification`);
    const res = await fetch(url, { 
        method: "POST",
        headers: {
            ...NormalHeaderAPI,
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    });

    if (!res.ok) throw new Error("Failed to confirm certificate verification");
    return await res.json();
}
