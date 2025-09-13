export interface User {
  id: string;
  username: string;
  role: 'Admin' | 'Student';
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export interface Exam {
  examId: string;
  title: string;
  maxAttempts: number;
  cooldownMinutes: number;
  lastModified: string;
  remainingAttempts: number;
  nextAttemptAvailableAt?: string;
}

export interface CreateExamRequest {
  title: string;
  maxAttempts: number;
  cooldownMinutes: number;
}

export interface Attempt {
  attemptId: string;
  attemptNumber: number;
  attemptStatus: 'InProgress' | 'Completed';
  startTime: string;
  endTime?: string;
  examTitle?: string;
}