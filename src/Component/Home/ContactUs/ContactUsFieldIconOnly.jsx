// ContactUsFieldIconOnly.jsx
export default function ContactUsFieldIconOnly({ svgPath, href, className }) {
  return (
    <div className="flex items-center p-1 rounded-lg">
      <a href={href}>
        <div
          className={`
            shrink-0
            bg-gray-100
            rounded-md
            w-10 h-10
            flex items-center justify-center
            transition-all duration-300
            text-[#0b3f32]
            hover:text-white hover:fill-white
            cursor-pointer
            ${className || ""}
          `}
        >
          <svg
            aria-hidden="true"
            className="w-5 h-5"
            viewBox="0 0 512 512" 
            xmlns="http://www.w3.org/2000/svg"
            fill="currentColor"
          >
            <path d={svgPath} />
          </svg>
        </div>
      </a>
    </div>
  );
}