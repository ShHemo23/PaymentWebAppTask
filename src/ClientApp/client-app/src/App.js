import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './AuthContext';
import LoginPage from './LoginPage';
import MainLayout from './MainLayout';
import { NotifierProvider } from './components/Notifier';
import './App.css';

// A wrapper for <Route> that redirects to the login
// screen if you're not yet authenticated.
function ProtectedRoute({ children }) {
  const { token } = useAuth();
  if (!token) {
    return <Navigate to="/login" replace state={{ from: window.location.pathname }} />;
  }
  return children;
}

function App() {
  return (
    <AuthProvider>
      <NotifierProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            
            {/* Redirect root path to /dashboard */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            
            {/* Protected routes */}
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <MainLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<div>Dashboard Content</div>} />
              <Route path="payments" element={<div>Payments List</div>} />
              <Route path="reports" element={<div>Reports View</div>} />
            </Route>
            
            {/* Catch-all route - redirect to login */}
            <Route path="*" element={<Navigate to="/login" replace />} />
          </Routes>
        </BrowserRouter>
      </NotifierProvider>
    </AuthProvider>
  );
}

export default App;