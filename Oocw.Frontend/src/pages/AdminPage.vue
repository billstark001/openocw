<template>
  <PageFrame>
    <div class="admin-page">
      <h2>{{ t('admin.title') }}</h2>
      <div class="tab-bar">
        <button v-for="tab in tabs" :key="tab.key"
          :class="['round-button', 's-hollow', activeTab === tab.key ? 'active' : '']"
          @click="activeTab = tab.key">
          {{ tab.label }}
        </button>
      </div>

      <!-- Settings Tab -->
      <div v-if="activeTab === 'settings'" class="tab-content">
        <h3>{{ t('admin.settings') }}</h3>
        <div class="form-row">
          <label>{{ t('admin.year') }}</label>
          <input type="number" v-model.number="settings.currentYear" class="text-input" />
        </div>
        <div class="form-row">
          <label>{{ t('admin.term') }}</label>
          <input type="number" v-model.number="settings.currentTerm" class="text-input" />
        </div>
        <p v-if="settingsMsg" class="msg">{{ settingsMsg }}</p>
        <button class="round-button s-main h" @click="saveSettings">{{ t('btn.save') }}</button>
      </div>

      <!-- Users Tab -->
      <div v-if="activeTab === 'users'" class="tab-content">
        <h3>{{ t('admin.users') }}</h3>
        <div v-for="u in users" :key="u.id" class="user-row">
          <span class="user-id">{{ u.loginName }}</span>
          <span class="round-button s-sub">{{ u.group }}</span>
          <input type="text" v-model="roleInputs[u.id]" class="text-input role-input" :placeholder="t('admin.role')" />
          <button class="round-button s-hollow h" @click="saveRole(u.id)">{{ t('btn.save') }}</button>
        </div>
        <div class="pagination">
          <button class="round-button s-hollow h" @click="changePage(usersPage - 1)" :disabled="usersPage <= 1">{{ t('search.hint.prev') }}</button>
          <span>{{ usersPage }} / {{ usersTotalPages }}</span>
          <button class="round-button s-hollow h" @click="changePage(usersPage + 1)" :disabled="usersPage >= usersTotalPages">{{ t('search.hint.next') }}</button>
        </div>
      </div>

      <!-- Organizations Tab -->
      <div v-if="activeTab === 'orgs'" class="tab-content">
        <h3>{{ t('admin.orgs') }}</h3>
        <div v-for="org in orgs" :key="org.id" class="org-row">
          <span class="round-button s-sub">{{ org.code }}</span>
          <span>{{ org.name }}</span>
          <span class="round-button s-hollow">{{ org.type }}</span>
        </div>
      </div>

      <!-- Review Queue Tab -->
      <div v-if="activeTab === 'review'" class="tab-content">
        <router-link to="/review" class="round-button s-main h">{{ t('review.title') }}</router-link>
      </div>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getSettings, updateSettings, listUsers, updateUserRole, GlobalSettings, AdminUser } from '@/api/admin';
import { listOrganizations, OrganizationBrief } from '@/api/organization';

const { t } = useI18n();

const activeTab = ref('settings');
const tabs = [
  { key: 'settings', label: t('admin.settings') },
  { key: 'users', label: t('admin.users') },
  { key: 'orgs', label: t('admin.orgs') },
  { key: 'review', label: t('review.title') },
];

// Settings
const settings = reactive<GlobalSettings>({ currentYear: 0, currentTerm: 0 });
const settingsMsg = ref('');

async function loadSettings() {
  const res = await getSettings();
  if (res.status === 200 && res.result) {
    settings.currentYear = res.result.currentYear;
    settings.currentTerm = res.result.currentTerm;
  }
}

async function saveSettings() {
  const res = await updateSettings({ ...settings });
  settingsMsg.value = res.status === 200 ? 'Saved.' : `Error: ${res.info}`;
}

// Users
const users = ref<AdminUser[]>([]);
const usersPage = ref(1);
const usersTotalPages = ref(1);
const roleInputs = reactive<Record<string, string>>({});

async function loadUsers() {
  const res = await listUsers(usersPage.value, 20);
  if (res.status === 200 && res.result) {
    users.value = res.result.list;
    usersTotalPages.value = res.result.totalPage || 1;
    res.result.list.forEach(u => { roleInputs[u.id] = u.group; });
  }
}

function changePage(p: number) {
  if (p < 1 || p > usersTotalPages.value) return;
  usersPage.value = p;
  loadUsers();
}

async function saveRole(id: string) {
  await updateUserRole(id, roleInputs[id]);
}

// Orgs
const orgs = ref<OrganizationBrief[]>([]);

async function loadOrgs() {
  const res = await listOrganizations();
  if (res.status === 200 && res.result) orgs.value = res.result.list;
}

watch(activeTab, (tab) => {
  if (tab === 'settings') loadSettings();
  else if (tab === 'users') loadUsers();
  else if (tab === 'orgs') loadOrgs();
});

onMounted(loadSettings);
</script>

<style scoped>
.admin-page { padding: 24px; }
.tab-bar { display: flex; gap: 8px; margin: 16px 0; flex-wrap: wrap; }
.tab-bar .active { background-color: var(--color-s-main-main); color: white; }
.tab-content { margin-top: 16px; }
.form-row { display: flex; flex-direction: column; margin-bottom: 10px; max-width: 300px; }
.form-row label { margin-bottom: 4px; color: var(--color-txt3); }
.text-input { padding: 6px 10px; border: 1px solid #ccc; border-radius: 4px; background: transparent; color: var(--color-txt1); }
.msg { color: var(--color-txt3); margin: 6px 0; }
.user-row { display: flex; align-items: center; gap: 10px; margin: 8px 0; }
.role-input { width: 100px; }
.org-row { display: flex; align-items: center; gap: 10px; margin: 8px 0; }
.pagination { display: flex; gap: 12px; align-items: center; margin-top: 16px; }
</style>
