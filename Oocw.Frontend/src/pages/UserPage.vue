<template>
  <PageFrame>
    <div class="user-page">
      <div class="section profile-section">
        <h2>{{ t('user.profile') }}</h2>
        <div v-if="userInfo">
          <p><strong>{{ t('user.loginName') }}:</strong> {{ userInfo.userName }}</p>
          <p><strong>{{ t('user.group') }}:</strong> {{ userInfo.group }}</p>
        </div>
        <div class="action-row">
          <button class="round-button s-main h" @click="togglePwdForm">{{ t('user.changePwd') }}</button>
          <button class="round-button s-hollow h" @click="logout">{{ t('btn.logout') }}</button>
        </div>
      </div>

      <div class="section pwd-section" v-if="showPwdForm">
        <h3>{{ t('user.changePwd') }}</h3>
        <div class="form-row">
          <label>{{ t('user.oldPwd') }}</label>
          <input type="password" v-model="oldPwd" class="text-input" />
        </div>
        <div class="form-row">
          <label>{{ t('user.newPwd') }}</label>
          <input type="password" v-model="newPwd" class="text-input" />
        </div>
        <p v-if="pwdMsg" class="msg">{{ pwdMsg }}</p>
        <button class="round-button s-main h" @click="changePwd">{{ t('btn.save') }}</button>
      </div>

      <div class="section enrolments-section">
        <h3>{{ t('user.enrolments') }}</h3>
        <div v-if="enrolments.length === 0" class="empty-msg">{{ t('user.noEnrolments') }}</div>
        <div v-for="sel in enrolments" :key="sel.id" class="enrolment-row">
          <router-link :to="`/class-instance/${sel.classInstanceId}`" class="enrolment-link">
            {{ sel.classInstanceId }}
          </router-link>
          <span class="status-badge round-button s-sub">{{ sel.currentStatus }}</span>
          <button class="round-button s-hollow h" @click="withdraw(sel.id)">{{ t('btn.withdraw') }}</button>
        </div>
      </div>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getUserInfo, UserInfo, requestPasswordChange } from '@/api/user';
import { tryLogOut } from '@/api/auth';
import { getMySelections, withdrawEnrolment, CourseSelectionItem } from '@/api/selection';

const { t } = useI18n();
const router = useRouter();

const userInfo = ref<UserInfo | null>(null);
const enrolments = ref<CourseSelectionItem[]>([]);
const showPwdForm = ref(false);
const oldPwd = ref('');
const newPwd = ref('');
const pwdMsg = ref('');

onMounted(async () => {
  const res = await getUserInfo();
  if (res.status === 200 && res.result) userInfo.value = res.result;
  const selRes = await getMySelections();
  if (selRes.status === 200 && selRes.result) enrolments.value = selRes.result.list;
});

function togglePwdForm() {
  showPwdForm.value = !showPwdForm.value;
  pwdMsg.value = '';
}

async function changePwd() {
  if (!oldPwd.value || !newPwd.value) { pwdMsg.value = 'Please fill in both fields.'; return; }
  const res = await requestPasswordChange(oldPwd.value, newPwd.value);
  pwdMsg.value = res.info || (res.status === 200 ? 'Password changed.' : 'Error.');
  if (res.status === 200) { oldPwd.value = ''; newPwd.value = ''; }
}

async function logout() {
  await tryLogOut();
  router.push('/auth');
}

async function withdraw(selectionId: string) {
  await withdrawEnrolment(selectionId);
  const selRes = await getMySelections();
  if (selRes.status === 200 && selRes.result) enrolments.value = selRes.result.list;
}
</script>

<style scoped>
.user-page { padding: 24px; }
.section { margin-bottom: 32px; }
.section h2, .section h3 { margin-bottom: 12px; }
.action-row { display: flex; gap: 10px; margin-top: 12px; }
.form-row { display: flex; flex-direction: column; margin-bottom: 10px; }
.form-row label { margin-bottom: 4px; color: var(--color-txt3); }
.text-input { padding: 6px 10px; border: 1px solid #ccc; border-radius: 4px; background: transparent; color: var(--color-txt1); }
.msg { color: var(--color-txt3); margin: 6px 0; }
.enrolment-row { display: flex; align-items: center; gap: 10px; margin: 8px 0; }
.enrolment-link { color: var(--color-s-acc-main); text-decoration: none; }
.empty-msg { color: var(--color-txt3); }
</style>
