import {useParams, useNavigate, useLocation, Link} from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import api from '../api';
import {useSearch} from "./SearchContext";
import {useAuth} from "./AuthContext";

const EditPlaylistPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const { playlistId } = useParams(); // Отримуємо ID плейлиста з URL
    const navigate = useNavigate(); // Хук для перенаправлення
    const [playlist, setPlaylist] = useState(null);
    const [newName, setNewName] = useState('');
    const [newDescription, setNewDescription] = useState('');
    const [newImage, setNewImage] = useState(null);
    const [previewImage, setPreviewImage] = useState(null); // Додано для попереднього перегляду
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const location = useLocation();

    useEffect(() => {
        const fetchPlaylist = async () => {
            try {
                const response = await api.get(`/api/Playlists/${playlistId}`);
                setPlaylist(response.data);
                setNewName(response.data.title);
                setNewDescription(response.data.description);
                setNewImage(response.data.imageUrl);
                setPreviewImage(response.data.imageUrl); // Встановлюємо зображення для попереднього перегляду
            } catch (err) {
                console.error("Не вдалося завантажити плейлист", err);
            }
        };

        fetchPlaylist();
    }, [playlistId]);


    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        if (file) {
            setNewImage(file); // Оновлюємо файл
            const reader = new FileReader();
            reader.onloadend = () => {
                setPreviewImage(reader.result); // Оновлюємо попередній перегляд
            };
            reader.readAsDataURL(file);
        }
    };

    const handleSaveChanges = async () => {
        const formData = new FormData();
        formData.append('NewName', newName);
        formData.append('NewDescription', newDescription);
        if (newImage) {
            formData.append('NewFile', newImage); // Додаємо файл
        }

        try {
            await api.put(`/api/Playlists/${playlistId}/edit`, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                },
            });
            alert("Плейлист успішно оновлено!");
            navigate('/saved'); // Перенаправляємо на сторінку після успішного збереження
        } catch (err) {
            console.error("Помилка при збереженні змін", err);
            alert("Не вдалося зберегти зміни.");
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
                        {[{ path: '/', label: 'Головна' },
                            { path: '/', label: 'Головна' },
                            { path: '/profile', label: 'Профіль' },
                            { path: '/saved', label: 'Плейлисти' },
                            { path: '/categories', label: 'Категорії'},
                            { path: '/news', label: 'Новини'},
                            { path: '/subscriptions', label: 'Підписки' },
                            { path: '/history', label: 'Історії' },
                            { path: '/settings', label: 'Налаштування' },
                        ].map(({ path, label }) => (
                            <a
                                key={path}
                                href={path}
                                className={`block py-2 px-4 rounded-lg ${
                                    location.pathname === path ? 'bg-blue-500' : 'hover:bg-gray-700'
                                }`}
                            >
                                {label}
                            </a>
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
                <div className="min-h-screen flex flex-col items-center bg-gray-100 py-10">
                    <div className="bg-white rounded-lg shadow-md p-6 w-full max-w-2xl">
                        <h1 className="text-2xl font-bold text-gray-800 mb-4">
                            Редагувати плейлист
                        </h1>

                        <div className="mb-4">
                            <label
                                htmlFor="name"
                                className="block text-sm font-medium text-gray-700"
                            >
                                Назва плейлиста
                            </label>
                            <input
                                type="text"
                                id="name"
                                className="mt-1 p-2 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                value={newName}
                                onChange={(e) => setNewName(e.target.value)}
                                placeholder="Введіть назву плейлиста"
                            />
                        </div>

                        <div className="mb-4">
                            <label
                                htmlFor="description"
                                className="block text-sm font-medium text-gray-700"
                            >
                                Опис плейлиста
                            </label>
                            <textarea
                                id="description"
                                rows={4}
                                className="mt-1 p-2 block w-full border border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                value={newDescription}
                                onChange={(e) => setNewDescription(e.target.value)}
                                placeholder="Введіть опис плейлиста"
                            />
                        </div>

                        {previewImage && (
                            <div className="mb-4">
                                <p className="text-sm font-medium text-gray-700 mb-2">
                                    Поточне зображення
                                </p>
                                <img
                                    src={previewImage}
                                    alt="Preview"
                                    className="w-32 h-32 object-cover rounded-md"
                                />
                            </div>
                        )}

                        <div className="mb-4">
                            <label
                                htmlFor="image"
                                className="block text-sm font-medium text-gray-700"
                            >
                                Оновити зображення
                            </label>
                            <input
                                type="file"
                                id="image"
                                className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                                onChange={handleImageChange}
                            />
                        </div>

                        <div className="flex justify-end space-x-4">
                            <button
                                className="px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400"
                                onClick={() => navigate(-1)}
                            >
                                Скасувати
                            </button>
                            <button
                                className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600"
                                onClick={handleSaveChanges}
                            >
                                Зберегти зміни
                            </button>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
};

export default EditPlaylistPage;