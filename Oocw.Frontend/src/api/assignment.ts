import { getInfo, fetchServer } from '@/utils/query';

export interface SubmissionContent {
  type: 'Text';
  text: string;
}

export interface AssignmentSubmission {
  id: string;
  studentId: string;
  grade: number;
  graderComment: string;
  contents: SubmissionContent[];
  createTime: string;
  updateTime?: string;
}

export async function getMySubmission(classInstanceId: string, contentId: string) {
  return getInfo<AssignmentSubmission>(
    `/api/assignment/my-submission?classInstanceId=${encodeURIComponent(classInstanceId)}&contentId=${encodeURIComponent(contentId)}`
  );
}

export async function submitAssignment(
  classInstanceId: string,
  contentId: string,
  text: string
) {
  return fetchServer<{ id: string }>(
    new Request('/api/assignment/submit', {
      credentials: 'include',
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        classInstanceId,
        contentId,
        contents: [{ type: 'Text', text }],
      }),
    })
  );
}
