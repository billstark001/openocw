<template>
  <PageFrame>
    <PageBanner />
    <RouteDisplay v-if="treeData"></RouteDisplay>
    <div v-if="!route.params.target" class="org-area">
      <div v-if="loadingOrgs" class="org-loading">
        <p>{{ t('crsl.noitem.db') }}</p>
      </div>
      <div v-for="org in rootOrgs" :key="org.code" class="org-panel">
        <router-link :to="'/db/' + org.code">
          <div class="org-icon" :data-type="org.type">
            {{ org.name.charAt(0).toUpperCase() }}
          </div>
          <p>{{ org.name }}</p>
        </router-link>
      </div>
    </div>
    <div id="subframe" v-if="route.params.target">
      <div id="sf-left">
        <NavList v-if="treeData" :data="treeData"></NavList>
      </div>
      <div id="sf-right">
        <CourseList v-if="courses" :query="courses" place="db" />
      </div>
    </div>
  </PageFrame>
  <PageFooter></PageFooter>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

import PageFrame from '../components/PageFrame.vue';
import RouteDisplay from '@/components/RouteDisplay.vue';
import NavList from '@/components/NavList.vue';
import { NavNode, NavListData } from '@/components/NavList.vue';
import PageBanner from '@/components/lesser/PageBanner.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import CourseList from '@/components/courses/CourseList.vue';

import * as utils from '@/utils/query';
import { CourseBrief, getCourseListByDepartment } from '@/api/query';
import { OrganizationBrief, listOrganizations } from '@/api/organization';

const { t } = useI18n();
const route = useRoute();

const treeData = ref<NavListData | undefined>();
const courses = ref<utils.QueryResult<CourseBrief[]> | undefined>();
const rootOrgs = ref<OrganizationBrief[]>([]);
const loadingOrgs = ref(true);

// ── org tree helpers ───────────────────────────────────────────────────────

function buildNavTree(orgs: OrganizationBrief[]): NavNode {
  const childrenMap = new Map<string | null, OrganizationBrief[]>();
  for (const o of orgs) {
    const key = o.parentId ?? null;
    if (!childrenMap.has(key)) childrenMap.set(key, []);
    childrenMap.get(key)!.push(o);
  }

  function buildNode(org: OrganizationBrief): NavNode {
    const kids = childrenMap.get(org.id) ?? [];
    return {
      key: org.code,
      name: org.name,
      action: kids.length > 0 ? 'children' : 'self',
      children: kids.map(buildNode),
    };
  }

  const roots = childrenMap.get(null) ?? [];
  return {
    key: 'root',
    action: 'none',
    children: [
      ...roots.map(buildNode),
      { key: 'meta.uncat', action: 'uncat', children: [] },
    ],
  };
}

// ── navigation helpers (unchanged logic) ──────────────────────────────────

function getCurrentOpr(cur: NavNode, parents: NavNode[]): string[] {
  let ret: string[] = [];
  if (cur.action === 'self') ret.push(cur.key);
  else if (cur.action === 'uncat') ret.push(cur.action);
  else if (cur.action === 'parent') ret = parents.length > 1 ? [parents[0].key] : [];
  else if (cur.action === 'children') {
    cur.children.forEach(c => ret = ret.concat(getCurrentOpr(c, [cur, ...parents])));
  }
  return ret;
}

function identifyCurrentOpr(root: NavNode, path: string[]): string | undefined {
  let parents: NavNode[] = [];
  let i = 0;
  let cur: NavNode = root;

  while (i < path.length) {
    const matched = cur.children.find(child => child.key === path[i]);
    if (matched) {
      parents = [cur, ...parents];
      cur = matched;
      i++;
    } else {
      cur = root;
      parents = [];
      break;
    }
  }

  const rets = getCurrentOpr(cur, parents);
  return rets.length > 0 ? rets.join(',') : undefined;
}

function getTargetPath() {
  const target = route.params.target;
  if (target === undefined) return [];
  if (Array.isArray(target)) return utils.decodePath(target.join());
  return utils.decodePath(target);
}

// ── data loading ───────────────────────────────────────────────────────────

let _orgTree: NavNode | undefined;

async function loadOrgs() {
  loadingOrgs.value = true;
  const res = await listOrganizations();
  if (res.status === 200 && res.result) {
    const orgs = res.result.list;
    rootOrgs.value = orgs.filter(o => o.parentId === null);
    _orgTree = buildNavTree(orgs);
  }
  loadingOrgs.value = false;
}

async function updateData() {
  if (!_orgTree) {
    console.warn('[DBPage] updateData called before org tree was loaded.');
    return;
  }

  const target = getTargetPath();
  const deptCode = identifyCurrentOpr(_orgTree, target);

  const _res = await getCourseListByDepartment(deptCode ?? 'uncat');
  courses.value = undefined;

  treeData.value = {
    node: _orgTree,
    selected: target,
    path: [],
  };

  await nextTick();
  courses.value = _res;
}

onMounted(async () => {
  await loadOrgs();
  if (route.params.target) {
    await updateData();
  }
});

watch(() => route.params, updateData);
</script>

<style scoped>
#subframe {
  overflow: hidden;
}

#sf-left {
  width: var(--leftbar-width);
  box-sizing: border-box;
  padding: 10px 10px 10px 0;
  float: left;
}

#sf-right {
  width: calc(100% - var(--leftbar-width));
  float: left;
}

.org-area {
  text-align: center;
  padding: 20px 0;
}

.org-loading {
  padding: 60px 20px;
  font-size: larger;
}

.org-panel {
  display: inline-block;
  width: calc(24% - 60px);
  max-width: 320px;
  min-width: 120px;
  box-sizing: border-box;
  margin: 30px;
  background-color: var(--db-panel-color);
  box-shadow: 0 0 17px #00000055;
}

.org-panel a {
  display: block;
  width: 100%;
  height: 100%;
  align-items: center;
  text-align: center;
  padding: 45px 0;
}

.org-panel * {
  transition: 300ms;
}

.org-panel a:hover {
  background-color: #ffffff22;
}

.org-panel a:hover * {
  opacity: 0.5;
}

.dark-mode .org-panel a:hover * {
  opacity: 1;
}

.org-icon {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  margin: 0 auto 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  font-weight: 300;
  background-color: var(--color-s-main-light-3);
  color: var(--color-s-main-dark-2);
}

.dark-mode .org-icon {
  background-color: var(--color-s-main-dark-2);
  color: var(--color-s-main-light-3);
}

.org-panel p {
  margin: auto;
  margin-top: 10px;
  font-size: large;
  max-width: calc(100% - 30px);
}
</style>
