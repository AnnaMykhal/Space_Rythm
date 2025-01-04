import React, { useEffect, useState } from 'react';
import {useLocation, useNavigate, useParams} from 'react-router-dom';
import api from '../api';
import {useAuth} from "./AuthContext";
import {useSearch} from "./SearchContext";
import {motion} from "framer-motion";

const NewsPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const navigate = useNavigate();
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const location = useLocation();
    const [news, setNews] = useState([]);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    useEffect(() => {
        const fetchNews = async () => {
            setLoading(true);
            try {
                const response = await api.get('api/news/latest'); // Виклик API для отримання новин
                setNews(response.data); // Зберігаємо отримані новини
            } catch (err) {
                setError('Не вдалося завантажити новини.');
            } finally {
                setLoading(false);
            }
        };
        fetchNews();
    }, []);

    return (
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
                        <div className="logo-container">
                            <div className="ellipse"></div>
                            <div className="triangle"></div>
                            <div className="logo-text">SpaceRythm</div>
                        </div>
                    </div>
                </header>

                <section className="p-6 bg-gray-50 rounded-lg shadow-md">
                    {loading && (
                        <motion.p
                            className="text-center text-gray-600"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.5 }}
                        >
                            Завантаження новин...
                        </motion.p>
                    )}
                    {error && (
                        <motion.p
                            className="text-red-500 text-center"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.5 }}
                        >
                            {error}
                        </motion.p>
                    )}
                    {!loading && !error && news.length === 0 && (
                        <motion.p
                            className="text-center text-gray-600"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.5 }}
                        >
                            Новин немає.
                        </motion.p>
                    )}
                    {!loading && !error && news.length > 0 && (
                        <motion.ul
                            className="space-y-6"
                            initial="hidden"
                            animate="visible"
                            variants={{
                                hidden: { opacity: 0, y: 10 },
                                visible: {
                                    opacity: 1,
                                    y: 0,
                                    transition: { staggerChildren: 0.2, duration: 0.6 },
                                },
                            }}
                        >
                            {news.map((item, index) => (
                                <motion.li
                                    key={index}
                                    className="p-4 bg-white rounded-lg shadow-lg hover:shadow-xl transition transform hover:scale-105"
                                    variants={{
                                        hidden: { opacity: 0, y: 10 },
                                        visible: { opacity: 1, y: 0 },
                                    }}
                                >
                                    <h3 className="text-xl font-semibold text-gray-800 mb-2">{item.title}</h3>
                                    <p className="text-sm text-gray-500 mb-4">{item.publishedDate}</p>
                                    <motion.a
                                        href={item.link}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="inline-block text-blue-600 hover:text-blue-800 font-medium"
                                        whileHover={{ scale: 1.1 }}
                                        transition={{ duration: 0.3 }}
                                    >
                                        Читати більше →
                                    </motion.a>
                                </motion.li>
                            ))}
                        </motion.ul>
                    )}
                </section>
            </main>
        </div>
    );
}

export default NewsPage;