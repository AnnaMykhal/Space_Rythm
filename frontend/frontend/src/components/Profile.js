import React, { useState, useEffect } from 'react';
import { useAuth } from './AuthContext';
import api from '../api';
import './Profile.css';
import {Link, useLocation, useNavigate} from "react-router-dom";
import {useSearch} from "./SearchContext";
import debounce from "lodash.debounce"; // CSS для кастомного стилю
import "font-awesome/css/font-awesome.min.css";
import {motion} from "framer-motion";

const Profile = () => {
    const { isAuthenticated, userInfo, login, setUserInfo  } = useAuth();
    const [followers, setFollowers] = useState(0);
    const [following, setFollowing] = useState(0);
    const [loading, setLoading] = useState(false);
    const [trackFile, setTrackFile] = useState(null);
    const [imageFile, setImageFile] = useState(null);
    const [title, setTitle] = useState("");
    const [artistName, setArtistName] = useState("");
    const [genre, setGenre] = useState("");
    const [tags, setTags] = useState("");
    const [description, setDescription] = useState("");
    const [categories, setCategories] = useState([]);
    const [selectedCategories, setSelectedCategories] = useState([]); // Для вибраних категорій
    const [avatarUploading, setAvatarUploading] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();
    const [error, setError] = useState(null);
    const [searchQuery] = useState('');
    const [searchResults, setSearchResults] = useState([]);
    const { setSearchQuery } = useSearch();
    const [uploadMessage, setUploadMessage] = useState("");
    const [showModal, setShowModal] = useState(false);
    const [trackUploading, setTrackUploading] = useState(false);
    const [tracks, setTracks] = useState([]);

    const Genre = {
        Rock: 1,
        Pop: 2,
        Hiphop: 3,
        Rap: 4,
        Electronic: 5,
        Classical: 6,
        Jazz: 7,
        Latin: 8,
        RAndB: 9,
        Soul: 10,
        Metal: 11,
        House: 12,
        Techno: 13,
        Trance: 14,
        Dubstep: 15,
        DrumAndBass: 16,
        Folk: 17,
        Reggae: 18,
        Indian: 19,
        Country: 20,
        Soundtracks: 21,
        Anime: 22,
        Chillout: 23,
        Hyperpop: 24,
        KPop: 25,
        Opera: 26,
        Choral: 27,
        PunkRock: 28,
        Grunge: 29,
        Blues: 30,
        Other: 31,
    };

    useEffect(() => {
        const fetchStats = async () => {
            if (userInfo?.id) {
                setLoading(true);
                try {
                    const followersResponse = await api.get(`/api/Followers/${userInfo.id}/followers`);
                    const followingResponse = await api.get(`/api/Followers/${userInfo.id}/following`);
                    setFollowers(followersResponse.data.length);
                    setFollowing(followingResponse.data.length);
                } catch (error) {
                    console.error('Failed to fetch stats:', error);
                } finally {
                    setLoading(false);
                }
            }
        };
        if (userInfo?.id) {
            fetchStats();
        }
    }, [userInfo]);

    const handleAvatarUpload = async (e) => {
        if (!isAuthenticated) {
            setUploadMessage("You must be logged in to upload an avatar.");
            return;
        }

        const file = e.target.files[0];
        if (!file) return;

        const allowedExtensions = ["jpg", "jpeg", "png", "gif"];
        const fileExtension = file.name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExtension)) {
            alert(`Invalid file type. Allowed: ${allowedExtensions.join(", ")}`);
            return;
        }

        const formData = new FormData();
        formData.append("avatar", file);

        setAvatarUploading(true);
        try {
            // Завантаження аватара
            const uploadResponse = await api.post('/api/Users/upload-avatar', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data',
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                },
            });

            setUploadMessage('Avatar uploaded successfully');

            // Отримання актуального URL аватара
            const avatarUrlResponse = await api.get('/api/Users/get-avatar-url', {
                params: { userId: userInfo.id },
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`,
                },
            });

            const newAvatarUrl = avatarUrlResponse.data.avatarUrl;

            // Оновлення стану користувача
            const updatedUserInfo = {
                ...userInfo,
                avatarUrl: `${newAvatarUrl}?t=${new Date().getTime()}`, // Додаємо timestamp для уникнення кешування
            };

            setUserInfo(updatedUserInfo);
            localStorage.setItem('userInfo', JSON.stringify(updatedUserInfo));

            console.log('Updated User Info:', updatedUserInfo);
        } catch (error) {
            console.error('Failed to upload avatar:', error);
            setUploadMessage(error.response?.data?.message || 'Failed to upload avatar.');
        } finally {
            setAvatarUploading(false);
        }
    };

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

    const handleSearchChange = (e) => {
        const query = e.target.value;
        setSearchQuery(query);
        navigate('/'); // Перенаправлення на головну сторінку
    };

    const handleTrackUpload = async () => {
        console.log("Track upload started");

        if (!isAuthenticated) {
            setUploadMessage("You must be logged in to upload a track.");
            return;
        }

        if (!trackFile || !title || !artistName || !genre || !selectedCategories.length) {
            setUploadMessage("All required fields (Track file, Title, Artist Name, Genre, Categories) must be filled.");
            return;
        }

        const formData = new FormData();
        formData.append("file", trackFile);
        formData.append("Title", title);
        formData.append("ArtistName", artistName);
        formData.append("Genre", parseInt(genre, 10));
        // Передаємо жанр, назва якого збігається з бекендом
        formData.append("Description", description || "");

        // Формат передачі масиву тегів
        tags.split(",").map((tag, index) => {
            const trimmedTag = tag.trim();
            if (trimmedTag) {
                formData.append(`Tags[${index}]`, trimmedTag);
            }
        });

        // Формат передачі масиву категорій
        selectedCategories.forEach((category, index) => {
            formData.append(`Categories[${index}]`, category);
        });

        if (imageFile) {
            formData.append("Image", imageFile);
        }

        console.log("FormData entries:");
        for (let pair of formData.entries()) {
            console.log(`${pair[0]}:`, pair[1]);
        }

        setTrackUploading(true);

        try {
            const response = await api.post("/api/Tracks/upload", formData, {
                headers: {
                    "Content-Type": "multipart/form-data",
                    "Authorization": `Bearer ${localStorage.getItem('userInfo')}`
                },
            });
            console.log("Upload successful:", response.data);
            setUploadMessage(`Track uploaded successfully: ${response.data.title}`);
            resetForm();
        } catch (error) {
            console.error("Failed to upload track:", error);
            setUploadMessage(error.response?.data?.message || "An error occurred while uploading the track.");
        } finally {
            setTrackUploading(false);
        }
    };

    const resetForm = () => {
        setTitle("");
        setDescription("");
        setArtistName("");
        setGenre("");
        setTags("");
        setSelectedCategories([]);
        setTrackFile(null);
        setImageFile(null);
    };


    const handleFileChange = (event, setFileCallback) => {
        const file = event.target.files[0];
        if (file) {
            setFileCallback(file);
        }
    };


    useEffect(() => {
        const fetchUserTracks = async () => {
            try {
                const response = await api.get("/api/Tracks/");
                const userTracks = response.data.filter(track => track.userId === userInfo?.id);
                setTracks(userTracks);
            } catch (error) {
                console.error("Error fetching user tracks:", error);
            }
        };

        fetchUserTracks();
    }, [userInfo?.id]);

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await api.get("/api/TrackCategories");
                console.log("Fetched Categories:", response.data); // Перевірка даних
                setCategories(response.data); // Перевірте, чи отримуєте масив з категоріями
            } catch (error) {
                console.error("Помилка завантаження категорій:", error);
            }
        };

        fetchCategories();
    }, []);

    const handleCategoryChange = (e) => {
        const selectedCategoryIds = Array.from(e.target.selectedOptions, option => option.value);
        setSelectedCategories(selectedCategoryIds);
        console.log("Selected Categories:", selectedCategoryIds); // Перевірка
    };

    if (!isAuthenticated) {
        return <div>Будь ласка, увійдіть у систему, щоб переглянути профіль.</div>;
    }

    if (!userInfo) return <p>Будь ласка, увійдіть у систему, щоб переглянути профіль.</p>;

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

                <section className="mt-6">
                    {/* Аватар */}
                    <div className="flex flex-col items-center">
                        <div className="relative">
                            <img
                                src={userInfo.avatarUrl || 'https://placehold.co/100x100'}
                                alt="Avatar"
                                className="w-24 h-24 rounded-full"
                            />
                            <div>
                                {userInfo.username}
                            </div>
                            <label
                                htmlFor="avatar-upload"
                                className="absolute bottom-0 right-0 bg-blue-500 text-white p-1 rounded-full cursor-pointer"
                            >
                                <i className="fas fa-pencil-alt"></i>
                                <input
                                    type="file"
                                    id="avatar-upload"
                                    className="hidden"
                                    onChange={handleAvatarUpload}
                                />
                            </label>
                        </div>
                    </div>

                    {/* Статистика */}
                    <div className="mt-4 flex justify-center">
                        <div className="mr-6 text-center">
                            <p className="text-gray-600">Підписники</p>
                            <p className="text-xl font-bold">{followers}</p>
                        </div>
                        <div className="text-center">
                            <p className="text-gray-600">Підписки</p>
                            <p className="text-xl font-bold">{following}</p>
                        </div>
                    </div>

                    {/* Відео */}
                    <div className="mt-6">
                        <h2 className="text-lg font-bold">Ваші треки</h2>
                        <div className="grid grid-cols-3 gap-4 mt-4">
                            {tracks.map((track) => (
                                <div
                                    key={track.trackId}
                                    className="bg-white shadow-md rounded-lg overflow-hidden flex flex-col"
                                >
                                    <img
                                        src={track.imagePath || "https://placehold.co/300x300"}
                                        alt={track.title}
                                        className="h-48 w-full object-cover"
                                    />
                                    <div className="p-4 flex flex-col justify-between">
                                        <h3 className="font-bold text-lg mb-2">{track.title}</h3>
                                        <p className="text-gray-600">{track.artistName}</p>
                                    </div>
                                </div>
                            ))}
                            <div
                                className="bg-blue-500 text-white text-xl rounded-lg flex justify-center items-center"
                                onClick={() => setShowModal(true)}
                            >
                                <i className="fa fa-plus"></i>
                            </div>
                        </div>
                    </div>
                </section>
            </main>

            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50 animate__animated animate__fadeIn">
                    <div className="bg-white p-6 rounded-lg w-1/3 animate__animated animate__zoomIn">
                        <h2 className="text-xl font-bold mb-4">Upload Track</h2>
                        <input
                            type="text"
                            placeholder="Track Title *"
                            value={title}
                            onChange={(e) => setTitle(e.target.value)}
                            className="border p-2 w-full mb-2"
                        />
                        <input
                            type="text"
                            placeholder="Artist Name *"
                            value={artistName}
                            onChange={(e) => setArtistName(e.target.value)}
                            className="border p-2 w-full mb-2"
                        />
                        <label className="block mb-2">Genre *</label>
                        <select
                            value={genre}
                            onChange={(e) => setGenre(e.target.value)}
                            className="border p-2 w-full mb-2"
                        >
                            <option value="" disabled>Select a genre</option>
                            {Object.keys(Genre).map((key) => (
                                <option key={Genre[key]} value={Genre[key]}>
                                    {key}
                                </option>
                            ))}
                        </select>

                        <input
                            type="text"
                            placeholder="Tags (comma separated)"
                            value={tags}
                            onChange={(e) => setTags(e.target.value)}
                            className="border p-2 w-full mb-2"
                        />
                        <label className="block mb-2">Categories *</label>
                        <select
                            id="categories"
                            multiple
                            value={selectedCategories}
                            onChange={(e) => {
                                const selectedValues = Array.from(e.target.selectedOptions, option => option.value);
                                setSelectedCategories(selectedValues);
                            }}
                            className="border p-2 w-full mb-2"
                        >
                            {categories.map(category => (
                                <option key={category.id} value={category.category}>
                                    {category.category}
                                </option>
                            ))}
                        </select>
                        <textarea
                            placeholder="Description"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            className="border p-2 w-full mb-2"
                        />
                        <label className="block mb-2">Upload Track *</label>
                        <input
                            type="file"
                            accept=".mp3,.wav"
                            onChange={(e) => handleFileChange(e, setTrackFile)}
                            className="mb-2"
                        />
                        <label className="block mb-2">Upload Image (optional)</label>
                        <input
                            type="file"
                            accept=".jpg,.jpeg,.png,.gif"
                            onChange={(e) => setImageFile(e.target.files[0])}
                            className="mb-2"
                        />
                        <button
                            onClick={handleTrackUpload}
                            disabled={trackUploading}
                            className="bg-green-500 text-white p-2 rounded w-full mb-2"
                        >
                            {trackUploading ? "Uploading..." : "Submit Track"}
                        </button>
                        <button
                            onClick={() => setShowModal(false)}
                            className="bg-red-500 text-white p-2 rounded w-full"
                        >
                            Close
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Profile;
