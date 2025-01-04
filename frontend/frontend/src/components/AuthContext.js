import React, { createContext, useContext, useEffect, useState } from 'react';
import api from '../api';

const AuthContext = createContext();

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [userInfo, setUserInfo] = useState(null);
    const [isUserLoaded, setIsUserLoaded] = useState(false); // Новий стан

    useEffect(() => {
        try {
            const storedUser = localStorage.getItem('userInfo');
            if (storedUser) {
                const parsedUser = JSON.parse(storedUser);
                setUserInfo(parsedUser);
                setIsAuthenticated(true);
            }
        } catch (error) {
            console.error('Failed to parse user info from localStorage:', error);
        } finally {
            setIsUserLoaded(true); // Завжди завершуємо ініціалізацію
        }
    }, []);


    const login = async (username) => {
        if (!username) {
            console.error('Login failed: Username is undefined.');
            return;
        }

        try {
            const response = await api.get(`/api/Users/by-username/${username}`);
            const updatedUserInfo = {
                ...response.data,
                avatarUrl: `${response.data.avatarUrl}?t=${new Date().getTime()}` // Додавання timestamp
            };

            setUserInfo(updatedUserInfo);
            setIsAuthenticated(true);
            localStorage.setItem('userInfo', JSON.stringify(updatedUserInfo));
        } catch (error) {
            console.error('Failed to login:', error.response?.data || error.message);
            alert('Не вдалося увійти. Перевірте ваш логін.');
        }
    };



    const logout = () => {
        setIsAuthenticated(false);
        setUserInfo(null);

        // Видалення даних з localStorage
        localStorage.removeItem('userInfo');
    };

    return (
        <AuthContext.Provider
            value={{
                isAuthenticated,
                userInfo,
                setUserInfo,
                isUserLoaded, // Додаємо у контекст
                login,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};
