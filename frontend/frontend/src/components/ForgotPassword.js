import React, { useState } from 'react';
import api from '../api';
import './ForgotPassword.css';
import {useNavigate} from "react-router-dom";
import {Alert, Button, Typography} from "antd";
import {TextField} from "@mui/material";
import {Box} from "@mui/system";

const ForgotPassword = () => {
    const [email, setEmail] = useState('');
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const validateEmail = (email) => {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    };

    const handleForgotPassword = async () => {
        setMessage('');
        setError('');

        if (!validateEmail(email)) {
            setError('Неправильний формат електронної адреси.');
            return;
        }

        const formData = new FormData();
        formData.append('email', email);

        try {
            const response = await api.post('/api/RegistrationAuthorization/forgot-password', formData, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                }
            });

            if (response.status === 200) {
                setMessage('Лист для відновлення пароля надіслано, перевірте вашу електронну пошту.');
                navigate("/");
            } else {
                setError('Помилка на сервері, спробуйте пізніше.');
            }
        } catch (error) {
            if (error.response && error.response.data && error.response.data.message) {
                setError(error.response.data.message);
            } else {
                setError('Помилка надсилання листа для відновлення паролю. Перевірте правильність введеної адреси.');
            }
        }
    };


    return (
        <Box
            sx={{
                maxWidth: 400,
                mx: "auto",
                mt: 4,
                p: 3,
                boxShadow: 3,
                borderRadius: 2,
                textAlign: "center",
                backgroundColor: "#fff",
            }}
        >
            <Typography variant="h5" gutterBottom>
                Відновлення паролю
            </Typography>
            <TextField
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                label="Електронна адреса"
                variant="outlined"
                fullWidth
                margin="normal"
            />
            <Button
                variant="contained"
                color="primary"
                onClick={handleForgotPassword}
                fullWidth
                sx={{ mt: 2 }}
            >
                Відновити пароль
            </Button>

            {/* Відображення повідомлень */}
            {message && <Alert severity="success" sx={{ mt: 2 }}>{message}</Alert>}
            {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
        </Box>
    );
};

export default ForgotPassword;
