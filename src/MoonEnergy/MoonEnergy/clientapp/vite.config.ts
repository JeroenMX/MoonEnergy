import {defineConfig} from 'vite'
import vue from '@vitejs/plugin-vue'
import mkcert from 'vite-plugin-mkcert';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        vue(),
        process.env.NODE_ENV === 'development' && mkcert()
    ],
    server: {
        https: true,
        proxy: {
            '/api': {
                target: 'https://localhost:5005',
                secure: false,
                changeOrigin: false
            },
            '/bff': {
                target: 'https://localhost:5005',
                secure: false,
                changeOrigin: false
            },
            '/signin-oidc': {
                target: 'https://localhost:5005',
                secure: false,
                changeOrigin: false
            }
        },
        port: 5000
    },
})
