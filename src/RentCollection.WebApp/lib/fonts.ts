// Font configuration for custom fonts
// INSTRUCTIONS: Copy your font files to src/RentCollection.WebApp/public/fonts/
//
// Required files:
//   - URWClassico-Reg.eot
//   - FenwickRg-Bold.eot
//   - Edmondsans-Regular.eot

import localFont from 'next/font/local'

// URWClassico - Primary font for headers and elegant text
export const urwClassico = localFont({
  src: '../public/fonts/URWClassico-Reg.eot',
  variable: '--font-urw-classico',
  weight: '400',
  style: 'normal',
  display: 'swap',
  fallback: ['Georgia', 'Cambria', 'Times New Roman', 'serif'],
})

// Fenwick - Bold display font for impact headings
export const fenwick = localFont({
  src: '../public/fonts/FenwickRg-Bold.eot',
  variable: '--font-fenwick',
  weight: '700',
  style: 'normal',
  display: 'swap',
  fallback: ['Arial Black', 'sans-serif'],
})

// Edmondsans - Clean sans-serif for body text
export const edmondsans = localFont({
  src: '../public/fonts/Edmondsans-Regular.eot',
  variable: '--font-edmondsans',
  weight: '400',
  style: 'normal',
  display: 'swap',
  fallback: ['system-ui', '-apple-system', 'sans-serif'],
})

// Font usage guide:
// - urwClassico: Property names, prices, elegant headers
// - fenwick: Large hero headings, impact statements
// - edmondsans: Body text, descriptions, UI elements

