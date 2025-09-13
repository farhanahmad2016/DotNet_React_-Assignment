import React, { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Login from './components/Login';
import AdminView from './components/AdminView';
import StudentView from './components/StudentView';
import './App.css';
import './components/Components.css';

const AppContent: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  // Show appropriate view based on user role
  const currentView = user?.role === 'Admin' ? 'admin' : 'student';

  if (!isAuthenticated) {
    return <Login />;
  }

  return (
    <div className="App">
      <header className="header">
        <h1>Exam System</h1>
        <div className="header-controls">
          <span className="user-info">
            Welcome, {user?.username} ({user?.role})
          </span>

          <button
            onClick={logout}
            className="btn-danger"
          >
            Logout
          </button>
        </div>
      </header>
      
      <main>
        {currentView === 'admin' ? <AdminView /> : <StudentView />}
      </main>
    </div>
  );
};

const App: React.FC = () => {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
};

export default App;