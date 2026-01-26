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
        sans: ['var(--font-edmondsans)', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
        serif: ['var(--font-urw-classico)', 'Georgia', 'Cambria', 'Times New Roman', 'serif'],
        display: ['var(--font-fenwick)', 'Arial Black', 'sans-serif'],
      },
      colors: {
        brand: {
          primary: 'var(--color-brand-primary)',
          secondary: 'var(--color-brand-secondary)',
          bg: 'var(--color-brand-bg)',
        },
        primary: {
          DEFAULT: 'var(--color-brand-primary)',
          50: 'var(--color-brand-bg)',
          100: 'var(--color-brand-bg)',
          600: 'var(--color-brand-primary)',
          700: 'var(--color-brand-secondary)',
        },
        accent: {
          DEFAULT: 'var(--color-brand-secondary)',
          50: 'var(--color-brand-bg)',
          600: 'var(--color-brand-primary)',
        },
        secondary: 'var(--color-brand-bg)',
        muted: 'var(--color-text-muted)',
        'bg-light': 'var(--color-brand-bg)',
        text: {
          primary: 'var(--color-text-primary)',
          secondary: 'var(--color-text-secondary)',
          muted: 'var(--color-text-muted)',
        },
        surface: {
          DEFAULT: 'var(--color-surface)',
          raised: 'var(--color-surface-raised)',
        },
        border: {
          muted: 'var(--color-border-muted)',
        },
        state: {
          success: 'var(--color-state-success)',
          warning: 'var(--color-state-warning)',
          error: 'var(--color-state-error)',
          info: 'var(--color-state-info)',
        },
      },
      boxShadow: {
        subtle: 'var(--shadow-subtle)',
      },
    },
  },
  plugins: [],
}
export default config
