import { getInfo, postInfo } from '@/utils/query';

export interface GlobalSettings {
  currentYear: number;
  currentTerm: number;
}

export interface AdminUser {
  id: string;
  loginName: string;
  group: string;
  createTime: string;
}

export interface AdminUserListResult {
  list: AdminUser[];
  totalCount: number;
  totalPage: number;
}

export interface ReviewRequest {
  systemId: string;
  senderId: string;
  targetCollection: string;
  targetObjectId: string;
  status: string;
  createTime: string;
  reviewComment?: string;
}

export interface ReviewQueueResult {
  list: ReviewRequest[];
  totalCount: number;
  totalPage: number;
}

export async function getSettings() {
  return getInfo<GlobalSettings>('/api/admin/settings');
}

export async function updateSettings(settings: GlobalSettings) {
  return postInfo<void>('/api/admin/settings', settings);
}

export async function listUsers(page = 1, pageSize = 20) {
  return getInfo<AdminUserListResult>(`/api/admin/users?page=${page}&pageSize=${pageSize}`);
}

export async function updateUserRole(id: string, role: string) {
  return postInfo<void>(`/api/admin/user/${encodeURIComponent(id)}/role`, { role });
}

export async function getReviewQueue(page = 1, pageSize = 20) {
  return getInfo<ReviewQueueResult>(`/api/review/queue?page=${page}&pageSize=${pageSize}`);
}

export async function approveRequest(requestId: string, comment?: string) {
  return postInfo<void>('/api/review/approve', { requestId, comment: comment ?? '' });
}

export async function rejectRequest(requestId: string, comment?: string) {
  return postInfo<void>('/api/review/reject', { requestId, comment: comment ?? '' });
}
