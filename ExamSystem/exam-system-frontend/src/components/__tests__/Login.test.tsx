import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import Login from '../Login';
import { AuthProvider } from '../../contexts/AuthContext';
import * as api from '../../services/api';

// Mock the API service
jest.mock('../../services/api');
const mockedApi = api as jest.Mocked<typeof api>;

const LoginWithProvider = () => (
  <AuthProvider>
    <Login />
  </AuthProvider>
);

describe('Login Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders login form', () => {
    render(<LoginWithProvider />);
    
    expect(screen.getByRole('heading', { name: 'Login' })).toBeInTheDocument();
    expect(screen.getByLabelText('Username:')).toBeInTheDocument();
    expect(screen.getByLabelText('Password:')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Login' })).toBeInTheDocument();
  });

  test('shows error message on failed login', async () => {
    mockedApi.authService.login.mockRejectedValue(new Error('Invalid credentials'));
    
    render(<LoginWithProvider />);
    
    fireEvent.change(screen.getByLabelText('Username:'), { target: { value: 'testuser' } });
    fireEvent.change(screen.getByLabelText('Password:'), { target: { value: 'wrongpassword' } });
    fireEvent.click(screen.getByRole('button', { name: 'Login' }));

    await waitFor(() => {
      expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });
  });

  test('successful login calls login function', async () => {
    const mockToken = 'mock-jwt-token';
    mockedApi.authService.login.mockResolvedValue({ token: mockToken });
    
    render(<LoginWithProvider />);
    
    fireEvent.change(screen.getByLabelText('Username:'), { target: { value: 'admin' } });
    fireEvent.change(screen.getByLabelText('Password:'), { target: { value: 'admin123' } });
    fireEvent.click(screen.getByRole('button', { name: 'Login' }));

    await waitFor(() => {
      expect(mockedApi.authService.login).toHaveBeenCalledWith({
        username: 'admin',
        password: 'admin123'
      });
    });
  });
});