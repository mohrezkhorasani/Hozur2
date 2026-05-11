export default function ContactusField({ title, svgPath, href }) {
  return (
    <div className="flex items-center gap-4 p-4 rounded-lg">
      {/* آیکون */}
      <div
        className="
          flex-shrink-0
          bg-gray-100
          rounded-md
          text-[#0b3f32]
          w-8 h-8
          flex items-center justify-center
        "
      >
        <svg
          aria-hidden="true"
          className="w-4 h-4"
          viewBox="0 0 512 512"
          xmlns="http://www.w3.org/2000/svg"
          fill="currentColor"
        >
          <path d={svgPath} />
        </svg>
      </div>

      {/* متن */}
      <div>
        <a
          href={href}
          className="
            text-sm font-semibold text-gray-800
            justify-center items-center
          "
        >
          {title}
        </a>
      </div>
    </div>
  );
}
