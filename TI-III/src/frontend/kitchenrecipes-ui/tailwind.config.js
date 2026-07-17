/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        oat: '#e7eef8',
        ember: '#516fa0',
        pine: '#6694d8',
        slate: '#1a2432',
        amber: {
          50: '#eff4fb',
          100: '#e1ebf8',
          200: '#c6d8f1',
          300: '#a8c3ea',
          400: '#87abe0',
          500: '#6c94d4',
          600: '#5778ad',
          700: '#48628d',
          800: '#3b506f',
          900: '#32435e',
        },
      },
      fontFamily: {
        heading: ['"Bree Serif"', 'serif'],
        body: ['"Source Sans 3"', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

