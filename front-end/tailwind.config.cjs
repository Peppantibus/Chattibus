/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        night: '#0b1120',
        surface: 'rgba(15, 23, 42, 0.8)',
        accent: {
          DEFAULT: '#f97316',
          soft: '#fb923c'
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif']
      },
      boxShadow: {
        glow: '0 25px 50px -12px rgba(251, 146, 60, 0.25)'
      }
    }
  },
  plugins: []
};
