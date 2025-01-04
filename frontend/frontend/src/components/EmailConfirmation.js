import React, { useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import api from '../api';

const ConfirmEmail = () => {
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        const confirmEmail = async () => {
            // Отримуємо параметри email і token з URL
            const queryParams = new URLSearchParams(location.search);
            const email = queryParams.get('email');
            const token = queryParams.get('token');

            if (!email || !token) {
                console.error('Missing email or token in URL.');
                navigate('/'); // Повертаємо на головну сторінку, якщо параметри відсутні
                return;
            }

            try {
                // Надсилаємо запит на підтвердження електронної пошти
                const response = await api.get(`/api/RegistrationAuthorization/confirm-email`, {
                    params: { email, token },
                });

                console.log('Email confirmed:', response.data);
                // Після успішного підтвердження переходимо на головну сторінку
                navigate('/');
            } catch (error) {
                console.error('Error confirming email:', error.response?.data?.message || error.message);
                // Якщо є помилка, можна додати логіку для відображення повідомлення
                navigate('/');
            }
        };

        confirmEmail();
    }, [location.search, navigate]);

    return (
        <div className="h-screen flex items-center justify-center">
            <div className="text-center">
                <h2 className="text-2xl font-bold mb-4">Підтвердження електронної пошти...</h2>
                <p>Зачекайте, вас буде перенаправлено на головну сторінку.</p>
            </div>
        </div>
    );
};

export default ConfirmEmail;
