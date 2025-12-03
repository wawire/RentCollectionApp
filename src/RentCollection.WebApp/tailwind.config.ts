import type { Config } from 'tailwindcss'

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['var(--font-urw-classico)', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
      },
      colors: {
        // Primary Palette - Property Market Theme
        primary: {
          DEFAULT: '#0B0B0D',   // Charcoal Black
          50: '#F5F5F6',
          100: '#E6E6E7',
          200: '#CECFD1',
          300: '#ADB0B3',
          400: '#878B8F',
          500: '#626569',
          600: '#4A4D50',
          700: '#353739',
          800: '#1F2022',
          900: '#0B0B0D',
        },
        accent: {
          DEFAULT: '#D4AF37',   // Matte Gold
          50: '#FBF8EF',
          100: '#F6EEDB',
          200: '#EFE0BC',
          300: '#E6CE93',
          400: '#DDBB69',
          500: '#D4AF37',
          600: '#B8942C',
          700: '#8F7122',
          800: '#644F18',
          900: '#3B2F0E',
        },
        surface: '#FFFFFF',     // Pure White
        muted: '#BFC2C5',       // Soft Grey
        'bg-light': '#FAF7F2',  // Ivory Tint
        'bg-dark': '#000000',   // Ultra Black
        secondary: '#DDE1E4',   // Silver

        // Semantic colors
        success: '#10b981',
        warning: '#f59e0b',
        error: '#ef4444',
        info: '#3b82f6',
      },
    },
  },
  plugins: [],
}
export default config
