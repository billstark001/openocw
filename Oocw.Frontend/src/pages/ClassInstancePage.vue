<template>
  <PageFrame>
    <div class="class-instance-page" v-if="instance">
      <div class="page-heading">
        <h2>{{ t('classInstance.title') }}</h2>
        <p class="meta">{{ instance.address.year }} / {{ instance.address.term }}</p>
      </div>

      <div class="chip-row" v-if="instance.lecturers.length">
        <span class="round-button s-hollow" v-for="l in instance.lecturers" :key="l">{{ l }}</span>
      </div>

      <div class="section">
        <h3>{{ t('classInstance.materials') }}</h3>
        <div v-if="materials.length === 0" class="empty-msg">{{ t('classInstance.noContents') }}</div>
        <div v-for="c in materials" :key="c.id" class="content-row">
          <span class="lecture-num">#{{ c.lectureNumber }}</span>
          <span class="content-text">{{ c.text }}</span>
          <span class="round-button s-sub">{{ c.type }}</span>
        </div>
      </div>

      <div class="section">
        <h3>{{ t('classInstance.assignments') }}</h3>
        <div v-if="assignments.length === 0" class="empty-msg">{{ t('classInstance.noContents') }}</div>
        <div v-for="a in assignments" :key="a.id" class="content-row">
          <span class="lecture-num">#{{ a.lectureNumber }}</span>
          <span class="content-text">{{ a.text }}</span>
          <router-link :to="`/assignment/${instance.id}/${a.id}`" class="round-button s-main h">
            {{ t('assignment.title') }}
          </router-link>
        </div>
      </div>

      <div class="section">
        <button class="round-button s-main h" @click="enrol">{{ t('classInstance.enrol') }}</button>
        <p v-if="enrolMsg" class="msg">{{ enrolMsg }}</p>
      </div>
    </div>
    <div v-else class="loading-area"><p>{{ loadingMsg }}</p></div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getClassInstanceInfo, ClassInstanceDetail } from '@/api/class-instance';
import { applyEnrolment } from '@/api/selection';

const { t } = useI18n();
const route = useRoute();

const instance = ref<ClassInstanceDetail | null>(null);
const loadingMsg = ref('Loading...');
const enrolMsg = ref('');

const materials = computed(() => instance.value?.contents.filter(c => c.type !== 'Assignment') ?? []);
const assignments = computed(() => instance.value?.contents.filter(c => c.type === 'Assignment') ?? []);

onMounted(async () => {
  const id = route.params.id as string;
  const res = await getClassInstanceInfo(id);
  if (res.status === 200 && res.result) {
    instance.value = res.result;
  } else {
    loadingMsg.value = `Error ${res.status}: ${res.info}`;
  }
});

async function enrol() {
  if (!instance.value) return;
  const res = await applyEnrolment(instance.value.id);
  enrolMsg.value = res.status === 200 ? t('status.enrolled') : `${t('status.error')}${res.info}`;
}
</script>

<style scoped>
.class-instance-page { padding: 24px; }
.page-heading { margin-bottom: 16px; }
.meta { color: var(--color-txt3); }
.chip-row { display: flex; flex-wrap: wrap; gap: 6px; margin: 8px 0; }
.section { margin-top: 24px; }
.section h3 { margin-bottom: 8px; }
.content-row { display: flex; align-items: center; gap: 10px; margin: 8px 0; }
.lecture-num { color: var(--color-txt3); font-size: smaller; min-width: 24px; }
.content-text { flex: 1; }
.empty-msg { color: var(--color-txt3); }
.msg { margin-top: 8px; color: var(--color-txt3); }
.loading-area { padding: 40px; text-align: center; }
</style>
