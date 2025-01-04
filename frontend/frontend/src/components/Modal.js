import './Modal.css';

const Modal = ({ message, onClose, isError }) => {
    return (
        <div className="fixed inset-0 bg-gray-700 flex items-center justify-center z-50">
            <div className={`bg-white p-6 rounded-lg shadow-lg animate__animated ${isError ? 'border-2 border-red-500' : 'border-2 border-green-500'}`}>
                <p className="text-center text-lg">{message}</p>
                <button
                    onClick={onClose}
                    className="mt-4 w-full bg-blue-500 text-white py-2 rounded-full"
                >
                    Закрити
                </button>
            </div>
        </div>
    );
};

export default Modal;
