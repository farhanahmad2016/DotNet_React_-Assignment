import React, { useState, useEffect } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { authService } from './services/api';
import AdminView from './components/AdminView';
import StudentView from './components/StudentView';
import './App.css';
import './components/Components.css';

const AppContent: React.FC = () => {
  const { user, login, logout, isAuthenticated } = useAuth();
  // State for current view - defaults to admin
  const [currentView, setCurrentView] = useState<'admin' | 'student'>('admin');



  // Auto-login function for seamless switching
  const autoLogin = async (role: 'admin' | 'student') => {
    try {
      const credentials = role === 'admin' 
        ? { username: 'admin', password: 'admin123' }
        : { username: 'student', password: 'student123' };
      
      const response = await authService.login(credentials);
      login(response.token);
      return true;
    } catch (error) {
      console.error('Auto-login failed:', error);
      return false;
    }
  };

  // Handle view switching with auto-login
  const handleViewSwitch = async (targetView: 'admin' | 'student') => {
    // Always auto-login with the required role for seamless switching
    const success = await autoLogin(targetView);
    if (success) {
      setCurrentView(targetView);
    }
  };

  // Auto-login on first load if no user
  useEffect(() => {
    if (!user) {
      autoLogin('admin'); // Default to admin login
    }
  }, [user]);

  if (!isAuthenticated) {
    return <div style={{ padding: '2rem', textAlign: 'center' }}>Loading...</div>;
  }

  return (
    <div className="App">
      <header className="header">
        <h1>Exam System</h1>
        <div className="header-controls">
          {/* View Switcher */}
          <div className="view-switcher">
            <button
              onClick={() => handleViewSwitch('admin')}
              className={`btn ${currentView === 'admin' ? 'btn-primary' : 'btn-secondary'}`}
            >
              Admin View
            </button>
            <button
              onClick={() => handleViewSwitch('student')}
              className={`btn ${currentView === 'student' ? 'btn-primary' : 'btn-secondary'}`}
            >
              Student View
            </button>
          </div>
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