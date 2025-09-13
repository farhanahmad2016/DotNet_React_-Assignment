import React, { useState, useEffect } from 'react';
import { examService, attemptService } from '../services/api';
import { CreateExamRequest, Attempt, Exam } from '../types';
import './Components.css';

const AdminView: React.FC = () => {
  const [title, setTitle] = useState('');
  const [maxAttempts, setMaxAttempts] = useState(1);
  const [cooldownMinutes, setCooldownMinutes] = useState(0);
  const [attempts, setAttempts] = useState<Attempt[]>([]);
  const [exams, setExams] = useState<Exam[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [attemptsData, examsData] = await Promise.all([
        attemptService.getAllAttempts(),
        attemptService.getAllExamsForAdmin()
      ]);
      setAttempts(attemptsData);
      setExams(examsData);
    } catch (error) {
      console.error('Failed to load data:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setMessage('');

    try {
      const examData: CreateExamRequest = {
        title,
        maxAttempts,
        cooldownMinutes,
      };

      await examService.createExam(examData);
      setMessage('Exam created successfully!');
      loadData();
    } catch (error: any) {
      console.error('Create exam error:', error);
      setMessage(`Failed to create exam: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString('en-US', {
      timeZone: 'UTC',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      timeZoneName: 'short'
    });
  };

  return (
    <div className="container">
      <h2>Admin View</h2>
      
      <div className="form-container">
        <h3>Create Exam</h3>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title:</label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              maxLength={200}
              className="form-input"
            />
          </div>
          <div className="form-group">
            <label>Max Attempts (1-1000):</label>
            <input
              type="number"
              value={maxAttempts}
              onChange={(e) => setMaxAttempts(parseInt(e.target.value) || 1)}
              min={1}
              max={1000}
              required
              className="form-input"
            />
          </div>
          <div className="form-group">
            <label>Cooldown Minutes (0-525600):</label>
            <input
              type="number"
              value={cooldownMinutes}
              onChange={(e) => setCooldownMinutes(parseInt(e.target.value) || 0)}
              min={0}
              max={525600}
              required
              className="form-input"
            />
          </div>
          {message && (
            <div className={message.includes('success') ? 'message-success' : 'message-error'}>
              {message}
            </div>
          )}
          <div style={{ display: 'flex', gap: '10px' }}>
            <button
              type="submit"
              disabled={loading}
              className="btn-primary"
            >
              {loading ? 'Creating...' : 'Create Exam'}
            </button>
            <button
              type="button"
              onClick={() => {
                setTitle('');
                setMaxAttempts(1);
                setCooldownMinutes(0);
                setMessage('');
              }}
              className="btn-secondary"
            >
              Clear Form
            </button>
          </div>
        </form>
      </div>

      <div>
        <h3>All Exams</h3>
        {exams.length === 0 ? (
          <p>No exams found.</p>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Title</th>
                <th>Max Attempts</th>
                <th>Cooldown (min)</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              {exams.map((exam) => (
                <tr key={exam.examId}>
                  <td>{exam.title}</td>
                  <td>{exam.maxAttempts}</td>
                  <td>{exam.cooldownMinutes}</td>
                  <td>{formatDateTime(exam.lastModified)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h3>Student Attempts History</h3>
          <button onClick={loadData} className="btn-secondary">Refresh</button>
        </div>
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

export default AdminView;
