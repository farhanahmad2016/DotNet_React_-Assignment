import axios from 'axios';
import { LoginRequest, LoginResponse, CreateExamRequest, Exam, Attempt } from '../types';

const API_BASE_URL = 'http://localhost:5083/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const authService = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    try {
      const response = await api.post('/auth/login', credentials);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Login failed');
    }
  },
};

export const examService = {
  createExam: async (exam: CreateExamRequest): Promise<Exam> => {
    try {
      console.log('Creating exam with data:', exam);
      const response = await api.post('/exam', exam);
      return response.data;
    } catch (error: any) {
      console.error('API Error:', error.response?.data || error.message);
      throw new Error(error.response?.data?.message || error.response?.data || 'Failed to create exam');
    }
  },
  
  updateExam: async (examId: string, exam: CreateExamRequest): Promise<Exam> => {
    try {
      const response = await api.put(`/exam/${examId}`, exam);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to update exam');
    }
  },
  
  getExamForStudent: async (): Promise<Exam> => {
    try {
      const response = await api.get('/exam/student');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to load exam');
    }
  },
};

export const attemptService = {
  getStudentAttempts: async (): Promise<Attempt[]> => {
    try {
      const response = await api.get('/attempt/student');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to load attempts');
    }
  },
  
  getAllAttempts: async (): Promise<Attempt[]> => {
    try {
      const response = await api.get('/attempt/all');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to load all attempts');
    }
  },
  
  getAllExamsForAdmin: async (): Promise<Exam[]> => {
    try {
      const response = await api.get('/attempt/admin/exams');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to load exams');
    }
  },
  
  getAllExams: async (): Promise<Exam[]> => {
    try {
      const response = await api.get('/attempt/exams');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to load exams');
    }
  },
  
  startAttempt: async (examId?: string): Promise<Attempt> => {
    try {
      const url = examId ? `/attempt/start/${examId}` : '/attempt/start';
      const response = await api.post(url);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to start attempt');
    }
  },
  
  submitAttempt: async (attemptId: string): Promise<Attempt> => {
    try {
      const response = await api.post(`/attempt/${attemptId}/submit`);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Failed to submit attempt');
    }
  },
};