import React, { useState } from 'react';
import './LoginRegistration.css';
import api from '../api';
import { useAuth } from './AuthContext';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import Modal from './Modal'

const LoginRegistration = () => {
    const [showPassword, setShowPassword] = useState(false);
    const [showRegistrationPassword, setShowRegistrationPassword] = useState(false);
    const [loginEmail, setLoginEmail] = useState('');
    const [loginPassword, setLoginPassword] = useState('');
    const [registerUsername, setRegisterUsername] = useState('');
    const [registerEmail, setRegisterEmail] = useState('');
    const [registerPassword, setRegisterPassword] = useState('');
    const [error, setError] = useState(null);
    const [successMessage, setSuccessMessage] = useState(null);
    const [isAgreementChecked, setIsAgreementChecked] = useState(false);
    const [showModal, setShowModal] = useState(false);
    const [modalMessage, setModalMessage] = useState('');
    const [isError, setIsError] = useState(false);
    const navigate = useNavigate();
    const { login } = useAuth();

    const handleLogin = async () => {
        if (!loginEmail || !loginPassword) {
            setModalMessage('Всі поля обов\'язкові для заповнення');
            setIsError(true);
            setShowModal(true);
            return;
        }

        try {
            const response = await api.post('/api/RegistrationAuthorization/authenticate', {
                Username: loginEmail,
                Password: loginPassword
            });

            console.log('Server response:', response.data);

            const token = response.data.jwtToken;
            if (!token) {
                throw new Error('Token not returned from server.');
            }

            localStorage.setItem('authToken', token);
            setModalMessage('Успішно авторизовано!');
            setIsError(false);
            setShowModal(true);

            await login(loginEmail);
            navigate('/');
        } catch (error) {
            console.error('Login failed:', error.response?.data || error.message);
            setModalMessage('Невірний логін або пароль');
            setIsError(true);
            setShowModal(true);
        }
    };

    const handleRegister = async () => {
        if (!registerEmail || !registerPassword || !registerUsername) {
            setModalMessage('Всі поля обов\'язкові для заповнення');
            setIsError(true);
            setShowModal(true);
            return;
        }

        if (!isAgreementChecked) {
            setModalMessage('Ви повинні погодитися з умовами');
            setIsError(true);
            setShowModal(true);
            return;
        }

        try {
            // Отримуємо всіх користувачів
            const usersResponse = await api.get('https://localhost:5017/api/Users/');

            // Перевіряємо, чи існує користувач з таким email або username
            const userExists = usersResponse.data.some(user => user.email === registerEmail || user.username === registerUsername);

            if (userExists) {
                setModalMessage('Користувач з таким логіном або електронною поштою вже існує.');
                setIsError(true);
                setShowModal(true);
                return;
            }

            const response = await api.post('/api/RegistrationAuthorization/Create', {
                email: registerEmail,
                password: registerPassword,
                username: registerUsername,
            });

            setModalMessage('Успішно зареєстровано! Підтвердіть вашу пошту.');
            setIsError(false);
            setShowModal(true);
        } catch (error) {
            setModalMessage('Помилка при реєстрації. Спробуйте ще раз.');
            setIsError(true);
            setShowModal(true);
        }
    };

    const togglePasswordVisibility = () => {
        setShowPassword(!showPassword);
    };

    const toggleRegistrationPasswordVisibility = () => {
        setShowRegistrationPassword(!showRegistrationPassword);
    };

    const closeModal = () => {
        setShowModal(false);
        // Очищаємо поля після закриття модалки
        setLoginEmail('');
        setLoginPassword('');
        setRegisterEmail('');
        setRegisterPassword('');
        setIsAgreementChecked(false);
    };

    return (
        <div className="flex h-screen">
            <div className="w-1/2 h-full relative">
                <img
                    src="https://storage.googleapis.com/a1aa/image/iCDARH7w8X4eeEwBzeG9OAeq9dQdzCfoSX3P6csZeoqOfIPyJA.jpg"
                    alt="Person with blue headphones looking down"
                    className="w-full h-full object-cover"
                />
                <div className="absolute bottom-4 left-4 flex items-center">
                    <Link to="/" className="logo-container">
                        <div className="ellipse"></div>
                        <div className="triangle"></div>
                        <div className="logo-text">SpaceRythm</div>
                    </Link>
                </div>
            </div>

            <div className="w-1/2 h-full flex items-center justify-center bg-white">
                {/* Модальне вікно для помилок або успіхів */}
                {showModal && (
                    <Modal
                        message={modalMessage}
                        onClose={() => setShowModal(false)}
                        isError={isError}
                    />
                )}

                <div className="w-3/4">
                    {/* Ваші форми і контент */}
                    <div className="text-center mb-8">
                        <h2 className="text-2xl font-bold mb-4">Вхід</h2>
                        <div className="mb-4">
                            <input
                                type="email"
                                value={loginEmail}
                                onChange={(e) => setLoginEmail(e.target.value)}
                                placeholder="Логін"
                                className="w-full p-3 border rounded-full mb-4"
                            />
                            <div className="relative">
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    value={loginPassword}
                                    onChange={(e) => setLoginPassword(e.target.value)}
                                    placeholder="Пароль"
                                    className="w-full p-3 border rounded-full"
                                />
                                <i
                                    className="fas fa-eye absolute right-4 top-1/2 transform -translate-y-1/2 text-gray-500 cursor-pointer"
                                    onClick={togglePasswordVisibility}
                                />
                            </div>
                        </div>
                        <button
                            className="w-full bg-blue-500 text-white py-3 rounded-full mb-4"
                            onClick={handleLogin}>
                            Увійти
                        </button>
                        <a href="/forgot-password" className="text-gray-500 underline italic">
                            Забули пароль?
                        </a>
                    </div>

                    {/* Форма реєстрації */}
                    <div className="text-center mb-8">
                        <h2 className="text-xl font-bold mb-4">або</h2>
                        <h2 className="text-2xl font-bold mb-4">Реєстрація</h2>
                        <div className="mb-4">
                            <input
                                type="text"
                                value={registerUsername}
                                onChange={(e) => setRegisterUsername(e.target.value)}
                                placeholder="Логін"
                                className="w-full p-3 border rounded-full mb-4"
                            />

                            <input
                                type="email"
                                value={registerEmail}
                                onChange={(e) => setRegisterEmail(e.target.value)}
                                placeholder="E-Mail"
                                className="w-full p-3 border rounded-full mb-4"
                            />
                            <div className="relative">
                                <input
                                    type={showRegistrationPassword ? 'text' : 'password'}
                                    value={registerPassword}
                                    onChange={(e) => setRegisterPassword(e.target.value)}
                                    placeholder="Пароль"
                                    className="w-full p-3 border rounded-full"
                                />
                                <i
                                    className="fas fa-eye absolute right-4 top-1/2 transform -translate-y-1/2 text-gray-500 cursor-pointer"
                                    onClick={toggleRegistrationPasswordVisibility}
                                />
                            </div>
                        </div>
                        <div className="flex items-center mb-4">
                            <div className="flex-grow border-t border-gray-300"></div>
                            <div className="flex justify-center space-x-4 mx-4">
                                <img alt="Google logo" className="icon-size"
                                     src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/google/google-original.svg"/>
                                <img alt="Facebook logo" className="icon-size"
                                     src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/facebook/facebook-original.svg"/>
                                <img alt="Apple logo" className="icon-size"
                                     src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/apple/apple-original.svg"/>
                            </div>
                            <div className="flex-grow border-t border-gray-300"></div>
                        </div>
                        {/* Agreement Section */}
                        <div className="flex items-center mb-4">
                            <input
                                type="radio"
                                className="mr-2"
                                checked={isAgreementChecked}
                                onChange={(e) => setIsAgreementChecked(e.target.checked)}
                            />
                            <span className="italic indented-text">
                    Я розумію, що можу скасувати підписку на свій обліковий запис у будь-який час.
                </span>
                        </div>

                        <button
                            className="w-full bg-blue-500 text-white py-3 rounded-full"
                            onClick={handleRegister}
                            disabled={!isAgreementChecked}>
                            Зареєструватися
                        </button>
                    </div>
                </div>
            </div>

        </div>
    );
};

export default LoginRegistration;
