import { buildParams, QueryResult, getInfo } from "@/utils/query";

export interface LecturerInfo {
  id: string,
  name: string
}

export interface CourseBrief {
  id: string,
  name: string,
  /** First class name, if available. */
  className?: string,
  tags: string[],
  lecturers: LecturerInfo[],
  description: string,
  imageLink?: string,
}

export interface FacultyBrief {
  name: string,
  names: Record<string, string>,
}

export function defaultCourseBrief(): CourseBrief {
  return {
    id: "AAA.B123",
    name: "Test Course",
    className: "Example class",
    tags: ["tag1", "标签2", "タグ3"],
    lecturers: [
      { id: "1", name: "Tadokoro Koji" },
      { id: "2", name: "Yajuu Senpai" },
    ],
    description: "Test English\n日本語文字テスト\n中文文本测试",
  }
}

// ── internal types matching the backend ListResult<T> wrapper ─────────────

interface _BackendCourseBrief {
  id: string;
  name: string;
  tags: string[];
  classes: { id: string; name: string }[];
  lecturers: { id: string; name: string }[];
  description: string;
  image?: string;
}

interface _ListResult<T> {
  list: T[];
  totalCount?: number;
  totalPage?: number;
}

function _adaptCourseBrief(b: _BackendCourseBrief): CourseBrief {
  return {
    id: b.id,
    name: b.name,
    className: b.classes?.[0]?.name,
    tags: b.tags ?? [],
    lecturers: (b.lecturers ?? []).map(l => ({ id: l.id, name: l.name })),
    description: b.description,
    imageLink: b.image,
  };
}

export interface CourseDetail {
  id: string;
  name: string;
  courseCode: string;
  credit: number;
  departments: string[];
  lecturers: string[];
  tags: string[];
  content: string;
  imageLink?: string;
  classes: { id: string; name: string }[];
}

// ── info ──────────────────────────────────────────────────────────────────

export async function getCourseInfo(id: string, classId?: string, lang?: string): Promise<QueryResult<CourseDetail>> {
  const scheme = `/api/course/info/${encodeURIComponent(id)}?` + buildParams({ classId, lang });
  return await getInfo<CourseDetail>(scheme);
}

export async function getFacultyBrief(id: string, lang?: string): Promise<QueryResult<FacultyBrief>> {
  const scheme = `/api/info/faculty/${encodeURIComponent(id)}?` + buildParams({ lang });
  return await getInfo<FacultyBrief>(scheme);
}

// ── list ──────────────────────────────────────────────────────────────────

export async function getCourseListByDepartment(
  id: string,
  lang?: string,
  page?: number,
  pageSize?: number,
): Promise<QueryResult<CourseBrief[]> & { totalPage?: number }> {
  const scheme = `/api/course/list/${encodeURIComponent(id)}?`
    + buildParams({ lang, page, pageSize });
  const raw = await getInfo<_ListResult<_BackendCourseBrief>>(scheme);
  if (raw.status !== 200 || !raw.result) {
    return { status: raw.status, info: raw.info };
  }
  return { status: 200, info: raw.info, result: raw.result.list.map(_adaptCourseBrief), totalPage: raw.result.totalPage };
}

export async function getCourseList(
  lang?: string,
  page?: number,
  pageSize?: number,
): Promise<QueryResult<CourseBrief[]> & { totalPage?: number }> {
  const scheme = `/api/course/list?` + buildParams({ lang, page, pageSize });
  const raw = await getInfo<_ListResult<_BackendCourseBrief>>(scheme);
  if (raw.status !== 200 || !raw.result) {
    return { status: raw.status, info: raw.info };
  }
  return { status: 200, info: raw.info, result: raw.result.list.map(_adaptCourseBrief), totalPage: raw.result.totalPage };
}

// ── search ────────────────────────────────────────────────────────────────

export interface SearchParams {
  infoVague?: string;
  codeVague?: string;
  contentVague?: string;
  page?: number;
  pageSize?: number;
}

export async function searchCourses(
  params: SearchParams,
  lang?: string,
): Promise<QueryResult<CourseBrief[]> & { totalPage?: number }> {
  const url = `/api/course/search?` + buildParams({ ...params, lang });
  const raw = await getInfo<_ListResult<_BackendCourseBrief>>(url);
  if (raw.status !== 200 || !raw.result) {
    return { status: raw.status, info: raw.info };
  }
  return {
    status: 200,
    info: raw.info,
    result: raw.result.list.map(_adaptCourseBrief),
    totalPage: raw.result.totalPage,
  };
}
