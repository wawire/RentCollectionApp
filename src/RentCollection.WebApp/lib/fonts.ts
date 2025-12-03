import localFont from 'next/font/local'

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
  display: 'swap', // Improves performance
})
