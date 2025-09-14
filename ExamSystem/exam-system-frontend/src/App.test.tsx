import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock the API service
jest.mock('./services/api', () => ({
  login: jest.fn(),
  getExamsForStudent: jest.fn(),
  getAllExams: jest.fn(),
  getStudentAttempts: jest.fn(),
  getAllAttempts: jest.fn(),
  startAttempt: jest.fn(),
  submitAttempt: jest.fn(),
  createExam: jest.fn(),
  updateExam: jest.fn(),
}));

describe('App Component', () => {
  test('renders without crashing', () => {
    render(<App />);
  });

  test('displays view switcher buttons', async () => {
    render(<App />);
    
    await waitFor(() => {
      expect(screen.getByText('Admin View')).toBeInTheDocument();
      expect(screen.getByText('Student View')).toBeInTheDocument();
    });
  });

  test('shows loading state initially', () => {
    render(<App />);
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  test('displays exam system title', async () => {
    render(<App />);
    
    await waitFor(() => {
      expect(screen.getByText('Exam System')).toBeInTheDocument();
    });
  });
});