import debounce from 'lodash.debounce';
import {Link, useLocation, useNavigate} from 'react-router-dom';
import { useSearch } from './SearchContext';
import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import api from '../api';
import {motion} from "framer-motion";

const SavedPage = ({ trackId, userId, onClose }) => {
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const [tracks, setTracks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const location = useLocation();
    const navigate = useNavigate();
    const [playlists, setPlaylists] = useState([]);
    const [newPlaylistName, setNewPlaylistName] = useState("");
    const [description, setDescription] = useState("");
    const [imageFile, setImageFile] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [showModal, setShowModal] = useState(false);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    useEffect(() => {
        if (!userInfo?.id) {
            return; // Якщо userInfo або його id ще не доступні, виходимо з useEffect
        }

        const fetchUserPlaylists = async () => {
            try {
                setLoading(true);
                const response = await api.get(`/api/playlists/user/${userInfo.id}`);
                console.log(response.data); // Перевірка відповіді API
                setPlaylists(response.data);
            } catch (err) {
                setError("Не вдалося завантажити плейлисти.");
                console.error(err); // Перевірка помилки
            } finally {
                setLoading(false);
            }
        };

        fetchUserPlaylists();
    }, [userInfo?.id]); // Тепер useEffect буде викликано лише коли userInfo.id доступне



    const handleAddToPlaylist = async (playlistId) => {
        try {
            await api.post(`/api/playlists/${playlistId}/add-track`, { trackId });
            alert("Трек додано до плейлиста!");
            onClose();
        } catch (err) {
            setError("Помилка при додаванні треку до плейлиста.");
        }
    };

    const handleCreatePlaylist = async () => {
        if (!newPlaylistName.trim()) {
            alert("Введіть назву плейлиста.");
            return;
        }

        console.log('playlist name:', newPlaylistName);
        console.log('userInfo.id:', userInfo.id);
        console.log('trackIds:', [trackId]);

        const formData = new FormData();
        formData.append("userId", userInfo.id);
        formData.append("name", newPlaylistName);
        formData.append("description", description); // Опис плейлиста (можна залишити порожнім)// Перетворення масиву у строку JSON
        // Якщо є файл зображення
        if (imageFile) {
            formData.append("imageFile", imageFile);
        }

        try {
            const response = await api.post("/api/playlists/create", formData, {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            });

            console.log('Playlist created:', response.data); // Логуємо відповідь сервера

            setPlaylists([...playlists, response.data]);
            setNewPlaylistName("");
            alert(`Плейлист "${newPlaylistName}" створено.`);
            setIsModalOpen(false);
        } catch (err) {
            console.error('Error details:', err.response?.data || err.message); // Логування помилки
            setError("Помилка при створенні плейлиста.");
        }
    };

    const handlePlaylistClick = (playlistId) => {
        navigate(`/playlist/${playlistId}`); // Перехід на сторінку плейлиста
    };

    function handleEditPlaylist(playlistId) {
        navigate(`/edit-playlist/${playlistId}`);
    }

    // Логіка для видалення плейлиста
    const handleDeletePlaylist = async (playlistId) => {
        try {
            await api.delete(`/api/Playlists/${playlistId}`);
            document.location.reload();
            alert("Плейлист успішно видалено!");
            onClose();
        } catch (err) {
            setError("Не вдалося видалити плейлист.");
        }
    };

    const handleImageChange = (e) => {
        const file = e.target.files[0]; // Беремо перший вибраний файл
        if (file) {
            setImageFile(file); // Зберігаємо файл у стані
        }
    };

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
                            {path: '/', label: 'Головна'},
                            {path: '/profile', label: 'Профіль'},
                            {path: '/saved', label: 'Плейлисти'},
                            {path: '/categories', label: 'Категорії'},
                            {path: '/news', label: 'Новини'},
                            {path: '/subscriptions', label: 'Підписки'},
                            {path: '/history', label: 'Історії'},
                            {path: '/settings', label: 'Налаштування'},
                        ].map(({path, label}) => (
                            <motion.a
                                key={path}
                                href={path}
                                initial={{opacity: 0, y: -10}}
                                animate={{opacity: 1, y: 0}}
                                whileHover={{scale: 1.1}}
                                transition={{duration: 0.3}}
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

                {/* Модальне вікно */}
                <div className="modal">
                    <div className="modal-content">
                        {error && <p className="error">{error}</p>}

                        <h3 className="text-2xl font-semibold mb-4">Ваші плейлисти:</h3>
                        {loading ? (
                            <p className="text-gray-500">Завантаження...</p>
                        ) : error ? (
                            <p className="text-red-500">{error}</p>
                        ) : playlists.length === 0 ? (
                            <p>У вас немає плейлистів.</p>
                        ) : (
                            playlists.map((playlist) => (
                                <div
                                    key={playlist.playlistId}
                                    className="playlist-item flex items-center justify-between bg-white p-4 rounded-lg shadow-md mb-4 hover:bg-gray-100 transition"
                                >
                                    <div className="flex items-center space-x-4">
                                        {playlist.imageUrl ? (
                                            <img
                                                src={playlist.imageUrl}
                                                alt="playlist image"
                                                className="w-16 h-16 object-cover rounded-full"
                                            />
                                        ) : (
                                            <div className="w-16 h-16 bg-gray-300 rounded-full flex justify-center items-center text-white text-xl">
                                                <span>📷</span>
                                            </div>
                                        )}
                                        <span className="text-lg font-semibold">{playlist.title}</span>
                                    </div>
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handlePlaylistClick(playlist.playlistId)}
                                            className="bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition"
                                        >
                                            Переглянути
                                        </button>
                                        <button
                                            onClick={() => handleEditPlaylist(playlist.playlistId)} // Викликаємо handleEditPlaylist
                                            className="bg-yellow-500 text-white px-4 py-2 rounded-lg hover:bg-yellow-600 transition"
                                        >
                                            Редагувати
                                        </button>
                                        <button
                                            onClick={() => handleDeletePlaylist(playlist.playlistId)}
                                            className="bg-red-500 text-white px-4 py-2 rounded-lg hover:bg-red-600 transition"
                                        >
                                            Видалити
                                        </button>
                                    </div>
                                </div>
                            ))
                        )}

                        <h3 className="text-xl font-medium mt-8 mb-4">Створити новий плейлист:</h3>
                        <input
                            type="text"
                            placeholder="Назва плейлиста"
                            value={newPlaylistName}
                            onChange={(e) => setNewPlaylistName(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg mb-4"
                            required
                        />
                        <textarea
                            placeholder="Опис плейлиста"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg mb-4"
                        />
                        <div className="mt-4">
                            <label className="block text-sm font-medium text-gray-700">Фото плейлиста (необов'язково):</label>
                            <input
                                type="file"
                                onChange={(e) => handleImageChange(e)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg mt-2"
                                accept="image/*"
                            />
                        </div>

                        <div className="flex justify-between mt-6">
                            <button
                                onClick={handleCreatePlaylist}
                                className="py-2 px-4 bg-green-500 text-white rounded-lg hover:bg-green-600 transition"
                            >
                                Створити
                            </button>
                            <button
                                onClick={onClose}
                                className="py-2 px-4 bg-gray-400 text-white rounded-lg hover:bg-gray-500 transition"
                            >
                                Закрити
                            </button>
                        </div>
                    </div>
                </div>

            </main>
        </div>
    );
};

export default SavedPage;
