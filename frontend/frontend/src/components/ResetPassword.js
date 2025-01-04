import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { TextField, Button, Typography, Alert, Box, Container } from '@mui/material';
import api from '../api';

const ResetPassword = () => {
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [message, setMessage] = useState(null);
    const [error, setError] = useState(null);
    const location = useLocation();
    const navigate = useNavigate();

    // Витягуємо токен та email з параметрів URL
    const queryParams = new URLSearchParams(location.search);
    const token = queryParams.get('token');
    const email = queryParams.get('email');

    useEffect(() => {
        if (!token || !email) {
            setError('Невірні параметри для скидання паролю.');
        }
    }, [token, email]);

    const handleResetPassword = async () => {
        if (password !== confirmPassword) {
            setError('Паролі не співпадають.');
            return;
        }
        try {
            const response = await api.post(
                '/api/RegistrationAuthorization/reset-password',
                JSON.stringify({
                    email,
                    token,
                    password,
                    confirmPassword,
                }),
                {
                    headers: {
                        'Content-Type': 'application/json',
                    }
                }
            );
            setMessage('Пароль успішно скинуто!');
            setError(null);
            setTimeout(() => {
                navigate('/');
            }, 2000);
        } catch (error) {
            console.error(error.response?.data || error.message);
            setError('Помилка скидання паролю. Будь ласка, спробуйте ще раз.');
        }
    };

    return (
        <Container maxWidth="xs" sx={{ mt: 4 }}>
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    padding: 3,
                    borderRadius: 2,
                    boxShadow: 3,
                    backgroundColor: 'white',
                }}
            >
                <Typography variant="h5" component="h2" sx={{ mb: 2 }}>
                    Скидання паролю
                </Typography>
                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
                {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
                <TextField
                    label="Новий пароль"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    fullWidth
                    variant="outlined"
                    margin="normal"
                />
                <TextField
                    label="Підтвердження пароля"
                    type="password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    fullWidth
                    variant="outlined"
                    margin="normal"
                />
                <Button
                    variant="contained"
                    color="primary"
                    onClick={handleResetPassword}
                    fullWidth
                    sx={{ mt: 2 }}
                >
                    Скинути пароль
                </Button>
            </Box>
        </Container>
    );
};

export default ResetPassword;
