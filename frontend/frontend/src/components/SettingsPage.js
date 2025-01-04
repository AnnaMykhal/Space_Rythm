import debounce from 'lodash.debounce';
import {Link, useLocation, useNavigate} from 'react-router-dom';
import { useSearch } from './SearchContext';
import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import api from '../api';
import {motion} from "framer-motion";

const SettingsPage = () => {
    const { isAuthenticated, userInfo, logout, isUserLoaded } = useAuth();
    const {searchQuery, setSearchQuery, searchResults, setSearchResults} = useSearch();
    const [tracks, setTracks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [userData, setUserData] = useState({});
    const [editMode, setEditMode] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();

    // Завантаження даних користувача
    useEffect(() => {
        if (isUserLoaded && userInfo?.id) {
            fetchUserData();
        }
        }, [isUserLoaded, userInfo]);

    const fetchUserData = async () => {
        if (!userInfo?.id) {
            setError('Ідентифікатор користувача відсутній.');
            return;
        }

        try {
            const response = await api.get(`/api/users/${userInfo.id}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
            });
            console.log('User data from API:', response.data); // Перевірка відповіді
            setUserData(response.data);
        } catch (err) {
            console.error('Error fetching user data:', err);
            setError('Не вдалося завантажити дані користувача.');
        }
    };

    const handleLogout = () => {
        // Видаляємо токен з localStorage
        localStorage.removeItem('authToken');
        localStorage.removeItem('userInfo');
        // Оновлюємо сторінку або перенаправляємо
        navigate('/')
        document.location.reload();
    };

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        console.log(`Name: ${name}`);
        console.log(`Name: ${value}`);
        setUserData((prev) => ({ ...prev, [name]: value }));
        console.log(`SetUserDate: ${setUserData.body}`);
    };

    const handleSaveChanges = async () => {
        try {
            setLoading(true);
            const response = await api.put(`/api/users/update`, userData, {
                headers: { Authorization: `Bearer ${localStorage.getItem('token')}` },
            });

            alert('Зміни збережено!');

            // Оновлюємо локальний стан та localStorage
            const updatedUserData = response.data;
            setUserData(updatedUserData);

            setEditMode(false);
            document.location.reload();
        } catch (err) {
            setError('Не вдалося зберегти зміни.');
        } finally {
            setLoading(false);
        }
    };


    // Скасування змін
    const handleCancel = () => {
        setEditMode(false);
        // Перезавантажуємо дані користувача, якщо редагування скасовано
        setUserData((prev) => ({ ...prev }));
    };


    if (!userData) {
        return <div>Завантаження...</div>;
    }

    if (!isUserLoaded) {
        return <div>Завантаження даних...</div>;
    }


    const handleDeleteAccount = async () => {
        const confirmation = window.confirm('Ви впевнені, що хочете видалити свій акаунт?');
        if (confirmation) {
            try {
                setLoading(true);

                // Спочатку виходимо з акаунту та очищаємо дані
                logout(); // Викликаємо функцію logout для очищення локальних даних

                // Перенаправляємо користувача на сторінку входу
                navigate('/');

                // Після виходу з системи видаляємо акаунт на сервері
                const response = await api.delete(`/api/users/${userInfo?.id}`, {
                    headers: {
                        Authorization: `Bearer ${localStorage.getItem("token")}`, // Передаємо токен авторизації
                    },
                });

                // Якщо акаунт успішно видалено, повідомляємо користувача
                if (response.status === 200) {
                    alert('Акаунт видалено.');
                    // Очищаємо дані з localStorage
                    localStorage.removeItem('token');
                    localStorage.removeItem('userInfo');
                }
            } catch (err) {
                setError('Не вдалося видалити акаунт.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        }
    };




    return(
        <div className="flex h-screen bg-gray-50 text-gray-800">
            {/* Бічна панель */}
            {isAuthenticated && (
                <aside className="w-1/5 bg-gray-900 text-white flex flex-col">
                    <div className="flex items-center p-4">
                        <img
                            src={userInfo?.avatarUrl || 'https://placehold.co/50x50'}
                            alt="User avatar"
                            className="rounded-full w-12 h-12 mr-3"
                        />
                        <div>
                            <p className="font-bold">{userInfo?.nickname || userInfo?.username}</p>
                            <p className="text-sm text-gray-400">{userInfo?.email}</p>
                        </div>
                    </div>
                    <nav className="mt-6">
                        {[
                            { path: '/', label: 'Головна' },
                            { path: '/profile', label: 'Профіль' },
                            { path: '/saved', label: 'Плейлисти' },
                            { path: '/categories', label: 'Категорії' },
                            { path: '/news', label: 'Новини' },
                            { path: '/subscriptions', label: 'Підписки' },
                            { path: '/history', label: 'Історії' },
                            { path: '/settings', label: 'Налаштування' },
                        ].map(({ path, label }) => (
                            <motion.a
                                key={path}
                                href={path}
                                initial={{ opacity: 0, y: -10 }}
                                animate={{ opacity: 1, y: 0 }}
                                whileHover={{ scale: 1.1 }}
                                transition={{ duration: 0.3 }}
                                className={`block py-2 px-4 rounded-lg transition-all duration-300 ${
                                    location.pathname === path
                                        ? 'bg-blue-500 text-white scale-105'
                                        : 'hover:bg-gray-700 hover:scale-105'
                                }`}
                            >
                                {label}
                            </motion.a>
                        ))}
                    </nav>
                </aside>
            )}

            {/* Основний контент */}
            <main className="flex-1 p-6 overflow-y-auto">
                <header className="flex items-center justify-between mb-4 bg-black text-white p-4">
                    {/* Пошук */}
                    <div className="flex items-center w-3/4">
                        <input
                            type="text"
                            placeholder="Пошук"
                            style={{
                                color: 'black',
                                fontFamily: 'Times New Roman, Times, serif',
                            }}
                            className="w-full p-2 border border-gray-300 rounded-l"
                            value={searchQuery}
                            onChange={handleSearchChange}
                        />
                        <button className="bg-blue-500 text-white p-2 rounded-r">
                            <i className="fas fa-search"></i>
                        </button>
                    </div>
                    <div className="flex items-center">
                        <Link to="/" className="logo-container">
                            <div className="ellipse"></div>
                            <div className="triangle"></div>
                            <div className="logo-text">SpaceRythm</div>
                        </Link>
                    </div>
                </header>

                <motion.h1
                    className="text-3xl font-extrabold text-gray-800 mb-6 text-center"
                    initial={{ opacity: 0, y: -20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5 }}
                >
                    Налаштування профілю
                </motion.h1>

                {error && <p className="text-red-500 mb-4 text-center">{error}</p>}

                <form className="space-y-6 max-w-4xl mx-auto">
                    {/* Email */}
                    <div className="flex flex-col">
                        <motion.label
                            className="text-sm font-medium text-gray-700 mb-2"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.3 }}
                        >
                            Email:
                        </motion.label>
                        <motion.input
                            type="email"
                            name="email"
                            value={userData.email}
                            onChange={handleInputChange}
                            disabled={!editMode}
                            className={`w-full p-4 border ${editMode ? 'border-gray-300' : 'border-transparent'} rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition`}
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.4}}
                        />
                    </div>

                    {/* Username */}
                    <div className="flex flex-col">
                        <motion.label
                            className="text-sm font-medium text-gray-700 mb-2"
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.3}}
                        >
                            Ім'я користувача:
                        </motion.label>
                        <motion.input
                            type="text"
                            name="username"
                            value={userData.username}
                            onChange={handleInputChange}
                            disabled={!editMode}
                            className={`w-full p-4 border ${editMode ? 'border-gray-300' : 'border-transparent'} rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition`}
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.4}}
                        />
                    </div>

                    {/* Biography */}
                    <div className="flex flex-col">
                        <motion.label
                            className="text-sm font-medium text-gray-700 mb-2"
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.3}}
                        >
                            Біографія:
                        </motion.label>
                        <motion.textarea
                            name="biography"
                            value={userData.biography}
                            onChange={handleInputChange}
                            disabled={!editMode}
                            className={`w-full p-4 border ${editMode ? 'border-gray-300' : 'border-transparent'} rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition`}
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.4}}
                        />
                    </div>

                    {/* Country */}
                    <div className="flex flex-col">
                        <motion.label
                            className="text-sm font-medium text-gray-700 mb-2"
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.3}}
                        >
                            Країна:
                        </motion.label>
                        <motion.input
                            type="text"
                            name="country"
                            value={userData.country || ''}
                            onChange={handleInputChange}
                            disabled={!editMode}
                            className={`w-full p-4 border ${editMode ? 'border-gray-300' : 'border-transparent'} rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition`}
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.4}}
                        />
                    </div>

                    {/* City */}
                    <div className="flex flex-col">
                        <motion.label
                            className="text-sm font-medium text-gray-700 mb-2"
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.3}}
                        >
                            Місто:
                        </motion.label>
                        <motion.input
                            type="text"
                            name="city"
                            value={userData.city || ''}
                            onChange={handleInputChange}
                            disabled={!editMode}
                            className={`w-full p-4 border ${editMode ? 'border-gray-300' : 'border-transparent'} rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition`}
                            initial={{opacity: 0}}
                            animate={{opacity: 1}}
                            transition={{duration: 0.4}}
                        />
                    </div>

                    {/* Password */}
                    {editMode && (
                        <div className="flex flex-col">
                            <motion.label
                                className="text-sm font-medium text-gray-700 mb-2"
                                initial={{opacity: 0}}
                                animate={{opacity: 1}}
                                transition={{duration: 0.3}}
                            >
                                Новий пароль:
                            </motion.label>
                            <motion.input
                                type="password"
                                name="password"
                                placeholder="Введіть новий пароль"
                                onChange={handleInputChange}
                                className="w-full p-4 border border-gray-300 rounded-lg bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition"
                                initial={{opacity: 0}}
                                animate={{opacity: 1}}
                                transition={{duration: 0.4}}
                            />
                        </div>
                    )}

                    {/* Buttons */}
                    <div className="flex gap-4">
                        {editMode ? (
                            <>
                                <motion.button
                                    type="button"
                                    onClick={handleSaveChanges}
                                    className="w-full md:w-auto bg-green-500 text-white py-3 px-6 rounded-lg hover:bg-green-600 transition"
                                    disabled={loading}
                                    whileHover={{scale: 1.05}}
                                    initial={{opacity: 0}}
                                    animate={{opacity: 1}}
                                    transition={{duration: 0.5}}
                                >
                                    Зберегти
                                </motion.button>
                                <motion.button
                                    type="button"
                                    onClick={handleCancel}
                                    className="w-full md:w-auto bg-gray-300 text-black py-3 px-6 rounded-lg hover:bg-gray-400 transition"
                                    disabled={loading}
                                    whileHover={{scale: 1.05}}
                                    initial={{opacity: 0}}
                                    animate={{opacity: 1}}
                                    transition={{duration: 0.5}}
                                >
                                    Скасувати
                                </motion.button>
                            </>
                        ) : (
                            <motion.button
                                type="button"
                                onClick={() => setEditMode(true)}
                                className="w-full md:w-auto bg-blue-500 text-white py-3 px-6 rounded-lg hover:bg-blue-600 transition"
                                whileHover={{ scale: 1.05 }}
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                transition={{ duration: 0.5 }}
                            >
                                Редагувати
                            </motion.button>
                        )}
                    </div>

                    {/* Logout and Delete Account */}
                    <div className="mt-6 flex justify-between gap-4">
                        <motion.button
                            onClick={handleLogout}
                            className="w-full md:w-auto bg-red-500 text-white py-3 px-6 rounded-lg hover:bg-red-600 transition"
                            whileHover={{ scale: 1.05 }}
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.5 }}
                        >
                            Вихід
                        </motion.button>
                        <motion.button
                            onClick={() => handleDeleteAccount(userInfo?.id)}
                            className="w-full md:w-auto bg-red-500 text-white py-3 px-6 rounded-lg hover:bg-red-600 transition"
                            whileHover={{ scale: 1.05 }}
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.5 }}
                        >
                            Видалити акаунт
                        </motion.button>
                    </div>
                </form>


            </main>
        </div>
    )
}

export default SettingsPage;