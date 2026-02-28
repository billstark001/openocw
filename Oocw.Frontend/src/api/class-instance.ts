import { getInfo } from '@/utils/query';

export interface ContentItem {
  id: string;
  lectureNumber: number;
  lectureDate?: string;
  type: 'Text' | 'File' | 'Media' | 'Assignment';
  text: string;
  isPublic: boolean;
}

export interface ClassInstanceDetail {
  id: string;
  classId: string;
  lecturers: string[];
  address: {
    year: number;
    term: number;
    time?: { day?: number; start?: number; end?: number };
    location?: string;
  };
  contents: ContentItem[];
}

export async function getClassInstanceInfo(id: string) {
  return getInfo<ClassInstanceDetail>(`/api/class-instance/info/${encodeURIComponent(id)}`);
}
