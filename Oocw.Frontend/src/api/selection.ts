import { getInfo, postInfo } from '@/utils/query';

export interface CourseSelectionItem {
  id: string;
  studentId: string;
  classInstanceId: string;
  currentStatus: string;
  createTime: string;
}

export async function getMySelections() {
  return getInfo<{ list: CourseSelectionItem[]; totalCount: number }>('/api/selection/my');
}

export async function applyEnrolment(classInstanceId: string) {
  return postInfo<{ id: string }>('/api/selection/apply', { classInstanceId });
}

export async function withdrawEnrolment(selectionId: string) {
  return postInfo<void>('/api/selection/withdraw', { selectionId });
}
