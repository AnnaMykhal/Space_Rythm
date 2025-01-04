import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import './MainPage.css';
import api from '../api';
import debounce from 'lodash.debounce';
import {useLocation, useNavigate} from 'react-router-dom';
import { useSearch } from './SearchContext';
import { motion } from 'framer-motion';

const MainPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const [tracks, setTracks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const location = useLocation();
    const navigate = useNavigate();


    useEffect(() => {
        const fetchTracks = async () => {
            try {
                setLoading(true);
                let response;
                if (isAuthenticated) {
                    // Авторизований користувач - отримати рекомендовані треки
                    response = await api.get('/api/Tracks/recommended', { params: { count: 10 } });
                    console.log("Authorize")
                } else {
                    // Неавторизований користувач - отримати топ треки
                    response = await api.get('/api/Tracks/top', { params: { count: 10 } });
                    console.log("No")
                }
                setTracks(response.data);
            } catch (err) {
                setError(err.message || 'Не вдалося завантажити треки');
            } finally {
                setLoading(false);
            }
        };
        fetchTracks();
    }, [isAuthenticated]);

    const fetchSearchResults = async (query) => {
        try {
            setLoading(true);
            setError(null);
            const response = await api.get('/api/Tracks/search', { params: { query } });
            setSearchResults(response.data);
        } catch (err) {
            setError('За Вашим запитом нічого не знайдено.');
        } finally {
            setLoading(false);
        }
    };

    const debouncedFetchSearchResults = debounce((query) => {
        fetchSearchResults(query);
    }, 500);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);

        if (query.trim() === '') {
            setSearchResults(tracks);
        } else {
            debouncedFetchSearchResults(query);
        }
    };

    // Функція форматування часу
    function formatDuration(durationInSeconds) {
        if (!Number.isFinite(durationInSeconds) || durationInSeconds < 0) {
            return '0:00'; // Значення за замовчуванням
        }
        const minutes = Math.floor(durationInSeconds / 60);
        const seconds = durationInSeconds % 60;
        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }


    function parseTimeSpanToSeconds(timeSpan) {
        const [hours, minutes, secondsWithFraction] = timeSpan.split(':').map((timePart) => {
            // Враховуємо тільки цілу частину секунд
            const [seconds] = timePart.split('.');
            return Number(seconds);
        });
        return (hours || 0) * 3600 + (minutes || 0) * 60 + (secondsWithFraction || 0);
    }

    function getCorrectEnding(count) {
        const lastDigit = count % 10;
        const lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
            return 'переглядів';
        }

        switch (lastDigit) {
            case 1:
                return 'перегляд';
            case 2:
            case 3:
            case 4:
                return 'перегляди';
            default:
                return 'переглядів';
        }
    }


    return (
        <div className="flex h-screen bg-gray-50 text-gray-800">
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
            <main className="flex-1 p-6 overflow-y-auto">
                <header className="flex items-center justify-between mb-4 bg-black text-white p-4">
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
                <section className="mb-8">
                    {loading ? (
                        <p className="animate-pulse text-lg text-center">Завантаження...</p>
                    ) : error ? (
                        <p className="text-gray-500 text-lg text-center">{error}</p>
                    ) : searchQuery.trim() ? (
                        <div>
                            <h2 className="text-xl font-bold mb-4 text-center animate__animated animate__fadeIn">Результати пошуку</h2>
                            {searchResults.length > 0 ? (
                                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
                                    {searchResults.map((result) => (
                                        <div key={result.trackId} className="bg-white rounded-lg shadow-lg p-4 hover:scale-105 transition-transform duration-300">
                                            <div className="video-container relative">
                                                <img
                                                    src={result.imagePath || '/placeholder-image.jpg'}
                                                    alt={result.title}
                                                    className="w-full h-40 object-cover rounded-lg transition-all duration-500 transform hover:scale-105"
                                                    onClick={() => navigate(`/track/${result.trackId}`)}
                                                />
                                                {/* Час треку без анімацій */}
                                                <div className="video-time">
                                                    {result.duration ? formatDuration(parseTimeSpanToSeconds(result.duration)) : "Невідомо"}
                                                </div>
                                            </div>
                                            <h3 className="text-lg font-bold mt-2 text-center">{result.title}</h3>
                                            <p className="text-sm text-gray-600 text-center">{result.artistName}</p>
                                            <p className="text-xs text-gray-400 text-center">{new Date(result.uploadDate).toLocaleDateString()}</p>
                                        </div>
                                    ))}
                                </div>
                            ) : (
                                <p className="text-center">За Вашим запитом нічого не знайдено.</p>
                            )}
                        </div>
                    ) : (
                        <div>
                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
                                {tracks.map((track) => (
                                    <div
                                        key={track.trackId}
                                        className="bg-white rounded-lg shadow-lg p-4 hover:scale-105 transition-transform duration-300"
                                        onClick={() => navigate(`/track/${track.trackId}`)}
                                    >
                                        <div className="video-container relative">
                                            <img
                                                src={track.imagePath || '/placeholder-image.jpg'}
                                                alt={track.title}
                                                className="w-full h-40 object-cover rounded-lg transition-all duration-500 transform hover:scale-105"
                                            />
                                            {/* Час треку без анімацій */}
                                            <div className="video-time">
                                                {track.duration ? formatDuration(parseTimeSpanToSeconds(track.duration)) : "Невідомо"}
                                            </div>
                                        </div>
                                        <h3 className="text-lg font-bold mt-2 text-center">{track.title}</h3>
                                        <p className="text-sm text-gray-600 text-center">{track.artistName}</p>
                                        <p className="text-xs text-gray-400 text-center">Завантажено: {new Date(track.uploadDate).toLocaleDateString()}</p>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </section>

                {!isAuthenticated && (
                    <div className="l-front-signup-teaser w-full flex justify-center items-center py-8 bg-gray-50 border-t border-gray-200">
                        <div className="signupModule text-center max-w-md">
                            <h2 className="signupModule__title text-2xl font-light mb-4 animate__animated animate__fadeIn">
                                Дякуємо за прослуховування. Тепер приєднуйтесь!
                            </h2>
                            <p className="signupModule__copy text-lg text-gray-700 mb-6 animate__animated animate__fadeIn">
                                Зберігайте треки, підписуйтесь на артистів і створюйте плейлисти. Все безкоштовно.
                            </p>
                            <div className="flex flex-col gap-4">
                                <button
                                    type="button"
                                    className="g-opacity-transition signupModule__signupCta sc-button sc-button-large signupButton sc-button-cta sc-button-primary bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 transition-all duration-300"
                                    onClick={() => window.location.href = '/auth'}
                                >
                                    Створити аккаунт
                                </button>
                                <p className="signupModule__signIn text-gray-600">
                                    Вже є акаунт?{' '}
                                    <button
                                        type="button"
                                        className="g-opacity-transition loginButton text-white hover:underline transition-all duration-300"
                                        onClick={() => window.location.href = '/auth'}
                                    >
                                        Увійти
                                    </button>
                                </p>
                            </div>
                        </div>
                    </div>
                )}
            </main>
        </div>
    );
};

export default MainPage;
