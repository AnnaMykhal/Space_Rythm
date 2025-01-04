import React, { useState, useEffect, useRef } from 'react';
import {useParams, useNavigate, useLocation, Link} from 'react-router-dom';
import api from '../api';
import { useAuth } from './AuthContext';
import {FaHeart, FaRegHeart, FaComment, FaHeadphones, FaPen, FaTrashAlt, FaShareAlt, FaBookmark} from 'react-icons/fa';
import debounce from "lodash.debounce";
import {useSearch} from "./SearchContext";
import './TrackPage.css';
import { motion } from 'framer-motion';

const TrackPage = () => {
    const { trackId } = useParams();
    const navigate = useNavigate();
    const location = useLocation(); // Для визначення поточної сторінки
    const audioRef = useRef(null);
    const { isAuthenticated, userInfo } = useAuth();

    const [track, setTrack, setTrackId] = useState(null);
    const [comments, setComments] = useState([]);
    const [likes, setLikes] = useState(0);
    const [isLiked, setIsLiked] = useState(false);
    const [isPlaying, setIsPlaying] = useState(false);
    const [volume, setVolume] = useState(1);
    const [newComment, setNewComment] = useState("");
    const [listeningCount, setListeningCount] = useState(0);
    const [editingCommentId, setEditingCommentId] = useState(null); // Стан для редагування коментаря
    const [editingCommentContent, setEditingCommentContent] = useState(""); // Стан для вмісту редагованого коментаря
    const [tracks, setTracks] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [searchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const { setSearchQuery } = useSearch();
    const [playlists, setPlaylists] = useState([]);
    const [selectedPlaylistId, setSelectedPlaylistId] = useState('');
    const [newPlaylistName, setNewPlaylistName] = useState("");
    const [imageFile, setImageFile] = useState(null);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [trackUploader, setTrackUploader] = useState(null); // Інформація про користувача
    const [isFollowing, setIsFollowing] = useState(false); // Статус підписки
    const [userDetails, setUserDetails] = useState(null);  // Додайте цей рядок для створення стейту
    const [likedComments, setLikedComments] = useState({})
    const [likesCount, setLikesCount] = useState({});
    const [description, setDescription] = useState("");
    const [currentTrackId, setCurrentTrackId] = useState(null);

    useEffect(() => {
        // Перевіряємо, чи трек доступний
        if (track && track.trackId) {
            setCurrentTrackId(track.trackId); // Присвоєння ID після завантаження
        }
    }, [track]); // Виконувати хук, якщо змінюється `track`

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

    const fetchTrackData = async () => {
        try {
            const trackResponse = await api.get(`/api/Tracks/`);
            const trackData = trackResponse.data.find(track => track.trackId === parseInt(trackId));
            setTrack(trackData);
            setLikes(trackData.likes);
            setListeningCount(trackData.plays);

            if (isAuthenticated) {
                const likedTracksResponse = await api.get(`/api/likes/user/${userInfo.id}`);
                const likedTracks = likedTracksResponse.data;
                setIsLiked(likedTracks.some(track => track.trackId === parseInt(trackId)));
            }

            const trackUploaderResponse = await api.get(`/api/tracks/by-id/${trackId}`);
            console.log('Track Uploader Data:', trackUploaderResponse.data);
            setTrackUploader(trackUploaderResponse.data);

            const commentsResponse = await api.get(`/api/comments/${trackId}`);
            setComments(commentsResponse.data);
        } catch (err) {
            console.error("Error fetching track data:", err);
            //navigate("/");
        }
    };

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    const addListening = async () => {
        try {
            await api.post('/api/listenings', {
                trackId: parseInt(trackId),
            });
            setListeningCount((prev) => prev + 1);
            console.log('Listening add')
        } catch (err) {
            console.error("Помилка при додаванні прослуховування", err);
        }
        // Перезагрузить текущую страницу
    };

    useEffect(() => {
        if (trackId) {
            fetchTrackData();
            addListening();
        }
    }, [trackId]);

    const handleLike = async () => {
        if (!isAuthenticated) {
            alert("Увійдіть, щоб ставити лайки!");
            return;
        }

        try {
            if (isLiked) {
                await api.delete(`/api/likes`, { params: { userId: userInfo.id, trackId } });
                setLikes((prev) => prev - 1);
            } else {
                await api.post('/api/likes', { userId: userInfo.id, trackId });
                setLikes((prev) => prev + 1);
            }
            setIsLiked(!isLiked);
        } catch (err) {
            console.error("Помилка при ставленні лайку", err);
        }
    };

    const fetchFollowStatus = async () => {
        if (!isAuthenticated) {
            setIsFollowing(false);
        }

        try {
            const response = await api.get(`/api/Followers/${userInfo.id}/following`);
            const following = response.data;

            // Перевіряємо, чи підписаний поточний користувач на автора треку
            const isAlreadyFollowing = following.some(follow => follow.id === trackUploader.userId);
            console.log("Is Following:", isAlreadyFollowing);

            setIsFollowing(isAlreadyFollowing);
        } catch (err) {
            console.error("Error fetching follow status:", err);
        }
    };

    const fetchUserDetails = async () => {
        try {
            console.log(`id: ${trackUploader.userId}`)
            const response = await api.get(`/api/users/${trackUploader.userId}`);
            console.log("User details:", response.data);
            setUserDetails(response.data); // Зберігаємо отримані дані про користувача в стейт
        } catch (err) {
            console.error("Error fetching user details:", err);
        }
    };

    const handleFollow = async () => {
        if (!isAuthenticated) {
            alert("Увійдіть, щоб підписуватися!");
            return;
        }

        if (userInfo.id === trackUploader.userId) {
            alert("Ви не можете підписатися на самого себе.");
            return;
        }

        try {
            console.log("isFollowing before:", isFollowing);

            if (isFollowing) {
                // Відписка
                await api.delete(`/api/Followers/${userInfo.id}/unfollow/${trackUploader.userId}`);
                console.log("Unfollowed");
            } else {
                // Підписка
                await api.post(`/api/Followers/${userInfo.id}/follow/${trackUploader.userId}`);
                console.log("Followed");
            }
        } catch (err) {
            console.error("Помилка при підписці:", err.response?.data || err.message);
        } finally {
            // Завжди оновлюємо статус після запиту
            fetchFollowStatus();
        }
    };

    useEffect(() => {
        console.log(`trackUploader: ${trackUploader?.userId}`);
        if (trackUploader?.userId) {
            fetchFollowStatus();
            fetchUserDetails();
        }
    }, [trackUploader?.userId]);



    const handleCommentLike = async (commentId) => {
        if (!isAuthenticated) {
            alert('Увійдіть, щоб ставити лайки!');
            return;
        }

        const isLiked = likedComments[commentId] || false; // Чи вже лайкнутий

        try {
            if (isLiked) {
                // Видалення лайка
                await api.delete(`/api/comments/likes`, { params: { userId: userInfo.id, commentId } });

                // Оновлення стану
                setLikedComments((prevState) => ({
                    ...prevState,
                    [commentId]: false,
                }));
                setLikesCount((prevCount) => ({
                    ...prevCount,
                    [commentId]: Math.max(0, prevCount[commentId] - 1), // Зменшення кількості лайків
                }));
            } else {
                // Додавання лайка
                await api.post(`/api/comments/likes`, { userId: userInfo.id, commentId });

                // Оновлення стану
                setLikedComments((prevState) => ({
                    ...prevState,
                    [commentId]: true,
                }));
                setLikesCount((prevCount) => ({
                    ...prevCount,
                    [commentId]: isLiked
                        ? Math.max(0, prevCount[commentId] - 1)
                        : (prevCount[commentId] || 0) + 1,
                }));

            }

            // Зберігаємо стан у localStorage
            localStorage.setItem(`likedComment_${commentId}`, !isLiked);
        } catch (err) {
            console.error('Помилка при ставленні лайку до коментаря', err);
        }
    };

    useEffect(() => {
        // Завантажуємо стан лайків із localStorage під час завантаження компоненту
        const initialLikes = {};
        comments.forEach((comment) => {
            const liked = localStorage.getItem(`likedComment_${comment.commentId}`);
            if (liked !== null) {
                initialLikes[comment.commentId] = JSON.parse(liked);
            }
        });
        setLikedComments(initialLikes);
    }, [comments]);

    useEffect(() => {
        const loadLikesCount = async () => {
            const counts = { ...likesCount }; // Зберігаємо існуючі значення
            comments.forEach((comment) => {
                // Якщо likesCount для цього коментаря ще не завантажено, використовуємо дані з API
                if (counts[comment.commentId] === undefined) {
                    counts[comment.commentId] = comment.likesCount;
                }
            });
            setLikesCount(counts); // Оновлюємо стан лайків
        };

        loadLikesCount();
    }, [comments]);

    const fetchLikesCount = async (commentId) => {
        try {
            const response = await api.get(`/api/comments/${commentId}`);
            return response.data;
        } catch (err) {
            console.error('Помилка при отриманні кількості лайків', err);
            return likesCount[commentId] || 0; // Повертаємо існуюче значення у разі помилки
        }
    };

    useEffect(() => {
        // Оновлення стану лайка для всіх коментарів
        comments.forEach((comment) => {
            const liked = localStorage.getItem(`likedComment_${comment.commentId}`);
            if (liked !== null) {
                // Оновлення стану лайка кожного коментаря
                setIsLiked(JSON.parse(liked));
            }
        });
    }, [comments]);

    const handleCommentSubmit = async () => {
        if (!isAuthenticated) {
            alert('Увійдіть, щоб коментувати!');
            return;
        }

        if (!isAuthenticated || newComment.trim() === "") return;

        try {
            await api.post('/api/comments', {
                userId: userInfo.id,
                trackId,
                content: newComment,
            });
            setNewComment("");
            fetchComments(); // Оновлюємо коментарі після додавання
        } catch (err) {
            console.error('Error adding comment:', err);
        }
    };

    const fetchComments = async () => {
        try {
            const response = await api.get(`/api/comments/${trackId}`);
            setComments(response.data);
        } catch (err) {
            console.error('Error fetching comments:', err);
        }
    };

    useEffect(() => {
        if (trackId) {
            fetchComments();
        }
    }, [trackId]);

    const handleDeleteComment = async (commentId) => {
        try {
            await api.delete(`/api/comments/${commentId}`);
            fetchComments(); // Оновлюємо коментарі після видалення
        } catch (err) {
            console.error('Error deleting comment:', err);
        }
    };

    const handleEditComment = (commentId, content) => {
        setEditingCommentId(commentId); // Встановлюємо ID редагованого коментаря
        setEditingCommentContent(content); // Встановлюємо поточний вміст коментаря для редагування
    };

    // Функція для збереження редагованого коментаря
    const saveEditedComment = async () => {
        try {
            await api.put('/api/comments', {
                commentId: editingCommentId,
                content: editingCommentContent,
            });
            fetchComments(); // Оновлюємо коментарі після редагування
            setEditingCommentId(null); // Скидаємо стан редагування
            setEditingCommentContent(""); // Очищаємо вміст редагованого коментаря
        } catch (err) {
            console.error('Error updating comment:', err);
        }
    };

    const handleShare = () => {
        const trackUrl = `${window.location.origin}/track/${track.trackId}`; // Формуємо URL треку
        navigator.clipboard.writeText(trackUrl)
            .then(() => {
                alert('Посилання скопійовано до буфера обміну!');
            })
            .catch((err) => {
                console.error('Помилка копіювання посилання:', err);
                alert('Не вдалося скопіювати посилання.');
            });
    };

    function getCorrectEnding(count) {
        const lastDigit = count % 10;
        const lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
            return 'Прослуховувань';
        }

        switch (lastDigit) {
            case 1:
                return 'Прослуховування';
            case 2:
            case 3:
            case 4:
                return 'Прослуховування';
            default:
                return 'Прослуховувань';
        }
    }

    const fetchPlaylists = async () => {
        try {
            setLoading(true);
            const response = await api.get(`/api/playlists/user/${userInfo.id}`);
            console.log(`playlist: ${response.data}`)
            setPlaylists(response.data);
        } catch (err) {
            setError("Не вдалося завантажити плейлисти.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (isModalOpen) {
            fetchPlaylists();
        }
    }, [isModalOpen]);

    const handleAddToPlaylist = async (playlistId) => {
        try {
            await api.post(`/api/playlists/${playlistId}/add-track`, { trackId });
            alert("Трек додано до плейлиста!");
            setIsModalOpen(false);
        } catch (err) {
            setError("Помилка при додаванні треку до плейлиста.");
            alert('Не вдалося додати трек до плейлиста.');
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
        formData.append("description", description); // Опис плейлиста (можна залишити порожнім)
        formData.append("trackIds", [trackId]); // Перетворення масиву у строку JSON
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

    const playNextTrack = () => {
        const currentIndex = tracks.findIndex(track => track.trackId === trackId);
        console.log("Current index for next track:", currentIndex);
        if (currentIndex >= 0 && currentIndex < tracks.length - 1) {
            const nextTrack = tracks[currentIndex + 1];
            setTrack(nextTrack);
            setTrackId(nextTrack.trackId);
            audioRef.current.load();
            audioRef.current.play();
        } else {
            console.log("Це останній трек у списку або trackId не знайдено.");
        }
    };

    const playPreviousTrack = () => {
        const currentIndex = tracks.findIndex(track => track.trackId === trackId);
        console.log("Current index for previous track:", currentIndex);
        if (currentIndex > 0) {
            const previousTrack = tracks[currentIndex - 1];
            setTrack(previousTrack);
            setTrackId(previousTrack.trackId);
            audioRef.current.load();
            audioRef.current.play();
        } else {
            console.log("Це перший трек у списку або trackId не знайдено.");
        }
    };

    useEffect(() => {
        addToHistory();
    }, [trackId, userInfo?.id]);

    const addToHistory = async () => {
        try {
            console.log(userInfo.id);
            console.log(trackId);

            // Додаємо новий запис до історії кожного разу
            const response = await api.post('/api/listening-history', {
                userId: userInfo.id,
                trackId: trackId,
                lastListenedAt: new Date(), // Додаємо час прослуховування
            }, {
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                },
            });

            console.log('Track added to history:', response.data);
        } catch (error) {
            console.error('Failed to add track to history:', error.response?.data || error.message);
        }
    };


    //const audioUrl = `${track.filePath}`;
    //console.log(`trackId:`, track.filePath);


    if (loading) return <div>Завантаження...</div>;
    if (error) return <div>{error}</div>;

    if (!track) return <div>Трек не знайдено!</div>;

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
                                className={`block py-2 px-4 rounded-lg ${
                                    (location.pathname === path ||
                                        (path === '/' && location.pathname.startsWith('/track')))
                                        ? 'bg-blue-500 text-white'
                                        : 'hover:bg-gray-700'
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

                {/* Контент треку */}
                <div className="flex-1 flex flex-col lg:flex-row p-6 space-y-4 lg:space-y-0 lg:space-x-4">
                    <div className="flex-1 bg-white rounded-lg shadow-lg p-6 border border-gray-300">
                        <img
                            src={track.imagePath || "/placeholder-image.jpg"}
                            alt={track.title}
                            className="w-full h-48 object-cover rounded-lg mb-4"
                        />
                        <h1 className="text-3xl font-bold mt-4 text-gray-800">{track.title}</h1>
                        <p className="text-lg text-gray-600">{track.artistName}</p>

                        {/* Виведення кількості прослуховувань */}
                        <div className="flex items-center mt-4 space-x-4">
                            <FaHeadphones className="text-gray-600"/>
                            <span className="text-gray-700">{track.plays} {getCorrectEnding(listeningCount)}</span>
                        </div>

                        {/* Аудіоплеєр */}
                        <div className="mt-4">
                            <div className="flex justify-between items-center mb-2">
                                {track && track.filePath ? (
                                    <audio
                                        ref={audioRef}
                                        src={track.filePath}
                                        controls
                                        className="w-full"
                                        onPlay={() => setIsPlaying(true)}
                                        onPause={() => setIsPlaying(false)}
                                        onEnded={playNextTrack}
                                    />
                                ) : (
                                    <p>Аудіо не доступне для відтворення.</p>
                                )}
                            </div>
                        </div>


                        {trackUploader ? (
                            <div className="flex items-center mt-4 space-x-4 bg-white shadow-md rounded-lg p-4">
                                {userDetails ? (
                                    <div className="flex items-center space-x-3">
                                        <img
                                            src={userDetails.avatarUrl || 'https://placehold.co/50x50'}
                                            alt="User Avatar"
                                            className="w-12 h-12 rounded-full border border-gray-300"
                                        />
                                        <div>
                                            <p className="text-lg font-medium text-gray-700">{userDetails.username}</p>
                                            <p className="text-sm text-gray-500">Автор</p>
                                        </div>
                                    </div>
                                ) : (
                                    <p className="text-gray-500">Завантаження даних користувача...</p>
                                )}

                                <div className="ml-auto">
                                    {isAuthenticated ? (
                                        trackUploader && userInfo && userInfo.id !== trackUploader.userId && (
                                            <button
                                                onClick={handleFollow}
                                                className={`px-4 py-2 text-sm rounded-lg shadow ${
                                                    isFollowing
                                                        ? "bg-red-500 text-white hover:bg-red-600"
                                                        : "bg-blue-500 text-white hover:bg-blue-600"
                                                } transition`}
                                            >
                                                {isFollowing ? "Відписатися" : "Підписатися"}
                                            </button>
                                        )
                                    ) : (
                                        <button
                                            onClick={() => alert("Авторизуйтесь, щоб підписатися!")}
                                            className="px-4 py-2 text-sm rounded-lg bg-gray-400 text-white shadow hover:bg-gray-500 transition"
                                        >
                                            Підписатися
                                        </button>
                                    )}
                                </div>
                            </div>
                        ) : (
                            <p className="text-gray-500">Завантаження...</p>
                        )}


                        {/* Лайки, коментарі */}
                        <div className="flex items-center mt-4 space-x-4">
                            {/* Кнопка лайка */}
                            <button
                                onClick={handleLike}
                                className="flex items-center space-x-2 rounded p-2 transition-colors hover:bg-gray-100"
                                style={{ backgroundColor: 'transparent' }}
                            >
                                {isLiked ? (
                                    <FaHeart className="text-red-500"/>
                                ) : (
                                    <FaRegHeart className="text-gray-600"/>
                                )}
                                <span className="text-black">{likes}</span>
                            </button>

                            {/* Кількість коментарів */}
                            <div className="flex items-center space-x-2">
                                <FaComment className="text-gray-600"/>
                                <span className="text-black">{comments.length}</span>
                            </div>

                            {/* Кнопка поділитися */}
                            <button
                                onClick={handleShare}
                                className="flex items-center space-x-2 rounded p-2 transition-colors hover:bg-gray-100"
                                style={{ backgroundColor: 'transparent' }}
                            >
                                <FaShareAlt className="text-gray-600"/>
                                <span className="text-black">Поділитися</span>
                            </button>

                            {/* Іконка додавання до плейлиста */}
                            <button
                                onClick={() =>
                                    isAuthenticated
                                        ? setIsModalOpen(true)
                                        : alert("Авторизуйтесь, щоб додати до плейлиста!")
                                }
                                className="flex items-center space-x-2 rounded p-2 transition-colors hover:bg-gray-100"
                                style={{ backgroundColor: 'transparent' }}
                            >
                                <FaBookmark className="text-gray-600"/>
                                <span className="text-black">Додати до плейлиста</span>
                            </button>
                        </div>



                        <div className="mt-4">
                            <h2 className="text-xl font-semibold">Коментарі</h2>
                            {comments.length === 0 ? (
                                <p>Немає коментарів</p>
                            ) : (
                                comments.map((comment) => (
                                    <div key={comment.commentId} className="bg-gray-100 p-4 rounded-lg mt-4 relative">
                                        {editingCommentId === comment.commentId ? (
                                            <div>
                        <textarea
                            className="border w-full p-3 rounded-lg"
                            value={editingCommentContent}
                            onChange={(e) => setEditingCommentContent(e.target.value)}
                        />
                                                <button
                                                    className="bg-blue-500 text-white py-2 px-4 rounded mt-2"
                                                    onClick={saveEditedComment}
                                                >
                                                    Зберегти
                                                </button>
                                                <button
                                                    className="bg-gray-500 text-white py-2 px-4 rounded mt-2 ml-2"
                                                    onClick={() => setEditingCommentId(null)}
                                                >
                                                    Скасувати
                                                </button>
                                            </div>
                                        ) : (
                                            <div>
                                                <p>
                                                    <strong>{comment.userName}:</strong> {comment.content}
                                                </p>
                                                <small className="text-gray-500">
                                                    {new Date(comment.postedDate).toLocaleString()}
                                                </small>
                                                {comment.isUpdated && (
                                                    <small className="text-gray-500 ml-2">(змінено)</small>
                                                )}

                                                {/* Кнопки редагування та видалення тільки для автора коментаря */}
                                                {isAuthenticated && comment.userId === userInfo.id && (
                                                    <div className="flex space-x-4 mt-2 absolute top-2 right-2">
                                                        <FaPen
                                                            className="text-gray-600 cursor-pointer"
                                                            onClick={() => handleEditComment(comment.commentId, comment.content)}
                                                        />
                                                        <FaTrashAlt
                                                            className="text-red-500 cursor-pointer"
                                                            onClick={() => handleDeleteComment(comment.commentId)}
                                                        />
                                                    </div>
                                                )}

                                                {/* Кнопка лайка */}
                                                <div className="mt-2 flex items-center space-x-2">
                                                    <button
                                                        onClick={() => handleCommentLike(comment.commentId)}
                                                        className="flex items-center space-x-2 rounded p-2 transition-colors hover:bg-gray-100"
                                                        style={{backgroundColor: 'transparent'}}
                                                    >
                                                        {likedComments[comment.commentId] ? (
                                                            <FaHeart className="text-red-500"/>
                                                        ) : (
                                                            <FaRegHeart className="text-gray-600"/>
                                                        )}
                                                        <span className="text-black">
                                                            {likesCount[comment.commentId] || comment.likesCount}
                                                        </span>
                                                    </button>
                                                </div>


                                            </div>
                                        )}
                                    </div>
                                ))
                            )}

                            {/* Додавання нового коментаря */}
                            {isAuthenticated ? (
                                <div className="mt-4">
            <textarea
                className="border w-full p-3 rounded-lg"
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                placeholder="Ваш коментар..."
            />
                                    <button
                                        className="bg-blue-500 text-white py-2 px-4 rounded mt-2"
                                        onClick={handleCommentSubmit}
                                    >
                                        Надіслати
                                    </button>
                                </div>
                            ) : (
                                <p className="mt-2 text-gray-500">Увійдіть, щоб коментувати.</p>
                            )}
                        </div>

                    </div>
                    <div className="w-1/4 bg-gray-100 p-4 rounded-lg shadow-md">
                        <h2 className="text-2xl font-bold mb-6 text-gray-800">Інші треки</h2>
                        <ul className="flex flex-col space-y-4">
                            {tracks
                                .filter((track) => track.trackId !== currentTrackId) // Виключаємо активний трек
                                .map((track) => (
                                    <li
                                        key={track.trackId}
                                        className="flex items-center bg-white shadow-md hover:shadow-lg rounded-lg p-4 transition transform hover:-translate-y-1 cursor-pointer"
                                        onClick={() => navigate(`/track/${track.trackId}`)}
                                    >
                                        <img
                                            src={track.imagePath || "/placeholder-image.jpg"}
                                            alt={track.title}
                                            className="w-16 h-16 object-cover rounded-full mr-4"
                                        />
                                        <div>
                                            <p className="font-bold text-gray-700 truncate">{track.title}</p>
                                            <p className="text-sm text-gray-500 truncate">{track.artistName}</p>
                                        </div>
                                    </li>
                                ))}
                        </ul>
                    </div>
                </div>
                {/* Спливаюче вікно для додавання до плейлиста */}
                {isModalOpen && (
                    <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center">
                        <div className="bg-white rounded-lg shadow-lg p-6 w-96 relative">
                            <h2 className="text-xl font-semibold mb-4">Додати до плейлиста</h2>

                            {loading ? (
                                <p className="text-gray-500">Завантаження...</p>
                            ) : playlists.length > 0 ? (
                                <div>
                                    <label
                                        htmlFor="existingPlaylist"
                                        className="block text-sm font-medium text-gray-700 mb-2"
                                    >
                                        Оберіть плейлист:
                                    </label>
                                    {loading ? (
                                        <p className="text-gray-500">Завантаження плейлистів...</p>
                                    ) : error ? (
                                        <p className="text-red-500">{error}</p>
                                    ) : playlists.length > 0 ? (
                                        <select
                                            id="existingPlaylist"
                                            className="w-full px-4 py-2 border border-gray-300 rounded-lg mb-4"
                                            onChange={(e) => setSelectedPlaylistId(e.target.value)}
                                            value={selectedPlaylistId}
                                        >
                                            <option value="" disabled>
                                                Виберіть плейлист
                                            </option>
                                            {playlists.map((playlist) => (
                                                <option key={playlist.playlistId} value={playlist.playlistId}>
                                                    {playlist.title}
                                                </option>
                                            ))}
                                        </select>
                                    ) : (
                                        <p className="text-gray-500">У вас немає створених плейлистів.</p>
                                    )}

                                    <button
                                        onClick={() => handleAddToPlaylist(selectedPlaylistId)}
                                        disabled={!selectedPlaylistId}
                                        className={`w-full py-2 px-4 ${
                                            selectedPlaylistId
                                                ? 'bg-blue-500 hover:bg-blue-600'
                                                : 'bg-gray-300 cursor-not-allowed'
                                        } text-white rounded-lg transition`}
                                    >
                                        Додати до обраного плейлиста
                                    </button>
                                </div>
                            ) : (
                                <p className="text-gray-500 mb-4">
                                    У вас немає створених плейлистів. Створіть новий!
                                </p>
                            )}

                            <h3 className="text-lg font-medium mt-4">Створити новий плейлист:</h3>
                            <input
                                type="text"
                                placeholder="Назва плейлиста"
                                value={newPlaylistName}
                                onChange={(e) => setNewPlaylistName(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg mt-2"
                                required
                            />
                            <textarea
                                placeholder="Опис плейлиста"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg mt-2"
                            />
                            <div className="mt-2">
                                <label className="block text-sm font-medium text-gray-700">
                                    Фото плейлиста (необов'язково):
                                </label>
                                <input
                                    type="file"
                                    onChange={(e) => handleImageChange(e)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg mt-2"
                                    accept="image/*"
                                />
                            </div>
                            <div className="flex justify-between mt-4">
                                <button
                                    onClick={handleCreatePlaylist}
                                    className="py-2 px-4 bg-green-500 text-white rounded-lg hover:bg-green-600 transition"
                                >
                                    Створити
                                </button>
                                <button
                                    onClick={() => setIsModalOpen(false)}
                                    className="py-2 px-4 bg-gray-400 text-white rounded-lg hover:bg-gray-500 transition"
                                >
                                    Закрити
                                </button>
                            </div>
                        </div>
                    </div>
                )}


            </main>
        </div>
    );
};

export default TrackPage;
