import debounce from 'lodash.debounce';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useSearch } from './SearchContext';
import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import api from '../api';
import {motion} from "framer-motion";

const HistoryPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [trackDetails, setTrackDetails] = useState({});
    const location = useLocation();
    const navigate = useNavigate();
    const [tracks, setTracks] = useState([]);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    useEffect(() => {
        if (isAuthenticated) {
            fetchHistory();  // Завантажуємо історію, якщо користувач автентифікований
            fetchTracks();   // Завантажуємо всі треки
        }
    }, [isAuthenticated, userInfo?.id]);

    const fetchHistory = async () => {
        setLoading(true);
        try {
            const response = await api.get(`/api/listening-history/${userInfo?.id}`, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                },
            });

            // Створюємо об'єкт для зберігання останнього прослуховування кожного треку
            const historyMap = {};
            response.data.forEach(item => {
                // Зберігаємо лише останній запис для кожного треку
                if (!historyMap[item.trackId] || new Date(item.timestamp) > new Date(historyMap[item.trackId].timestamp)) {
                    historyMap[item.trackId] = item;
                }
            });

            // Перетворюємо об'єкт назад в масив і сортуємо по даті прослуховування (найновіші перші)
            const filteredHistory = Object.values(historyMap).sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp));

            setHistory(filteredHistory); // Збереження фільтрованої і відсортованої історії
        } catch (err) {
            setError(err.response?.data || err.message); // Обробка помилки
        } finally {
            setLoading(false);
        }
    };

    const fetchTracks = async () => {
        try {
            const response = await api.get('/api/tracks');
            setTracks(response.data); // Збереження всіх треків
        } catch (err) {
            setError(err.response?.data || err.message); // Обробка помилки
        }
    };

    const getTrackInfo = (trackId) => {
        // Шукаємо трек по його trackId
        return tracks.find(track => track.trackId === trackId) || {};
    };

    if (!isAuthenticated) {
        return <div>Please log in to view your history.</div>;
    }

    if (loading) {
        return <div>Loading...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

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
                        <Link to="/" className="logo-container">
                            <div className="ellipse"></div>
                            <div className="triangle"></div>
                            <div className="logo-text">SpaceRythm</div>
                        </Link>
                    </div>
                </header>

                <div className="p-6 bg-gray-50 rounded-lg shadow-lg max-w-6xl mx-auto">
                    <motion.h1
                        className="text-3xl font-bold mb-8 text-center text-gray-800"
                        initial={{opacity: 0, y: -20}}
                        animate={{opacity: 1, y: 0}}
                        transition={{duration: 0.5}}
                    >
                        Історія прослуховувань
                    </motion.h1>

                    <div className="block w-full">
                        {history.length === 0 ? (
                            <motion.div
                                className="text-center text-gray-500 text-lg"
                                initial={{opacity: 0}}
                                animate={{opacity: 1}}
                                transition={{duration: 0.5}}
                            >
                                Немає історії прослуховування.
                            </motion.div>
                        ) : (
                            <motion.div
                                className="grid grid-cols-1 md:grid-cols-2 gap-8"
                                initial="hidden"
                                animate="visible"
                                variants={{
                                    hidden: {opacity: 0},
                                    visible: {opacity: 1, transition: {staggerChildren: 0.2}},
                                }}
                            >
                                {history.map((historyItem) => {
                                    const track = historyItem.track;
                                    const trackInfo = getTrackInfo(track.trackId);

                                    return (
                                        <motion.div
                                            key={track.trackId}
                                            className="flex flex-col md:flex-row items-center p-6 bg-white rounded-lg shadow-md hover:shadow-lg transition-transform transform hover:scale-105"
                                            onClick={() => navigate(`/track/${track.trackId}`)}
                                            whileHover={{scale: 1.05}}
                                            style={{cursor: 'pointer'}}
                                            variants={{
                                                hidden: {opacity: 0, y: 20},
                                                visible: {opacity: 1, y: 0},
                                            }}
                                        >
                                            <motion.img
                                                src={trackInfo.imagePath || '/placeholder-image.jpg'}
                                                alt={track.title}
                                                className="w-24 h-24 md:w-32 md:h-32 object-cover rounded-lg mr-4 mb-4 md:mb-0"
                                                whileHover={{rotate: 5}}
                                            />
                                            <div className="flex-1">
                                                <h3 className="text-xl font-bold text-gray-800">{track.title}</h3>
                                                <p className="text-md text-gray-600">
                                                    Автор: {trackInfo.artistName || 'Unknown'}
                                                </p>
                                            </div>
                                        </motion.div>
                                    );
                                })}
                            </motion.div>
                        )}
                    </div>
                </div>


            </main>
        </div>
    );
};

export default HistoryPage;
