import React, { useState, useEffect } from 'react';
import api from '../api';
import {useAuth} from "./AuthContext";
import {useSearch} from "./SearchContext";
import {useLocation, useNavigate} from "react-router-dom";
import { Link } from 'react-router-dom';
import {motion} from "framer-motion";


const CategoriesPage = () => {
    const [categories, setCategories] = useState([]);
    const [newCategory, setNewCategory] = useState('');
    const [newImage, setNewImage] = useState(null);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery, searchResults, setSearchResults } = useSearch();
    const location = useLocation();
    const navigate = useNavigate();

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    // Завантаження категорій
    useEffect(() => {
        const fetchCategories = async () => {
            try {
                setLoading(true);
                const response = await api.get('/api/trackcategories');
                setCategories(response.data);
            } catch (err) {
                console.error('Error fetching categories:', err);
                setError('Не вдалося завантажити категорії.');
            } finally {
                setLoading(false);
            }
        };
        fetchCategories();
    }, []);

    // Додавання нової категорії
    const handleAddCategory = async () => {
        if (!newCategory) {
            setError('Назва категорії не може бути порожньою.');
            return;
        }

        const formData = new FormData();
        formData.append('Category', newCategory);
        if (newImage) formData.append('Image', newImage);

        try {
            setLoading(true);
            await api.post('/api/trackcategories', formData, {
                headers: { 'Content-Type': 'multipart/form-data' },
            });
            setNewCategory('');
            setNewImage(null);
            setError('');
            // Оновлення списку категорій
            const updatedCategories = await api.get('/api/trackcategories');
            setCategories(updatedCategories.data);
        } catch (err) {
            console.error('Error creating category:', err);
            setError('Не вдалося створити категорію.');
        } finally {
            setLoading(false);
        }
    };

    // Видалення категорії
    const handleDeleteCategory = async (id) => {
        const confirmation = window.confirm('Ви впевнені, що хочете видалити цю категорію?');
        if (!confirmation) return;

        try {
            setLoading(true);
            await api.delete(`/api/trackcategories/`);
            setCategories(categories.filter((category) => category.id !== id));
        } catch (err) {
            console.error('Error deleting category:', err);
            setError('Не вдалося видалити категорію.');
        } finally {
            setLoading(false);
        }
    };

    const handleCategoryClick = (id) => {
        navigate(`/category/${id}`);
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
                        <Link to="/" className="logo-container">
                            <div className="ellipse"></div>
                            <div className="triangle"></div>
                            <div className="logo-text">SpaceRythm</div>
                        </Link>
                    </div>
                </header>
                <div className="p-6">
                    {error && <p className="text-red-500 mb-4">{error}</p>}
                    {/*<div className="mb-6">*/}
                    {/*    <h2 className="text-lg font-semibold mb-2">Додати нову категорію</h2>*/}
                    {/*    <input*/}
                    {/*        type="text"*/}
                    {/*        placeholder="Назва категорії"*/}
                    {/*        value={newCategory}*/}
                    {/*        onChange={(e) => setNewCategory(e.target.value)}*/}
                    {/*        className="border p-2 rounded mr-2"*/}
                    {/*    />*/}
                    {/*    <input*/}
                    {/*        type="file"*/}
                    {/*        onChange={(e) => setNewImage(e.target.files[0])}*/}
                    {/*        className="border p-2 rounded mr-2"*/}
                    {/*    />*/}
                    {/*    <button*/}
                    {/*        onClick={handleAddCategory}*/}
                    {/*        className="bg-blue-500 text-white px-4 py-2 rounded"*/}
                    {/*        disabled={loading}*/}
                    {/*    >*/}
                    {/*        Додати*/}
                    {/*    </button>*/}
                    {/*</div>*/}
                    <div className="p-6">
                        <h1 className="text-3xl font-extrabold mb-6 text-gray-800">Категорії</h1>
                        {error && (
                            <p className="text-red-500 mb-4 text-center animate-pulse">{error}</p>
                        )}
                        {loading ? (
                            <p className="text-gray-500 text-center animate-bounce">
                                Завантаження...
                            </p>
                        ) : (
                            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
                                {categories.map((category) => (
                                    <div
                                        key={category.id}
                                        className="relative border rounded-lg overflow-hidden shadow-lg cursor-pointer group transform transition-transform duration-300 hover:scale-105"
                                        onClick={() => handleCategoryClick(category.id)}
                                    >
                                        {category.imageUrl && (
                                            <div className="overflow-hidden">
                                                <img
                                                    src={category.imageUrl}
                                                    alt={category.category}
                                                    className="w-full h-40 object-cover transition-transform duration-300 group-hover:scale-110"
                                                />
                                            </div>
                                        )}
                                        <div
                                            className="p-4 bg-white text-center transition-colors duration-300 group-hover:bg-gray-100">
                                            <h3 className="text-lg font-semibold text-gray-800 group-hover:text-gray-900">
                                                {category.category}
                                            </h3>
                                        </div>
                                        <div
                                            className="absolute inset-0 bg-gradient-to-t from-black via-transparent opacity-0 group-hover:opacity-30 transition-opacity duration-300"></div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                </div>
            </main>
        </div>
    );
};

export default CategoriesPage;
