import Cookies from 'js-cookie';
import { BaseURL, NormalHeaderAPI } from './apiNeed';
import { combinePath } from '../Tools/tools';



export async function startNewMeet(roomID, name, title = "",teachers=[],dabir=null) {
    let tStr= ""
    teachers.map((t)=>{
        tStr+="Teachers="+t+"&"
        console.log(tStr)
    })
    const url = combinePath(BaseURL, `/user/StartNewMeet/${roomID}?MeetTitle=${title}&Name=${name}&${tStr}RequiredToEnterName=false`);
    console.log(url)
    const res = await fetch(url, {
        method: "POST",
        headers: NormalHeaderAPI
    });

    if (!res.ok) throw new Error("Failed to start meet");

    return await res.json();
}

export async function joinMeeting(roomID, name, modir = false, pass = "123") {
    // Encode each parameter properly
    const encodedName = encodeURIComponent(name);
    const encodedRoomID = encodeURIComponent(roomID);
    const encodedPass = encodeURIComponent(pass);
    // modir is boolean, no need to encode (safe as true/false)
    
    const url = combinePath(BaseURL, `/user/GetMeetingURL?nick=${encodedName}&meetid=${encodedRoomID}&modir=${modir}&pass=${encodedPass}`);
    console.log(url);
    
    const res = await fetch(url, {
        method: "GET",
        headers: NormalHeaderAPI   // fixed the duplicate 'headers' property
    });
    
    if (!res.ok) throw new Error("Failed to start meet");
    
    let json = await res.json();
    console.log(json);
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
    console.log(res)
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
    console.log(res)
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
