import debounce from 'lodash.debounce';
import {Link, useLocation, useNavigate} from 'react-router-dom';
import { useSearch } from './SearchContext';
import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import api from '../api';
import {motion} from "framer-motion";

const SubscriptionPage = () => {
    const { isAuthenticated, userInfo } = useAuth();
    const { searchQuery, setSearchQuery } = useSearch();
    const [tracks, setTracks] = useState([]);
    const [tabUsers, setTabUsers] = useState([]);
    const [activeTab, setActiveTab] = useState('followers');
    const [selectedUserId, setSelectedUserId] = useState(null); // Обраний користувач
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const location = useLocation();
    const navigate = useNavigate();
    const [userTrackCounts, setUserTrackCounts] = useState({});
    const [userFollowersCounts, setUserFollowersCounts] = useState({});
    const [userFollowingCounts, setUserFollowingCounts] = useState({});

    const handleTabChange = (tab) => setActiveTab(tab);

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/');
    };

    const fetchTracksForUser = async () => {
        try {
            setLoading(true);
            const response = await api.get('api/tracks/');
            setTracks(response.data);
        } catch (err) {
            setError('Не вдалося завантажити треки.');
        } finally {
            setLoading(false);
        }
    };

    const fetchTabData = async () => {
        try {
            setLoading(true);

            // Перевірка, чи userInfo визначений
            const endpoint =
                activeTab === 'followers'
                    ? `/api/followers/${userInfo.id}/followers`
                    : `/api/followers/${userInfo.id}/following`;

            const response = await api.get(endpoint);
            console.log('API response:', response); // Логування відповіді
            if (Array.isArray(response.data)) {
                // Додаємо аватар до кожного користувача, якщо його немає - ставимо placehold
                const updatedUsers = response.data.map(user => ({
                    ...user,
                    avatarUrl: user.avatar
                        ? `https://localhost:5017/api/Users/get-avatar?userId=${user.id}`
                        : 'https://placehold.co/50x50'
                }));
                setTabUsers(updatedUsers);
            } else {
                throw new Error('Дані мають неправильний формат');
            }
        } catch (err) {
            console.error('Error fetching tab data:', err);
            setError('Не вдалося завантажити дані для вкладки.');
        } finally {
            setLoading(false);
        }
    };

    const fetchUserFollowerFollowingCounts = async () => {
        const followersCounts = {};
        const followingCounts = {};
        try {
            await Promise.all(
                tabUsers.map(async (user) => {
                    const followersRes = await api.get(`/api/followers/${user.id}/followers`);
                    const followingRes = await api.get(`/api/followers/${user.id}/following`);
                    followersCounts[user.id] = followersRes.data.length;
                    followingCounts[user.id] = followingRes.data.length;
                })
            );
            setUserFollowersCounts(followersCounts);
            setUserFollowingCounts(followingCounts);
        } catch (err) {
            setError('Не вдалося завантажити кількість підписок/підписників.');
        }
    };

    const calculateTrackCounts = () => {
        const trackCounts = tabUsers.reduce((acc, user) => {
            const userTracks = tracks.filter((track) => track.userId === user.id);
            acc[user.id] = userTracks.length;
            return acc;
        }, {});
        setUserTrackCounts(trackCounts);
    };

    useEffect(() => {
        fetchTracksForUser();
    }, []);

    useEffect(() => {
        if (activeTab) {
            fetchTabData();
        }
    }, [activeTab]);

    useEffect(() => {
        console.log("userInfo:", userInfo?.id); // Логування даних користувача
        if (isAuthenticated && userInfo?.id) {
            fetchTabData();
        }
    }, [isAuthenticated, userInfo?.id]);



    useEffect(() => {
        if (tabUsers.length > 0) {
            fetchUserFollowerFollowingCounts();
        }
    }, [tabUsers]);

    useEffect(() => {
        if (tracks.length && tabUsers.length) {
            calculateTrackCounts();
        }
    }, [tracks, tabUsers]);

    const renderTabUsers = () => (
        <motion.div
            className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-6 mt-8"
            variants={{
                hidden: { opacity: 0 },
                visible: { opacity: 1, transition: { staggerChildren: 0.2 } },
            }}
        >
            {tabUsers.map((user) => (
                <motion.div
                    key={user.id}
                    className="bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition-all duration-300 transform hover:scale-105"
                    whileHover={{ scale: 1.05 }}
                    variants={{
                        hidden: { opacity: 0, y: 20 },
                        visible: { opacity: 1, y: 0 },
                    }}
                >
                    <div className="flex items-center gap-4">
                        <motion.img
                            src={user.avatarUrl || 'https://placehold.co/50x50'}
                            alt="Avatar"
                            className="w-16 h-16 rounded-full"
                            whileHover={{ rotate: 10 }}
                        />
                        <div>
                            <p className="font-bold text-lg text-gray-800">{user.username}</p>
                            <p className="text-sm text-gray-500">Кількість треків: {userTrackCounts[user.id] || 0}</p>
                            <p className="text-sm text-gray-500">
                                Підписників: {userFollowersCounts[user.id] || 0}, Підписок: {userFollowingCounts[user.id] || 0}
                            </p>
                        </div>
                    </div>
                </motion.div>
            ))}
        </motion.div>
    );

    if (loading) return <p>Завантаження...</p>;
    //if (error) return <p>{error}</p>;

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
                    <div className="flex items-center w-3/4">
                        <input
                            type="text"
                            placeholder="Пошук"
                            style={{color: 'black', fontFamily: 'Times New Roman, Times, serif'}}
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

                <div className="p-6 bg-gray-50 rounded-lg shadow-lg">
                    <motion.div
                        className="flex gap-4 mb-6"
                        initial={{ opacity: 0, y: -20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.5 }}
                    >
                        <motion.button
                            onClick={() => setActiveTab('following')}
                            className={`py-2 px-6 rounded-lg font-medium transition-all ${
                                activeTab === 'following'
                                    ? 'bg-blue-500 text-white shadow-lg scale-105'
                                    : 'bg-gray-300 text-gray-800 hover:bg-blue-500 hover:text-white hover:scale-105'
                            }`}
                            whileTap={{ scale: 0.95 }}
                        >
                            Підписки
                        </motion.button>
                        <motion.button
                            onClick={() => setActiveTab('followers')}
                            className={`py-2 px-6 rounded-lg font-medium transition-all ${
                                activeTab === 'followers'
                                    ? 'bg-blue-500 text-white shadow-lg scale-105'
                                    : 'bg-gray-300 text-gray-800 hover:bg-blue-500 hover:text-white hover:scale-105'
                            }`}
                            whileTap={{ scale: 0.95 }}
                        >
                            Підписники
                        </motion.button>
                    </motion.div>

                    <motion.div
                        initial="hidden"
                        animate="visible"
                        variants={{
                            hidden: { opacity: 0, y: 20 },
                            visible: {
                                opacity: 1,
                                y: 0,
                                transition: { staggerChildren: 0.2, duration: 0.6 },
                            },
                        }}
                    >
                        {renderTabUsers()}
                    </motion.div>
                </div>

            </main>
        </div>
    );
};

export default SubscriptionPage;
