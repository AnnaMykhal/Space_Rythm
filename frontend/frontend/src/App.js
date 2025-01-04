import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import LoginRegistration from './components/LoginRegestration';
import ForgotPassword from './components/ForgotPassword';
import ResetPassword from './components/ResetPassword';
import ConfirmEmail from "./components/EmailConfirmation";
import MainPage from './components/MainPage';
import Profile from './components/Profile'
import TrackPage from "./components/TrackPage";
import { AuthProvider, useAuth } from './components/AuthContext';
import {SearchProvider} from "./components/SearchContext";
import SavedPage from "./components/SavedPage";
import SubscriptionPage from "./components/SubscriptionsPage";
import HistoryPage from "./components/HistoryPage";
import SettingsPage from "./components/SettingsPage";
import PlaylistPage from "./components/PlaylistPage";
import EditPlaylistPage from './components/EditPlaylistPage';
import CategoriesPage from "./components/CategoriesPage";
import CategoryTracksPage from "./components/CategoryTracksPage";
import NewsPage from "./components/NewsPage";

function App() {
    return (
        <SearchProvider>
            <AuthProvider>
                <Router>
                    <Routes>
                        <Route path="/" element={<MainPage />} />
                        <Route path="/auth" element={<LoginRegistration />} />
                        <Route path="/forgot-password" element={<ForgotPassword />} />
                        <Route path="/reset-password" element={<ResetPassword />} />
                        <Route path="/confirm-email" element={<ConfirmEmail />} />
                        <Route path="/profile" element={<Profile />} />
                        <Route path="/track/:trackId" element={<TrackPage />} />
                        <Route path="/saved" element={<SavedPage />} />
                        <Route path="/subscriptions" element={<SubscriptionPage />} />
                        <Route path="/history" element={<HistoryPage />} />
                        <Route path="/settings" element={<SettingsPage />} />
                        <Route path="/playlist/:playlistId" element={<PlaylistPage />} />
                        <Route path="/edit-playlist/:playlistId" element={<EditPlaylistPage />} />
                        <Route path="/categories" element={<CategoriesPage />} />
                        <Route path="/category/:categoryId" element={<CategoryTracksPage />} />
                        <Route path="/news" element={<NewsPage/>} />
                    </Routes>
                </Router>
            </AuthProvider>
        </SearchProvider>
    );
}

export default App;
