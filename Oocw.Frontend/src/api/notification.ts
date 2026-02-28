import { getInfo, postInfo } from '@/utils/query';

export interface NotificationItem {
  systemId: string;
  userId: string;
  read: boolean;
  createTime: string;
  message: Record<string, string>;
}

export interface NotificationListResult {
  list: NotificationItem[];
  totalCount: number;
  totalPage: number;
}

export async function listNotifications(unreadOnly = false, page = 1, pageSize = 20) {
  return getInfo<NotificationListResult>(
    `/api/notification/list?unreadOnly=${unreadOnly}&page=${page}&pageSize=${pageSize}`
  );
}

export async function markRead(systemId: string) {
  return postInfo<void>(`/api/notification/read/${encodeURIComponent(systemId)}`);
}

export async function markAllRead() {
  return postInfo<void>('/api/notification/read-all');
}
