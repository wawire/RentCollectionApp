// Font configuration for custom fonts
// Place your font files in: public/fonts/
//
// For URWClassico family:
//   - URWClassico-Regular.ttf (or .eot, .woff, .woff2)
//   - URWClassico-Italic.ttf
//   - URWClassico-Bold.ttf
//   - URWClassico-BoldItalic.ttf
//
// For alternative fonts:
//   - URWClassico-Reg.eot
//   - FenwickRg-Bold.eot
//   - Edmondsans-Regular.eot

import localFont from 'next/font/local'

// Uncomment and configure after adding your font files to public/fonts/
/*
export const urwClassico = localFont({
  src: [
    {
      path: '../public/fonts/URWClassico-Regular.ttf',
      weight: '400',
      style: 'normal',
    },
    {
      path: '../public/fonts/URWClassico-Italic.ttf',
      weight: '400',
      style: 'italic',
    },
    {
      path: '../public/fonts/URWClassico-Bold.ttf',
      weight: '700',
      style: 'normal',
    },
    {
      path: '../public/fonts/URWClassico-BoldItalic.ttf',
      weight: '700',
      style: 'italic',
    },
  ],
  variable: '--font-urw-classico',
  display: 'swap',
})
*/

// Temporary fallback - using system fonts until custom fonts are added
export const urwClassico = {
  variable: '--font-urw-classico',
  style: { fontFamily: 'ui-serif, Georgia, Cambria, "Times New Roman", Times, serif' }
}

