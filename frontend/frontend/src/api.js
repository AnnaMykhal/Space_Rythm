import axios from 'axios';

// Створюємо екземпляр axios з базовою URL-адресою
const api = axios.create({
    baseURL: 'https://localhost:5017', // Адреса бекенду
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
});

// Інтерцептор для додавання токена
api.interceptors.request.use(config => {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, error => {
    return Promise.reject(error);
});


export default api;
