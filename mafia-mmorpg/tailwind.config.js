/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts,css}",
    "./src/**/*.component.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        mafia: {
          primary: '#1a1a1a',
          secondary: '#2d2d2d',
          accent: '#8b0000',
          gold: '#ffd700',
          silver: '#c0c0c0',
          bronze: '#cd7f32'
        }
      },
      fontFamily: {
        'mafia': ['Cinzel', 'serif'],
        'modern': ['Inter', 'sans-serif']
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}
