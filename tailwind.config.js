module.exports = {
  theme: {
    extend: {
      colors: {
        mainblue: '#11207A',
        mainGreen: '#BBD430',
        pistachio: {
          DEFAULT: '#84cc16',
          600: '#65a30d',
          700: '#4d7c0f',
        },
        navy: {
          DEFAULT: '#0f2b5b',
          800: '#0a1d40',
        },
      },
      animation: {
        'shimmer': 'shimmer 1.5s ease-in-out infinite',
      },
      keyframes: {
        shimmer: {
          '0%': { transform: 'translateX(-100%)' },
          '100%': { transform: 'translateX(100%)' },
        },
      },
    },
  },
  plugins: [],
}