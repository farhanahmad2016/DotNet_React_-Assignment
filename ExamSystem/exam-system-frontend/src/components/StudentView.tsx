import React, { useState, useEffect } from 'react';
import { examService, attemptService } from '../services/api';
import { Exam, Attempt } from '../types';
import './Components.css';

const StudentView: React.FC = () => {
  const [exams, setExams] = useState<Exam[]>([]);
  const [attempts, setAttempts] = useState<Attempt[]>([]);
  const [currentAttempt, setCurrentAttempt] = useState<Attempt | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [examsData, attemptsData] = await Promise.all([
        attemptService.getAllExams(),
        attemptService.getStudentAttempts(),
      ]);
      setExams(examsData);
      setAttempts(attemptsData);
      
      // Find current in-progress attempt
      const inProgress = attemptsData.find(a => a.attemptStatus === 'InProgress');
      setCurrentAttempt(inProgress || null);
    } catch (error) {
      console.error('Failed to load data:', error);
      setMessage('Failed to load exam data');
    }
  };

  const handleStartAttempt = async (examId: string) => {
    setLoading(true);
    setMessage('');

    try {
      const attempt = await attemptService.startAttempt(examId);
      setCurrentAttempt(attempt);
      setAttempts(prev => [...prev, attempt]);
      await loadData(); // Refresh to update remaining attempts
      setMessage('Attempt started successfully!');
    } catch (error) {
      setMessage('Failed to start attempt. Check cooldown or max attempts.');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmitAttempt = async () => {
    if (!currentAttempt) return;

    setLoading(true);
    setMessage('');

    try {
      await attemptService.submitAttempt(currentAttempt.attemptId);
      setCurrentAttempt(null);
      
      // Refresh all data to get updated remaining attempts
      await loadData();
      
      setMessage('Attempt submitted successfully!');
    } catch (error) {
      setMessage('Failed to submit attempt');
    } finally {
      setLoading(false);
    }
  };

  const canStartAttempt = (exam: Exam) => {
    if (exam.remainingAttempts <= 0) return false;
    if (currentAttempt) return false;
    if (exam.nextAttemptAvailableAt && new Date(exam.nextAttemptAvailableAt) > new Date()) {
      return false;
    }
    return true;
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  const getNextAttemptMessage = (exam: Exam) => {
    if (exam.remainingAttempts <= 0) {
      return 'No attempts left.';
    }
    
    if (exam.nextAttemptAvailableAt && new Date(exam.nextAttemptAvailableAt) > new Date()) {
      return `Next attempt available at ${formatDateTime(exam.nextAttemptAvailableAt)}.`;
    }
    
    return '';
  };

  if (exams.length === 0) {
    return <div className="container">Loading exams...</div>;
  }

  return (
    <div className="container">
      <h2>Student View</h2>
      
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3>Available Exams</h3>
        <button onClick={loadData} className="btn-secondary">Refresh</button>
      </div>
      
      {message && (
        <div className={message.includes('success') ? 'message-success' : 'message-error'}>
          {message}
        </div>
      )}
      
      {currentAttempt && (
        <div className="exam-details" style={{ marginBottom: '20px', backgroundColor: '#e8f5e8' }}>
          <h4>Current Attempt in Progress</h4>
          <button
            onClick={handleSubmitAttempt}
            disabled={loading}
            className="btn-success"
          >
            {loading ? 'Submitting...' : 'Submit Current Attempt'}
          </button>
        </div>
      )}
      
      {exams.map((exam) => (
        <div key={exam.examId} className="exam-details" style={{ marginBottom: '20px' }}>
          <h4>{exam.title}</h4>
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-label">Max Attempts</div>
              <div className="stat-value">{exam.maxAttempts}</div>
            </div>
            <div className="stat-card">
              <div className="stat-label">Remaining</div>
              <div className="stat-value">{exam.remainingAttempts}</div>
            </div>
            <div className="stat-card">
              <div className="stat-label">Cooldown</div>
              <div className="stat-value">{exam.cooldownMinutes}m</div>
            </div>
          </div>
          
          {canStartAttempt(exam) ? (
            <button
              onClick={() => handleStartAttempt(exam.examId)}
              disabled={loading}
              className="btn-primary"
            >
              {loading ? 'Starting...' : 'Start Attempt'}
            </button>
          ) : (
            <div className="cooldown-message">
              {getNextAttemptMessage(exam)}
            </div>
          )}
        </div>
      ))}

      <div>
        <h3>My Attempts History</h3>
        {attempts.length === 0 ? (
          <p>No attempts found.</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Exam</th>
                <th>Attempt #</th>
                <th>Status</th>
                <th>Start Time</th>
                <th>End Time</th>
              </tr>
            </thead>
            <tbody>
              {attempts.map((attempt) => (
                <tr key={attempt.attemptId}>
                  <td>{attempt.examTitle || 'Unknown'}</td>
                  <td>{attempt.attemptNumber}</td>
                  <td>{attempt.attemptStatus}</td>
                  <td>{formatDateTime(attempt.startTime)}</td>
                  <td>{attempt.endTime ? formatDateTime(attempt.endTime) : '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default StudentView;