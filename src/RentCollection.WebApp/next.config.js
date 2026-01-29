/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  turbopack: {
    root: __dirname,
  },
  env: {
    API_BASE_URL: process.env.API_BASE_URL || 'https://localhost:7000',
  },
}

module.exports = nextConfig
