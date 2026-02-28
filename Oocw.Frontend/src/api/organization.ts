import { getInfo } from '@/utils/query';

export interface OrganizationBrief {
  id: string;
  code: string;
  name: string;
  type: string;
  parentId: string | null;
}

export interface OrgListResult {
  list: OrganizationBrief[];
  totalCount: number;
}

export async function listOrganizations(lang?: string) {
  const qs = lang ? `?lang=${encodeURIComponent(lang)}` : '';
  return getInfo<OrgListResult>(`/api/organization/list${qs}`);
}
