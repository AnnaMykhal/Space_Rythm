import React, { useEffect, useState } from 'react';
import {useLocation, useNavigate, useParams} from 'react-router-dom';
import api from '../api';
import {useAuth} from "./AuthContext";
import {useSearch} from "./SearchContext";
import {motion} from "framer-motion";

const PlaylistPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const [tracks, setTracks] = useState([]);
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const { playlistId } = useParams(); // Отримуємо ID плейлиста з URL
    const navigate = useNavigate();
    const [playlist, setPlaylist] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const location = useLocation();

    useEffect(() => {
        const fetchTracks = async () => {
            try {
                setLoading(true);
                const response = await api.get('/api/Tracks/latest', { params: { count: 10 } });
                setTracks(response.data);
            } catch (err) {
                setError(err.message || 'Не вдалося завантажити треки');
            } finally {
                setLoading(false);
            }
        };
        fetchTracks();
    }, []);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    useEffect(() => {
        const fetchPlaylistDetails = async () => {
            try {
                setLoading(true);
                const response = await api.get(`/api/playlists/${playlistId}`);
                setPlaylist(response.data);
            } catch (err) {
                setError('Не вдалося завантажити плейлист.');
            } finally {
                setLoading(false);
            }
        };

        fetchPlaylistDetails();
    }, [playlistId]);

    if (loading) return <p>Завантаження...</p>;

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

                {error && <p>{error}</p>}
                {playlist ? (
                    <>
                        <h2 className="text-xl font-bold mb-4">Назва плейлиста: {playlist.title}</h2>
                        {/* Перевіряємо, чи існують треки перед використанням map */}
                        {playlist.playlistTracks && playlist.playlistTracks.length > 0 ? (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                {playlist.playlistTracks.map(({ track, addedDate }) => (
                                    <div
                                        key={track.trackId}
                                        className="bg-white rounded-lg shadow-lg p-4 hover:shadow-xl transition-shadow"
                                        onClick={() => navigate(`/track/${track.trackId}`)}
                                    >
                                        <div className="video-container relative">
                                            {tracks
                                                .filter((trackIm) => trackIm.trackId === track.trackId) // Фільтруємо по trackId
                                                .map((trackIm) => (
                                                    <img
                                                        key={trackIm.trackId}
                                                        src={trackIm.imagePath || '/placeholder-image.jpg'}
                                                        alt={track.title}
                                                        className="w-full h-40 object-cover rounded-lg"
                                                    />
                                                ))}
                                            <div className="video-time absolute bottom-2 right-2 bg-black bg-opacity-50 text-white text-xs px-2 py-1 rounded">
                                                {track.duration ? formatDuration(parseTimeSpanToSeconds(track.duration)) : "Невідомо"}
                                            </div>
                                        </div>
                                        <h3 className="text-lg font-bold mt-2">{track.title}</h3>
                                        <p className="text-sm text-gray-600">{track.artistName}</p>
                                        <p className="text-xs text-gray-400">додано до плейлиста: {new Date(addedDate).toLocaleDateString()}</p>
                                    </div>
                                ))}

                            </div>
                        ) : (
                            <p>У цьому плейлисті немає треків.</p>
                        )}

                    </>
                ) : (
                    <p>Плейлист не знайдено.</p>
                )}
            </main>
        </div>
    );
};

export default PlaylistPage;
