import React, { useState, useEffect } from 'react';
import {Link, useParams} from 'react-router-dom';
import {useAuth} from "./AuthContext";
import {useSearch} from "./SearchContext";
import {useLocation, useNavigate} from "react-router-dom";
import api from '../api';
import {motion} from "framer-motion";

const CategoryTracksPage = () => {
    const { categoryId } = useParams();
    const [tracks, setTracks] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        const fetchTracks = async () => {
            try {
                setLoading(true);
                const response = await api.get('/api/tracks');
                // Фільтрація треків за категорією
                const filteredTracks = response.data.filter((track) =>
                    track.categoriesId.includes(parseInt(categoryId))
                );
                setTracks(filteredTracks);
            } catch (err) {
                console.error('Error fetching tracks:', err);
                setError('Не вдалося завантажити треки.');
            } finally {
                setLoading(false);
            }
        };
        fetchTracks();
    }, [categoryId]);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

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
                                initial={{opacity: 0, y: -10}}
                                animate={{opacity: 1, y: 0}}
                                whileHover={{scale: 1.1}}
                                transition={{duration: 0.3}}
                                className={`block py-2 px-4 rounded-lg transition-all duration-300 ${
                                    location.pathname === path ||
                                    (path === '/categories' && location.pathname.startsWith('/category'))
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
                        <Link to="/" className="logo-container">
                            <div className="ellipse"></div>
                            <div className="triangle"></div>
                            <div className="logo-text">SpaceRythm</div>
                        </Link>
                    </div>
                </header>
                <div className="p-6">
                    <motion.h1
                        className="text-3xl font-bold mb-6 text-center text-blue-600"
                        initial={{ opacity: 0, y: -20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.5 }}
                    >
                        Треки категорії
                    </motion.h1>
                    {error && (
                        <motion.p
                            className="text-red-500 mb-4 text-center"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.3 }}
                        >
                            {error}
                        </motion.p>
                    )}
                    {loading ? (
                        <motion.p
                            className="text-center text-gray-500"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ duration: 0.3 }}
                        >
                            Завантаження...
                        </motion.p>
                    ) : (
                        <motion.div
                            className="grid grid-cols-2 gap-6"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            transition={{ staggerChildren: 0.2, duration: 0.5 }}
                        >
                            {tracks.map((track) => (
                                <motion.div
                                    key={track.trackId}
                                    className="border rounded-lg p-4 shadow-lg bg-white hover:shadow-xl hover:scale-105 transition transform"
                                    whileHover={{ scale: 1.05 }}
                                    whileTap={{ scale: 0.95 }}
                                    initial={{ opacity: 0, y: 20 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    transition={{ duration: 0.4 }}
                                >
                                    <img
                                        src={track.imagePath}
                                        alt={track.title}
                                        className="w-full h-32 object-cover rounded-lg"
                                    />
                                    <h3 className="text-xl font-semibold mt-4 text-gray-800">{track.title}</h3>
                                    <p className="text-gray-500 mt-1">Виконавець: {track.artistName}</p>
                                    <audio
                                        controls
                                        src={track.filePath}
                                        className="mt-4 w-full"
                                    />
                                </motion.div>
                            ))}
                        </motion.div>
                    )}
                </div>
            </main>
        </div>
    );
};

export default CategoryTracksPage;
