import { useState, useEffect } from "react";
import Cookies from "js-cookie";
import MobileModal from "./MobileModal";
import CodeModal from "./CodeModal";

export default function LoginModal({ dismiss }) {
  const [showMobile, setShowMobile] = useState(false);
  const [showCode, setShowCode] = useState(false);
  const [mobile, setMobile] = useState("");

  useEffect(() => {
    if (!Cookies.get("t1")) {
      setShowMobile(true);
    }
  }, []);

  const handleMobileNext = (num) => {
    // setMobile(num);
    // setShowMobile(false);
    // setShowCode(true);

    alert("ثبت نام در حال حاضر غیر فعال است")
    dismiss() 
  };

  const handleSuccess = () => {
    // setShowCode(false);
    alert("ثبت نام در حال حاضر غیر فعال است")
    dismiss()
  };

  return (
    <>
      {showMobile && <MobileModal onNext={handleMobileNext} dismiss={dismiss}/>}
      {showCode && <CodeModal mobile={mobile} onSuccess={handleSuccess} />}
    </>
  );
}